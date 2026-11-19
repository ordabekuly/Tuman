using UnityEngine;
using UnityEngine.InputSystem;

[DisallowMultipleComponent]
public sealed class PlayerShooting : MonoBehaviour
{
    public TumanGame.PlayerStats stats;
    public Vector2 AimDirection { get; private set; } = Vector2.down;
    public float LastShotTime { get; private set; } = float.NegativeInfinity;
    [SerializeField] private float cooldown;
    [SerializeField] private float range;
    [SerializeField] private float projectileSpeed;
    private float nextShot;
    private PlayerHealth health;
    public int ShotsFired { get; private set; }

    private void Awake() => health = GetComponent<PlayerHealth>();
    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame) Fire();
    }

    // One tap fires one projectile, aimed at the nearest living enemy.
    public void Fire()
    {
        if(TumanGame.GameSession.Instance && !TumanGame.GameSession.Instance.IsPlaying)return;
        if (!isActiveAndEnabled || Time.timeScale <= 0 || (health && !health.IsAlive) || Time.time < nextShot) return;
        var ownCollider = GetComponent<Collider2D>();
        Vector2 origin = ownCollider ? ownCollider.bounds.center : transform.position;
        EnemyHealth target = null;
        Vector2 targetPosition = origin;
        float shotRange=stats ? stats.shotRange : range;
        float nearest = shotRange * shotRange;
        foreach (var enemy in FindObjectsByType<EnemyHealth>())
        {
            if (!enemy.IsAlive) continue;
            var collider = enemy.GetComponent<Collider2D>();
            Vector2 point = collider ? collider.bounds.center : enemy.transform.position;
            float distance = (point - origin).sqrMagnitude;
            if (distance > nearest) continue;
            nearest = distance; target = enemy; targetPosition = point;
        }
        if(target) AimDirection=(targetPosition-origin).normalized;
        else { var body=GetComponent<Rigidbody2D>(); if(body && body.linearVelocity.sqrMagnitude>.01f)AimDirection=body.linearVelocity.normalized; }
        nextShot = Time.time + (stats ? stats.shotCooldown : cooldown);
        LastShotTime = Time.time;
        ShotsFired++;
        LightProjectile.Create(origin, AimDirection, stats ? stats.projectileSpeed : projectileSpeed, shotRange, gameObject,
            stats ? stats.shotDamage : 1, stats ? stats.projectileRadius : .09f);
        if(TumanGame.GameSession.Instance)TumanGame.GameSession.Instance.Sound.PlayShot();
    }
}
