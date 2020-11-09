using Unity.Entities;
using Unity.Mathematics;

namespace TD
{
    [GenerateAuthoringComponent]
    public struct StartInstantiateComponent : IComponentData
    {
        public Entity prefab;
    }
}