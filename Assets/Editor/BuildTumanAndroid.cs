using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

[InitializeOnLoad]
public static class BuildTumanAndroid
{
    const string Request = "Submission_Backup/android-build.request";
    static double ready;
    static BuildTumanAndroid()
    {
        ready = EditorApplication.timeSinceStartup + 5;
        EditorApplication.update += Check;
    }
    [MenuItem("Tuman/Build Android APK")]
    public static void Queue()
    {
        Directory.CreateDirectory("Submission_Backup");
        File.WriteAllText(Request, "build");
        ready = EditorApplication.timeSinceStartup + 2;
    }
    static void Check()
    {
        if (!File.Exists(Request) || EditorApplication.isCompiling || EditorApplication.isUpdating || BuildPipeline.isBuildingPlayer || EditorApplication.timeSinceStartup < ready) return;
        if (EditorApplication.isPlayingOrWillChangePlaymode) { EditorApplication.isPlaying = false; ready = EditorApplication.timeSinceStartup + 4; return; }
        ready = double.MaxValue;
        try
        {
            if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android)
            {
                EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
                ready = EditorApplication.timeSinceStartup + 8;
                return;
            }
            File.Delete(Request);
            PlayerSettings.companyName = "Ordabekuly";
            PlayerSettings.productName = "Tuman";
            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, "com.ordabekuly.tuman");
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel26;
            PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevelAuto;
            PlayerSettings.Android.useCustomKeystore = false;
            EditorUserBuildSettings.buildAppBundle = false;
            EditorUserBuildSettings.exportAsGoogleAndroidProject = false;
            Directory.CreateDirectory("Builds/Android");
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions {
                scenes = new[] { "Assets/Scenes/Tuman11.unity" },
                locationPathName = "Builds/Android/Tuman.apk",
                target = BuildTarget.Android,
                options = BuildOptions.None
            });
            string result = $"result={report.summary.result}\nerrors={report.summary.totalErrors}\nsize={report.summary.totalSize}\nduration={report.summary.totalTime}\n";
            File.WriteAllText("Submission_Backup/android-build-result.txt", result);
            Debug.Log("Tuman Android build: " + result);
        }
        catch (Exception error)
        {
            File.WriteAllText("Submission_Backup/android-build-result.txt", error.ToString());
            Debug.LogException(error);
        }
    }
}
