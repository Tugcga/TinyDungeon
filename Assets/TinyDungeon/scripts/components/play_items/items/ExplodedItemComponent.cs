using Unity.Entities;
using Unity.Mathematics;

namespace TD
{
    [GenerateAuthoringComponent]
    public struct ExplodedItemComponent : IComponentData
    {
        public Entity explosionPrefab;
        public int damage;
        public float delayMinimum;
        public float delayMaximum;
        public float damageRadius;
    }
}