using Unity.Entities;
using Unity.Mathematics;

namespace TD
{
    [GenerateAuthoringComponent]
    public struct LineMoveInitTag : IComponentData
    {
        public float2 hostPosition;
    }
}