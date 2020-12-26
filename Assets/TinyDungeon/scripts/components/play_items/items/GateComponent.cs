using Unity.Entities;
using Unity.Mathematics;

namespace TD
{
    [GenerateAuthoringComponent]
    public struct GateComponent : IComponentData
    {
        public GateColors gateColor;
        public bool isActive;

        public Entity soundOpenClose;
    }
}