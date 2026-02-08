using UnityEngine;

/// <summary>
/// Detects when player passes through a barrier gap with minimal clearance.
/// Triggers a "Close Call" screen shake effect for dramatic tension.
/// Attach this to the Player.
/// </summary>
public class CloseCallDetector : MonoBehaviour
{
    [Header("Detection Settings")]
    [Tooltip("Distance threshold for a 'close call' (units from wall edge)")]
    [SerializeField] private float closeCallThreshold = 0.5f;
    
    [Tooltip("Cooldown between close call triggers (seconds)")]
    [SerializeField] private float closeCallCooldown = 1f;

    [Header("Visual Feedback")]
    [Tooltip("Particle effect to spawn on close call")]
    [SerializeField] private GameObject closeCallEffectPrefab;

    [Header("Audio")]
    [Tooltip("Sound to play on close call")]
    [SerializeField] private AudioClip closeCallSound;

    private float lastCloseCallTime = -999f;
    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        // Only check against walls
        if (!other.CompareTag("Wall")) return;
        
        // Check cooldown
        if (Time.time - lastCloseCallTime < closeCallCooldown) return;

        // Calculate distance to the wall edge
        float distance = CalculateDistanceToCollider(other);

        // If within close call threshold, trigger effect
        if (distance > 0 && distance < closeCallThreshold)
        {
            TriggerCloseCall();
        }
    }

    /// <summary>
    /// Calculates the closest distance from player to the collider edge.
    /// </summary>
    private float CalculateDistanceToCollider(Collider2D other)
    {
        // Get closest point on the other collider to our position
        Vector2 closestPoint = other.ClosestPoint(transform.position);
        float distance = Vector2.Distance(transform.position, closestPoint);
        
        // Account for our own collider size
        Collider2D myCollider = GetComponent<Collider2D>();
        if (myCollider != null)
        {
            Vector2 myClosestPoint = myCollider.ClosestPoint(closestPoint);
            distance = Vector2.Distance(myClosestPoint, closestPoint);
        }

        return distance;
    }

    /// <summary>
    /// Triggers the close call effects.
    /// </summary>
    private void TriggerCloseCall()
    {
        lastCloseCallTime = Time.time;

        // Screen shake
        if (ScreenShakeController.Instance != null)
        {
            ScreenShakeController.Instance.ShakeOnCloseCall();
        }

        // Spawn visual effect
        if (closeCallEffectPrefab != null)
        {
            Instantiate(closeCallEffectPrefab, transform.position, Quaternion.identity);
        }

        // Play sound
        if (closeCallSound != null)
        {
            if (audioSource != null)
            {
                audioSource.PlayOneShot(closeCallSound);
            }
            else
            {
                AudioSource.PlayClipAtPoint(closeCallSound, transform.position);
            }
        }

        Debug.Log("CLOSE CALL! Near miss detected!");
    }
}
