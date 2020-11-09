using Unity.Entities;
using Unity.Mathematics;

namespace TD
{

    [GenerateAuthoringComponent]
    public struct StartAmmoIdentifierComponent : IComponentData
    {
        public int ammoIndex;

        public int ammoCount;
    }
}