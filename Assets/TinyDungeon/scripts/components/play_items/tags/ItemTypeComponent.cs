using Unity.Entities;
using Unity.Mathematics;

namespace TD
{
    public enum ItemType
    {
        ITEM_UNDEFINED,
        ITEM_PLAYER,
        ITEM_TOWER,
        ITEM_BARREL,
        ITEM_GATE,
        ITEM_BULLET,
        ITEM_GATE_SWITCHER
    }

    [GenerateAuthoringComponent]
    public struct ItemTypeComponent : IComponentData
    {
        public ItemType type;
    }
}