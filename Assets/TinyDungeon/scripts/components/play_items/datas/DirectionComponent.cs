using Unity.Entities;
using Unity.Mathematics;

namespace TD
{
    [GenerateAuthoringComponent]
    public struct DirectionComponent : IComponentData
    {
        public float2 direction;
    }
}
