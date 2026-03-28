using UnityEngine;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// Handles populating the Leaderboard Canvas.
/// Fetches data from the LeaderboardManager to display the top scores.
/// </summary>
public class LeaderboardUI : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("The parent container (e.g., Scroll View Content) for the score entries")]
    [SerializeField] private Transform entryContainer;

    [Tooltip("The TextMeshProUGUI prefab to instantiate for each entry")]
    [SerializeField] private TextMeshProUGUI entryPrefab;

    [Tooltip("Loading text to show while fetching from 'server' (even mock servers)")]
    [SerializeField] private TextMeshProUGUI statusText;

    private void OnEnable()
    {
        // Fetch and populate every time the leaderboard UI is opened
        RefreshLeaderboard();
    }

    /// <summary>
    /// Clears the current list and requests new data from the manager.
    /// </summary>
    public void RefreshLeaderboard()
    {
        if (LeaderboardManager.Instance == null)
        {
            SetStatus("Leaderboard Manager not found.");
            return;
        }

        // Clear existing entries (but carefully skip our hidden template prefab)
        foreach (Transform child in entryContainer)
        {
            if (entryPrefab != null && child.gameObject == entryPrefab.gameObject)
                continue;
            
            Destroy(child.gameObject);
        }

        SetStatus("Loading scores...");

        // Fetch scores (Action callback simulates web request)
        LeaderboardManager.Instance.FetchTopScores(OnScoresFetched, OnFetchFailed);
    }

    private void OnScoresFetched(List<ScoreEntry> scores)
    {
        if (scores == null || scores.Count == 0)
        {
            SetStatus("No scores submitted yet.");
            return;
        }

        SetStatus(""); // Clear the status message

        // Populate the list
        int rank = 1;
        foreach (var entry in scores)
        {
            TextMeshProUGUI textInstance = Instantiate(entryPrefab, entryContainer);
            textInstance.gameObject.SetActive(true); // Turn it on!
            
            string skinTag = string.IsNullOrEmpty(entry.equippedSkin) ? "Unknown" : entry.equippedSkin;
            textInstance.text = $"{rank}. {entry.playerName} <color=yellow>[{skinTag}]</color> - <color=green>{entry.score}</color>";
            rank++;
        }
    }

    private void OnFetchFailed(string errorMessage)
    {
        SetStatus($"Error: {errorMessage}");
        Debug.LogError($"[LeaderboardUI] {errorMessage}");
    }

    private void SetStatus(string message)
    {
        if (statusText != null)
        {
            statusText.text = message;
            statusText.gameObject.SetActive(!string.IsNullOrEmpty(message));
        }
    }
}
