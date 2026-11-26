using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.U2D.Sprites;
using UnityEngine;
using Object = UnityEngine.Object;

// Repairs only the two orchard sides; horizontal rails and the entrance stay in place.
public static class FixGardenFences
{
    const string ScenePath = "Assets/Scenes/TumanVillageDraft.unity";
    const string ArtPath = "Assets/Art/Environment/GardenSideFence_v1.png";
    const float Left = -28.82f, Right = -13.15f, Bottom = 11.1f, Top = 22.05f;

    [MenuItem("Tuman/Fix Garden Side Fences")]
    public static void Run()
    {
        Directory.CreateDirectory("Submission_Map");
        if (!File.Exists("Submission_Map/BeforeFenceFix.unity"))
            File.Copy(ScenePath, "Submission_Map/BeforeFenceFix.unity");
        ImportSprite();
        EditorSceneManager.OpenScene(ScenePath);
        ApplyToOpenScene();
        PolishVillage.Verify();
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
        AssetDatabase.SaveAssets();
        PolishVillage.Render("Submission_Map/Garden_Fences_Fixed.png", new Vector2(-21,16.6f), 7.3f, 1400, 1000);
        PolishVillage.Render("Submission_Map/Village_Landscape.png", Vector2.zero, 25, 1600, 1200);
        Debug.Log("GARDEN_FENCES_PASS: connected side fences, aligned collision, entrance and all stage destinations reachable.");
    }

    static void ImportSprite()
    {
        AssetDatabase.ImportAsset(ArtPath, ImportAssetOptions.ForceSynchronousImport);
        var importer = (TextureImporter)AssetImporter.GetAtPath(ArtPath);
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Multiple;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.filterMode = FilterMode.Point;
        importer.alphaIsTransparency = true;
        importer.maxTextureSize = 2048;
        importer.spritePixelsPerUnit = 64;
        importer.isReadable = true;
        importer.SaveAndReimport();
        var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(ArtPath);
        var pixels = tex.GetPixels32();
        int xmin = tex.width, xmax = 0, ymin = tex.height, ymax = 0;
        for (int y = 0; y < tex.height; y++)
        for (int x = 0; x < tex.width; x++)
            if (pixels[y * tex.width + x].a > 24)
            { xmin = Math.Min(xmin,x); xmax = Math.Max(xmax,x); ymin = Math.Min(ymin,y); ymax = Math.Max(ymax,y); }
        if (xmax <= xmin || ymax <= ymin) throw new Exception("Side fence has no visible pixels.");
        var factories = new SpriteDataProviderFactories(); factories.Init();
        var provider = factories.GetSpriteEditorDataProviderFromObject(importer); provider.InitSpriteEditorDataProvider();
        var old = provider.GetSpriteRects().ToDictionary(r => r.name);
        // Contiguous bands preserve the artwork while allowing depth sorting along the fence.
        var rects = Enumerable.Range(0,6).Select(i => {
            int y0 = ymin + (ymax-ymin+1)*i/6, y1 = ymin+(ymax-ymin+1)*(i+1)/6;
            string name = "GardenSide_"+i;
            return new SpriteRect { name=name, rect=new Rect(xmin,y0,xmax-xmin+1,y1-y0),
                alignment=SpriteAlignment.Custom,pivot=new Vector2(.5f,0),
                spriteID=old.TryGetValue(name,out var previous)?previous.spriteID:GUID.Generate() };
        }).ToArray();
        provider.SetSpriteRects(rects);
        provider.GetDataProvider<ISpriteNameFileIdDataProvider>().SetNameFileIdPairs(rects.Select(r=>new SpriteNameFileIdPair(r.name,r.spriteID)));
        provider.Apply(); importer.isReadable = false; importer.SaveAndReimport();
    }

    public static void ApplyToOpenScene()
    {
        var root = GameObject.Find("Village environment sprites").transform;
        var parts = AssetDatabase.LoadAllAssetsAtPath(ArtPath).OfType<Sprite>().OrderBy(s=>s.rect.y).ToArray();
        if(parts.Length != 6) throw new Exception("Import GardenSideFence_v1 with Tuman/Fix Garden Side Fences first.");
        var material = root.GetComponentsInChildren<SpriteRenderer>().First(r=>r.name=="Fence").sharedMaterial;
        foreach(var r in root.GetComponentsInChildren<SpriteRenderer>())
            if(r.name=="FenceVertical" || r.name.StartsWith("Garden side ")) Object.DestroyImmediate(r.gameObject);
        float totalHeight = parts.Sum(s=>s.rect.height), scale = (Top-Bottom)*64/totalHeight;
        foreach(float x in new[]{Left,Right})
        {
            float y=Bottom;
            for(int i=0;i<parts.Length;i++)
            {
                var go=new GameObject("Garden side "+(x==Left?"left ":"right ")+i,typeof(SpriteRenderer));
                go.transform.SetParent(root,false); go.transform.position=new Vector3(x,y,0); go.transform.localScale=Vector3.one*scale;
                var r=go.GetComponent<SpriteRenderer>(); r.sprite=parts[i]; r.sharedMaterial=material;
                r.flipX=x==Right; r.color=new Color(.83f,.83f,.83f,1);
                r.sortingOrder=500-Mathf.RoundToInt(y*10);
                y+=parts[i].rect.height/64*scale;
            }
            if(Mathf.Abs(y-Top)>.001f) throw new Exception("Fence sections do not meet.");
        }
        foreach(var c in root.GetComponentsInChildren<BoxCollider2D>())
        {
            var p=c.transform.position;
            if(Mathf.Abs(p.y-16)<.01f && c.size.y==10 && (Mathf.Abs(p.x+28.5f)<.01f || Mathf.Abs(p.x+11.5f)<.01f))
            { c.transform.position=new Vector3(p.x<-20?Left:Right,16.5f,0); c.size=new Vector2(.6f,10.8f); }
            // Trim invisible ends of the horizontal collision to the newly joined right corner.
            else if(Mathf.Abs(p.x+20)<.01f && Mathf.Abs(p.y-21.5f)<.01f && c.size.x==18)
            { c.transform.position=new Vector3((-29+Right+.3f)/2,21.5f,0); c.size=new Vector2(Right+.3f+29,1); }
            else if(Mathf.Abs(p.x+15)<.01f && Mathf.Abs(p.y-11.5f)<.01f && c.size.x==6)
            { c.transform.position=new Vector3((-18+Right+.3f)/2,11.5f,0); c.size=new Vector2(Right+.3f+18,1); }
        }
    }
}
