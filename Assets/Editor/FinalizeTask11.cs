using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.U2D.Sprites;
using UnityEngine;
using TumanGame;
public static class FinalizeTask11
{
    public static void Run()
    {
        SetupTask11.RefreshAnimations();
        const string path="Assets/Art/Animations/Werewolf/Actions.png";
        var importer=(TextureImporter)AssetImporter.GetAtPath(path);
        importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Multiple;
        importer.spritePixelsPerUnit=100;importer.filterMode=FilterMode.Point;importer.textureCompression=TextureImporterCompression.Uncompressed;
        importer.maxTextureSize=4096;importer.SaveAndReimport();
        var texture=AssetDatabase.LoadAssetAtPath<Texture2D>(path);int rowHeight=texture.height/4;
        // The generator returned seven columns, with the last clipped. Keep six complete columns.
        var rects=new SpriteRect[24];
        for(int row=0;row<4;row++)for(int col=0;col<6;col++)
            rects[row*6+col]=new SpriteRect{name="Action_"+row+"_"+col,rect=new Rect(25+col*260,texture.height-(row+1)*rowHeight,235,rowHeight),alignment=SpriteAlignment.Center,pivot=new Vector2(.5f,.5f),spriteID=GUID.Generate()};
        var factory=new SpriteDataProviderFactories();factory.Init();var provider=factory.GetSpriteEditorDataProviderFromObject(importer);
        provider.InitSpriteEditorDataProvider();provider.SetSpriteRects(rects);
        provider.GetDataProvider<ISpriteNameFileIdDataProvider>().SetNameFileIdPairs(rects.Select(r=>new SpriteNameFileIdPair(r.name,r.spriteID)));
        provider.Apply();importer.SaveAndReimport();
        var frames=AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().OrderBy(s=>s.name).ToArray();
        string prefab="Assets/Prefabs/Werewolf.prefab";var root=PrefabUtility.LoadPrefabContents(prefab);
        var motion=root.GetComponent<EnemyMotion>();motion.idleFrames=frames.Take(6).ToArray();motion.walkFrames=frames.Skip(6).Take(6).ToArray();
        motion.attackFrames=frames.Skip(12).Take(6).ToArray();motion.deathFrames=frames.Skip(18).Take(6).ToArray();
        root.GetComponent<SpriteRenderer>().sprite=motion.idleFrames[0];
        PrefabUtility.SaveAsPrefabAsset(root,prefab);PrefabUtility.UnloadPrefabContents(root);
        foreach(string guid in AssetDatabase.FindAssets("t:EnemyStats",new[]{"Assets/Data"})) {
            var stats=AssetDatabase.LoadAssetAtPath<EnemyStats>(AssetDatabase.GUIDToAssetPath(guid));
            stats.idleFps=5;stats.walkFps=8;stats.runFps=12;EditorUtility.SetDirty(stats);
        }
        AssetDatabase.SaveAssets();
        EditorSceneManager.OpenScene("Assets/Scenes/Tuman11.unity");
        Debug.Log("TASK11_ANIMATIONS_FINALIZED");
    }
}
