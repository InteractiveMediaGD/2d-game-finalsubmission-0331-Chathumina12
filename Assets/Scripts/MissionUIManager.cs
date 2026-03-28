using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Oversees the UI Overlay for Missions. Manages the 3 active MissionUI cards on screen.
/// </summary>
public class MissionUIManager : MonoBehaviour
{
    [Tooltip("The 3 instanced MissionUI card components in the UI hierarchy.")]
    public List<MissionUI> missionCards;

    private void Start()
    {
        // Delay by 0.1s to ensure MissionManager has finished generating its random missions for the run
        Invoke("InitializeUI", 0.1f);
    }

    private void InitializeUI()
    {
        if (MissionManager.Instance == null)
        {
            Debug.LogWarning("[MissionUIManager] Cannot initialize, MissionManager instance missing!");
            return;
        }

        // Subscribe to any progress made
        MissionManager.Instance.OnMissionUpdated += HandleMissionUpdated;

        // Turn on the cards we need and set them up
        for (int i = 0; i < MissionManager.Instance.activeMissions.Count && i < missionCards.Count; i++)
        {
            missionCards[i].gameObject.SetActive(true);
            missionCards[i].Setup(i, MissionManager.Instance.activeMissions[i]);
        }
    }

    private void OnDestroy()
    {
        // Prevent memory leaks
        if (MissionManager.Instance != null)
        {
            MissionManager.Instance.OnMissionUpdated -= HandleMissionUpdated;
        }
    }

    private void HandleMissionUpdated(int index, float currentProgress, float targetAmount, bool isCompleted)
    {
        if (index >= 0 && index < missionCards.Count)
        {
            missionCards[index].UpdateProgress(currentProgress, targetAmount, isCompleted);
        }
    }
}
