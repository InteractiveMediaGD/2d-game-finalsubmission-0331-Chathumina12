using UnityEngine;
using System.Collections;

/// <summary>
/// Overhauled System Admin boss. Handles HP, world-space health bar, 
/// targeted shooting at the player, strafing, and death → EndBossFight.
/// </summary>
public class SystemAdminBoss : MonoBehaviour
{
    [Header("Health")]
    public int maxHealth = 500;
    private int currentHealth;

    [Header("Movement")]
    public float moveSpeed = 3f;
    public float strafeSpeed = 2f;
    private float minX, maxX, lockY;
    private int strafeDir = 1;

    [Header("Attack: Targeted Shot")]
    public GameObject projectilePrefab;
    public float shootInterval = 1.5f;
    public float projectileSpeed = 7f;
    public int projectilesPerBurst = 3;
    public float burstDelay = 0.2f;

    [Header("Effects")]
    public GameObject explosionPrefab;

    // Internal references
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private Transform playerTransform;
    private bool isDead = false;

    // World-Space Health Bar
    private UnityEngine.UI.Slider healthSlider;
    private TMPro.TextMeshProUGUI hpText;
    private GameObject healthBarCanvas;

    // ============================================================
    //  LIFECYCLE
    // ============================================================

    private void Start()
    {
        int difficulty = PlayerPrefs.GetInt("DifficultyLevel", 1);
        if (difficulty == 2) // Hard
        {
            maxHealth = Mathf.RoundToInt(maxHealth * 1.5f);
            Debug.Log($"[SystemAdminBoss] Hard Mode: Max Health increased to {maxHealth}");
        }

        currentHealth = maxHealth;
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Fix sprite scale: if still using the placeholder Background sprite, apply crimson fallback look
        if (spriteRenderer != null && spriteRenderer.sprite != null
            && spriteRenderer.sprite.name.Contains("Background"))
        {
            // Placeholder fallback — make it large and menacing
            transform.localScale = new Vector3(2.5f, 2.5f, 1f);
            spriteRenderer.color = new Color(0.45f, 0.05f, 0.08f, 1f);
        }
        else if (spriteRenderer != null && spriteRenderer.sprite != null)
        {
            // Real PixelWhale sprite — keep white tint, scale is set by the prefab builder
            spriteRenderer.color = Color.white;
        }

        if (spriteRenderer != null) originalColor = spriteRenderer.color;

        // Force Z = 0 so the 2D camera always renders the boss
        Vector3 pos = transform.position;
        pos.z = 0f;
        transform.position = pos;

        // Find the player once
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) playerTransform = p.transform;

        // Camera-based boundary setup
        if (Camera.main != null)
        {
            Vector3 bl = Camera.main.ViewportToWorldPoint(new Vector3(0.1f, 0, 0));
            Vector3 tr = Camera.main.ViewportToWorldPoint(new Vector3(0.9f, 0.8f, 0));
            minX = bl.x;
            maxX = tr.x;
            lockY = tr.y; // boss sits at the 80% height line
        }
        else
        {
            minX = -7f; maxX = 7f; lockY = 3f;
        }

