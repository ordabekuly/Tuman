using UnityEngine;
namespace TumanGame
{
    public sealed class EnemyBolt:MonoBehaviour
    {
        GameObject owner;EnemyStats stats;Vector2 direction;float remaining;
        readonly RaycastHit2D[] hits=new RaycastHit2D[64];static Sprite sprite;
        public static void Create(Vector2 origin,Vector2 target,GameObject owner,EnemyStats stats)
        {
            var obj=new GameObject("Ghost Bolt");obj.transform.position=origin;
            var renderer=obj.AddComponent<SpriteRenderer>();renderer.sortingOrder=15;
            if(!sprite) {
                var texture=new Texture2D(8,8){filterMode=FilterMode.Point};var colors=new Color[64];
                for(int y=0;y<8;y++)for(int x=0;x<8;x++)colors[y*8+x]=Mathf.Abs(x-3)+Mathf.Abs(y-3)<4?Color.white:Color.clear;
                texture.SetPixels(colors);texture.Apply();sprite=Sprite.Create(texture,new Rect(0,0,8,8),new Vector2(.5f,.5f),20);
            }
            renderer.sprite=sprite;renderer.color=new Color(.2f,.9f,1);
            var bolt=obj.AddComponent<EnemyBolt>();bolt.owner=owner;bolt.stats=stats;bolt.remaining=stats.projectileRange;
            bolt.direction=(target-origin).normalized;
        }
        void FixedUpdate()
        {
            if(GameSession.Instance&&!GameSession.Instance.IsPlaying)return;
            float step=Mathf.Min(stats.projectileSpeed*Time.fixedDeltaTime,remaining);
            var filter=new ContactFilter2D();filter.SetLayerMask(Physics2D.DefaultRaycastLayers);filter.useTriggers=false;
            int count=Physics2D.CircleCast(transform.position,stats.projectileRadius,direction,filter,hits,step);
            RaycastHit2D? nearest=null;
            for(int i=0;i<count;i++) {
                var hit=hits[i];
                if(hit.collider.GetComponentInParent<EnemyHealth>())continue;
                if(owner&&(hit.transform==owner.transform||hit.transform.IsChildOf(owner.transform)))continue;
                if(!nearest.HasValue||hit.distance<nearest.Value.distance)nearest=hit;
            }
            if(nearest.HasValue) {
                var player=nearest.Value.collider.GetComponentInParent<PlayerHealth>();if(player)player.TakeDamage(stats.damage);
                Destroy(gameObject);return;
            }
            if(count==hits.Length){Destroy(gameObject);return;}
            transform.position+=(Vector3)(direction*step);remaining-=step;
            if(remaining<=0)Destroy(gameObject);
        }
    }
}
