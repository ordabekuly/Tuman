using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class VerifyEnemyAttack
{
    static EnemyAI ai;
    static PlayerHealth health;
    static Rigidbody2D enemy,player;
    static int phase;
    static float deadline;
    static bool windup,hit,cooldown,blocked,death,restart;
    static int afterHit;
    [MenuItem("Tuman/Verify Enemy Attack (Play Mode)")]
    public static void Run()
    {
        if(!EditorApplication.isPlaying)return;
        ai=GameObject.Find("Enemy").GetComponent<EnemyAI>();
        health=GameObject.Find("Player").GetComponent<PlayerHealth>();
        enemy=ai.GetComponent<Rigidbody2D>();player=health.GetComponent<Rigidbody2D>();
        if(health.CurrentHealth!=3){Debug.LogError("Run attack check in a fresh Play session.");return;}
        EditorApplication.update-=Tick;
        windup=hit=cooldown=blocked=death=restart=false;
        phase=0;Place(new Vector2(3,4),new Vector2(4.65f,4));
        deadline=Time.time+.2f;EditorApplication.update+=Tick;
    }
    static void Place(Vector2 e,Vector2 p)
    {
        enemy.position=e;enemy.linearVelocity=Vector2.zero;
        player.position=p;player.linearVelocity=Vector2.zero;Physics2D.SyncTransforms();
    }
    static void Tick()
    {
        if(!EditorApplication.isPlaying){EditorApplication.update-=Tick;return;}
        if(phase==5)
        {
            var obj=GameObject.Find("Player");
            if(!obj||obj.GetComponent<PlayerHealth>()==health)return;
            restart=obj.GetComponent<PlayerHealth>().CurrentHealth==3&&obj.GetComponent<PlayerMovement>().enabled;
            EditorApplication.update-=Tick;
            string report=$"windup_before_damage={windup}\nattack_deals_one_damage={hit}\ncooldown_prevents_spam={cooldown}\nwall_prevents_damage={blocked}\ndeath_stops_movement={death}\nrestart_restores_player={restart}\n";
            Directory.CreateDirectory("Submission_Backup");
            File.WriteAllText("Submission_Backup/attack-verification.txt",report);
            if(windup&&hit&&cooldown&&blocked&&death&&restart)Debug.Log("Attack verification PASS\n"+report);
            else Debug.LogError("Attack verification FAILED\n"+report);
            return;
        }
        if(!health||!ai){EditorApplication.update-=Tick;return;}
        if(Time.time<deadline)return;
        switch(phase)
        {
            case 0:
                windup=health.CurrentHealth==3&&ai.CurrentState==EnemyAI.State.Attack;
                phase=1;deadline=Time.time+.5f;break;
            case 1:
                hit=health.CurrentHealth==2;afterHit=health.CurrentHealth;
                phase=2;deadline=Time.time+.3f;break;
            case 2:
                cooldown=health.CurrentHealth==afterHit;
                Place(new Vector2(-2,2),new Vector2(2,2));
                phase=3;deadline=Time.time+2;break;
            case 3:
                blocked=health.CurrentHealth==afterHit&&!ai.CanSeePlayer();
                ai.enabled=false;health.TakeDamage(99);
                death=!health.IsAlive&&!health.GetComponent<PlayerMovement>().enabled&&player.linearVelocity==Vector2.zero;
                phase=4;deadline=Time.time+.1f;break;
            case 4:
                phase=5;SceneManager.LoadScene(SceneManager.GetActiveScene().path);break;
        }
    }
}
