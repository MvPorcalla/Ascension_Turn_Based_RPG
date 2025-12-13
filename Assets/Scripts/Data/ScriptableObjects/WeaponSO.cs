// ════════════════════════════════════════════
// WeaponSO.cs
// ScriptableObject defining weapon data, stats, rarity, and skills
// ════════════════════════════════════════════

using System;
using System.Collections.Generic;
using UnityEngine;
using Ascension.Data.Enums;

namespace Ascension.Data.SO.Item
{
    [CreateAssetMenu(fileName = "NewWeapon", menuName = "Items/Weapon")]
    public class WeaponSO : ItemBaseSO
    {
        #region Serialized Fields
        [Header("Weapon Info")]
        [SerializeField] private string weaponName;
        [SerializeField] private WeaponType weaponType;
        [SerializeField] private AttackRangeType attackRangeType;

        [Header("Rarity System")]
        [SerializeField, Tooltip("Rarity configuration - affects stat multipliers and bonus slots")]
        private WeaponRaritySO rarityConfig;

        [SerializeField, Tooltip("Randomly rolled bonus stats (filled at craft/drop time)")]
        private List<WeaponBonusStat> bonusStats = new List<WeaponBonusStat>();

        [Header("Base Stats (Before Rarity Multiplier)")]
        [SerializeField] private float baseAD;
        [SerializeField] private float baseAP;

        [Header("Defensive Stats")]
        [SerializeField] private float baseHP;
        [SerializeField] private float baseDefense;

        [Header("Offensive Stats")]
        [SerializeField] private float baseAttackSpeed;
        [SerializeField] private float baseCritRate;
        [SerializeField] private float baseCritDamage;

        [Header("Utility Stats")]
        [SerializeField] private float baseEvasion;
        [SerializeField] private float baseTenacity;
        [SerializeField] private float baseLethality;
        [SerializeField, Range(0f, 100f)] private float basePenetration;
        [SerializeField, Range(0f, 100f)] private float baseLifesteal;

        [Header("Weapon Skill")]
        [SerializeField] private AbilitySO defaultWeaponSkill;

        #endregion

        // ✅ Public getter to allow other scripts to read it
        public string WeaponName => weaponName;
        public WeaponType WeaponType => weaponType;
        public AttackRangeType AttackRangeType => attackRangeType;

        // Public accessors for GearInfoPopUp
        public List<WeaponBonusStat> BonusStats => bonusStats;
        public AbilitySO DefaultWeaponSkill => defaultWeaponSkill;

        #region Properties - Calculated Stats
        public float BonusAD => CalculateBonusStat(BonusStatType.AttackDamage, baseAD);
        public float BonusAP => CalculateBonusStat(BonusStatType.AbilityPower, baseAP);
        public float BonusHP => CalculateBonusStat(BonusStatType.Health, baseHP);
        public float BonusDefense => CalculateBonusStat(BonusStatType.Defense, baseDefense);
        public float BonusAttackSpeed => CalculateBonusStat(BonusStatType.AttackSpeed, baseAttackSpeed);
        public float BonusCritRate => CalculateBonusStat(BonusStatType.CritRate, baseCritRate);
        public float BonusCritDamage => CalculateBonusStat(BonusStatType.CritDamage, baseCritDamage);
        public float BonusEvasion => CalculateBonusStat(BonusStatType.Evasion, baseEvasion);
        public float BonusTenacity => CalculateBonusStat(BonusStatType.Tenacity, baseTenacity);
        public float BonusLethality => CalculateBonusStat(BonusStatType.Lethality, baseLethality);
        public float BonusPenetration => CalculateBonusStat(BonusStatType.Penetration, basePenetration);
        public float BonusLifesteal => CalculateBonusStat(BonusStatType.Lifesteal, baseLifesteal);
        #endregion

        #region Public Methods
        public void RollBonusStats()
        {
            if (rarityConfig == null || rarityConfig.BonusStatSlots <= 0)
            {
                bonusStats.Clear();
                return;
            }

            bonusStats.Clear();

            var availableStats = new List<BonusStatType>((BonusStatType[])Enum.GetValues(typeof(BonusStatType)));

            for (int i = 0; i < rarityConfig.BonusStatSlots && availableStats.Count > 0; i++)
            {
                int randomIndex = UnityEngine.Random.Range(0, availableStats.Count);
                var statType = availableStats[randomIndex];
                availableStats.RemoveAt(randomIndex);

                float baseValue = GetBaseStatValue(statType);
                float rollPercent = UnityEngine.Random.Range(rarityConfig.BonusStatMinRoll, rarityConfig.BonusStatMaxRoll);
                float rolledValue = baseValue * rollPercent;

                bonusStats.Add(new WeaponBonusStat(statType, rolledValue));
            }

            Debug.Log($"[WeaponSO] Rolled {bonusStats.Count} bonus stats for {weaponName}");
        }

