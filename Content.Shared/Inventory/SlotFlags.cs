using Robust.Shared.Serialization;

namespace Content.Shared.Inventory;

/// <summary>
///     Defines what slot types an item can fit into.
/// </summary>
[Serializable, NetSerializable]
[Flags]
public enum SlotFlags
{
    NONE = 0,
    PREVENTEQUIP = 1 << 0,
    HEAD = 1 << 1,
    EYES = 1 << 2,
    EARS = 1 << 3,
    MASK = 1 << 4,
    OUTERCLOTHING = 1 << 5,
    INNERCLOTHING = 1 << 6,
    LEGS = 1 << 7,
    NECK = 1 << 8,
    BACK = 1 << 9,
    BELT = 1 << 10,
    GLOVES = 1 << 11,
    IDCARD = 1 << 12,
    POCKET = 1 << 13,
    FEET = 1 << 14,
    SUITSTORAGE = 1 << 15,
    All = ~NONE,

    WITHOUT_POCKET = All & ~POCKET
}
