using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

public static class ConfigureMobileControls
{
    [MenuItem("Tuman/Configure Mobile Controls")]
    public static void Configure()
    {
        if(EditorApplication.isPlaying||EditorSceneManager.GetActiveScene().path!="Assets/Scenes/Courtyard.unity")return;
        if(GameObject.Find("Mobile Canvas")){Debug.Log("Mobile controls already configured.");return;}
        var player=GameObject.Find("Player");
        if(!player||!player.GetComponent<PlayerHealth>()||!player.GetComponent<PlayerMovement>())return;
        var font=AssetDatabase.LoadAssetAtPath<Font>("Assets/UI/Fonts/Arial.ttf");
        RectTransform Rect(string name,Transform parent,Vector2 anchor,Vector2 position,Vector2 size)
        {
            var obj=new GameObject(name,typeof(RectTransform));
            obj.transform.SetParent(parent,false);
            var r=obj.GetComponent<RectTransform>();r.anchorMin=r.anchorMax=anchor;
            r.anchoredPosition=position;r.sizeDelta=size;return r;
        }
        Image Panel(RectTransform r,Color color)
        {var image=r.gameObject.AddComponent<Image>();image.color=color;return image;}
        Text Label(string name,Transform parent,string value,Vector2 anchor,Vector2 pos,Vector2 size,int points)
        {
            var text=Rect(name,parent,anchor,pos,size).gameObject.AddComponent<Text>();
            text.font=font;text.fontSize=points;text.text=value;text.alignment=TextAnchor.MiddleCenter;
            text.color=new Color(1,.96f,.82f);text.raycastTarget=false;return text;
        }
        var canvasObj=new GameObject("Mobile Canvas",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));
        var canvas=canvasObj.GetComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;
        var scaler=canvasObj.GetComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution=new Vector2(1920,1080);scaler.matchWidthOrHeight=1;
        var safe=Rect("Safe Area",canvasObj.transform,Vector2.zero,Vector2.zero,Vector2.zero);
        safe.anchorMax=Vector2.one;safe.offsetMin=safe.offsetMax=Vector2.zero;
        safe.gameObject.AddComponent<MobileSafeArea>();
        var pad=Rect("Movement Joystick",safe,Vector2.zero,new Vector2(220,210),new Vector2(280,280));
        Panel(pad,new Color(.12f,.18f,.14f,.8f));
        var knob=Rect("Knob",pad,new Vector2(.5f,.5f),Vector2.zero,new Vector2(110,110));
        Panel(knob,new Color(.85f,.73f,.43f,.9f)).raycastTarget=false;
        var joystick=pad.gameObject.AddComponent<MobileJoystick>();
        var j=new SerializedObject(joystick);j.FindProperty("knob").objectReferenceValue=knob;j.ApplyModifiedProperties();
        Label("Move Label",pad,"MOVE",new Vector2(.5f,.5f),new Vector2(0,-180),new Vector2(280,70),36);
        var hpPanel=Rect("Health Backdrop",safe,new Vector2(0,1),new Vector2(220,-100),new Vector2(360,110));
        Panel(hpPanel,new Color(.04f,.06f,.04f,.85f)).raycastTarget=false;
        var hp=Label("Health",hpPanel,"HP: 3 / 3",new Vector2(.5f,.5f),Vector2.zero,new Vector2(340,100),48);
        var death=Rect("Death Panel",safe,new Vector2(.5f,.5f),Vector2.zero,new Vector2(700,420));
        Panel(death,new Color(.04f,.055f,.04f,.95f));
        Label("Death Title",death,"YOU DIED",new Vector2(.5f,.5f),new Vector2(0,100),new Vector2(600,100),64);
        var restart=Rect("Restart",death,new Vector2(.5f,.5f),new Vector2(0,-70),new Vector2(520,150));
        var image=Panel(restart,new Color(.35f,.25f,.12f,1));
        var button=restart.gameObject.AddComponent<Button>();button.targetGraphic=image;
        Label("Restart Text",restart,"RESTART",new Vector2(.5f,.5f),Vector2.zero,new Vector2(500,140),48);
        var health=player.GetComponent<PlayerHealth>();
        UnityEventTools.AddPersistentListener(button.onClick,health.RestartLevel);
        var h=new SerializedObject(health);h.FindProperty("showPrototypeHUD").boolValue=false;h.ApplyModifiedProperties();
        var movement=new SerializedObject(player.GetComponent<PlayerMovement>());
        movement.FindProperty("joystick").objectReferenceValue=joystick;movement.ApplyModifiedProperties();
        var hud=canvasObj.AddComponent<MobileHUD>();var u=new SerializedObject(hud);
        u.FindProperty("health").objectReferenceValue=health;u.FindProperty("healthLabel").objectReferenceValue=hp;
        u.FindProperty("deathPanel").objectReferenceValue=death.gameObject;u.ApplyModifiedProperties();
        death.gameObject.SetActive(false);
        if(!Object.FindAnyObjectByType<EventSystem>())
        {
            var events=new GameObject("EventSystem",typeof(EventSystem),typeof(InputSystemUIInputModule));
            events.GetComponent<InputSystemUIInputModule>().AssignDefaultActions();
        }
        if(Camera.main&&!Camera.main.GetComponent<MapCameraFit>())Camera.main.gameObject.AddComponent<MapCameraFit>();
        PlayerSettings.defaultInterfaceOrientation=UIOrientation.AutoRotation;
        PlayerSettings.allowedAutorotateToPortrait=false;PlayerSettings.allowedAutorotateToPortraitUpsideDown=false;
        PlayerSettings.allowedAutorotateToLandscapeLeft=true;PlayerSettings.allowedAutorotateToLandscapeRight=true;
        EditorSettings.unityRemoteDevice="Any Android Device";
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
        Selection.activeGameObject=canvasObj;
        Debug.Log("Mobile controls ready: joystick, safe-area HUD, touch restart, landscape, Unity Remote Android.");
    }
}
