#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

public class CurrencySetupBuilder : EditorWindow
{
    [MenuItem("Tools/Setup Currency System")]
    public static void SetupCurrencySystem()
    {
        // 1. Create the Data Fragment Prefab
        string prefabFolderPath = "Assets/Prefabs";
        if (!AssetDatabase.IsValidFolder(prefabFolderPath))
        {
            AssetDatabase.CreateFolder("Assets", "Prefabs");
        }

        string prefabPath = prefabFolderPath + "/DataFragment.prefab";
        
        // Create a temporary GameObject to turn into a prefab
        GameObject tempFragment = new GameObject("DataFragment");
        
        // Add SpriteRenderer (default it to a square or circle)
        SpriteRenderer sr = tempFragment.AddComponent<SpriteRenderer>();
        // Try to assign a default Unity sprite if possible
        sr.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
        sr.color = Color.cyan; // Give it a shiny tech color

        // Add triggering Collider
        CircleCollider2D col = tempFragment.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.5f;

        // Add the Pickup Script
        DataFragmentPickup pickupScript = tempFragment.AddComponent<DataFragmentPickup>();
        
        // Save as Prefab
        GameObject savedPrefab = PrefabUtility.SaveAsPrefabAsset(tempFragment, prefabPath);
        DestroyImmediate(tempFragment); // Clean up the temp object from the scene

        Debug.Log($"[Currency Setup] Data Fragment Prefab created at {prefabPath}");

        // 2. Find the LevelGenerator in the current scene
        LevelGenerator generator = FindObjectOfType<LevelGenerator>();
        if (generator != null)
        {
            // Use SerializedObject to modify the private serialized fields safely
            SerializedObject serializedGenerator = new SerializedObject(generator);
            
            // Link the prefab
            SerializedProperty prefabProp = serializedGenerator.FindProperty("dataFragmentPrefab");
            if (prefabProp != null)
            {
                prefabProp.objectReferenceValue = savedPrefab;
            }

            // Set the spawn chance to 50%
            SerializedProperty chanceProp = serializedGenerator.FindProperty("fragmentSpawnChance");
            if (chanceProp != null)
            {
                chanceProp.floatValue = 0.5f;
            }

            serializedGenerator.ApplyModifiedProperties();
            Debug.Log("[Currency Setup] LevelGenerator successfully linked to Data Fragment Prefab!");
            Selection.activeGameObject = generator.gameObject;
        }
        else
        {
            Debug.LogWarning("[Currency Setup] Could not find a LevelGenerator in the active scene! You will need to attach the Prefab manually.");
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
}
#endif
