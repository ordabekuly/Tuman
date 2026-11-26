using System;
using UnityEngine;
using UnityEngine.Audio;
namespace TumanGame
{
    [CreateAssetMenu(menuName="Tuman/Sound Library")]
    public sealed class SoundLibrary : ScriptableObject
    {
        [Serializable] public sealed class Cue
        {
            public SoundEvent kind;
            public AudioClip[] clips;
            [Range(0,1)] public float gain=.6f;
            [NonSerialized] public int last=-1;
        }
        public AudioMixer mixer;
        public AudioMixerGroup tracks,sfx;
        public AudioMixerSnapshot normal,paused;
        public AudioClip menuMusic,levelMusic;
        public Cue[] cues;
        public Cue Find(SoundEvent kind)=>Array.Find(cues,c=>c.kind==kind);
    }
}
