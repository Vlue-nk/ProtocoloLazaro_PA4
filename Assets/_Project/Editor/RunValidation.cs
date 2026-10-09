using UnityEditor;
using UnityEditor.SceneManagement;
public static class RunValidation
{
    [MenuItem("Lazaro/Validate Gameplay")]
    public static void InEditor()
    {
        if (EditorApplication.isPlaying) return;
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        SessionState.SetBool("Lazaro.RunValidation", true);
        EditorSceneManager.OpenScene("Assets/_Project/Scenes/MainMenu.unity");
        EditorApplication.EnterPlaymode();
    }
    public static void Run()
    {
        BuildLazaroScene.Build();
        EditorSceneManager.OpenScene("Assets/_Project/Scenes/MainMenu.unity");
        EditorApplication.EnterPlaymode();
    }
}
