using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using TumanGame;
using Object=UnityEngine.Object;

[InitializeOnLoad]
public static class VerifySoundPass
{
 const string Flag="Tuman.SoundTest";
 static List<string> report=new List<string>();
 static int phase,hp,windups;static double due;static bool failed;
 static GameSession game;static GameAudio audio;static EnemyAI wolf;
 static AudioSource level;static int managerId,samples;
 static VerifySoundPass(){EditorApplication.playModeStateChanged+=State;}
 public static void Run()
 {
  SessionState.SetBool(Flag+".saveExists",File.Exists(GameSession.SavePath));
  SessionState.SetString(Flag+".save",File.Exists(GameSession.SavePath)?File.ReadAllText(GameSession.SavePath):"");
  SessionState.SetBool(Flag,true);EditorSceneManager.OpenScene("Assets/Scenes/TumanVillageDraft.unity");EditorApplication.isPlaying=true;
 }
 static void State(PlayModeStateChange s)
 {
  if(!SessionState.GetBool(Flag,false))return;
  if(s==PlayModeStateChange.EnteredPlayMode){phase=0;due=EditorApplication.timeSinceStartup+1.5;EditorApplication.update+=Tick;}
  if(s==PlayModeStateChange.EnteredEditMode)
  {
   if(SessionState.GetBool(Flag+".saveExists",false))File.WriteAllText(GameSession.SavePath,SessionState.GetString(Flag+".save",""));
   else if(File.Exists(GameSession.SavePath))File.Delete(GameSession.SavePath);
   SessionState.SetBool(Flag,false);File.WriteAllLines("Submission_Task12/sound-checks.txt",report);EditorApplication.Exit(failed?1:0);
  }
 }
 static void Check(string n,bool ok){report.Add((ok?"PASS ":"FAIL ")+n);if(!ok)throw new Exception(n);}
 static int Count(SoundEvent e)=>audio.EventCounts.TryGetValue(e,out int n)?n:0;
 static void Tick()
 {
  if(EditorApplication.timeSinceStartup<due)return;due=EditorApplication.timeSinceStartup+.2;
  try
  {
   switch(phase++)
   {
    case 0:
     game=GameSession.Instance;audio=game.Sound;managerId=audio.GetInstanceID();
     Check("one persistent audio owner",Object.FindObjectsByType<GameAudio>().Length==1);
     Check("exactly one active listener",Object.FindObjectsByType<AudioListener>().Count(a=>a.enabled)==1);
     Check("all 18 audio sources routed",audio.GetComponentsInChildren<AudioSource>().Length==18&&audio.GetComponentsInChildren<AudioSource>().All(a=>a.outputAudioMixerGroup&&a.outputAudioMixerGroup.audioMixer==audio.Mixer));
     Check("11 event cues use imported clips",audio.Library.cues.Length==11&&audio.Library.cues.All(c=>c.clips.Length>0&&c.clips.All(x=>x)));
     Check("two distinct music loops",audio.Library.menuMusic&&audio.Library.levelMusic&&audio.Library.menuMusic!=audio.Library.levelMusic);
     game.SetMusic(.5f);game.SetEffects(.25f);
     Check("Music 50 percent is -6.02dB",audio.Mixer.GetFloat("MusicVol",out float mv)&&Mathf.Abs(mv+6.0206f)<.01f);
     Check("SFX 25 percent is -12.04dB",audio.Mixer.GetFloat("SFXVol",out float sv)&&Mathf.Abs(sv+12.0412f)<.01f);
     var saved=new SaveService(GameSession.SavePath);Check("saved volumes survive disk reload",saved.Data.musicVolume==.5f&&saved.Data.effectsVolume==.25f);
     game.SetMusic(0);Check("zero volume is finite -80dB",audio.Mixer.GetFloat("MusicVol",out mv)&&mv==-80);game.SetMusic(.55f);
     game.StartLevel(0);foreach(var ai in Object.FindObjectsByType<EnemyAI>())ai.enabled=false;
     level=audio.GetComponentsInChildren<AudioSource>().Single(a=>a.name=="Level music");
     due=EditorApplication.timeSinceStartup+.3;break;
    case 1:
     var menu=audio.GetComponentsInChildren<AudioSource>().Single(a=>a.name=="Menu music");
     Check("crossfade overlaps both tracks",menu.isPlaying&&level.isPlaying&&menu.volume>0&&level.volume>0);
     due=EditorApplication.timeSinceStartup+1;break;
    case 2:
     Check("crossfade finishes on level track",level.isPlaying&&audio.GetComponentsInChildren<AudioSource>().Single(a=>a.name=="Menu music").volume==0);
     samples=level.timeSamples;game.Retry();foreach(var ai in Object.FindObjectsByType<EnemyAI>())ai.enabled=false;
     Check("Retry keeps music playback position",level.timeSamples>=samples&&audio.GetInstanceID()==managerId);
     game.Pause();Check("Paused snapshot requested",audio.Paused&&Time.timeScale==0);
     due=EditorApplication.timeSinceStartup+.5;break;
    case 3:
     Check("pause transition uses real time",audio.Mixer.updateMode==UnityEngine.Audio.AudioMixerUpdateMode.UnscaledTime);
     Check("pause preserves user fader",audio.Mixer.GetFloat("MusicVol",out float v)&&Mathf.Abs(v-GameAudio.Decibels(.55f))<.01f);
     game.Resume();game.player.transform.position=new Vector3(-20,5);game.player.GetComponent<Rigidbody2D>().position=new Vector2(-20,5);
     wolf=Object.FindObjectsByType<EnemyAI>().First();wolf.transform.position=new Vector3(-19.4f,5);wolf.GetComponent<Rigidbody2D>().position=new Vector2(-19.4f,5);
     hp=game.player.CurrentHealth;windups=Count(SoundEvent.WolfWindup);wolf.enabled=true;Physics2D.SyncTransforms();
     due=EditorApplication.timeSinceStartup+.08;break;
    case 4:
     Check("real wolf windup sounds before damage",Count(SoundEvent.WolfWindup)>windups&&game.player.CurrentHealth==hp);
     due=EditorApplication.timeSinceStartup+1;break;
    case 5:
     Check("attack sound and damage occur after windup",Count(SoundEvent.EnemyAttack)>0&&game.player.CurrentHealth<hp);wolf.enabled=false;
     game.Sound.PlayShot();game.Collect(false);game.UI.ShowSettings();game.UI.MusicSlider.value=.4f;
     Check("real settings slider reaches mixer",audio.Mixer.GetFloat("MusicVol",out float mv2)&&Mathf.Abs(mv2-GameAudio.Decibels(.4f))<.01f);
     foreach(SoundEvent e in Enum.GetValues(typeof(SoundEvent)))audio.Play(e);
     Check("all event clips can play",Enum.GetValues(typeof(SoundEvent)).Cast<SoundEvent>().All(e=>Count(e)>0));
     Check("pitch stays within variation range",audio.GetComponentsInChildren<AudioSource>().Where(s=>s.name.StartsWith("SFX")).All(s=>s.pitch>=.92f&&s.pitch<=1.08f));
     game.ShowHome();
     UnityEngine.SceneManagement.SceneManager.LoadScene("TumanVillageDraft");due=EditorApplication.timeSinceStartup+1.5;break;
    case 6:
     Check("scene reload retains exactly one manager",Object.FindObjectsByType<GameAudio>().Length==1&&GameSession.Instance.Sound.GetInstanceID()==managerId);
     Check("launch reapplies saved volume",audio.Mixer.GetFloat("MusicVol",out float mv3)&&Mathf.Abs(mv3-GameAudio.Decibels(.4f))<.01f);
     Finish();break;
   }
  }
  catch(Exception e){failed=true;report.Add(e.ToString());Debug.LogException(e);Finish();}
 }
 static void Finish(){EditorApplication.update-=Tick;EditorApplication.isPlaying=false;}
}
