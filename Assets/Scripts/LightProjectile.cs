using UnityEngine;

public sealed class LightProjectile : MonoBehaviour
{
    private Vector2 direction;
    private float speed, remaining;
    private GameObject owner;
    private bool resolved;
    private int damage;
    private float radius;
    private static Sprite dot;
    private readonly RaycastHit2D[] hits = new RaycastHit2D[64];

    public static void Create(Vector2 origin, Vector2 direction, float speed, float range, GameObject owner, int damage=1, float radius=.09f)
    {
        var go = new GameObject("Light Projectile");
        go.transform.position = origin;
        if (!dot)
        {
            var texture = new Texture2D(8, 8) { filterMode = FilterMode.Point };
            var pixels = new Color[64];
            for (int y = 0; y < 8; y++)
                for (int x = 0; x < 8; x++)
                    pixels[y * 8 + x] = new Vector2(x - 3.5f, y - 3.5f).sqrMagnitude <= 14 ? Color.white : Color.clear;
            texture.SetPixels(pixels); texture.Apply();
            dot = Sprite.Create(texture, new Rect(0, 0, 8, 8), new Vector2(.5f, .5f), 32);
        }
        var renderer = go.AddComponent<SpriteRenderer>();
        renderer.sprite = dot; renderer.color = new Color(1, .85f, .25f); renderer.sortingOrder = 20;
        var projectile = go.AddComponent<LightProjectile>();
        projectile.direction = direction; projectile.speed = speed; projectile.remaining = range; projectile.owner = owner;
        projectile.damage=damage; projectile.radius=radius;
    }

    private void FixedUpdate()
    {
        if (resolved) return;
        float step = Mathf.Min(speed * Time.fixedDeltaTime, remaining);
        var filter = new ContactFilter2D();
        filter.SetLayerMask(Physics2D.DefaultRaycastLayers); filter.useTriggers = false;
        int count = Physics2D.CircleCast(transform.position, radius, direction, filter, hits, step);
        RaycastHit2D? nearest = null;
        for (int i = 0; i < count; i++)
        {
            var hit = hits[i];
            if (owner && (hit.transform == owner.transform || hit.transform.IsChildOf(owner.transform))) continue;
            if (!nearest.HasValue || hit.distance < nearest.Value.distance) nearest = hit;
        }
        if (nearest.HasValue)
        {
            resolved = true;
            var enemy = nearest.Value.collider.GetComponentInParent<EnemyHealth>();
            if (enemy) enemy.TakeDamage(damage);
            Destroy(gameObject);
            return;
        }
        // Fail closed if the non-alloc query buffer was filled.
        if (count == hits.Length) { Destroy(gameObject); return; }
        transform.position += (Vector3)(direction * step);
        remaining -= step;
        if (remaining <= 0) Destroy(gameObject);
    }
}
