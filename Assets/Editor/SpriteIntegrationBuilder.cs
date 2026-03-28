#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

/// <summary>
/// One-click integration of PixelWhale_SF_Project sprites into existing prefabs.
/// Run via Tools > Apply PixelWhale Sprites.
/// 
/// Sprite Mapping (chosen after visual inspection):
///   Player (Virus)        → Character/Shooting Soldier.png      (armored pixel hacker)
///   Boss (SystemAdmin)    → Character/9svvgui4ylvz clone.png    (large armored beast)
///   Enemy (Antivirus)     → Character/New Piskel.png            (yellow cyber-dog)
///   Enemy (Shooter)       → Character/illustration2 clone.png   (white armored grunt)
///   Player Projectile     → Object/adawx4.png                   (blue laser gun beam)
///   Enemy Projectile      → Object/adawx3.png                   (holographic panel blast)
///   UI Panel (shop/miss.) → Ui/Ui_09.png                        (cyan sci-fi panels)
///   Crosshair / HUD       → Ui/Ui_11.png                        (cyan HUD elements)
///   Skill buttons         → Ui/Skill_Frame.png                  (circular target frame)
/// </summary>
public class SpriteIntegrationBuilder : EditorWindow
{
    // ===== Paths =====
    private const string PACK = "Assets/Prefabs/Game assets/PixelWhale_SF_Project/";

    // Characters
    private const string SP_PLAYER      = PACK + "Character/Shooting Soldier.png";
    private const string SP_BOSS        = PACK + "Character/9svvgui4ylvz clone.png";
    private const string SP_ENEMY_AV    = PACK + "Character/New Piskel.png";
    private const string SP_ENEMY_SHOOT = PACK + "Character/illustration2 clone.png";

    // Combat Objects
    private const string SP_PROJ_PLAYER = PACK + "Object/adawx4.png";   // blue laser gun
    private const string SP_PROJ_ENEMY  = PACK + "Object/adawx3.png";   // holographic panel blast

    // UI 
    private const string SP_UI_PANEL    = PACK + "Ui/Ui_09.png";        // sci-fi panel set
    private const string SP_UI_HUD      = PACK + "Ui/Ui_11.png";        // HUD/crosshair sheet
    private const string SP_UI_FRAME    = PACK + "Ui/Skill_Frame.png";  // circular target lock
    private const string SP_UI_BTN      = PACK + "Ui/Ui_03.png";        // tech button

    // Prefab Paths
    private const string PF_BOSS        = "Assets/Prefabs/SystemAdminBoss.prefab";
    private const string PF_ENEMY_AV    = "Assets/Prefabs/Enemy_Antivirus.prefab";
    private const string PF_ENEMY_SHOOT = "Assets/Prefabs/EnemyShooter.prefab";
    private const string PF_PROJ_PLAYER = "Assets/Prefabs/Projectile.prefab";
    private const string PF_PROJ_ENEMY  = "Assets/Prefabs/EnemyProjectile.prefab";

    // ===================================================================
    [MenuItem("Tools/Apply PixelWhale Sprites")]
    public static void ApplyAllSprites()
    {
        int appliedCount = 0;

        appliedCount += ApplyPlayerSprite();
        appliedCount += ApplyBossSprite();
        appliedCount += ApplyEnemyAntivirusSprite();
        appliedCount += ApplyEnemyShooterSprite();
        appliedCount += ApplyPlayerProjectileSprite();
        appliedCount += ApplyEnemyProjectileSprite();
        appliedCount += ApplyUISprites();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog(
            "PixelWhale Integration Complete",
            $"Applied sprites to {appliedCount} objects.\n\nReview the Console for any warnings about missing sprites.",
            "OK"
        );

        Debug.Log($"[SpriteIntegration] Done! Applied {appliedCount} sprite assignments.");
    }

    // ===================================================================
    //  1. PLAYER (VirusController in scene)
    // ===================================================================
    private static int ApplyPlayerSprite()
    {
        Sprite playerSprite = LoadSprite(SP_PLAYER);
        if (playerSprite == null) { Warn("Player sprite", SP_PLAYER); return 0; }

        // Apply to live scene object (player is in the scene, not a prefab)
        VirusController vc = Object.FindObjectOfType<VirusController>();
        if (vc != null)
        {
            SpriteRenderer sr = vc.GetComponent<SpriteRenderer>();
            if (sr == null) sr = vc.gameObject.AddComponent<SpriteRenderer>();
            sr.sprite = playerSprite;
            sr.color  = Color.white;
            // Scale: Shooting Soldier is a small pixel sprite, scale up a bit
            vc.transform.localScale = new Vector3(0.5f, 0.5f, 1f);
            EditorUtility.SetDirty(vc.gameObject);
            Debug.Log("[SpriteIntegration] ✓ Player → Shooting Soldier.png");
            return 1;
        }

        Warn("VirusController not found in scene", "run in GameScene with player in hierarchy");
        return 0;
    }

