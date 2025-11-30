// Rarity configuration and bonus stats

using UnityEngine;
using System;

[CreateAssetMenu(fileName = "New Weapon Rarity", menuName = "Game/Weapon Rarity")]
public class WeaponRaritySO : ScriptableObject
{
    [Header("Rarity Info")]
    public string rarityName;
    public RarityTier tier;
    public Color rarityColor = Color.white;
    
    [Header("Stat Multipliers")]
    [Tooltip("Multiply all base weapon stats by this amount")]
    [Range(0.5f, 5f)]
    public float statMultiplier = 1f;
    
    [Header("Bonus Stat Slots")]
    [Tooltip("Number of random bonus stats this rarity can roll")]
    [Range(0, 6)]
    public int bonusStatSlots = 0;
    
    [Tooltip("Min value for bonus stats (% of base stat)")]
    [Range(0f, 1f)]
    public float bonusStatMinRoll = 0.1f;
    
    [Tooltip("Max value for bonus stats (% of base stat)")]
    [Range(0f, 2f)]
    public float bonusStatMaxRoll = 0.5f;
    
    [Header("Visual Effects")]
    public Sprite rarityIcon;
    public GameObject rarityParticleEffect;
    public AudioClip rarityDropSound;
    
    [Header("Crafting Requirements (Future)")]
    [Tooltip("Minimum successful hits needed in blacksmith minigame")]
    public int minCraftingHits = 0;
    
    [Tooltip("Perfect hits bonus: additional stat roll chance")]
    [Range(0f, 1f)]
    public float perfectHitBonusChance = 0f;
}

[Serializable]
public class WeaponBonusStat
{
    public BonusStatType statType;
    public float value;
    
    public WeaponBonusStat(BonusStatType type, float val)
    {
        statType = type;
        value = val;
    }
    
    public string GetDisplayText()
    {
        switch (statType)
        {
            case BonusStatType.AttackDamage:
                return $"+{value:F1} Attack Damage";
            case BonusStatType.AbilityPower:
                return $"+{value:F1} Ability Power";
            case BonusStatType.Health:
                return $"+{value:F0} Health";
            case BonusStatType.Defense:
                return $"+{value:F1} Defense";
            case BonusStatType.AttackSpeed:
                return $"+{value:F1} Attack Speed";
            case BonusStatType.CritRate:
                return $"+{value:F1}% Crit Rate";
            case BonusStatType.CritDamage:
                return $"+{value:F1}% Crit Damage";
            case BonusStatType.Evasion:
                return $"+{value:F1}% Evasion";
            case BonusStatType.Tenacity:
                return $"+{value:F1}% Tenacity";
            case BonusStatType.Lethality:
                return $"+{value:F0} Lethality";
            case BonusStatType.Penetration:
                return $"+{value:F1}% Penetration";
            case BonusStatType.Lifesteal:
                return $"+{value:F1}% Lifesteal";
            default:
                return "";
        }
    }
}