        public override string GetInfoText()
        {
            string rarityName = rarityConfig != null ? rarityConfig.RarityName : "Common";
            string rarityColorHex = rarityConfig != null ? ColorUtility.ToHtmlStringRGB(rarityConfig.RarityColor) : "FFFFFF";

            string info = $"<b>{weaponName}</b>\n";
            info += $"<color=#{rarityColorHex}>{rarityName} {weaponType}</color> ({attackRangeType})\n\n";

            info += FormatStatText(BonusAD, "Attack Damage", "#ff6b6b");
            info += FormatStatText(BonusAP, "Ability Power", "#4ecdc4");
            info += FormatStatText(BonusHP, "HP");
            info += FormatStatText(BonusDefense, "Defense");
            info += FormatStatText(BonusAttackSpeed, "Attack Speed");
            info += FormatStatText(BonusCritRate, "Crit Rate", "%");
            info += FormatStatText(BonusCritDamage, "Crit Damage", "%");
            info += FormatStatText(BonusEvasion, "Evasion", "%");
            info += FormatStatText(BonusTenacity, "Tenacity", "%");
            info += FormatStatText(BonusLethality, "Lethality");
            info += FormatStatText(BonusPenetration, "Penetration", "%");
            info += FormatStatText(BonusLifesteal, "Lifesteal", "%");

            if (bonusStats.Count > 0)
            {
                info += $"\n<color=#{rarityColorHex}><b>Bonus Stats:</b></color>\n";
                foreach (var bonus in bonusStats)
                    info += $"<color=#ffd700>• {bonus.GetDisplayText()}</color>\n";
            }

            if (!string.IsNullOrEmpty(Description))
                info += $"\n<i>{Description}</i>";

            if (defaultWeaponSkill != null)
                info += $"\n\n<color=#ffd93d> {defaultWeaponSkill.AbilityName}</color>";

            return info;
        }
        #endregion

        #region Private Methods
        // Private method to get rarity multiplier safely
        private float GetRarityMultiplier() => rarityConfig != null ? rarityConfig.StatMultiplier : 1f;

        private float CalculateBonusStat(BonusStatType type, float baseValue)
        {
            float totalBonus = 0f;

            foreach (var bonus in bonusStats)
            {
                if (bonus.StatType == type)
                    totalBonus += bonus.Value;
            }

            return baseValue * GetRarityMultiplier() + totalBonus;
        }

        private float GetBaseStatValue(BonusStatType type)
        {
            return type switch
            {
                BonusStatType.AttackDamage => baseAD,
                BonusStatType.AbilityPower => baseAP,
                BonusStatType.Health => baseHP,
                BonusStatType.Defense => baseDefense,
                BonusStatType.AttackSpeed => baseAttackSpeed,
                BonusStatType.CritRate => baseCritRate,
                BonusStatType.CritDamage => baseCritDamage,
                BonusStatType.Evasion => baseEvasion,
                BonusStatType.Tenacity => baseTenacity,
                BonusStatType.Lethality => baseLethality,
                BonusStatType.Penetration => basePenetration,
                BonusStatType.Lifesteal => baseLifesteal,
                _ => 0f
            };
        }

        private string FormatStatText(float value, string name, string suffix = "")
        {
            if (value <= 0) return string.Empty;

            string color = suffix == "%" ? "#ffffff" : "#ffffff";
            return !string.IsNullOrEmpty(suffix)
                ? $"+{value:F1}{suffix} {name}\n"
                : $"+{value:F1} {name}\n";
        }

        private void OnValidate()
        {
            // Assign directly to the protected fields
            itemName = weaponName;
            itemType = ItemType.Weapon;
            isStackable = false;

            if (rarityConfig != null)
                rarity = (Rarity)((int)rarityConfig.Tier);

            if (string.IsNullOrEmpty(itemID))
                itemID = $"weapon_{name.ToLower().Replace(" ", "_")}";
        }
        #endregion

        #region Debug Helpers

        [ContextMenu("Generate Item ID")]
        private void GenerateItemID()
        {
            itemID = $"weapon_{weaponName.ToLower().Replace(" ", "_")}";
            Debug.Log($"Generated ID: {itemID}");
            
            #if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
            #endif
        }

        [ContextMenu("Print Weapon Info")]
        private void DebugPrintInfo()
        {
            Debug.Log(GetInfoText());
        }
        #endregion
    }
}
