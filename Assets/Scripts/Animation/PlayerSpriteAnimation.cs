using UnityEngine;
namespace TumanGame
{
    [RequireComponent(typeof(SpriteRenderer),typeof(Rigidbody2D))]
    public sealed class PlayerSpriteAnimation : MonoBehaviour
    {
        public AnimationLibrary library;
        public PlayerStats stats;
        SpriteRenderer renderer2d; Rigidbody2D body; PlayerHealth health; PlayerShooting shooting;
        int direction, frame, previousState=-1; float clock,deathClock;
        public string CurrentAnimation { get; private set; }
        void Awake(){renderer2d=GetComponent<SpriteRenderer>();body=GetComponent<Rigidbody2D>();health=GetComponent<PlayerHealth>();shooting=GetComponent<PlayerShooting>();}
        void LateUpdate()
        {
            if(!library||!stats||library.directions==null||library.directions.Length<4)return;
            if(!health.IsAlive) {
                deathClock+=Time.deltaTime;
                transform.rotation=Quaternion.Euler(0,0,Mathf.Lerp(0,80,deathClock/stats.deathDuration));
                CurrentAnimation="Death";return;
            }
            if(deathClock>0){deathClock=0;transform.rotation=Quaternion.identity;}
            bool moving=body.linearVelocity.sqrMagnitude>.01f;
            bool firing=Time.time-shooting.LastShotTime<stats.shootPoseDuration;
            Vector2 aim=firing?shooting.AimDirection:body.linearVelocity;
            int oldDirection=direction;
            if(aim.sqrMagnitude>.01f)direction=Mathf.Abs(aim.x)>Mathf.Abs(aim.y)?(aim.x<0?2:3):(aim.y>0?1:0);
            int state=firing?(moving?3:2):(moving?1:0);
            if(previousState!=state||oldDirection!=direction){clock=0;frame=0;previousState=state;}
            var d=library.directions[direction];
            Sprite[] frames=state==3?d.walkShoot:state==2?d.idleShoot:state==1?d.walk:d.idle;
            if(frames==null||frames.Length==0)frames=d.idle;
            if(frames==null||frames.Length==0)return;
            float fps=firing?stats.shootingFps:moving?stats.walkFps:stats.idleFps;
            clock+=Time.deltaTime; frame=(int)(clock*fps)%frames.Length;
            renderer2d.sprite=frames[frame];
            CurrentAnimation=(state==3?"WalkShoot":state==2?"IdleShoot":state==1?"Walk":"Idle")+" / "+direction;
        }
    }
}
