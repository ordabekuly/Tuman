using UnityEngine;
namespace TumanGame
{
    public sealed class VillageAtmosphere : MonoBehaviour
    {
        public Transform[] trees, ripples, mist;
        public SpriteRenderer[] glows;
        Vector3[] treeScale,waterOrigin,mistOrigin;
        void Awake()
        {
            treeScale=new Vector3[trees.Length];waterOrigin=new Vector3[ripples.Length];mistOrigin=new Vector3[mist.Length];
            for(int i=0;i<trees.Length;i++)treeScale[i]=trees[i].localScale;
            for(int i=0;i<ripples.Length;i++)waterOrigin[i]=ripples[i].position;
            for(int i=0;i<mist.Length;i++)mistOrigin[i]=mist[i].position;
        }
        void Update()
        {
            float t=Time.time;
            for(int i=0;i<trees.Length;i++)if(trees[i])trees[i].localScale=Vector3.Scale(treeScale[i],new Vector3(1+Mathf.Sin(t*.9f+i*1.7f)*.008f,1,1));
            for(int i=0;i<ripples.Length;i++)if(ripples[i])ripples[i].position=waterOrigin[i]+new Vector3(Mathf.Sin(t*.65f+i)*.14f,-Mathf.Repeat(t*.22f+i*.37f,1.8f),0);
            for(int i=0;i<mist.Length;i++)if(mist[i])mist[i].position=mistOrigin[i]+new Vector3(Mathf.Sin(t*.045f+i)*2,Mathf.Cos(t*.035f+i)*.5f,0);
            for(int i=0;i<glows.Length;i++)if(glows[i]){var c=glows[i].color;c.a=.12f+Mathf.Sin(t*2.3f+i)*.015f;glows[i].color=c;}
        }
    }
}
