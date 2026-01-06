// ══════════════════════════════════════════════════════════════════
// Assets\Scripts\Modules\InventorySystem\Config\InventoryConfig.cs
// Centralized inventory configuration constants
// ✅ REFACTORED: Pocket system completely removed
// ══════════════════════════════════════════════════════════════════

namespace Ascension.Inventory.Config
{
    /// <summary>
    /// SINGLE SOURCE OF TRUTH for inventory slot capacities
    /// Change these values to affect the entire game
    /// </summary>
    public static class InventoryConfig
    {
        // ═══════════════════════════════════════════════════════════
        // 🎮 FEATURE TOGGLES
        // ═══════════════════════════════════════════════════════════
        
        /// <summary>
        /// Enable/disable bag inventory system
        /// Set to FALSE to use storage-only inventory
        /// </summary>
        public const bool ENABLE_BAG = true;
        
        // ═══════════════════════════════════════════════════════════
        // 🎨 UI DISPLAY OPTIONS
        // ═══════════════════════════════════════════════════════════
        
        /// <summary>
        /// Show equipped gear preview in storage room
        /// Replaces the old pocket inventory display
        /// </summary>
        public const bool SHOW_EQUIPPED_GEAR_IN_STORAGE = true;
        
        // ═══════════════════════════════════════════════════════════
        // Default Slot Capacities
        // ═══════════════════════════════════════════════════════════
        
        /// <summary>
        /// Default bag slots (player's main inventory)
        /// </summary>
        public const int DEFAULT_BAG_SLOTS = 12;
        
        /// <summary>
        /// Default storage slots (home storage)
        /// </summary>
        public const int DEFAULT_STORAGE_SLOTS = 60;
        
        // ═══════════════════════════════════════════════════════════
        // Maximum Slot Capacities (for upgrades)
        // ═══════════════════════════════════════════════════════════
        
        /// <summary>
        /// Maximum bag slots after all upgrades
        /// </summary>
        public const int MAX_BAG_SLOTS = 24;
        
        /// <summary>
        /// Maximum storage slots after all upgrades
        /// </summary>
        public const int MAX_STORAGE_SLOTS = 200;
        
        // ═══════════════════════════════════════════════════════════
        // Runtime Helpers
        // ═══════════════════════════════════════════════════════════
        
        /// <summary>
        /// Check if location is enabled in config
        /// </summary>
        public static bool IsLocationEnabled(Ascension.Inventory.Enums.ItemLocation location)
        {
            return location switch
            {
                Ascension.Inventory.Enums.ItemLocation.Bag => ENABLE_BAG,
                Ascension.Inventory.Enums.ItemLocation.Storage => true, // Always enabled
                _ => false // Pocket and None are disabled
            };
        }
    }
}