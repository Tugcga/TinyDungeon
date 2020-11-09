using Unity.Entities;
using Unity.Mathematics;

namespace TD
{
    [GenerateAuthoringComponent]
    public struct PlayerPositionComponent : IComponentData
    {
        public float2 position;
        public bool isActive;  // turn off, if the player is dead
    }
}
