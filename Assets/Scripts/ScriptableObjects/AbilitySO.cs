// ════════════════════════════════════════════
// AbilitySO.cs
// ScriptableObject representing an ability (formerly SkillSO)
// ════════════════════════════════════════════

using System;
using UnityEngine;
using Ascension.Systems; 

namespace Ascension.Data.SO
{
    [CreateAssetMenu(fileName = "New Ability", menuName = "Abilities/Ability")]
    public class AbilitySO : ItemBaseSO
    {
        #region Serialized Fields
        [Header("Ability Info")]
        [SerializeField] private string abilityName;

        [Header("Category & Requirements")]
        [SerializeField] private AbilityCategory category;
        [SerializeField] private WeaponType[] compatibleWeaponTypes;

        [Header("Damage & Scaling")]
        [SerializeField] private DamageType damageType = DamageType.Physical;
        [SerializeField] private float baseDamage;
        [SerializeField, Range(0f, 5f)] private float adRatio;
        [SerializeField, Range(0f, 5f)] private float apRatio;

        [Header("Targeting")]
        [SerializeField] private TargetType targetType = TargetType.Single;
        [SerializeField, Tooltip("Only used if targetType == Multi")] private int maxTargets = 1;

        [Header("Cooldown")]
        [SerializeField] private int turnCooldown = 0;

        [Header("Effects")]
        [SerializeField] private bool canCrit = true;
        #endregion

        #region Properties
        public string AbilityName => abilityName;
        public AbilityCategory Category => category;
        public WeaponType[] CompatibleWeaponTypes => compatibleWeaponTypes;
        public DamageType DamageType => damageType;
        public float BaseDamage => baseDamage;
        public float ADRatio => adRatio;
        public float APRatio => apRatio;
        public TargetType TargetType => targetType;
        public int MaxTargets => maxTargets;
        public int TurnCooldown => turnCooldown;
        public bool CanCrit => canCrit;
        #endregion

        #region Unity Callbacks
        private void OnValidate()
        {
            // Sync with ItemBaseSO protected fields
            itemName = abilityName;
            itemType = ItemType.Ability;
            isStackable = false;

            if (string.IsNullOrWhiteSpace(itemID))
                itemID = $"ability_{name.ToLower().Replace(" ", "_")}";
        }
        #endregion

        #region Public Methods
        public override string GetInfoText()
        {
            string info = $"<b>{abilityName}</b>\n";
            info += $"{category} Ability\n";
            info += $"{damageType} Damage\n\n";

            info += $"Base Damage: {baseDamage}\n";
            if (adRatio > 0) info += $"AD Ratio: {adRatio * 100}%\n";
            if (apRatio > 0) info += $"AP Ratio: {apRatio * 100}%\n";

            info += $"Target: {targetType}\n";
            if (turnCooldown > 0) info += $"Cooldown: {turnCooldown} turns\n";

            if (!string.IsNullOrEmpty(Description))
                info += $"\n{Description}";

            return info;
        }
        #endregion

        #region Debug Helpers
        [ContextMenu("Test: Add to Inventory")]
        private void DebugAddToInventory()
        {
            if (!Application.isPlaying)
            {
                Debug.LogWarning("[AbilitySO] Enter Play Mode first!");
                return;
            }

            if (InventoryManager.Instance == null)
            {
                Debug.LogError("[AbilitySO] InventoryManager not found!");
                return;
            }

            InventoryManager.Instance.AddItem(itemID, 1, false);
            Debug.Log($"[AbilitySO] Added {abilityName} to storage");
        }

        [ContextMenu("Print Ability Info")]
        private void DebugPrintInfo()
        {
            Debug.Log($"=== {abilityName} ===");
            Debug.Log($"ID: {itemID}");
            Debug.Log($"Category: {category}");
            Debug.Log($"Damage Type: {damageType}");
            Debug.Log($"Base Damage: {baseDamage}");
            Debug.Log($"AD Ratio: {adRatio * 100}%");
            Debug.Log($"AP Ratio: {apRatio * 100}%");
            Debug.Log($"Target: {targetType}");
            Debug.Log($"Cooldown: {turnCooldown} turns");
            Debug.Log($"Can Crit: {canCrit}");

            if (compatibleWeaponTypes != null && compatibleWeaponTypes.Length > 0)
                Debug.Log($"Compatible Weapons: {string.Join(", ", compatibleWeaponTypes)}");
        }
        #endregion
    }

    public enum AbilityCategory
    {
        Weapon,
        Normal,
        Ultimate
    }

    public enum DamageType
    {
        Physical,
        Magic,
        True
    }

    public enum TargetType
    {
        Single,
        Multi,
        AllEnemies,
        Self,
        AllAllies
    }
}
