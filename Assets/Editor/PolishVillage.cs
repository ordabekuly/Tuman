using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Tilemaps;
using TumanGame;
using Object=UnityEngine.Object;

public static class PolishVillage
{
    const string Scene="Assets/Scenes/TumanVillageDraft.unity",Dir="Assets/StoryMap/Atmosphere";
    static Transform root;static Material material;static Sprite glow,square;
    static Dictionary<string,Sprite> art;static System.Random rng;
    static List<Transform> trees=new List<Transform>(),ripples=new List<Transform>(),mist=new List<Transform>();
    static List<SpriteRenderer> lamps=new List<SpriteRenderer>();static int meshId;
    static float Rand(float a,float b)=>(float)(a+rng.NextDouble()*(b-a));
    static float River(float y)=>8.7f+Mathf.Sin((y-5)*.12f)*1.45f+Mathf.Sin((y-5)*.28f)*.45f;
    static float Half(float y)=>2.65f+Mathf.Sin(y*.17f)*.3f;
    [MenuItem("Tuman/Polish Village Landscape")]
    public static void Run()
    {
        Directory.CreateDirectory("Submission_Map");Directory.CreateDirectory(Dir);AssetDatabase.Refresh();
        if(!File.Exists("Submission_Map/BeforeLandscape.unity"))File.Copy(Scene,"Submission_Map/BeforeLandscape.unity");
        EditorSceneManager.OpenScene(Scene);rng=new System.Random(425);meshId=0;trees.Clear();ripples.Clear();mist.Clear();lamps.Clear();
        var old=GameObject.Find("Living village landscape");if(old)Object.DestroyImmediate(old);
        root=new GameObject("Living village landscape").transform;
        art=AssetDatabase.LoadAllAssetsAtPath("Assets/Art/Environment/TumanVillageProps_v1.png").OfType<Sprite>().ToDictionary(s=>s.name);
        square=AssetDatabase.LoadAllAssetsAtPath("Assets/StoryMap/White.asset").OfType<Sprite>().First();
        material=AssetDatabase.LoadAssetAtPath<Material>(Dir+"/Terrain.mat");if(!material){material=new Material(Shader.Find("Sprites/Default"));AssetDatabase.CreateAsset(material,Dir+"/Terrain.mat");}
        glow=MakeGlow();
        var maps=Object.FindObjectsByType<Tilemap>();var ground=maps.First(m=>m.name=="Ground");
        maps.First(m=>m.name=="Roads").ClearAllTiles();maps.First(m=>m.name=="River - impassable").ClearAllTiles();
        var obstacles=maps.First(m=>m.name.StartsWith("Obstacles"));
        // The graveyard keeps stone walls; the outside edge becomes forest silhouettes.
        for(int x=-32;x<32;x++)for(int y=-24;y<24;y++)
        {
            var p=new Vector3Int(x,y,0);ground.SetTileFlags(p,TileFlags.None);
            float v=.82f+Mathf.PerlinNoise((x+40)*.15f,(y+40)*.15f)*.28f;ground.SetColor(p,new Color(v*.9f,v,v*.91f));
            if(x==-32||x==31||y==-24||y==23)obstacles.SetTile(p,null);
        }
        var legacy=GameObject.Find("Village draft - replaceable scenery");
        foreach(var c in legacy.GetComponentsInChildren<BoxCollider2D>().ToArray())if(c.transform.position.y<-7||c.transform.position.y>11)Object.DestroyImmediate(c.gameObject);
        foreach(var label in Object.FindObjectsByType<TextMesh>())label.gameObject.SetActive(false);
        var props=GameObject.Find("Village environment sprites");
        foreach(Transform child in props.transform.Cast<Transform>().ToArray())
        {
            if(new[]{"AppleTree","PearTree","ForestTree","DeadTree"}.Contains(child.name))Object.DestroyImmediate(child.gameObject);
        }
        // Organic clearings under the village, orchard and ritual circle.
        Patch(new Vector2(-18,2),new Vector2(12,6),new Color(.24f,.25f,.15f,.55f),1);
        Patch(new Vector2(-20,16),new Vector2(7.5f,4.5f),new Color(.23f,.27f,.13f,.5f),1);
        Patch(new Vector2(-18,-15.5f),new Vector2(6.7f,5.7f),new Color(.28f,.26f,.19f,.9f),1);
        Patch(new Vector2(22,14),new Vector2(6,6),new Color(.24f,.27f,.23f,.65f),1);
        Trail(new[]{new Vector2(-26,0),new Vector2(-22,0),new Vector2(-18,1),new Vector2(-12,1),new Vector2(-6,-.5f),new Vector2(0,1),new Vector2(4,4.8f),new Vector2(14,4.8f),new Vector2(19,4.5f),new Vector2(22,7),new Vector2(23,13),new Vector2(23,19)},1.1f);
        Trail(new[]{new Vector2(-21,0),new Vector2(-22,5),new Vector2(-21,10),new Vector2(-20,15)},.85f);
        Trail(new[]{new Vector2(-18,1),new Vector2(-18,-4),new Vector2(-20,-8),new Vector2(-18,-11),new Vector2(-18,-16)},.95f);
        Trail(new[]{new Vector2(-22,4),new Vector2(-17,4),new Vector2(-15,4)},.8f);
        Trail(new[]{new Vector2(-18,-5),new Vector2(-15,-8),new Vector2(-12,-8)},.75f);
        // Smooth banks, water strips and a real impassable collider on either side of bridge.
        RiverStrip("Soft bank",.85f,new Color(.27f,.29f,.18f),2,false,-24,24);
        RiverStrip("Wet edge",.3f,new Color(.17f,.24f,.22f),3,false,-24,24);
        RiverStrip("River surface",0,new Color(.075f,.22f,.25f),4,false,-24,24);
        RiverStrip("River deep current",-.6f,new Color(.065f,.19f,.23f),5,false,-24,24);
        RiverStrip("River collision south",0,Color.clear,4,true,-24,3);
        RiverStrip("River collision north",0,Color.clear,4,true,7,24);
        var bridge=props.transform.Find("Bridge");bridge.GetComponent<SpriteRenderer>().sortingOrder=12;
        // Slightly irregular orchard rows and clustered woodland with clear combat routes.
        for(int row=0;row<2;row++)for(int col=0;col<4;col++)Tree(new Vector2(-26+col*3.7f+Rand(-.35f,.35f),13.8f+row*4+Rand(-.3f,.3f)),true,Rand(3.5f,4.1f));
        for(int i=0;i<230;i++)
        {
            float x=Rand(-31,30),y=Rand(-23,22);bool woodland=y<-7||x<-29||x>29||y>21.8f;
            if(!woodland||Mathf.Abs(x-River(y))<Half(y)+2)continue;
            if(x<4&&y<-7&&(Vector2.Distance(new Vector2(x,y),new Vector2(-18,-15))<7.8f||Mathf.Abs(x+18)<2.8f))continue;
            if(x>14&&y>6)continue;
            if(Physics2D.OverlapCircle(new Vector2(x,y),1.3f))continue;
            Tree(new Vector2(x,y),false,Rand(3.4f,5.1f));
        }
        foreach(var p in new[]{new Vector2(16,18),new Vector2(27.5f,19),new Vector2(28,9)})Tree(p,false,3.8f,true);
        // Stone circle and small trails of gravel guide players without large debug labels.
        for(int i=0;i<24;i++){float a=i*Mathf.PI*2/24;var p=new Vector2(-18+Mathf.Cos(a)*5.2f,-15.5f+Mathf.Sin(a)*4.2f);Patch(p,new Vector2(.3f,.22f),new Color(.44f,.46f,.39f),3);}
        for(int i=0;i<190;i++)
        {
            float x=Rand(-30,30),y=Rand(-22,22);if(Mathf.Abs(x-River(y))<Half(y)+.5f)continue;
            Patch(new Vector2(x,y),new Vector2(Rand(.09f,.24f),Rand(.06f,.14f)),new Color(.4f,.44f,.26f,.5f),2);
        }
        foreach(var p in new[]{new Vector2(-25,1),new Vector2(-15,5),new Vector2(-6,1),new Vector2(-12,-7),new Vector2(3.5f,7.4f),new Vector2(14,7.4f),new Vector2(21,7.5f)})Lamp(p);
        for(int i=0;i<65;i++)
        {
            float y=Rand(-22,22);if(y>2&&y<8)continue;float x=River(y)+Rand(-1.6f,1.6f);
            var r=SpriteObj("Moving current",square,new Vector2(x,y),new Vector2(Rand(.25f,.8f),.025f),new Color(.4f,.7f,.68f,.28f),6);ripples.Add(r.transform);
        }
        for(int i=0;i<13;i++)
        {var r=SpriteObj("Low drifting mist",glow,new Vector2(Rand(-28,28),Rand(-21,21)),new Vector2(Rand(8,14),Rand(1.3f,2.5f)),new Color(.64f,.76f,.73f,.075f),1100);mist.Add(r.transform);}
        Boundary(new Vector2(-32,0),new Vector2(1,49));Boundary(new Vector2(32,0),new Vector2(1,49));Boundary(new Vector2(0,-24),new Vector2(65,1));Boundary(new Vector2(0,24),new Vector2(65,1));
        var ambient=root.gameObject.AddComponent<VillageAtmosphere>();ambient.trees=trees.ToArray();ambient.ripples=ripples.ToArray();ambient.mist=mist.ToArray();ambient.glows=lamps.ToArray();
        var game=Object.FindAnyObjectByType<GameSession>();game.seamlessStages=true;
        game.levels[0].enemySpawns=new[]{new Vector2(-24,13),new Vector2(-17,13)};EditorUtility.SetDirty(game.levels[0]);
        AssetDatabase.SaveAssets();EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
        Verify();Render("Submission_Map/Village_Landscape.png",Vector2.zero,25,1600,1200);
        Render("Submission_Map/Village_Closeup.png",new Vector2(-21,3),7,1600,900);
        Debug.Log("LANDSCAPE_PASS");
    }
    static Sprite MakeGlow()
    {
        var existing=AssetDatabase.LoadAllAssetsAtPath(Dir+"/SoftLight.asset").OfType<Sprite>().FirstOrDefault();if(existing)return existing;
        var tex=new Texture2D(64,64,TextureFormat.RGBA32,false){filterMode=FilterMode.Bilinear};
        for(int y=0;y<64;y++)for(int x=0;x<64;x++){float d=Vector2.Distance(new Vector2(x,y),new Vector2(31.5f,31.5f))/32;tex.SetPixel(x,y,new Color(1,1,1,Mathf.Pow(Mathf.Clamp01(1-d),2)));}
        tex.Apply();AssetDatabase.CreateAsset(tex,Dir+"/SoftLight.asset");var s=Sprite.Create(tex,new Rect(0,0,64,64),new Vector2(.5f,.5f),64);AssetDatabase.AddObjectToAsset(s,tex);return s;
    }
    static SpriteRenderer SpriteObj(string name,Sprite s,Vector2 p,Vector2 scale,Color color,int order)
    {var go=new GameObject(name,typeof(SpriteRenderer));go.transform.SetParent(root,false);go.transform.position=p;go.transform.localScale=new Vector3(scale.x,scale.y,1);var r=go.GetComponent<SpriteRenderer>();r.sprite=s;r.color=color;r.sortingOrder=order;return r;}
    static void Tree(Vector2 p,bool fruit,float width,bool dead=false)
    {
        var s=art[dead?"DeadTree":fruit?(rng.Next(2)==0?"AppleTree":"PearTree"):"ForestTree"];
        SpriteObj("Tree shadow",glow,p+new Vector2(.2f,0),new Vector2(width*.9f,.75f),new Color(.02f,.05f,.04f,.6f),3);
        var r=SpriteObj(dead?"Dead woodland":"Wind in branches",s,p,Vector2.one*(width/s.bounds.size.x),Color.white,500-Mathf.RoundToInt(p.y*10));
        var c=r.gameObject.AddComponent<BoxCollider2D>();float scale=r.transform.localScale.x;c.size=new Vector2(.48f,.4f)/scale;c.offset=new Vector2(0,.25f)/scale;
        trees.Add(r.transform);Physics2D.SyncTransforms();
    }
    static void Lamp(Vector2 p)
    {var s=art["Lantern"];SpriteObj("Lantern",s,p,Vector2.one*(.7f/s.bounds.size.x),Color.white,500-Mathf.RoundToInt(p.y*10));lamps.Add(SpriteObj("Amber lamplight",glow,p+Vector2.up*.4f,new Vector2(5,5),new Color(1,.66f,.22f,.13f),1000));}
    static void Boundary(Vector2 p,Vector2 size){var go=new GameObject("Map boundary",typeof(BoxCollider2D));go.transform.SetParent(root,false);go.transform.position=p;go.GetComponent<BoxCollider2D>().size=size;}
    static GameObject Polygon(string name,Vector2[] points,Color color,int order,bool collision=false)
    {
        var go=new GameObject(name,typeof(MeshFilter),typeof(MeshRenderer));go.transform.SetParent(root,false);
        var vertices=points.Select(p=>(Vector3)p).ToArray();var triangles=new List<int>();for(int i=1;i<points.Length-1;i++){triangles.Add(0);triangles.Add(i);triangles.Add(i+1);}
        var mesh=new Mesh{name=name};mesh.vertices=vertices;mesh.triangles=triangles.ToArray();mesh.colors=Enumerable.Repeat(color,points.Length).ToArray();mesh.uv=Enumerable.Repeat(Vector2.zero,points.Length).ToArray();mesh.RecalculateBounds();
        string path=Dir+"/Mesh_"+(meshId++)+".asset";var old=AssetDatabase.LoadAssetAtPath<Mesh>(path);if(old){EditorUtility.CopySerialized(mesh,old);Object.DestroyImmediate(mesh);mesh=old;}else AssetDatabase.CreateAsset(mesh,path);
        go.GetComponent<MeshFilter>().sharedMesh=mesh;var r=go.GetComponent<MeshRenderer>();r.sharedMaterial=material;r.sortingOrder=order;
        if(collision){var c=go.AddComponent<PolygonCollider2D>();c.points=points;}
        return go;
    }
    static void Patch(Vector2 p,Vector2 radius,Color color,int order)
    {var points=new Vector2[16];for(int i=0;i<16;i++){float a=i*Mathf.PI*2/16;float v=Rand(.87f,1.08f);points[i]=p+new Vector2(Mathf.Cos(a)*radius.x*v,Mathf.Sin(a)*radius.y*v);}Polygon("Earth and gravel",points,color,order);}
    static void Trail(Vector2[] nodes,float half)
    {
        for(int i=0;i<nodes.Length-1;i++)
        {var a=nodes[i];var b=nodes[i+1];var n=new Vector2(-(b-a).normalized.y,(b-a).normalized.x);Polygon("Worn path edge",new[]{a+n*(half+.3f),b+n*(half+.3f),b-n*(half+.3f),a-n*(half+.3f)},new Color(.27f,.27f,.17f),2);Polygon("Earth trail",new[]{a+n*half,b+n*half,b-n*half,a-n*half},new Color(.32f,.28f,.20f),3);Patch(a,Vector2.one*half,new Color(.32f,.28f,.20f),3);}
    }
    static void RiverStrip(string name,float expand,Color color,int order,bool collider,float start,float end)
    {
        // Short convex segments avoid concave polygon fan artifacts and share bank endpoints.
        for(float y=start;y<end;y+=.5f){float next=Mathf.Min(end,y+.5f);Polygon(name,new[]{new Vector2(River(y)-Half(y)-expand,y),new Vector2(River(next)-Half(next)-expand,next),new Vector2(River(next)+Half(next)+expand,next),new Vector2(River(y)+Half(y)+expand,y)},color,order,collider);}
    }
    public static void Verify()
    {
        foreach(var map in Object.FindObjectsByType<TilemapCollider2D>())map.ProcessTilemapChanges();Physics2D.SyncTransforms();
        var player=Object.FindAnyObjectByType<GameSession>().player;player.gameObject.SetActive(false);
        bool Free(Vector2 p)=>Physics2D.OverlapCircle(p,.28f)==null;
        var seen=new HashSet<Vector2Int>();var q=new Queue<Vector2Int>();var start=new Vector2Int(-50,0);q.Enqueue(start);seen.Add(start);
        while(q.Count>0){var p=q.Dequeue();foreach(var d in new[]{Vector2Int.up,Vector2Int.down,Vector2Int.left,Vector2Int.right}){var n=p+d;if(n.x<-62||n.x>62||n.y<-46||n.y>46||seen.Contains(n)||!Free((Vector2)n*.5f))continue;seen.Add(n);q.Enqueue(n);}}
        var game=Object.FindAnyObjectByType<GameSession>();var report=new List<string>();
        foreach(var l in game.levels)foreach(var p in l.enemySpawns.Concat(l.ghostSpawns).Concat(l.herbSpawns).Concat(l.tokenSpawns))
        {bool ok=seen.Contains(Vector2Int.RoundToInt(p*2));report.Add((ok?"PASS ":"FAIL ")+"reachable "+p);}
        bool riverBlocked=!Free(new Vector2(River(0),0)),bridgeOpen=Free(new Vector2(9,5));report.Add("River blocked="+riverBlocked+" Bridge open="+bridgeOpen);
        player.gameObject.SetActive(true);File.WriteAllLines("Submission_Map/landscape-checks.txt",report);
        if(report.Any(s=>s.StartsWith("FAIL"))||!riverBlocked||!bridgeOpen)throw new Exception("Landscape collision reachability failed; inspect landscape-checks.txt");
    }
    public static void Finish()
    {
        EditorSceneManager.OpenScene(Scene);root=GameObject.Find("Living village landscape").transform;
        material=AssetDatabase.LoadAssetAtPath<Material>(Dir+"/Terrain.mat");
        // Combine static terrain by draw order; atmosphere sprites remain independently animated.
        foreach(var group in root.GetComponentsInChildren<MeshRenderer>().Where(r=>!r.name.StartsWith("Terrain batch")).GroupBy(r=>r.sortingOrder).ToArray())
        {
            var mesh=new Mesh{name="Terrain batch "+group.Key};mesh.indexFormat=UnityEngine.Rendering.IndexFormat.UInt32;
            mesh.CombineMeshes(group.Select(r=>new CombineInstance{mesh=r.GetComponent<MeshFilter>().sharedMesh,transform=r.transform.localToWorldMatrix}).ToArray(),true,true);
            var path=Dir+"/Batch_"+group.Key+".asset";var saved=AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if(saved){EditorUtility.CopySerialized(mesh,saved);Object.DestroyImmediate(mesh);mesh=saved;}else AssetDatabase.CreateAsset(mesh,path);
            var go=new GameObject("Terrain batch "+group.Key,typeof(MeshFilter),typeof(MeshRenderer));go.transform.SetParent(root,false);go.GetComponent<MeshFilter>().sharedMesh=mesh;go.GetComponent<MeshRenderer>().sharedMaterial=material;go.GetComponent<MeshRenderer>().sortingOrder=group.Key;
            foreach(var r in group){Object.DestroyImmediate(r.GetComponent<MeshFilter>());Object.DestroyImmediate(r);}
        }
        foreach(var p in new[]{new Vector2(-22,-15.4f),new Vector2(-18,-11.4f),new Vector2(-14,-15.4f),new Vector2(17,16.7f),new Vector2(20,16.7f),new Vector2(26,16.7f)})Boundary(p,new Vector2(.65f,.5f));
        var game=Object.FindAnyObjectByType<GameSession>();game.levels[0].enemySpawns=new[]{new Vector2(-24,13),new Vector2(-17,13)};EditorUtility.SetDirty(game.levels[0]);
        AssetDatabase.SaveAssets();EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
        Verify();Render("Submission_Map/Village_Landscape.png",Vector2.zero,25,1600,1200);Render("Submission_Map/Village_Closeup.png",new Vector2(-21,3),7,1600,900);
        Debug.Log("LANDSCAPE_FINISH_PASS: static terrain batched by sorting order");
    }
    public static void TextureTerrain()
    {
        EditorSceneManager.OpenScene(Scene);
        var m=AssetDatabase.LoadAssetAtPath<Material>(Dir+"/Terrain.mat");m.shader=Shader.Find("Tuman/LivingLandscape");
        var tile=AssetDatabase.LoadAssetAtPath<Tile>("Assets/Art/Tiles/Dirt.asset");var s=tile.sprite;var r=s.rect;
        m.SetTexture("_MainTex",s.texture);m.SetVector("_TileRect",new Vector4(r.x/s.texture.width,r.y/s.texture.height,r.width/s.texture.width,r.height/s.texture.height));EditorUtility.SetDirty(m);
        AssetDatabase.SaveAssets();Verify();Render("Submission_Map/Village_Landscape.png",Vector2.zero,25,1600,1200);Render("Submission_Map/Village_Closeup.png",new Vector2(-21,3),7,1600,900);
        Debug.Log("LANDSCAPE_TEXTURE_PASS");
    }
    public static void Render(string file,Vector2 centre,float size,int width,int height)
    {
        var cam=Camera.main;var pos=cam.transform.position;float zoom=cam.orthographicSize;var rt=new RenderTexture(width,height,24);var previous=RenderTexture.active;var old=cam.targetTexture;var tex=new Texture2D(width,height,TextureFormat.RGB24,false);
        try{cam.transform.position=new Vector3(centre.x,centre.y,-10);cam.orthographicSize=size;cam.targetTexture=rt;cam.Render();RenderTexture.active=rt;tex.ReadPixels(new Rect(0,0,width,height),0,0);tex.Apply();File.WriteAllBytes(file,tex.EncodeToPNG());}
        finally{cam.targetTexture=old;cam.transform.position=pos;cam.orthographicSize=zoom;RenderTexture.active=previous;rt.Release();Object.DestroyImmediate(rt);Object.DestroyImmediate(tex);}
    }
}
