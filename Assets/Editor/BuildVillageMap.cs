using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.U2D.Sprites;
using UnityEngine;
using UnityEngine.Tilemaps;

// Editor-only scene authoring. This does not implement player or enemy behaviour.
public static class BuildVillageMap
{
    [MenuItem("Tuman/Populate Courtyard Tiles")]
    public static void Populate()
    {
        var maps = Object.FindObjectsByType<Tilemap>(FindObjectsSortMode.None);
        var ground=maps.First(m=>m.name=="Ground");
        var road=maps.First(m=>m.name=="Roads");
        var wall=maps.First(m=>m.name.StartsWith("Obstacles"));
        Tile Load(string n)=>AssetDatabase.LoadAssetAtPath<Tile>("Assets/Art/Tiles/"+n+".asset");
        var ga=Load("GrassA");var gb=Load("GrassB");var dt=Load("Dirt");var st=Load("Stone");
        for(int x=-12;x<12;x++)for(int y=-8;y<8;y++)
        {
            var p=new Vector3Int(x,y,0);
            ground.SetTile(p,((x*7+y*3)&3)==0?gb:ga);
            if(x==-12||x==11||y==-8||y==7)wall.SetTile(p,st);
            else if(y==-2||y==-1||x==5||x==6)road.SetTile(p,dt);
        }
        for(int y=0;y<4;y++)wall.SetTile(new Vector3Int(0,y,0),st);
        for(int x=-8;x<-4;x++)wall.SetTile(new Vector3Int(x,4,0),st);
        foreach(var m in maps){m.RefreshAllTiles();EditorUtility.SetDirty(m);}
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
    }
    [MenuItem("Tuman/Fix Map Lighting")]
    public static void FixLighting()
    {
        var light = Object.FindFirstObjectByType<UnityEngine.Rendering.Universal.Light2D>();
        if (!light)
        {
            light = new GameObject("Global Light 2D").AddComponent<UnityEngine.Rendering.Universal.Light2D>();
            light.lightType = UnityEngine.Rendering.Universal.Light2D.LightType.Global;
            light.intensity = 1;
        }
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
    }
    const string Output = "Assets/Scenes/Courtyard.unity";
    [MenuItem("Tuman/Build Courtyard Map")]
    public static void Build()
    {
        if (File.Exists(Output)) { Debug.LogWarning("Courtyard already exists; not overwritten."); return; }
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        const string source = "Assets/Art/Tilesets/VillageSource.jpeg";
        var importer = (TextureImporter)AssetImporter.GetAtPath(source);
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Multiple;
        importer.spritePixelsPerUnit = 88;
        importer.filterMode = FilterMode.Point;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.maxTextureSize = 2048;
        string[] names = { "GrassA", "GrassB", "Dirt", "Stone", "Fence" };
        // Pixel rectangles exclude the source's drawn grid. Coordinates are top-left.
        Rect[] rects = { new Rect(3,3,85,86), new Rect(95,3,85,86), new Rect(3,99,85,86),
                        new Rect(694,3,76,86), new Rect(786,18,163,65) };
        importer.SaveAndReimport();
        var factories = new SpriteDataProviderFactories();
        factories.Init();
        var provider = factories.GetSpriteEditorDataProviderFromObject(importer);
        provider.InitSpriteEditorDataProvider();
        var spriteRects = names.Select((n,i) => new SpriteRect {
            name=n, rect=new Rect(rects[i].x,768-rects[i].y-rects[i].height,rects[i].width,rects[i].height),
            alignment=SpriteAlignment.Custom,pivot=new Vector2(.5f,.5f),spriteID=GUID.Generate()
        }).ToArray();
        provider.SetSpriteRects(spriteRects);
        provider.GetDataProvider<ISpriteNameFileIdDataProvider>().SetNameFileIdPairs(
            spriteRects.Select(s=>new SpriteNameFileIdPair(s.name,s.spriteID)));
        provider.Apply();
        importer.SaveAndReimport();
        var sprites=AssetDatabase.LoadAllAssetsAtPath(source).OfType<Sprite>().ToDictionary(s=>s.name);
        Directory.CreateDirectory("Assets/Art/Tiles");
        AssetDatabase.Refresh();
        Tile TileAsset(string name, Tile.ColliderType collision)
        {
            var tile=ScriptableObject.CreateInstance<Tile>();
            tile.sprite=sprites[name]; tile.colliderType=collision;
            // Normalize the imperfect source rectangles to one world cell.
            tile.transform=Matrix4x4.Scale(new Vector3(88f/rects[System.Array.IndexOf(names,name)].width,
                88f/rects[System.Array.IndexOf(names,name)].height,1));
            AssetDatabase.CreateAsset(tile,"Assets/Art/Tiles/"+name+".asset"); return tile;
        }
        var grassA=TileAsset("GrassA",Tile.ColliderType.None);
        var grassB=TileAsset("GrassB",Tile.ColliderType.None);
        var dirt=TileAsset("Dirt",Tile.ColliderType.None);
        var stone=TileAsset("Stone",Tile.ColliderType.Grid);
        var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
        var camera=new GameObject("Main Camera",typeof(Camera)); camera.tag="MainCamera";
        camera.transform.position=new Vector3(0,0,-10);
        var cam=camera.GetComponent<Camera>(); cam.orthographic=true;cam.orthographicSize=8.7f;
        cam.clearFlags=CameraClearFlags.SolidColor;cam.backgroundColor=new Color(.055f,.075f,.055f);
        var grid=new GameObject("Courtyard Grid",typeof(Grid));
        Tilemap Layer(string name,int order,bool solid)
        {
            var obj=new GameObject(name,typeof(Tilemap),typeof(TilemapRenderer));
            obj.transform.SetParent(grid.transform,false);obj.GetComponent<TilemapRenderer>().sortingOrder=order;
            if(solid) obj.AddComponent<TilemapCollider2D>();
            return obj.GetComponent<Tilemap>();
        }
        var ground=Layer("Ground",0,false);var roads=Layer("Roads",1,false);
        var walls=Layer("Obstacles - Raycast blockers",2,true);
        Layer("Decorations",3,false);
        for(int x=-12;x<12;x++)for(int y=-8;y<8;y++)
        {
            var cell=new Vector3Int(x,y,0);
            ground.SetTile(cell,((x*7+y*3)&3)==0?grassB:grassA);
            if(x==-12||x==11||y==-8||y==7) walls.SetTile(cell,stone);
            else if(y==-2||y==-1||x==5||x==6) roads.SetTile(cell,dirt);
        }
        // Two ends are open, allowing the player to circle around the sight blocker.
        for(int y=0;y<4;y++) walls.SetTile(new Vector3Int(0,y,0),stone);
        for(int x=-8;x<-4;x++)walls.SetTile(new Vector3Int(x,4,0),stone);
        var points=new GameObject("AI Setup Markers");
        foreach(var entry in new[]{("Player Spawn",new Vector3(-6,-3,0)),
            ("Enemy Spawn",new Vector3(4,3,0)),("Patrol A",new Vector3(3,4,0)),
            ("Patrol B",new Vector3(8,4,0))})
        {
            var marker=new GameObject(entry.Item1);marker.transform.SetParent(points.transform);
            marker.transform.position=entry.Item2;
        }
        AssetDatabase.SaveAssets();EditorSceneManager.SaveScene(scene,Output);
        Selection.activeGameObject=grid;
        Debug.Log("Tuman Courtyard saved: 24x16 cells, ground/roads/obstacles/decorations, collider walls, spawn and patrol markers.");
    }
}
