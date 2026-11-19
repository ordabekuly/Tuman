using UnityEngine;
namespace TumanGame
{
    [CreateAssetMenu(menuName="Tuman/Enemy Stats")]
    public sealed class EnemyStats : ScriptableObject
    {
        [Min(1)] public int maxHealth, damage;
        [Min(.1f)] public float patrolSpeed, chaseSpeed, visionRange, attackRange, stopDistance;
        [Min(.01f)] public float arrivalDistance;
        [Min(0)] public float patrolWait, lostSightTime, attackWindup, attackCooldown, recoveryTime, deathDuration;
        [Min(1)] public float idleFps, walkFps, runFps;
    }
}
