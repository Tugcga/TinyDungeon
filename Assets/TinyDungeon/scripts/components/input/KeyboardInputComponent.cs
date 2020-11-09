using Unity.Entities;
using Unity.Mathematics;

namespace TD
{
    [GenerateAuthoringComponent]
    public struct KeyboardInputComponent : IComponentData
    {
        public bool isPressFront;
        public bool isPressBack;
        public bool isPressLeft;
        public bool isPressRight;

        public bool isPressAction;
    }
}
