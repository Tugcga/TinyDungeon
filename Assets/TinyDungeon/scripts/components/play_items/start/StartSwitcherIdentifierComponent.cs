using Unity.Entities;
using Unity.Mathematics;

namespace TD
{

    [GenerateAuthoringComponent]
    public struct StartSwitcherIdentifierComponent : IComponentData
    {
        public int switcherIndex;
        public bool isActive;

        public float radius;
        public GateColors color;
    }
}