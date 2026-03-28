using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Singleton manager that handles saving, loading, unlocking, and equipping skins using PlayerPrefs.
/// </summary>
public class SkinManager : MonoBehaviour
{
    public static SkinManager Instance { get; private set; }

    [Header("Skin Database")]
    [Tooltip("Array holding all available skins in the game. Used to load the default skin.")]
    [SerializeField] private VirusSkin[] allSkins;

    private string GetUnlockedKey() => "UnlockedSkins";
    private string GetEquippedKey() => "EquippedSkin";

    // Hashset for fast lookup of unlocked skin IDs
    private HashSet<string> unlockedSkins = new HashSet<string>();
    private string equippedSkinID;

    public System.Action<string> OnSkinEquipped;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        LoadSkins();
    }



    /// <summary>
    /// Loads the unlocked skins and currently equipped skin from PlayerPrefs.
    /// </summary>
    private void LoadSkins()
    {
        // Load Unlocked Skins
        string unlockedString = PlayerPrefs.GetString(GetUnlockedKey(), "default"); // "default" is unlocked initially
        string[] splitIds = unlockedString.Split(new char[] { ',' }, System.StringSplitOptions.RemoveEmptyEntries);
        
        unlockedSkins.Clear();
        foreach (var id in splitIds)
        {
            unlockedSkins.Add(id);
        }

        // Load Equipped Skin
        equippedSkinID = PlayerPrefs.GetString(GetEquippedKey(), "default");

        // Failsafe: if the equipped skin isn't in our unlocked list, revert to default.
        if (!unlockedSkins.Contains(equippedSkinID))
        {
            EquipSkin("default");
        }
    }

    private void SaveSkins()
    {
        // Convert HashSet to comma-separated string
        string unlockedString = string.Join(",", unlockedSkins);
        PlayerPrefs.SetString(GetUnlockedKey(), unlockedString);
        PlayerPrefs.SetString(GetEquippedKey(), equippedSkinID);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// Unlocks a skin permanently. Note: Does not deduct cost here; handled by StoreUI.
    /// </summary>
    public void UnlockSkin(string skinID)
    {
        if (unlockedSkins.Add(skinID)) // Add returns true if it wasn't already in the set
        {
            SaveSkins();
            Debug.Log($"[SkinManager] Unlocked new skin: {skinID}");
        }
    }

    /// <summary>
    /// Equips a skin if it has been unlocked.
    /// </summary>
    public bool EquipSkin(string skinID)
    {
        if (unlockedSkins.Contains(skinID))
        {
            equippedSkinID = skinID;
            SaveSkins();
            OnSkinEquipped?.Invoke(equippedSkinID);
            Debug.Log($"[SkinManager] Equipped skin: {skinID}");
            return true;
        }
        
        Debug.LogWarning($"[SkinManager] Attempted to equip locked skin: {skinID}");
        return false;
    }

    /// <summary>
    /// Checks if the specific skin is unlocked.
    /// </summary>
    public bool IsSkinUnlocked(string skinID)
    {
        return unlockedSkins.Contains(skinID);
    }

    /// <summary>
    /// Returns the currently equipped SkinID.
    /// </summary>
    public string GetEquippedSkinID()
    {
        return equippedSkinID;
    }

    /// <summary>
    /// Returns the full VirusSkin scriptable object for the currently equipped skin.
    /// </summary>
    public VirusSkin GetEquippedSkinData()
    {
        if (allSkins == null || allSkins.Length == 0) return null;
        
        return allSkins.FirstOrDefault(s => s.skinID == equippedSkinID) ?? allSkins[0];
    }

    /// <summary>
    /// Applies the equipped VirusSkin visuals dynamically to a target player object.
    /// </summary>
    public void ApplySkinToPlayer(GameObject playerObject)
    {
        VirusSkin equippedSkin = GetEquippedSkinData();
        if (equippedSkin == null || playerObject == null) return;

        // Apply Sprite
        SpriteRenderer sr = playerObject.GetComponent<SpriteRenderer>();
        if (sr != null && equippedSkin.playerSprite != null)
        {
            sr.sprite = equippedSkin.playerSprite;
        }

        // Apply TrailRenderer Color
        TrailRenderer tr = playerObject.GetComponent<TrailRenderer>();
        if (tr != null)
        {
            tr.startColor = equippedSkin.trailColor;
            tr.endColor = equippedSkin.trailColor;
        }

        // Store Death Particle Prefab into the VirusController directly
        VirusController vc = playerObject.GetComponent<VirusController>();
        if (vc != null)
        {
            vc.deathParticlePrefab = equippedSkin.deathParticlePrefab;
        }

        Debug.Log($"[SkinManager] Applied {equippedSkin.displayName} skin properties dynamically!");
    }
    
    // Editor functions for debugging
    [ContextMenu("Run: Unlock All Skins")]
    private void DebugUnlockAll()
    {
        if (allSkins != null)
        {
            foreach (var skin in allSkins)
            {
                UnlockSkin(skin.skinID);
            }
        }
    }
}
