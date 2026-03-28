#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

public class VirusSkinSetupBuilder : EditorWindow
{
    [MenuItem("Tools/Setup Customization System (Virus Skins)")]
    public static void SetupCustomizationSystem()
    {
        // 1. Create the Resources/Skins folders if they don't exist
        string resourcesFolder = "Assets/Resources";
        if (!AssetDatabase.IsValidFolder(resourcesFolder))
        {
            AssetDatabase.CreateFolder("Assets", "Resources");
        }

        string skinsFolder = "Assets/Resources/Skins";
        if (!AssetDatabase.IsValidFolder(skinsFolder))
        {
            AssetDatabase.CreateFolder("Assets/Resources", "Skins");
        }

        // 2. Generate 3 Default Skins automatically
        VirusSkin[] generatedSkins = new VirusSkin[3];
        
        generatedSkins[0] = CreateSkinAsset("default", "Default Virus", 0, Color.green, skinsFolder);
        generatedSkins[1] = CreateSkinAsset("neon", "Neon Hacker", 100, Color.cyan, skinsFolder);
        generatedSkins[2] = CreateSkinAsset("ninja", "Stealth Node", 250, Color.red, skinsFolder);

        // 3. Find SkinManager and ShopController in the active scene and link them up
        bool linkedSuccessfully = LinkManagers(generatedSkins);

        if (linkedSuccessfully)
        {
            Debug.Log("[VirusSkin Setup] Successfully generated 3 sample skins and linked them to your SkinManager & ShopController!");
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    private static VirusSkin CreateSkinAsset(string id, string displayName, int cost, Color trailColor, string folderPath)
    {
        string assetPath = $"{folderPath}/{id}_Skin.asset";
        
        // Check if it already exists
        VirusSkin existingSkin = AssetDatabase.LoadAssetAtPath<VirusSkin>(assetPath);
        if (existingSkin != null)
        {
            return existingSkin;
        }

        // Create new ScriptableObject
        VirusSkin newSkin = ScriptableObject.CreateInstance<VirusSkin>();
        newSkin.skinID = id;
        newSkin.displayName = displayName;
        newSkin.cost = cost;
        newSkin.trailColor = trailColor;

        // Try to assign a default Unity sprite as a placeholder
        newSkin.playerSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
        newSkin.storeIcon = newSkin.playerSprite;

        AssetDatabase.CreateAsset(newSkin, assetPath);
        return newSkin;
    }

    private static bool LinkManagers(VirusSkin[] skins)
    {
        bool success = false;

        // Find SkinManager (true = include inactive objects)
        SkinManager skinManager = FindObjectOfType<SkinManager>(true);
        if (skinManager != null)
        {
            SerializedObject serializedManager = new SerializedObject(skinManager);
            SerializedProperty allSkinsProp = serializedManager.FindProperty("allSkins");
            allSkinsProp.ClearArray();
            
            for (int i = 0; i < skins.Length; i++)
            {
                allSkinsProp.InsertArrayElementAtIndex(i);
                allSkinsProp.GetArrayElementAtIndex(i).objectReferenceValue = skins[i];
            }
            serializedManager.ApplyModifiedProperties();
            success = true;
        }
        else
        {
            Debug.LogWarning("[VirusSkin Setup] Could not find SkinManager in the scene! You'll need to link the skins manually.");
        }

        // Find ShopController (true = include inactive objects)
        ShopController shopController = FindObjectOfType<ShopController>(true);
        if (shopController != null)
        {
            SerializedObject serializedShop = new SerializedObject(shopController);
            SerializedProperty availableSkinsProp = serializedShop.FindProperty("availableSkins");
            availableSkinsProp.ClearArray();
            
            for (int i = 0; i < skins.Length; i++)
            {
                availableSkinsProp.InsertArrayElementAtIndex(i);
                availableSkinsProp.GetArrayElementAtIndex(i).objectReferenceValue = skins[i];
            }
            serializedShop.ApplyModifiedProperties();
            success = true;
        }
        else
        {
            Debug.LogWarning("[VirusSkin Setup] Could not find ShopController in the MainMenu! Open the MainMenu scene and drag the skins into the ShopController manually.");
        }

        return success;
    }
}
#endif
