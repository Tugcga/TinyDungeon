using Unity.Entities;
using Unity.Mathematics;

namespace TD
{
    [GenerateAuthoringComponent]
    public struct AmmoComponent : IComponentData
    {
        public int count;
    }
}