    // ===================================================================
    //  2. BOSS PREFAB
    // ===================================================================
    private static int ApplyBossSprite()
    {
        return ApplySpriteToPrefab(PF_BOSS, SP_BOSS, "SystemAdminBoss",
            scale: new Vector3(1f, 1f, 1f),
            // Adjust hitbox to match the armored beast silhouette (~200×160 px sprite)
            colliderSize: new Vector2(1.2f, 0.9f));
    }

    // ===================================================================
    //  3. ENEMY ANTIVIRUS PREFAB
    // ===================================================================
    private static int ApplyEnemyAntivirusSprite()
    {
        return ApplySpriteToPrefab(PF_ENEMY_AV, SP_ENEMY_AV, "Enemy_Antivirus",
            scale: new Vector3(0.4f, 0.4f, 1f),
            colliderSize: new Vector2(1.8f, 1.0f));
    }

    // ===================================================================
    //  4. ENEMY SHOOTER PREFAB
    // ===================================================================
    private static int ApplyEnemyShooterSprite()
    {
        return ApplySpriteToPrefab(PF_ENEMY_SHOOT, SP_ENEMY_SHOOT, "EnemyShooter",
            scale: new Vector3(0.35f, 0.35f, 1f),
            colliderSize: new Vector2(1.2f, 1.8f));
    }

    // ===================================================================
    //  5. PLAYER PROJECTILE PREFAB
    // ===================================================================
    private static int ApplyPlayerProjectileSprite()
    {
        Sprite sprite = LoadSprite(SP_PROJ_PLAYER);
        if (sprite == null) { Warn("Player Projectile sprite", SP_PROJ_PLAYER); return 0; }

        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PF_PROJ_PLAYER);
        if (prefab == null) { Warn("Prefab", PF_PROJ_PLAYER); return 0; }

        using (var scope = new PrefabUtility.EditPrefabContentsScope(PF_PROJ_PLAYER))
        {
            GameObject root = scope.prefabContentsRoot;

            SpriteRenderer sr = root.GetComponent<SpriteRenderer>();
            if (sr == null) sr = root.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.color  = new Color(0.4f, 0.8f, 1f, 1f); // blue laser tint
            root.transform.localScale = new Vector3(0.25f, 0.25f, 1f);

            // Adjust collider to laser beam proportions (wide on X, thin on Y)
            BoxCollider2D bc = root.GetComponent<BoxCollider2D>();
            if (bc != null) bc.size = new Vector2(2.0f, 0.5f);
        }

