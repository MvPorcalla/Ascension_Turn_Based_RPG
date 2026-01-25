// ══════════════════════════════════════════════════════════════════
// Assets/Scripts/Modules/InventorySystem/Enums/InventoryEnums.cs
// ✅ FIXED: Consistent enum numbering
// ══════════════════════════════════════════════════════════════════

namespace Ascension.Inventory.Enums
{
    /// <summary>
    /// Item location within the inventory system.
    /// Explicit values for save compatibility.
    /// </summary>
    public enum ItemLocation
    {
        /// <summary>Storage (home base, large capacity)</summary>
        Storage = 0,
        /// <summary>Bag (portable inventory, limited slots)</summary>
        Bag = 1,
        /// <summary>Equipped (currently worn/wielded gear)</summary>
        Equipped = 2,
        /// <summary>None (invalid/error state)</summary>
        None = -1
    }
}