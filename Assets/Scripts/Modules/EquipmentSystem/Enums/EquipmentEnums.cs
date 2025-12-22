// ════════════════════════════════════════════
// Assets\Scripts\Modules\EquipmentSystem\Enums\EquipmentEnums.cs
// Enums for equipment system
// ════════════════════════════════════════════

namespace Ascension.Equipment.Enums
{
    /// <summary>
    /// Equipment slot types
    /// </summary>
    public enum GearSlotType
    {
        Weapon,
        Helmet,
        Chest,
        Gloves,
        Boots,
        Accessory1,
        Accessory2
    }

    /// <summary>
    /// Skill loadout slot types (renamed from HotbarSlotType)
    /// </summary>
    public enum SkillSlotType
    {
        NormalSkill1,
        NormalSkill2,
        UltimateSkill
    }

    /// <summary>
    /// Storage filter modes for equipment room
    /// </summary>
    public enum EquipmentStorageFilter
    {
        All,         // Show everything (weapons, gear)
        Weapons,     // Show only weapons
        Helmets,     // Show only helmets
        Chests,      // Show only chest pieces
        Gloves,      // Show only gloves
        Boots,       // Show only boots
        Accessories, // Show only accessories
        Abilities    // Show only abilities for skill loadout
    }
}