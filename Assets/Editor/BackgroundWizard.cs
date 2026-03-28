#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public class BackgroundWizard : Editor
{
    [MenuItem("Game Polish/Create Custom Background")]
    public static void CreateCustomBackgroundUI()
    {
        Debug.Log("[BackgroundWizard] Cleaning up old backgrounds...");

        // 1. Purge all legacy BackgroundScrollers and RawImageScrollers
        BackgroundScroller[] oldScrollers = Object.FindObjectsByType<BackgroundScroller>(FindObjectsSortMode.None);
        foreach (var scroller in oldScrollers)
        {
            if (scroller != null) DestroyImmediate(scroller.gameObject);
        }

        RawImageScroller[] oldRaw = Object.FindObjectsByType<RawImageScroller>(FindObjectsSortMode.None);
        foreach (var scroller in oldRaw)
        {
            if (scroller != null) DestroyImmediate(scroller.gameObject);
        }

        // 2. Kill legacy background explicitly generated layers
        string[] oldNames = { "Canvas_Space_BG", "Animated_Space_BG", "Background_Layer", "Background_FarLayer" };
        foreach (string n in oldNames)
        {
            GameObject legacyObj = GameObject.Find(n);
            if (legacyObj != null) DestroyImmediate(legacyObj);
        }

        // 3. Create the gorgeous new Custom Background Object
        GameObject customBg = new GameObject("[YOUR CUSTOM BACKGROUND]");
        InfiniteBackground infiniteBg = customBg.AddComponent<InfiniteBackground>();

        // Select it in Editor so you can instantly see the Inspector and drop your image
        Selection.activeGameObject = customBg;

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        
        Debug.Log("=====================================");
        Debug.Log("[BackgroundWizard] SUCCESS! Look at the Inspector to the right.");
        Debug.Log("[BackgroundWizard] Drag ANY image you want into the 'Background Image' slot on [YOUR CUSTOM BACKGROUND]!");
        Debug.Log("=====================================");
    }
}
#endif
