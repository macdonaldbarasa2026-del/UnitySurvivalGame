#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace SurvivalGame.Editor
{
    /// <summary>
    /// Entry points used by GitHub Actions (game-ci/unity-builder buildMethod).
    /// Both flavours are built from the same committed scene and settings.
    /// </summary>
    public static class CiBuild
    {
        private const string PackageId = "com.mbgee.unitysurvivalgame";
        private const string GameName = "Unity Survival";

        public static void BuildDebug() => Build(true);

        public static void BuildRelease() => Build(false);

        private static void Build(bool development)
        {
            string fileName = development ? "UnitySurvival-debug.apk" : "UnitySurvival-release.apk";
            string output = Path.Combine("build", "Android", fileName);
            Directory.CreateDirectory(Path.GetDirectoryName(output));

            PlayerSettings.companyName = "MBGee";
            PlayerSettings.productName = GameName;
            PlayerSettings.bundleVersion = "1.0.0";
            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, PackageId);
            PlayerSettings.defaultScreenOrientation = ScreenOrientation.LandscapeLeft;
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel23;
            PlayerSettings.Android.bundleVersionCode = 1;
            PlayerSettings.Android.forceInternetPermission = false;

            EditorUserBuildSettings.development = development;
            EditorUserBuildSettings.androidBuildSystem = AndroidBuildSystem.Gradle;

            string[] scenes = EditorBuildSettings.scenes
                .Where(scene => scene.enabled)
                .Select(scene => scene.path)
                .ToArray();

            if (scenes.Length == 0)
            {
                throw new InvalidOperationException("No enabled scenes in EditorBuildSettings. Cannot build.");
            }

            BuildReport report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = output,
                target = BuildTarget.Android,
                options = development ? BuildOptions.Development : BuildOptions.None
            });

            if (report.summary.result != BuildResult.Succeeded)
            {
                throw new Exception("Android build failed: " + report.summary.result +
                                    " (errors: " + report.summary.totalErrors + ")");
            }

            Debug.Log("[CiBuild] Built " + output + " (" + report.summary.totalSize + " bytes)");
        }
    }
}
#endif
