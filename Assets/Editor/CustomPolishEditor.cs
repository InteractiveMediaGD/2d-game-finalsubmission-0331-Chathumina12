#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;
using System.IO;

/// <summary>
/// One-click visual overhaul editor script.
/// Menu: Game Polish → Apply Full Overhaul
///
/// What it does:
///  1. Scans the asset directories to find backgrounds, bars, buttons, and sprites.
///  2. Assigns background sprites to background GameObjects in the scene.
///  3. Rebuilds the Health Bar with Cyberpunk bar assets from the project.
///  4. Applies button sprites from the Cyberpunk pack to every Button in the scene.
///  5. Creates HealthPack and RapidFire pickup Prefabs in Assets/Prefabs/Pickups/.
///  6. Wires the new scripts (HealthBar, PowerUpFloat, MainMenuUI).
/// </summary>
public class CustomPolishEditor : Editor
{
    // ─────────────────────────────────────────────────────────────────────
    //  Root asset paths (relative to Assets/)
    // ─────────────────────────────────────────────────────────────────────
    private const string BG_PATH     = "Assets/Prefabs/Game assets/PixelWhale_SF_Project/Background";
    private const string BARS_PATH   = "Assets/Prefabs/Game assets/Cyberpunk_UI_Asset_Pack_v1.1/03_Bars_and_Indicators/Progress_Bar";
    private const string BTN_PATH    = "Assets/Prefabs/Game assets/Cyberpunk_UI_Asset_Pack_v1.1/02_Interactive_Buttons/Primary_Button";
    private const string PANEL_PATH  = "Assets/Prefabs/Game assets/Cyberpunk_UI_Asset_Pack_v1.1/01_Panels_and_Windows";
    private const string ROBOT_PATH  = "Assets/Prefabs/Game assets/TopView_Robot_Asset_Pack";
    private const string OBJ_PATH    = "Assets/Prefabs/Game assets/PixelWhale_SF_Project/Object";
    private const string PICKUP_OUT  = "Assets/Prefabs/Pickups";

    // ─────────────────────────────────────────────────────────────────────
    //  Entry point
    // ─────────────────────────────────────────────────────────────────────
    [MenuItem("Game Polish/Apply Full Overhaul")]
    public static void ApplyOverhaul()
    {
        Debug.Log("[CustomPolishEditor] == Starting Full Overhaul ==");

        ApplyBackground();
        ApplyHealthBar();
        ApplyButtonSprites();
        CreatePickupPrefabs();
        ApplyMainMenuUI();

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        AssetDatabase.SaveAssets();
        Debug.Log("[CustomPolishEditor] == Overhaul Complete! Save your scene. ==");
    }

