using UnityEngine;

/// <summary>
/// Enemy Shooter - Moves to the left (scrolling) and periodically shoots projectiles at the player.
/// Spawns projectiles aimed directly at the player's current position.
/// </summary>
public class EnemyShooter : MonoBehaviour
{
    [Header("Movement")]
    [Tooltip("Speed at which the enemy moves to the left")]
    [SerializeField] private float moveSpeed = 3f;

    [Header("Shooting")]
    [Tooltip("Minimum time between shots (seconds)")]
    [SerializeField] private float minShootInterval = 2f;
    
    [Tooltip("Maximum time between shots (seconds)")]
    [SerializeField] private float maxShootInterval = 4f;
    
    [Tooltip("The projectile prefab to instantiate")]
    [SerializeField] private GameObject projectilePrefab;
    
    [Tooltip("Speed of the fired projectile")]
    [SerializeField] private float projectileSpeed = 8f;

    [Header("Score")]
    [Tooltip("Points awarded when destroyed")]
    [SerializeField] private int scoreValue = 2;

    [Header("Effects")]
    [Tooltip("Particle effect to spawn on death")]
    [SerializeField] private GameObject deathEffectPrefab;

    [Header("Audio")]
    [Tooltip("Sound to play on death")]
    [SerializeField] private AudioClip deathSound;
    
    [Tooltip("Sound to play when shooting")]
    [SerializeField] private AudioClip shootSound;

    private Transform playerTransform;
    private float nextShootTime;
    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void OnEnable()
    {
        // Find the player each time we're enabled (works with object pooling)
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }
        
        // Reset shoot time when enabled
        SetNextShootTime();
        
        // Set velocity to move left
        if (rb != null)
        {
            rb.velocity = new Vector2(-moveSpeed, 0f);
        }
    }

    private void Start()
    {
        // Start is kept for initial setup if not using pooling
        if (playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTransform = player.transform;
            }
        }
    }

    private void Update()
    {
        // If no Rigidbody2D, move manually
        if (rb == null)
        {
            transform.Translate(Vector2.left * moveSpeed * Time.deltaTime);
        }
        
        // Check if it's time to shoot
        if (Time.time >= nextShootTime && playerTransform != null)
        {
            Shoot();
            SetNextShootTime();
        }
    }

    private void SetNextShootTime()
    {
        // Randomize the next shoot time between min and max intervals
        nextShootTime = Time.time + Random.Range(minShootInterval, maxShootInterval);
    }

    private void Shoot()
    {
        if (projectilePrefab == null)
        {
            Debug.LogWarning("EnemyShooter: No projectile prefab assigned!");
            return;
        }
        
        // Calculate direction to player
        Vector2 direction = (playerTransform.position - transform.position).normalized;
        
        // Instantiate the projectile
        GameObject projectile = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
        
        // Set the projectile's direction and speed
        EnemyProjectile enemyProjectile = projectile.GetComponent<EnemyProjectile>();
        if (enemyProjectile != null)
        {
            enemyProjectile.Initialize(direction, projectileSpeed);
        }
        
        // Play shoot sound
        GameAudio.EnemyShoot();
        
        Debug.Log("EnemyShooter fired at player!");
    }

    /// <summary>
    /// Called when hit by a player projectile.
    /// </summary>
    public void OnHitByProjectile()
    {
        // Add score
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddScore(scoreValue);
        }
        
        // Spawn death effect
        if (deathEffectPrefab != null)
        {
            Instantiate(deathEffectPrefab, transform.position, Quaternion.identity);
        }
        
        // Audio
        GameAudio.EnemyHit();
        GameAudio.EnemyDiedShooter();
        
        // Destroy this enemy
        Destroy(gameObject);
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
                    playerHealth.TakeDamage(10);
                    
                    // Screen shake on damage
                    if (ScreenShakeController.Instance != null)
                    {
                        ScreenShakeController.Instance.ShakeOnDamage();
                    }
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
            
            Debug.Log("Player hit by EnemyShooter!");
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Backup collision check (if not using triggers)
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerHealthController playerHealth = collision.gameObject.GetComponent<PlayerHealthController>();
            if (playerHealth != null && !playerHealth.IsInvincible)
            {
                playerHealth.TakeDamage(10);
                
                if (ScreenShakeController.Instance != null)
                {
                    ScreenShakeController.Instance.ShakeOnDamage();
                }
            }
            else if (playerHealth == null)
            {
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.GameOver();
                }
            }
        }
    }
}
