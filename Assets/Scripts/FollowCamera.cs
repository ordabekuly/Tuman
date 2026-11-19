using UnityEngine;
namespace TumanGame
{
    [RequireComponent(typeof(Camera))]
    public sealed class FollowCamera:MonoBehaviour
    {
        public Transform target;
        public Vector2 minimum,maximum;
        public float viewHeight=5.6f,smoothing=8;
        Camera view;
        void Awake(){view=GetComponent<Camera>();}
        Vector3 Position()
        {
            if(!view)view=GetComponent<Camera>();view.orthographicSize=viewHeight;
            float halfX=viewHeight*view.aspect;
            float x=maximum.x-minimum.x<=halfX*2?(minimum.x+maximum.x)*.5f:Mathf.Clamp(target.position.x,minimum.x+halfX,maximum.x-halfX);
            float y=maximum.y-minimum.y<=viewHeight*2?(minimum.y+maximum.y)*.5f:Mathf.Clamp(target.position.y,minimum.y+viewHeight,maximum.y-viewHeight);
            return new Vector3(x,y,transform.position.z);
        }
        public void Snap(){if(target)transform.position=Position();}
        void LateUpdate(){if(target)transform.position=Vector3.Lerp(transform.position,Position(),1-Mathf.Exp(-smoothing*Time.unscaledDeltaTime));}
    }
}
