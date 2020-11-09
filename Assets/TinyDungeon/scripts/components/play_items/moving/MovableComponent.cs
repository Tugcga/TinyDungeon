using Unity.Entities;
using Unity.Mathematics;

namespace TD
{
    [GenerateAuthoringComponent]
    public struct MovableComponent : IComponentData
    {
        public float2 position;
        public float2 nextPosition;

        public bool isMove;  // turn on, when the entity start moving

        //this property used for navmesh based on bvh-tree
        public int lastTriangleIndex;  // by default should be -1
    }
}
