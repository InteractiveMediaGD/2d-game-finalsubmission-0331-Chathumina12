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
            // Violent screen shake on damage
            if (ScreenShakeController.Instance != null)
            {
                ScreenShakeController.Instance.ShakeOnDamage();
            }
            
            // Trigger game over
            if (GameManager.Instance != null)
            {
                GameManager.Instance.GameOver();
            }
            
            Debug.Log("Player hit by Antivirus! System Format initiated.");
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Backup collision check (if not using triggers)
        if (collision.gameObject.CompareTag("Player"))
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.GameOver();
            }
        }
    }
}
