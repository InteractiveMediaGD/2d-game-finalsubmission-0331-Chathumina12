using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Spawns enemies between firewall barriers using object pooling.
/// Supports multiple enemy types: Antivirus (basic) and Shooter (ranged).
/// </summary>
public class EnemySpawner : MonoBehaviour
{
    [Header("Prefab References")]
    [Tooltip("The Enemy_Antivirus prefab to spawn")]
    [SerializeField] private GameObject enemyPrefab;
    
    [Tooltip("The Enemy_Shooter prefab to spawn")]
    [SerializeField] private GameObject shooterPrefab;

    [Header("Pool Settings")]
    [Tooltip("Number of basic enemies to pre-instantiate")]
    [SerializeField] private int poolSize = 10;
    
    [Tooltip("Number of shooter enemies to pre-instantiate")]
    [SerializeField] private int shooterPoolSize = 5;

    [Header("Spawn Settings")]
    [Tooltip("Chance to spawn a basic enemy between barriers (0-1)")]
    [SerializeField] private float spawnChance = 0.5f;
    
    [Tooltip("Chance to spawn a shooter enemy between barriers (0-1)")]
    [SerializeField] private float shooterSpawnChance = 0.3f;
    
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

    // Object pools
    private Queue<GameObject> enemyPool;
    private Queue<GameObject> shooterPool;
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
        shooterPool = new Queue<GameObject>();
        activeEnemies = new List<GameObject>();
        
        // Initialize basic enemy pool
        for (int i = 0; i < poolSize; i++)
        {
            GameObject enemy = Instantiate(enemyPrefab, transform);
            enemy.SetActive(false);
            enemyPool.Enqueue(enemy);
        }
        
        // Initialize shooter enemy pool
        if (shooterPrefab != null)
        {
            for (int i = 0; i < shooterPoolSize; i++)
            {
                GameObject shooter = Instantiate(shooterPrefab, transform);
                shooter.SetActive(false);
                shooterPool.Enqueue(shooter);
            }
        }
    }

    /// <summary>
    /// Called by LevelGenerator when a new segment is spawned.
    /// Spawns enemies between barriers based on spawn chances.
    /// </summary>
    /// <param name="xPosition">X position to spawn the enemy at</param>
    /// <param name="gapCenterY">Y position of the firewall gap center</param>
    /// <param name="gapSize">Size of the gap (for positioning)</param>
    public void TrySpawnEnemy(float xPosition, float gapCenterY, float gapSize)
    {
        // Calculate Y position within the gap
        float halfGap = gapSize / 2f * 0.7f; // Stay within 70% of gap
        float randomY = gapCenterY + Random.Range(-halfGap, halfGap);
        randomY = Mathf.Clamp(randomY, minY, maxY);
        
        // Try to spawn basic enemy
        if (Random.value <= spawnChance && enemyPool.Count > 0)
        {
            GameObject enemy = enemyPool.Dequeue();
            enemy.transform.position = new Vector3(xPosition, randomY, 0);
            enemy.SetActive(true);
            activeEnemies.Add(enemy);
        }
        
        // Try to spawn shooter enemy (at a slightly different Y to avoid overlap)
        if (Random.value <= shooterSpawnChance && shooterPool.Count > 0)
        {
            GameObject shooter = shooterPool.Dequeue();
            float shooterY = gapCenterY + Random.Range(-halfGap, halfGap);
            shooterY = Mathf.Clamp(shooterY, minY, maxY);
            // Offset X slightly so shooters spawn further ahead
            shooter.transform.position = new Vector3(xPosition + 2f, shooterY, 0);
            shooter.SetActive(true);
            activeEnemies.Add(shooter);
        }
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
    /// Returns an enemy to the appropriate pool.
    /// </summary>
    private void RecycleEnemy(GameObject enemy, int activeIndex)
    {
        enemy.SetActive(false);
        activeEnemies.RemoveAt(activeIndex);
        
        // Return to the correct pool based on enemy type
        if (enemy.GetComponent<EnemyShooter>() != null)
        {
            shooterPool.Enqueue(enemy);
        }
        else
        {
            enemyPool.Enqueue(enemy);
        }
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
                
                // Return to the correct pool based on enemy type
                if (enemy.GetComponent<EnemyShooter>() != null)
                {
                    shooterPool.Enqueue(enemy);
                }
                else
                {
                    enemyPool.Enqueue(enemy);
                }
            }
        }
        activeEnemies.Clear();
    }
}
