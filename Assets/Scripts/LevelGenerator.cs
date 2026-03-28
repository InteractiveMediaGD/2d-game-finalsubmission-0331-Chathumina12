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

    [Header("Collectibles & Nodes")]
    [Tooltip("The Data Fragment prefab to spawn in gaps")]
    [SerializeField] private GameObject dataFragmentPrefab;
    
    [Tooltip("Probability (0.0 to 1.0) of spawning a data fragment in a gap")]
    [SerializeField] [Range(0f, 1f)] private float fragmentSpawnChance = 0.5f;

    [Tooltip("The Encrypted Node Prefab to spawn in risky areas")]
    [SerializeField] private GameObject encryptedNodePrefab;

    [Tooltip("Probability of spawning an Encrypted Node halfway between firewalls")]
    [SerializeField] [Range(0f, 1f)] private float nodeSpawnChance = 0.35f;

    [Header("Power-Up Spawning")]
    [Tooltip("Reference to the PickupSpawner — if assigned, uses it to handle power-up selection logic")]
    [SerializeField] private PickupSpawner pickupSpawner;

    [Tooltip("HealthPack prefab (used when PickupSpawner is not assigned)")]
    [SerializeField] private GameObject healthPackPrefab;

    [Tooltip("RapidFire prefab (used when PickupSpawner is not assigned)")]
    [SerializeField] private GameObject rapidFirePrefab;

    [Tooltip("Chance (0-1) to spawn a power-up inside a firewall gap. 0.2 = 20% per segment.")]
    [SerializeField] [Range(0f, 1f)] private float powerUpSpawnChance = 0.20f;

    // Object pool
    private Queue<GameObject> segmentPool;
    
    // Track the X position of the furthest spawned segment
    private float furthestSpawnX;
    
    // List of active segments for recycling checks
    private List<GameObject> activeSegments;

    private void Start()
    {
        // ── Apply Difficulty Modifiers ─────────────────────────────────────
        int difficulty = PlayerPrefs.GetInt("DifficultyLevel", 1);
        if (difficulty == 0) // Easy
        {
            gapSize += 1.5f;
            Debug.Log($"[LevelGenerator] Easy Mode: Gap size increased to {gapSize}");
        }
        else if (difficulty == 2) // Hard
        {
            gapSize = Mathf.Max(1.5f, gapSize - 1.0f);
            Debug.Log($"[LevelGenerator] Hard Mode: Gap size decreased to {gapSize}");
        }
        // ───────────────────────────────────────────────────────────────────

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
        
        // Position the segment — Z is explicitly 0 so sprites are visible in 2D camera
        segment.transform.position = new Vector3(furthestSpawnX, 0f, 0f);
        
        // Randomize the gap position
        float randomGapY = Random.Range(minGapY, maxGapY);
        FirewallSegment firewall = segment.GetComponent<FirewallSegment>();
        if (firewall != null)
        {
            firewall.SetGapPosition(randomGapY, gapSize);
            // Guarantee all SpriteRenderers are enabled after pool reuse
            firewall.EnsureRenderersEnabled();
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

        // Spawn Data Fragment exactly inside the firewall gap
        if (dataFragmentPrefab != null && Random.value <= fragmentSpawnChance)
        {
            Instantiate(dataFragmentPrefab, new Vector3(furthestSpawnX, randomGapY, 0f), Quaternion.identity);
        }
        
        // Spawn Encrypted Node (offset vertically so it's a risky detour between firewalls)
        if (encryptedNodePrefab != null && Random.value <= nodeSpawnChance)
        {
            float nodeX = furthestSpawnX + (segmentSpacing / 2f);
            float nodeY = randomGapY + Random.Range(1.5f, 2.5f) * (Random.value > 0.5f ? 1 : -1);
            nodeY = Mathf.Clamp(nodeY, minGapY, maxGapY);
            Instantiate(encryptedNodePrefab, new Vector3(nodeX, nodeY, 0f), Quaternion.identity);
        }

        // ── Power-Up Spawning (20% chance per segment) ────────────────────────
        // Spawn a health or rapid-fire pickup inside the gap so it is guaranteed
        // to be reachable. Z is forced to 0 so it renders in front of the background.
        if (Random.value <= powerUpSpawnChance)
        {
            float pickupX = furthestSpawnX;
            float pickupY = randomGapY;

            if (pickupSpawner != null)
            {
                // Delegate to PickupSpawner so its weight & tracking logic applies
                pickupSpawner.SpawnPickupAt(pickupX, pickupY);
            }
            else
            {
                // Fallback: choose randomly between health and rapidfire prefabs
                GameObject[] candidates = new GameObject[2];
                int count = 0;
                if (healthPackPrefab  != null) candidates[count++] = healthPackPrefab;
                if (rapidFirePrefab   != null) candidates[count++] = rapidFirePrefab;

                if (count > 0)
                {
                    GameObject chosen = candidates[Random.Range(0, count)];
                    // Force Z = 0 so the sprite is visible to the 2D camera
                    Instantiate(chosen, new Vector3(pickupX, pickupY, 0f), Quaternion.identity);
                }
            }
        }
        // ─────────────────────────────────────────────────────────────────────

        // Update furthest spawn position
        furthestSpawnX += segmentSpacing;
    }

    /// <summary>
    /// Checks for segments that have passed behind the player and recycles them.
    /// </summary>
    /// <summary>
    /// Checks for segments that have passed behind the player and recycles them.
    /// </summary>
    private void RecyclePassedSegments()
    {
        for (int i = activeSegments.Count - 1; i >= 0; i--)
        {
            GameObject segment = activeSegments[i];
            
            // If segment was destroyed externally (e.g. boss arena clear), remove from list
            if (segment == null)
            {
                activeSegments.RemoveAt(i);
                continue;
            }
            
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
            // Skip if destroyed externally
            if (segment == null) continue;
            
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

    /// <summary>
    /// Iterates every GameObject in the object pool and explicitly calls
    /// SetActive(false) on it, flushing any state left over from the boss fight.
    /// Call this BEFORE RestartAfterBoss() so every segment starts completely clean.
    /// </summary>
    public void ResetPool()
    {
        // Also pull back any remaining active segments into the pool first
        for (int i = activeSegments.Count - 1; i >= 0; i--)
        {
            GameObject seg = activeSegments[i];
            if (seg == null) { activeSegments.RemoveAt(i); continue; }

            FirewallSegment fw = seg.GetComponent<FirewallSegment>();
            if (fw != null) fw.StopParticles();

            seg.SetActive(false);
            segmentPool.Enqueue(seg);
            activeSegments.RemoveAt(i);
        }

        // Now deactivate every object sitting in the queue
        // (temporarily drain and re-fill to touch all entries)
        List<GameObject> temp = new List<GameObject>(segmentPool);
        segmentPool.Clear();
        foreach (GameObject seg in temp)
        {
            if (seg != null)
            {
                seg.SetActive(false);
                segmentPool.Enqueue(seg);
            }
        }

        Debug.Log($"[LevelGenerator] ResetPool complete. Pool size: {segmentPool.Count}");
    }

    /// <summary>
    /// Restarts the level generator after a boss fight ends.
    /// Unlike ResetLevel(), this recalculates furthestSpawnX from the PLAYER'S CURRENT
    /// position so firewalls appear immediately ahead of wherever the player is.
    /// Call this from GameManager.EndBossFight() after the transition coroutine completes.
    /// </summary>
    public void RestartAfterBoss()
    {
        if (player == null)
        {
            Debug.LogWarning("[LevelGenerator] RestartAfterBoss: player reference missing, falling back to ResetLevel.");
            ResetLevel();
            return;
        }

        // 1. Return every active segment to the pool cleanly
        for (int i = activeSegments.Count - 1; i >= 0; i--)
        {
            GameObject seg = activeSegments[i];
            if (seg == null) { activeSegments.RemoveAt(i); continue; }

            FirewallSegment fw = seg.GetComponent<FirewallSegment>();
            if (fw != null) fw.StopParticles();

            seg.SetActive(false);
            segmentPool.Enqueue(seg);
        }
        activeSegments.Clear();

        // 2. Reset furthestSpawnX relative to the player's current world X
        //    so the first batch of firewalls spawns immediately ahead of the player.
        furthestSpawnX = player.position.x + segmentSpacing;

        // 3. Fill the look-ahead area with fresh segments
        int fillCount = Mathf.Min(poolSize, Mathf.CeilToInt(spawnDistance / segmentSpacing));
        for (int i = 0; i < fillCount; i++)
        {
            SpawnSegment();
        }

        Debug.Log($"[LevelGenerator] RestartAfterBoss complete. furthestSpawnX reset to {furthestSpawnX:F1} (player at {player.position.x:F1}).");
    }
}
