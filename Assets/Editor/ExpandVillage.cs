using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Tilemaps;
using TumanGame;
public static class ExpandVillage
{
    public static void Run()
    {
        EditorSceneManager.OpenScene("Assets/Scenes/Tuman11.unity");
        var session=Object.FindAnyObjectByType<GameSession>();
        var ghost=AssetDatabase.LoadAssetAtPath<EnemyStats>("Assets/Data/GhostStats.asset");
        if(!ghost){ghost=ScriptableObject.CreateInstance<EnemyStats>();AssetDatabase.CreateAsset(ghost,"Assets/Data/GhostStats.asset");}
        ghost.rangedAttack=true;ghost.maxHealth=3;ghost.damage=1;ghost.patrolSpeed=1;ghost.chaseSpeed=1.6f;
        ghost.visionRange=10;ghost.attackRange=7;ghost.stopDistance=5;ghost.retreatDistance=3;
        ghost.arrivalDistance=.15f;ghost.patrolWait=1;ghost.lostSightTime=2;
        ghost.attackWindup=.8f;ghost.attackCooldown=1.6f;ghost.recoveryTime=.3f;ghost.deathDuration=.7f;
        ghost.projectileSpeed=4.5f;ghost.projectileRange=12;ghost.projectileRadius=.12f;
        ghost.idleFps=5;ghost.walkFps=8;ghost.runFps=10;EditorUtility.SetDirty(ghost);
        var importer=(TextureImporter)AssetImporter.GetAtPath("Assets/Art/Enemies/Ghost.png");
        importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Single;
        importer.spritePixelsPerUnit=600;importer.filterMode=FilterMode.Point;importer.textureCompression=TextureImporterCompression.Uncompressed;
        importer.maxTextureSize=2048;importer.SaveAndReimport();
        var root=new GameObject("Ghost");root.SetActive(false);
        var body=root.AddComponent<Rigidbody2D>();body.gravityScale=0;body.freezeRotation=true;body.interpolation=RigidbodyInterpolation2D.Interpolate;
        var collider=root.AddComponent<BoxCollider2D>();collider.size=new Vector2(.65f,.45f);collider.offset=new Vector2(0,-.5f);
        root.AddComponent<EnemyAI>().Configure(null,System.Array.Empty<Transform>(),ghost);
        root.AddComponent<EnemyHealth>().stats=ghost;
        var visual=new GameObject("Ghost Visual",typeof(SpriteRenderer));visual.transform.SetParent(root.transform,false);
        var sprite=visual.GetComponent<SpriteRenderer>();sprite.sprite=AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Enemies/Ghost.png");sprite.sortingOrder=5;
        root.AddComponent<GhostMotion>().visual=visual.transform;
        session.ghostTemplate=PrefabUtility.SaveAsPrefabAsset(root,"Assets/Prefabs/Ghost.prefab");Object.DestroyImmediate(root);
        for(int i=0;i<3;i++) {
            var level=session.levels[i];level.playerSpawn=new Vector2(-10,-3);level.ghostStats=ghost;level.goal="Clear all creatures";
            if(i==0){level.enemySpawns=new[]{new Vector2(-4,-4),new Vector2(-5,3)};level.ghostSpawns=System.Array.Empty<Vector2>();}
            if(i==1){level.enemySpawns=new[]{new Vector2(-5,3),new Vector2(9,-4)};level.ghostSpawns=new[]{new Vector2(10,3)};}
            if(i==2){level.enemySpawns=new[]{new Vector2(-5,3),new Vector2(9,-4)};level.ghostSpawns=new[]{new Vector2(9,3),new Vector2(12,0)};}
            level.herbSpawns=new[]{new Vector2(-7,-5),new Vector2(5,-4)};
            level.tokenSpawns=new[]{new Vector2(-7,-2),new Vector2(5,-2),new Vector2(12,-5)};
            level.description=i==0?"Reclaim the western courtyard.":i==1?"Cross the gate. Dodge the ghost's light.":"Two courtyards. Wolves and ghosts together.";
            EditorUtility.SetDirty(level);
        }
        var maps=Object.FindObjectsByType<Tilemap>();
        var ground=maps.First(m=>m.name=="Ground");var roads=maps.First(m=>m.name=="Roads");var walls=maps.First(m=>m.name.StartsWith("Obstacles"));
        var grassA=AssetDatabase.LoadAssetAtPath<Tile>("Assets/Art/Tiles/GrassA.asset");var grassB=AssetDatabase.LoadAssetAtPath<Tile>("Assets/Art/Tiles/GrassB.asset");
        var dirt=AssetDatabase.LoadAssetAtPath<Tile>("Assets/Art/Tiles/Dirt.asset");var stone=AssetDatabase.LoadAssetAtPath<Tile>("Assets/Art/Tiles/Stone.asset");
        ground.ClearAllTiles();roads.ClearAllTiles();walls.ClearAllTiles();
        for(int x=-16;x<16;x++)for(int y=-9;y<9;y++) {
            var p=new Vector3Int(x,y,0);ground.SetTile(p,((x*7+y*3)&3)==0?grassB:grassA);
            if(x==-16||x==15||y==-9||y==8)walls.SetTile(p,stone);
            else if(y==-2||y==-1||x==8||x==9)roads.SetTile(p,dirt);
            if(x==0&&y!=-3&&y!=-2&&y!=-1)walls.SetTile(p,stone);
        }
        for(int x=-10;x<-6;x++)walls.SetTile(new Vector3Int(x,4,0),stone);
        for(int x=6;x<10;x++)walls.SetTile(new Vector3Int(x,5,0),stone);
        foreach(var map in maps)EditorUtility.SetDirty(map);
        var fit=Camera.main.GetComponent<MapCameraFit>();if(fit)Object.DestroyImmediate(fit);
        var follow=Camera.main.GetComponent<FollowCamera>();if(!follow)follow=Camera.main.gameObject.AddComponent<FollowCamera>();
        follow.target=session.player.transform;follow.minimum=new Vector2(-16,-9);follow.maximum=new Vector2(16,9);follow.viewHeight=5.6f;follow.smoothing=8;
        Camera.main.orthographicSize=5.6f;
        EditorUtility.SetDirty(session);AssetDatabase.SaveAssets();EditorSceneManager.MarkSceneDirty(session.gameObject.scene);EditorSceneManager.SaveScene(session.gameObject.scene);
        Debug.Log("GHOST_EXPANSION_SETUP_PASS: 32x18 map, follow camera, level composition 2W / 2W+1G / 2W+2G.");
    }
}
