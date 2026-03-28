#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.InputSystem;

/// <summary>
/// Automates the tedious process of binding the new HorizontalMove actions to your Input System.
/// </summary>
public class InputSystemBossUpdater : EditorWindow
{
    [MenuItem("Tools/1-Click Setup Boss Controls")]
    public static void SetupBossControls()
    {
        string path = "Assets/Settings/GameControls.inputactions";
        InputActionAsset asset = AssetDatabase.LoadAssetAtPath<InputActionAsset>(path);
        
        if (asset == null)
        {
            Debug.LogError($"[InputSetup] Could not find InputActions asset at {path}!");
            return;
        }

        var map = asset.FindActionMap("Player");
        if (map == null)
        {
            Debug.LogError("[InputSetup] Could not find 'Player' Action Map!");
            return;
        }

        // 1. Add the new Action
        InputAction horizontalAction = map.FindAction("HorizontalMove");
        if (horizontalAction == null)
        {
            horizontalAction = map.AddAction("HorizontalMove", type: InputActionType.Value);
            horizontalAction.expectedControlType = "Axis";
            
            // Add A/D keys
            horizontalAction.AddCompositeBinding("1DAxis")
                .With("Negative", "<Keyboard>/a")
                .With("Positive", "<Keyboard>/d");
                
            // Add Left/Right Arrow keys
            horizontalAction.AddCompositeBinding("1DAxis")
                .With("Negative", "<Keyboard>/leftArrow")
                .With("Positive", "<Keyboard>/rightArrow");
                
            EditorUtility.SetDirty(asset);
            AssetDatabase.SaveAssets();
            
            Debug.Log("<color=cyan>[InputSetup] Success! Generated 'HorizontalMove' action (A/D and Left/Right Arrows) natively into your GameControls.</color>");
        }
        else
        {
            Debug.Log("<color=yellow>[InputSetup] 'HorizontalMove' already exists in GameControls. Skipping generation.</color>");
        }

        // Final Instruction dialog
        EditorUtility.DisplayDialog(
            "Boss Controls Generated!", 
            "I've hard-coded the new Horizontal controls into your Input System!\n\n" +
            "FINAL STEP:\n" +
            "1. Click the Player prefab in your GameScene.\n" +
            "2. Scroll down to the 'Player Input' component.\n" +
            "3. Expand 'Events' -> 'Player' -> 'HorizontalMove'.\n" +
            "4. Click the (+) button, drag your Player inside it, and select 'VirusController -> OnHorizontalMove'.\n\n" +
            "You are now ready to fight!", 
            "Got it, thanks!"
        );
    }
}
#endif
