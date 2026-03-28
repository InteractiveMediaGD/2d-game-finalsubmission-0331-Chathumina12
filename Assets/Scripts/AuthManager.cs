using UnityEngine;
using System;

/// <summary>
/// Singleton manager that handles local player authentication (Username).
/// It sandboxes player saves (scores, economy, skins) by prefixing PlayerPrefs keys.
/// </summary>
public class AuthManager : MonoBehaviour
{
    public static AuthManager Instance { get; private set; }

    [Tooltip("The currently logged-in player's username.")]
    public string CurrentPlayerName { get; private set; } = "";

    /// <summary>
    /// Fired when a user successfully logs in, so other managers can reload their specific data.
    /// </summary>
    public Action<string> OnUserLoggedIn;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        
        Debug.Log("[AuthManager] Initialized. Waiting for user login...");
    }

    /// <summary>
    /// Logs in the player with the given username.
    /// </summary>
    public void Login(string username)
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            Debug.LogWarning("[AuthManager] Login failed: Username cannot be strictly whitespace.");
            return;
        }

        // Clean the username (remove leading/trailing spaces)
        CurrentPlayerName = username.Trim();
        
        // Save the last logged in user so they don't have to type it next time (Optional)
        PlayerPrefs.SetString("LastLoggedInUser", CurrentPlayerName);
        PlayerPrefs.Save();

        Debug.Log($"[AuthManager] Login successful: Welcome {CurrentPlayerName}!");

        // Broadcast to EconomyManager, SkinManager, etc., that a new profile is active
        OnUserLoggedIn?.Invoke(CurrentPlayerName);
    }
    
    /// <summary>
    /// Try auto-login if a previous user exists.
    /// </summary>
    public bool TryAutoLogin()
    {
        string lastUser = PlayerPrefs.GetString("LastLoggedInUser", "");
        if (!string.IsNullOrEmpty(lastUser))
        {
            Login(lastUser);
            return true;
        }
        return false;
    }

    /// <summary>
    /// Logs the current player out.
    /// </summary>
    public void Logout()
    {
        Debug.Log($"[AuthManager] {CurrentPlayerName} logged out.");
        CurrentPlayerName = "";
        PlayerPrefs.DeleteKey("LastLoggedInUser");
        PlayerPrefs.Save();
        
        // Optional: Broadcast logout
    }

    /// <summary>
    /// Helper to get a sandboxed PlayerPrefs key for the active user.
    /// E.g., GetPrefKey("DataFragments") returns "Hacker99_DataFragments".
    /// </summary>
    public string GetPrefKey(string baseKey)
    {
        if (string.IsNullOrEmpty(CurrentPlayerName))
        {
            Debug.LogWarning($"[AuthManager] Warning: Accessing key '{baseKey}' without an active login. Defaulting to 'Guest_{baseKey}'.");
            return $"Guest_{baseKey}";
        }
        return $"{CurrentPlayerName}_{baseKey}";
    }
}
