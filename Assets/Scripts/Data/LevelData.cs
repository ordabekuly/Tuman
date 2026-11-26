using UnityEngine;
namespace TumanGame
{
    [CreateAssetMenu(menuName="Tuman/Level Data")]
    public sealed class LevelData : ScriptableObject
    {
        [Min(1)] public int levelNumber;
        public string title;
        [TextArea] public string description;
        [TextArea] public string goal;
        [Min(1)] public float timeLimit;
        public Vector2 playerSpawn;
        public Vector2[] enemySpawns;
        public Vector2[] ghostSpawns;
        public Vector2[] herbSpawns, tokenSpawns;
        public EnemyStats enemyStats;
        public EnemyStats ghostStats;
        public Color atmosphere;
        public int WerewolfCount => enemySpawns == null ? 0 : enemySpawns.Length;
        public int GhostCount => ghostSpawns == null ? 0 : ghostSpawns.Length;
        public int EnemyCount => WerewolfCount + GhostCount;
    }
}
