using Unity.Entities;
using Unity.Mathematics;

namespace TD
{
    [GenerateAuthoringComponent]
    public struct DeadCooldawnComponent : IComponentData
    {
        public double startTime;
        public float delayTime;
    }
}
