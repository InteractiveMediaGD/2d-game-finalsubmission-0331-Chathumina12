using UnityEngine;
using System;

/// <summary>
/// Singleton manager for handling the persistent "Data Fragments" virtual currency.
/// Uses PlayerPrefs for local saving.
/// </summary>
public class EconomyManager : MonoBehaviour
{
    public static EconomyManager Instance { get; private set; }

    private const string DATA_FRAGMENTS_KEY = "DataFragments";

    [Header("Economy State")]
    [Tooltip("Current balance of Data Fragments")]
    [SerializeField] private int currentBalance = 0;

    // Event fired when the balance changes, useful for updating UI
    public Action<int> OnBalanceChanged;
    public Action<int> OnFragmentsAdded;

    private void Awake()
    {
        // Singleton pattern implementation
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
        // Keep the manager alive across scenes
        DontDestroyOnLoad(gameObject);
    }

    private string GetSaveKey()
    {
        return "DataFragments";
    }

    private void Start()
    {
        LoadBalance();
    }

    /// <summary>
    /// Gets the current Data Fragment balance.
    /// </summary>
    public int GetBalance()
    {
        return currentBalance;
    }

    /// <summary>
    /// Adds Data Fragments to the player's balance.
    /// </summary>
    /// <param name="amount">The amount to add</param>
    public void AddFragments(int amount)
    {
        if (amount < 0) return; // Prevent negative additions

        currentBalance += amount;
        SaveBalance();
        
        // Notify any listeners (like the UI) that the balance changed
        OnBalanceChanged?.Invoke(currentBalance);
        OnFragmentsAdded?.Invoke(amount);
        Debug.Log($"[Economy] Added {amount} Data Fragments. New Balance: {currentBalance}");
    }

    /// <summary>
    /// Attempts to spend Data Fragments.
    /// </summary>
    /// <param name="amount">The amount to spend</param>
    /// <returns>True if the transaction was successful, false if insufficient funds.</returns>
    public bool SpendFragments(int amount)
    {
        if (amount < 0) return false;

        if (currentBalance >= amount)
        {
            currentBalance -= amount;
            SaveBalance();
            
            OnBalanceChanged?.Invoke(currentBalance);
            Debug.Log($"[Economy] Spent {amount} Data Fragments. Remaining Balance: {currentBalance}");
            return true;
        }

        Debug.LogWarning($"[Economy] Insufficient fragments! Attempted to spend {amount}, but only have {currentBalance}.");
        return false;
    }

    /// <summary>
    /// Saves the current balance to PlayerPrefs.
    /// </summary>
    private void SaveBalance()
    {
        PlayerPrefs.SetInt(GetSaveKey(), currentBalance);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// Loads the balance from PlayerPrefs.
    /// </summary>
    private void LoadBalance()
    {
        currentBalance = PlayerPrefs.GetInt(GetSaveKey(), 0);
        OnBalanceChanged?.Invoke(currentBalance);
    }
    
    // Editor quick-test functions
    [ContextMenu("Run: Add +100 Fragments")]
    private void DebugAdd100() => AddFragments(100);
    
    [ContextMenu("Run: Clear Fragments")]
    private void DebugClear() 
    {
        currentBalance = 0;
        SaveBalance();
        OnBalanceChanged?.Invoke(currentBalance);
        Debug.Log("[Economy] Fragments cleared.");
    }
}