    // ─────────────────────────────────────────────────────────────────────
    //  1. BACKGROUND
    // ─────────────────────────────────────────────────────────────────────
    [MenuItem("Game Polish/Apply Background Only")]
    public static void ApplyBackground()
    {
        // Best background for a top-down sci-fi runner: top_view_city_bg_01.png
        Sprite bgSprite = LoadSprite(BG_PATH + "/top_view_city_bg_01.png");
        if (bgSprite == null)
            bgSprite = LoadSprite(BG_PATH + "/sf_space.png"); // fallback

        if (bgSprite == null)
        {
            Debug.LogWarning("[CustomPolishEditor] No background sprite found. Skipping.");
            return;
        }

        // Try to find existing background SpriteRenderer in scene
        SpriteRenderer[] allRenderers = FindObjectsByType<SpriteRenderer>(FindObjectsSortMode.None);
        SpriteRenderer bgRenderer = null;

        foreach (SpriteRenderer sr in allRenderers)
        {
            string n = sr.gameObject.name.ToLower();
            if (n.Contains("background") || n.Contains("bg") || n.Contains("back"))
            {
                bgRenderer = sr;
                break;
            }
        }

        if (bgRenderer == null)
        {
            // Create a new background object
            GameObject bgObj = new GameObject("Background_Layer");
            bgObj.transform.position = new Vector3(0, 0, 10f); // push behind everything
            bgRenderer = bgObj.AddComponent<SpriteRenderer>();
            bgRenderer.sortingOrder = -10;
        }

        bgRenderer.sprite = bgSprite;
        bgRenderer.drawMode = SpriteDrawMode.Tiled;
        bgRenderer.size = new Vector2(40f, 25f);

        // Attach BackgroundScroller if not present
        BackgroundScroller scroller = bgRenderer.GetComponent<BackgroundScroller>();
        if (scroller == null) bgRenderer.gameObject.AddComponent<BackgroundScroller>();

        Debug.Log($"[CustomPolishEditor] Background assigned: {bgSprite.name}");

        // Second far layer (sf_space) for parallax depth
        Sprite farSprite = LoadSprite(BG_PATH + "/sf_space.png");
        if (farSprite != null && bgSprite.name != "sf_space")
        {
            GameObject farObj = new GameObject("Background_FarLayer");
            farObj.transform.position = new Vector3(0, 0, 11f);
            SpriteRenderer farSR = farObj.AddComponent<SpriteRenderer>();
            farSR.sortingOrder = -11;
            farSR.sprite = farSprite;
            farSR.drawMode = SpriteDrawMode.Tiled;
            farSR.size = new Vector2(40f, 25f);
            farSR.color = new Color(0.5f, 0.5f, 0.7f, 0.6f); // dim it to be "far"

            BackgroundScroller farScroller = farObj.AddComponent<BackgroundScroller>();
            // parallaxFactor will default to 0.5 in the script — set a lower one via SerializedObject
            SerializedObject so = new SerializedObject(farScroller);
            so.FindProperty("parallaxFactor").floatValue = 0.25f;
            so.ApplyModifiedProperties();
        }
    }

    // ─────────────────────────────────────────────────────────────────────
    //  2. HEALTH BAR
    // ─────────────────────────────────────────────────────────────────────
    [MenuItem("Game Polish/Apply Health Bar Only")]
    public static void ApplyHealthBar()
    {
        Sprite barBkg  = LoadSprite(BARS_PATH + "/Progress_Bar_Bk.png");
        Sprite barFill = LoadSprite(BARS_PATH + "/Progress_Bar_Fill.png");

        if (barBkg == null || barFill == null)
        {
            Debug.LogWarning("[CustomPolishEditor] Cyberpunk progress bar sprites not found. Skipping health bar.");
            return;
        }

        // Find Slider in scene (the existing health bar)
        Slider[] sliders = FindObjectsByType<Slider>(FindObjectsSortMode.None);
        Slider healthSlider = null;

        foreach (Slider s in sliders)
        {
            string n = s.gameObject.name.ToLower();
            if (n.Contains("health") || n.Contains("hp") || n.Contains("life"))
            {
                healthSlider = s;
                break;
            }
        }

        if (healthSlider == null && sliders.Length > 0)
            healthSlider = sliders[0]; // fallback: first slider in scene

        if (healthSlider == null)
        {
            Debug.LogWarning("[CustomPolishEditor] No health Slider found in scene.");
            return;
        }

        // Apply background sprite
        Image bgImg = healthSlider.GetComponentInChildren<Image>();
        if (bgImg != null)
        {
            bgImg.sprite = barBkg;
            bgImg.type   = Image.Type.Sliced;
        }

        // Apply fill sprite
        if (healthSlider.fillRect != null)
        {
            Image fillImg = healthSlider.fillRect.GetComponent<Image>();
            if (fillImg != null)
            {
                fillImg.sprite     = barFill;
                fillImg.type       = Image.Type.Sliced;
                fillImg.fillMethod = Image.FillMethod.Horizontal;
                fillImg.color      = new Color(0.10f, 0.95f, 0.40f); // start green
            }
        }

        // Attach HealthBar script
        HealthBar hb = healthSlider.gameObject.GetComponent<HealthBar>();
        if (hb == null)
        {
            hb = healthSlider.gameObject.AddComponent<HealthBar>();
            // Wire fill image
            SerializedObject so = new SerializedObject(hb);
            if (healthSlider.fillRect != null)
                so.FindProperty("fillImage").objectReferenceValue = healthSlider.fillRect.GetComponent<Image>();
            so.FindProperty("backgroundImage").objectReferenceValue = bgImg;
            so.ApplyModifiedProperties();
        }

        Debug.Log($"[CustomPolishEditor] Health bar upgraded with Cyberpunk sprites.");
    }

