#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;

/// <summary>
/// Final bulletproof sweep to ensure the game is bright, perfectly seamless,
/// and the buttons have explicit visual contrast.
/// </summary>
public class FinalUIFixer : Editor
{
    private const string SKY_STONE_PATH = "Assets/Prefabs/Game assets/PixelWhale_SF_Project/Background/sky_stone.png";
    private const string UI_BUTTON_PATH = "Assets/Prefabs/Game assets/PixelWhale_SF_Project/Ui/Ui_01.png"; // Bright pixel button

    [MenuItem("Game Polish/ULTIMATE FIX: Main Menu UI & Background")]
    public static void FixMainMenuFinal()
    {
        // 1. Enforce perfect looping metadata on the background sprite
        ForcePerfectLoopingSettings(SKY_STONE_PATH);
        Texture2D skyTex = AssetDatabase.LoadAssetAtPath<Texture2D>(SKY_STONE_PATH);
        Sprite btnSprite = AssetDatabase.LoadAssetAtPath<Sprite>(UI_BUTTON_PATH);

        Canvas canvas = Object.FindAnyObjectByType<Canvas>();
        if (canvas != null && skyTex != null)
        {
            // Destroy the old buggy difficulty panel background image component
            // We want the background explicitly linked to the Canvas root.
            GameObject diffPanel = GameObject.Find("DifficultyPanel");
            if (diffPanel != null)
            {
                Image bg = diffPanel.GetComponent<Image>();
                if (bg != null) DestroyImmediate(bg); // remove the dark tint
            }

            // Create a proper RawImage background behind everything
            GameObject bgObj = GameObject.Find("Animated_Space_BG");
            if (bgObj == null)
            {
                bgObj = new GameObject("Animated_Space_BG");
                bgObj.transform.SetParent(canvas.transform, false);
                bgObj.transform.SetSiblingIndex(0); // Send to back!
                
                RawImage raw = bgObj.AddComponent<RawImage>();
                raw.texture = skyTex;
                raw.uvRect = new Rect(0, 0, 1, 1);
                
                // Stretch to fill
                RectTransform rt = bgObj.GetComponent<RectTransform>();
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.one;
                rt.offsetMin = Vector2.zero;
                rt.offsetMax = Vector2.zero;

                RawImageScroller scroller = bgObj.AddComponent<RawImageScroller>();
                scroller.scrollSpeedX = 0.03f;
            }
        }

        // 2. Fix Buttons unconditionally
        Button[] buttons = Object.FindObjectsByType<Button>(FindObjectsSortMode.None);
        foreach (var btn in buttons)
        {
            Image img = btn.GetComponent<Image>();
            if (img != null)
            {
                if (btnSprite != null)
                {
                    img.sprite = btnSprite;
                    img.type = Image.Type.Sliced;
                }
                // Force an opaque, vibrant cyan tint if sprite mapping fails
                img.color = new Color(0.2f, 0.85f, 0.95f, 1f); 
            }

            // Force text to be pure black geometry so it POPS against the bright cyan button
            TextMeshProUGUI tmp = btn.GetComponentInChildren<TextMeshProUGUI>();
            if (tmp != null)
            {
                tmp.color = Color.black;
                tmp.fontStyle = FontStyles.Bold;
                
                // Add a slight white outline so it looks awesome
                tmp.outlineColor = Color.white;
                tmp.outlineWidth = 0.1f;
            }
        }

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Debug.Log("[FinalUIFixer] Main Menu Background and Buttons flawlessly secured.");
    }

    [MenuItem("Game Polish/ULTIMATE FIX: Game Scene Background & Darkness")]
    public static void FixGameSceneFinal()
    {
        ForcePerfectLoopingSettings(SKY_STONE_PATH);
        Sprite skySprite = AssetDatabase.LoadAssetAtPath<Sprite>(SKY_STONE_PATH);

        // 1. Force Global Light to be Bright White
        UnityEngine.Rendering.Universal.Light2D[] lights = Object.FindObjectsByType<UnityEngine.Rendering.Universal.Light2D>(FindObjectsSortMode.None);
        foreach (var l in lights)
        {
            if (l.lightType == UnityEngine.Rendering.Universal.Light2D.LightType.Global)
            {
                l.color = Color.white;
                l.intensity = 1.0f;
            }
        }

        // 2. Fix the Background Scrollers
        BackgroundScroller[] scrollers = Object.FindObjectsByType<BackgroundScroller>(FindObjectsSortMode.None);
        foreach (var scroller in scrollers)
        {
            SpriteRenderer sr = scroller.GetComponent<SpriteRenderer>();
            if (sr != null && skySprite != null)
            {
                sr.sprite = skySprite;
                sr.drawMode = SpriteDrawMode.Tiled;
                // Force it to be MASSIVELY wide so the camera NEVER sees the edge
                sr.size = new Vector2(250f, 25f); 
                sr.color = Color.white;
            }
        }

        // 3. Optional: Add a RawImage background directly to the Game Camera Canvas as a fallback
        // in case the SpriteRenderers are acting up on your specific Unity version.
        Canvas gameCanvas = Object.FindAnyObjectByType<Canvas>();
        if (gameCanvas != null && AssetDatabase.LoadAssetAtPath<Texture2D>(SKY_STONE_PATH) != null)
        {
            GameObject bgObj = GameObject.Find("Canvas_Space_BG");
            if (bgObj == null && gameCanvas.renderMode == RenderMode.ScreenSpaceCamera)
            {
                bgObj = new GameObject("Canvas_Space_BG");
                bgObj.transform.SetParent(gameCanvas.transform, false);
                bgObj.transform.SetSiblingIndex(0); // Send to very back of UI

                RawImage raw = bgObj.AddComponent<RawImage>();
                raw.texture = AssetDatabase.LoadAssetAtPath<Texture2D>(SKY_STONE_PATH);
                raw.color = new Color(0.6f, 0.6f, 0.8f, 1f); // slightly dimmed so UI pops
                
                RectTransform rt = bgObj.GetComponent<RectTransform>();
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.one;
                rt.offsetMin = Vector2.zero;
                rt.offsetMax = Vector2.zero;

                // Add scroller
                RawImageScroller scroll = bgObj.AddComponent<RawImageScroller>();
                scroll.scrollSpeedX = 0.02f;
            }
        }

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Debug.Log("[FinalUIFixer] Game Scene brightened and background looping permanently solved.");
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Utility
    // ─────────────────────────────────────────────────────────────────────────
    private static void ForcePerfectLoopingSettings(string path)
    {
        TextureImporter ti = AssetImporter.GetAtPath(path) as TextureImporter;
        if (ti != null)
        {
            bool modified = false;

            if (ti.wrapMode != TextureWrapMode.Repeat)
            {
                ti.wrapMode = TextureWrapMode.Repeat;
                modified = true;
            }

            // Correctly access spriteMeshType through TextureImporterSettings
            TextureImporterSettings settings = new TextureImporterSettings();
            ti.ReadTextureSettings(settings);

            if (settings.spriteMeshType != SpriteMeshType.FullRect)
            {
                settings.spriteMeshType = SpriteMeshType.FullRect;
                ti.SetTextureSettings(settings);
                modified = true;
            }

            if (modified)
            {
                ti.SaveAndReimport();
                Debug.Log($"[FinalUIFixer] Force-updated {path} to WrapMode:Repeat and MeshType:FullRect.");
            }
        }
    }
}
#endif
