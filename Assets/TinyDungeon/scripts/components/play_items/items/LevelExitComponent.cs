using Unity.Entities;
using Unity.Mathematics;

namespace TD
{
    [GenerateAuthoringComponent]
    public struct LevelExitComponent : IComponentData
    {
        public int levelIndex;  // index of the next level
        public float activeRadius;  // distance from the center to the player, when the exit trigger should fire
    }
}