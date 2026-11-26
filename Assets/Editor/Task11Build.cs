using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
public static class Task11Build
{
    static bool expansion;
    public static void GhostExpansion(){expansion=true;Android();}
    public static void Android()
    {
        try {
            PlayerSettings.companyName="Ordabekuly";PlayerSettings.productName="Tuman";PlayerSettings.bundleVersion="0.11.0";
            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android,"com.ordabekuly.tuman");
            PlayerSettings.Android.bundleVersionCode=expansion?12:11;PlayerSettings.Android.useCustomKeystore=false;
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android,ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.targetArchitectures=AndroidArchitecture.ARM64;
            PlayerSettings.Android.minSdkVersion=AndroidSdkVersions.AndroidApiLevel26;
            PlayerSettings.Android.targetSdkVersion=AndroidSdkVersions.AndroidApiLevel35;
            EditorUserBuildSettings.buildAppBundle=false;EditorUserBuildSettings.exportAsGoogleAndroidProject=false;
            Directory.CreateDirectory("Builds/Android");
            var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions {scenes=new[]{"Assets/Scenes/Tuman11.unity"},
                locationPathName=expansion?"Builds/Android/Tuman_Expanded.apk":"Builds/Android/Tuman_Task11.apk",target=BuildTarget.Android,options=BuildOptions.None});
            string result=$"result={report.summary.result}\nerrors={report.summary.totalErrors}\nsize={report.summary.totalSize}\nduration={report.summary.totalTime}\n";
            File.WriteAllText(expansion?"Submission_Backup/expanded-apk-result.txt":"Submission_Backup/task11-apk-result.txt",result);Debug.Log(result);
            EditorApplication.Exit(report.summary.result==BuildResult.Succeeded?0:1);
        }catch(Exception e){File.WriteAllText("Submission_Backup/task11-apk-result.txt",e.ToString());Debug.LogException(e);EditorApplication.Exit(1);}
    }
}
