using Unity.Entities;
using Unity.Mathematics;

namespace TD
{
    public struct StartAtackPlayerTag : IComponentData
    {
        public float2 atackDirection; // vector to the point of the atacker
    }
}