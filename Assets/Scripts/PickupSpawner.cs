using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Spawns pickups (Health, Shield, RapidFire) at random intervals during gameplay.
/// Can be used standalone or integrated with the level generation system.
/// </summary>
public class PickupSpawner : MonoBehaviour
{
    [Header("Pickup Prefabs")]
    [Tooltip("HealthPack pickup prefab")]
    [SerializeField] private GameObject healthPackPrefab;
    
    [Tooltip("Shield pickup prefab")]
    [SerializeField] private GameObject shieldPrefab;
    
    [Tooltip("RapidFire pickup prefab")]
    [SerializeField] private GameObject rapidFirePrefab;

    [Header("Spawn Settings")]
    [Tooltip("Minimum time between pickup spawns (seconds)")]
    [SerializeField] private float minSpawnInterval = 5f;
    
    [Tooltip("Maximum time between pickup spawns (seconds)")]
    [SerializeField] private float maxSpawnInterval = 10f;
    
    [Tooltip("Chance to spawn a pickup when timer triggers (0-1)")]
    [SerializeField] private float spawnChance = 1.0f;

    [Header("Spawn Position")]
    [Tooltip("Minimum Y position for pickup spawn")]
    [SerializeField] private float minY = -3f;
    
    [Tooltip("Maximum Y position for pickup spawn")]
    [SerializeField] private float maxY = 3f;
    
    [Tooltip("How far ahead of the player to spawn pickups")]
    [SerializeField] private float spawnDistanceAhead = 15f;

    [Header("Pickup Weights")]
    [Tooltip("Spawn weight for HealthPack (higher = more likely)")]
    [SerializeField] private float healthPackWeight = 50f;
    
    [Tooltip("Spawn weight for Shield (higher = more likely)")]
    [SerializeField] private float shieldWeight = 30f;
    
    [Tooltip("Spawn weight for RapidFire (higher = more likely)")]
    [SerializeField] private float rapidFireWeight = 20f;

    [Header("Player Reference")]
    [Tooltip("Reference to the player transform")]
    [SerializeField] private Transform player;

    [Header("Despawn Settings")]
    [Tooltip("Distance behind player to destroy pickups")]
    [SerializeField] private float despawnDistance = 10f;

    // Internal tracking
    private float nextSpawnTime;
    private List<GameObject> activePickups = new List<GameObject>();
    private bool hasLoggedPlayerWarning = false;
    private bool hasLoggedPrefabWarning = false;

