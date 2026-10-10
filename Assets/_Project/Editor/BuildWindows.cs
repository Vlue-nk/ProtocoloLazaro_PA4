using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class BuildWindows
{
    public static void Development() => Build(true);
    public static void Release() => Build(false);
    [MenuItem("Lazaro/Build Windows Release")]
    public static void ReleaseFromEditor()
    {
        if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) Build(false);
    }
    [MenuItem("Lazaro/Build Windows Profiler")]
    public static void ProfilerFromEditor()
    {
        if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) Build(true);
    }
    private static void Build(bool development)
    {
        BuildLazaroScene.Validate();
        string root = Environment.GetEnvironmentVariable("LAZARO_BUILD_ROOT");
        if (string.IsNullOrEmpty(root))
            root = Path.GetFullPath(Path.Combine(Application.dataPath, "..", development ? "BuildsProfiler" : "Builds"));
        Directory.CreateDirectory(root);
        var result = BuildPipeline.BuildPlayer(new BuildPlayerOptions
        {
            scenes = new[] { "Assets/_Project/Scenes/MainMenu.unity", "Assets/_Project/Scenes/Laboratory_Main.unity" },
            locationPathName = Path.Combine(root, "ProtocoloLazaro.exe"),
            target = BuildTarget.StandaloneWindows64,
            options = development ? BuildOptions.Development : BuildOptions.None
        });
        if (result.summary.result != BuildResult.Succeeded) throw new Exception("Build failed: " + result.summary.result);
        UnityEngine.Debug.Log("LAZARO_BUILD_SUCCESS " + result.summary.totalSize);
    }
}
