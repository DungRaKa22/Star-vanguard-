using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace StarVanguard.EditorTools
{
    public static class StarVanguardBuildPipeline
    {
        public static void BuildWindowsRelease()
        {
            string projectRoot = Directory.GetCurrentDirectory();
            string buildDir = Path.Combine(projectRoot, "..", "Build", "Windows");
            
            if (!Directory.Exists(buildDir))
            {
                Directory.CreateDirectory(buildDir);
            }

            string exePath = Path.Combine(buildDir, "StarVanguard.exe");

            string[] scenes = new string[]
            {
                "Assets/Scenes/BootstrapScene.unity",
                "Assets/Scenes/MainMenuScene.unity",
                "Assets/Scenes/GameplayScene.unity"
            };

            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = exePath,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None
            };

            Debug.Log($"[STARVANGUARD-BUILD] Starting StandaloneWindows64 build to: {exePath}");
            BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
            BuildSummary summary = report.summary;

            if (summary.result == BuildResult.Succeeded)
            {
                Debug.Log($"[STARVANGUARD-BUILD] SUCCESS! Total size: {summary.totalSize} bytes, Duration: {summary.totalTime.TotalSeconds:F1}s");
                EditorApplication.Exit(0);
            }
            else
            {
                Debug.LogError($"[STARVANGUARD-BUILD] FAILED with result: {summary.result}, errors: {summary.totalErrors}");
                EditorApplication.Exit(1);
            }
        }
    }
}
