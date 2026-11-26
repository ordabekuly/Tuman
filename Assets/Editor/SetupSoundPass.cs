using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.Audio;
using TumanGame;
using Object=UnityEngine.Object;

public static class SetupSoundPass
{
 const BindingFlags Flags=BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance|BindingFlags.Static;
 static Type Type(string n)=>typeof(Editor).Assembly.GetType("UnityEditor.Audio."+n,true);
 static object Call(object o,string n,params object[] a)=>o.GetType().GetMethods(Flags).First(m=>m.Name==n&&m.GetParameters().Length==a.Length).Invoke(o,a);
 static object Get(object o,string n)=>o.GetType().GetProperty(n,Flags).GetValue(o);
 static void Set(object o,string n,object v)=>o.GetType().GetProperty(n,Flags).SetValue(o,v);
 [MenuItem("Tuman/Setup Sound Pass")]
 public static void Run()
 {
  Directory.CreateDirectory("Assets/Resources");Directory.CreateDirectory("Assets/Audio");
  AssetDatabase.Refresh();
  foreach(string path in AssetDatabase.FindAssets("t:AudioClip",new[]{"Assets/Audio"}).Select(AssetDatabase.GUIDToAssetPath))
  {
   var importer=(AudioImporter)AssetImporter.GetAtPath(path);bool music=path.Contains("/Music/");
   importer.forceToMono=!music;var s=importer.defaultSampleSettings;
   s.loadType=music?AudioClipLoadType.Streaming:AudioClipLoadType.DecompressOnLoad;
   s.compressionFormat=music?AudioCompressionFormat.Vorbis:AudioCompressionFormat.ADPCM;s.quality=.7f;
   importer.defaultSampleSettings=s;importer.SaveAndReimport();
  }
  var mixer=AssetDatabase.LoadAssetAtPath<AudioMixer>("Assets/Audio/TumanMix.mixer");
  if(mixer && !mixer.FindSnapshot("Paused")){AssetDatabase.DeleteAsset("Assets/Audio/TumanMix.mixer");mixer=null;}
  if(!mixer)
  {
   var mc=Type("AudioMixerController").GetMethod("CreateMixerControllerAtPath",Flags).Invoke(null,new object[]{"Assets/Audio/TumanMix.mixer"});
   mixer=(AudioMixer)mc;var master=Get(mc,"masterGroup");
   var music=Group(mc,"Music",master);var tracks=Group(mc,"Tracks",music);var sfx=Group(mc,"SFX",master);
   var viewType=Type("MixerGroupView");var view=Activator.CreateInstance(viewType);
   viewType.GetField("name",Flags).SetValue(view,"All groups");
   var allGroups=new[]{master,music,tracks,sfx};var guids=Array.CreateInstance(Get(master,"groupID").GetType(),4);
   for(int i=0;i<4;i++)guids.SetValue(Get(allGroups[i],"groupID"),i);
   viewType.GetField("guids",Flags).SetValue(view,guids);
   var views=Array.CreateInstance(viewType,1);views.SetValue(view,0);Set(mc,"views",views);
   var normal=Get(mc,"TargetSnapshot");((Object)normal).name="Normal";
   Call(mc,"CloneNewSnapshotFromTarget",false);var paused=Get(mc,"TargetSnapshot");((Object)paused).name="Paused";
   Set(mc,"startSnapshot",normal);
   var effect=Activator.CreateInstance(Type("AudioMixerEffectController"),Flags,null,new object[]{"Lowpass Simple"},null);
   Call(effect,"PreallocateGUIDs");Call(mc,"AddNewSubAsset",effect,true);Call(tracks,"InsertEffect",effect,1);
   foreach(var snapshot in new[]{normal,paused})
   {
    bool p=snapshot==paused;
    Call(tracks,"SetValueForVolume",mc,snapshot,p?-6f:0f);
    Call(effect,"SetValueForParameter",mc,snapshot,"Cutoff freq",p?800f:22000f);
    Call(music,"SetValueForVolume",mc,snapshot,0f);Call(sfx,"SetValueForVolume",mc,snapshot,0f);
   }
   // Build exposed parameters from their stable GUIDs; runtime owns these faders.
   var ep=Type("ExposedAudioParameter");var exposed=Array.CreateInstance(ep,2);
   var groups=new[]{music,sfx};var names=new[]{"MusicVol","SFXVol"};
   for(int i=0;i<2;i++)
   {
    var item=Activator.CreateInstance(ep);ep.GetField("guid",Flags).SetValue(item,Call(groups[i],"GetGUIDForVolume"));
    ep.GetField("name",Flags).SetValue(item,names[i]);exposed.SetValue(item,i);
   }
   Set(mc,"exposedParameters",exposed);Set(mc,"TargetSnapshot",normal);
   foreach(var obj in AssetDatabase.LoadAllAssetsAtPath("Assets/Audio/TumanMix.mixer"))EditorUtility.SetDirty(obj);
  }
  mixer.updateMode=AudioMixerUpdateMode.UnscaledTime;
  var library=AssetDatabase.LoadAssetAtPath<SoundLibrary>("Assets/Resources/TumanSound.asset");
  if(!library){library=ScriptableObject.CreateInstance<SoundLibrary>();AssetDatabase.CreateAsset(library,"Assets/Resources/TumanSound.asset");}
  library.mixer=mixer;library.tracks=mixer.FindMatchingGroups("Tracks").Single();library.sfx=mixer.FindMatchingGroups("SFX").Single();
  library.normal=mixer.FindSnapshot("Normal");library.paused=mixer.FindSnapshot("Paused");
  library.menuMusic=Clip("Music/Menu.ogg");library.levelMusic=Clip("Music/Level.ogg");
  library.cues=new[]{Cue(SoundEvent.Footstep,.35f,"footstep00","footstep01","footstep02"),Cue(SoundEvent.Shot,.45f,"laser1","laser2"),
   Cue(SoundEvent.Hit,.65f,"chop"),Cue(SoundEvent.Pickup,.65f,"handleCoins","handleCoins2"),
   Cue(SoundEvent.WolfWindup,.85f,"creak1","creak2"),Cue(SoundEvent.GhostWindup,.6f,"phaserUp1"),
   Cue(SoundEvent.EnemyAttack,.55f,"knifeSlice","knifeSlice2"),Cue(SoundEvent.Death,.7f,"lowDown"),
   Cue(SoundEvent.Click,.4f,"click_001","click_002"),Cue(SoundEvent.Win,.7f,"confirmation_002"),Cue(SoundEvent.Lose,.65f,"error_004")};
  EditorUtility.SetDirty(library);AssetDatabase.SaveAssets();
  Debug.Log("SOUND_SETUP_PASS: Music/Tracks and SFX, Normal/Paused, 11 sound events and 2 tracks.");
 }
 static object Group(object mixer,string name,object parent)
 {
  var g=Call(mixer,"CreateNewGroup",name,true);Call(mixer,"AddChildToParent",g,parent);return g;
 }
 static AudioClip Clip(string p)=>AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/"+p)??throw new Exception("Missing clip "+p);
 static SoundLibrary.Cue Cue(SoundEvent kind,float gain,params string[] files)=>new SoundLibrary.Cue{kind=kind,gain=gain,clips=files.Select(n=>Clip("SFX/"+n+".ogg")).ToArray()};
}
