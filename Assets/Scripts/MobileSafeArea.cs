using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public sealed class MobileSafeArea : MonoBehaviour
{
    private Rect previous;
    private Vector2Int resolution;
    private void OnEnable()=>Apply();
    private void Update()
    {
        if(previous!=Screen.safeArea||resolution!=new Vector2Int(Screen.width,Screen.height))Apply();
    }
    private void Apply()
    {
        if(Screen.width<1||Screen.height<1)return;
        var r=(RectTransform)transform;
        previous=Screen.safeArea;resolution=new Vector2Int(Screen.width,Screen.height);
        r.anchorMin=new Vector2(previous.xMin/Screen.width,previous.yMin/Screen.height);
        r.anchorMax=new Vector2(previous.xMax/Screen.width,previous.yMax/Screen.height);
        r.offsetMin=r.offsetMax=Vector2.zero;
    }
}
