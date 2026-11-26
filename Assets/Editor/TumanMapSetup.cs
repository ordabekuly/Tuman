using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Tuman.Editor
{
    public static class TumanMapSetup
    {
        [MenuItem("Tuman/Create Gameplay Scene")]
        public static void CreateScene()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var cameraObject = new GameObject("Main Camera", typeof(Camera));
            cameraObject.tag = "MainCamera";
            cameraObject.transform.position = new Vector3(0, 0, -10);
            var camera = cameraObject.GetComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 9;
            camera.backgroundColor = new Color(0.08f, 0.13f, 0.10f);
            var grid = new GameObject("CourtyardGrid", typeof(Grid), typeof(CourtyardMap));
            var ground = Layer(grid, "Ground", 0, false);
            var roads = Layer(grid, "Roads", 1, false);
            var obstacles = Layer(grid, "Obstacles", 2, true);
            Layer(grid, "Decorations", 3, false);
            var settings = new SerializedObject(grid.GetComponent<CourtyardMap>());
            settings.FindProperty("ground").objectReferenceValue = ground;
            settings.FindProperty("roads").objectReferenceValue = roads;
            settings.FindProperty("obstacles").objectReferenceValue = obstacles;
            settings.ApplyModifiedPropertiesWithoutUndo();
            EditorSceneManager.SaveScene(scene, "Assets/Scenes/Gameplay.unity");
            Selection.activeGameObject = grid;
        }

        static Tilemap Layer(GameObject grid, string name, int order, bool solid)
        {
            var obj = new GameObject(name, typeof(Tilemap), typeof(TilemapRenderer));
            obj.transform.SetParent(grid.transform, false);
            obj.GetComponent<TilemapRenderer>().sortingOrder = order;
            if (solid)
            {
                obj.AddComponent<TilemapCollider2D>();
                var body = obj.AddComponent<Rigidbody2D>();
                body.bodyType = RigidbodyType2D.Static;
            }
            return obj.GetComponent<Tilemap>();
        }
    }
}
