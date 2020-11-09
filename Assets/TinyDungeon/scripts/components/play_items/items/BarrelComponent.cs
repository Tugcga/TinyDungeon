using Unity.Entities;
using Unity.Mathematics;

namespace TD
{
    [GenerateAuthoringComponent]
    public struct BarrelComponent : IComponentData
    {
        //public int barrelIndex;
        public bool isActive;
    }
}