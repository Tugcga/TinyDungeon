using Unity.Entities;
using Unity.Mathematics;

namespace TD
{
    [GenerateAuthoringComponent]
    public struct MouseInputComponent : IComponentData
    {
        public bool isLeftPress;
        public bool isRightPress;

        public float2 leftPressPosition;
        public float2 rightPressPosition;
        
        public float2 currentPosition;
        public float2 delta;
        public float deltaTime;

        public float2 mouseGroundPosition;
    }
}
