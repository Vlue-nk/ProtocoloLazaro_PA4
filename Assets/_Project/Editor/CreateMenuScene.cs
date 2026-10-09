using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
public static class CreateMenuScene
{
    [MenuItem("Lazaro/Open Laboratory")]
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
        view.in2DMode = false;
        view.orthographic = false;
        view.LookAt(Vector3.zero, Quaternion.Euler(55,-35,0), 27);
        var ceiling = GameObject.Find("Ceiling - hide in Scene view to inspect interior");
        if (ceiling) SceneVisibilityManager.instance.Hide(ceiling, true);
        Selection.activeGameObject = GameObject.Find("Player");
        view.Focus();
    }
}
