using System.IO;
using UnityEditor;
using UnityEngine;

public static class VerifyEnemyVision
{
    static EnemyAI ai;
    static Rigidbody2D enemy, player;
    static int phase;
    static float deadline;
    static bool visible, pursued, blocked, returned;

    [MenuItem("Tuman/Verify Enemy Vision (Play Mode)")]
    public static void Run()
    {
        if (!EditorApplication.isPlaying) { Debug.LogWarning("Enter Play mode first."); return; }
        ai=GameObject.Find("Enemy").GetComponent<EnemyAI>();
        enemy=ai.GetComponent<Rigidbody2D>();
        player=GameObject.Find("Player").GetComponent<Rigidbody2D>();
        EditorApplication.update-=Tick;
        phase=0;visible=pursued=blocked=returned=false;
        Place(new Vector2(3,4),new Vector2(6,4));
        deadline=Time.time+1;
        EditorApplication.update+=Tick;
    }
    static void Place(Vector2 e,Vector2 p)
    {
        enemy.position=e;enemy.linearVelocity=Vector2.zero;
        player.position=p;player.linearVelocity=Vector2.zero;
        Physics2D.SyncTransforms();
    }
    static void Tick()
    {
        if (!EditorApplication.isPlaying || !enemy || !player || !ai)
        { EditorApplication.update-=Tick;return; }
        if(Time.time<deadline)return;
        if(phase==0)
        {
            visible=ai.CanSeePlayer()&&(ai.CurrentState==EnemyAI.State.Chase||ai.CurrentState==EnemyAI.State.Attack);
            pursued=enemy.position.x>3.2f;
            Place(new Vector2(-2,2),new Vector2(2,2));
            phase=1;deadline=Time.time+.1f;
        }
        else if(phase==1)
        {
            blocked=!ai.CanSeePlayer();
            Place(new Vector2(3,4),new Vector2(-8,-5));
            phase=2;deadline=Time.time+2;
        }
        else
        {
            returned=ai.CurrentState==EnemyAI.State.Patrol;
            EditorApplication.update-=Tick;
            Directory.CreateDirectory("Submission_Backup");
            string report=$"visible_chase={visible}\npursuit_movement={pursued}\nwall_blocks_raycast={blocked}\nreturn_to_patrol={returned}\n";
            File.WriteAllText("Submission_Backup/vision-verification.txt",report);
            if(visible&&pursued&&blocked&&returned)Debug.Log("Enemy vision verification PASS\n"+report);
            else Debug.LogError("Enemy vision verification FAILED\n"+report);
        }
    }
}
