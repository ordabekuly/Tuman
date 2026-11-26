using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using TumanGame;
using Object=UnityEngine.Object;

[InitializeOnLoad]
public static class VerifyMapJourney
{
    const string Flag="MapJourney.Verify";
    static readonly List<string> results=new List<string>();
    static int phase;static double due;static bool failed;
    static GameSession game;static Vector2 position;static int hp,score;
    static Vector3 ripplePosition;static VillageAtmosphere atmosphere;
    static VerifyMapJourney(){EditorApplication.playModeStateChanged+=State;}
    public static void Run()
    {
        var path=GameSession.SavePath;
        SessionState.SetBool(Flag+".hadSave",File.Exists(path));
        SessionState.SetString(Flag+".save",File.Exists(path)?File.ReadAllText(path):"");
        Directory.CreateDirectory(Path.GetDirectoryName(path));File.WriteAllText(path,JsonUtility.ToJson(SaveData.Defaults(),true));
        EditorSceneManager.OpenScene("Assets/Scenes/TumanVillageDraft.unity");
        EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene("Assets/Scenes/TumanVillageDraft.unity",true),new EditorBuildSettingsScene("Assets/Scenes/Tuman11.unity",false)};
        SessionState.SetBool(Flag,true);EditorApplication.isPlaying=true;
    }
    static void State(PlayModeStateChange state)
    {
        if(!SessionState.GetBool(Flag,false))return;
        if(state==PlayModeStateChange.EnteredPlayMode){phase=0;due=EditorApplication.timeSinceStartup+.6;EditorApplication.update+=Tick;}
        if(state==PlayModeStateChange.EnteredEditMode)
        {
            if(SessionState.GetBool(Flag+".hadSave",false))File.WriteAllText(GameSession.SavePath,SessionState.GetString(Flag+".save",""));
            else if(File.Exists(GameSession.SavePath))File.Delete(GameSession.SavePath);
            SessionState.SetBool(Flag,false);File.WriteAllLines("Submission_Map/journey-checks.txt",results);
            EditorApplication.Exit(failed?1:0);
        }
    }
    static void Check(string label,bool pass){results.Add((pass?"PASS: ":"FAIL: ")+label);if(!pass){failed=true;throw new Exception(label);}}
    static void FreezeEnemies(){foreach(var ai in Object.FindObjectsByType<EnemyAI>())ai.enabled=false;}
    static void Win()
    {
        foreach(var enemy in Object.FindObjectsByType<EnemyHealth>().Where(e=>e.CountsForProgress&&e.IsAlive).ToArray())enemy.TakeDamage(999);
        Check("stage completion unlocks next stage",!game.IsPlaying&&game.UI.CurrentScreen=="Victory");
    }
    static void AdvanceThroughButton()
    {
        Object.FindObjectsByType<Button>().Single(b=>b.name=="Primary"&&b.gameObject.activeInHierarchy).onClick.Invoke();
    }
    static void Tick()
    {
        if(EditorApplication.timeSinceStartup<due)return;
        due=EditorApplication.timeSinceStartup+.25;
        try
        {
            switch(phase++)
            {
                case 0:
                    game=GameSession.Instance;Check("new scene uses seamless progression",game&&game.seamlessStages);
                    atmosphere=Object.FindAnyObjectByType<VillageAtmosphere>();
                    if(atmosphere){Check("living landscape references ready",atmosphere.trees.Length>20&&atmosphere.ripples.Length>20&&atmosphere.mist.Length>0&&atmosphere.glows.Length>0);ripplePosition=atmosphere.ripples[0].position;}
                    game.StartLevel(0);FreezeEnemies();
                    game.NextStage();Check("cannot skip unfinished stage",game.CurrentLevel.levelNumber==1);
                    game.player.transform.position=new Vector3(-20,12,0);game.player.GetComponent<Rigidbody2D>().position=new Vector2(-20,12);
                    game.player.TakeDamage(1);position=game.player.GetComponent<Rigidbody2D>().position;hp=game.player.CurrentHealth;
                    Win();score=game.Score;break;
                case 1:AdvanceThroughButton();break;
                case 2:
                    FreezeEnemies();Check("result button enters stage two",game.IsPlaying&&game.CurrentLevel.levelNumber==2);
                    if(atmosphere)Check("water current animates during play",Vector3.Distance(ripplePosition,atmosphere.ripples[0].position)>.001f);
                    Check("position survives stage transition",Vector2.Distance(position,game.player.GetComponent<Rigidbody2D>().position)<.05f);
                    Check("health survives stage transition",game.player.CurrentHealth==hp);
                    Check("score survives stage transition",game.Score==score);
                    game.player.transform.position=new Vector3(-18,-16,0);game.player.GetComponent<Rigidbody2D>().position=new Vector2(-18,-16);
                    position=new Vector2(-18,-16);Win();score=game.Score;break;
                case 3:AdvanceThroughButton();break;
                case 4:
                    FreezeEnemies();Check("third stage continues at ritual site",game.CurrentLevel.levelNumber==3&&Vector2.Distance(game.player.GetComponent<Rigidbody2D>().position,position)<.05f);
                    Check("third stage contains wolves and ghosts",Object.FindObjectsByType<EnemyHealth>().Count(e=>e.IsAlive&&e.CountsForProgress)==4);
                    Check("third stage retains health and score",game.player.CurrentHealth==hp&&game.Score==score);
                    game.Pause();game.Resume();Check("pause retains location",Vector2.Distance(game.player.GetComponent<Rigidbody2D>().position,position)<.05f);
                    game.Retry();FreezeEnemies();Check("retry deliberately resets spawn health and score",Vector2.Distance(game.player.GetComponent<Rigidbody2D>().position,game.CurrentLevel.playerSpawn)<.05f&&game.player.CurrentHealth==game.player.MaxHealth&&game.Score==0);
                    break;
                case 5:
                    game.seamlessStages=false;game.StartLevel(0);FreezeEnemies();game.player.transform.position=new Vector3(-20,12,0);game.player.GetComponent<Rigidbody2D>().position=new Vector2(-20,12);game.player.TakeDamage(1);break;
                case 6:Win();break;
                case 7:AdvanceThroughButton();break;
                case 8:
                    FreezeEnemies();Check("legacy scenes retain independent level starts",game.CurrentLevel.levelNumber==2&&Vector2.Distance(game.player.GetComponent<Rigidbody2D>().position,game.CurrentLevel.playerSpawn)<.05f&&game.Score==0&&game.player.CurrentHealth==game.player.MaxHealth);
                    Finish();break;
            }
        }
        catch(Exception e){failed=true;Debug.LogException(e);results.Add("ERROR: "+e.Message);Finish();}
    }
    static void Finish(){EditorApplication.update-=Tick;EditorApplication.isPlaying=false;}
}
