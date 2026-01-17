// ════════════════════════════════════════════
// Assets/Scripts/UI/Popups/PopupContext.cs
// Defines available actions based on where popup was opened
// ════════════════════════════════════════════

using Ascension.Inventory.Enums;

namespace Ascension.UI.Popups
{
    /// <summary>
    /// Context data for popups - tells popup where it was opened from
    /// and what actions are available
    /// </summary>
    public class PopupContext
    {
        public ItemLocation SourceLocation { get; private set; }
        public PopupSource Source { get; private set; }
        public bool CanEquip { get; private set; }
        public bool CanMove { get; private set; }
        public bool CanUse { get; private set; }
        public bool CanSell { get; private set; }

        // ═══════════════════════════════════════════════════════════
        // Factory Methods
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// Storage scene context - can equip and move items
        /// </summary>
        public static PopupContext FromStorage()
        {
            return new PopupContext
            {
                SourceLocation = ItemLocation.Storage,
                Source = PopupSource.StorageUI,
                CanEquip = true,
                CanMove = true,   // Can move to Bag
                CanUse = false,   // Can't use items from storage
                CanSell = false
            };
        }

        /// <summary>
        /// Bag context (from Storage scene) - can equip, move, and use
        /// </summary>
        public static PopupContext FromBag()
        {
            return new PopupContext
            {
                SourceLocation = ItemLocation.Bag,
                Source = PopupSource.BagUI,
                CanEquip = true,
                CanMove = true,   // Can move to Storage
                CanUse = true,    // Can use potions
                CanSell = false
            };
        }

        /// <summary>
        /// ✅ NEW: Inventory Panel context (persistent panel)
        /// Can equip and use, but NO move (not in storage scene)
        /// </summary>
        public static PopupContext FromInventoryPanel()
        {
            return new PopupContext
            {
                SourceLocation = ItemLocation.Bag,
                Source = PopupSource.InventoryPanel,
                CanEquip = true,
                CanMove = false,  // ✅ NO move button (not in storage scene)
                CanUse = true,    // Can use potions
                CanSell = false
            };
        }

        /// <summary>
        /// Equipped gear context - can only unequip
        /// </summary>
        public static PopupContext FromEquippedGear()
        {
            return new PopupContext
            {
                SourceLocation = ItemLocation.Equipped,
                Source = PopupSource.EquippedGear,
                CanEquip = false, // Already equipped
                CanMove = false,  // Can't move equipped items
                CanUse = false,
                CanSell = false
            };
        }

        /// <summary>
        /// Shop context (future)
        /// </summary>
        public static PopupContext FromShop()
        {
            return new PopupContext
            {
                SourceLocation = ItemLocation.None,
                Source = PopupSource.Shop,
                CanEquip = false,
                CanMove = false,
                CanUse = false,
                CanSell = false  // Can only buy in shop
            };
        }
    }

    // ═══════════════════════════════════════════════════════════
    // Enums
    // ═══════════════════════════════════════════════════════════

    public enum PopupSource
    {
        StorageUI,
        BagUI,
        EquippedGear,
        InventoryPanel,  // ✅ NEW: Persistent inventory panel
        Shop,
        Inventory,
        Loot,
        Quest
    }
}