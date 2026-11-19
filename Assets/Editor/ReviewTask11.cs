using System;
using System.IO;
using System.Linq;
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
        EditorSettings.unityRemoteDevice="None";
        EditorSceneManager.OpenScene("Assets/Scenes/Tuman11.unity");
        var type=Type.GetType("UnityEditor.GameView,UnityEditor");
        if(type!=null){var view=EditorWindow.GetWindow(type);view.Show();view.maximized=true;}
        EditorApplication.isPlaying=true;
    }
    static void Tick()
    {
        if(!File.Exists(Request)||EditorApplication.isCompiling||EditorApplication.isUpdating)return;
        string command;
        try{command=File.ReadAllText(Request).Trim();}catch(IOException){return;}
        if(command=="exit") {
            if(EditorApplication.isPlaying){EditorApplication.isPlaying=false;return;}
            File.Delete(Request);EditorApplication.Exit(0);return;
        }
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
            case "capture":CaptureScreens(game);break;
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
    static void CaptureScreens(GameSession game)
    {
        string directory="Submission_Task11/Screenshots";Directory.CreateDirectory(directory);
        game.ShowHome();Render(game,directory+"/01_Home.png",1600,900);
        game.UI.ShowLevels();Render(game,directory+"/02_LevelSelect.png",1600,900);
        game.UI.ShowSettings();Render(game,directory+"/03_Settings.png",1600,900);
        game.UI.ShowConfirm();Render(game,directory+"/04_ResetConfirm.png",1600,900);
        game.ShowHome();game.StartLevel(0);Render(game,directory+"/05_HUD_16x9.png",1600,900);
        game.Pause();Render(game,directory+"/06_Pause.png",1600,900);
        game.ShowHome();Render(game,directory+"/07_Home_Wide.png",2400,1080);
        game.StartLevel(0);Render(game,directory+"/08_HUD_Wide.png",2400,1080);
        game.ShowHome();Debug.Log("TASK11_UI_CAPTURES_READY");
    }
    static void Render(GameSession game,string path,int width,int height)
    {
        var camera=Camera.main;
        game.UI.SendMessage("Update");
        var canvas=UnityEngine.Object.FindObjectsByType<Canvas>().First(c=>c.name=="Tuman Interface");
        var previousTarget=camera.targetTexture;var previousMode=canvas.renderMode;float aspect=camera.aspect;
        var target=new RenderTexture(width,height,24,RenderTextureFormat.ARGB32);target.Create();
        camera.targetTexture=target;camera.aspect=width/(float)height;
        canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=camera;canvas.planeDistance=1;
        Canvas.ForceUpdateCanvases();
        var request=new UnityEngine.Rendering.Universal.UniversalRenderPipeline.SingleCameraRequest{destination=target};
        UnityEngine.Rendering.RenderPipeline.SubmitRenderRequest(camera,request);
        var previous=RenderTexture.active;RenderTexture.active=target;
        var texture=new Texture2D(width,height,TextureFormat.RGBA32,false);
        texture.ReadPixels(new Rect(0,0,width,height),0,0);texture.Apply();File.WriteAllBytes(path,texture.EncodeToPNG());
        RenderTexture.active=previous;camera.targetTexture=previousTarget;camera.aspect=aspect;canvas.renderMode=previousMode;
        target.Release();UnityEngine.Object.DestroyImmediate(target);UnityEngine.Object.DestroyImmediate(texture);
        Canvas.ForceUpdateCanvases();
    }
    sealed class SaveWindow:EditorWindow
    {
        static string json;
        public static void ShowSave(string text){json=text;var w=GetWindow<SaveWindow>();w.titleContent=new GUIContent("Tuman save.json");w.minSize=new Vector2(600,500);w.Show();}
        void OnGUI(){EditorGUILayout.LabelField("save.json — actual Editor persistent save",EditorStyles.boldLabel);EditorGUILayout.TextArea(json??"",GUILayout.ExpandHeight(true));}
    }
}
