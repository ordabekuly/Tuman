using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using TumanGame;
public static class ApplyControlSkin
{
    public static void Run()
    {
        EditorSceneManager.OpenScene("Assets/Scenes/Tuman11.unity");
        var session=Object.FindAnyObjectByType<GameSession>();
        Sprite Load(string name) {
            string path="Assets/UI/Controls/"+name+".png";
            var importer=(TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Single;
            importer.spritePixelsPerUnit=100;importer.alphaIsTransparency=true;importer.isReadable=true;
            importer.filterMode=FilterMode.Bilinear;importer.textureCompression=TextureImporterCompression.Uncompressed;
            importer.maxTextureSize=1024;importer.SaveAndReimport();return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }
        session.joystickBase=Load("JoystickBaseGold");session.joystickKnob=Load("JoystickKnobGold");session.fireIcon=Load("FireFlame");
        EditorUtility.SetDirty(session);EditorSceneManager.MarkSceneDirty(session.gameObject.scene);EditorSceneManager.SaveScene(session.gameObject.scene);
        Debug.Log("CONTROL_SKIN_APPLIED: gold joystick base/knob, transparent flaming FIRE icon.");
    }
}
