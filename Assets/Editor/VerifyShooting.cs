using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

[InitializeOnLoad]
public static class VerifyShooting
{
    private const string Request = "Submission_Backup/shooting-check.request";
    private static int phase;
    private static float deadline;
    private static EnemyHealth health;
    private static Rigidbody2D enemy, player;
    private static PlayerShooting shooting;
    private static Button fire;
    private static bool wall, first, second, third, cooldown, deadPlayer;

    static VerifyShooting()
    {
        EditorApplication.delayCall += Bootstrap;
        EditorApplication.playModeStateChanged += OnPlay;
    }
    private static void Bootstrap()
    {
        if (!File.Exists(Request)) return;
        if (EditorApplication.isPlaying) { EditorApplication.isPlaying = false; return; }
        ConfigureShooting.Configure();
        File.Delete(Request);
        SessionState.SetBool("Tuman.VerifyShooting", true);
        EditorApplication.isPlaying = true;
    }
    private static void OnPlay(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.EnteredEditMode && File.Exists(Request)) EditorApplication.delayCall += Bootstrap;
        if (state == PlayModeStateChange.EnteredPlayMode && SessionState.GetBool("Tuman.VerifyShooting", false))
        {
            SessionState.SetBool("Tuman.VerifyShooting", false);
            EditorApplication.delayCall += Run;
        }
    }
    [MenuItem("Tuman/Verify Shooting (Play Mode)")]
    public static void Run()
    {
        if (!EditorApplication.isPlaying) return;
        health = Object.FindAnyObjectByType<EnemyHealth>();
        shooting = Object.FindAnyObjectByType<PlayerShooting>();
        fire = GameObject.Find("Mobile Canvas/Safe Area/Fire Button").GetComponent<Button>();
        enemy = health.GetComponent<Rigidbody2D>(); player = shooting.GetComponent<Rigidbody2D>();
        health.GetComponent<EnemyAI>().enabled = false;
        wall = first = second = third = cooldown = deadPlayer = false;
        Place(new Vector2(-2, 2), new Vector2(2, 2));
        fire.onClick.Invoke();
        int shots = shooting.ShotsFired; fire.onClick.Invoke();
        cooldown = shooting.ShotsFired == shots && shots == 1;
        phase = 0; deadline = Time.time + .8f;
        EditorApplication.update -= Tick; EditorApplication.update += Tick;
    }
    private static void Place(Vector2 e, Vector2 p)
    {
        enemy.position = e; player.position = p;
        enemy.linearVelocity = player.linearVelocity = Vector2.zero;
        Physics2D.SyncTransforms();
    }
    private static void Tick()
    {
        if (!EditorApplication.isPlaying) { EditorApplication.update -= Tick; return; }
        if (Time.time < deadline) return;
        switch (phase++)
        {
            case 0:
                wall = health.CurrentHealth == 3;
                Place(new Vector2(3, 4), new Vector2(6, 4));
                fire.onClick.Invoke(); deadline = Time.time + .6f; break;
            case 1:
                first = health && health.CurrentHealth == 2;
                fire.onClick.Invoke(); deadline = Time.time + .6f; break;
            case 2:
                second = health && health.CurrentHealth == 1;
                fire.onClick.Invoke(); deadline = Time.time + .6f; break;
            case 3:
                third = !health;
                shooting.GetComponent<PlayerHealth>().TakeDamage(99);
                int shots = shooting.ShotsFired; fire.onClick.Invoke();
                deadPlayer = shots == shooting.ShotsFired;
                string report = $"wall_blocks_projectile={wall}\nfirst_hit_leaves_2_hp={first}\nsecond_hit_leaves_1_hp={second}\nthird_hit_destroys_enemy={third}\nrapid_tap_cooldown={cooldown}\ndead_player_cannot_fire={deadPlayer}\n";
                File.WriteAllText("Submission_Backup/shooting-verification.txt", report);
                if (wall && first && second && third && cooldown && deadPlayer) Debug.Log("Shooting verification PASS\n" + report);
                else Debug.LogError("Shooting verification FAILED\n" + report);
                EditorApplication.update -= Tick;
                EditorApplication.isPlaying = false;
                break;
        }
    }
}
