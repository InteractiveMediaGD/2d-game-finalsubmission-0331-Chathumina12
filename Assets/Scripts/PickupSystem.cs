using UnityEngine;

/// <summary>
/// Flexible pickup system that handles different pickup types.
/// Place on a prefab with a Collider2D set to IsTrigger.
/// </summary>
public class PickupSystem : MonoBehaviour
{
    /// <summary>
    /// Types of pickups available in the game.
    /// </summary>
    public enum PickupType
    {
        HealthPack,
        Shield,
        RapidFire
    }

    [Header("Pickup Settings")]
    [Tooltip("Type of pickup effect")]
    [SerializeField] private PickupType pickupType = PickupType.HealthPack;

    [Header("Health Pack Settings")]
    [Tooltip("Amount of health to restore")]
    [SerializeField] private int healthAmount = 20;

    [Header("Rapid Fire Settings")]
    [Tooltip("Duration of rapid fire effect (seconds)")]
    [SerializeField] private float rapidFireDuration = 5f;
    
    [Tooltip("Fire rate multiplier during rapid fire (0.5 = 2x faster)")]
    [SerializeField] private float rapidFireMultiplier = 0.5f;

    [Header("Visual Feedback")]
    [Tooltip("Particle effect to spawn on pickup")]
    [SerializeField] private GameObject pickupEffectPrefab;

    [Header("Audio")]
    [Tooltip("Sound to play on pickup")]
    [SerializeField] private AudioClip pickupSound;

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Check if the player touched the pickup
        if (other.CompareTag("Player"))
        {
            // Apply the pickup effect
            ApplyPickupEffect(other.gameObject);
            
            // Spawn visual effect
            if (pickupEffectPrefab != null)
            {
                Instantiate(pickupEffectPrefab, transform.position, Quaternion.identity);
            }
            
            // Play specific pickup sound based on type
            switch (pickupType)
            {
                case PickupType.HealthPack: GameAudio.GotHealth(); break;
                case PickupType.RapidFire: GameAudio.GotRapidFire(); break;
                case PickupType.Shield: GameAudio.GotShield(); break;
            }
            
            Debug.Log($"Player collected {pickupType} pickup!");
            
            // Destroy the pickup object after collection
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Applies the pickup effect based on the pickup type.
    /// </summary>
    /// <param name="player">The player GameObject</param>
    private void ApplyPickupEffect(GameObject player)
    {
        switch (pickupType)
        {
            case PickupType.HealthPack:
                ApplyHealthPack();
                break;
                
            case PickupType.Shield:
                ApplyShield(player);
                break;
                
            case PickupType.RapidFire:
                ApplyRapidFire(player);
                break;
        }
    }

    /// <summary>
    /// Restores health to the player using GameManager.
    /// </summary>
    private void ApplyHealthPack()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ModifyHealth(healthAmount);
            Debug.Log($"Health restored by {healthAmount}!");
        }
        else
        {
            Debug.LogWarning("PickupSystem: GameManager not found for HealthPack!");
        }
    }

    /// <summary>
    /// Activates a shield on the player that blocks the next 1 hit.
    /// </summary>
    /// <param name="player">The player GameObject</param>
    private void ApplyShield(GameObject player)
    {
        PlayerHealthController healthController = player.GetComponent<PlayerHealthController>();
        if (healthController != null)
        {
            healthController.ActivateShield();
            Debug.Log("Shield activated! Next hit will be blocked.");
        }
        else
        {
            Debug.LogWarning("PickupSystem: PlayerHealthController not found for Shield!");
        }
    }

    /// <summary>
    /// Temporarily decreases the weapon cooldown on the player's shooting script.
    /// </summary>
    /// <param name="player">The player GameObject</param>
    private void ApplyRapidFire(GameObject player)
    {
        VirusController virusController = player.GetComponent<VirusController>();
        if (virusController != null)
        {
            virusController.ActivateRapidFire(rapidFireDuration, rapidFireMultiplier);
            Debug.Log($"Rapid Fire activated for {rapidFireDuration} seconds!");
        }
        else
        {
            Debug.LogWarning("PickupSystem: VirusController not found for RapidFire!");
        }
    }
}
