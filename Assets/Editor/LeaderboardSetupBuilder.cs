#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

public class LeaderboardSetupBuilder : EditorWindow
{
    [MenuItem("Tools/Setup Leaderboard UI Elements")]
    public static void BuildUI()
    {
        // 1. Search the entire scene for LeaderboardContainer (even if Canvas is hidden)
        Transform containerTransform = null;
        Canvas[] allCanvases = Resources.FindObjectsOfTypeAll<Canvas>();
        
        foreach (Canvas c in allCanvases)
        {
            // Skip prefab asset files, only look in the open scene
            if (EditorUtility.IsPersistent(c.transform.root.gameObject)) continue;

            foreach (Transform t in c.GetComponentsInChildren<Transform>(true))
            {
                if (t.name == "LeaderboardContainer")
                {
                    containerTransform = t;
                    break;
                }
            }
            if (containerTransform != null) break;
        }

        if (containerTransform == null)
        {
            Debug.LogError("Could not find 'LeaderboardContainer'. Make sure you ran the Main Menu builder first.");
            return;
        }

        GameObject lbContainer = containerTransform.gameObject;

        // Strip existing children EXCEPT the BackButton
        for (int i = lbContainer.transform.childCount - 1; i >= 0; i--)
        {
            Transform child = lbContainer.transform.GetChild(i);
            if (child.name != "BackButton")
                DestroyImmediate(child.gameObject);
        }

        // Add Logic script if missing
        LeaderboardUI lbUI = lbContainer.GetComponent<LeaderboardUI>();
        if (lbUI == null) lbUI = lbContainer.AddComponent<LeaderboardUI>();

        // 2. Title Text
        GameObject titleObj = CreateUIObject("Title", lbContainer.transform);
        TextMeshProUGUI titleTmp = titleObj.AddComponent<TextMeshProUGUI>();
        titleTmp.text = "GLOBAL LEADERBOARD";
        titleTmp.fontSize = 64;
        titleTmp.color = Color.white;
        titleTmp.alignment = TextAlignmentOptions.Center;
        
        RectTransform titleRect = titleObj.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0, 1);
        titleRect.anchorMax = new Vector2(1, 1);
        titleRect.pivot = new Vector2(0.5f, 1);
        titleRect.anchoredPosition = new Vector2(0, -80);
        titleRect.sizeDelta = new Vector2(0, 100);

        // 3. Status Text (Loading/Error indicators)
        GameObject statusObj = CreateUIObject("StatusText", lbContainer.transform);
        TextMeshProUGUI statusTmp = statusObj.AddComponent<TextMeshProUGUI>();
        statusTmp.text = "Connecting to database...";
        statusTmp.fontSize = 28;
        statusTmp.color = new Color(0.7f, 0.7f, 0.7f); // Light gray
        statusTmp.alignment = TextAlignmentOptions.Center;
        
        RectTransform statusRect = statusObj.GetComponent<RectTransform>();
        statusRect.anchorMin = new Vector2(0, 0.5f);
        statusRect.anchorMax = new Vector2(1, 0.5f);
        statusRect.anchoredPosition = new Vector2(0, 0);

        // 4. Score Scroll View Layout
        GameObject scrollObj = CreateUIObject("ScrollList", lbContainer.transform);
        RectTransform scrollRect = scrollObj.GetComponent<RectTransform>();
        scrollRect.anchorMin = new Vector2(0.1f, 0.2f);
        scrollRect.anchorMax = new Vector2(0.9f, 0.8f);
        scrollRect.offsetMin = Vector2.zero;
        scrollRect.offsetMax = Vector2.zero;
        Image scrollImg = scrollObj.AddComponent<Image>();
        scrollImg.color = new Color(0, 0, 0, 0.5f); // Transparent black box
        
        ScrollRect scrollComp = scrollObj.AddComponent<ScrollRect>();
        scrollComp.horizontal = false; // Only vertical scrolling

        // Viewport
        GameObject viewportObj = CreateUIObject("Viewport", scrollObj.transform);
        RectTransform viewRect = viewportObj.GetComponent<RectTransform>();
        StretchToFill(viewRect);
        Image viewImg = viewportObj.AddComponent<Image>();
        Mask mask = viewportObj.AddComponent<Mask>();
        mask.showMaskGraphic = false;

        // Content (Vertical Layout Wrapper)
        GameObject contentObj = CreateUIObject("Content", viewportObj.transform);
        RectTransform contentRect = contentObj.GetComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0, 1);
        contentRect.anchorMax = new Vector2(1, 1);
        contentRect.pivot = new Vector2(0.5f, 1);
        contentRect.sizeDelta = new Vector2(0, 500); // Will auto expand

        VerticalLayoutGroup vlg = contentObj.AddComponent<VerticalLayoutGroup>();
        vlg.childForceExpandHeight = false;
        vlg.childForceExpandWidth = true;
        vlg.childControlHeight = true;
        vlg.childControlWidth = true;
        vlg.spacing = 15;
        vlg.padding = new RectOffset(20, 20, 20, 20); // Inner spacing

        ContentSizeFitter csf = contentObj.AddComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        scrollComp.viewport = viewRect;
        scrollComp.content = contentRect;

        // 5. Template Score Entry
        GameObject templateObj = CreateUIObject("EntryTemplate", contentObj.transform);
        TextMeshProUGUI templateTmp = templateObj.AddComponent<TextMeshProUGUI>();
        templateTmp.text = "1. Hacker_9021 <color=yellow>[Neon]</color> - <color=green>800</color>";
        templateTmp.fontSize = 36;
        templateTmp.color = Color.white;
        templateTmp.alignment = TextAlignmentOptions.Left;
        
        // Hide it so the template isn't visible, only clones created by the script will be active
        templateObj.SetActive(false);

        // 6. Hook up to the component
        SerializedObject so = new SerializedObject(lbUI);
        so.FindProperty("entryContainer").objectReferenceValue = contentObj.transform;
        
        // Using a disabled scene GameObject as a reference prefab is a valid Unity trick for dynamic UI
        so.FindProperty("entryPrefab").objectReferenceValue = templateObj;
        so.FindProperty("statusText").objectReferenceValue = statusObj.GetComponent<TextMeshProUGUI>();
        so.ApplyModifiedProperties();

        // 7. Make sure the Back Button is on top so it can be clicked
        Transform backBtn = lbContainer.transform.Find("BackButton");
        if (backBtn != null) backBtn.SetAsLastSibling();

        Selection.activeGameObject = lbContainer;
        Debug.Log("[Leaderboard Setup] Successfully generated dynamic Leaderboard scrolling UI!");
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
}
#endif
