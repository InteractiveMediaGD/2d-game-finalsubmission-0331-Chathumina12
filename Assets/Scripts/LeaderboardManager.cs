using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

[Serializable]
public class ScoreEntry
{
    public string playerName;
    public int score;
    public string equippedSkin;

    public ScoreEntry(string name, int s, string skin)
    {
        playerName = name;
        score = s;
        equippedSkin = skin;
    }
}

[Serializable]
public class LeaderboardData
{
    public List<ScoreEntry> entries = new List<ScoreEntry>();
}

/// <summary>
/// Handles submitting and fetching scores directly from Firebase Realtime Database 
/// using UnityWebRequest REST API (No SDK required).
/// </summary>
public class LeaderboardManager : MonoBehaviour
{
    public static LeaderboardManager Instance { get; private set; }

    [Header("Firebase Configuration")]
    [Tooltip("Paste your Firebase Realtime Database URL here (must end in .firebaseio.com, NO trailing slash)")]
    public string firebaseDatabaseURL = "https://your-project-id-default-rtdb.firebaseio.com";

    private const int MAX_ENTRIES = 10;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// Starts the asynchronous submission Coroutine to Firebase.
    /// </summary>
    public void SubmitScore(int score, string playerName, string equippedSkin)
    {
        StartCoroutine(SubmitScoreRoutine(score, playerName, equippedSkin));
    }

    /// <summary>
    /// Uses a Read-Modify-Write pattern to fetch the JSON array, insert the score, 
    /// truncate the top 10, and PUT it back to the database.
    /// </summary>
    private IEnumerator SubmitScoreRoutine(int score, string playerName, string equippedSkin)
    {
        if (string.IsNullOrEmpty(firebaseDatabaseURL) || !firebaseDatabaseURL.StartsWith("http"))
        {
            Debug.LogError("[LeaderboardManager] Please configure your Firebase URL in the Inspector!");
            yield break;
        }

        string requestUrl = $"{firebaseDatabaseURL}/leaderboard.json";

        // 1. GET current leaderboard
        using (UnityWebRequest getReq = UnityWebRequest.Get(requestUrl))
        {
            yield return getReq.SendWebRequest();

            LeaderboardData data = new LeaderboardData();

            if (getReq.result == UnityWebRequest.Result.Success && !string.IsNullOrEmpty(getReq.downloadHandler.text) && getReq.downloadHandler.text != "null")
            {
                try
                {
                    data = JsonUtility.FromJson<LeaderboardData>(getReq.downloadHandler.text);
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"[LeaderboardManager] Failed to parse existing leaderboard DB: {e.Message}");
                }
            }

            if (data == null || data.entries == null) data = new LeaderboardData();

            // 2. Modify & Sort
            data.entries.Add(new ScoreEntry(playerName, score, equippedSkin));
            data.entries = data.entries.OrderByDescending(x => x.score).Take(MAX_ENTRIES).ToList();

            // 3. PUT back
            string json = JsonUtility.ToJson(data);
            using (UnityWebRequest putReq = UnityWebRequest.Put(requestUrl, json))
            {
                putReq.SetRequestHeader("Content-Type", "application/json");
                yield return putReq.SendWebRequest();

                if (putReq.result == UnityWebRequest.Result.Success)
                {
                    Debug.Log($"[LeaderboardManager] Successfully submitted score {score} for {playerName}.");
                }
                else
                {
                    Debug.LogError($"[LeaderboardManager] Failed to submit score: {putReq.error}");
                }
            }
        }
    }

    /// <summary>
    /// Fetches the top 10 scores from Firebase.
    /// </summary>
    public void FetchTopScores(Action<List<ScoreEntry>> onSuccess, Action<string> onFailure = null)
    {
        StartCoroutine(FetchTopScoresRoutine(onSuccess, onFailure));
    }

    private IEnumerator FetchTopScoresRoutine(Action<List<ScoreEntry>> onSuccess, Action<string> onFailure)
    {
        if (string.IsNullOrEmpty(firebaseDatabaseURL) || !firebaseDatabaseURL.StartsWith("http"))
        {
            onFailure?.Invoke("Firebase URL not configured.");
            yield break;
        }

        string requestUrl = $"{firebaseDatabaseURL}/leaderboard.json";

        using (UnityWebRequest getReq = UnityWebRequest.Get(requestUrl))
        {
            yield return getReq.SendWebRequest();

            if (getReq.result != UnityWebRequest.Result.Success)
            {
                onFailure?.Invoke($"Network Error: {getReq.error}");
                yield break;
            }

            string json = getReq.downloadHandler.text;
            if (string.IsNullOrEmpty(json) || json == "null")
            {
                onSuccess?.Invoke(new List<ScoreEntry>()); // Give empty list if network success but no items
                yield break;
            }

            try
            {
                LeaderboardData data = JsonUtility.FromJson<LeaderboardData>(json);
                if (data != null && data.entries != null)
                {
                    // Ensure sorted
                    data.entries = data.entries.OrderByDescending(x => x.score).ToList();
                    onSuccess?.Invoke(data.entries);
                }
                else
                {
                    onSuccess?.Invoke(new List<ScoreEntry>());
                }
            }
            catch (Exception e)
            {
                onFailure?.Invoke($"Parse Error: {e.Message}");
            }
        }
    }

    [ContextMenu("Run: Generate Fake Scores to Firebase")]
    private void DebugGenerateFakeScores()
    {
        SubmitScore(150, "Virus_Zero", "Default");
        SubmitScore(85, "HackerBoy99", "Ninja");
        SubmitScore(220, "SystemAdmin", "Neon Hacker");
    }
}
