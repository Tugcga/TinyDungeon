using Unity.Entities;
using Unity.Mathematics;

namespace TD
{
    [GenerateAuthoringComponent]
    public struct LifeComponent : IComponentData
    {
        public int maxLife;
        public int life;
    }
}
