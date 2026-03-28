using UnityEngine;
using System.Collections.Generic;

public enum MissionType
{
    SurviveTime,
    GrazeWalls,
    DefeatEnemies,
    PassBarriers,
    CollectFragments
}

[System.Serializable]
public class MissionData
{
    public MissionType type;
    public string description;
    public float targetAmount;
    public int rewardFragments;
    
    // Runtime Tracking
    [HideInInspector] public float currentAmount = 0;
    [HideInInspector] public bool isCompleted = false;

    // Clone method so we don't accidentally modify the master list
    public MissionData Clone()
    {
        return new MissionData
        {
            type = this.type,
            description = this.description,
            targetAmount = this.targetAmount,
            rewardFragments = this.rewardFragments,
            currentAmount = 0,
            isCompleted = false
        };
    }
}

/// <summary>
/// Singleton manager that randomly assigns 3 missions per run and listens for stat events.
/// </summary>
public class MissionManager : MonoBehaviour
{
    public static MissionManager Instance { get; private set; }

    [Header("Mission Pool")]
    [Tooltip("The master list of all possible missions that can be randomly selected.")]
    public List<MissionData> possibleMissions = new List<MissionData>();

    [Header("Active Missions")]
    [Tooltip("The 3 missions currently active in this run.")]
    public List<MissionData> activeMissions = new List<MissionData>();

    // Fired when a mission makes progress or completes (MissionIndex, CurrentProgress, TargetAmount, IsCompleted)
    public System.Action<int, float, float, bool> OnMissionUpdated;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        GenerateMissionsForRun();

        // Subscribe to all generic base code events!
        EnemyAntivirus.OnEnemyDestroyed += HandleEnemyDestroyed;
        CloseCallDetector.OnCloseCallTriggered += HandleCloseCall;
        
        if (GameManager.Instance != null)
            GameManager.Instance.OnBarrierPassedEvent += HandleBarrierPassed;
        
        if (EconomyManager.Instance != null)
            EconomyManager.Instance.OnFragmentsAdded += HandleFragmentsCollected;
    }

    private void OnDestroy()
    {
        // Unsubscribe to prevent memory leaks!
        EnemyAntivirus.OnEnemyDestroyed -= HandleEnemyDestroyed;
        CloseCallDetector.OnCloseCallTriggered -= HandleCloseCall;
        
        if (GameManager.Instance != null)
            GameManager.Instance.OnBarrierPassedEvent -= HandleBarrierPassed;
            
        if (EconomyManager.Instance != null)
            EconomyManager.Instance.OnFragmentsAdded -= HandleFragmentsCollected;
    }

    private void Update()
    {
        // Continuously update Survival Time missions
        if (GameManager.Instance != null && !GameManager.Instance.IsGameOver)
        {
            AddProgress(MissionType.SurviveTime, Time.deltaTime);
        }
    }

    /// <summary>
    /// Randomly selects 3 unique missions from the master list.
    /// </summary>
    private void GenerateMissionsForRun()
    {
        activeMissions.Clear();

        if (possibleMissions.Count == 0) return;

        // Create a temporary copy to pick from without picking duplicates
        List<MissionData> pool = new List<MissionData>(possibleMissions);
        
        int missionsToPick = Mathf.Min(3, pool.Count);

        for (int i = 0; i < missionsToPick; i++)
        {
            int randomIndex = Random.Range(0, pool.Count);
            MissionData picked = pool[randomIndex].Clone();
            activeMissions.Add(picked);
            pool.RemoveAt(randomIndex);
        }
    }

    // --- EVENT HANDLERS ---
    private void HandleEnemyDestroyed() => AddProgress(MissionType.DefeatEnemies, 1f);
    private void HandleCloseCall() => AddProgress(MissionType.GrazeWalls, 1f);
    private void HandleBarrierPassed() => AddProgress(MissionType.PassBarriers, 1f);
    private void HandleFragmentsCollected(int amount) => AddProgress(MissionType.CollectFragments, amount);

    /// <summary>
    /// Adds progress to all active missions of a specific type.
    /// </summary>
    private void AddProgress(MissionType type, float amount)
    {
        for (int i = 0; i < activeMissions.Count; i++)
        {
            MissionData mission = activeMissions[i];
            
            if (mission.type == type && !mission.isCompleted)
            {
                mission.currentAmount += amount;

                if (mission.currentAmount >= mission.targetAmount)
                {
                    mission.currentAmount = mission.targetAmount;
                    mission.isCompleted = true;
                    GiveReward(mission);
                }

                // Notify UI
                OnMissionUpdated?.Invoke(i, mission.currentAmount, mission.targetAmount, mission.isCompleted);
            }
        }
    }

    private void GiveReward(MissionData mission)
    {
        Debug.Log($"[MissionManager] Mission Completed: '{mission.description}' Awarded {mission.rewardFragments} Fragments!");
        
        if (EconomyManager.Instance != null)
        {
            // Note: This inherently calls OnFragmentsAdded, but since current mission is marked 'isCompleted = true',
            // it won't infinitely loop if we also happen to have a "Collect Fragments" mission!
            EconomyManager.Instance.AddFragments(mission.rewardFragments);
        }
    }
}
