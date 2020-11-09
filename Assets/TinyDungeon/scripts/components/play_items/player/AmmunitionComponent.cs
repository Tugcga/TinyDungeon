using Unity.Entities;
using Unity.Mathematics;

namespace TD
{
    [GenerateAuthoringComponent]
    public struct AmmunitionComponent : IComponentData
    {
        public int maxBulletsCount;
        public int bulletsCount;
    }
}