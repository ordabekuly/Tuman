using UnityEngine;
namespace TumanGame
{
    [CreateAssetMenu(menuName="Tuman/Player Stats")]
    public sealed class PlayerStats : ScriptableObject
    {
        [Min(1)] public int maxHealth;
        [Min(.1f)] public float moveSpeed;
        [Min(0)] public float invulnerabilityTime;
        [Min(.05f)] public float shotCooldown;
        [Min(.1f)] public float shotRange, projectileSpeed, projectileRadius;
        [Min(1)] public int shotDamage;
        [Min(1)] public float idleFps, walkFps, shootingFps;
        [Min(.05f)] public float shootPoseDuration, deathDuration;
    }
}
