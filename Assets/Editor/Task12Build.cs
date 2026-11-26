using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
public static class Task12Build
{
 public static void Android()
 {
  Directory.CreateDirectory("Submission_Task12");
  try{
   PlayerSettings.bundleVersion="0.12.0";PlayerSettings.Android.bundleVersionCode=13;
   PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android,"com.ordabekuly.tuman");
   EditorUserBuildSettings.buildAppBundle=false;EditorUserBuildSettings.exportAsGoogleAndroidProject=false;
   var result=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{"Assets/Scenes/TumanVillageDraft.unity"},locationPathName="Submission_Task12/Tuman_Sound.apk",target=BuildTarget.Android,options=BuildOptions.None});
   File.WriteAllText("Submission_Task12/build-result.txt",$"{result.summary.result}\nerrors={result.summary.totalErrors}\nbytes={result.summary.totalSize}\nduration={result.summary.totalTime}");
   EditorApplication.Exit(result.summary.result==BuildResult.Succeeded?0:1);
  }catch(Exception e){File.WriteAllText("Submission_Task12/build-result.txt",e.ToString());Debug.LogException(e);EditorApplication.Exit(1);}
 }
}
