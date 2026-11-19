using UnityEngine;
namespace TumanGame
{
    // Procedural squash/recoil around a fixed feet pivot preserves the original werewolf art.
    public sealed class EnemyMotion : MonoBehaviour
    {
        EnemyAI ai; Rigidbody2D body; Vector3 original; bool dead; float deathTime;
        public Sprite[] idleFrames,walkFrames,attackFrames,deathFrames;
        float clock;EnemyAI.State previous;
        void Awake(){ai=GetComponent<EnemyAI>();body=GetComponent<Rigidbody2D>();original=transform.localScale;}
        public void Die(){dead=true;deathTime=0;}
        void LateUpdate()
        {
            if(dead) {
                deathTime+=Time.unscaledDeltaTime;
                float duration=ai.stats?ai.stats.deathDuration:.5f;
                float p=Mathf.Clamp01(deathTime/Mathf.Max(.01f,duration));
                if(deathFrames!=null&&deathFrames.Length>0) {
                    GetComponent<SpriteRenderer>().sprite=deathFrames[Mathf.Min(deathFrames.Length-1,(int)(p*deathFrames.Length))];
                    if(p>=1)Destroy(gameObject);return;
                }
                transform.rotation=Quaternion.Euler(0,0,90*p);transform.localScale=original*(1-.6f*p);
                var sr=GetComponent<SpriteRenderer>();var c=sr.color;c.a=1-p;sr.color=c;
                if(p>=1)Destroy(gameObject);return;
            }
            bool walking=body.linearVelocity.sqrMagnitude>.01f;
            if(idleFrames!=null&&idleFrames.Length>0&&ai.stats) {
                if(previous!=ai.CurrentState){clock=0;previous=ai.CurrentState;}
                clock+=Time.deltaTime;
                var frames=ai.CurrentState==EnemyAI.State.Attack?attackFrames:walking?walkFrames:idleFrames;
                float fps=ai.CurrentState==EnemyAI.State.Attack?frames.Length/Mathf.Max(.05f,ai.stats.attackWindup+ai.stats.attackCooldown):
                    walking?(ai.CurrentState==EnemyAI.State.Chase?ai.stats.runFps:ai.stats.walkFps):ai.stats.idleFps;
                if(frames!=null&&frames.Length>0)GetComponent<SpriteRenderer>().sprite=frames[(int)(clock*fps)%frames.Length];
                return;
            }
            float wave=walking?Mathf.Sin(Time.time*(ai.CurrentState==EnemyAI.State.Chase?15:9))*.035f:Mathf.Sin(Time.time*3)*.012f;
            if(ai.CurrentState==EnemyAI.State.Attack)wave=Mathf.Sin(Time.time*18)*.05f;
            transform.localScale=Vector3.Scale(original,new Vector3(1-wave,1+wave,1));
        }
    }
}
