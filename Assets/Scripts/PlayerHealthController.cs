using UnityEngine;
using System.Collections;

/// <summary>
/// Handles player health, damage, invincibility frames, and collision detection.
/// Attach this to the player object (Virus).
/// </summary>
public class PlayerHealthController : MonoBehaviour
{
    [Header("Damage Settings")]
    [Tooltip("Damage taken from enemy collisions")]
    [SerializeField] private int collisionDamage = 10;

    [Header("Invincibility Settings")]
    [Tooltip("Duration of invincibility after being hit (seconds)")]
    [SerializeField] private float invincibilityDuration = 1.5f;
    
    [Tooltip("How fast the sprite flashes during invincibility")]
    [SerializeField] private float flashSpeed = 0.1f;

    [Header("References")]
    [Tooltip("SpriteRenderer for flash effect (auto-detected if not assigned)")]
    [SerializeField] private SpriteRenderer spriteRenderer;

    // State tracking
    private bool isInvincible = false;
    private Coroutine invincibilityCoroutine;
    
    // Shield system
    private bool isShielded = false;
    
    [Header("Shield Settings")]
    [Tooltip("Visual GameObject for the shield (child of player, set active when shielded)")]
    [SerializeField] private GameObject shieldVisual;

    // Public accessors
    public bool IsInvincible => isInvincible;
    public bool IsShielded => isShielded;

    private void Awake()
    {
        // Auto-detect SpriteRenderer if not assigned
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            
            if (spriteRenderer == null)
            {
                Debug.LogWarning("PlayerHealthController: No SpriteRenderer found. Flash effect will not work.");
            }
        }
    }

    /// <summary>
    /// Deals damage to the player through the GameManager.
    /// Triggers invincibility frames if not already invincible.
    /// Shield absorbs the hit if active.
    /// </summary>
    /// <param name="damage">Amount of damage to deal</param>
    public void TakeDamage(int damage)
    {
        // Ignore damage if invincible
        if (isInvincible) return;
        
        // Shield absorbs the hit
        if (isShielded)
        {
            DeactivateShield();
            Debug.Log("Shield absorbed the hit!");
            
            // Brief invincibility after shield breaks
            StartInvincibility();
            return;
        }

        // Apply damage through GameManager
        if (GameManager.Instance != null)
        {
            GameManager.Instance.TakeDamage(damage);
            Debug.Log($"Player took {damage} damage! Health: {GameManager.Instance.PlayerHealth}");
        }
        else
        {
            Debug.LogWarning("PlayerHealthController: GameManager not found!");
        }

        // Start invincibility frames
        StartInvincibility();
    }

    /// <summary>
    /// Starts the invincibility period with sprite flashing.
    /// </summary>
    private void StartInvincibility()
    {
        // Stop any existing invincibility coroutine
        if (invincibilityCoroutine != null)
        {
            StopCoroutine(invincibilityCoroutine);
        }

        invincibilityCoroutine = StartCoroutine(InvincibilityCoroutine());
    }

    /// <summary>
    /// Coroutine that handles invincibility duration and sprite flashing.
    /// </summary>
    private IEnumerator InvincibilityCoroutine()
    {
        isInvincible = true;
        
        float elapsedTime = 0f;
        bool isVisible = true;

        // Flash the sprite during invincibility
        while (elapsedTime < invincibilityDuration)
        {
            // Toggle sprite visibility for flash effect
            if (spriteRenderer != null)
            {
                isVisible = !isVisible;
                Color color = spriteRenderer.color;
                color.a = isVisible ? 1f : 0.3f; // Flash between full and 30% opacity
                spriteRenderer.color = color;
            }

            yield return new WaitForSeconds(flashSpeed);
            elapsedTime += flashSpeed;
        }

        // Restore full visibility
        if (spriteRenderer != null)
        {
            Color color = spriteRenderer.color;
            color.a = 1f;
            spriteRenderer.color = color;
        }

        isInvincible = false;
        invincibilityCoroutine = null;
        
        Debug.Log("Invincibility ended.");
    }

    /// <summary>
    /// Handles collision with enemy projectiles and bodies.
    /// </summary>
    private void OnTriggerEnter2D(Collider2D other)
    {
        HandleCollision(other.gameObject, other.tag);
    }

    /// <summary>
    /// Handles physical collision with enemy objects.
    /// </summary>
    private void OnCollisionEnter2D(Collision2D collision)
    {
        HandleCollision(collision.gameObject, collision.gameObject.tag);
    }

    /// <summary>
    /// Processes collision with enemy objects.
    /// </summary>
    /// <param name="hitObject">The object that was hit</param>
    /// <param name="tag">Tag of the hit object</param>
    private void HandleCollision(GameObject hitObject, string tag)
    {
        // Safety check to dynamically identify any firewalls even if their tags were left blank
        bool isFirewall = hitObject.GetComponentInParent<FirewallSegment>() != null;

        // Check if we hit an enemy projectile, body, or the lethal firewalls!
        if (tag == "EnemyProjectile" || tag == "EnemyBody" || tag == "Obstacle" || tag == "Firewall" || isFirewall)
        {
            // Take damage
            TakeDamage(collisionDamage);

            // Destroy the projectile (but not enemy bodies or massive firewalls)
            if (tag == "EnemyProjectile")
            {
                Destroy(hitObject);
                Debug.Log("Enemy projectile destroyed on player hit.");
            }
        }
    }

    /// <summary>
    /// Forces invincibility state (useful for power-ups or special abilities).
    /// </summary>
    /// <param name="duration">Custom duration for invincibility</param>
    public void ForceInvincibility(float duration)
    {
        if (invincibilityCoroutine != null)
        {
            StopCoroutine(invincibilityCoroutine);
        }

        invincibilityCoroutine = StartCoroutine(CustomInvincibilityCoroutine(duration));
    }

    /// <summary>
    /// Custom duration invincibility coroutine.
    /// </summary>
    private IEnumerator CustomInvincibilityCoroutine(float duration)
    {
        isInvincible = true;
        
        float elapsedTime = 0f;
        bool isVisible = true;

        while (elapsedTime < duration)
        {
            if (spriteRenderer != null)
            {
                isVisible = !isVisible;
                Color color = spriteRenderer.color;
                color.a = isVisible ? 1f : 0.3f;
                spriteRenderer.color = color;
            }

            yield return new WaitForSeconds(flashSpeed);
            elapsedTime += flashSpeed;
        }

        if (spriteRenderer != null)
        {
            Color color = spriteRenderer.color;
            color.a = 1f;
            spriteRenderer.color = color;
        }

        isInvincible = false;
        invincibilityCoroutine = null;
    }

    #region Shield System
    /// <summary>
    /// Activates the shield that blocks the next 1 hit.
    /// Called by the PickupSystem when collecting a Shield pickup.
    /// </summary>
    public void ActivateShield()
    {
        isShielded = true;
        
        // Show shield visual if assigned
        if (shieldVisual != null)
        {
            shieldVisual.SetActive(true);
        }
        
        Debug.Log("Shield activated!");
    }

    /// <summary>
    /// Deactivates the shield after it has blocked a hit.
    /// </summary>
    private void DeactivateShield()
    {
        isShielded = false;
        
        // Hide shield visual if assigned
        if (shieldVisual != null)
        {
            shieldVisual.SetActive(false);
        }
        
        Debug.Log("Shield deactivated!");
    }
    #endregion
}
