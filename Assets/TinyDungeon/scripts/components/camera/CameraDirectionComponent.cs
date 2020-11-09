using Unity.Entities;
using Unity.Mathematics;

namespace TD
{
    [GenerateAuthoringComponent]
    public struct CameraDirectionComponent : IComponentData
    {
        public float2 forward;
        public float2 right;
    }
}