    // ─────────────────────────────────────────────────────────────────────
    //  3. BUTTON SPRITES
    // ─────────────────────────────────────────────────────────────────────
    [MenuItem("Game Polish/Apply Button Sprites Only")]
    public static void ApplyButtonSprites()
    {
        Sprite btnNormal  = LoadSprite(BTN_PATH + "/Btn_Primary_Normal.png");
        Sprite btnHover   = LoadSprite(BTN_PATH + "/Btn_Primary_Hover.png");
        Sprite btnPressed = LoadSprite(BTN_PATH + "/Btn_Primary_Pressed.png");

        if (btnNormal == null)
        {
            Debug.LogWarning("[CustomPolishEditor] Primary button sprites not found. Skipping.");
            return;
        }

        Button[] buttons = FindObjectsByType<Button>(FindObjectsSortMode.None);
        int count = 0;

        foreach (Button btn in buttons)
        {
            Image img = btn.GetComponent<Image>();
            if (img == null) continue;

            img.sprite = btnNormal;
            img.type   = Image.Type.Sliced;

            SpriteState ss = new SpriteState();
            if (btnHover   != null) ss.highlightedSprite = btnHover;
            if (btnPressed != null) ss.pressedSprite     = btnPressed;
            btn.spriteState   = ss;
            btn.transition    = Selectable.Transition.SpriteSwap;

            count++;
        }

        Debug.Log($"[CustomPolishEditor] Applied Cyberpunk button sprites to {count} buttons.");
    }

    // ─────────────────────────────────────────────────────────────────────
    //  4. CREATE PICKUP PREFABS
    // ─────────────────────────────────────────────────────────────────────
    [MenuItem("Game Polish/Create Pickup Prefabs Only")]
    public static void CreatePickupPrefabs()
    {
        Directory.CreateDirectory(Path.Combine(Application.dataPath, "../" + PICKUP_OUT));
        AssetDatabase.Refresh();

        // Skill icons from PixelWhale UI folder — use two distinct ones
        Sprite healthSprite  = LoadSprite(OBJ_PATH + "/asdasx1.png"); // visually distinct sci-fi object
        if (healthSprite == null)
            healthSprite = LoadSprite(OBJ_PATH + "/sf_fire_planet.png");

        Sprite rapidSprite   = LoadSprite(OBJ_PATH + "/sf_ice.png");
        if (rapidSprite == null)
            rapidSprite = LoadSprite(OBJ_PATH + "/adasdasdwx.png");

        CreatePickupPrefab("HealthPack",  healthSprite, new Color(0.1f, 0.95f, 0.3f), PICKUP_OUT + "/HealthPack.prefab");
        CreatePickupPrefab("RapidFire",   rapidSprite,  new Color(0.2f, 0.7f,  1.0f), PICKUP_OUT + "/RapidFire.prefab");

        AssetDatabase.Refresh();
        Debug.Log("[CustomPolishEditor] HealthPack and RapidFire prefabs created in " + PICKUP_OUT);
    }

