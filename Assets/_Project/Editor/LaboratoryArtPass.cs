using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>Installs modular art over the validated collision layout.</summary>
public static class LaboratoryArtPass
{
    private const string ScenePath = "Assets/_Project/Scenes/Laboratory_Main.unity";
    private const string RootName = "Laboratory modular architecture";
    private static Material palette;

    public static void ApplyAndValidate()
    {
        Apply();
        RunValidation.InEditor();
    }

    [MenuItem("Lazaro/Apply Modular Architecture")]
    public static void Apply()
    {
        if (EditorApplication.isPlaying) return;
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        var scene = EditorSceneManager.OpenScene(ScenePath);
        Decorate();
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        Debug.Log("LAZARO_MODULAR_ART_SAVED");
    }

    public static void Decorate()
    {
        // Detail modules contain openings: retain the opaque structural backing.
        foreach (string name in new[] { "North", "South", "East", "West", "Spine", "West partition", "East partition" })
        {
            var geometry = GameObject.Find(name);
            if (geometry && geometry.TryGetComponent<Renderer>(out var renderer)) renderer.enabled = true;
        }
        if (GameObject.Find(RootName)) return;
        palette = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Materials/SpaceStationKit.mat");
        if (!palette) throw new InvalidOperationException("Space Station palette is missing.");
        var root = new GameObject(RootName).transform;
        var walls = new GameObject("Wall modules").transform;
        walls.SetParent(root);
        for (int i = 0; i < 10; i++)
        {
            float x = -18 + i * 4;
            Module(i % 3 == 0 ? "wall-detail" : "wall", new Vector3(x, 2.5f, 14.9f), new Vector3(4, 5, .4f), 180, walls);
            Module(i % 3 == 0 ? "wall-detail" : "wall", new Vector3(x, 2.5f, -14.9f), new Vector3(4, 5, .4f), 0, walls);
        }
        for (int i = 0; i < 8; i++)
        {
            float z = -13.125f + i * 3.75f;
            Module("wall", new Vector3(-19.9f, 2.5f, z), new Vector3(3.75f, 5, .4f), 90, walls);
            Module("wall", new Vector3(19.9f, 2.5f, z), new Vector3(3.75f, 5, .4f), -90, walls);
        }
        for (int i = 0; i < 6; i++)
        {
            float z = -11 + (i + .5f) * (22f / 6);
            Module(i % 2 == 0 ? "wall-detail" : "wall", new Vector3(0, 2.5f, z), new Vector3(22f / 6, 5, .5f), 90, walls);
        }
        foreach (float side in new[] { -10f, 10f })
            for (int i = 0; i < 3; i++)
                Module("wall-detail", new Vector3(side - 4 + i * 4, 2.5f, 0), new Vector3(4, 5, .5f), 0, walls);

        var floors = new GameObject("Floor panels").transform;
        floors.SetParent(root);
        for (int x = 0; x < 10; x++)
            for (int z = 0; z < 8; z++)
                Module((x + z) % 4 == 0 ? "floor-detail" : "floor-panel", new Vector3(-18 + x * 4, .015f, -13.125f + z * 3.75f), new Vector3(3.98f, .025f, 3.73f), 0, floors);

        var exit = GameObject.Find("Extraction");
        if (exit)
        {
            Module("door-double-closed", exit.transform.position, new Vector3(3, 3.4f, .4f), 0, exit.transform);
            exit.GetComponent<Renderer>().enabled = false;
        }
    }

    private static void Module(string name, Vector3 center, Vector3 size, float yaw, Transform parent)
    {
        string path = "Assets/ThirdParty/Kenney/SpaceStationKit/Models/" + name + ".fbx";
        var source = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (!source) throw new InvalidOperationException("Missing module: " + path);
        var model = (GameObject)PrefabUtility.InstantiatePrefab(source);
        model.transform.position = Vector3.zero;
        var renderers = model.GetComponentsInChildren<Renderer>();
        var bounds = BoundsOf(renderers);
        var scale = new Vector3(size.x / Mathf.Max(.001f, bounds.size.x), size.y / Mathf.Max(.001f, bounds.size.y), size.z / Mathf.Max(.001f, bounds.size.z));
        model.transform.localScale = Vector3.Scale(model.transform.localScale, scale);
        model.transform.rotation = Quaternion.Euler(0, yaw, 0);
        bounds = BoundsOf(renderers);
        model.transform.position = center - bounds.center;
        model.transform.SetParent(parent, true);
        foreach (var renderer in renderers)
        {
            renderer.sharedMaterials = Enumerable.Repeat(palette, renderer.sharedMaterials.Length).ToArray();
            renderer.gameObject.isStatic = true;
        }
    }

    private static Bounds BoundsOf(Renderer[] renderers)
    {
        if (renderers.Length == 0) throw new InvalidOperationException("Module has no mesh renderer.");
        var bounds = renderers[0].bounds;
        foreach (var renderer in renderers) bounds.Encapsulate(renderer.bounds);
        return bounds;
    }
}
