using Unity.Entities;
using Unity.Mathematics;

namespace TD
{
    public enum HostTypes
    {
        HOST_PLAYER,
        HOST_ENEMY
    }

    public struct HostTypeComponent : IComponentData
    {
        public HostTypes host;
    }
}