using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
public static class CreateMenuScene
{
    [MenuItem("Lazaro/Open Laboratory %#l")]
    public static void OpenLaboratory()
    {
        if (EditorApplication.isPlaying) { Debug.LogWarning("Stop Play mode before opening an editing scene."); return; }
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "MainMenu" && !Object.FindAnyObjectByType<Camera>())
        {
            var camera = new GameObject("MenuCamera").AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(.025f,.055f,.075f);
            EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        }
        EditorSceneManager.OpenScene("Assets/_Project/Scenes/Laboratory_Main.unity");
        var view = SceneView.lastActiveSceneView ?? EditorWindow.GetWindow<SceneView>();
        view.LookAt(new Vector3(0,0,0), Quaternion.Euler(60,0,0), 38);
        Selection.activeGameObject = GameObject.Find("Laboratory Geometry");
        view.Focus();
    }
}
