// ══════════════════════════════════════════════════════════════════
// Assets\Scripts\Modules\InventorySystem\Config\InventoryConfig.cs
// Centralized inventory configuration constants
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
        // Default Slot Capacities
        // ═══════════════════════════════════════════════════════════
        
        /// <summary>
        /// Default bag slots (player's main inventory)
        /// </summary>
        public const int DEFAULT_BAG_SLOTS = 12;
        
        /// <summary>
        /// Default pocket slots (quick access for consumables)
        /// </summary>
        public const int DEFAULT_POCKET_SLOTS = 6;
        
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
        /// Maximum pocket slots after all upgrades
        /// </summary>
        public const int MAX_POCKET_SLOTS = 12;
        
        /// <summary>
        /// Maximum storage slots after all upgrades
        /// </summary>
        public const int MAX_STORAGE_SLOTS = 200;
        
        // ═══════════════════════════════════════════════════════════
        // Upgrade Costs (optional, for future upgrade system)
        // ═══════════════════════════════════════════════════════════

    }
}