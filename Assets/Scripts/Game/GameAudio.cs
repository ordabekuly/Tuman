using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
namespace TumanGame
{
    public enum SoundEvent { Footstep, Shot, Hit, Pickup, WolfWindup, GhostWindup, EnemyAttack, Death, Click, Win, Lose }
    // Persistent owner with two music sources and a fixed pool of SFX voices.
    public sealed class GameAudio : MonoBehaviour
    {
        public static GameAudio Instance { get; private set; }
        public SoundLibrary Library { get; private set; }
        public AudioMixer Mixer => Library.mixer;
        public bool Paused { get; private set; }
        public bool LevelMusic { get; private set; }
        public readonly Dictionary<SoundEvent,int> EventCounts = new Dictionary<SoundEvent,int>();
        AudioSource menu,level;
        readonly List<AudioSource> voices = new List<AudioSource>();
        int cursor;
        float target,fromMenu,fromLevel,fade=1,stepDistance;
        float musicVolume=.55f,effectsVolume=.8f;
        bool started,havePosition;
        Vector2 previousPosition;
        public static GameAudio GetOrCreate()=>Instance?Instance:new GameObject("Tuman Audio").AddComponent<GameAudio>();
        void Awake()
        {
            if(Instance && Instance!=this){Destroy(gameObject);return;}
            Instance=this;DontDestroyOnLoad(gameObject);
            Library=Resources.Load<SoundLibrary>("TumanSound");
            if(!Library)throw new System.InvalidOperationException("TumanSound library missing; run Tuman/Setup Sound Pass.");
            Library.mixer.updateMode=AudioMixerUpdateMode.UnscaledTime;
            menu=Source("Menu music",Library.tracks);level=Source("Level music",Library.tracks);
            menu.clip=Library.menuMusic;level.clip=Library.levelMusic;
            menu.loop=level.loop=true;menu.volume=level.volume=0;
            for(int i=0;i<16;i++)voices.Add(Source("SFX voice "+i,Library.sfx));
        }
        AudioSource Source(string name,AudioMixerGroup group)
        {
            var go=new GameObject(name);go.transform.SetParent(transform,false);
            var s=go.AddComponent<AudioSource>();s.playOnAwake=false;s.spatialBlend=0;s.outputAudioMixerGroup=group;return s;
        }
        void Start(){started=true;ApplyVolumes();Library.normal.TransitionTo(0);}
        public static float Decibels(float value)=>20*Mathf.Log10(Mathf.Max(.0001f,Mathf.Clamp01(value)));
        public void Apply(SaveData data)
        {
            musicVolume=data.musicVolume;effectsVolume=data.effectsVolume;
            if(started)ApplyVolumes();
        }
        void ApplyVolumes()
        {
            bool a=Mixer.SetFloat("MusicVol",Decibels(musicVolume));
            bool b=Mixer.SetFloat("SFXVol",Decibels(effectsVolume));
            if(!a||!b)Debug.LogError("Missing exposed mixer volume parameter.");
        }
        public void PlayMusic(bool inLevel)
        {
            if(LevelMusic==inLevel && (inLevel?level:menu).isPlaying)return;
            LevelMusic=inLevel;target=inLevel?1:0;fromMenu=menu.volume;fromLevel=level.volume;fade=0;
            var next=inLevel?level:menu;if(!next.isPlaying)next.Play();
        }
        public void SetPaused(bool value){Paused=value;(value?Library.paused:Library.normal).TransitionTo(.3f);}
        void Update()
        {
            if(fade<1)
            {
                fade=Mathf.Min(1,fade+Time.unscaledDeltaTime);
                menu.volume=Mathf.Lerp(fromMenu,(1-target)*.28f,fade);
                level.volume=Mathf.Lerp(fromLevel,target*.32f,fade);
                if(fade>=1){if(target==1)menu.Stop();else level.Stop();}
            }
            var game=GameSession.Instance;
            if(!game||!game.IsPlaying||!game.player.IsAlive){havePosition=false;stepDistance=0;return;}
            Vector2 p=game.player.GetComponent<Rigidbody2D>().position;
            if(havePosition)
            {
                float d=Vector2.Distance(p,previousPosition);if(d<1)stepDistance+=d;
                if(stepDistance>.65f){stepDistance=0;Play(SoundEvent.Footstep);}
            }
            previousPosition=p;havePosition=true;
        }
        public void PlayShot()=>Play(SoundEvent.Shot);
        public void PlayPickup()=>Play(SoundEvent.Pickup);
        public void Play(SoundEvent kind,float gain=1)
        {
            var cue=Library.Find(kind);if(cue==null||cue.clips.Length==0)return;
            int i=Random.Range(0,cue.clips.Length);if(i==cue.last && cue.clips.Length>1)i=(i+1)%cue.clips.Length;cue.last=i;
            var s=voices.Find(v=>!v.isPlaying)??voices[cursor++%voices.Count];
            s.Stop();s.pitch=Random.Range(.92f,1.08f);s.volume=1;
            s.PlayOneShot(cue.clips[i],cue.gain*Mathf.Clamp01(gain));
            EventCounts[kind]=EventCounts.TryGetValue(kind,out int count)?count+1:1;
        }
        public void Windup(bool ghost,Vector2 position)
        {
            var game=GameSession.Instance;float distance=game?Vector2.Distance(position,game.player.transform.position):0;
            Play(ghost?SoundEvent.GhostWindup:SoundEvent.WolfWindup,Mathf.Clamp(1-distance/24,.35f,1));
        }
        void OnDestroy(){if(Instance==this)Instance=null;}
    }
}
