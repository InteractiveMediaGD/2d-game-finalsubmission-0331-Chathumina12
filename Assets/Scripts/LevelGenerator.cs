using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Spawns and manages Firewall barriers using object pooling.
/// Recycles barriers as they pass behind the player to prevent memory leaks.
/// </summary>
public class LevelGenerator : MonoBehaviour
{
    [Header("Prefab Reference")]
    [Tooltip("The Firewall_Segment prefab to spawn")]
    [SerializeField] private GameObject firewallPrefab;

    [Header("Pool Settings")]
    [Tooltip("Number of segments to pre-instantiate")]
    [SerializeField] private int poolSize = 5;

    [Header("Spawn Settings")]
    [Tooltip("Distance ahead of player to spawn segments")]
    [SerializeField] private float spawnDistance = 15f;
    
    [Tooltip("Horizontal spacing between consecutive segments")]
    [SerializeField] private float segmentSpacing = 8f;

    [Header("Gap Settings")]
    [Tooltip("Vertical size of the gap opening")]
    [SerializeField] private float gapSize = 3f;
    
    [Tooltip("Minimum Y position for gap center")]
    [SerializeField] private float minGapY = -3f;
    
    [Tooltip("Maximum Y position for gap center")]
    [SerializeField] private float maxGapY = 3f;

    [Header("Despawn Settings")]
    [Tooltip("Distance behind player to recycle segments")]
    [SerializeField] private float despawnDistance = 5f;

    [Header("Player Reference")]
    [Tooltip("Reference to the player transform (Virus)")]
    [SerializeField] private Transform player;

    [Header("Enemy Spawner Reference")]
    [Tooltip("Reference to the EnemySpawner for spawning enemies between barriers")]
    [SerializeField] private EnemySpawner enemySpawner;

    // Object pool
    private Queue<GameObject> segmentPool;
    
    // Track the X position of the furthest spawned segment
    private float furthestSpawnX;
    
    // List of active segments for recycling checks
    private List<GameObject> activeSegments;

    private void Start()
    {
        InitializePool();
        SpawnInitialSegments();
    }

    private void Update()
    {
        if (player == null) return;
        
        // Spawn new segments if player is approaching the furthest one
        while (player.position.x + spawnDistance > furthestSpawnX)
        {
            SpawnSegment();
        }
        
        // Check for segments to recycle
        RecyclePassedSegments();
    }

    /// <summary>
    /// Pre-instantiates all segment objects and adds them to the pool.
    /// </summary>
    private void InitializePool()
    {
        segmentPool = new Queue<GameObject>();
        activeSegments = new List<GameObject>();
        
        for (int i = 0; i < poolSize; i++)
        {
            GameObject segment = Instantiate(firewallPrefab, transform);
            segment.SetActive(false);
            segmentPool.Enqueue(segment);
        }
    }

    /// <summary>
    /// Spawns the initial set of segments when the game starts.
    /// </summary>
    private void SpawnInitialSegments()
    {
        if (player == null)
        {
            Debug.LogError("LevelGenerator: Player reference is not assigned!");
            return;
        }
        
        // Start spawning from ahead of the player
        furthestSpawnX = player.position.x + segmentSpacing;
        
        // Fill the visible area with segments
        int initialCount = Mathf.Min(poolSize, Mathf.CeilToInt(spawnDistance / segmentSpacing));
        for (int i = 0; i < initialCount; i++)
        {
            SpawnSegment();
        }
    }

    /// <summary>
    /// Spawns a segment from the pool at the next position.
    /// </summary>
    private void SpawnSegment()
    {
        if (segmentPool.Count == 0)
        {
            // Pool exhausted - this shouldn't happen with proper recycling
            Debug.LogWarning("LevelGenerator: Object pool is empty! Consider increasing pool size.");
            return;
        }
        
        // Get segment from pool
        GameObject segment = segmentPool.Dequeue();
        
        // Position the segment
        segment.transform.position = new Vector3(furthestSpawnX, 0, 0);
        
        // Randomize the gap position
        float randomGapY = Random.Range(minGapY, maxGapY);
        FirewallSegment firewall = segment.GetComponent<FirewallSegment>();
        if (firewall != null)
        {
            firewall.SetGapPosition(randomGapY, gapSize);
        }
        
        // Activate and track the segment
        segment.SetActive(true);
        activeSegments.Add(segment);
        
        // Spawn enemy between this barrier and the next
        if (enemySpawner != null)
        {
            // Enemy spawns between current and next barrier (in the middle of the gap)
            float enemyX = furthestSpawnX + (segmentSpacing / 2f);
            enemySpawner.TrySpawnEnemy(enemyX, randomGapY, gapSize);
        }
        
        // Update furthest spawn position
        furthestSpawnX += segmentSpacing;
    }

    /// <summary>
    /// Checks for segments that have passed behind the player and recycles them.
    /// </summary>
    private void RecyclePassedSegments()
    {
        for (int i = activeSegments.Count - 1; i >= 0; i--)
        {
            GameObject segment = activeSegments[i];
            
            // Check if segment is behind the player beyond despawn distance
            if (segment.transform.position.x < player.position.x - despawnDistance)
            {
                RecycleSegment(segment, i);
            }
        }
    }

    /// <summary>
    /// Recycles a segment back into the pool.
    /// </summary>
    /// <param name="segment">The segment to recycle</param>
    /// <param name="activeIndex">Index in the active segments list</param>
    private void RecycleSegment(GameObject segment, int activeIndex)
    {
        // Stop particles before deactivating
        FirewallSegment firewall = segment.GetComponent<FirewallSegment>();
        if (firewall != null)
        {
            firewall.StopParticles();
        }
        
        // Notify GameManager that a barrier was passed
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnBarrierPassed();
        }
        
        // Deactivate and return to pool
        segment.SetActive(false);
        activeSegments.RemoveAt(activeIndex);
        segmentPool.Enqueue(segment);
    }

    /// <summary>
    /// Resets the level generator. Call this when restarting the game.
    /// </summary>
    public void ResetLevel()
    {
        // Return all active segments to pool
        foreach (GameObject segment in activeSegments)
        {
            FirewallSegment firewall = segment.GetComponent<FirewallSegment>();
            if (firewall != null)
            {
                firewall.StopParticles();
            }
            segment.SetActive(false);
            segmentPool.Enqueue(segment);
        }
        activeSegments.Clear();
        
        // Respawn initial segments
        SpawnInitialSegments();
    }
}