    private static void CreatePickupPrefab(string goName, Sprite sprite, Color glowColor, string savePath)
    {
        // Root object
        GameObject root = new GameObject(goName);
        root.layer = LayerMask.NameToLayer("Default");
        root.tag   = "Pickup";

        // Sprite Renderer (main)
        SpriteRenderer sr = root.AddComponent<SpriteRenderer>();
        if (sprite != null) sr.sprite = sprite;
        sr.sortingOrder = 5;
        sr.color        = Color.white;

        // Glow SpriteRenderer (child, slightly larger, colored)
        GameObject glowObj = new GameObject("Glow");
        glowObj.transform.SetParent(root.transform, false);
        SpriteRenderer glowSR = glowObj.AddComponent<SpriteRenderer>();
        if (sprite != null) glowSR.sprite = sprite;
        glowSR.color        = new Color(glowColor.r, glowColor.g, glowColor.b, 0.5f);
        glowSR.sortingOrder = 4;
        glowObj.transform.localScale = Vector3.one * 1.3f;

        // CircleCollider2D as trigger
        CircleCollider2D col = root.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius    = 0.4f;

        // Rigidbody2D (kinematic — so it doesn't fall)
        Rigidbody2D rb = root.AddComponent<Rigidbody2D>();
        rb.bodyType  = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;

        // PowerUpFloat animation
        PowerUpFloat floatScript = root.AddComponent<PowerUpFloat>();
        SerializedObject so = new SerializedObject(floatScript);
        if (glowSR != null)
            so.FindProperty("glowRenderer").objectReferenceValue = glowSR;
        so.ApplyModifiedProperties();

        // Save as prefab
        string fullPath = savePath;
        if (!fullPath.StartsWith("Assets/"))
            fullPath = "Assets/" + fullPath;

        PrefabUtility.SaveAsPrefabAsset(root, PICKUP_OUT + "/" + goName + ".prefab");
        DestroyImmediate(root);
    }

    // ─────────────────────────────────────────────────────────────────────
    //  5. MAIN MENU UI SCRIPT ATTACHMENT
    // ─────────────────────────────────────────────────────────────────────
    [MenuItem("Game Polish/Apply MainMenu UI Script Only")]
    public static void ApplyMainMenuUI()
    {
        MainMenuController mmc = FindFirstObjectByType<MainMenuController>();
        if (mmc == null)
        {
            Debug.LogWarning("[CustomPolishEditor] MainMenuController not found in scene. Open the MainMenu scene first.");
            return;
        }

        MainMenuUI ui = mmc.GetComponent<MainMenuUI>();
        if (ui == null)
        {
            ui = mmc.gameObject.AddComponent<MainMenuUI>();
            Debug.Log("[CustomPolishEditor] MainMenuUI script attached to " + mmc.gameObject.name);
        }

        // Wire all Buttons found on the canvas
        Button[] allBtns = FindObjectsByType<Button>(FindObjectsSortMode.None);
        SerializedObject so = new SerializedObject(ui);
        SerializedProperty btnsArr = so.FindProperty("menuButtons");
        btnsArr.arraySize = allBtns.Length;
        for (int i = 0; i < allBtns.Length; i++)
            btnsArr.GetArrayElementAtIndex(i).objectReferenceValue = allBtns[i];
        so.ApplyModifiedProperties();

        // Apply background sprite to a MainMenu background Image (Main_Menu_Bk.png)
        Sprite menuBg = LoadSprite(PANEL_PATH + "/Main_Menu_Bk.png");
        if (menuBg != null)
        {
            // Look for an Image that acts as the full-screen background
            Image[] imgs = FindObjectsByType<Image>(FindObjectsSortMode.None);
            foreach (Image img in imgs)
            {
                string n = img.gameObject.name.ToLower();
                if (n.Contains("background") || n.Contains("bg") || n == "panel")
                {
                    img.sprite = menuBg;
                    img.type   = Image.Type.Sliced;
                    Debug.Log("[CustomPolishEditor] Main Menu background image applied to " + img.gameObject.name);
                    break;
                }
            }
        }

        Debug.Log("[CustomPolishEditor] MainMenu UI polished.");
    }

    // ─────────────────────────────────────────────────────────────────────
    //  Utility
    // ─────────────────────────────────────────────────────────────────────
    private static Sprite LoadSprite(string assetPath)
    {
        // Normalise slashes
        assetPath = assetPath.Replace("\\", "/");
        Sprite s = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
        if (s == null)
            Debug.LogWarning($"[CustomPolishEditor] Asset not found: {assetPath}");
        return s;
    }
}
#endif
