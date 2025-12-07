// ════════════════════════════════════════════
// PotionSO.cs
// ScriptableObject defining potions with percentage or flat restore values
// Supports buffs, duration types, and usage restrictions
// ════════════════════════════════════════════

using System.Collections.Generic;
using UnityEngine;
using Ascension.Manager;
using Ascension.Data.SO.Item;

namespace Ascension.Data.SO.Item
{
    [CreateAssetMenu(fileName = "New Potion", menuName = "Items/Potion")]
    public class PotionSO : ItemBaseSO
    {
        #region Serialized Fields

        [Header("Potion Settings")]
        [Tooltip("Defines the potion type")]
        public PotionType potionType;

        [Header("Restore Type")]
        [Tooltip("Use percentage or flat values for healing")]
        public RestoreType restoreType = RestoreType.Percentage;

        [Header("Restore Effects")]
        [Tooltip("Percentage (0-100) or flat amount to restore")]
        [Range(0f, 100f)]
        public float healthRestorePercent = 20f;

        [Tooltip("Flat HP amount (only used if RestoreType = Flat)")]
        public float healthRestoreFlat = 50f;

        [Range(0f, 100f)]
        public float manaRestorePercent = 0f;

        [Tooltip("Flat Mana amount (only used if RestoreType = Flat)")]
        public float manaRestoreFlat = 0f;

        [Header("Duration Settings")]
        [Tooltip("How the restore duration is measured")]
        public DurationType durationType = DurationType.Instant;

        [Tooltip("Duration value (seconds for real-time, turns for turn-based)")]
        public float restoreDuration = 0f;

        [Header("Buffs")]
        public List<PotionBuff> buffs = new List<PotionBuff>();

        [Header("Usage Settings")]
        public bool canUseInCombat = true;
        public bool canUseOutOfCombat = true;

        #endregion

        #region Calculated Properties

        public float HealthRestore => restoreType == RestoreType.Percentage ? healthRestorePercent : healthRestoreFlat;
        public float ManaRestore => restoreType == RestoreType.Percentage ? manaRestorePercent : manaRestoreFlat;
        public float RestoreAmount => Mathf.Max(HealthRestore, ManaRestore);
        public float Duration => restoreDuration;
        public bool GrantsBuff => buffs != null && buffs.Count > 0;
        public float BuffDuration => GrantsBuff ? buffs[0].duration : 0f;
        public BuffType BuffType => GrantsBuff ? buffs[0].type : BuffType.AttackDamage;
        public float BuffValue => GrantsBuff ? buffs[0].value : 0f;
        public bool IsTurnBased => durationType == DurationType.TurnBased;
        public int TurnDuration => IsTurnBased ? Mathf.RoundToInt(restoreDuration) : 0;

        #endregion

        #region Unity Callbacks

        private void OnValidate()
        {
            itemType = ItemType.Consumable;
            isStackable = true;
            maxStackSize = 999;

            // Auto-set duration type for Rejuvenation potions
            if (potionType == PotionType.Rejuvenation)
            {
                durationType = DurationType.TurnBased;
            }
        }

        #endregion

        #region Public Methods

        public float GetActualHealAmount(float maxHP)
        {
            return restoreType == RestoreType.Percentage ? (healthRestorePercent / 100f) * maxHP : healthRestoreFlat;
        }

        public float GetActualManaAmount(float maxMana)
        {
            return restoreType == RestoreType.Percentage ? (manaRestorePercent / 100f) * maxMana : manaRestoreFlat;
        }

        public override string GetInfoText()
        {
            string info = $"<b>{itemName}</b>\n{rarity} Potion\n\n";

            if (HealthRestore > 0)
                info += GetRestoreDescription(HealthRestore, "HP", restoreType);

            if (ManaRestore > 0)
                info += GetRestoreDescription(ManaRestore, "Mana", restoreType);

            if (GrantsBuff)
            {
                info += "\n<color=yellow>Buffs:</color>\n";
                foreach (var buff in buffs)
                {
                    info += $"• {buff.GetDescription()}\n";
                }
            }

            if (!string.IsNullOrEmpty(description))
                info += $"\n<i>{description}</i>";

            return info;
        }

