using UnityEngine;

/// <summary>
/// Projectile behavior - flies forward at high speed and destroys enemies.
/// </summary>
public class Projectile : MonoBehaviour
{
    [Header("Movement")]
    [Tooltip("Speed of the projectile")]
    [SerializeField] private float speed = 20f;

    [Header("Lifetime")]
    [Tooltip("Time before auto-destroy (seconds)")]
    [SerializeField] private float lifetime = 3f;

    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        // Set velocity to move right
        if (rb != null)
        {
            rb.velocity = new Vector2(speed, 0f);
        }
        
        // Auto-destroy after lifetime expires
        Destroy(gameObject, lifetime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Check if we hit an enemy
        if (other.CompareTag("Enemy"))
        {
            // Try EnemyAntivirus first
            EnemyAntivirus enemy = other.GetComponent<EnemyAntivirus>();
            if (enemy != null)
            {
                enemy.OnHitByProjectile();
            }
            
            // Also try EnemyShooter
            EnemyShooter shooter = other.GetComponent<EnemyShooter>();
            if (shooter != null)
            {
                shooter.OnHitByProjectile();
            }
            
            // Destroy this projectile
            Destroy(gameObject);
        }
        
        // Optional: Destroy on hitting walls (firewalls)
        if (other.CompareTag("Wall"))
        {
            Destroy(gameObject);
        }
    }
}
