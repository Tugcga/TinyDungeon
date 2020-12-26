using Unity.Entities;
using Unity.Mathematics;

namespace TD
{
    [GenerateAuthoringComponent]
    public struct PlayerAnimationsComponent : IComponentData
    {
        //0 - iddle, 1 - forward, 2 - back, 3 - right, 4 - left, 5 - die
        public int currentAnimation;
        public int newAimation;
    }
}
