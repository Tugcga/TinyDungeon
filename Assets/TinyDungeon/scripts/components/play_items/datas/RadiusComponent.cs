using Unity.Entities;
using Unity.Mathematics;

namespace TD
{
    [GenerateAuthoringComponent]
    public struct RadiusComponent : IComponentData
    {
        public float Value;
    }
}