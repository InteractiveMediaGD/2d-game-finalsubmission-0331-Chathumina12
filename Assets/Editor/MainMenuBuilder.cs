#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

public class MainMenuBuilder : EditorWindow
{
    [MenuItem("Tools/Build Main Menu UI")]
    public static void BuildUI()
    {
        // 1. Create Canvas
        GameObject canvasObj = new GameObject("MainMenuCanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        
        canvasObj.AddComponent<GraphicRaycaster>();

        // 2. Create MainMenuContainer
        GameObject mainMenuContainer = CreateUIObject("MainMenuContainer", canvasObj.transform);
        StretchToFill(mainMenuContainer.GetComponent<RectTransform>());

        // HighScore Text
        GameObject highScoreObj = CreateText("HighScoreText", mainMenuContainer.transform, "High Score: 0");
        RectTransform hsRect = highScoreObj.GetComponent<RectTransform>();
        hsRect.anchorMin = new Vector2(0.5f, 1f);
        hsRect.anchorMax = new Vector2(0.5f, 1f);
        hsRect.pivot = new Vector2(0.5f, 1f);
        hsRect.anchoredPosition = new Vector2(0, -50);

        // Main Menu Buttons
        GameObject playButton = CreateButton("PlayButton", mainMenuContainer.transform, "Play", new Vector2(0, 50));
        GameObject storeButton = CreateButton("StoreButton", mainMenuContainer.transform, "Store", new Vector2(0, -50));
        GameObject leaderboardButton = CreateButton("LeaderboardButton", mainMenuContainer.transform, "Leaderboard", new Vector2(0, -150));

        // 3. Create StoreContainer
        GameObject storeContainer = CreateUIObject("StoreContainer", canvasObj.transform);
        StretchToFill(storeContainer.GetComponent<RectTransform>());
        AddBackground(storeContainer);
        GameObject storeBackButton = CreateButton("BackButton", storeContainer.transform, "Back", new Vector2(0, -400));
        
        // Add ShopController to StoreContainer
        storeContainer.AddComponent<ShopController>();

        // 4. Create LeaderboardContainer
        GameObject leaderboardContainer = CreateUIObject("LeaderboardContainer", canvasObj.transform);
        StretchToFill(leaderboardContainer.GetComponent<RectTransform>());
        AddBackground(leaderboardContainer);
        GameObject leaderboardBackButton = CreateButton("BackButton", leaderboardContainer.transform, "Back", new Vector2(0, -400));

        // 5. Create MenuController
        GameObject menuControllerObj = new GameObject("MenuController");
        menuControllerObj.transform.SetParent(canvasObj.transform);
        MainMenuController controller = menuControllerObj.AddComponent<MainMenuController>();

        // Disable sub-menus initially
        storeContainer.SetActive(false);
        leaderboardContainer.SetActive(false);

        // 6. Link Everything in the Controller
        SerializedObject so = new SerializedObject(controller);
        
        so.FindProperty("mainMenuPanel").objectReferenceValue = mainMenuContainer;
        so.FindProperty("storePanel").objectReferenceValue = storeContainer;
        so.FindProperty("leaderboardPanel").objectReferenceValue = leaderboardContainer;
        
        so.FindProperty("highScoreText").objectReferenceValue = highScoreObj.GetComponent<TextMeshProUGUI>();
        
        so.FindProperty("playButton").objectReferenceValue = playButton.GetComponent<Button>();
        so.FindProperty("storeButton").objectReferenceValue = storeButton.GetComponent<Button>();
        so.FindProperty("leaderboardButton").objectReferenceValue = leaderboardButton.GetComponent<Button>();
        
        so.FindProperty("closeStoreButton").objectReferenceValue = storeBackButton.GetComponent<Button>();
        so.FindProperty("closeLeaderboardButton").objectReferenceValue = leaderboardBackButton.GetComponent<Button>();

        so.ApplyModifiedProperties();

        // Ensure EventSystem exists
        if (Object.FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject esObj = new GameObject("EventSystem");
            esObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
            esObj.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }

        Selection.activeGameObject = canvasObj;
        Debug.Log("Main Menu UI successfully generated!");
    }

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

    private static void AddBackground(GameObject obj)
    {
        Image img = obj.AddComponent<Image>();
        img.color = new Color(0, 0, 0, 0.9f); // Dark opaque background
    }

    private static GameObject CreateText(string name, Transform parent, string text)
    {
        GameObject obj = CreateUIObject(name, parent);
        TextMeshProUGUI tmp = obj.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = 48;
        tmp.alignment = TextAlignmentOptions.Center;
        
        RectTransform rect = obj.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(400, 100);
        
        return obj;
    }

    private static GameObject CreateButton(string name, Transform parent, string text, Vector2 anchoredPosition)
    {
        GameObject buttonObj = CreateUIObject(name, parent);
        Image img = buttonObj.AddComponent<Image>();
        img.color = Color.white;
        Button btn = buttonObj.AddComponent<Button>();
        
        RectTransform rect = buttonObj.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(300, 80);
        rect.anchoredPosition = anchoredPosition;

        GameObject textObj = CreateUIObject("Text", buttonObj.transform);
        TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = 36;
        tmp.color = Color.black;
        tmp.alignment = TextAlignmentOptions.Center;
        StretchToFill(textObj.GetComponent<RectTransform>());

        return buttonObj;
    }
}
#endif