    private void Start()
    {
        // Find player if not assigned
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
                Debug.Log("PickupSpawner: Player found automatically!");
            }
            else
            {
                Debug.LogError("PickupSpawner: Player not found! Make sure Player has 'Player' tag.");
            }
        }
        
        // Check if prefabs are assigned
        if (healthPackPrefab == null && shieldPrefab == null && rapidFirePrefab == null)
        {
            Debug.LogError("PickupSpawner: No pickup prefabs assigned! Drag prefabs into the Inspector.");
        }
        else
        {
            Debug.Log($"PickupSpawner: Ready! HealthPack={healthPackPrefab != null}, Shield={shieldPrefab != null}, RapidFire={rapidFirePrefab != null}");
        }
        
        // Set initial spawn time (spawn quickly for first pickup)
        nextSpawnTime = Time.time + 3f; // First spawn after 3 seconds
        Debug.Log($"PickupSpawner: First pickup will spawn in 3 seconds");
    }

    private void Update()
    {
        // Check player reference
        if (player == null)
        {
            if (!hasLoggedPlayerWarning)
            {
                Debug.LogWarning("PickupSpawner: No player reference! Pickups won't spawn.");
                hasLoggedPlayerWarning = true;
            }
            return;
        }
        
        // Skip if game over
        if (GameManager.Instance != null && GameManager.Instance.IsGameOver) return;
        
        // Check if it's time to spawn
        if (Time.time >= nextSpawnTime)
        {
            Debug.Log($"PickupSpawner: Attempting to spawn pickup... (Time: {Time.time:F1})");
            TrySpawnPickup();
            SetNextSpawnTime();
        }
        
        // Clean up passed pickups
        CleanupPickups();
    }

    /// <summary>
    /// Sets the next spawn time based on random interval.
    /// </summary>
    private void SetNextSpawnTime()
    {
        nextSpawnTime = Time.time + Random.Range(minSpawnInterval, maxSpawnInterval);
    }

    /// <summary>
    /// Attempts to spawn a pickup based on spawn chance.
    /// </summary>
    private void TrySpawnPickup()
    {
        // Random chance check
        if (Random.value > spawnChance) return;
        
        // Get a random pickup type based on weights
        GameObject pickupPrefab = GetRandomPickupPrefab();
        if (pickupPrefab == null)
        {
            Debug.LogWarning("PickupSpawner: No pickup prefabs assigned!");
            return;
        }
        
        // Calculate spawn position
        float spawnX = player.position.x + spawnDistanceAhead;
        float spawnY = Random.Range(minY, maxY);
        Vector3 spawnPosition = new Vector3(spawnX, spawnY, 0f);
        
        // Spawn the pickup
        GameObject pickup = Instantiate(pickupPrefab, spawnPosition, Quaternion.identity);
        activePickups.Add(pickup);
        
        Debug.Log($"Pickup spawned at {spawnPosition}!");
    }

    /// <summary>
    /// Gets a random pickup prefab based on configured weights.
    /// </summary>
    /// <returns>The selected pickup prefab</returns>
    private GameObject GetRandomPickupPrefab()
    {
        // Calculate total weight
        float totalWeight = 0f;
        
        if (healthPackPrefab != null) totalWeight += healthPackWeight;
        if (shieldPrefab != null) totalWeight += shieldWeight;
        if (rapidFirePrefab != null) totalWeight += rapidFireWeight;
        
        if (totalWeight <= 0f) return null;
        
        // Random selection
        float randomValue = Random.Range(0f, totalWeight);
        float currentWeight = 0f;
        
        // Check HealthPack
        if (healthPackPrefab != null)
        {
            currentWeight += healthPackWeight;
            if (randomValue <= currentWeight)
            {
                return healthPackPrefab;
            }
        }
        
        // Check Shield
        if (shieldPrefab != null)
        {
            currentWeight += shieldWeight;
            if (randomValue <= currentWeight)
            {
                return shieldPrefab;
            }
        }
        
        // Check RapidFire
        if (rapidFirePrefab != null)
        {
            return rapidFirePrefab;
        }
        
        return null;
    }

    /// <summary>
    /// Removes pickups that have passed behind the player.
    /// </summary>
    private void CleanupPickups()
    {
        for (int i = activePickups.Count - 1; i >= 0; i--)
        {
            GameObject pickup = activePickups[i];
            
            // Remove if already collected (destroyed)
            if (pickup == null)
            {
                activePickups.RemoveAt(i);
                continue;
            }
            
            // Destroy if behind player
            if (pickup.transform.position.x < player.position.x - despawnDistance)
            {
                Destroy(pickup);
                activePickups.RemoveAt(i);
            }
        }
    }

    /// <summary>
    /// Called by LevelGenerator to spawn a pickup at a specific position.
    /// </summary>
    /// <param name="xPosition">X position to spawn at</param>
    /// <param name="yPosition">Y position to spawn at</param>
    public void SpawnPickupAt(float xPosition, float yPosition)
    {
        if (Random.value > spawnChance) return;
        
        GameObject pickupPrefab = GetRandomPickupPrefab();
        if (pickupPrefab == null) return;
        
        Vector3 spawnPosition = new Vector3(xPosition, yPosition, 0f);
        GameObject pickup = Instantiate(pickupPrefab, spawnPosition, Quaternion.identity);
        activePickups.Add(pickup);
    }

    /// <summary>
    /// Resets all pickups (for game restart).
    /// </summary>
    public void ResetPickups()
    {
        foreach (GameObject pickup in activePickups)
        {
            if (pickup != null)
            {
                Destroy(pickup);
            }
        }
        activePickups.Clear();
        SetNextSpawnTime();
    }
}
