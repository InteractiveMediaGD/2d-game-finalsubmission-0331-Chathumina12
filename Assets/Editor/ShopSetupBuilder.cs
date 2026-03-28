#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

public class ShopSetupBuilder : EditorWindow
{
    [MenuItem("Tools/Build Modular Shop UI")]
    public static void BuildShopUI()
    {
        // 1. Create the Prefab Folder if it doesn't exist
        string prefabFolderPath = "Assets/Prefabs";
        if (!AssetDatabase.IsValidFolder(prefabFolderPath))
        {
            AssetDatabase.CreateFolder("Assets", "Prefabs");
        }

        string prefabPath = prefabFolderPath + "/ShopItemCard.prefab";

        // 2. Build the Shop Item Card (Temporary Object)
        GameObject tempCard = CreateUIObject("ShopItemCard", null);
        RectTransform cardRect = tempCard.GetComponent<RectTransform>();
        cardRect.sizeDelta = new Vector2(300, 400);

        // Add a default white background
        Image bgImg = tempCard.AddComponent<Image>();
        bgImg.color = new Color(0.9f, 0.9f, 0.9f, 1f);

        // Name Text
        GameObject nameObj = CreateText("ItemNameText", tempCard.transform, "Skin Name", 32);
        RectTransform nameRect = nameObj.GetComponent<RectTransform>();
        nameRect.anchorMin = new Vector2(0, 1);
        nameRect.anchorMax = new Vector2(1, 1);
        nameRect.pivot = new Vector2(0.5f, 1);
        nameRect.anchoredPosition = new Vector2(0, -20);
        nameRect.sizeDelta = new Vector2(0, 50);

        // Icon Image
        GameObject iconObj = CreateUIObject("IconImage", tempCard.transform);
        Image iconImg = iconObj.AddComponent<Image>();
        iconImg.color = Color.gray; // Placeholder color
        RectTransform iconRect = iconObj.GetComponent<RectTransform>();
        iconRect.sizeDelta = new Vector2(150, 150);
        iconRect.anchoredPosition = new Vector2(0, 20);

        // Cost Text
        GameObject costObj = CreateText("ItemCostText", tempCard.transform, "Cost: 100", 28);
        RectTransform costRect = costObj.GetComponent<RectTransform>();
        costRect.anchorMin = new Vector2(0, 0);
        costRect.anchorMax = new Vector2(1, 0);
        costRect.pivot = new Vector2(0.5f, 0);
        costRect.anchoredPosition = new Vector2(0, 100);
        costRect.sizeDelta = new Vector2(0, 40);

        // Button
        GameObject buttonObj = CreateButton("InteractionButton", tempCard.transform, "BUY", new Vector2(0, 20));
        RectTransform btnRect = buttonObj.GetComponent<RectTransform>();
        btnRect.anchorMin = new Vector2(0, 0);
        btnRect.anchorMax = new Vector2(1, 0);
        btnRect.pivot = new Vector2(0.5f, 0);
        btnRect.sizeDelta = new Vector2(250, 60);

        // Locked Overlay
        GameObject lockObj = CreateUIObject("LockedOverlay", tempCard.transform);
        StretchToFill(lockObj.GetComponent<RectTransform>());
        Image lockImg = lockObj.AddComponent<Image>();
        lockImg.color = new Color(0, 0, 0, 0.7f); // Dark semi-transparent
        GameObject lockText = CreateText("LockText", lockObj.transform, "LOCKED", 48);
        lockText.GetComponent<TextMeshProUGUI>().color = Color.red;

        // Equipped Outline
        GameObject outlineObj = CreateUIObject("EquippedOutline", tempCard.transform);
        StretchToFill(outlineObj.GetComponent<RectTransform>());
        Image outlineImg = outlineObj.AddComponent<Image>();
        outlineImg.color = new Color(0, 1, 0, 0.4f); // Green outline

        // Add ShopItemUI script
        ShopItemUI itemScript = tempCard.AddComponent<ShopItemUI>();
        itemScript.itemNameText = nameObj.GetComponent<TextMeshProUGUI>();
        itemScript.itemCostText = costObj.GetComponent<TextMeshProUGUI>();
        itemScript.itemIconImage = iconImg;
        itemScript.interactionButton = buttonObj.GetComponent<Button>();
        itemScript.buttonActionText = buttonObj.GetComponentInChildren<TextMeshProUGUI>();
        itemScript.lockedOverlay = lockObj;
        itemScript.equippedOutline = outlineObj;

        // Save Prefab
        GameObject savedPrefab = PrefabUtility.SaveAsPrefabAsset(tempCard, prefabPath);
        DestroyImmediate(tempCard);
        Debug.Log($"[Shop Setup] Shop Item Prefab created successfully at {prefabPath}");

        // 3. Find StoreContainer in the scene
        GameObject storeContainer = GameObject.Find("StoreContainer");
        if (storeContainer != null)
        {
            // Add Grid Layout
            GridLayoutGroup grid = storeContainer.GetComponent<GridLayoutGroup>();
            if (grid == null)
            {
                grid = storeContainer.AddComponent<GridLayoutGroup>();
                grid.cellSize = new Vector2(300, 400);
                grid.spacing = new Vector2(30, 30);
                grid.padding = new RectOffset(50, 50, 50, 50);
                grid.childAlignment = TextAnchor.UpperCenter;
            }

            // Add Content Size Fitter for dynamic scrolling
            ContentSizeFitter fitter = storeContainer.GetComponent<ContentSizeFitter>();
            if (fitter == null)
            {
                fitter = storeContainer.AddComponent<ContentSizeFitter>();
                fitter.verticalFit = ContentSizeFitter.FitMode.MinSize;
            }

            // Attach ShopController
            ShopController controller = storeContainer.GetComponent<ShopController>();
            if (controller == null)
            {
                controller = storeContainer.AddComponent<ShopController>();
            }

            // Link the prefab using SerializedObject
            SerializedObject so = new SerializedObject(controller);
            so.FindProperty("shopItemPrefab").objectReferenceValue = savedPrefab;
            so.ApplyModifiedProperties();

            Debug.Log("[Shop Setup] StoreContainer successfully configured with grid layout and ShopController!");
            Selection.activeGameObject = storeContainer;
        }
        else
        {
            Debug.LogWarning("[Shop Setup] Could not find 'StoreContainer' in the scene! You'll need to manually attach the Grid Layout and Controller.");
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    // --- Helper Methods ---
    private static GameObject CreateUIObject(string name, Transform parent)
    {
        GameObject obj = new GameObject(name);
        if (parent != null) obj.transform.SetParent(parent, false);
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

    private static GameObject CreateText(string name, Transform parent, string text, int fontSize)
    {
        GameObject obj = CreateUIObject(name, parent);
        TextMeshProUGUI tmp = obj.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.black;
        StretchToFill(obj.GetComponent<RectTransform>());
        return obj;
    }

    private static GameObject CreateButton(string name, Transform parent, string text, Vector2 anchoredPosition)
    {
        GameObject btnObj = CreateUIObject(name, parent);
        Image img = btnObj.AddComponent<Image>();
        img.color = Color.white;
        Button btn = btnObj.AddComponent<Button>();

        GameObject textObj = CreateText("Text", btnObj.transform, text, 36);
        textObj.GetComponent<TextMeshProUGUI>().color = Color.black;
        
        RectTransform rect = btnObj.GetComponent<RectTransform>();
        rect.anchoredPosition = anchoredPosition;

        return btnObj;
    }
}
#endif
