using Unity.Entities;
using Unity.Mathematics;

namespace TD
{
    [GenerateAuthoringComponent]
    public struct UIFadeScreenComponent : IComponentData
    {
        public bool isActive;
        public bool direction;  // true - increase the alpha (fade in), false - decrease it (fade out)

        public float speed;
        public float alphaValue;
    }
}