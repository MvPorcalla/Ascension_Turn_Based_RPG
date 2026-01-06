// ══════════════════════════════════════════════════════════════════
// Assets\Scripts\Modules\InventorySystem\Config\InventoryConfig.cs
// Centralized inventory configuration constants
// ✅ CLEANED: Removed unnecessary ENABLE_BAG toggle
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
        // 🎨 UI DISPLAY OPTIONS
        // ═══════════════════════════════════════════════════════════
        
        /// <summary>
        /// Show equipped gear preview in storage room
        /// Visual-only option - doesn't affect gameplay
        /// </summary>
        public const bool SHOW_EQUIPPED_GEAR_IN_STORAGE = true;
        
        // ═══════════════════════════════════════════════════════════
        // Default Slot Capacities
        // ═══════════════════════════════════════════════════════════
        
        /// <summary>
        /// Default bag slots (player's portable inventory)
        /// </summary>
        public const int DEFAULT_BAG_SLOTS = 12;
        
        /// <summary>
        /// Default storage slots (home/base storage)
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
    }
}