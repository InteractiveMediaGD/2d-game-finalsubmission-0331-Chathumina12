#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

public class EncryptedNodeSetupBuilder : EditorWindow
{
    [MenuItem("Tools/Build Encrypted Node System")]
    public static void BuildEncryptedNode()
    {
        string prefabsFolder = "Assets/Resources/Prefabs";
        if (!AssetDatabase.IsValidFolder("Assets/Resources"))
            AssetDatabase.CreateFolder("Assets", "Resources");
        if (!AssetDatabase.IsValidFolder(prefabsFolder))
            AssetDatabase.CreateFolder("Assets/Resources", "Prefabs");

        // 1. Generate Success Particles Prefab
        GameObject particlesObj = new GameObject("HackSuccess_Particles");
        ParticleSystem ps = particlesObj.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.duration = 1f;
        main.loop = false;
        main.startColor = Color.green;
        main.startSpeed = new ParticleSystem.MinMaxCurve(5f, 15f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.2f, 0.5f);
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.5f, 1.5f);
        
        var emission = ps.emission;
        emission.rateOverTime = 0;
        emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0.0f, 50, 80) });

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.5f;

        // Auto destroy script
        particlesObj.AddComponent<DestroyAfterDelay>().delay = 2.0f;

        string particlePath = $"{prefabsFolder}/HackSuccess_Particles.prefab";
        GameObject savedParticles = PrefabUtility.SaveAsPrefabAsset(particlesObj, particlePath);
        DestroyImmediate(particlesObj);


        // 2. Generate Encrypted Node Base
        GameObject nodeObj = new GameObject("EncryptedNode");
        
        // Background Base Graphic
        SpriteRenderer bgRenderer = nodeObj.AddComponent<SpriteRenderer>();
        bgRenderer.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
        bgRenderer.color = new Color(0, 0, 0, 0.8f);
        nodeObj.transform.localScale = new Vector3(3f, 3f, 1f); // Make it a large holding zone
        
        // Trigger Setup
        CircleCollider2D col = nodeObj.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.5f; // Since scale is 3, actual radius is 1.5 units 

        // 3. Child Progress Graphic
        GameObject progressObj = new GameObject("ProgressRing");
        progressObj.transform.SetParent(nodeObj.transform);
        progressObj.transform.localPosition = Vector3.zero;
        progressObj.transform.localScale = Vector3.one;
        SpriteRenderer progRenderer = progressObj.AddComponent<SpriteRenderer>();
        progRenderer.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
        progRenderer.color = new Color(0f, 0.4f, 1f, 0.6f); // Cyan
        progRenderer.sortingOrder = 1;

        // 4. Logic component
        NodeHacking hackScript = nodeObj.AddComponent<NodeHacking>();
        SerializedObject soNode = new SerializedObject(hackScript);
        soNode.FindProperty("nodeGraphic").objectReferenceValue = progRenderer;
        soNode.FindProperty("successParticles").objectReferenceValue = savedParticles;
        soNode.ApplyModifiedProperties();

        // Save as Prefab
        string nodePath = $"{prefabsFolder}/EncryptedNode.prefab";
        GameObject savedNode = PrefabUtility.SaveAsPrefabAsset(nodeObj, nodePath);
        DestroyImmediate(nodeObj);


        // 5. Connect to LevelGenerator in open scene
        LevelGenerator levelGen = FindObjectOfType<LevelGenerator>(true);
        if (levelGen != null)
        {
            SerializedObject soLevel = new SerializedObject(levelGen);
            soLevel.FindProperty("encryptedNodePrefab").objectReferenceValue = savedNode;
            soLevel.ApplyModifiedProperties();
            Debug.Log("[Node Setup] Successfully generated EncryptedNode perfectly linked into your LevelGenerator!");
        }
        else
        {
            Debug.LogWarning("[Node Setup] Generated prefabs, but could not find a LevelGenerator in the current scene to attach them to. Please attach manually.");
        }

        AssetDatabase.SaveAssets();
    }
}
#endif
