using Unity.Entities;

namespace TD
{
    [GenerateAuthoringComponent]
    public struct RotateComponent : IComponentData
    {
        public float Speed;
    }
}

