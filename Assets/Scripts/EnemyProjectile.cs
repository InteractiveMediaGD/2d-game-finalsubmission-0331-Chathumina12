using UnityEngine;

/// <summary>
/// Enemy Projectile - Travels in a straight line toward its target direction.
/// Destroys itself after 5 seconds or on hitting the player.
/// Requires a trigger collider to be set up on the GameObject.
/// </summary>
public class EnemyProjectile : MonoBehaviour
{
    [Header("Movement")]
    [Tooltip("Direction the projectile travels (set by EnemyShooter)")]
    private Vector2 moveDirection;
    
    [Tooltip("Speed of the projectile (set by EnemyShooter)")]
    private float speed = 8f;

    [Header("Lifetime")]
    [Tooltip("Time before auto-destroy (seconds)")]
    [SerializeField] private float lifetime = 5f;

    [Header("Damage")]
    [Tooltip("Damage dealt to player on hit")]
    [SerializeField] private int damage = 10;

    [Header("Effects")]
    [Tooltip("Particle effect to spawn on hit")]
    [SerializeField] private GameObject hitEffectPrefab;

    [Header("Audio")]
    [Tooltip("Sound to play on hit")]
    [SerializeField] private AudioClip hitSound;

    private Rigidbody2D rb;
    private bool isInitialized = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        // Auto-destroy after lifetime expires (saves memory)
        Destroy(gameObject, lifetime);
    }

    /// <summary>
    /// Initialize the projectile with a direction and speed.
    /// Called by EnemyShooter when spawning the projectile.
    /// </summary>
    /// <param name="direction">Normalized direction to travel</param>
    /// <param name="projectileSpeed">Speed of the projectile</param>
    public void Initialize(Vector2 direction, float projectileSpeed)
    {
        moveDirection = direction.normalized;
        speed = projectileSpeed;
        isInitialized = true;
        
        // Set velocity using Rigidbody2D if available
        if (rb != null)
        {
            rb.velocity = moveDirection * speed;
        }
        
        // Rotate projectile to face the direction of travel
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    private void Update()
    {
        // If no Rigidbody2D, move manually using Vector2.MoveTowards
        if (rb == null && isInitialized)
        {
            Vector2 targetPosition = (Vector2)transform.position + moveDirection * speed * 100f;
            transform.position = Vector2.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Check if we hit the player
        if (other.CompareTag("Player"))
        {
            // Try to damage the player through PlayerHealthController
            PlayerHealthController playerHealth = other.GetComponent<PlayerHealthController>();
            if (playerHealth != null)
            {
                // Only deal damage if player is not invincible
                if (!playerHealth.IsInvincible)
                {
                    playerHealth.TakeDamage(damage);
                    
                    // Screen shake on damage
                    if (ScreenShakeController.Instance != null)
                    {
                        ScreenShakeController.Instance.ShakeOnDamage();
                    }
                    
                    Debug.Log("Player hit by EnemyProjectile!");
                }
            }
            else
            {
                // Fallback: Instant game over if no health controller
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.GameOver();
                }
            }
            
            // Spawn hit effect
            SpawnHitEffect();
            
            // Destroy this projectile
            Destroy(gameObject);
        }
        
        // Optional: Destroy on hitting walls
        if (other.CompareTag("Wall"))
        {
            SpawnHitEffect();
            Destroy(gameObject);
        }
    }

    private void SpawnHitEffect()
    {
        // Spawn hit effect
        if (hitEffectPrefab != null)
        {
            Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);
        }
        
        // Play hit sound
        if (hitSound != null)
        {
            AudioSource.PlayClipAtPoint(hitSound, transform.position);
        }
    }
}
