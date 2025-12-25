// ══════════════════════════════════════════════════════════════════
// Scripts/Modules/InventorySystem/Constants/InventoryConstants.cs
// Centralized constants for inventory system
// ══════════════════════════════════════════════════════════════════

namespace Ascension.Inventory.Constants
{
    /// <summary>
    /// Item ID prefixes for categorization
    /// </summary>
    public static class ItemIDPrefixes
    {
        public const string Skill = "skill_";
        public const string Ability = "ability_";
        public const string Weapon = "weapon_";
        public const string Gear = "gear_";
        public const string Potion = "potion_";
        public const string Material = "material_";
        public const string Misc = "misc_";
    }

    /// <summary>
    /// Debug logging tags
    /// </summary>
    public static class LogTags
    {
        public const string InventoryCore = "[InventoryCore]";
        public const string InventoryManager = "[InventoryManager]";
        public const string SlotCapacity = "[SlotCapacityManager]";
        public const string ItemQuery = "[ItemQueryService]";
        public const string ItemStacking = "[ItemStackingService]";
        public const string ItemLocation = "[ItemLocationService]";
    }
}