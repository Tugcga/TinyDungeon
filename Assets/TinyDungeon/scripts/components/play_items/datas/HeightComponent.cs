using Unity.Entities;
using Unity.Mathematics;

namespace TD
{
    [GenerateAuthoringComponent]
    public struct HeightComponent : IComponentData
    {
        public float Value;
    }
}
