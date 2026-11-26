using UnityEngine;
namespace TumanGame
{
    public sealed class WorldDepthSort : MonoBehaviour
    {
        SpriteRenderer[] sprites;
        void Awake(){sprites=GetComponentsInChildren<SpriteRenderer>(true);}
        void LateUpdate()
        {
            if(sprites==null)return;
            int order=500-Mathf.RoundToInt((transform.position.y-.75f)*10);
            foreach(var sprite in sprites)if(sprite)sprite.sortingOrder=order;
        }
    }
}
