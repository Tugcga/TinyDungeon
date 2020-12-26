using Unity.Entities;
using Unity.Mathematics;

namespace TD
{
    [GenerateAuthoringComponent]
    public struct PlayerComponent : IComponentData
    {
        public Entity bulletPrefab;
        public float bulletLifetime;

        public float speed;
        public float2 direction;

        public Entity shotFlash;
        public float flashLifetime;
    }
}
