using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.U2D.Sprites;
using UnityEngine;
using UnityEngine.Tilemaps;
using TumanGame;
using Object=UnityEngine.Object;

public static class ApplyStoryMapArt
{
    const string Source="Assets/Art/Environment/TumanVillageProps_v1.png";
    const string Scene="Assets/Scenes/TumanVillageDraft.unity";
    static Transform root;
    static System.Collections.Generic.Dictionary<string,Sprite> art;
    [MenuItem("Tuman/Apply Village Sprites")]
    public static void Run()
    {
        var importer=(TextureImporter)AssetImporter.GetAtPath(Source);
        importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Multiple;
        importer.spritePixelsPerUnit=64;importer.filterMode=FilterMode.Point;
        importer.textureCompression=TextureImporterCompression.Uncompressed;importer.alphaIsTransparency=true;
        importer.maxTextureSize=2048;importer.SaveAndReimport();
        var specs=new[]{
            ("Home",new Rect(2,24,374,314)),("HouseA",new Rect(385,70,280,270)),
            ("HouseB",new Rect(680,70,273,270)),("HouseC",new Rect(964,70,286,270)),
            ("AppleTree",new Rect(10,360,303,270)),("PearTree",new Rect(315,360,280,270)),
            ("ForestTree",new Rect(603,355,330,275)),("DeadTree",new Rect(950,355,300,275)),
            ("Bridge",new Rect(8,643,438,212)),("Fence",new Rect(462,698,224,124)),
            ("FenceVertical",new Rect(700,655,115,168)),("FenceCorner",new Rect(831,655,170,169)),
            ("Gate",new Rect(1016,659,232,170)),("GraveA",new Rect(130,906,122,179)),
            ("GraveB",new Rect(254,915,112,170)),("GraveC",new Rect(368,920,109,165)),
            ("RitualStone",new Rect(477,864,136,220)),("Pedestal",new Rect(610,925,191,155)),
            ("Barrel",new Rect(811,915,130,170)),("Branches",new Rect(946,954,188,123)),
            ("Lantern",new Rect(1135,915,115,163)),("Garden",new Rect(60,1105,385,144)),
            ("Cabbages",new Rect(458,1103,357,144)),("Carrots",new Rect(840,1102,310,146))};
        var factories=new SpriteDataProviderFactories();factories.Init();
        var provider=factories.GetSpriteEditorDataProviderFromObject(importer);provider.InitSpriteEditorDataProvider();
        var rects=specs.Select(s=>new SpriteRect{name=s.Item1,rect=new Rect(s.Item2.x,1254-s.Item2.y-s.Item2.height,s.Item2.width,s.Item2.height),alignment=SpriteAlignment.Custom,pivot=new Vector2(.5f,0),spriteID=GUID.Generate()}).ToArray();
        provider.SetSpriteRects(rects);
        provider.GetDataProvider<ISpriteNameFileIdDataProvider>().SetNameFileIdPairs(rects.Select(s=>new SpriteNameFileIdPair(s.name,s.spriteID)));
        provider.Apply();importer.SaveAndReimport();
        art=AssetDatabase.LoadAllAssetsAtPath(Source).OfType<Sprite>().ToDictionary(s=>s.name);
        EditorSceneManager.OpenScene(Scene);
        var draft=GameObject.Find("Village draft - replaceable scenery");
        foreach(var r in draft.GetComponentsInChildren<SpriteRenderer>())Object.DestroyImmediate(r.gameObject);
        var existing=GameObject.Find("Village environment sprites");if(existing)Object.DestroyImmediate(existing);
        root=new GameObject("Village environment sprites").transform;
        Put("Home",-26,.5f,5.8f);Put("HouseA",-15,4.5f,4.7f);
        Put("HouseB",-6,.5f,4.7f);Put("HouseC",-12,-7.5f,4.7f);
        for(int x=-27;x<=-14;x+=4)foreach(int y in new[]{14,18})Put((x%2==0)?"PearTree":"AppleTree",x,y-.55f,3.1f);
        for(int x=-29;x<=1;x+=3)for(int y=-21;y<=-8;y+=3)
        {float dx=x+18,dy=y+15;if(dx*dx+dy*dy<44||Mathf.Abs(x+18)<3)continue;Put("ForestTree",x,y-.55f,3.2f);}
        foreach(var p in new[]{new Vector2(-22,-15),new Vector2(-18,-11),new Vector2(-14,-15)})Put("RitualStone",p.x,p.y-.85f,1.35f);
        Put("Pedestal",-18,-16.3f,1.8f);
        for(int x=17;x<=27;x+=3)foreach(int y in new[]{11,17})if(x!=23)Put(new[]{"GraveA","GraveB","GraveC"}[Mathf.Abs(x+y)%3],x,y-.55f,.95f);
        Put("Pedestal",23,18.7f,1.6f);Put("DeadTree",27,19,3);Put("DeadTree",17,18,2.5f);
        Put("Bridge",9,2.8f,12,3);
        Put("Garden",-28,-3.1f,4.3f,3);Put("Cabbages",-28,-2.8f,3.5f,4);
        Put("Garden",-28,-4.5f,4.3f,3);Put("Carrots",-28,-4.2f,3.5f,4);
        Put("Barrel",-22.8f,.8f,.8f);Put("Barrel",-9,5,.8f);Put("Branches",-8,-11,1.2f);
        Put("Lantern",-23.2f,1.3f,.8f);
        // Keep the existing orchard boundary colliders but replace stone visuals with wood.
        var obstacles=Object.FindObjectsByType<Tilemap>().First(m=>m.name.StartsWith("Obstacles"));
        for(int x=-29;x<=-12;x++)foreach(int y in new[]{11,21})HideBoundary(obstacles,x,y);
        for(int y=11;y<=21;y++)foreach(int x in new[]{-29,-12})HideBoundary(obstacles,x,y);
        for(int x=-28;x<-12;x+=2){Put("Fence",x,21,2.1f);if(x<-22||x>-19)Put("Fence",x,11,2.1f);}
        for(int y=12;y<21;y+=2){Put("FenceVertical",-28.5f,y,1);Put("FenceVertical",-11.5f,y,1);}
        Collide(-20,21.5f,18,1);Collide(-28.5f,16,1,10);Collide(-11.5f,16,1,10);
        Collide(-25.5f,11.5f,7,1);Collide(-15,11.5f,6,1);
        Collide(27,19.25f,.65f,.6f);Collide(17,18.25f,.65f,.6f);
        Collide(-22.8f,1,.6f,.45f);Collide(-9,5.2f,.6f,.45f);
        FixGardenFences.ApplyToOpenScene();
        var game=Object.FindAnyObjectByType<GameSession>();game.seamlessStages=true;
        if(!game.player.GetComponent<WorldDepthSort>())game.player.gameObject.AddComponent<WorldDepthSort>();
        for(int i=0;i<game.levels.Length;i++)
        {
            var l=game.levels[i];
            l.herbSpawns=i==0?new[]{new Vector2(-22,1)}:i==1?new[]{new Vector2(-18,-8)}:new[]{new Vector2(22,6)};
            l.tokenSpawns=i==0?new[]{new Vector2(-20,15)}:i==1?new[]{new Vector2(-18,-16)}:new[]{new Vector2(23,19)};
            EditorUtility.SetDirty(l);
        }
        EditorUtility.SetDirty(game);AssetDatabase.SaveAssets();EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
        BuildStoryMap.VerifySaved();
        Debug.Log("STORY_ART_PASS: 24 isolated sprites imported, placeholder visuals replaced, stage-local pickups, seamless stages enabled.");
    }
    static void HideBoundary(Tilemap map,int x,int y){map.SetTile(new Vector3Int(x,y,0),null);}
    static void Collide(float x,float y,float width,float height)
    {var go=new GameObject("Prop collision",typeof(BoxCollider2D));go.transform.SetParent(root,false);go.transform.position=new Vector3(x,y,0);go.GetComponent<BoxCollider2D>().size=new Vector2(width,height);}
    static void Put(string name,float x,float y,float width,int fixedOrder=int.MinValue)
    {
        var sprite=art[name];var go=new GameObject(name,typeof(SpriteRenderer));go.transform.SetParent(root,false);go.transform.position=new Vector3(x,y,0);
        go.transform.localScale=Vector3.one*(width/sprite.bounds.size.x);
        var r=go.GetComponent<SpriteRenderer>();r.sprite=sprite;r.sortingOrder=fixedOrder==int.MinValue?500-Mathf.RoundToInt(y*10):fixedOrder;
    }
}
