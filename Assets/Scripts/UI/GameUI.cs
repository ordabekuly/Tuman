using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
namespace TumanGame
{
    public sealed class GameUI:MonoBehaviour
    {
        GameSession game;RectTransform safe,content;GameObject screen,hud,confirm;
        Text hp,objective,timer,score,saveStatus;
        public MobileJoystick Joystick { get; private set; }
        public string CurrentScreen { get; private set; }
        public Slider MusicSlider { get; private set; }
        public Slider EffectsSlider { get; private set; }
        readonly Color ink=new Color(.055f,.09f,.095f,.97f),gold=new Color(.85f,.69f,.40f),cream=new Color(.95f,.91f,.79f);
        public void Build(GameSession session)
        {
            game=session;
            var root=new GameObject("Tuman Interface",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));
            var canvas=root.GetComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;canvas.sortingOrder=30;
            var scale=root.GetComponent<CanvasScaler>();scale.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scale.referenceResolution=new Vector2(1600,900);scale.matchWidthOrHeight=.5f;
            safe=Rect("Safe Area",root.transform,Vector2.zero,Vector2.zero,Vector2.zero);
            safe.anchorMax=Vector2.one;safe.offsetMin=safe.offsetMax=Vector2.zero;safe.gameObject.AddComponent<MobileSafeArea>();
            if(!FindAnyObjectByType<EventSystem>())new GameObject("EventSystem",typeof(EventSystem),typeof(InputSystemUIInputModule)).GetComponent<InputSystemUIInputModule>().AssignDefaultActions();
            BuildHUD();
        }
        public RectTransform Rect(string name,Transform parent,Vector2 anchor,Vector2 pos,Vector2 size)
        {
            var go=new GameObject(name,typeof(RectTransform));go.transform.SetParent(parent,false);
            var r=go.GetComponent<RectTransform>();r.anchorMin=r.anchorMax=anchor;r.anchoredPosition=pos;r.sizeDelta=size;return r;
        }
        Image Panel(string name,Transform parent,Vector2 pos,Vector2 size,Color color)
        {
            var r=Rect(name,parent,new Vector2(.5f,.5f),pos,size);var image=r.gameObject.AddComponent<Image>();image.color=color;return image;
        }
        Text Label(string name,Transform parent,string value,Vector2 pos,Vector2 size,int points,Color? color=null,TextAnchor align=TextAnchor.MiddleLeft)
        {
            var r=Rect(name,parent,new Vector2(.5f,.5f),pos,size);var t=r.gameObject.AddComponent<Text>();t.font=game.font;t.fontSize=points;
            t.text=value;t.color=color??cream;t.alignment=align;t.raycastTarget=false;t.horizontalOverflow=HorizontalWrapMode.Wrap;t.verticalOverflow=VerticalWrapMode.Truncate;return t;
        }
        public Button Button(string name,Transform parent,string title,Vector2 pos,Vector2 size,Action action,bool accent=false)
        {
            var image=Panel(name,parent,pos,size,accent?new Color(.54f,.35f,.16f):new Color(.13f,.2f,.20f));
            var outline=image.gameObject.AddComponent<Outline>();outline.effectColor=new Color(gold.r,gold.g,gold.b,.65f);outline.effectDistance=new Vector2(1,-1);
            var b=image.gameObject.AddComponent<Button>();b.targetGraphic=image;
            var colors=b.colors;colors.highlightedColor=new Color(1.2f,1.2f,1.15f);colors.pressedColor=new Color(.7f,.75f,.75f);b.colors=colors;
            b.navigation=new Navigation{mode=Navigation.Mode.None};b.onClick.AddListener(()=>action());
            Label("Label",image.transform,title,Vector2.zero,size-new Vector2(24,10),30,null,TextAnchor.MiddleCenter);return b;
        }
        void NewScreen(string title,bool backdrop=true)
        {
            if(screen){screen.SetActive(false);Destroy(screen);}if(confirm){confirm.SetActive(false);Destroy(confirm);}
            hud.SetActive(false);CurrentScreen=title;
            screen=Rect(title,safe,new Vector2(.5f,.5f),Vector2.zero,Vector2.zero).gameObject;
            var sr=(RectTransform)screen.transform;sr.anchorMin=Vector2.zero;sr.anchorMax=Vector2.one;sr.offsetMin=sr.offsetMax=Vector2.zero;
            var back=Panel("Backdrop",screen.transform,Vector2.zero,new Vector2(4000,2400),new Color(.025f,.055f,.065f,.91f));
            if(backdrop&&game.menuBackground){back.sprite=game.menuBackground;back.color=new Color(.28f,.38f,.40f,1);}
            Panel("Shade",screen.transform,Vector2.zero,new Vector2(4000,2400),new Color(.015f,.035f,.045f,.60f)).raycastTarget=false;
            content=Rect("Content",screen.transform,new Vector2(.5f,.5f),Vector2.zero,new Vector2(1320,760));
            Label("Chapter",content,"T U M A N   /   A VILLAGE IN THE MIST",new Vector2(0,335),new Vector2(1280,45),21,gold);
            Panel("Divider",content,new Vector2(0,300),new Vector2(1280,2),new Color(gold.r,gold.g,gold.b,.5f)).raycastTarget=false;
        }
        public void ShowHome()
        {
            NewScreen("Home");
            Label("Title",content,"TUMAN",new Vector2(-320,160),new Vector2(650,165),132,cream);
            Label("Subtitle",content,"Keep your light.\nFind your way home.",new Vector2(-315,45),new Vector2(640,100),35);
            Label("Description",content,"A quiet village. A restless mist.\nThree courtyards to reclaim.",new Vector2(-315,-70),new Vector2(640,80),25,new Color(.7f,.78f,.77f));
            Button("Continue",content,game.Save.Data.completedLevels==0?"BEGIN JOURNEY  >":"CONTINUE JOURNEY  >",new Vector2(-330,-190),new Vector2(600,85),()=>game.StartLevel(game.Save.Data.lastLevel-1),true);
            Button("LevelSelect",content,"COURTYARDS",new Vector2(-485,-290),new Vector2(290,70),ShowLevels);
            Button("Settings",content,"SETTINGS",new Vector2(-175,-290),new Vector2(290,70),ShowSettings);
            var card=Panel("Travel journal",content,new Vector2(405,0),new Vector2(430,500),ink);
            Label("Journal heading",card.transform,"YOUR JOURNEY",new Vector2(0,180),new Vector2(350,50),28,gold);
            Label("Progress",card.transform,$"{game.Save.Data.completedLevels} / 3\nCOURTYARDS CLEARED",new Vector2(0,55),new Vector2(350,150),40);
            Label("BestScore",card.transform,$"BEST SCORE\n{game.Save.Data.bestScore:N0}",new Vector2(0,-80),new Vector2(350,100),30);
            saveStatus=Label("SaveStatus",card.transform,game.Save.Status,new Vector2(0,-190),new Vector2(350,55),20,new Color(.6f,.78f,.67f));
            Label("Controls",content,"MOVE  WASD / JOYSTICK       FIRE  SPACE / HOLD FIRE",new Vector2(10,-380),new Vector2(1300,38),18,new Color(.65f,.74f,.73f));
        }
        public void ShowLevels()
        {
            NewScreen("Courtyards");
            Label("Heading",content,"CHOOSE YOUR PATH",new Vector2(0,235),new Vector2(1280,70),48);
            for(int i=0;i<game.levels.Length;i++)
            {
                int n=i;var level=game.levels[i];bool unlocked=game.Save.IsUnlocked(level.levelNumber);
                var card=Panel("Level "+level.levelNumber,content,new Vector2(-430+i*430,-15),new Vector2(400,390),ink);
                Label("Number",card.transform,"0"+level.levelNumber,new Vector2(0,120),new Vector2(340,100),58,unlocked?gold:Color.gray);
                Label("Title",card.transform,level.title,new Vector2(0,40),new Vector2(340,65),31);
                Label("Details",card.transform,$"{level.EnemyCount} WEREWOLVES  /  {level.timeLimit:0}s\n{level.description}",new Vector2(0,-45),new Vector2(340,100),22,new Color(.7f,.77f,.74f));
                var b=Button("Play",card.transform,unlocked?"ENTER  >":"LOCKED",new Vector2(0,-145),new Vector2(340,65),()=>game.StartLevel(n),unlocked);
                b.interactable=unlocked;
            }
            Button("Back",content,"<  HOME",new Vector2(-500,-310),new Vector2(280,65),ShowHome);
        }
        Slider SliderControl(string title,float value,Vector2 position,UnityEngine.Events.UnityAction<float> changed)
        {
            Label(title,content,title,position+new Vector2(0,42),new Vector2(650,40),26);
            var track=Panel(title+" Slider",content,position,new Vector2(650,26),new Color(.13f,.23f,.23f));
            var slider=track.gameObject.AddComponent<Slider>();slider.minValue=0;slider.maxValue=1;slider.wholeNumbers=false;
            var fill=Panel("Fill",track.transform,Vector2.zero,new Vector2(650,26),gold);
            fill.rectTransform.anchorMin=Vector2.zero;fill.rectTransform.anchorMax=Vector2.one;
            fill.rectTransform.offsetMin=fill.rectTransform.offsetMax=Vector2.zero;
            slider.fillRect=fill.rectTransform;
            var handle=Panel("Handle",track.transform,Vector2.zero,new Vector2(40,54),cream);
            slider.handleRect=handle.rectTransform;slider.targetGraphic=handle;
            slider.direction=Slider.Direction.LeftToRight;slider.SetValueWithoutNotify(value);slider.onValueChanged.AddListener(changed);
            slider.navigation=new Navigation{mode=Navigation.Mode.None};return slider;
        }
        public void ShowSettings()
        {
            NewScreen("Settings");
            Label("Heading",content,"MAKE IT YOURS",new Vector2(0,230),new Vector2(1280,70),48);
            Panel("Settings panel",content,new Vector2(-180,-20),new Vector2(900,390),ink);
            MusicSlider=SliderControl("AMBIENCE",game.Save.Data.musicVolume,new Vector2(-180,70),game.SetMusic);
            EffectsSlider=SliderControl("EFFECTS",game.Save.Data.effectsVolume,new Vector2(-180,-75),game.SetEffects);
            Label("Save note",content,"Settings are saved immediately and restored next launch.",new Vector2(-180,-170),new Vector2(760,60),20,new Color(.7f,.77f,.74f));
            Label("Reset explanation",content,"START FRESH\n\nClear scores and unlocks.\nThis cannot be undone.",new Vector2(450,25),new Vector2(330,200),27);
            Button("Reset",content,"RESET PROGRESS",new Vector2(450,-150),new Vector2(320,70),ShowConfirm);
            Button("Back",content,"<  BACK",new Vector2(-500,-310),new Vector2(280,65),()=>{if(game.Paused)ShowPause();else ShowHome();});
        }
        public void ShowConfirm()
        {
            confirm=Panel("Reset confirmation",safe,Vector2.zero,new Vector2(4000,2400),new Color(0,0,0,.75f)).gameObject;
            var panel=Panel("Confirm panel",confirm.transform,Vector2.zero,new Vector2(850,430),ink);
            Label("Question",panel.transform,"RESET YOUR JOURNEY?",new Vector2(0,110),new Vector2(730,75),42,gold);
            Label("Warning",panel.transform,"All scores, unlocked courtyards and settings will return to defaults.",new Vector2(0,20),new Vector2(730,90),26);
            Button("Cancel",panel.transform,"KEEP PROGRESS",new Vector2(-200,-120),new Vector2(350,75),()=>{confirm.SetActive(false);Destroy(confirm);});
            Button("ConfirmReset",panel.transform,"RESET",new Vector2(200,-120),new Vector2(350,75),game.ResetProgress,true);
        }
        void BuildHUD()
        {
            hud=Rect("HUD",safe,new Vector2(.5f,.5f),Vector2.zero,Vector2.zero).gameObject;
            var hr=(RectTransform)hud.transform;hr.anchorMin=Vector2.zero;hr.anchorMax=Vector2.one;hr.offsetMin=hr.offsetMax=Vector2.zero;
            var hpBox=Rect("HP",hr,new Vector2(0,1),new Vector2(180,-75),new Vector2(300,100));
            hpBox.gameObject.AddComponent<Image>().color=ink;
            hp=Label("Health",hpBox,"",Vector2.zero,new Vector2(260,65),30);
            var center=Rect("Objective",hr,new Vector2(.5f,1),new Vector2(0,-80),new Vector2(650,120));
            objective=Label("Goal",center,"",new Vector2(0,20),new Vector2(650,60),26,null,TextAnchor.MiddleCenter);
            timer=Label("Time",center,"",new Vector2(0,-28),new Vector2(650,40),23,gold,TextAnchor.MiddleCenter);
            var scoreBox=Rect("Score",hr,new Vector2(1,1),new Vector2(-170,-155),new Vector2(280,70));
            score=Label("ScoreValue",scoreBox,"",Vector2.zero,new Vector2(260,60),26,gold,TextAnchor.MiddleCenter);
            var pause=Button("Pause",hr,"II  PAUSE",Vector2.zero,new Vector2(240,75),game.Pause);
            var pr=(RectTransform)pause.transform;pr.anchorMin=pr.anchorMax=new Vector2(1,1);pr.anchoredPosition=new Vector2(-155,-65);
            var pad=Rect("Movement Joystick",hr,Vector2.zero,new Vector2(175,165),new Vector2(220,220));
            pad.gameObject.AddComponent<Image>().color=new Color(.12f,.19f,.19f,.8f);
            var knob=Rect("Knob",pad,new Vector2(.5f,.5f),Vector2.zero,new Vector2(80,80));knob.gameObject.AddComponent<Image>().color=gold;
            knob.GetComponent<Image>().raycastTarget=false;
            Joystick=pad.gameObject.AddComponent<MobileJoystick>();Joystick.SetKnob(knob);
            var fire=Button("Fire",hr,"FIRE",Vector2.zero,new Vector2(200,150),()=>{},true);
            var fr=(RectTransform)fire.transform;fr.anchorMin=fr.anchorMax=new Vector2(1,0);fr.anchoredPosition=new Vector2(-170,165);
            fire.gameObject.AddComponent<HoldFire>().shooting=game.player.GetComponent<PlayerShooting>();
            game.player.GetComponent<PlayerMovement>().SetJoystick(Joystick);
            hud.SetActive(false);
        }
        public void ShowHUD(){if(screen)screen.SetActive(false);hud.SetActive(true);CurrentScreen="HUD";}
        public void ShowPause()
        {
            NewScreen("Pause",false);
            Label("Heading",content,"A MOMENT OF QUIET",new Vector2(0,150),new Vector2(1280,100),64,null,TextAnchor.MiddleCenter);
            Button("Resume",content,"RESUME",new Vector2(0,15),new Vector2(560,90),game.Resume,true);
            Button("Settings",content,"SETTINGS",new Vector2(0,-100),new Vector2(560,75),ShowSettings);
            Button("Home",content,"RETURN HOME",new Vector2(0,-205),new Vector2(560,75),game.ShowHome);
        }
        public void ShowResult(bool won,string reason)
        {
            NewScreen(won?"Victory":"Defeat",false);
            Label("Result title",content,won?"LIGHT RETURNS":"LOST IN THE MIST",new Vector2(0,150),new Vector2(1280,100),64,won?gold:cream,TextAnchor.MiddleCenter);
            Label("Reason",content,reason,new Vector2(0,55),new Vector2(1000,60),30,null,TextAnchor.MiddleCenter);
            Label("Score",content,$"SCORE  {game.Score:N0}    /    BEST  {game.Save.Data.bestScore:N0}",new Vector2(0,-20),new Vector2(1000,60),30,gold,TextAnchor.MiddleCenter);
            Button("Primary",content,won&&game.CurrentLevel.levelNumber<3?"NEXT COURTYARD  >":"TRY AGAIN",new Vector2(0,-130),new Vector2(600,85),()=>{if(won&&game.CurrentLevel.levelNumber<3)game.StartLevel(game.CurrentLevel.levelNumber);else game.Retry();},true);
            Button("Levels",content,"COURTYARDS",new Vector2(-165,-240),new Vector2(290,70),ShowLevels);
            Button("Home",content,"HOME",new Vector2(165,-240),new Vector2(290,70),game.ShowHome);
            Label("Saved",content,game.Save.Status,new Vector2(0,-330),new Vector2(1000,45),21,null,TextAnchor.MiddleCenter);
        }
        void Update()
        {
            if(!game||!game.CurrentLevel||!hud.activeSelf)return;
            hp.text=$"HP  {game.player.CurrentHealth} / {game.player.MaxHealth}";
            objective.text=$"{game.CurrentLevel.title}   /   {game.CurrentLevel.goal}";
            timer.text=$"{game.Defeated} / {game.CurrentLevel.EnemyCount} CLEARED     {Mathf.CeilToInt(game.Remaining)}s";
            score.text="SCORE  "+game.Score;
        }
        void OnDestroy(){if(safe)Destroy(safe.parent.gameObject);}
    }
}
