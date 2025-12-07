// ──────────────────────────────────────────────────
// WeaponEnums.cs
// All weapon-related enums in one place
// ──────────────────────────────────────────────────

namespace Ascension.Data.Enums
{
    public enum WeaponType
    {
        Sword,
        Axe,
        Dagger,
        Bow,
        Staff,
        Wand,
        Hammer,
        Spear,
        Shield
    }

    public enum AttackRangeType
    {
        Melee,
        Ranged,
        Magic
    }

    public enum BonusStatType
    {
        AttackDamage,
        AbilityPower,
        Health,
        Defense,
        AttackSpeed,
        CritRate,
        CritDamage,
        Evasion,
        Tenacity,
        Lethality,
        Penetration,
        Lifesteal
    }

    public enum RarityTier
    {
        Common = 0,
        Rare = 1,
        Epic = 2,
        Legendary = 3,
        Mythic = 4
    }
}
