using Unity.Entities;
using Unity.Mathematics;

namespace TD
{

    [GenerateAuthoringComponent]
    public struct StartTowerIdentifierComponent : IComponentData
    {
        public int towerIndex;
        public bool isActive;

        //search properties
        public float searchAngle;
        public float visibleDistance;
        public float visibleAngle;
    }
}