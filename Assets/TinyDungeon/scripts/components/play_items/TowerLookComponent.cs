using Unity.Entities;
using Unity.Mathematics;

namespace TD
{
    [GenerateAuthoringComponent]
    public struct TowerLookComponent : IComponentData
    {
        public float angle;
    }
}
