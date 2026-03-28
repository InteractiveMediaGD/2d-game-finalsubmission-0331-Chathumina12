#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;

public class UIFixer : Editor
{
    private const string BARS_PATH   = "Assets/Prefabs/Game assets/Cyberpunk_UI_Asset_Pack_v1.1/03_Bars_and_Indicators/Progress_Bar";
    private const string BTN_PATH    = "Assets/Prefabs/Game assets/Cyberpunk_UI_Asset_Pack_v1.1/02_Interactive_Buttons/Primary_Button";
    private const string PANEL_PATH  = "Assets/Prefabs/Game assets/Cyberpunk_UI_Asset_Pack_v1.1/01_Panels_and_Windows";
    private const string BG_PATH     = "Assets/Prefabs/Game assets/PixelWhale_SF_Project/Background";

    [MenuItem("Game Polish/1. FIX Main Menu UI")]
    public static void FixMainMenu()
    {
        Sprite btnSprite = LoadAndFormatSprite(BTN_PATH + "/Btn_Primary_Normal.png");
        Sprite bgSprite = LoadAndFormatSprite(PANEL_PATH + "/Main_Menu_Bk.png");

        // 1. Fix Canvas Background
        Canvas canvas = Object.FindAnyObjectByType<Canvas>();
        if (canvas != null)
        {
            GameObject bgObj = GameObject.Find("DifficultyPanel");
            if (bgObj != null)
            {
                Image bgImage = bgObj.GetComponent<Image>();
                if (bgImage != null && bgSprite != null)
                {
                    bgImage.sprite = bgSprite;
                    bgImage.color = Color.white; // Reset tint so the texture shows fully
                    Debug.Log("[UIFixer] Background image applied to Main Menu.");
                }
            }
        }

        // 2. Fix All Buttons
        Button[] buttons = Object.FindObjectsByType<Button>(FindObjectsSortMode.None);
        int fixedCount = 0;
        foreach (var btn in buttons)
        {
            Image img = btn.GetComponent<Image>();
            if (img != null && btnSprite != null)
            {
                img.sprite = btnSprite;
                img.type = Image.Type.Sliced;
                img.color = Color.white;
            }

            // Fix the text color to be visible!
            TextMeshProUGUI tmp = btn.GetComponentInChildren<TextMeshProUGUI>();
            if (tmp != null)
            {
                // Dark neon teal/cyan color that pops on the bright white/cyan button backgrounds
                tmp.color = new Color(0.02f, 0.15f, 0.20f, 1f);
                tmp.fontStyle = FontStyles.Bold;
            }
            fixedCount++;
        }

        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
        Debug.Log($"[UIFixer] Fixed {fixedCount} buttons in the Main Menu.");
    }

    [MenuItem("Game Polish/2. FIX GameScene Background & Canvas")]
    public static void FixGameScene()
    {
        // 1. Force simple background and PERFECT loop mode
        // We'll use sf_space.png or bg02.png for a cleaner look.
        string targetBgPath = BG_PATH + "/sf_space.png";
        ForceTextureRepeat(targetBgPath);
        Sprite simpleBg = LoadAndFormatSprite(targetBgPath);

        BackgroundScroller[] scrollers = Object.FindObjectsByType<BackgroundScroller>(FindObjectsSortMode.None);
        foreach (var scroller in scrollers)
        {
            SpriteRenderer sr = scroller.GetComponent<SpriteRenderer>();
            if (sr != null && simpleBg != null)
            {
                sr.sprite = simpleBg;
                sr.drawMode = SpriteDrawMode.Tiled;
                sr.size = new Vector2(50f, 25f);
                sr.color = Color.white; // clear any tint

                // Remove second exact instances if they were created for transform wrap previously
                Transform duplicate = sr.transform.Find("TileB");
                if (duplicate != null) DestroyImmediate(duplicate.gameObject);
            }
        }
        Debug.Log("[UIFixer] Background replaced with seamlessly looping simple space texture.");

        // 2. Fix the blue UI Panel block on the right
        // Usually called "ScorePanel", "UIPanel", or "StatsPanel" inside the Canvas.
        Image[] images = Object.FindObjectsByType<Image>(FindObjectsSortMode.None);
        foreach (var img in images)
        {
            // If it's a massive background shape
            if (img.rectTransform.rect.height > 600 && img.rectTransform.anchorMin.x > 0.5f)
            {
                // Make it a sleek transparent Cyberpunk panel instead of opaque bright blue
                img.color = new Color(0.04f, 0.08f, 0.15f, 0.85f);
                
                Sprite outline = LoadAndFormatSprite(PANEL_PATH + "/Main_Menu_Bk.png");
                if (outline != null)
                {
                    img.sprite = outline;
                    img.type = Image.Type.Sliced;
                }
            }

            // Also fix the little fragments block at the bottom right
            if (img.gameObject.name.ToLower().Contains("fragment"))
            {
                img.color = new Color(0.02f, 0.05f, 0.10f, 0.95f);
            }
        }

        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
        Debug.Log("[UIFixer] GameScene UI Panels overhauled to sleek dark theme.");
    }

    // ─────────────────────────────────────────────────────────────────────
    //  UTILITIES
    // ─────────────────────────────────────────────────────────────────────
    private static void ForceTextureRepeat(string path)
    {
        TextureImporter ti = AssetImporter.GetAtPath(path) as TextureImporter;
        if (ti != null && ti.wrapMode != TextureWrapMode.Repeat)
        {
            ti.wrapMode = TextureWrapMode.Repeat;
            ti.SaveAndReimport();
        }
    }

    private static Sprite LoadAndFormatSprite(string path)
    {
        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }
}
#endif
