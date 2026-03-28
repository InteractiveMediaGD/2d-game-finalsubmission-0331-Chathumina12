#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

public class FirebaseSetupBuilder : EditorWindow
{
    private string firebaseURL = "https://your-project-id.firebaseio.com";

    [MenuItem("Tools/Setup Firebase Leaderboard")]
    public static void ShowWindow()
    {
        GetWindow<FirebaseSetupBuilder>("Firebase Setup", true, typeof(EditorWindow));
    }

    private void OnGUI()
    {
        GUILayout.Space(10);
        GUILayout.Label("Firebase REST API Integration", EditorStyles.boldLabel);
        
        EditorGUILayout.HelpBox(
            "1. Go to Firebase Console and create a project.\n" +
            "2. Add a 'Realtime Database'.\n" +
            "3. Set its Security Rules to 'true' for reading and writing (for testing).\n" +
            "4. Paste the resulting Database URL below.", MessageType.Info);
        
        GUILayout.Space(10);
        firebaseURL = EditorGUILayout.TextField("Database URL:", firebaseURL);
        GUILayout.Space(10);

        if (GUILayout.Button("Apply Firebase Configuration", GUILayout.Height(30)))
        {
            ApplyFirebaseURL();
        }
    }

    private void ApplyFirebaseURL()
    {
        LeaderboardManager manager = FindObjectOfType<LeaderboardManager>();
        
        // If not found, try to find it on any inactive objects
        if (manager == null)
        {
            LeaderboardManager[] all = Resources.FindObjectsOfTypeAll<LeaderboardManager>();
            if (all.Length > 0) manager = all[0];
        }

        // If STILL not found, just generate an empty one 
        if (manager == null)
        {
            GameObject managerObj = new GameObject("LeaderboardManager");
            manager = managerObj.AddComponent<LeaderboardManager>();
            Debug.Log("[Firebase Setup] Auto-generated a new LeaderboardManager GameObject.");
        }

        // Clean the URL format
        string finalUrl = firebaseURL.Trim();
        if (finalUrl.EndsWith("/")) 
        {
            finalUrl = finalUrl.Substring(0, finalUrl.Length - 1); // remove trailing slash
        }

        Undo.RecordObject(manager, "Applied Firebase URL");
        
        SerializedObject serializeObj = new SerializedObject(manager);
        serializeObj.FindProperty("firebaseDatabaseURL").stringValue = finalUrl;
        serializeObj.ApplyModifiedProperties();

        EditorUtility.SetDirty(manager);
        
        Debug.Log($"[Firebase Setup] Success! Linked LeaderboardManager to: {finalUrl}");
        
        this.Close();
    }
}
#endif
