using System.Linq;
using ProtocoloLazaro;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class MenuCinematicInstaller
{
    private const string MenuPath = "Assets/_Project/Scenes/MainMenu.unity";
    private const string LabPath = "Assets/_Project/Scenes/Laboratory_Main.unity";

    [MenuItem("Lazaro/Update Menu Cinematic")]
    public static void Install()
    {
        if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        var menu = EditorSceneManager.OpenScene(MenuPath);
        var previous = menu.GetRootGameObjects().FirstOrDefault(g => g.name == "Menu cinematic laboratory");
        if (previous) Object.DestroyImmediate(previous);
        var lab = EditorSceneManager.OpenScene(LabPath, OpenSceneMode.Additive);
        var backdrop = new GameObject("Menu cinematic laboratory");
        SceneManager.MoveGameObjectToScene(backdrop, menu);
        foreach (var original in lab.GetRootGameObjects())
        {
            if (original.GetComponent<GameManager>() || original.GetComponent<MissionManager>() ||
                original.GetComponent<PlayerController>() || original.GetComponent<NoiseSystem>() ||
                original.GetComponent<Canvas>() || original.GetComponent<UnityEngine.EventSystems.EventSystem>()) continue;
            var copy = Object.Instantiate(original);
            copy.name = original.name;
            SceneManager.MoveGameObjectToScene(copy, menu);
            copy.transform.SetParent(backdrop.transform, true);
            foreach (var label in copy.GetComponentsInChildren<TextMeshPro>(true))
                Object.DestroyImmediate(label.gameObject);
            if (!copy) continue;
            // This is a saved visual set: no missions, damage, navigation or gameplay input.
            foreach (var behaviour in copy.GetComponentsInChildren<MonoBehaviour>(true).Reverse())
                if (!(behaviour is TMP_Text) && !(behaviour is Volume)) Object.DestroyImmediate(behaviour);
            foreach (var agent in copy.GetComponentsInChildren<NavMeshAgent>(true)) Object.DestroyImmediate(agent);
            foreach (var collider in copy.GetComponentsInChildren<Collider>(true)) Object.DestroyImmediate(collider);
        }
        EditorSceneManager.CloseScene(lab, true);
        SceneManager.SetActiveScene(menu);
        var camera = menu.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<Camera>(true)).Single();
        camera.transform.position = new Vector3(-17, 2.7f, 5.3f);
        camera.transform.LookAt(new Vector3(-10, 1.5f, 8));
        camera.fieldOfView = 62;
        camera.nearClipPlane = .1f;
        camera.GetUniversalAdditionalCameraData().renderPostProcessing = true;
        if (!camera.GetComponent<MenuCinematic>()) camera.gameObject.AddComponent<MenuCinematic>();
        if (!camera.GetComponent<AudioListener>()) camera.gameObject.AddComponent<AudioListener>();
        var panel = menu.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<Image>(true)).Single(i => i.name == "Main menu");
        panel.color = new Color(.01f, .025f, .04f, .65f);
        RenderSettings.ambientMode = AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(.3f, .38f, .48f);
        RenderSettings.fog = true;
        RenderSettings.fogColor = new Color(.025f, .055f, .075f);
        RenderSettings.fogMode = FogMode.Linear;
        RenderSettings.fogStartDistance = 18;
        RenderSettings.fogEndDistance = 55;
        EditorSceneManager.SaveScene(menu);
        AssetDatabase.SaveAssets();
        Debug.Log("LAZARO_MENU_CINEMATIC_SAVED");
    }
}
