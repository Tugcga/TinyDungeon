using Unity.Entities;
using Unity.Mathematics;

namespace TD
{
    [GenerateAuthoringComponent]
    public struct RandomComponent : IComponentData
    {
        public Random random;
    }
}
