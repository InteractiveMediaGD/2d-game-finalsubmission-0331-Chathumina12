using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Controls the Player (Virus) with auto-movement on X-axis
/// and player-controlled vertical movement on Y-axis.
/// Now includes shooting capability.
/// </summary>
public class VirusController : MonoBehaviour
{
    [Header("Movement Settings")]
    [Tooltip("Constant horizontal speed (auto-run)")]
    [SerializeField] private float currentSpeed = 5f;
    
    /// <summary>
    /// Public accessor for speed (used by BackgroundScroller).
    /// Returns GameManager scroll speed when available.
    /// </summary>
    public float CurrentSpeed => GameManager.Instance != null ? GameManager.Instance.ScrollSpeed : currentSpeed;
    
    [Tooltip("Vertical movement speed")]
    [SerializeField] private float verticalSpeed = 5f;

    [Header("Screen Bounds")]
    [Tooltip("Minimum Y position (bottom of play area)")]
    [SerializeField] private float minY = -4f;
    
    [Tooltip("Maximum Y position (top of play area)")]
    [SerializeField] private float maxY = 4f;

    [Header("Shooting Settings")]
    [Tooltip("Projectile prefab to fire")]
    [SerializeField] private GameObject projectilePrefab;
    
    [Tooltip("Transform where projectiles spawn (should be in front of player)")]
    [SerializeField] private Transform firePoint;
    
    [Tooltip("Minimum time between shots (seconds)")]
    [SerializeField] private float fireRate = 0.25f;

    // Components
    private Rigidbody2D rb;
    
    // Input
    private float inputY = 0f;
    private float inputX = 0f;
    
    [Header("Boss Fight Tracking")]
    public bool isBossFightActive = false;
    private float arenaMinX;
    private float arenaMaxX;

    [Header("Scale Override")]
    [Tooltip("Enable to force-apply overrideScale at Awake (fixes tiny robot in environment)")]
    [SerializeField] private bool applyScaleOverride = true;
    
    [Tooltip("Uniform scale to apply to the player robot at startup. Set to (1,1,1) to keep prefab default.")]
    [SerializeField] private Vector3 overrideScale = new Vector3(1.5f, 1.5f, 1f);

    // Shooting
    private float nextFireTime = 0f;

    private void Awake()
    {
        // Cache the Rigidbody2D component
        rb = GetComponent<Rigidbody2D>();
        
        if (rb == null)
        {
            Debug.LogError("VirusController: Rigidbody2D component is missing!");
        }
        
        // Apply scale correction so the robot player is visually proportionate
        // to the firewall environment. Adjustable from the Inspector.
        if (applyScaleOverride && overrideScale != Vector3.zero)
        {
            transform.localScale = overrideScale;
            Debug.Log($"[VirusController] Scale override applied: {overrideScale}");
        }
        
        // Create firePoint if not assigned
        if (firePoint == null)
        {
            GameObject fp = new GameObject("FirePoint");
            fp.transform.SetParent(transform);
            fp.transform.localPosition = new Vector3(0.5f, 0f, 0f);
            firePoint = fp.transform;
        }
    }

    [HideInInspector]
    public GameObject deathParticlePrefab; // Assigned dynamically by SkinManager

    private void Start()
    {
        ApplyEquippedSkin();
    }

    /// <summary>
    /// Fetches the currently equipped skin from SkinManager and applies its visuals dynamically.
    /// </summary>
    private void ApplyEquippedSkin()
    {
        if (SkinManager.Instance != null)
        {
            SkinManager.Instance.ApplySkinToPlayer(this.gameObject);
        }
    }

    /// <summary>
    /// Optional hook for external systems (like HealthController) to spawn the actual death particles properly.
    /// </summary>
    public void SpawnDeathParticles()
    {
        if (deathParticlePrefab != null)
        {
            Instantiate(deathParticlePrefab, transform.position, Quaternion.identity);
        }
    }

