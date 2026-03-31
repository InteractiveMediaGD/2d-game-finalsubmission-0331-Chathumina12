#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using UnityEditor.Events;
using UnityEngine.Events;
using TMPro;

/// <summary>
/// Hardened automated Setup Script for the final UI fixes.
/// </summary>
public class UIFixesSetupBuilder : EditorWindow
{
    [MenuItem("Tools/UI Fixes/1. Build Pause Menu (Run in GameScene)")]
    public static void BuildPauseMenu()
    {
        // Require EventSystem to guarantee clicks work!
        if (FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            Debug.Log("Created EventSystem (was missing).");
        }

        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("Could not find a Canvas in the scene!");
            return;
        }

        GameObject pauseManagerObj = new GameObject("PauseMenuManager");
        PauseMenu pauseMenuComp = pauseManagerObj.AddComponent<PauseMenu>();

        GameObject panelObj = CreateUIObject("PauseMenuPanel", canvas.transform);
        Image panelImg = panelObj.AddComponent<Image>();
        panelImg.color = new Color(0.1f, 0.1f, 0.1f, 0.9f);
        StretchToFill(panelObj.GetComponent<RectTransform>());

        GameObject titleObj = CreateUIObject("TitleText", panelObj.transform);
        TextMeshProUGUI titleTmp = titleObj.AddComponent<TextMeshProUGUI>();
        titleTmp.text = "PAUSED";
        titleTmp.fontSize = 72;
        titleTmp.color = Color.white;
        titleTmp.alignment = TextAlignmentOptions.Center;
        RectTransform titleRect = titleObj.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0, 1);
        titleRect.anchorMax = new Vector2(1, 1);
        titleRect.pivot = new Vector2(0.5f, 1);
        titleRect.anchoredPosition = new Vector2(0, -100);
        titleRect.sizeDelta = new Vector2(0, 150);

        Slider musicSlider = CreateSlider("MusicSlider", panelObj.transform, new Vector2(0, 50), "Music Volume");
        Slider sfxSlider = CreateSlider("SFXSlider", panelObj.transform, new Vector2(0, -50), "SFX Volume");

        // Use custom styling similar to their screenshot
        Button btnResume = CreateButton("ResumeButton", "RESUME", panelObj.transform, new Vector2(0, -180), new Color(0.2f, 0.4f, 0.8f));
        Button btnRestart = CreateButton("RestartButton", "RESTART", panelObj.transform, new Vector2(0, -280), new Color(0.2f, 0.4f, 0.8f));
        Button btnMainMenu = CreateButton("MainMenuButton", "MAIN MENU", panelObj.transform, new Vector2(0, -380), new Color(0.8f, 0.2f, 0.2f));

        // Inject dependencies directly into the script instead of relying on UnityEventEditor tools
        SerializedObject soMenu = new SerializedObject(pauseMenuComp);
        soMenu.FindProperty("pauseMenuPanel").objectReferenceValue = panelObj;
        soMenu.FindProperty("musicSlider").objectReferenceValue = musicSlider;
        soMenu.FindProperty("sfxSlider").objectReferenceValue = sfxSlider;
        soMenu.FindProperty("resumeButton").objectReferenceValue = btnResume;
        soMenu.FindProperty("restartButton").objectReferenceValue = btnRestart;
        soMenu.FindProperty("mainMenuButton").objectReferenceValue = btnMainMenu;
        soMenu.ApplyModifiedProperties();

        panelObj.SetActive(false);
        Selection.activeGameObject = pauseManagerObj;
        
        Debug.Log("<color=green>Pause Menu successfully built and wired natively inside PauseMenu.cs!</color>");
    }

    [MenuItem("Tools/UI Fixes/2. Fix Game Over Restart Button (Run in GameScene)")]
    public static void FixRestartButton()
    {
        Button[] buttons = Resources.FindObjectsOfTypeAll<Button>();
        Button restartBtn = null;
        
        foreach (var b in buttons)
        {
            if ((b.gameObject.name.Contains("Restart") || b.gameObject.name.Contains("Retry")) && b.gameObject.scene.IsValid())
            {
                restartBtn = b;
                break;
            }
        }

        if (restartBtn == null)
        {
            Debug.LogError("Could not find a button named 'RestartButton' in the GameScene.");
            return;
        }

        PauseMenu pMenu = FindObjectOfType<PauseMenu>();
        GameManager gManager = FindObjectOfType<GameManager>();

        while (restartBtn.onClick.GetPersistentEventCount() > 0)
        {
            UnityEventTools.RemovePersistentListener(restartBtn.onClick, 0);
        }

        // We use string method names so it flawlessly bridges to the instance.
        if (pMenu != null)
        {
            UnityAction action = new UnityAction(pMenu.RestartGame);
            UnityEventTools.AddPersistentListener(restartBtn.onClick, action);
            Debug.Log($"<color=green>Restart Button fixed! Wired to PauseMenu.RestartGame()</color>");
        }
        else if (gManager != null)
        {
            UnityAction action = new UnityAction(gManager.RestartGame);
            UnityEventTools.AddPersistentListener(restartBtn.onClick, action);
            Debug.Log($"<color=green>Restart Button fixed! Wired to GameManager.RestartGame()</color>");
        }
        else
        {
            Debug.LogError("No PauseMenu or GameManager found to link Restart button to.");
            return;
        }

        EditorUtility.SetDirty(restartBtn);
    }

    [MenuItem("Tools/UI Fixes/3. Change Leaderboard to Quit (Run in MainMenu Scene)")]
    public static void ChangeLeaderboardToQuit()
    {
        MainMenuController menuController = FindObjectOfType<MainMenuController>();
        if (menuController == null)
        {
            Debug.LogError("Could not find MainMenuController. Ensure you are in the MainMenu scene.");
            return;
        }

        // Specifically find MainMenuContainer or fallback to finding by name in all UI
        GameObject container = GameObject.Find("MainMenuContainer");
        if (container == null) // fallback if name differs
        {
            container = GameObject.Find("MainMenuPanel");
        }

        if (container == null)
        {
            Debug.LogError("Could not find 'MainMenuContainer' in the Hierarchy. Please select the Leaderboard button manually and change it, or name the parent container 'MainMenuContainer'.");
            return;
        }

        Button[] buttons = container.GetComponentsInChildren<Button>(true);
        Button lbBtn = null;
        
        foreach (var b in buttons)
        {
            string btnName = b.gameObject.name.ToLower();
            string txt = b.GetComponentInChildren<TextMeshProUGUI>()?.text.ToLower() ?? "";
            
            if (btnName.Contains("leaderboard") || txt.Contains("leaderboard"))
            {
                lbBtn = b;
                break;
            }
        }

        if (lbBtn == null)
        {
            Debug.LogError("Could not find a Leaderboard Button inside the MainMenuContainer! Wait, let's search globally...");
            // Global fallback
            foreach(var b in Resources.FindObjectsOfTypeAll<Button>())
            {
                if ((b.gameObject.name.ToLower().Contains("leaderboard") || b.GetComponentInChildren<TextMeshProUGUI>()?.text.ToLower().Contains("leaderboard") == true) && b.gameObject.scene.IsValid())
                {
                    lbBtn = b;
                    break;
                }
            }
            if(lbBtn == null) return;
        }

        // Change Text & Name
        lbBtn.gameObject.name = "QuitButton";
        TextMeshProUGUI textComp = lbBtn.GetComponentInChildren<TextMeshProUGUI>();
        if (textComp != null) textComp.text = "QUIT GAME";

        // Remove old scripts that might block it
        LeaderboardUI lbUI = lbBtn.GetComponent<LeaderboardUI>();
        if (lbUI != null) DestroyImmediate(lbUI);

        // Wipe existing listeners
        while (lbBtn.onClick.GetPersistentEventCount() > 0)
        {
            UnityEventTools.RemovePersistentListener(lbBtn.onClick, 0);
        }

        // Hard un-bind to QuitGame
        UnityAction action = new UnityAction(menuController.QuitGame);
        UnityEventTools.AddPersistentListener(lbBtn.onClick, action);

        EditorUtility.SetDirty(lbBtn);
        Debug.Log("<color=green>Found Leaderboard button in MainMenuContainer, changed to QUIT GAME, and wired correctly!</color>");
    }

    // --- Helper UI Generation Methods ---
    
    private static GameObject CreateUIObject(string name, Transform parent)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        obj.AddComponent<RectTransform>();
        return obj;
    }

    private static void StretchToFill(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    private static Button CreateButton(string name, string text, Transform parent, Vector2 pos, Color c)
    {
        GameObject btnObj = CreateUIObject(name, parent);
        Image btnImg = btnObj.AddComponent<Image>();
        btnImg.color = c;
        Button btn = btnObj.AddComponent<Button>();
        
        RectTransform rect = btnObj.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(300, 80);
        rect.anchoredPosition = pos;

        GameObject textObj = CreateUIObject("Text", btnObj.transform);
        TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = 36;
        tmp.color = Color.white;
        tmp.alignment = TextAlignmentOptions.Center;
        StretchToFill(textObj.GetComponent<RectTransform>());

        return btn;
    }

    private static Slider CreateSlider(string name, Transform parent, Vector2 pos, string labelText)
    {
        GameObject sliderObj = CreateUIObject(name, parent);
        RectTransform rect = sliderObj.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(400, 40);
        rect.anchoredPosition = pos;

        Slider slider = sliderObj.AddComponent<Slider>();
        
        GameObject bgObj = CreateUIObject("Background", sliderObj.transform);
        Image bgImg = bgObj.AddComponent<Image>();
        bgImg.color = Color.black;
        StretchToFill(bgObj.GetComponent<RectTransform>());

        GameObject fillAreaObj = CreateUIObject("Fill Area", sliderObj.transform);
        RectTransform fillAreaRect = fillAreaObj.GetComponent<RectTransform>();
        StretchToFill(fillAreaRect);
        fillAreaRect.offsetMin = new Vector2(5, 0);
        fillAreaRect.offsetMax = new Vector2(-15, 0);

        GameObject fillObj = CreateUIObject("Fill", fillAreaObj.transform);
        Image fillImg = fillObj.AddComponent<Image>();
        fillImg.color = Color.green;
        RectTransform fillRect = fillObj.GetComponent<RectTransform>();
        StretchToFill(fillRect);
        fillRect.offsetMax = Vector2.zero;

        GameObject handleAreaObj = CreateUIObject("Handle Slide Area", sliderObj.transform);
        RectTransform handleAreaRect = handleAreaObj.GetComponent<RectTransform>();
        StretchToFill(handleAreaRect);
        handleAreaRect.offsetMin = new Vector2(10, 0);
        handleAreaRect.offsetMax = new Vector2(-10, 0);

        GameObject handleObj = CreateUIObject("Handle", handleAreaObj.transform);
        Image handleImg = handleObj.AddComponent<Image>();
        handleImg.color = Color.white;
        RectTransform handleRect = handleObj.GetComponent<RectTransform>();
        handleRect.sizeDelta = new Vector2(40, 0);

        slider.fillRect = fillRect;
        slider.handleRect = handleRect;
        slider.targetGraphic = handleImg;
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.value = 0.5f;

        GameObject labelObj = CreateUIObject("Label", sliderObj.transform);
        TextMeshProUGUI tmp = labelObj.AddComponent<TextMeshProUGUI>();
        tmp.text = labelText;
        tmp.fontSize = 24;
        tmp.color = Color.white;
        RectTransform labelRect = labelObj.GetComponent<RectTransform>();
        labelRect.anchorMin = new Vector2(0, 1);
        labelRect.anchorMax = new Vector2(1, 1);
        labelRect.sizeDelta = new Vector2(0, 40);
        labelRect.anchoredPosition = new Vector2(0, 30);

        return slider;
    }
}
#endif
