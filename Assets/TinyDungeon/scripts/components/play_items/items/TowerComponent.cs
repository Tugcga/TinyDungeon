using Unity.Entities;
using Unity.Mathematics;

namespace TD
{
    public enum TowerState
    {
        STATE_SEARCH,
        STATE_TARGET,
        STATE_RETURN_TO_SEARCH,
        STATE_CHECK_DIRECTION,
        STATE_WAIT
    }

    [GenerateAuthoringComponent]
    public struct TowerComponent : IComponentData
    {
        public Entity bulletPrefab;
        public Entity flashPrefab;
        public float bulletLifetime;
        public float flashLifetime;

        public float atackTargetRadius;  // how wide the target, the system calculate when it should emit the bullet with repect to distance to the target

        public float waitTime; // in seconds, for state STATE_WAIT (we use this state after cheking drection)
        public float shotDelta;
        public bool isActive;
        public Entity weaponCorner;
        public float3 weaponCornerPosition;
    }
}