        BuildWorldSpaceHealthBar();
        StartCoroutine(DescendIntoView());
    }

    // ============================================================
    //  SPAWN ANIMATION
    // ============================================================

    private IEnumerator DescendIntoView()
    {
        // Target = centre-X of the camera, lockY
        float targetX = Camera.main != null ? Camera.main.transform.position.x : 0f;
        Vector3 target = new Vector3(targetX, lockY, 0f);

        while (Vector3.Distance(transform.position, target) > 0.05f)
        {
            transform.position = Vector3.MoveTowards(transform.position, target, moveSpeed * Time.deltaTime);
            yield return null;
        }
        transform.position = target;

        // Start the attack loop
        StartCoroutine(StrafeLoop());
        StartCoroutine(ShootLoop());
    }

    // ============================================================
    //  MOVEMENT — horizontal strafe
    // ============================================================

    private IEnumerator StrafeLoop()
    {
        while (!isDead)
        {
            float newX = transform.position.x + strafeDir * strafeSpeed * Time.deltaTime;

            if (newX >= maxX) { newX = maxX; strafeDir = -1; }
            else if (newX <= minX) { newX = minX; strafeDir = 1; }

            transform.position = new Vector3(newX, lockY, 0f);
            yield return null;
        }
    }

    // ============================================================
    //  ATTACK — shoot EnemyProjectiles AT the player
    // ============================================================

    private IEnumerator ShootLoop()
    {
        yield return new WaitForSeconds(1f); // initial delay

        while (!isDead)
        {
            if (playerTransform != null && projectilePrefab != null)
            {
                for (int i = 0; i < projectilesPerBurst; i++)
                {
                    if (isDead) yield break;
                    ShootAtPlayer();
                    yield return new WaitForSeconds(burstDelay);
                }
            }

            yield return new WaitForSeconds(shootInterval);
        }
    }

    private void ShootAtPlayer()
    {
        if (playerTransform == null || projectilePrefab == null) return;

        Vector3 spawnPos = transform.position + Vector3.down * 0.5f;
        Vector2 dir = ((Vector2)(playerTransform.position - spawnPos)).normalized;

        GameObject proj = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);

        // Use the EnemyProjectile.Initialize API that already exists
        EnemyProjectile ep = proj.GetComponent<EnemyProjectile>();
        if (ep != null)
        {
            ep.Initialize(dir, projectileSpeed);
        }
        else
        {
            // Fallback: set velocity manually if EnemyProjectile is missing
            Rigidbody2D rb = proj.GetComponent<Rigidbody2D>();
            if (rb != null) rb.velocity = dir * projectileSpeed;
        }
    }

    // ============================================================
    //  DAMAGE
    // ============================================================

    /// <summary>
    /// Called by Projectile.cs when a player bullet hits this boss.
    /// </summary>
    public void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHealth -= damage;
        if (currentHealth < 0) currentHealth = 0;

        // Update UI
        if (healthSlider != null) healthSlider.value = currentHealth;
        if (hpText != null) hpText.text = $"{currentHealth} / {maxHealth}";

        StartCoroutine(FlashWhite());

        if (currentHealth <= 0)
        {
            StartCoroutine(Die());
        }
    }

    /// <summary>
    /// Legacy hook — kept for backwards compat with Projectile.cs.
    /// </summary>
    public void OnHitByProjectile()
    {
        TakeDamage(15);
    }

    private IEnumerator FlashWhite()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.white;
            yield return new WaitForSeconds(0.06f);
            spriteRenderer.color = originalColor;
        }
    }

    // ============================================================
    //  DEATH
    // ============================================================

    private IEnumerator Die()
    {
        isDead = true;
        StopCoroutine(nameof(StrafeLoop));
        StopCoroutine(nameof(ShootLoop));

        // Destroy health bar
        if (healthBarCanvas != null) Destroy(healthBarCanvas);

        // Explosion particles
        if (explosionPrefab != null)
        {
            Instantiate(explosionPrefab, transform.position, Quaternion.identity);
            Instantiate(explosionPrefab, transform.position + new Vector3(1.2f, -0.8f), Quaternion.identity);
            Instantiate(explosionPrefab, transform.position + new Vector3(-1.2f, -0.8f), Quaternion.identity);
        }

        if (ScreenShakeController.Instance != null)
            ScreenShakeController.Instance.ShakeOnDamage();

        // Tell GameManager
        if (GameManager.Instance != null)
        {
            GameManager.Instance.EndBossFight();
        }

        // Hide and destroy
        if (spriteRenderer != null) spriteRenderer.enabled = false;
        yield return new WaitForSeconds(0.6f);
        Destroy(gameObject);
    }

    // ============================================================
    //  CONTACT DAMAGE (if player touches the boss body)
    // ============================================================

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isDead) return;
        if (other.CompareTag("Player"))
        {
            PlayerHealthController ph = other.GetComponent<PlayerHealthController>();
            if (ph != null && !ph.IsInvincible)
            {
                ph.TakeDamage(20);
                if (ScreenShakeController.Instance != null)
                    ScreenShakeController.Instance.ShakeOnDamage();
            }
        }
    }

    // ============================================================
    //  WORLD-SPACE HEALTH BAR  (child of the boss, counter-scales)
    // ============================================================

    private void BuildWorldSpaceHealthBar()
    {
        healthBarCanvas = new GameObject("BossHP_Canvas");
        healthBarCanvas.transform.SetParent(transform, false);

        Canvas canvas = healthBarCanvas.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.sortingOrder = 100;

        RectTransform cRect = healthBarCanvas.GetComponent<RectTransform>();

        // Counter-scale so UI stays same size regardless of boss scale
        float sx = transform.localScale.x != 0 ? transform.localScale.x : 1f;
        float sy = transform.localScale.y != 0 ? transform.localScale.y : 1f;
        cRect.localScale = new Vector3(1f / sx, 1f / sy, 1f);

        // 6 world-units wide, 0.35 tall, hovering 1 world-unit above boss centre
        cRect.sizeDelta = new Vector2(6f, 0.35f);
        cRect.localPosition = new Vector3(0f, 1f / sy, 0f);

        // --- Dark background ---
        GameObject bg = new GameObject("BG");
        bg.transform.SetParent(healthBarCanvas.transform, false);
        UnityEngine.UI.Image bgImg = bg.AddComponent<UnityEngine.UI.Image>();
        bgImg.color = new Color(0.08f, 0.08f, 0.08f, 0.92f);
        Stretch(bg.GetComponent<RectTransform>());

        // --- Fill area ---
        GameObject fillArea = new GameObject("FillArea");
        fillArea.transform.SetParent(bg.transform, false);
        RectTransform faRect = fillArea.AddComponent<RectTransform>();
        faRect.anchorMin = Vector2.zero; faRect.anchorMax = Vector2.one;
        faRect.offsetMin = new Vector2(0.04f, 0.04f);
        faRect.offsetMax = new Vector2(-0.04f, -0.04f);

        GameObject fill = new GameObject("Fill");
        fill.transform.SetParent(fillArea.transform, false);
        UnityEngine.UI.Image fillImg = fill.AddComponent<UnityEngine.UI.Image>();
        fillImg.color = new Color(0.85f, 0.12f, 0.12f, 1f);
        Stretch(fill.GetComponent<RectTransform>());

        // --- Name label ---
        GameObject nameObj = new GameObject("Name");
        nameObj.transform.SetParent(bg.transform, false);
        TMPro.TextMeshProUGUI nameTMP = nameObj.AddComponent<TMPro.TextMeshProUGUI>();
        nameTMP.text = "SYSTEM ADMIN";
        nameTMP.alignment = TMPro.TextAlignmentOptions.Center;
        nameTMP.color = Color.white;
        nameTMP.fontSize = 0.22f;
        nameTMP.fontStyle = TMPro.FontStyles.Bold;
        nameTMP.enableWordWrapping = false;
        Stretch(nameObj.GetComponent<RectTransform>());

        // --- HP number overlay ---
        GameObject hpObj = new GameObject("HPNum");
        hpObj.transform.SetParent(bg.transform, false);
        hpText = hpObj.AddComponent<TMPro.TextMeshProUGUI>();
        hpText.text = $"{currentHealth} / {maxHealth}";
        hpText.alignment = TMPro.TextAlignmentOptions.Center;
        hpText.color = new Color(1f, 1f, 1f, 0.7f);
        hpText.fontSize = 0.15f;
        hpText.enableWordWrapping = false;
        Stretch(hpObj.GetComponent<RectTransform>());

        // --- Slider component ---
        healthSlider = bg.AddComponent<UnityEngine.UI.Slider>();
        healthSlider.interactable = false;
        healthSlider.transition = UnityEngine.UI.Selectable.Transition.None;
        healthSlider.fillRect = fill.GetComponent<RectTransform>();
        healthSlider.maxValue = maxHealth;
        healthSlider.value = currentHealth;
    }

    private static void Stretch(RectTransform r)
    {
        r.anchorMin = Vector2.zero; r.anchorMax = Vector2.one;
        r.offsetMin = Vector2.zero; r.offsetMax = Vector2.zero;
    }
}
