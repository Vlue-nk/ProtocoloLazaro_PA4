using UnityEditor;
using UnityEditor.SceneManagement;
public static class RunValidation
{
    public static void Run()
    {
        BuildLazaroScene.Build();
        EditorSceneManager.OpenScene("Assets/_Project/Scenes/MainMenu.unity");
        EditorApplication.EnterPlaymode();
    }
}
