using UnityEngine;
namespace TumanGame
{
    [CreateAssetMenu(menuName="Tuman/Item Values")]
    public sealed class ItemValues : ScriptableObject
    {
        [Min(1)] public int enemyScore, tokenScore, herbHealing, completionBonus;
        [Min(.1f)] public float pickupRadius;
    }
}
