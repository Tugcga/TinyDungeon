using Unity.Entities;
using Unity.Mathematics;

namespace TD
{
    [GenerateAuthoringComponent]
    public struct TowerSoundComponent : IComponentData
    {
        public Entity shotSound;
        public Entity alarmSound;
    }
}