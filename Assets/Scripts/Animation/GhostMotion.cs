using UnityEngine;
namespace TumanGame
{
    public sealed class GhostMotion:MonoBehaviour
    {
        public Transform visual;
        Vector3 rest,scale;EnemyAI ai;SpriteRenderer image;bool dying;float elapsed;
        void Awake(){ai=GetComponent<EnemyAI>();rest=visual.localPosition;scale=visual.localScale;image=visual.GetComponent<SpriteRenderer>();}
        public void Die(){dying=true;elapsed=0;}
        void LateUpdate()
        {
            if(dying) {
                elapsed+=Time.unscaledDeltaTime;float t=Mathf.Clamp01(elapsed/ai.stats.deathDuration);
                visual.localScale=scale*(1-t*.7f);image.color=new Color(.6f,1,1,1-t);
                if(t>=1)Destroy(gameObject);return;
            }
            visual.localPosition=rest+Vector3.up*(Mathf.Sin(Time.time*3)*.08f);
            image.color=ai.CurrentState==EnemyAI.State.Attack?new Color(.7f,1,1):Color.white;
            visual.localScale=scale*(1+Mathf.Sin(Time.time*4)*.025f);
        }
    }
}
