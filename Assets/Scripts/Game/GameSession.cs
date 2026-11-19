using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Rendering.Universal;
namespace TumanGame
{
    public sealed class GameSession : MonoBehaviour
    {
        public static GameSession Instance { get; private set; }
        public LevelData[] levels;
        public ItemValues items;
        public GameObject enemyTemplate;
        public GameObject ghostTemplate;
        public PlayerHealth player;
        public Font font;
        public Sprite menuBackground;
        public Sprite joystickBase,joystickKnob,fireIcon;
        public bool IsPlaying { get; private set; }
        public bool Paused { get; private set; }
        public LevelData CurrentLevel { get; private set; }
        public SaveService Save { get; private set; }
        public GameUI UI { get; private set; }
        public GameAudio Sound { get; private set; }
        public int Score { get; private set; }
        public int Defeated { get; private set; }
        public float Remaining { get; private set; }
        readonly List<GameObject> spawned=new List<GameObject>();
        readonly HashSet<int> defeatedIds=new HashSet<int>();
        public static string SavePath=>Path.Combine(Application.persistentDataPath,"save.json");
        void Awake()
        {
            Instance=this;Time.timeScale=1;
            if(!FindAnyObjectByType<AudioListener>())gameObject.AddComponent<AudioListener>();
            Save=new SaveService(SavePath);
            Sound=gameObject.AddComponent<GameAudio>();Sound.Apply(Save.Data);
            UI=gameObject.AddComponent<GameUI>();UI.Build(this);
        }
        void Start(){ShowHome();}
        void Update()
        {
            if(!IsPlaying)return;
            Remaining=Mathf.Max(0,Remaining-Time.deltaTime);
            if(!player.IsAlive)End(false,"You fell in the mist");
            else if(Remaining<=0)End(false,"The mist closed in");
        }
        void ClearRun()
        {
            foreach(var obj in spawned)if(obj)Destroy(obj);
            spawned.Clear();defeatedIds.Clear();
            foreach(var shot in FindObjectsByType<LightProjectile>())Destroy(shot.gameObject);
            foreach(var shot in FindObjectsByType<EnemyBolt>())Destroy(shot.gameObject);
            var body=player.GetComponent<Rigidbody2D>();body.linearVelocity=Vector2.zero;
        }
        public void StartLevel(int index)
        {
            if(index<0||index>=levels.Length||!Save.IsUnlocked(levels[index].levelNumber))return;
            ClearRun();Time.timeScale=1;Paused=false;
            CurrentLevel=levels[index];Score=0;Defeated=0;Remaining=CurrentLevel.timeLimit;
            Save.Data.lastLevel=CurrentLevel.levelNumber;Save.Save();
            player.gameObject.SetActive(true);player.Restore();
            player.transform.position=CurrentLevel.playerSpawn;
            player.GetComponent<Rigidbody2D>().position=CurrentLevel.playerSpawn;
            Physics2D.SyncTransforms();
            var light=FindAnyObjectByType<Light2D>();if(light)light.color=CurrentLevel.atmosphere;
            SpawnEnemies(enemyTemplate,CurrentLevel.enemySpawns,CurrentLevel.enemyStats,"Werewolf");
            SpawnEnemies(ghostTemplate,CurrentLevel.ghostSpawns,CurrentLevel.ghostStats,"Ghost");
            SpawnItems(CurrentLevel.herbSpawns,true);SpawnItems(CurrentLevel.tokenSpawns,false);
            IsPlaying=true;UI.ShowHUD();
            var follow=Camera.main?Camera.main.GetComponent<FollowCamera>():null;if(follow)follow.Snap();
        }
        void SpawnEnemies(GameObject template,Vector2[] positions,EnemyStats values,string kind)
        {
            if(positions==null||positions.Length==0)return;
            if(!template||!values)throw new System.InvalidOperationException("Missing "+kind+" template or stats.");
            for(int i=0;i<positions.Length;i++)
            {
                Vector2 position=positions[i];
                var enemy=Instantiate(template,position,Quaternion.identity);enemy.name=kind+" "+(i+1);
                var a=new GameObject("Patrol A");a.transform.position=position+Vector2.left;
                var b=new GameObject("Patrol B");b.transform.position=position+Vector2.right;
                spawned.Add(a);spawned.Add(b);spawned.Add(enemy);
                enemy.GetComponent<EnemyAI>().Configure(player.transform,new[]{a.transform,b.transform},values);
                var health=enemy.GetComponent<EnemyHealth>();health.stats=values;health.CountsForProgress=true;
                enemy.SetActive(true);
            }
        }
        void SpawnItems(Vector2[] points,bool healing)
        {
            if(points==null)return;
            foreach(var point in points){var go=new GameObject(healing?"Healing herb":"Light token");go.transform.position=point;
                var pickup=go.AddComponent<WorldPickup>();pickup.healing=healing;pickup.session=this;spawned.Add(go);}
        }
        public void EnemyDefeated(EnemyHealth enemy)
        {
            if(!IsPlaying||!defeatedIds.Add(enemy.GetInstanceID()))return;
            Defeated++;Score+=items.enemyScore;Sound.PlayPickup();
            if(Defeated>=CurrentLevel.EnemyCount)End(true,"The courtyard is safe");
        }
        public void Collect(bool healing)
        {
            if(!IsPlaying)return;
            if(healing)player.Heal(items.herbHealing);else Score+=items.tokenScore;
            Sound.PlayPickup();
        }
        void End(bool won,string reason)
        {
            if(!IsPlaying)return;
            IsPlaying=false;Paused=false;player.GetComponent<Rigidbody2D>().linearVelocity=Vector2.zero;
            if(won){Score+=items.completionBonus;Save.Complete(CurrentLevel.levelNumber,Score);}
            else{Save.Data.bestScore=Mathf.Max(Save.Data.bestScore,Score);Save.Save();}
            Time.timeScale=0;UI.ShowResult(won,reason);
        }
        public void Retry(){if(CurrentLevel)StartLevel(CurrentLevel.levelNumber-1);}
        public void ShowHome()
        {
            IsPlaying=false;Paused=false;Time.timeScale=1;ClearRun();
            player.gameObject.SetActive(false);UI.ShowHome();
        }
        public void Pause()
        {
            if(!IsPlaying)return;
            IsPlaying=false;Paused=true;Time.timeScale=0;Save.Save();UI.ShowPause();
        }
        public void Resume(){if(!Paused)return;Paused=false;Time.timeScale=1;IsPlaying=true;UI.ShowHUD();}
        public void SetMusic(float volume){Save.Data.musicVolume=volume;Sound.Apply(Save.Data);Save.Save();}
        public void SetEffects(float volume){Save.Data.effectsVolume=volume;Sound.Apply(Save.Data);Save.Save();}
        public void ResetProgress(){Save.Reset();Sound.Apply(Save.Data);ShowHome();}
        void OnApplicationPause(bool pause){if(pause){Save?.Save();if(IsPlaying)Pause();}}
        void OnApplicationQuit(){Save?.Save();}
        void OnDestroy(){if(Instance==this)Instance=null;Time.timeScale=1;}
    }
}
