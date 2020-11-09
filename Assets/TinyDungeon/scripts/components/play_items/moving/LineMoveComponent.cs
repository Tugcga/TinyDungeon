using Unity.Entities;
using Unity.Mathematics;

namespace TD
{
    /*
     * Used for entities that should move directly along the line
     */
    [GenerateAuthoringComponent]
    public struct LineMoveComponent : IComponentData
    {
        public float2 startPoint;
        public float2 endPoint;
        public bool isFreeLife;  // if turn off, then the end of the line in the collision edge

        public float2 currentPoint;
    }
}