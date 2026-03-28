using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;

/// <summary>
/// Handles individual item display and interaction in the Shop Grid Layout.
/// Highly modular with UnityEvents exposed for easy integration with tweening engines (e.g. DOTween/LeanTween).
/// </summary>
public class ShopItemUI : MonoBehaviour
{
    [Header("UI Data Elements")]
    public TextMeshProUGUI itemNameText;
    public TextMeshProUGUI itemCostText;
    public Image itemIconImage;

    [Header("UI State Elements")]
    public GameObject lockedOverlay;
    public GameObject equippedOutline;
    public Button interactionButton;
    public TextMeshProUGUI buttonActionText;

    [Header("Modular Animation Hooks (Tweening)")]
    [Tooltip("Fires when the item UI is first initialized")]
    public UnityEvent OnInit;
    [Tooltip("Fires when the item is successfully purchased")]
    public UnityEvent OnPurchaseSuccess;
    [Tooltip("Fires when a purchase fails (e.g. insufficient funds)")]
    public UnityEvent OnPurchaseFail;
    [Tooltip("Fires when this item becomes equipped")]
    public UnityEvent OnEquipped;

    private VirusSkin linkedSkin;
    private ShopController parentController;

    /// <summary>
    /// Initializes the UI template with the specific skin data.
    /// </summary>
    public void Initialize(VirusSkin skin, ShopController controller)
    {
        linkedSkin = skin;
        parentController = controller;

        if (itemNameText != null) itemNameText.text = skin.displayName;
        if (itemIconImage != null) itemIconImage.sprite = skin.storeIcon;
        if (itemCostText != null) itemCostText.text = skin.cost.ToString();

        // Hook up the button
        if (interactionButton != null)
        {
            interactionButton.onClick.RemoveAllListeners();
            interactionButton.onClick.AddListener(OnInteractionClicked);
        }

        OnInit?.Invoke();
        RefreshState();
    }

    /// <summary>
    /// Refreshes the visual state (Locked, Unlocked, Equipped) based on the Skin Manager.
    /// </summary>
    public void RefreshState()
    {
        bool isUnlocked = SkinManager.Instance.IsSkinUnlocked(linkedSkin.skinID);
        bool isEquipped = SkinManager.Instance.GetEquippedSkinID() == linkedSkin.skinID;
        bool isAffordable = EconomyManager.Instance.GetBalance() >= linkedSkin.cost;

        // Visual Toggles
        if (lockedOverlay != null) lockedOverlay.SetActive(!isUnlocked);
        if (equippedOutline != null) equippedOutline.SetActive(isEquipped);

        // Update Button Logic & Text
        if (buttonActionText != null)
        {
            if (isEquipped)
            {
                buttonActionText.text = "EQUIPPED";
                interactionButton.interactable = false;
            }
            else if (isUnlocked)
            {
                buttonActionText.text = "EQUIP";
                interactionButton.interactable = true;
            }
            else
            {
                buttonActionText.text = isAffordable ? "BUY" : "LOCKED (NEED FUNDS)";
                interactionButton.interactable = isAffordable;
            }
        }
    }

    /// <summary>
    /// Fires when the player clicks the interactive button on this grid item.
    /// Handles purchasing logic vs equipping logic.
    /// </summary>
    private void OnInteractionClicked()
    {
        bool isUnlocked = SkinManager.Instance.IsSkinUnlocked(linkedSkin.skinID);

        if (isUnlocked)
        {
            // Equip Logic
            SkinManager.Instance.EquipSkin(linkedSkin.skinID);
            OnEquipped?.Invoke();
            parentController.RefreshAllItems(); // Update grid to show new equipped status
        }
        else
        {
            // Purchase Logic
            if (EconomyManager.Instance.SpendFragments(linkedSkin.cost))
            {
                // Successful purchase
                SkinManager.Instance.UnlockSkin(linkedSkin.skinID);
                SkinManager.Instance.EquipSkin(linkedSkin.skinID); // Optional auto-equip
                
                OnPurchaseSuccess?.Invoke();
                parentController.RefreshAllItems();
            }
            else
            {
                // Failed purchase
                OnPurchaseFail?.Invoke();
            }
        }
    }
}
