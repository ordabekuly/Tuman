using System;
using System.IO;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using TumanGame;

[InitializeOnLoad]
public static class VerifyTask11
{
    static readonly List<string> results=new List<string>();
    static GameSession game;static EnemyHealth target;static int phase;static float due;static string realPath;
    static bool success=true;static float originalSpeed;
    static VerifyTask11(){EditorApplication.playModeStateChanged+=State;}
    static void Check(string label,bool pass)
    {
        results.Add(label+"="+pass);success&=pass;
        if(!pass)Debug.LogError("Task11 check failed: "+label);
    }
    public static void DataChecks()
    {
        string dir=Path.GetFullPath("Submission_Backup/SaveTests");Directory.CreateDirectory(dir);
        string path=Path.Combine(dir,"save.json");if(File.Exists(path))File.Delete(path);
        var save=new SaveService(path);
        Check("missing_save_defaults",save.Data.completedLevels==0&&save.IsUnlocked(1)&&!save.IsUnlocked(2));
        save.Complete(1,450);save.Data.musicVolume=.23f;save.Data.effectsVolume=.64f;save.Save();
        var loaded=new SaveService(path);
        Check("json_roundtrip",loaded.IsUnlocked(2)&&loaded.Data.bestScore==450&&Mathf.Approximately(loaded.Data.musicVolume,.23f));
        foreach(string broken in new[]{"{ broken", "{}", "null", "[]", "{\"format\":\"TUMAN\",\"version\":99}"}) {
            File.WriteAllText(path,broken);var recovered=new SaveService(path);
            Check("corrupt_defaults_"+results.Count,recovered.Data.completedLevels==0&&recovered.Data.bestScore==0&&recovered.IsUnlocked(1));
        }
        save.Reset();loaded=new SaveService(path);Check("reset_persisted",save.LastWriteSucceeded&&loaded.Status=="Journey restored"&&loaded.Data.bestScore==0&&!loaded.IsUnlocked(2));
    }
    public static void Run()
    {
        SetupTask11.RefreshAnimations();
        Directory.CreateDirectory("Submission_Backup");results.Clear();success=true;DataChecks();
        realPath=GameSession.SavePath;
        SessionState.SetString("Task11.TestSavePath",realPath);
        // Back up an actual user save; only the test session gets a clean save.
        if(File.Exists(realPath))File.Copy(realPath,realPath+".before-task11-test",true);
        Directory.CreateDirectory(Path.GetDirectoryName(realPath));
        File.WriteAllText(realPath,JsonUtility.ToJson(SaveData.Defaults(),true));
        File.WriteAllLines("Submission_Backup/task11-data-checks.txt",results);
        SessionState.SetBool("Task11.PlayTest",true);
        EditorSceneManager.OpenScene("Assets/Scenes/Tuman11.unity");EditorApplication.isPlaying=true;
    }
    static void State(PlayModeStateChange state)
    {
        if(state==PlayModeStateChange.EnteredPlayMode&&SessionState.GetBool("Task11.PlayTest",false))
        {phase=0;due=Time.realtimeSinceStartup+.6f;EditorApplication.update+=Tick;}
        if(state==PlayModeStateChange.EnteredEditMode&&SessionState.GetBool("Task11.PlayTest",false))
        {
            SessionState.SetBool("Task11.PlayTest",false);
            string path=SessionState.GetString("Task11.TestSavePath","");
            if(File.Exists(path+".before-task11-test")){File.Copy(path+".before-task11-test",path,true);File.Delete(path+".before-task11-test");}
            else if(File.Exists(path))File.Delete(path);
            EditorApplication.Exit(success?0:1);
        }
    }
    static void Tick()
    {
        if(!EditorApplication.isPlaying){EditorApplication.update-=Tick;return;}
        if(Time.realtimeSinceStartup<due)return;
        try {
            switch(phase++) {
                case 0:
                    game=GameSession.Instance;
                    Check("home_screen",game&&game.UI.CurrentScreen=="Home"&&!game.IsPlaying);
                    Check("three_data_levels",game.levels.Length==3&&game.levels[0].EnemyCount==2&&game.levels[1].EnemyCount==3&&game.levels[2].EnemyCount==4);
                    game.StartLevel(1);Check("locked_level_denied",!game.IsPlaying);
                    game.UI.ShowSettings();game.UI.MusicSlider.value=.31f;game.UI.EffectsSlider.value=.47f;
                    Check("settings_ui_saves",Mathf.Approximately(new SaveService(GameSession.SavePath).Data.musicVolume,.31f));
                    game.StartLevel(0);due=Time.realtimeSinceStartup+.4f;break;
                case 1:
                    Check("level_one_spawn_count",UnityEngine.Object.FindObjectsByType<EnemyHealth>().Length==2);
                    foreach(var e in UnityEngine.Object.FindObjectsByType<EnemyHealth>())e.GetComponent<EnemyAI>().enabled=false;
                    target=UnityEngine.Object.FindObjectsByType<EnemyHealth>()[0];
                    target.GetComponent<Rigidbody2D>().position=new Vector2(6,-3);
                    foreach(var e in UnityEngine.Object.FindObjectsByType<EnemyHealth>())if(e!=target)e.GetComponent<Rigidbody2D>().position=new Vector2(7,5);
                    game.player.GetComponent<Rigidbody2D>().position=new Vector2(3,-3);Physics2D.SyncTransforms();
                    game.player.GetComponent<PlayerShooting>().Fire();due=Time.realtimeSinceStartup+.5f;break;
                case 2:
                    Check("shot_one_leaves_two_hp",target.CurrentHealth==2);
                    game.player.GetComponent<PlayerShooting>().Fire();due=Time.realtimeSinceStartup+.5f;break;
                case 3:
                    Check("shot_two_leaves_one_hp",target.CurrentHealth==1);
                    game.player.GetComponent<PlayerShooting>().Fire();due=Time.realtimeSinceStartup+.5f;break;
                case 4:
                    Check("third_shot_kills",!target||!target.IsAlive);
                    var animator=game.player.GetComponent<PlayerSpriteAnimation>();
                    Check("four_direction_animation_assets",animator.library.directions.Length==4&&Array.TrueForAll(animator.library.directions,d=>d.idle.Length==8&&d.walk.Length==8&&d.walkShoot.Length==8&&d.idleShoot.Length>0));
                    originalSpeed=animator.stats.moveSpeed;animator.stats.moveSpeed=7;
                    var joystick=game.UI.Joystick;var ev=new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current){pointerId=123};
                    var rect=(RectTransform)joystick.transform;Canvas.ForceUpdateCanvases();
                    ev.position=RectTransformUtility.WorldToScreenPoint(null,rect.TransformPoint(new Vector3(rect.rect.width*.45f,0,0)));
                    joystick.OnPointerDown(ev);due=Time.realtimeSinceStartup+.2f;break;
                case 5:
                    Check("live_balance_without_code_edit",game.player.GetComponent<Rigidbody2D>().linearVelocity.x>6);
                    game.player.GetComponent<PlayerSpriteAnimation>().stats.moveSpeed=originalSpeed;
                    game.UI.Joystick.OnPointerUp(new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current){pointerId=123});
                    foreach(var e in UnityEngine.Object.FindObjectsByType<EnemyHealth>())e.TakeDamage(100);
                    due=Time.realtimeSinceStartup+.3f;break;
                case 6:
                    Check("completion_unlocks_level_two",game.Save.IsUnlocked(2)&&game.UI.CurrentScreen=="Victory");
                    game.StartLevel(1);due=Time.realtimeSinceStartup+.2f;break;
                case 7:
                    Check("level_two_from_data",game.CurrentLevel.levelNumber==2&&UnityEngine.Object.FindObjectsByType<EnemyHealth>().Length==3);
                    foreach(var e in UnityEngine.Object.FindObjectsByType<EnemyHealth>())e.TakeDamage(100);
                    due=Time.realtimeSinceStartup+.2f;break;
                case 8:
                    game.StartLevel(2);due=Time.realtimeSinceStartup+.2f;break;
                case 9:
                    Check("level_three_from_data",game.CurrentLevel.levelNumber==3&&UnityEngine.Object.FindObjectsByType<EnemyHealth>().Length==4);
                    game.Pause();Check("pause_saves",!game.IsPlaying&&game.Paused&&game.Save.LastWriteSucceeded);
                    game.Resume();Check("resume",game.IsPlaying&&!game.Paused);
                    game.SendMessage("OnApplicationPause",true);Check("background_pause_saves",game.Paused&&game.Save.LastWriteSucceeded);
                    game.Resume();foreach(var e in UnityEngine.Object.FindObjectsByType<EnemyHealth>())e.TakeDamage(100);
                    due=Time.realtimeSinceStartup+.2f;break;
                case 10:
                    Check("all_progress_persists",new SaveService(GameSession.SavePath).Data.completedLevels==3);
                    File.Copy(GameSession.SavePath,"Submission_Backup/save.example.json",true);
                    game.ResetProgress();Check("reset_returns_home_and_locks",game.UI.CurrentScreen=="Home"&&!game.Save.IsUnlocked(2));
                    File.WriteAllLines("Submission_Backup/task11-play-checks.txt",results);
                    Debug.Log("TASK11_PLAY_CHECKS_"+(success?"PASS":"FAILED")+"\n"+string.Join("\n",results));
                    EditorApplication.update-=Tick;EditorApplication.isPlaying=false;break;
            }
        } catch(Exception error) {
            success=false;Debug.LogException(error);File.WriteAllText("Submission_Backup/task11-play-checks.txt",string.Join("\n",results)+"\n"+error);
            if(game&&game.player)game.player.GetComponent<PlayerSpriteAnimation>().stats.moveSpeed=originalSpeed;
            EditorApplication.update-=Tick;EditorApplication.isPlaying=false;
        }
    }
}
