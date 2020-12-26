using Unity.Entities;
using Unity.Mathematics;

namespace TD
{
    [GenerateAuthoringComponent]
    public struct UIGlobalStateComponent : IComponentData
    {
        public bool isGearActive;
        public float activeUIHeight;
        public float nonActiveUIHeight;
    }
}