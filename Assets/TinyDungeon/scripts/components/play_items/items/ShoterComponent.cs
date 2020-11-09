using Unity.Entities;
using Unity.Mathematics;

namespace TD
{
    [GenerateAuthoringComponent]
    public struct ShoterComponent : IComponentData
    {
        //public Entity bulletPrefab;
        public double shootLastTime;
        public float shootCooldawn;
    }
}
