using UnityEngine;

/// <summary>
/// Player projectile — flies along transform.right at constant speed.
/// Detects hits on Enemy, EnemyBody (boss), and Wall tags.
/// </summary>
public class Projectile : MonoBehaviour
{
    [Header("Movement")]
    [Tooltip("Speed of the projectile")]
    [SerializeField] private float speed = 20f;

    [Header("Lifetime")]
    [Tooltip("Time before auto-destroy (seconds)")]
    [SerializeField] private float lifetime = 3f;

    [Header("Damage")]
    [Tooltip("Damage dealt to the boss per hit")]
    [SerializeField] private int bossDamage = 15;

    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        // Fly along transform.right (inherits FirePoint rotation)
        if (rb != null)
        {
            rb.velocity = transform.right * speed;
        }
        
        Destroy(gameObject, lifetime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // --- Standard enemies (tagged "Enemy") ---
        if (other.CompareTag("Enemy"))
        {
            EnemyAntivirus enemy = other.GetComponent<EnemyAntivirus>();
            if (enemy != null) enemy.OnHitByProjectile();
            
            EnemyShooter shooter = other.GetComponent<EnemyShooter>();
            if (shooter != null) shooter.OnHitByProjectile();
            
            Destroy(gameObject);
            return;
        }
        
        // --- Boss (tagged "EnemyBody") ---
        if (other.CompareTag("EnemyBody"))
        {
            SystemAdminBoss boss = other.GetComponent<SystemAdminBoss>();
            if (boss != null)
            {
                boss.TakeDamage(bossDamage);
            }
            Destroy(gameObject);
            return;
        }
        
        // --- Walls / firewalls ---
        if (other.CompareTag("Wall"))
        {
            Destroy(gameObject);
        }
    }
}
