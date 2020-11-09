using Unity.Entities;
using Unity.Mathematics;

namespace TD
{

    [GenerateAuthoringComponent]
    public struct StartGateIdentifierComponent : IComponentData
    {
        public GateColors gateColor;
        public int gateIndex;  // this index should be different for each gate with the same color
        public bool isActiveGate;
    }
}