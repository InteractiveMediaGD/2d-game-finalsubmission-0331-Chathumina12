using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Spawns enemies between firewall barriers using object pooling.
/// </summary>
public class EnemySpawner : MonoBehaviour
{
    [Header("Prefab Reference")]
    [Tooltip("The Enemy_Antivirus prefab to spawn")]
    [SerializeField] private GameObject enemyPrefab;

    [Header("Pool Settings")]
    [Tooltip("Number of enemies to pre-instantiate")]
    [SerializeField] private int poolSize = 10;

    [Header("Spawn Settings")]
    [Tooltip("Chance to spawn an enemy between barriers (0-1)")]
    [SerializeField] private float spawnChance = 0.5f;
    
    [Tooltip("Minimum Y position for enemy spawn")]
    [SerializeField] private float minY = -3f;
    
    [Tooltip("Maximum Y position for enemy spawn")]
    [SerializeField] private float maxY = 3f;

    [Header("Player Reference")]
    [Tooltip("Reference to the player transform")]
    [SerializeField] private Transform player;

    [Header("Despawn Settings")]
    [Tooltip("Distance behind player to recycle enemies")]
    [SerializeField] private float despawnDistance = 5f;

    // Object pool
    private Queue<GameObject> enemyPool;
    private List<GameObject> activeEnemies;

    private void Start()
    {
        InitializePool();
    }

    private void Update()
    {
        RecyclePassedEnemies();
    }

    /// <summary>
    /// Pre-instantiates all enemy objects and adds them to the pool.
    /// </summary>
    private void InitializePool()
    {
        enemyPool = new Queue<GameObject>();
        activeEnemies = new List<GameObject>();
        
        for (int i = 0; i < poolSize; i++)
        {
            GameObject enemy = Instantiate(enemyPrefab, transform);
            enemy.SetActive(false);
            enemyPool.Enqueue(enemy);
        }
    }

    /// <summary>
    /// Called by LevelGenerator when a new segment is spawned.
    /// Spawns an enemy between barriers based on spawn chance.
    /// </summary>
    /// <param name="xPosition">X position to spawn the enemy at</param>
    /// <param name="gapCenterY">Y position of the firewall gap center</param>
    /// <param name="gapSize">Size of the gap (for positioning)</param>
    public void TrySpawnEnemy(float xPosition, float gapCenterY, float gapSize)
    {
        // Random chance to spawn
        if (Random.value > spawnChance) return;
        
        if (enemyPool.Count == 0)
        {
            Debug.LogWarning("EnemySpawner: Pool exhausted!");
            return;
        }
        
        // Get enemy from pool
        GameObject enemy = enemyPool.Dequeue();
        
        // Position within the gap (randomize Y within safe bounds)
        float halfGap = gapSize / 2f * 0.7f; // Stay within 70% of gap
        float randomY = gapCenterY + Random.Range(-halfGap, halfGap);
        randomY = Mathf.Clamp(randomY, minY, maxY);
        
        enemy.transform.position = new Vector3(xPosition, randomY, 0);
        
        // Activate and track
        enemy.SetActive(true);
        activeEnemies.Add(enemy);
    }

    /// <summary>
    /// Recycles enemies that have passed behind the player.
    /// </summary>
    private void RecyclePassedEnemies()
    {
        if (player == null) return;
        
        for (int i = activeEnemies.Count - 1; i >= 0; i--)
        {
            GameObject enemy = activeEnemies[i];
            
            // Skip if enemy was destroyed (by projectile)
            if (enemy == null)
            {
                activeEnemies.RemoveAt(i);
                continue;
            }
            
            // Check if behind player
            if (enemy.transform.position.x < player.position.x - despawnDistance)
            {
                RecycleEnemy(enemy, i);
            }
        }
    }

    /// <summary>
    /// Returns an enemy to the pool.
    /// </summary>
    private void RecycleEnemy(GameObject enemy, int activeIndex)
    {
        enemy.SetActive(false);
        activeEnemies.RemoveAt(activeIndex);
        enemyPool.Enqueue(enemy);
    }

    /// <summary>
    /// Resets all enemies (for game restart).
    /// </summary>
    public void ResetEnemies()
    {
        foreach (GameObject enemy in activeEnemies)
        {
            if (enemy != null)
            {
                enemy.SetActive(false);
                enemyPool.Enqueue(enemy);
            }
        }
        activeEnemies.Clear();
    }
}
