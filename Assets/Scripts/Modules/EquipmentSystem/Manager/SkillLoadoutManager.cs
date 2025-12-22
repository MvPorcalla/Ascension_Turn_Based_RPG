// ════════════════════════════════════════════
// Assets\Scripts\Modules\EquipmentSystem\Manager\SkillLoadoutManager.cs
// Manager for skill loadout system - handles skill assignments (renamed from HotbarManager)
// ════════════════════════════════════════════

using System;
using UnityEngine;
using Ascension.Core;
using Ascension.Data.SO.Item;
using Ascension.Data.SO.Database;
using Ascension.Data.Save;
using Ascension.Equipment.Data;
using Ascension.Equipment.Enums;

namespace Ascension.Equipment.Manager
{
    public class SkillLoadoutManager : MonoBehaviour, IGameService
    {
        #region Singleton
        public static SkillLoadoutManager Instance { get; private set; }
        #endregion

        #region Serialized Fields
        [Header("References")]
        [SerializeField] private GameDatabaseSO database;
        #endregion

        #region Private Fields
        private SkillLoadout _skillLoadout;
        #endregion

        #region Properties
        public SkillLoadout Loadout => _skillLoadout;
        public GameDatabaseSO Database => database;
        #endregion

        #region Events
        public event Action<SkillSlotType, string> OnSkillSlotChanged;  // (slotType, skillId)
        public event Action OnLoadoutChanged;                           // Any loadout change
        #endregion

        #region Unity Callbacks
        private void Awake()
        {
            InitializeSingleton();
        }
        #endregion

        #region IGameService Implementation
        public void Initialize()
        {
            Debug.Log("[SkillLoadoutManager] Initializing...");
            
            InitializeData();
            ValidateReferences();
            
            Debug.Log("[SkillLoadoutManager] Ready");
        }

        private void InitializeData()
        {
            _skillLoadout = new SkillLoadout();
        }

        private void ValidateReferences()
        {
            if (database == null)
                Debug.LogError("[SkillLoadoutManager] GameDatabaseSO not assigned!");
        }
        #endregion

        #region Public Methods - Skill Assignment
        /// <summary>
        /// Assign a skill to a skill slot
        /// </summary>
        public bool AssignSkill(string skillId, SkillSlotType slotType)
        {
            // Validate skill exists
            AbilitySO skill = GetSkill(skillId);
            if (skill == null)
            {
                Debug.LogError($"[SkillLoadoutManager] Skill not found: {skillId}");
                return false;
            }

            // TODO Phase 3: Validate weapon compatibility
            // For now, allow any skill

            // Assign to slot
            SetSkillSlot(slotType, skillId);

            // Fire events
            OnSkillSlotChanged?.Invoke(slotType, skillId);
            OnLoadoutChanged?.Invoke();

            Debug.Log($"[SkillLoadoutManager] Assigned skill '{skill.AbilityName}' to {slotType}");
            return true;
        }

        /// <summary>
        /// Unassign a skill from a skill slot
        /// </summary>
        public bool UnassignSkill(SkillSlotType slotType)
        {
            SetSkillSlot(slotType, string.Empty);

            OnSkillSlotChanged?.Invoke(slotType, string.Empty);
            OnLoadoutChanged?.Invoke();

            Debug.Log($"[SkillLoadoutManager] Unassigned skill from {slotType}");
            return true;
        }
        #endregion

        #region Public Methods - Queries
        /// <summary>
        /// Get skill ID assigned to a slot
        /// </summary>
        public string GetSlotSkillId(SkillSlotType slotType)
        {
            return slotType switch
            {
                SkillSlotType.NormalSkill1 => _skillLoadout.normalSkill1Id,
                SkillSlotType.NormalSkill2 => _skillLoadout.normalSkill2Id,
                SkillSlotType.UltimateSkill => _skillLoadout.ultimateSkillId,
                _ => string.Empty
            };
        }

        /// <summary>
        /// Check if a slot is empty
        /// </summary>
        public bool IsSlotEmpty(SkillSlotType slotType)
        {
            return string.IsNullOrEmpty(GetSlotSkillId(slotType));
        }

        /// <summary>
        /// Check if a skill is assigned to any slot
        /// </summary>
        public bool IsSkillAssigned(string skillId)
        {
            return _skillLoadout.IsSkillAssigned(skillId);
        }

        /// <summary>
        /// Get skill from database
        /// </summary>
        public AbilitySO GetSkill(string skillId)
        {
            if (string.IsNullOrEmpty(skillId) || database == null)
                return null;

            ItemBaseSO item = database.GetItem(skillId);
            return item as AbilitySO;
        }
        #endregion

        #region Public Methods - Load/Save
        /// <summary>
        /// Load skill loadout from save data
        /// </summary>
        public void LoadSkillLoadout(SkillLoadoutSaveData saveData)
        {
            if (saveData == null)
            {
                Debug.LogWarning("[SkillLoadoutManager] Cannot load null save data");
                return;
            }

            _skillLoadout.normalSkill1Id = saveData.normalSkill1Id ?? string.Empty;
            _skillLoadout.normalSkill2Id = saveData.normalSkill2Id ?? string.Empty;
            _skillLoadout.ultimateSkillId = saveData.ultimateSkillId ?? string.Empty;

            OnLoadoutChanged?.Invoke();
            
            Debug.Log($"[SkillLoadoutManager] Loaded skill loadout - {_skillLoadout.GetAssignedSkillCount()} skills assigned");
        }

        /// <summary>
        /// Save skill loadout to data structure
        /// </summary>
        public SkillLoadoutSaveData SaveSkillLoadout()
        {
            return new SkillLoadoutSaveData
            {
                normalSkill1Id = _skillLoadout.normalSkill1Id,
                normalSkill2Id = _skillLoadout.normalSkill2Id,
                ultimateSkillId = _skillLoadout.ultimateSkillId
            };
        }

        /// <summary>
        /// Clear all skill slots
        /// </summary>
        public void ClearAll()
        {
            _skillLoadout.ClearAll();
            OnLoadoutChanged?.Invoke();
            
            Debug.Log("[SkillLoadoutManager] All skill slots cleared");
        }
        #endregion

        #region Private Methods
        private void InitializeSingleton()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        private void SetSkillSlot(SkillSlotType slotType, string skillId)
        {
            switch (slotType)
            {
                case SkillSlotType.NormalSkill1:
                    _skillLoadout.normalSkill1Id = skillId;
                    break;
                case SkillSlotType.NormalSkill2:
                    _skillLoadout.normalSkill2Id = skillId;
                    break;
                case SkillSlotType.UltimateSkill:
                    _skillLoadout.ultimateSkillId = skillId;
                    break;
            }
        }
        #endregion

        #region Debug Tools
        [ContextMenu("Debug: Print Skill Loadout")]
        private void DebugPrintLoadout()
        {
            Debug.Log("=== SKILL LOADOUT ===");
            Debug.Log($"Normal Skill 1: {_skillLoadout.normalSkill1Id}");
            Debug.Log($"Normal Skill 2: {_skillLoadout.normalSkill2Id}");
            Debug.Log($"Ultimate Skill: {_skillLoadout.ultimateSkillId}");
            Debug.Log($"Assigned Skills: {_skillLoadout.GetAssignedSkillCount()}");
        }

        [ContextMenu("Debug: Clear Loadout")]
        private void DebugClearLoadout()
        {
            ClearAll();
        }
        #endregion
    }
}