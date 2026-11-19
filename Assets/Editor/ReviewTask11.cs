using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using TumanGame;

[InitializeOnLoad]
public static class ReviewTask11
{
    const string Request="Submission_Backup/review.request";
    static ReviewTask11(){EditorApplication.update+=Tick;}
    public static void Open()
    {
        EditorSceneManager.OpenScene("Assets/Scenes/Tuman11.unity");
        var type=Type.GetType("UnityEditor.GameView,UnityEditor");
        if(type!=null){var view=EditorWindow.GetWindow(type);view.Show();view.maximized=true;}
        EditorApplication.isPlaying=true;
    }
    static void Tick()
    {
        if(!File.Exists(Request)||EditorApplication.isCompiling||EditorApplication.isUpdating)return;
        string command=File.ReadAllText(Request).Trim();
        if(command=="data"&&EditorApplication.isPlaying){EditorApplication.isPlaying=false;return;}
        if(command=="play"&&!EditorApplication.isPlaying){File.Delete(Request);Open();return;}
        if(command=="data") {
            var type=Type.GetType("UnityEditor.GameView,UnityEditor");if(type!=null)EditorWindow.GetWindow(type).maximized=false;
            Selection.activeObject=AssetDatabase.LoadAssetAtPath<PlayerStats>("Assets/Data/PlayerStats.asset");
            EditorGUIUtility.PingObject(Selection.activeObject);File.Delete(Request);return;
        }
        if(!EditorApplication.isPlaying||!GameSession.Instance||!GameSession.Instance.UI)return;
        File.Delete(Request);
        var game=GameSession.Instance;
        switch(command) {
            case "home":game.ShowHome();break;
            case "levels":game.UI.ShowLevels();break;
            case "settings":game.UI.ShowSettings();break;
            case "hud":game.StartLevel(0);break;
            case "confirm":game.UI.ShowSettings();game.UI.ShowConfirm();break;
            case "save":
                game.SetMusic(.37f);game.SetEffects(.62f);
                Directory.CreateDirectory("Submission_Task11");
                File.Copy(GameSession.SavePath,"Submission_Task11/save.json",true);
                SaveWindow.ShowSave(File.ReadAllText(GameSession.SavePath));break;
        }
    }
    sealed class SaveWindow:EditorWindow
    {
        static string json;
        public static void ShowSave(string text){json=text;var w=GetWindow<SaveWindow>();w.titleContent=new GUIContent("Tuman save.json");w.minSize=new Vector2(600,500);w.Show();}
        void OnGUI(){EditorGUILayout.LabelField("save.json — actual Editor persistent save",EditorStyles.boldLabel);EditorGUILayout.TextArea(json??"",GUILayout.ExpandHeight(true));}
    }
}
