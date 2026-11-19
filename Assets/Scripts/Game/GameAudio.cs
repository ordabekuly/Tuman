using UnityEngine;
namespace TumanGame
{
    public sealed class GameAudio:MonoBehaviour
    {
        AudioSource music,effects;AudioClip shot,pickup;
        public void Apply(SaveData data)
        {
            if(!music){music=gameObject.AddComponent<AudioSource>();music.loop=true;music.clip=Tone(110,2,true);music.Play();
                effects=gameObject.AddComponent<AudioSource>();shot=Tone(650,.12f,false);pickup=Tone(880,.18f,false);}
            music.volume=data.musicVolume*.14f;effects.volume=data.effectsVolume*.45f;
        }
        public void PlayShot(){effects.PlayOneShot(shot);}
        public void PlayPickup(){effects.PlayOneShot(pickup);}
        static AudioClip Tone(float hz,float seconds,bool ambient)
        {
            const int sampleRate=22050;int n=(int)(sampleRate*seconds);var data=new float[n];
            for(int i=0;i<n;i++) {float t=i/(float)sampleRate;float envelope=ambient?.6f:Mathf.Sin(Mathf.PI*i/n)*(1-i/(float)n);
                data[i]=Mathf.Sin(2*Mathf.PI*hz*t)*envelope*.2f+ (ambient?Mathf.Sin(2*Mathf.PI*hz*1.5f*t)*.06f:0);}
            var clip=AudioClip.Create("Original procedural tone",n,1,sampleRate,false);clip.SetData(data,0);return clip;
        }
    }
}