        public bool CanUse(bool isInCombat)
        {
            if (!canUseInCombat && isInCombat)
            {
                Debug.Log($"[PotionSO] Cannot use {itemName} in combat");
                return false;
            }

            if (!canUseOutOfCombat && !isInCombat)
            {
                Debug.Log($"[PotionSO] Can only use {itemName} in combat");
                return false;
            }

            return true;
        }

        #endregion

        #region Private Methods

        private string GetRestoreDescription(float amount, string statName, RestoreType type)
        {
            string amountText = type == RestoreType.Percentage ? $"{amount}%" : $"{amount}";

            return durationType switch
            {
                DurationType.Instant => $"Restores {amountText} {statName} instantly\n",
                DurationType.RealTime => $"Restores {amountText} {statName} over {restoreDuration:F1}s\n",
                DurationType.TurnBased => 
                    $"Restores {(amount / Mathf.RoundToInt(restoreDuration)):F1} {statName} per turn for {Mathf.RoundToInt(restoreDuration)} turns (Total: {amountText})\n",
                _ => $"Restores {amountText} {statName}\n",
            };
        }

        #endregion

        #region Debug Helpers

        [ContextMenu("Generate Item ID")]
        private void GenerateItemID()
        {
            itemID = $"potion_{itemName.ToLower().Replace(" ", "_").Replace("'", "")}";
            Debug.Log($"Generated ID: {itemID}");

#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }

        [ContextMenu("Test: Add to Inventory")]
        private void DebugAddToInventory()
        {
            if (Application.isPlaying && InventoryManager.Instance != null)
            {
                InventoryManager.Instance.AddItem(itemID, 5);
                Debug.Log($"[PotionSO] Added 5x {itemName} to inventory");
            }
            else
            {
                Debug.LogWarning("Can only test in Play Mode with InventoryManager present");
            }
        }

        [ContextMenu("Print Potion Info")]
        private void DebugPrintInfo()
        {
            Debug.Log(GetInfoText());
        }

        [ContextMenu("Test Heal Amount (1000 HP)")]
        private void DebugTestHealAmount()
        {
            float testMaxHP = 1000f;
            float healAmount = GetActualHealAmount(testMaxHP);
            Debug.Log($"[{itemName}] Would heal {healAmount} HP on a player with {testMaxHP} max HP");
        }

        #endregion
    }

    [System.Serializable]
    public class PotionBuff
    {
        public BuffType type;
        public float value;
        public float duration;
        public DurationType durationType = DurationType.RealTime;

        public string GetDescription()
        {
            string buffDesc = type switch
            {
                BuffType.AttackDamage => $"+{value} Attack Damage",
                BuffType.AbilityPower => $"+{value} Ability Power",
                BuffType.Defense => $"+{value} Defense",
                BuffType.Speed => $"+{value}% Movement Speed",
                BuffType.CritRate => $"+{value}% Crit Rate",
                BuffType.AttackSpeed => $"+{value}% Attack Speed",
                BuffType.Regeneration => $"+{value} HP/s Regeneration",
                BuffType.Resistance => $"+{value}% Damage Resistance",
                BuffType.Invisibility => "Invisibility",
                BuffType.Invulnerability => "Invulnerability",
                _ => "Unknown Buff"
            };

            string durationDesc = durationType switch
            {
                DurationType.TurnBased => $"{duration} turns",
                DurationType.RealTime => $"{duration}s",
                _ => ""
            };

            return $"{buffDesc} ({durationDesc})";
        }
    }

    public enum PotionType
    {
        HealthPotion,
        ManaPotion,
        Elixir,
        BuffPotion,
        Antidote,
        Rejuvenation,
        Utility
    }

    public enum RestoreType
    {
        Percentage,
        Flat
    }

    public enum DurationType
    {
        Instant,
        RealTime,
        TurnBased
    }

    public enum BuffType
    {
        AttackDamage,
        AbilityPower,
        Defense,
        Speed,
        CritRate,
        AttackSpeed,
        Regeneration,
        Resistance,
        Invisibility,
        Invulnerability
    }
}
