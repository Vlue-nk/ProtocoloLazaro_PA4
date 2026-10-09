using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;

public static class BuildWindows
{
    public static void Development() => Build(true);
    public static void Release() => Build(false);
    private static void Build(bool development)
    {
        BuildLazaroScene.Validate();
        string root = Environment.GetEnvironmentVariable("LAZARO_BUILD_ROOT");
        if (string.IsNullOrEmpty(root)) throw new Exception("LAZARO_BUILD_ROOT is required and must be outside the repository.");
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
