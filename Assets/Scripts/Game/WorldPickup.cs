using UnityEngine;
namespace TumanGame
{
    public sealed class WorldPickup:MonoBehaviour
    {
        public bool healing;
        public GameSession session;
        void Start()
        {
            var texture=new Texture2D(8,8){filterMode=FilterMode.Point};
            var colors=new Color[64];
            for(int y=0;y<8;y++)for(int x=0;x<8;x++){
                bool visible=healing?(x==3||y==3):(Mathf.Abs(x-3)+Mathf.Abs(y-3)<4);
                colors[y*8+x]=visible?(healing?new Color(.5f,.9f,.45f):new Color(1,.8f,.35f)):Color.clear;
            }
            texture.SetPixels(colors);texture.Apply();
            var sprite=gameObject.AddComponent<SpriteRenderer>();sprite.sortingOrder=10;
            sprite.sprite=Sprite.Create(texture,new Rect(0,0,8,8),new Vector2(.5f,.5f),12);
        }
        void Update()
        {
            if(!session||!session.IsPlaying)return;
            if(Vector2.Distance(transform.position,session.player.transform.position)<=session.items.pickupRadius)
            {session.Collect(healing);Destroy(gameObject);}
        }
    }
}
