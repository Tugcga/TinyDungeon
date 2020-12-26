using Unity.Entities;
using Unity.Mathematics;

namespace TD
{
    [GenerateAuthoringComponent]
    public struct PlayerSoundComponent : IComponentData
    {
        public Entity shotSound;
        public Entity moveSound;
        public Entity ammoPickupSound;
        public Entity missShotSound;
    }
}