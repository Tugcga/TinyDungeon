using Unity.Entities;
using Unity.Mathematics;

namespace TD
{
    /*
     * This cimponent use for applying damage, not immediatly, but after some random delay
     * This component assigns to entity when it apply damage from explosion
     */
    [GenerateAuthoringComponent]
    public struct DelayDamageComponent : IComponentData
    {
        public int damage;
        public double startTime;
        public float delay;
    }
}