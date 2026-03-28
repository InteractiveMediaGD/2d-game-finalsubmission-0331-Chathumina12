using UnityEngine;

/// <summary>
/// Monitors game progress and triggers the Boss encounter.
/// Spawns the boss using Camera.main.ViewportToWorldPoint for pixel-perfect positioning.
/// </summary>
public class BossManager : MonoBehaviour
{
    [Tooltip("Number of firewalls to pass before triggering the System Admin boss")]
    public int firewallsToTrigger = 15;
    
    [Tooltip("The SystemAdminBoss prefab to spawn")]
    public GameObject bossPrefab;

    private int currentFirewallCount = 0;
    private bool hasSpawnedBoss = false;
    
    [Header("Testing")]
    [Tooltip("If true, spawns the boss immediately on the first firewall for debugging")]
    public bool testBossMode = false;

    private void Start()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnBarrierPassedEvent += HandleBarrierPassed;
        }
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnBarrierPassedEvent -= HandleBarrierPassed;
        }
    }

    private void HandleBarrierPassed()
    {
        if (hasSpawnedBoss) return;
        
        currentFirewallCount++;
        
        int threshold = testBossMode ? 1 : firewallsToTrigger;
        
        if (currentFirewallCount >= threshold)
        {
            SpawnBoss();
        }
    }

    private void SpawnBoss()
    {
        hasSpawnedBoss = true;
        
        // Lock the arena
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetBossFightState(true);
        }

        if (bossPrefab != null && Camera.main != null)
        {
            // Viewport (0.5, 1.2) = top-centre, slightly above the visible screen
            // The boss's own DescendIntoView coroutine will lerp it down to (0.5, 0.8)
            Vector3 spawnWorld = Camera.main.ViewportToWorldPoint(new Vector3(0.5f, 1.3f, 0f));
            spawnWorld.z = 0f;  // Explicitly Z=0 so the 2D camera renders it
            
            GameObject bossInstance = Instantiate(bossPrefab, spawnWorld, Quaternion.identity);
            
            if (ScreenShakeController.Instance != null)
                ScreenShakeController.Instance.ShakeOnDamage();
                
            Debug.Log($"[BossManager] Boss spawned at {spawnWorld} (viewport 0.5, 1.3 → will descend to 0.8)");
        }
        else
        {
            Debug.LogError("[BossManager] Boss Prefab or Camera is missing!");
            if (GameManager.Instance != null) GameManager.Instance.SetBossFightState(false);
        }
    }
}
