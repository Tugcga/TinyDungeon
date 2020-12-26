using Unity.Entities;
using Unity.Mathematics;

namespace TD
{
    [GenerateAuthoringComponent]
    public struct SceneTag : IComponentData
    {
        public int index;
    }
}