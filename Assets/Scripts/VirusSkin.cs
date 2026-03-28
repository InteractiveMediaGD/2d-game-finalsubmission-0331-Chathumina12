using UnityEngine;

/// <summary>
/// Defines data for a Virus cosmetic skin, integrating directly with ShopUI and gameplay mechanics.
/// </summary>
[CreateAssetMenu(fileName = "NewVirusSkin", menuName = "Store/Virus Skin", order = 1)]
public class VirusSkin : ScriptableObject
{
    [Header("Shop Information")]
    [Tooltip("Unique Identifier for this skin (e.g. 'ninja' or 'neon')")]
    public string skinID;

    [Tooltip("The name displayed to the user in the shop")]
    public string displayName;

    [Tooltip("Cost in Data Fragments. Set to 0 if default.")]
    public int cost;

    [Tooltip("The preview sprite shown in the Shop UI grid")]
    public Sprite storeIcon;

    [Header("In-Game Apply Settings")]
    [Tooltip("The actual sprite applied to the Virus player object")]
    public Sprite playerSprite;

    [Tooltip("The neon color applied to the TrailRenderer component behind the player")]
    public Color trailColor = Color.cyan;

    [Tooltip("Specific particle effect spawned when the virus gets destroyed!")]
    public GameObject deathParticlePrefab;
}
