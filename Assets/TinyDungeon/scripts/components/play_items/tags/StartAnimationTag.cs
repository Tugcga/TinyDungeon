using Unity.Entities;
using Unity.Mathematics;

namespace TD
{
    public struct StartAnimationTag : IComponentData
    {
        public int animationIndex;
    }
}