using Unity.Entities;
using Unity.Mathematics;

namespace TD
{
    [GenerateAuthoringComponent]
    public struct BulletComponent : IComponentData
    {
        public Entity explosionPrefab;
        public float explosionLifetime;
        public int damage;
        public float speed;
    }
}