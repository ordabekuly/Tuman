using UnityEngine;
namespace TumanGame
{
    [System.Serializable] public sealed class DirectionFrames
    {
        public Sprite[] idle, walk, idleShoot, walkShoot;
    }
    [CreateAssetMenu(menuName="Tuman/Animation Library")]
    public sealed class AnimationLibrary : ScriptableObject
    {
        // Down, Up, Left, Right. All frames share the existing centre pivot.
        public DirectionFrames[] directions;
    }
}
