using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Tilemaps;
using TumanGame;
using Object = UnityEngine.Object;

// Editable spatial prototype. Final building/tree artwork can replace these silhouettes.
public static class BuildStoryMap
{
    const string Folder="Assets/StoryMap";
    const string ScenePath="Assets/Scenes/TumanVillageDraft.unity";
    static Tilemap ground,roads,solids,water;
    static Transform props;
    static Sprite square;
    static Tile dirt,stone;
    static readonly HashSet<Vector2Int> blocked=new HashSet<Vector2Int>();
    static readonly Color wood=new Color(.32f,.22f,.14f);
    [MenuItem("Tuman/Build Story Village Draft")]
    public static void Run()
    {
        if(File.Exists(ScenePath))throw new InvalidOperationException("Draft exists; open it rather than overwriting edits.");
        Directory.CreateDirectory(Folder);Directory.CreateDirectory("Submission_Map");AssetDatabase.Refresh();
        EditorSceneManager.OpenScene("Assets/Scenes/Tuman11.unity");
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(),ScenePath);
        var session=Object.FindAnyObjectByType<GameSession>();
        var maps=Object.FindObjectsByType<Tilemap>();
        ground=maps.First(m=>m.name=="Ground");roads=maps.First(m=>m.name=="Roads");
        solids=maps.First(m=>m.name.StartsWith("Obstacles"));
        foreach(var map in maps)map.ClearAllTiles();blocked.Clear();
        var oldMarkers=GameObject.Find("AI Setup Markers");if(oldMarkers)Object.DestroyImmediate(oldMarkers);
        props=new GameObject("Village draft - replaceable scenery").transform;
        var texture=new Texture2D(1,1);texture.SetPixel(0,0,Color.white);texture.Apply();texture.filterMode=FilterMode.Point;
        AssetDatabase.CreateAsset(texture,Folder+"/White.asset");
        square=Sprite.Create(texture,new Rect(0,0,1,1),new Vector2(.5f,.5f),1);
        AssetDatabase.AddObjectToAsset(square,texture);
        dirt=AssetDatabase.LoadAssetAtPath<Tile>("Assets/Art/Tiles/Dirt.asset");
        stone=AssetDatabase.LoadAssetAtPath<Tile>("Assets/Art/Tiles/Stone.asset");
        var grassA=AssetDatabase.LoadAssetAtPath<Tile>("Assets/Art/Tiles/GrassA.asset");
        var grassB=AssetDatabase.LoadAssetAtPath<Tile>("Assets/Art/Tiles/GrassB.asset");
        var waterTile=NewTile("River",new Color(.10f,.28f,.34f),Tile.ColliderType.Grid);
        var plank=NewTile("Bridge planks",new Color(.48f,.35f,.22f),Tile.ColliderType.None);
        var obj=new GameObject("River - impassable",typeof(Tilemap),typeof(TilemapRenderer),typeof(TilemapCollider2D));
        obj.transform.SetParent(ground.transform.parent,false);water=obj.GetComponent<Tilemap>();obj.GetComponent<TilemapRenderer>().sortingOrder=2;
        for(int x=-32;x<32;x++)for(int y=-24;y<24;y++)
        {
            var p=new Vector3Int(x,y,0);ground.SetTile(p,((x*7+y*3)&3)==0?grassB:grassA);
            if(x==-32||x==31||y==-24||y==23)Block(x,y,stone);
            int riverLeft=5+Mathf.RoundToInt(Mathf.Sin(y*.16f)*1.6f);
            if(x>=riverLeft&&x<riverLeft+6)
            {
                if(y>=3&&y<=6)roads.SetTile(p,plank);
                else {water.SetTile(p,waterTile);blocked.Add(new Vector2Int(x,y));}
            }
        }
        Path(-24,0,3,0);Path(3,0,3,4);Path(-20,0,-20,15);Path(-18,0,-18,-15);
        Path(-25,4,-4,4);Path(-25,4,-25,0);Path(1,4,22,4);Path(22,4,22,12);
        House("Player house",-26,2);House("Neighbour house A",-15,6);
        House("Neighbour house B",-6,2);House("Neighbour house C",-12,-6);
        Label("HOME",-26,-.6f);Label("VILLAGE",-16,10);
        // Orchard entrance faces the village; trees keep the central fighting lane open.
        FenceRect(-29,11,-12,21,-22,-19);
        for(int x=-27;x<=-14;x+=4)foreach(int y in new[]{14,18})Tree(x,y,true);
        Label("ORCHARD",-20,22);Marker("Orchard fragment",-20,15);
        for(int row=0;row<2;row++)for(int x=-29;x<-25;x++)
        {
            Rect("Vegetable bed",x,-2-row,.85f,.65f,new Color(.27f,.16f,.10f),3);
            Rect("Vegetables",x,-1.95f-row,.5f,.3f,new Color(.38f,.47f,.16f),4);
        }
        // Wide forest paths and an open ritual arena, no scripted quest triggers yet.
        for(int x=-29;x<=1;x+=3)for(int y=-21;y<=-8;y+=3)
        {
            float dx=x+18,dy=y+15;
            if(dx*dx+dy*dy<44||Mathf.Abs(x+18)<3)continue;
            Tree(x,y,false);
        }
        Label("FOREST",-5,-10);Label("RITUAL SITE",-18,-20);
        foreach(var p in new[]{new Vector2(-22,-15),new Vector2(-18,-11),new Vector2(-14,-15)})
        {Rect("Standing ritual stone",p.x,p.y,1,1.7f,new Color(.46f,.51f,.48f),4);Solid(p.x,p.y-.45f,.85f,.65f);}
        Marker("Ritual pedestal",-18,-16);
        FenceRect(15,7,29,21,21,23);Label("GRAVEYARD",22,22);
        for(int x=17;x<=27;x+=3)foreach(int y in new[]{11,17})
        {
            if(x==23)continue;
            Rect("Gravestone",x,y,.8f,1.1f,new Color(.48f,.53f,.50f),4);
            Rect("Grave inscription",x,y+.1f,.4f,.08f,new Color(.23f,.28f,.26f),5);
            Solid(x,y-.3f,.7f,.5f);
        }
        Marker("Graveyard fragment",23,19);
        // Visual bridge rails have collision; the four-cell-wide deck remains open.
        Rect("Bridge north rail",9,7,12,.2f,wood,4);Solid(9,7,12,.2f);
        Rect("Bridge south rail",9,2.8f,12,.2f,wood,4);Solid(9,2.8f,12,.2f);
        for(int x=3;x<=14;x++)Rect("Plank seam",x+.5f,5,.045f,3.9f,new Color(.22f,.16f,.10f),3);
        Label("BRIDGE",9,8.2f);
        var newLevels=new LevelData[3];
        for(int i=0;i<3;i++)
        {
            var level=Object.Instantiate(session.levels[i]);level.name="Draft level "+(i+1);
            level.timeLimit=600;level.playerSpawn=new Vector2(-25,0);
            level.title=new[]{"The orchard","Forest stones","Across the river"}[i];
            level.description="Village layout prototype. Clear enemies; story objectives are planned.";
            level.enemySpawns=i==0?new[]{new Vector2(-24,12),new Vector2(-17,12)}:
                i==1?new[]{new Vector2(-23,-13),new Vector2(-13,-16),new Vector2(-19,-18)}:
                new[]{new Vector2(18,9),new Vector2(26,9)};
            level.ghostSpawns=i==2?new[]{new Vector2(19,14),new Vector2(26,14)}:Array.Empty<Vector2>();
            level.herbSpawns=new[]{new Vector2(-22,1),new Vector2(-18,-8),new Vector2(22,6)};
            level.tokenSpawns=new[]{new Vector2(-20,15),new Vector2(-18,-16),new Vector2(23,19)};
            AssetDatabase.CreateAsset(level,Folder+"/DraftLevel_"+(i+1)+".asset");newLevels[i]=level;
        }
        session.levels=newLevels;session.player.transform.position=new Vector3(-25,0,0);
        var follow=Camera.main.GetComponent<FollowCamera>();follow.minimum=new Vector2(-32,-24);follow.maximum=new Vector2(32,24);
        follow.target=session.player.transform;follow.viewHeight=6;Camera.main.transform.position=new Vector3(-23,0,-10);
        Validate(newLevels);
        EditorUtility.SetDirty(session);AssetDatabase.SaveAssets();
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
        Preview();Selection.activeGameObject=ground.transform.parent.gameObject;
        Debug.Log("STORY_MAP_PASS: separate 64x48 draft, four houses, orchard, forest ritual site, river/bridge, graveyard, cloned levels, all objectives reachable.");
    }
    static Tile NewTile(string name,Color color,Tile.ColliderType collision)
    {var t=ScriptableObject.CreateInstance<Tile>();t.sprite=square;t.color=color;t.colliderType=collision;AssetDatabase.CreateAsset(t,Folder+"/"+name+".asset");return t;}
    static void Block(int x,int y,Tile t){solids.SetTile(new Vector3Int(x,y,0),t);blocked.Add(new Vector2Int(x,y));}
    static void Path(int ax,int ay,int bx,int by)
    {
        for(int x=Math.Min(ax,bx);x<=Math.Max(ax,bx);x++)for(int y=Math.Min(ay,by);y<=Math.Max(ay,by);y++)
        for(int d=0;d<2;d++){var p=new Vector3Int(x,y+d,0);if(!blocked.Contains(new Vector2Int(p.x,p.y)))roads.SetTile(p,dirt);}
    }
    static void Rect(string name,float x,float y,float w,float h,Color color,int order)
    {var go=new GameObject(name,typeof(SpriteRenderer));go.transform.SetParent(props,false);go.transform.position=new Vector3(x,y,0);go.transform.localScale=new Vector3(w,h,1);var r=go.GetComponent<SpriteRenderer>();r.sprite=square;r.color=color;r.sortingOrder=order;}
    static void Solid(float x,float y,float w,float h)
    {
        var go=new GameObject("Scenery collision",typeof(BoxCollider2D));go.transform.SetParent(props,false);go.transform.position=new Vector3(x,y,0);go.GetComponent<BoxCollider2D>().size=new Vector2(w,h);
        for(int cx=Mathf.CeilToInt(x-w/2);cx<=Mathf.FloorToInt(x+w/2);cx++)for(int cy=Mathf.CeilToInt(y-h/2);cy<=Mathf.FloorToInt(y+h/2);cy++)blocked.Add(new Vector2Int(cx,cy));
    }
    static void House(string name,float x,float y)
    {
        Rect(name+" shadow",x+.3f,y-.3f,4.6f,4.4f,new Color(.08f,.12f,.10f),2);
        Rect(name+" front",x,y,4,3,new Color(.54f,.47f,.34f),4);
        Rect(name+" flat roof",x,y+1.4f,4.3f,2,new Color(.37f,.38f,.32f),5);
        Rect(name+" roof inset",x,y+1.45f,3.6f,1.35f,new Color(.47f,.47f,.39f),6);
        Rect(name+" door",x,y-.8f,.8f,1.4f,wood,5);
        Rect(name+" window",x+1.2f,y-.35f,.55f,.55f,new Color(.76f,.61f,.27f),5);
        Solid(x,y,4,3.5f);
    }
    static void Tree(int x,int y,bool fruit)
    {
        Rect("Tree trunk",x,y,.35f,1.1f,wood,4);Solid(x,y-.2f,.65f,.65f);
        Rect("Tree canopy",x,y+1.3f,2.1f,1.8f,fruit?new Color(.28f,.39f,.19f):new Color(.16f,.29f,.22f),4);
        Rect("Tree crown",x,y+2,1.25f,1.1f,new Color(.24f,.36f,.22f),4);
        if(fruit)Rect("Fruit",x+.55f,y+1.4f,.2f,.2f,new Color(.65f,.34f,.18f),5);
    }
    static void FenceRect(int ax,int ay,int bx,int by,int gateA,int gateB)
    {for(int x=ax;x<=bx;x++){if(x<gateA||x>gateB)Block(x,ay,stone);Block(x,by,stone);}for(int y=ay;y<=by;y++){Block(ax,y,stone);Block(bx,y,stone);}}
    static void Marker(string name,float x,float y){Rect(name,x,y,.8f,.6f,new Color(.68f,.58f,.30f),4);}
    static void Label(string text,float x,float y)
    {var go=new GameObject(text,typeof(TextMesh));go.transform.SetParent(props,false);go.transform.position=new Vector3(x,y,-1);var t=go.GetComponent<TextMesh>();t.text=text;t.fontSize=40;t.characterSize=.14f;t.anchor=TextAnchor.MiddleCenter;t.color=new Color(.88f,.85f,.67f);go.GetComponent<MeshRenderer>().sortingOrder=20;}
    static void Validate(LevelData[] levels)
    {
        var start=new Vector2Int(-25,0);var seen=new HashSet<Vector2Int>{start};var queue=new Queue<Vector2Int>();queue.Enqueue(start);
        while(queue.Count>0){var p=queue.Dequeue();foreach(var d in new[]{Vector2Int.up,Vector2Int.down,Vector2Int.left,Vector2Int.right}){var n=p+d;if(n.x<=-32||n.x>=31||n.y<=-24||n.y>=23||blocked.Contains(n)||!seen.Add(n))continue;queue.Enqueue(n);}}
        foreach(var l in levels)foreach(var p in l.enemySpawns.Concat(l.ghostSpawns).Concat(l.herbSpawns).Concat(l.tokenSpawns))
            if(!seen.Contains(Vector2Int.RoundToInt(p)))throw new Exception("Unreachable spawn/objective: "+p);
        File.WriteAllText("Submission_Map/validation.txt","PASS: all enemy, pickup and objective coordinates reachable from home via four-neighbour flood fill.\nMap 64x48. River crossing restricted to bridge. Original Task11 scene/data preserved.\nScenery is an editable spatial prototype; no quest, dialogue, ritual triggers or final scenery art implemented.\n");
    }
    static void Preview()
    {
        var cam=Camera.main;var oldPos=cam.transform.position;float oldSize=cam.orthographicSize;var oldTarget=cam.targetTexture;
        var rt=new RenderTexture(1600,1200,24);var tex=new Texture2D(1600,1200,TextureFormat.RGB24,false);var previous=RenderTexture.active;
        try{cam.transform.position=new Vector3(0,0,-10);cam.orthographicSize=25;cam.targetTexture=rt;cam.Render();RenderTexture.active=rt;tex.ReadPixels(new Rect(0,0,1600,1200),0,0);tex.Apply();File.WriteAllBytes("Submission_Map/VillageDraft_Overview.png",tex.EncodeToPNG());}
        finally{cam.targetTexture=oldTarget;cam.transform.position=oldPos;cam.orthographicSize=oldSize;RenderTexture.active=previous;rt.Release();Object.DestroyImmediate(rt);Object.DestroyImmediate(tex);}
    }
    public static void VerifySaved()
    {
        EditorSceneManager.OpenScene(ScenePath);
        var maps=Object.FindObjectsByType<Tilemap>();
        var road=maps.First(m=>m.name=="Roads");
        var river=maps.First(m=>m.name=="River - impassable");
        var plank=AssetDatabase.LoadAssetAtPath<Tile>(Folder+"/Bridge planks.asset");
        for(int x=3;x<=14;x++)for(int y=3;y<=6;y++)road.SetTile(new Vector3Int(x,y,0),plank);
        foreach(var c in Object.FindObjectsByType<TilemapCollider2D>())c.ProcessTilemapChanges();
        Physics2D.SyncTransforms();
        if(!river.GetComponent<Collider2D>().OverlapPoint(new Vector2(8.5f,.5f)))throw new Exception("River must block movement.");
        if(river.GetComponent<Collider2D>().OverlapPoint(new Vector2(8.5f,4.5f)))throw new Exception("Bridge must remain open.");
        var session=Object.FindAnyObjectByType<GameSession>();
        if(session.levels.Length!=3||session.levels.Any(l=>!AssetDatabase.GetAssetPath(l).StartsWith(Folder)))throw new Exception("Draft must use separate level assets.");
        if(!session.enemyTemplate||!session.ghostTemplate||!session.player)throw new Exception("Missing gameplay references.");
        var follow=Camera.main.GetComponent<FollowCamera>();
        if(follow.minimum!=new Vector2(-32,-24)||follow.maximum!=new Vector2(32,24))throw new Exception("Incorrect camera bounds.");
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
        File.AppendAllText("Submission_Map/validation.txt","PASS: reopened saved scene, real river collider blocks shore crossing and leaves bridge open; three isolated level assets, player/enemy references and camera bounds verified.\n");
        Preview();Debug.Log("STORY_MAP_SAVED_VERIFY_PASS");
    }
}
