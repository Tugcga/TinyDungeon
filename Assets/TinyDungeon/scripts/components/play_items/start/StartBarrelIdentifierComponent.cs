using Unity.Entities;
using Unity.Mathematics;

namespace TD
{

    [GenerateAuthoringComponent]
    public struct StartBarrelIdentifierComponent : IComponentData
    {
        public int barrelIndex;
        public bool isActive;

        public float damageRadius;
    }
}