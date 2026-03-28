#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

/// <summary>
/// Automates the creation of the System Admin Boss Prefab and Manager.
/// Run via Tools > Build Boss Fight System.
/// </summary>
public class BossSystemSetupBuilder : EditorWindow
{
    [MenuItem("Tools/Build Boss Fight System")]
    public static void BuildBossSystem()
    {
        // 1. Always regenerate the prefab
        string prefabPath = "Assets/Prefabs/SystemAdminBoss.prefab";
        GameObject existing = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        if (existing != null)
        {
            AssetDatabase.DeleteAsset(prefabPath);
            Debug.Log("[BossSystem] Deleted old boss prefab — regenerating.");
        }

        // --- Create the Boss hierarchy ---
        GameObject boss = new GameObject("SystemAdminBoss");
        boss.tag = "EnemyBody";

        // Visual — use the PixelWhale armored beast sprite for the boss
        SpriteRenderer sr = boss.AddComponent<SpriteRenderer>();
        Sprite bossSprite = AssetDatabase.LoadAssetAtPath<Sprite>(
            "Assets/Prefabs/Game assets/PixelWhale_SF_Project/Character/9svvgui4ylvz clone.png");
        if (bossSprite != null)
        {
            sr.sprite = bossSprite;
            sr.color  = Color.white;
            boss.transform.localScale = new Vector3(1.0f, 1.0f, 1f);
        }
        else
        {
            // Fallback if sprite not found
            sr.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd");
            sr.color  = new Color(0.45f, 0.05f, 0.08f, 1f);
            boss.transform.localScale = new Vector3(2.5f, 2.5f, 1f);
            Debug.LogWarning("[BossSystem] PixelWhale boss sprite not found — using placeholder.");
        }

        // Physics — Kinematic Rigidbody2D + trigger collider
        Rigidbody2D rb = boss.AddComponent<Rigidbody2D>();
        rb.isKinematic = true;
        rb.gravityScale = 0f;

        BoxCollider2D col = boss.AddComponent<BoxCollider2D>();
        col.isTrigger = true;

        // Main boss script
        SystemAdminBoss bossScript = boss.AddComponent<SystemAdminBoss>();

        // Assign projectile prefab if available
        GameObject projPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/EnemyProjectile.prefab");
        if (projPrefab != null) bossScript.projectilePrefab = projPrefab;

        // Assign explosion prefab if available
        GameObject expPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/DeathParticles.prefab");
        if (expPrefab != null) bossScript.explosionPrefab = expPrefab;

        // Save prefab
        if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
            AssetDatabase.CreateFolder("Assets", "Prefabs");

        PrefabUtility.SaveAsPrefabAsset(boss, prefabPath);
        DestroyImmediate(boss);

        GameObject savedPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        Debug.Log("[BossSystem] Generated SystemAdminBoss prefab at " + prefabPath);

        // 2. Setup BossManager in scene
        BossManager manager = FindObjectOfType<BossManager>();
        if (manager == null)
        {
            GameObject managerObj = new GameObject("BossManager");
            manager = managerObj.AddComponent<BossManager>();
        }

        manager.bossPrefab = savedPrefab;
        Selection.activeGameObject = manager.gameObject;

        Debug.Log("[BossSystem] BossManager hooked into scene with prefab reference.");
    }
}
#endif
