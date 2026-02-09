using UnityEngine;

/// <summary>
/// Enemy Antivirus (Triangle) - Destroys player on contact, killed by projectiles.
/// Represents the threatening "triangle" shape in psychological shape language.
/// </summary>
public class EnemyAntivirus : MonoBehaviour
{
    [Header("Score")]
    [Tooltip("Points awarded when destroyed")]
    [SerializeField] private int scoreValue = 1;

    [Header("Effects")]
    [Tooltip("Particle effect to spawn on death")]
    [SerializeField] private GameObject deathEffectPrefab;

    [Header("Audio")]
    [Tooltip("Sound to play on death")]
    [SerializeField] private AudioClip deathSound;

    /// <summary>
    /// Called when hit by a projectile.
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
        
        // Play death sound
        if (deathSound != null)
        {
            AudioSource.PlayClipAtPoint(deathSound, transform.position);
        }
        
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
                    
                    // Violent screen shake on damage
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
            
            Debug.Log("Player hit by Antivirus!");
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
