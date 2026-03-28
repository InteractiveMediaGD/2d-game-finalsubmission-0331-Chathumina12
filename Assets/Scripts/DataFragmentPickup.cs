using UnityEngine;

/// <summary>
/// A collectible item that rewards the player with "Data Fragments" currency.
/// </summary>
public class DataFragmentPickup : MonoBehaviour
{
    [Header("Economy Settings")]
    [Tooltip("Amount of Data Fragments awarded upon collection")]
    [SerializeField] private int fragmentValue = 5;

    [Header("Effects")]
    [Tooltip("Particle effect prefab spawned on collection")]
    [SerializeField] private GameObject collectionEffect;

    [Tooltip("Sound played on collection")]
    [SerializeField] private AudioClip collectionSound;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Check if the player collided with this pickup
        if (collision.CompareTag("Player"))
        {
            Collect();
        }
    }

    private void Collect()
    {
        // Add fragments via EconomyManager
        if (EconomyManager.Instance != null)
        {
            EconomyManager.Instance.AddFragments(fragmentValue);
        }
        else
        {
            Debug.LogWarning("[DataFragmentPickup] EconomyManager missing in scene! Could not add fragments.");
        }

        // Spawn visual effect
        if (collectionEffect != null)
        {
            Instantiate(collectionEffect, transform.position, Quaternion.identity);
        }

        // Play sound effect
        GameAudio.GotFragment();

        // Destroy the pickup object
        Destroy(gameObject);
    }
}
