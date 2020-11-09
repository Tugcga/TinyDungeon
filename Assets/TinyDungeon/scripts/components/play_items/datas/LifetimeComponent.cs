using Unity.Entities;
using Unity.Mathematics;

namespace TD
{
    [GenerateAuthoringComponent]
    public struct LifetimeComponent : IComponentData
    {
        public double lifeTime;
        public double startTime;
    }
}