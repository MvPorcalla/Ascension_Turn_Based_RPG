// ════════════════════════════════════════════
// GearSO.cs
// ScriptableObject for defining armor and accessories with stats, rarity, and bonus rolls
// ════════════════════════════════════════════

using System;
using System.Collections.Generic;
using UnityEngine;
using Ascension.Data.Enums;

namespace Ascension.Data.SO.Item
{
    [CreateAssetMenu(fileName = "NewGear", menuName = "Items/Gear")]
    public class GearSO : ItemBaseSO
    {
        #region Serialized Fields
        [Header("Gear Info")]
        [SerializeField] private string gearName;
        [SerializeField] private GearType gearType;

        [Header("Rarity System")]
        [SerializeField, Tooltip("Rarity configuration - affects stat multipliers and bonus slots")]
        private WeaponRaritySO rarityConfig;

        [SerializeField, Tooltip("Randomly rolled bonus stats (filled at craft/drop time)")]
        private List<WeaponBonusStat> bonusStats = new List<WeaponBonusStat>();

        [Header("Base Stats (Before Rarity Multiplier)")]
        [SerializeField] private float baseHP;
        [SerializeField] private float baseDefense;

        [Header("Offensive Stats")]
        [SerializeField] private float baseAD;
        [SerializeField] private float baseAP;
        [SerializeField] private float baseAttackSpeed;
        [SerializeField] private float baseCritRate;
        [SerializeField] private float baseCritDamage;

        [Header("Utility Stats")]
        [SerializeField] private float baseEvasion;
        [SerializeField] private float baseTenacity;
        [SerializeField] private float baseLethality;
        [SerializeField, Range(0f, 100f)] private float basePenetration;
        [SerializeField, Range(0f, 100f)] private float baseLifesteal;
        #endregion

        // ✅ Public getter to allow other scripts to read it
        public string GearName => gearName;
        public GearType GearType => gearType;

        #region Properties - Calculated Stats
        public float BonusHP => CalculateBonusStat(BonusStatType.Health, baseHP);
        public float BonusDefense => CalculateBonusStat(BonusStatType.Defense, baseDefense);
        public float BonusAD => CalculateBonusStat(BonusStatType.AttackDamage, baseAD);
        public float BonusAP => CalculateBonusStat(BonusStatType.AbilityPower, baseAP);
        public float BonusAttackSpeed => CalculateBonusStat(BonusStatType.AttackSpeed, baseAttackSpeed);
        public float BonusCritRate => CalculateBonusStat(BonusStatType.CritRate, baseCritRate);
        public float BonusCritDamage => CalculateBonusStat(BonusStatType.CritDamage, baseCritDamage);
        public float BonusEvasion => CalculateBonusStat(BonusStatType.Evasion, baseEvasion);
        public float BonusTenacity => CalculateBonusStat(BonusStatType.Tenacity, baseTenacity);
        public float BonusLethality => CalculateBonusStat(BonusStatType.Lethality, baseLethality);
        public float BonusPenetration => CalculateBonusStat(BonusStatType.Penetration, basePenetration);
        public float BonusLifesteal => CalculateBonusStat(BonusStatType.Lifesteal, baseLifesteal);

        public List<WeaponBonusStat> BonusStats => bonusStats;
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

            Debug.Log($"[GearSO] Rolled {bonusStats.Count} bonus stats for {gearName}");
        }

        public override string GetInfoText()
        {
            string rarityName = rarityConfig != null ? rarityConfig.RarityName : "Common";
            string rarityColorHex = rarityConfig != null ? ColorUtility.ToHtmlStringRGB(rarityConfig.RarityColor) : "FFFFFF";

            string info = $"<b>{gearName}</b>\n";
            info += $"<color=#{rarityColorHex}>{rarityName} {gearType}</color>\n\n";

            // Defensive stats first
            info += FormatStatText(BonusHP, "HP", "#2dff03ff");
            info += FormatStatText(BonusDefense, "Defense", "#ff5f03ff");

            // Offensive stats next
            info += FormatStatText(BonusAD, "Attack Damage", "#ff6b6b");
            info += FormatStatText(BonusAP, "Ability Power", "#4ecdc4");
            info += FormatStatText(BonusAttackSpeed, "Attack Speed");
            info += FormatStatText(BonusCritRate, "Crit Rate", "%");
            info += FormatStatText(BonusCritDamage, "Crit Damage", "%");

            // Utility stats last
            info += FormatStatText(BonusEvasion, "Evasion", "%");
            info += FormatStatText(BonusTenacity, "Tenacity", "%");
            info += FormatStatText(BonusLethality, "Lethality");
            info += FormatStatText(BonusPenetration, "Penetration", "%");
            info += FormatStatText(BonusLifesteal, "Lifesteal", "%");

            // Bonus stats
            if (bonusStats.Count > 0)
            {
                info += $"\n<color=#{rarityColorHex}><b>Bonus Stats:</b></color>\n";
                foreach (var bonus in bonusStats)
                    info += $"<color=#ffd700>• {bonus.GetDisplayText()}</color>\n";
            }

            // Description
            if (!string.IsNullOrEmpty(description))
                info += $"\n<i>{description}</i>";

            return info;
        }

        #endregion

        #region Private Methods
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
                BonusStatType.Health => baseHP,
                BonusStatType.Defense => baseDefense,
                BonusStatType.AttackDamage => baseAD,
                BonusStatType.AbilityPower => baseAP,
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
            return !string.IsNullOrEmpty(suffix)
                ? $"+{value:F1}{suffix} {name}\n"
                : $"+{value:F1} {name}\n";
        }

        private void OnValidate()
        {
            itemName = gearName;
            itemType = ItemType.Gear;
            isStackable = false;

            if (rarityConfig != null)
                rarity = (Rarity)((int)rarityConfig.Tier);

            if (string.IsNullOrEmpty(itemID))
                itemID = $"gear_{gearType.ToString().ToLower()}_{name.ToLower().Replace(" ", "_")}";
        }
        #endregion

        #region Debug Helpers
        [ContextMenu("Generate Item ID")]
        private void GenerateItemID()
        {
            itemID = $"gear_{gearType.ToString().ToLower()}_{gearName.ToLower().Replace(" ", "_")}";
            Debug.Log($"Generated ID: {itemID}");
            
            #if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
            #endif
        }

        [ContextMenu("Print Gear Info")]
        private void DebugPrintInfo()
        {
            Debug.Log(GetInfoText());
        }
        #endregion
    }

    public enum GearType
    {
        Helmet,
        ChestPlate,
        Gloves,
        Boots,
        Accessory
    }
} 