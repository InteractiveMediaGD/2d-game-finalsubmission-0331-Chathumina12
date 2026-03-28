using UnityEngine;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// Standard ShopController that manages the grid layout of skins and 
/// hooks into the EconomyManager (CurrencyManager) to read balances seamlessly.
/// </summary>
public class ShopController : MonoBehaviour
{
    [Header("Shop Database")]
    [Tooltip("List of all purchasable skins to display in the grid.")]
    public VirusSkin[] availableSkins;

    [Header("Grid Layout References")]
    [Tooltip("The parent object containing the Unity GridLayoutGroup component.")]
    [SerializeField] private Transform gridContainer;
    
    [Tooltip("The UI Prefab template featuring the ShopItemUI script.")]
    [SerializeField] private GameObject shopItemPrefab;

    [Header("Economy UI")]
    [Tooltip("UI element displaying total Data Fragments from CurrencyManager.")]
    [SerializeField] private TextMeshProUGUI totalFragmentsText;

    // Track instantiated items so we can refresh them collectively
    private List<ShopItemUI> spawnedItems = new List<ShopItemUI>();

    private void Start()
    {
        InitializeShopGrid();
        UpdateCurrencyDisplay();
        
        // Listen to balance changes from the Currency Manager (EconomyManager) automatically
        if (EconomyManager.Instance != null)
        {
            EconomyManager.Instance.OnBalanceChanged += HandleBalanceChanged;
        }
    }

    private void OnDestroy()
    {
        if (EconomyManager.Instance != null)
        {
            EconomyManager.Instance.OnBalanceChanged -= HandleBalanceChanged;
        }
    }

    /// <summary>
    /// Instantiates grid items dynamically for every available skin.
    /// This makes adding new skins completely modular without editing UI manually.
    /// </summary>
    private void InitializeShopGrid()
    {
        // Clear existing children (if any)
        foreach (Transform child in gridContainer)
        {
            Destroy(child.gameObject);
        }
        spawnedItems.Clear();

        // Spawn a ShopItemUI card for every skin
        if (availableSkins != null && shopItemPrefab != null)
        {
            foreach (VirusSkin skin in availableSkins)
            {
                GameObject newCard = Instantiate(shopItemPrefab, gridContainer);
                ShopItemUI itemUI = newCard.GetComponent<ShopItemUI>();
                
                if (itemUI != null)
                {
                    itemUI.Initialize(skin, this);
                    spawnedItems.Add(itemUI);
                }
                else
                {
                    Debug.LogWarning("[ShopController] Missing ShopItemUI script on your shopItemPrefab!");
                }
            }
        }
    }

    /// <summary>
    /// Refreshes the visual state of all cards in the grid.
    /// Usually called when a purchase is made or an item is equipped.
    /// </summary>
    public void RefreshAllItems()
    {
        foreach (ShopItemUI item in spawnedItems)
        {
            item.RefreshState();
        }
        UpdateCurrencyDisplay();
    }

    /// <summary>
    /// Hook triggered via C# event whenever Data Fragments are collected or spent.
    /// </summary>
    private void HandleBalanceChanged(int newBalance)
    {
        UpdateCurrencyDisplay();
        
        // Refresh items because affordability (BUY vs LOCKED) might have changed
        foreach (ShopItemUI item in spawnedItems)
        {
            item.RefreshState();
        }
    }

    private void UpdateCurrencyDisplay()
    {
        if (totalFragmentsText != null && EconomyManager.Instance != null)
        {
            totalFragmentsText.text = $"Fragments: {EconomyManager.Instance.GetBalance()}";
        }
    }
}
