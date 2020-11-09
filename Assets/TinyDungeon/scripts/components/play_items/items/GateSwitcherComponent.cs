using Unity.Entities;
using Unity.Mathematics;

namespace TD
{
    public enum GateColors
    {
        GATE_INDEFINED,
        GATE_RED,
        GATE_GREEN,
        GATE_BLUE,
        GATE_WHITE
    }

    //use for identify, what type of object corresponds to the given collision edge
    //for gates additional identifier is color, for barrels tis is numerical number
    public enum ColliderType
    {
        COLLIDER_UNDEFINED,
        COLLIDER_GATE,
        COLLIDER_BARELL,
        COLLIDER_TOWER
    }

    [GenerateAuthoringComponent]
    public struct GateSwitcherComponent : IComponentData
    {
        public float radius;
        public GateColors gateColor;
        public bool isActive;

        public double lastActionTime;
        public float actionCooldawn;
    }
}