using System;
using System.IO;
using UnityEngine;
namespace TumanGame
{
    [Serializable] public sealed class SaveData
    {
        public string format;
        public int version;
        public int completedLevels;
        public int bestScore;
        public int lastLevel;
        public int[] unlockedLevels;
        public float musicVolume, effectsVolume;
        public bool vibration;
        public static SaveData Defaults() => new SaveData {
            format="TUMAN", version=1, completedLevels=0, bestScore=0, lastLevel=1,
            unlockedLevels=new[]{1}, musicVolume=.55f, effectsVolume=.8f, vibration=true
        };
    }
    // Filesystem logic is isolated so corruption tests never touch the player's actual save.
    public sealed class SaveService
    {
        public readonly string Path;
        public SaveData Data { get; private set; }
        public string Status { get; private set; }
        public bool LastWriteSucceeded { get; private set; }
        public SaveService(string path) { Path=path; Load(); }
        public static bool Valid(SaveData d)
        {
            if(d==null||d.format!="TUMAN"||d.version!=1||d.bestScore<0||d.completedLevels<0||d.completedLevels>3||
               d.lastLevel<1||d.lastLevel>3||d.unlockedLevels==null||d.unlockedLevels.Length<1||d.unlockedLevels.Length>3||
               float.IsNaN(d.musicVolume)||float.IsNaN(d.effectsVolume)||d.musicVolume<0||d.musicVolume>1||d.effectsVolume<0||d.effectsVolume>1) return false;
            int expected=Math.Min(3,d.completedLevels+1);
            if(d.lastLevel>expected)return false;
            if(d.unlockedLevels.Length!=expected)return false;
            for(int i=0;i<expected;i++)if(d.unlockedLevels[i]!=i+1)return false;
            return true;
        }
        public void Load()
        {
            try {
                if(!File.Exists(Path)){Data=SaveData.Defaults();Status="New journey";return;}
                var info=new FileInfo(Path);
                if(info.Length>65536)throw new InvalidDataException("Oversized save");
                string json=File.ReadAllText(Path).Trim();
                if(!json.StartsWith("{")||!json.EndsWith("}"))throw new InvalidDataException("Invalid JSON envelope");
                var data=JsonUtility.FromJson<SaveData>(json);
                if(!Valid(data))throw new InvalidDataException("Invalid save fields");
                Data=data;Status="Journey restored";
            } catch(Exception e) when(e is InvalidDataException||e is IOException||e is UnauthorizedAccessException||e is ArgumentException||e is NotSupportedException) {
                Data=SaveData.Defaults();Status="Save unavailable - safe defaults loaded";
                Debug.LogWarning("Tuman save recovery: "+e.GetType().Name);
            }
        }
        public bool Save()
        {
            LastWriteSucceeded=false;
            try {
                if(!Valid(Data))throw new InvalidDataException("Refusing invalid data");
                Directory.CreateDirectory(System.IO.Path.GetDirectoryName(Path));
                string temp=Path+".tmp";
                File.WriteAllText(temp,JsonUtility.ToJson(Data,true));
                if(File.Exists(Path)) {
                    try{File.Replace(temp,Path,null);}
                    catch(PlatformNotSupportedException){File.Copy(temp,Path,true);File.Delete(temp);}
                    catch(IOException){File.Copy(temp,Path,true);File.Delete(temp);}
                } else File.Move(temp,Path);
                LastWriteSucceeded=true;Status="Progress saved";return true;
            } catch(Exception e) when(e is InvalidDataException||e is IOException||e is UnauthorizedAccessException||e is ArgumentException||e is NotSupportedException) {
                Status="Could not save - check device storage";Debug.LogWarning("Tuman save: "+e.GetType().Name);return false;
            }
        }
        public bool IsUnlocked(int level)=>Array.IndexOf(Data.unlockedLevels,level)>=0;
        public void Complete(int level,int score)
        {
            Data.completedLevels=Math.Max(Data.completedLevels,level);
            Data.bestScore=Math.Max(Data.bestScore,score);
            Data.lastLevel=Math.Min(3,level+1);
            int n=Math.Min(3,Data.completedLevels+1);Data.unlockedLevels=new int[n];
            for(int i=0;i<n;i++)Data.unlockedLevels[i]=i+1;
            Save();
        }
        public void Reset() { Data=SaveData.Defaults();Save(); }
    }
}