    private void Update()
    {
        if (Camera.main != null && firePoint != null && Mouse.current != null)
        {
            // Capture mouse position using the New Input System
            Vector3 screenMousePos = Mouse.current.position.ReadValue();
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(screenMousePos);
            mousePosition.z = 0f;
            
            // Calculate direction from player to cursor
            Vector3 direction = (mousePosition - transform.position).normalized;
            
            // Calculate angle and apply INSTANTLY — no lerp lag
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            firePoint.rotation = Quaternion.Euler(0, 0, angle);
            
            // Allow left mouse button to fire as well
            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                Fire();
            }
        }
    }

    private void FixedUpdate()
    {
        float speedMultiplier = GameManager.Instance != null ? (GameManager.Instance.ScrollSpeed / currentSpeed) : 1f;

        if (isBossFightActive)
        {
            // Explicitly read legacy Input axes so horizontal works without Input System mapping
            float hInput = Input.GetAxis("Horizontal");
            float vInput = Input.GetAxis("Vertical");  
            
            // Also merge any New Input System values that might be coming in
            if (Mathf.Abs(inputX) > 0.01f) hInput = inputX;
            if (Mathf.Abs(inputY) > 0.01f) vInput = inputY;
            
            rb.velocity = new Vector2(hInput * verticalSpeed, vInput * verticalSpeed);
        }
        else
        {
            // Auto-run: Strictly apply velocity to the X-axis based on game scrolling speed
            float speed = currentSpeed;
            if (GameManager.Instance != null)
            {
                speed = GameManager.Instance.ScrollSpeed;
            }
            rb.velocity = new Vector2(speed, inputY * verticalSpeed * speedMultiplier);
        }

        // Clamp position to keep player within screen bounds
        ClampPosition();
    }

    /// <summary>
    /// Clamps using Camera.main.ScreenToWorldPoint so the player NEVER leaves the visible screen.
    /// </summary>
    private void ClampPosition()
    {
        if (Camera.main != null)
        {
            // Calculate exact world-space screen edges from the camera viewport
            Vector3 bottomLeft = Camera.main.ScreenToWorldPoint(new Vector3(0, 0, 0));
            Vector3 topRight = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, 0));
            
            // Add small padding so the sprite doesn't clip the edge
            float padding = 0.5f;
            
            Vector3 pos = transform.position;
            pos.y = Mathf.Clamp(pos.y, bottomLeft.y + padding, topRight.y - padding);
            
            if (isBossFightActive)
            {
                pos.x = Mathf.Clamp(pos.x, bottomLeft.x + padding, topRight.x - padding);
            }
            
            transform.position = pos;
        }
        else
        {
            // Fallback to hardcoded bounds
            Vector3 pos = transform.position;
            pos.y = Mathf.Clamp(pos.y, minY, maxY);
            if (isBossFightActive)
            {
                pos.x = Mathf.Clamp(pos.x, arenaMinX, arenaMaxX);
            }
            transform.position = pos;
        }
    }

    /// <summary>
    /// Toggles boss fight movement rules.
    /// </summary>
    public void SetBossFightMode(bool isActive)
    {
        isBossFightActive = isActive;
        if (isActive)
        {
            // Reset lingering inputs
            inputX = 0f;
            if (rb != null) rb.velocity = Vector2.zero;
            Debug.Log("[VirusController] Boss Fight Mode ON — full movement unlocked, clamping via Camera viewport.");
        }
        else
        {
            inputX = 0f;
            Debug.Log("[VirusController] Boss Fight Mode OFF — returning to auto-runner.");
        }
    }

    /// <summary>
    /// Called by the Input System when vertical movement input is received.
    /// </summary>
    /// <param name="context">Input action callback context</param>
    public void OnVerticalMove(InputAction.CallbackContext context)
    {
        // Read the vertical input value (-1 to 1)
        inputY = context.ReadValue<float>();
    }

    /// <summary>
    /// Called by the Input System when horizontal movement input is received.
    /// Map this to your Move action's X-axis (A/D or Left/Right arrows).
    /// </summary>
    public void OnHorizontalMove(InputAction.CallbackContext context)
    {
        inputX = context.ReadValue<float>();
    }

    /// <summary>
    /// Called by the Input System when Fire input is received (Spacebar).
    /// </summary>
    /// <param name="context">Input action callback context</param>
    public void OnFire(InputAction.CallbackContext context)
    {
        // Only fire on button press (not release)
        if (context.performed)
        {
            Fire();
        }
    }

    /// <summary>
    /// Fires a projectile at the FirePoint position with its current rotation toward the mouse.
    /// Speed is consistent regardless of angle since Projectile uses transform.right * speed.
    /// </summary>
    private void Fire()
    {
        // Check fire rate
        if (Time.time < nextFireTime) return;
        
        // Check if we have a projectile prefab
        if (projectilePrefab == null)
        {
            Debug.LogWarning("VirusController: No projectile prefab assigned!");
            return;
        }
        
        // Spawn projectile at fire point with the FirePoint's exact rotation
        Vector3 spawnPos = firePoint != null ? firePoint.position : transform.position;
        Quaternion spawnRot = firePoint != null ? firePoint.rotation : Quaternion.identity;
        Instantiate(projectilePrefab, spawnPos, spawnRot);
        
        // Play shooting sound
        GameAudio.PlayerShoot();
        
        // Trigger screen shake (subtle)
        if (ScreenShakeController.Instance != null)
        {
            ScreenShakeController.Instance.ShakeOnShoot();
        }
        
        // Set next fire time
        nextFireTime = Time.time + fireRate;
    }

    /// <summary>
    /// Alternative: Called by legacy Input System or for direct value setting.
    /// </summary>
    /// <param name="value">Vertical input value from -1 to 1</param>
    public void SetVerticalInput(float value)
    {
        inputY = value;
    }

    /// <summary>
    /// Alternative: Called by legacy Input System or for direct value setting.
    /// </summary>
    public void SetHorizontalInput(float value)
    {
        inputX = value;
    }

    /// <summary>
    /// Updates screen bounds. Call this if your camera or play area changes.
    /// </summary>
    /// <param name="newMinY">New minimum Y bound</param>
    /// <param name="newMaxY">New maximum Y bound</param>
    public void SetScreenBounds(float newMinY, float newMaxY)
    {
        minY = newMinY;
        maxY = newMaxY;
    }

    #region Rapid Fire System
    // Rapid fire state
    private bool isRapidFireActive = false;
    private float normalFireRate;
    private Coroutine rapidFireCoroutine;

    /// <summary>
    /// Activates rapid fire mode, decreasing the weapon cooldown temporarily.
    /// Called by the PickupSystem when collecting a RapidFire pickup.
    /// </summary>
    /// <param name="duration">How long rapid fire lasts (seconds)</param>
    /// <param name="multiplier">Fire rate multiplier (0.5 = 2x faster)</param>
    public void ActivateRapidFire(float duration, float multiplier)
    {
        // Cancel existing rapid fire if active
        if (rapidFireCoroutine != null)
        {
            StopCoroutine(rapidFireCoroutine);
        }
        
        rapidFireCoroutine = StartCoroutine(RapidFireCoroutine(duration, multiplier));
    }

    /// <summary>
    /// Coroutine that handles the rapid fire duration and restoration.
    /// </summary>
    private System.Collections.IEnumerator RapidFireCoroutine(float duration, float multiplier)
    {
        // Store normal fire rate if not already in rapid fire
        if (!isRapidFireActive)
        {
            normalFireRate = fireRate;
        }
        
        // Apply rapid fire
        isRapidFireActive = true;
        fireRate = normalFireRate * multiplier;
        
        Debug.Log($"Rapid Fire activated! Fire rate: {fireRate} (was {normalFireRate})");
        
        // Wait for duration
        yield return new WaitForSeconds(duration);
        
        // Restore normal fire rate
        fireRate = normalFireRate;
        isRapidFireActive = false;
        rapidFireCoroutine = null;
        
        Debug.Log($"Rapid Fire ended. Fire rate restored to: {fireRate}");
    }
    #endregion
}

