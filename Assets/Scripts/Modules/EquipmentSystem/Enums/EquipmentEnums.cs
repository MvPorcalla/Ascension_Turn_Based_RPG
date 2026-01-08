// ════════════════════════════════════════════
// Assets\Scripts\Modules\EquipmentSystem\Enums\EquipmentEnums.cs
// Core enums for equipment system
// ════════════════════════════════════════════

namespace Ascension.Equipment.Enums
{
    /// <summary>
    /// Equipment gear slot types (7 slots total)
    /// </summary>
    public enum GearSlotType
    {
        Weapon,      // Main weapon
        Helmet,      // Head armor
        Chest,       // Body armor
        Gloves,      // Hand armor
        Boots,       // Foot armor
        Accessory1,  // First accessory slot
        Accessory2   // Second accessory slot
    }

    /// <summary>
    /// Skill loadout slot types (3 slots total)
    /// Used for combat hotbar skills
    /// </summary>
    public enum SkillSlotType
    {
        NormalSkill1,   // First normal skill
        NormalSkill2,   // Second normal skill
        UltimateSkill   // Ultimate/special skill
    }
}