using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;

public static class VerifyMobileControls
{
    static MobileJoystick joystick;
    static Rigidbody2D body;
    static PointerEventData pointer;
    static Vector2 start;
    static float deadline;
    static int phase;
    static bool moved,released;
    [MenuItem("Tuman/Verify Mobile Controls (Play Mode)")]
    public static void Run()
    {
        if(!EditorApplication.isPlaying)return;
        joystick=Object.FindAnyObjectByType<MobileJoystick>();
        body=GameObject.Find("Player").GetComponent<Rigidbody2D>();
        body.position=new Vector2(-5,-4);body.linearVelocity=Vector2.zero;
        Physics2D.SyncTransforms();Canvas.ForceUpdateCanvases();
        start=body.position;phase=0;moved=released=false;
        var rect=(RectTransform)joystick.transform;
        Vector3 point=rect.TransformPoint(new Vector3(rect.rect.width*.45f,0,0));
        pointer=new PointerEventData(EventSystem.current){pointerId=123,position=RectTransformUtility.WorldToScreenPoint(null,point)};
        joystick.OnPointerDown(pointer);
        deadline=Time.time+.6f;
        EditorApplication.update-=Tick;EditorApplication.update+=Tick;
    }
    static void Tick()
    {
        if(!EditorApplication.isPlaying||!body){EditorApplication.update-=Tick;return;}
        if(Time.time<deadline)return;
        if(phase==0)
        {
            moved=body.position.x>start.x+.5f&&joystick.Value.x>.5f;
            joystick.OnPointerUp(pointer);phase=1;deadline=Time.time+.15f;
        }
        else
        {
            released=joystick.Value==Vector2.zero&&body.linearVelocity.sqrMagnitude<.001f;
            EditorApplication.update-=Tick;
            Directory.CreateDirectory("Submission_Backup");
            string report=$"pointer_drag_moves_player={moved}\npointer_release_stops_player={released}\n";
            File.WriteAllText("Submission_Backup/mobile-verification.txt",report);
            if(moved&&released)Debug.Log("Mobile control verification PASS\n"+report);
            else Debug.LogError("Mobile control verification FAILED\n"+report);
        }
    }
}
