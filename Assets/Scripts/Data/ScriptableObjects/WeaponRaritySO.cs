// ════════════════════════════════════════════
// WeaponRaritySO.cs
// ScriptableObject defining weapon rarity, stat multipliers, bonus stats, and visual/audio effects
// ════════════════════════════════════════════

using UnityEngine;
using System;
using Ascension.Data.Enums;

namespace Ascension.Data.SO
{
    [CreateAssetMenu(fileName = "NewWeaponRarity", menuName = "Items/Weapon Rarity")]
    public class WeaponRaritySO : ScriptableObject
    {
        #region Serialized Fields
        [Header("Rarity Info")]
        [Tooltip("Display name of the rarity")]
        [SerializeField] private string rarityName;
        [SerializeField] private RarityTier tier;
        [SerializeField] private Color rarityColor = Color.white;

        [Header("Stat Multipliers")]
        [Tooltip("Multiply all base weapon stats by this amount")]
        [Range(0.5f, 5f)]
        [SerializeField] private float statMultiplier = 1f;

        [Header("Bonus Stat Slots")]
        [Tooltip("Number of random bonus stats this rarity can roll")]
        [Range(0, 6)]
        [SerializeField] private int bonusStatSlots = 0;

        [Tooltip("Min value for bonus stats (% of base stat)")]
        [Range(0f, 1f)]
        [SerializeField] private float bonusStatMinRoll = 0.1f;

        [Tooltip("Max value for bonus stats (% of base stat)")]
        [Range(0f, 2f)]
        [SerializeField] private float bonusStatMaxRoll = 0.5f;

        [Header("Visual Effects")]
        [SerializeField] private Sprite rarityIcon;
        [SerializeField] private GameObject rarityParticleEffect;
        [SerializeField] private AudioClip rarityDropSound;

        [Header("Crafting Requirements (Future)")]
        [Tooltip("Minimum successful hits needed in blacksmith minigame")]
        [SerializeField] private int minCraftingHits = 0;

        [Tooltip("Perfect hits bonus: additional stat roll chance")]
        [Range(0f, 1f)]
        [SerializeField] private float perfectHitBonusChance = 0f;
        #endregion

        #region Properties
        public string RarityName => rarityName;
        public RarityTier Tier => tier;
        public Color RarityColor => rarityColor;

        public float StatMultiplier => statMultiplier;
        public int BonusStatSlots => bonusStatSlots;
        public float BonusStatMinRoll => bonusStatMinRoll;
        public float BonusStatMaxRoll => bonusStatMaxRoll;

        public Sprite RarityIcon => rarityIcon;
        public GameObject RarityParticleEffect => rarityParticleEffect;
        public AudioClip RarityDropSound => rarityDropSound;

        public int MinCraftingHits => minCraftingHits;
        public float PerfectHitBonusChance => perfectHitBonusChance;
        #endregion
    }

    // ════════════════════════════════════════════
    // WeaponBonusStat.cs
    // Represents a bonus stat rolled on a weapon and provides display text
    // ════════════════════════════════════════════
    [Serializable]
    public class WeaponBonusStat
    {
        #region Serialized Fields
        [SerializeField] private BonusStatType statType;
        [SerializeField] private float value;
        #endregion

        #region Properties
        public BonusStatType StatType => statType;
        public float Value => value;
        #endregion

        #region Constructors
        public WeaponBonusStat(BonusStatType type, float val)
        {
            statType = type;
            value = val;
        }
        #endregion

        #region Public Methods
        public string GetDisplayText()
        {
            // Return formatted string based on stat type
            return statType switch
            {
                BonusStatType.AttackDamage => $"+{value:F1} Attack Damage",
                BonusStatType.AbilityPower => $"+{value:F1} Ability Power",
                BonusStatType.Health => $"+{value:F0} Health",
                BonusStatType.Defense => $"+{value:F1} Defense",
                BonusStatType.AttackSpeed => $"+{value:F1} Attack Speed",
                BonusStatType.CritRate => $"+{value:F1}% Crit Rate",
                BonusStatType.CritDamage => $"+{value:F1}% Crit Damage",
                BonusStatType.Evasion => $"+{value:F1}% Evasion",
                BonusStatType.Tenacity => $"+{value:F1}% Tenacity",
                BonusStatType.Lethality => $"+{value:F0} Lethality",
                BonusStatType.Penetration => $"+{value:F1}% Penetration",
                BonusStatType.Lifesteal => $"+{value:F1}% Lifesteal",
                _ => string.Empty
            };
        }
        #endregion
    }
}
