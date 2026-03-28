#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class MissionSystemSetupBuilder : EditorWindow
{
    [MenuItem("Tools/Setup Dynamic Mission System")]
    public static void BuildMissionSystem()
    {
        // 1. Setup Mission Manager
        MissionManager manager = FindObjectOfType<MissionManager>();
        if (manager == null)
        {
            GameObject managerObj = new GameObject("MissionManager");
            manager = managerObj.AddComponent<MissionManager>();
        }

        manager.possibleMissions = new List<MissionData>
        {
            new MissionData { type = MissionType.SurviveTime, description = "Survive for 30 seconds", targetAmount = 30, rewardFragments = 25 },
            new MissionData { type = MissionType.SurviveTime, description = "Survive for 60 seconds", targetAmount = 60, rewardFragments = 75 },
            new MissionData { type = MissionType.DefeatEnemies, description = "Destroy 5 Antiviruses", targetAmount = 5, rewardFragments = 30 },
            new MissionData { type = MissionType.DefeatEnemies, description = "Destroy 12 Antiviruses", targetAmount = 12, rewardFragments = 80 },
            new MissionData { type = MissionType.GrazeWalls, description = "Perform 3 Close Calls", targetAmount = 3, rewardFragments = 45 },
            new MissionData { type = MissionType.PassBarriers, description = "Pass 10 Firewalls", targetAmount = 10, rewardFragments = 40 },
            new MissionData { type = MissionType.CollectFragments, description = "Collect 50 Fragments", targetAmount = 50, rewardFragments = 50 }
        };

        // 2. Setup Canvas Overlay
        GameObject canvasObj = new GameObject("MissionOverlay_Canvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 50; // Render on top of normal UI
        canvasObj.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasObj.AddComponent<GraphicRaycaster>();

        MissionUIManager uiManager = canvasObj.AddComponent<MissionUIManager>();
        uiManager.missionCards = new List<MissionUI>();

        // Layout Group
        GameObject layoutObj = new GameObject("MissionContainer");
        layoutObj.transform.SetParent(canvasObj.transform, false);
        RectTransform layoutRect = layoutObj.AddComponent<RectTransform>();
        layoutRect.anchorMin = new Vector2(0, 0.5f); // Mid-Left
        layoutRect.anchorMax = new Vector2(0, 0.5f);
        layoutRect.pivot = new Vector2(0, 0.5f);
        layoutRect.anchoredPosition = new Vector2(10, 0); // Centered vertically on the left edge
        layoutRect.sizeDelta = new Vector2(200, 200);

        VerticalLayoutGroup vlg = layoutObj.AddComponent<VerticalLayoutGroup>();
        vlg.childAlignment = TextAnchor.UpperLeft;
        vlg.childControlHeight = false;
        vlg.childControlWidth = false;
        vlg.childForceExpandHeight = false;
        vlg.childForceExpandWidth = false;
        vlg.spacing = 5;

        // 3. Create 3 Mission Cards
        for (int i = 0; i < 3; i++)
        {
            GameObject cardObj = CreateMissionCard(i.ToString(), layoutObj.transform);
            uiManager.missionCards.Add(cardObj.GetComponent<MissionUI>());
            cardObj.SetActive(false); // They get activated at runtime
        }

        // 4. Generate Collected Data Fragments UI
        GameObject fragContainer = new GameObject("FragmentsCounter");
        fragContainer.transform.SetParent(canvasObj.transform, false);
        RectTransform fragRect = fragContainer.AddComponent<RectTransform>();
        fragRect.anchorMin = new Vector2(1, 0); // Bottom-Right
        fragRect.anchorMax = new Vector2(1, 0);
        fragRect.pivot = new Vector2(1, 0);
        fragRect.anchoredPosition = new Vector2(-30, 30); // Safe area offset from corner
        fragRect.sizeDelta = new Vector2(220, 40);

        Image fragBg = fragContainer.AddComponent<Image>();
        fragBg.color = new Color(0, 0, 0, 0.6f);

        GameObject fragTextObj = new GameObject("Text");
        fragTextObj.transform.SetParent(fragContainer.transform, false);
        RectTransform ftRect = fragTextObj.AddComponent<RectTransform>();
        StretchRect(ftRect);
        
        TextMeshProUGUI fragTmp = fragTextObj.AddComponent<TextMeshProUGUI>();
        fragTmp.text = "Fragments: 0";
        fragTmp.fontSize = 20;
        fragTmp.color = Color.cyan;
        fragTmp.alignment = TextAlignmentOptions.Center;

        FragmentUIController fragUi = fragContainer.AddComponent<FragmentUIController>();
        SerializedObject soFrag = new SerializedObject(fragUi);
        soFrag.FindProperty("fragmentText").objectReferenceValue = fragTmp;
        soFrag.ApplyModifiedProperties();

        Selection.activeGameObject = manager.gameObject;
        Debug.Log("[Mission System Setup] Successfully generated MissionManager with default missions, animated UI Overlay, and auto-updating Fragments Tracker!");
    }

    private static GameObject CreateMissionCard(string id, Transform parent)
    {
        // Main Card
        GameObject cardObj = new GameObject("MissionCard_" + id);
        cardObj.transform.SetParent(parent, false);
        RectTransform cardRect = cardObj.AddComponent<RectTransform>();
        cardRect.sizeDelta = new Vector2(190, 35); // Compact card
        
        Image bgImg = cardObj.AddComponent<Image>();
        bgImg.color = new Color(0.1f, 0.1f, 0.15f, 0.85f); // Dark tint

        // Outline
        Outline outline = cardObj.AddComponent<Outline>();
        outline.effectColor = new Color(0, 0.6f, 0.3f, 1f); // Thin green border
        outline.effectDistance = new Vector2(2, -2);

        MissionUI missionUI = cardObj.AddComponent<MissionUI>();
        missionUI.backgroundImage = bgImg;

        // Description Text
        GameObject descObj = new GameObject("Description");
        descObj.transform.SetParent(cardObj.transform, false);
        RectTransform descRect = descObj.AddComponent<RectTransform>();
        descRect.anchorMin = new Vector2(0, 0.5f);
        descRect.anchorMax = new Vector2(1, 1);
        descRect.offsetMin = new Vector2(10, 0);
        descRect.offsetMax = new Vector2(-10, -2);
        
        TextMeshProUGUI descTmp = descObj.AddComponent<TextMeshProUGUI>();
        descTmp.text = "Mission Description Goes Here";
        descTmp.fontSize = 10; // Compact font
        descTmp.color = Color.white;
        descTmp.alignment = TextAlignmentOptions.BottomLeft;
        missionUI.descriptionText = descTmp;

        // Progress Text
        GameObject progTextObj = new GameObject("ProgressText");
        progTextObj.transform.SetParent(cardObj.transform, false);
        RectTransform progTextRect = progTextObj.AddComponent<RectTransform>();
        progTextRect.anchorMin = new Vector2(0, 0);
        progTextRect.anchorMax = new Vector2(1, 0.5f);
        progTextRect.offsetMin = new Vector2(10, 3);
        progTextRect.offsetMax = new Vector2(-10, 0);
        
        TextMeshProUGUI progTmp = progTextObj.AddComponent<TextMeshProUGUI>();
        progTmp.text = "0 / 10";
        progTmp.fontSize = 9; // Compact font
        progTmp.color = Color.gray;
        progTmp.alignment = TextAlignmentOptions.TopRight;
        missionUI.progressText = progTmp;

        // Progress Slider
        GameObject sliderObj = new GameObject("ProgressBar");
        sliderObj.transform.SetParent(cardObj.transform, false);
        RectTransform sliderRect = sliderObj.AddComponent<RectTransform>();
        sliderRect.anchorMin = new Vector2(0, 0);
        sliderRect.anchorMax = new Vector2(1, 0.5f);
        sliderRect.offsetMin = new Vector2(10, 2);
        sliderRect.offsetMax = new Vector2(-60, -2); // Leave room for numbers

        Slider sliderComp = sliderObj.AddComponent<Slider>();
        missionUI.progressBar = sliderComp;

        // Background of slider
        GameObject sliderBgObj = new GameObject("Background");
        sliderBgObj.transform.SetParent(sliderObj.transform, false);
        Image sliderBgImg = sliderBgObj.AddComponent<Image>();
        sliderBgImg.color = new Color(0, 0, 0, 0.6f);
        StretchRect(sliderBgObj.GetComponent<RectTransform>());

        // Fill Area
        GameObject fillAreaObj = new GameObject("Fill Area");
        fillAreaObj.transform.SetParent(sliderObj.transform, false);
        RectTransform fillAreaRect = fillAreaObj.AddComponent<RectTransform>();
        StretchRect(fillAreaRect);
        
        GameObject fillObj = new GameObject("Fill");
        fillObj.transform.SetParent(fillAreaObj.transform, false);
        Image fillImg = fillObj.AddComponent<Image>();
        fillImg.color = new Color(0f, 0.8f, 0.2f); // Neon Green
        StretchRect(fillObj.GetComponent<RectTransform>());

        sliderComp.fillRect = fillObj.GetComponent<RectTransform>();

        return cardObj;
    }

    private static void StretchRect(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }
}
#endif
