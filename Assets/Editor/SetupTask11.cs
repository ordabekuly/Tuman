using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.U2D.Sprites;
using UnityEngine;
using UnityEngine.SceneManagement;
using TumanGame;

public static class SetupTask11
{
    const string ScenePath="Assets/Scenes/Tuman11.unity";
    static T Asset<T>(string name) where T:ScriptableObject
    {
        string path="Assets/Data/"+name+".asset";
        var a=AssetDatabase.LoadAssetAtPath<T>(path);
        if(!a){a=ScriptableObject.CreateInstance<T>();AssetDatabase.CreateAsset(a,path);}
        return a;
    }
    [MenuItem("Tuman/Setup Task 11")]
    public static void Run()
    {
        Directory.CreateDirectory("Assets/Data");Directory.CreateDirectory("Assets/Prefabs");
        AssetDatabase.Refresh();
        if(!File.Exists(ScenePath))AssetDatabase.CopyAsset("Assets/Scenes/Courtyard.unity",ScenePath);
        EditorSceneManager.OpenScene(ScenePath);
        var existing=UnityEngine.Object.FindAnyObjectByType<GameSession>();
        if(existing){RefreshAnimations();Debug.Log("Task11 already configured; assets and scene kept.");return;}
        var player=GameObject.Find("Player");var enemy=GameObject.Find("Enemy");
        var p=Asset<PlayerStats>("PlayerStats");
        p.maxHealth=5;p.moveSpeed=4;p.invulnerabilityTime=.75f;p.shotCooldown=.3f;p.shotRange=18;p.projectileSpeed=14;
        p.projectileRadius=.09f;p.shotDamage=1;p.idleFps=6;p.walkFps=10;p.shootingFps=12;p.shootPoseDuration=.34f;p.deathDuration=.6f;
        var item=Asset<ItemValues>("ItemValues");item.enemyScore=100;item.tokenScore=50;item.herbHealing=2;item.completionBonus=250;item.pickupRadius=.6f;
        var enemies=new EnemyStats[3];
        for(int i=0;i<3;i++) {
            var e=Asset<EnemyStats>("Werewolf_"+(i+1));enemies[i]=e;
            e.maxHealth=3;e.damage=1;e.patrolSpeed=1.4f+i*.15f;e.chaseSpeed=2.2f+i*.25f;e.visionRange=7+i;
            e.attackRange=1.55f;e.stopDistance=1.2f;e.arrivalDistance=.15f;e.patrolWait=.7f;e.lostSightTime=1.5f;
            e.attackWindup=.5f;e.attackCooldown=1.2f;e.recoveryTime=.2f;e.deathDuration=.5f;EditorUtility.SetDirty(e);
        }
        var levels=new LevelData[3];
        string[] names={"The home courtyard","Beyond the gate","The last lantern"};
        string[] descriptions={"Find your footing. Reclaim your home.","The mist grows restless. Keep moving.","One final courtyard. Keep your light."};
        Vector2[][] points={new[]{new Vector2(3,-3),new Vector2(6,4)},new[]{new Vector2(4,-3),new Vector2(7,4),new Vector2(-5,2)},
            new[]{new Vector2(4,-3),new Vector2(7,4),new Vector2(-5,2),new Vector2(-6,-4)}};
        for(int i=0;i<3;i++) {
            var l=Asset<LevelData>("Level_0"+(i+1));levels[i]=l;l.levelNumber=i+1;l.title=names[i];l.description=descriptions[i];
            l.goal="Clear the werewolves";l.timeLimit=90+i*30;l.playerSpawn=new Vector2(-6,-3);
            l.enemySpawns=points[i];l.enemyStats=enemies[i];
            l.herbSpawns=new[]{new Vector2(-3,-4)};l.tokenSpawns=new[]{new Vector2(-3,-2),new Vector2(5,-5)};
            l.atmosphere=new Color(.85f-i*.08f,.91f-i*.06f,1);EditorUtility.SetDirty(l);
        }
        player.GetComponent<PlayerMovement>().stats=p;player.GetComponent<PlayerHealth>().stats=p;player.GetComponent<PlayerShooting>().stats=p;
        var oldAnimator=player.GetComponent<Animator>();if(oldAnimator)UnityEngine.Object.DestroyImmediate(oldAnimator);
        var library=Asset<AnimationLibrary>("PlayerAnimations");library.directions=new DirectionFrames[4];
        string[] directions={"Down","Up","Left","Right"};
        for(int i=0;i<4;i++) {
            string d=directions[i];var frames=new DirectionFrames();library.directions[i]=frames;
            frames.idle=Slice("Assets/Tuman_Walk_and_Idle_8Frames/Idle/Idle_"+d+"_8frames_64px.png",8,1,32,false);
            frames.walk=Slice("Assets/Tuman_Walk_and_Idle_8Frames/Walk/Walk_"+d+"_8frames_64px.png",8,1,32,false);
            if(i==0) {
                var down=Slice("Assets/Art/Animations/PlayerShoot/DownWalk.png",8,1,0,true);
                frames.walkShoot=down;frames.idleShoot=new[]{down[0],down[1],down[0],down[3]};
            } else {
                var shoot=Slice("Assets/Art/Animations/PlayerShoot/"+d+".png",8,2,0,true);
                frames.idleShoot=shoot.Take(8).ToArray();frames.walkShoot=shoot.Skip(8).Take(8).ToArray();
            }
        }
        var anim=player.AddComponent<PlayerSpriteAnimation>();anim.library=library;anim.stats=p;
        enemy.SetActive(false);
        enemy.GetComponent<EnemyAI>().Configure(null,Array.Empty<Transform>(),enemies[0]);
        enemy.GetComponent<EnemyHealth>().stats=enemies[0];enemy.AddComponent<EnemyMotion>();
        var prefab=PrefabUtility.SaveAsPrefabAsset(enemy,"Assets/Prefabs/Werewolf.prefab");
        UnityEngine.Object.DestroyImmediate(enemy);
        var mobile=GameObject.Find("Mobile Canvas");if(mobile)UnityEngine.Object.DestroyImmediate(mobile);
        var go=new GameObject("Task 11 - Session");var session=go.AddComponent<GameSession>();
        session.levels=levels;session.items=item;session.player=player.GetComponent<PlayerHealth>();session.enemyTemplate=prefab;
        session.font=AssetDatabase.LoadAssetAtPath<Font>("Assets/UI/Fonts/Arial.ttf");
        var bg=AssetImporter.GetAtPath("Assets/UI/Art/VillageEvening.png") as TextureImporter;
        if(bg){bg.textureType=TextureImporterType.Sprite;bg.spriteImportMode=SpriteImportMode.Single;bg.SaveAndReimport();}
        session.menuBackground=AssetDatabase.LoadAssetAtPath<Sprite>("Assets/UI/Art/VillageEvening.png");
        var light=UnityEngine.Object.FindAnyObjectByType<UnityEngine.Rendering.Universal.Light2D>();if(light)light.intensity=.9f;
        EditorUtility.SetDirty(p);EditorUtility.SetDirty(item);EditorUtility.SetDirty(library);AssetDatabase.SaveAssets();
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
        EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene(ScenePath,true)};
        PlayerSettings.defaultInterfaceOrientation=UIOrientation.AutoRotation;
        PlayerSettings.allowedAutorotateToPortrait=false;PlayerSettings.allowedAutorotateToPortraitUpsideDown=false;
        PlayerSettings.allowedAutorotateToLandscapeLeft=true;PlayerSettings.allowedAutorotateToLandscapeRight=true;
        PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.Android,false);
        PlayerSettings.SetGraphicsAPIs(BuildTarget.Android,new[]{UnityEngine.Rendering.GraphicsDeviceType.OpenGLES3});
        Debug.Log("TASK11_SETUP_PASS: data assets, 3 levels, player animation, game session, original map preserved.");
    }
    public static void RefreshAnimations()
    {
        var library=AssetDatabase.LoadAssetAtPath<AnimationLibrary>("Assets/Data/PlayerAnimations.asset");
        var down=Slice("Assets/Art/Animations/PlayerShoot/Down.png",8,2,0,true);
        library.directions[0].idleShoot=down.Take(8).ToArray();library.directions[0].walkShoot=down.Skip(8).Take(8).ToArray();
        string[] others={"Up","Left","Right"};
        for(int i=0;i<others.Length;i++) {
            var frames=Slice("Assets/Art/Animations/PlayerShoot/"+others[i]+".png",8,2,0,true);
            int index=others[i]=="Up"?1:others[i]=="Left"?2:3;
            library.directions[index].idleShoot=frames.Take(8).ToArray();library.directions[index].walkShoot=frames.Skip(8).Take(8).ToArray();
        }
        EditorUtility.SetDirty(library);AssetDatabase.SaveAssets();
    }
    public static Sprite[] Slice(string path,int columns,int rows,float ppu,bool trim)
    {
        var importer=(TextureImporter)AssetImporter.GetAtPath(path);
        if(!importer)throw new FileNotFoundException(path);
        importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Multiple;
        importer.filterMode=FilterMode.Point;importer.textureCompression=TextureImporterCompression.Uncompressed;
        importer.maxTextureSize=4096;importer.isReadable=true;importer.SaveAndReimport();
        var texture=AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        int width=texture.width/columns,height=texture.height/rows;
        var pixels=trim?texture.GetPixels32():null;
        var rects=new SpriteRect[columns*rows];
        int globalMin=height,globalMax=0;
        // Ignore the outer border: generated sheets may contain detached edge artifacts.
        if(trim)for(int row=0;row<rows;row++)for(int col=0;col<columns;col++) {
            int y0=texture.height-(row+1)*height;
            for(int y=height/12;y<height*11/12;y++)for(int x=width/7;x<width*6/7;x++) {
                var color=pixels[(y0+y)*texture.width+col*width+x];
                if(color.a>230 && color.r<220 && color.g<220 && color.b<220) {globalMin=Math.Min(globalMin,y);globalMax=Math.Max(globalMax,y);}
            }
        }
        if(trim&&globalMax<=globalMin)throw new InvalidDataException("No visible character in "+path);
        if(trim)ppu=(globalMax-globalMin+1)/1.4f;
        if(trim&&path.EndsWith("/Up.png"))ppu=height*.60f/1.4f;
        importer.spritePixelsPerUnit=ppu;
        for(int row=0;row<rows;row++)for(int col=0;col<columns;col++) {
            int y0=texture.height-(row+1)*height;
            int y=trim?Math.Max(0,globalMin-3):0,h=trim?Math.Min(height-y,globalMax-globalMin+7):height;
            rects[row*columns+col]=new SpriteRect{name="Frame_"+(row*columns+col).ToString("D2"),rect=new Rect(col*width,y0+y,width,h),
                alignment=trim?SpriteAlignment.Custom:SpriteAlignment.Center,pivot=trim?new Vector2(.5f,.7f*ppu/h):new Vector2(.5f,.5f),spriteID=GUID.Generate()};
        }
        var factory=new SpriteDataProviderFactories();factory.Init();var provider=factory.GetSpriteEditorDataProviderFromObject(importer);
        provider.InitSpriteEditorDataProvider();provider.SetSpriteRects(rects);
        var ids=provider.GetDataProvider<ISpriteNameFileIdDataProvider>();
        ids.SetNameFileIdPairs(rects.Select(r=>new SpriteNameFileIdPair(r.name,r.spriteID)));
        provider.Apply();importer.SaveAndReimport();
        return AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().OrderBy(s=>s.name).ToArray();
    }
}
