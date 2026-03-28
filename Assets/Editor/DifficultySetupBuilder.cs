#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Editor script that auto-generates the Difficulty Selection UI, deletes old Auth panels,
/// and wires the MainMenuController automatically so the user doesn't have to drag and drop everything.
/// </summary>
public class DifficultySetupBuilder
{
    [MenuItem("Tools/Setup Difficulty UI")]
    public static void BuildDifficultyUI()
    {
        MainMenuController menuController = Object.FindAnyObjectByType<MainMenuController>();
        if (menuController == null)
        {
            Debug.LogError("[Setup] Could not find MainMenuController. Please open the MainMenu scene first!");
            return;
        }

        // 1. Delete Old Backend Panels cleanly
        DestroyIfExists("LoginPanel");
        DestroyIfExists("LeaderboardPanel");
        DestroyIfExists("Canvas/LoginPanel");
        DestroyIfExists("Canvas/LeaderboardPanel");

        // 2. Find Canvas
        Canvas canvas = Object.FindAnyObjectByType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("[Setup] Could not find a Canvas in the scene!");
            return;
        }

        // 3. Prevent duplicates
        GameObject oldDiff = GameObject.Find("DifficultyPanel");
        if (oldDiff != null) Object.DestroyImmediate(oldDiff);

        // 4. Create DifficultyPanel Base
        GameObject diffPanel = new GameObject("DifficultyPanel");
        diffPanel.transform.SetParent(canvas.transform, false);
        RectTransform rt = diffPanel.AddComponent<RectTransform>();
        
        // Stretch to fill screen
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        // Dark semi-transparent background
        Image bg = diffPanel.AddComponent<Image>();
        bg.color = new Color(0.05f, 0.07f, 0.12f, 0.95f);

        // Vertical Layout Group for automatic centering
        VerticalLayoutGroup vlg = diffPanel.AddComponent<VerticalLayoutGroup>();
        vlg.childAlignment = TextAnchor.MiddleCenter;
        vlg.spacing = 30;
        vlg.childControlWidth = false;
        vlg.childControlHeight = false;
        vlg.childForceExpandWidth = false;
        vlg.childForceExpandHeight = false;

        // 5. Title Text
        GameObject titleObj = new GameObject("TitleText");
        titleObj.transform.SetParent(diffPanel.transform, false);
        TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
        titleText.text = "SELECT DIFFICULTY";
        titleText.fontSize = 80;
        titleText.alignment = TextAlignmentOptions.Center;
        titleText.color = new Color(0.00f, 1.00f, 0.85f, 1.00f); // Neon Cyan
        titleText.fontStyle = FontStyles.Bold;
        RectTransform titleRt = titleObj.GetComponent<RectTransform>();
        titleRt.sizeDelta = new Vector2(1000, 120);

        // Spacer to separate title from buttons
        GameObject spacer = new GameObject("Spacer");
        spacer.transform.SetParent(diffPanel.transform, false);
        RectTransform spacerRt = spacer.AddComponent<RectTransform>();
        spacerRt.sizeDelta = new Vector2(100, 50);

        // 6. Create Buttons
        Button btnEasy = CreateDiffButton("Btn_Easy", "EASY (-20% Speed, Wide Gaps)", new Color(0.0f, 0.9f, 0.4f), diffPanel.transform);
        Button btnMed = CreateDiffButton("Btn_Medium", "NORMAL (Standard)", new Color(1.0f, 0.8f, 0.0f), diffPanel.transform);
        Button btnHard = CreateDiffButton("Btn_Hard", "HARD (+30% Speed, Tight Gaps)", new Color(1.0f, 0.3f, 0.3f), diffPanel.transform);
        
        // Extra spacer before back button
        GameObject spacer2 = new GameObject("Spacer");
        spacer2.transform.SetParent(diffPanel.transform, false);
        spacer2.AddComponent<RectTransform>().sizeDelta = new Vector2(100, 30);

        Button btnBack = CreateDiffButton("Btn_Back", "BACK TO MENU", new Color(0.5f, 0.5f, 0.5f), diffPanel.transform);

        // 7. Auto-Wire the MainMenuController
        SerializedObject so = new SerializedObject(menuController);
        
        so.FindProperty("difficultyPanel").objectReferenceValue = diffPanel;
        so.FindProperty("btnEasy").objectReferenceValue = btnEasy;
        so.FindProperty("btnMedium").objectReferenceValue = btnMed;
        so.FindProperty("btnHard").objectReferenceValue = btnHard;
        so.FindProperty("closeDifficultyButton").objectReferenceValue = btnBack;
        
        // Find existing Play button to repath it to OpenDifficulty
        Button existingPlayBtn = FindButtonByName("PlayButton") ?? FindButtonByName("Btn_Play");
        if (existingPlayBtn != null)
        {
            so.FindProperty("selectDifficultyButton").objectReferenceValue = existingPlayBtn;
        }

        so.ApplyModifiedProperties();

        // Hide it so it doesn't overlap the main menu at start
        diffPanel.SetActive(false);

        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
        Debug.Log("[Setup] Difficulty UI successfully built and wired into MainMenuController!");
    }

    private static void DestroyIfExists(string name)
    {
        GameObject obj = GameObject.Find(name);
        if (obj != null)
        {
            Object.DestroyImmediate(obj);
            Debug.Log($"[Setup] Cleaned up legacy UI: {name}");
        }
    }

    private static Button FindButtonByName(string name)
    {
        GameObject obj = GameObject.Find(name);
        return obj != null ? obj.GetComponent<Button>() : null;
    }

    private static Button CreateDiffButton(string name, string text, Color color, Transform parent)
    {
        GameObject btnObj = new GameObject(name);
        btnObj.transform.SetParent(parent, false);
        RectTransform rt = btnObj.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(600, 90);

        // Semi-transparent dark button body
        Image img = btnObj.AddComponent<Image>();
        img.color = new Color(0.1f, 0.1f, 0.15f, 0.95f);

        Button btn = btnObj.AddComponent<Button>();
        ColorBlock cb = btn.colors;
        cb.normalColor = color;
        cb.highlightedColor = Color.white;
        cb.pressedColor = new Color(0.6f, 0.6f, 0.6f);
        cb.selectedColor = color;
        btn.colors = cb;

        GameObject txtObj = new GameObject("Text");
        txtObj.transform.SetParent(btnObj.transform, false);
        RectTransform txtRt = txtObj.AddComponent<RectTransform>();
        
        // Stretch text to fill button
        txtRt.anchorMin = Vector2.zero;
        txtRt.anchorMax = Vector2.one;
        txtRt.offsetMin = Vector2.zero;
        txtRt.offsetMax = Vector2.zero;

        TextMeshProUGUI tmp = txtObj.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = 32;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = new Color(0.95f, 0.95f, 0.95f, 1f);
        tmp.fontStyle = FontStyles.Bold;

        return btn;
    }
}
#endif
