using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class ConfigureEnemyPatrol
{
    [MenuItem("Tuman/Configure Enemy Patrol")]
    public static void Configure()
    {
        if (EditorApplication.isPlaying) return;
        var scene = EditorSceneManager.GetActiveScene();
        if (scene.path != "Assets/Scenes/Courtyard.unity")
        {
            Debug.LogError("Open Courtyard before configuring patrol.");
            return;
        }
        GameObject enemy = GameObject.Find("Enemy");
        var playerObject = GameObject.Find("Player");
        GameObject a = GameObject.Find("Patrol A");
        GameObject b = GameObject.Find("Patrol B");
        if (!enemy || !a || !b)
        {
            Debug.LogError("Enemy, Patrol A and Patrol B are required.");
            return;
        }
        if (playerObject && !playerObject.GetComponent<PlayerHealth>())
            Undo.AddComponent<PlayerHealth>(playerObject);
        var buildScenes = new System.Collections.Generic.List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        int sceneIndex = buildScenes.FindIndex(s => s.path == scene.path);
        if (sceneIndex < 0) buildScenes.Add(new EditorBuildSettingsScene(scene.path, true));
        else buildScenes[sceneIndex].enabled = true;
        EditorBuildSettings.scenes = buildScenes.ToArray();
        Directory.CreateDirectory("Submission_Backup");
        File.Copy(scene.path, "Submission_Backup/Courtyard.before-patrol.unity", true);
        Undo.RecordObject(enemy, "Configure enemy patrol");
        var body = enemy.GetComponent<Rigidbody2D>();
        if (!body) body = Undo.AddComponent<Rigidbody2D>(enemy);
        Undo.RecordObject(body, "Configure enemy body");
        body.bodyType = RigidbodyType2D.Dynamic;
        body.gravityScale = 0;
        body.constraints = RigidbodyConstraints2D.FreezeRotation;
        body.interpolation = RigidbodyInterpolation2D.Interpolate;
        var ai = enemy.GetComponent<EnemyAI>();
        if (!ai) ai = Undo.AddComponent<EnemyAI>(enemy);
        var settings = new SerializedObject(ai);
        var points = settings.FindProperty("patrolPoints");
        points.arraySize = 2;
        points.GetArrayElementAtIndex(0).objectReferenceValue = a.transform;
        points.GetArrayElementAtIndex(1).objectReferenceValue = b.transform;
        settings.FindProperty("patrolSpeed").floatValue = 2;
        settings.FindProperty("arrivalDistance").floatValue = .15f;
        settings.FindProperty("waitTime").floatValue = 1;
        settings.FindProperty("player").objectReferenceValue = GameObject.Find("Player")?.transform;
        settings.FindProperty("detectionDistance").floatValue = 6;
        settings.FindProperty("chaseSpeed").floatValue = 3;
        settings.FindProperty("stopDistance").floatValue = 1.5f;
        settings.FindProperty("lostSightTime").floatValue = 1.5f;
        settings.ApplyModifiedProperties();
        EditorUtility.SetDirty(ai);
        EditorUtility.SetDirty(body);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Selection.activeGameObject = enemy;
        Debug.Log("Enemy patrol and raycast chase configured, Player assigned.");
    }
}
