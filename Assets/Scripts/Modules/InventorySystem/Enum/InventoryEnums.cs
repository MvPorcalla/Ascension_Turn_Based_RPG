// ──────────────────────────────────────────────────
// Assets\Scripts\Modules\InventorySystem\Enum\InventoryEnums.cs
// Inventory-related enumerations
// ──────────────────────────────────────────────────

namespace Ascension.Inventory.Enums
{
    /// <summary>
    /// Item location within the inventory system
    /// Explicit values for save compatibility:
    /// - Storage = 0 (default, unlimited capacity)
    /// - Bag = 1 (player's main inventory)
    /// 
    /// </summary>
    public enum ItemLocation
    {
        Storage = 0,    // Default storage (unlimited, slower access)
        Bag = 2,        // Player bag (12 slots, expandable with equipment)
        Equipped = 3,   // Currently equipped items
        None = -1       // Indicates no valid location (used when all locations full)
    }
}