using Unity.Entities;
using Unity.Mathematics;

namespace TD
{

    [GenerateAuthoringComponent]
    public struct StartLevelExitIdentifierComponent : IComponentData
    {
        public int levelIndex;
        public float activeRadius;
    }
}