        Debug.Log("[SpriteIntegration] ✓ PlayerProjectile → adawx4.png (blue laser)");
        return 1;
    }

    // ===================================================================
    //  6. ENEMY PROJECTILE PREFAB
    // ===================================================================
    private static int ApplyEnemyProjectileSprite()
    {
        Sprite sprite = LoadSprite(SP_PROJ_ENEMY);
        if (sprite == null) { Warn("Enemy Projectile sprite", SP_PROJ_ENEMY); return 0; }

        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PF_PROJ_ENEMY);
        if (prefab == null) { Warn("Prefab", PF_PROJ_ENEMY); return 0; }

        using (var scope = new PrefabUtility.EditPrefabContentsScope(PF_PROJ_ENEMY))
        {
            GameObject root = scope.prefabContentsRoot;

            SpriteRenderer sr = root.GetComponent<SpriteRenderer>();
            if (sr == null) sr = root.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.color  = new Color(1f, 0.5f, 0.1f, 1f); // orange threat tint
            root.transform.localScale = new Vector3(0.28f, 0.28f, 1f);

            // Collider: holographic panel shape (squarish)
            BoxCollider2D bc = root.GetComponent<BoxCollider2D>();
            if (bc != null) bc.size = new Vector2(1.0f, 1.0f);
        }

        Debug.Log("[SpriteIntegration] ✓ EnemyProjectile → adawx3.png (orange blast)");
        return 1;
    }

    // ===================================================================
    //  7. UI SPRITES — applied to live scene canvases
    // ===================================================================
    private static int ApplyUISprites()
    {
        int total = 0;

        Sprite panelSprite  = LoadSprite(SP_UI_PANEL);
        Sprite frameSprite  = LoadSprite(SP_UI_FRAME);
        Sprite hudSprite    = LoadSprite(SP_UI_HUD);
        Sprite btnSprite    = LoadSprite(SP_UI_BTN);

        // ---- Mission Overlay Canvas ----
        Canvas[] allCanvases = Object.FindObjectsOfType<Canvas>();
        foreach (Canvas canvas in allCanvases)
        {
            string cName = canvas.gameObject.name.ToLower();

            // Mission overlay container backgrounds
            if (cName.Contains("mission"))
            {
                total += ApplySpriteToChildImages(canvas.gameObject, "MissionCard", frameSprite, new Color(0.1f, 0.8f, 0.9f, 0.85f));
                Debug.Log($"[SpriteIntegration] ✓ MissionCanvas '{canvas.name}' → Skill_Frame borders");
            }

            // Shop canvas
            if (cName.Contains("shop"))
            {
                total += ApplySpriteToChildImages(canvas.gameObject, "Panel", panelSprite, Color.white);
                total += ApplySpriteToChildImages(canvas.gameObject, "Button", btnSprite, Color.white);
                Debug.Log($"[SpriteIntegration] ✓ ShopCanvas '{canvas.name}' → Ui_09 panels + Ui_03 buttons");
            }

            // Game HUD canvas (score, health)
            if (cName.Contains("gamehud") || cName.Contains("hud") || cName.Contains("gamecanvas"))
            {
                total += ApplySpriteToChildImages(canvas.gameObject, "Panel", panelSprite, new Color(0.6f, 0.9f, 1f, 0.8f));
                Debug.Log($"[SpriteIntegration] ✓ HUDCanvas '{canvas.name}' → Ui_09 background");
            }
        }

        // No canvases found — skip gracefully
        if (allCanvases.Length == 0)
            Debug.LogWarning("[SpriteIntegration] No canvases found in scene. Open the GameScene and re-run.");

        return total;
    }

    // ===================================================================
    //  HELPERS
    // ===================================================================

    /// <summary>
    /// Opens a prefab, sets its SpriteRenderer, scale, and collider size, then saves.
    /// </summary>
    private static int ApplySpriteToPrefab(string prefabPath, string spritePath,
        string label, Vector3 scale, Vector2 colliderSize)
    {
        Sprite sprite = LoadSprite(spritePath);
        if (sprite == null) { Warn($"{label} sprite", spritePath); return 0; }

        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        if (prefab == null) { Warn($"{label} prefab", prefabPath); return 0; }

        using (var scope = new PrefabUtility.EditPrefabContentsScope(prefabPath))
        {
            GameObject root = scope.prefabContentsRoot;

            SpriteRenderer sr = root.GetComponent<SpriteRenderer>();
            if (sr == null) sr = root.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.color  = Color.white;

            root.transform.localScale = scale;

            BoxCollider2D bc = root.GetComponent<BoxCollider2D>();
            if (bc != null) bc.size = colliderSize;
        }

        Debug.Log($"[SpriteIntegration] ✓ {label} → {System.IO.Path.GetFileName(spritePath)}");
        return 1;
    }

    /// <summary>
    /// Searches all Image components inside a parent whose name contains nameFilter
    /// and applies the given sprite with tint.
    /// </summary>
    private static int ApplySpriteToChildImages(GameObject parent, string nameFilter,
        Sprite sprite, Color tint)
    {
        if (sprite == null) return 0;

        Image[] images = parent.GetComponentsInChildren<Image>(true);
        int count = 0;
        foreach (Image img in images)
        {
            if (img.gameObject.name.ToLower().Contains(nameFilter.ToLower()))
            {
                img.sprite = sprite;
                img.color  = tint;
                EditorUtility.SetDirty(img);
                count++;
            }
        }
        return count;
    }

    /// <summary>Loads a sprite from an asset path and logs a warning if missing.</summary>
    private static Sprite LoadSprite(string path)
    {
        Sprite s = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        if (s == null)
        {
            // Some sprites are in spritesheets — try loading as Texture2D
            Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            if (tex != null)
            {
                // Create a sprite from the whole texture
                s = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height),
                    new Vector2(0.5f, 0.5f), 100f);
            }
        }
        return s;
    }

    private static void Warn(string label, string path) =>
        Debug.LogWarning($"[SpriteIntegration] ⚠ Could not load {label} at: {path}");
}
#endif
