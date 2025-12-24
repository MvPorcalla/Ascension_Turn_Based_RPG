// ──────────────────────────────────────────────────
// Assets\Scripts\Modules\InventorySystem\Enum\InventoryEnums.cs
// Inventory-related enumerations
// ──────────────────────────────────────────────────

namespace Ascension.Inventory.Enums
{
    /// <summary>
    /// Item location within the inventory system
    /// Explicit values match migration rules:
    /// - Storage = 0 (default when both bools are false)
    /// - Pocket = 1 (when isInPocket was true)
    /// - Bag = 2 (when isInBag was true)
    /// </summary>
    public enum ItemLocation
    {
        Storage = 0,    // Default storage (unlimited, slower access)
        Pocket = 1,     // Quick access (6 slots, consumables/materials/misc only)
        Bag = 2,        // Player bag (12 slots, expandable with equipment)
        None = -1       // ✅ NEW: Indicates no valid location (used when all locations full)
    }
}