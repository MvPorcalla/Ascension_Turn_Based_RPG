// ════════════════════════════════════════════════════════════════════════
// Assets/Scripts/Modules/SkillSystem/Manager/SkillLoadoutManager.cs
// ✅ Manages equipped skill slots (like EquipmentManager for skills)
// TODO: Add skill unlock validation when SkillCollectionManager is implemented
// ════════════════════════════════════════════════════════════════════════

using UnityEngine;
using Ascension.Core;
using Ascension.Data.Save;

namespace Ascension.Skill.Manager
{
    /// <summary>
    /// Manages which skills are equipped in the player's loadout
    /// Similar to EquipmentManager, but for skills
    /// TODO: Integrate with SkillCollectionManager for unlock validation
    /// </summary>
    public class SkillLoadoutManager : MonoBehaviour
    {
        #region Private Fields
        private string _normalSkill1Id = string.Empty;
        private string _normalSkill2Id = string.Empty;
        private string _ultimateSkillId = string.Empty;
        #endregion

        #region Properties
        public string NormalSkill1Id => _normalSkill1Id;
        public string NormalSkill2Id => _normalSkill2Id;
        public string UltimateSkillId => _ultimateSkillId;
        #endregion

        #region Initialization
        public void Init()
        {
            Debug.Log("[SkillLoadoutManager] Initialized (stub - full system pending)");
        }
        #endregion

        #region Public API
        /// <summary>
        /// Equip a skill to a slot
        /// TODO: Add validation against SkillCollectionManager (check if unlocked)
        /// TODO: Add skill type validation (normal vs ultimate)
        /// </summary>
        public bool EquipSkill(string skillId, int slotIndex)
        {
            if (string.IsNullOrEmpty(skillId))
            {
                Debug.LogWarning("[SkillLoadoutManager] Cannot equip null/empty skill ID");
                return false;
            }

            // TODO: Validate skill is unlocked
            // if (!GameBootstrap.SkillCollection.IsSkillUnlocked(skillId))
            //     return false;

            // TODO: Validate skill type matches slot (normal vs ultimate)

            switch (slotIndex)
            {
                case 0:
                    _normalSkill1Id = skillId;
                    Debug.Log($"[SkillLoadoutManager] Equipped {skillId} to Normal Slot 1");
                    break;
                case 1:
                    _normalSkill2Id = skillId;
                    Debug.Log($"[SkillLoadoutManager] Equipped {skillId} to Normal Slot 2");
                    break;
                case 2:
                    _ultimateSkillId = skillId;
                    Debug.Log($"[SkillLoadoutManager] Equipped {skillId} to Ultimate Slot");
                    break;
                default:
                    Debug.LogWarning($"[SkillLoadoutManager] Invalid slot index: {slotIndex}");
                    return false;
            }

            GameEvents.TriggerSkillLoadoutChanged();
            return true;
        }

        public void UnequipSkill(int slotIndex)
        {
            switch (slotIndex)
            {
                case 0:
                    _normalSkill1Id = string.Empty;
                    break;
                case 1:
                    _normalSkill2Id = string.Empty;
                    break;
                case 2:
                    _ultimateSkillId = string.Empty;
                    break;
                default:
                    Debug.LogWarning($"[SkillLoadoutManager] Invalid slot index: {slotIndex}");
                    return;
            }

            GameEvents.TriggerSkillLoadoutChanged();
        }

        public void ClearAllSkills()
        {
            _normalSkill1Id = string.Empty;
            _normalSkill2Id = string.Empty;
            _ultimateSkillId = string.Empty;

            GameEvents.TriggerSkillLoadoutChanged();
        }
        #endregion

        #region Save/Load
        public SkillLoadoutSaveData SaveSkillLoadout()
        {
            return new SkillLoadoutSaveData
            {
                normalSkill1Id = _normalSkill1Id,
                normalSkill2Id = _normalSkill2Id,
                ultimateSkillId = _ultimateSkillId
            };
        }

        public void LoadSkillLoadout(SkillLoadoutSaveData saveData)
        {
            if (saveData == null)
            {
                Debug.LogWarning("[SkillLoadoutManager] Cannot load null save data");
                return;
            }

            _normalSkill1Id = saveData.normalSkill1Id ?? string.Empty;
            _normalSkill2Id = saveData.normalSkill2Id ?? string.Empty;
            _ultimateSkillId = saveData.ultimateSkillId ?? string.Empty;

            GameEvents.TriggerSkillLoadoutChanged();
        }
        #endregion

        #region Debug
        [ContextMenu("Debug: Print Loadout")]
        private void DebugPrintLoadout()
        {
            Debug.Log("=== SKILL LOADOUT ===");
            Debug.Log($"Normal 1: {(_normalSkill1Id == string.Empty ? "Empty" : _normalSkill1Id)}");
            Debug.Log($"Normal 2: {(_normalSkill2Id == string.Empty ? "Empty" : _normalSkill2Id)}");
            Debug.Log($"Ultimate: {(_ultimateSkillId == string.Empty ? "Empty" : _ultimateSkillId)}");
        }
        #endregion
    }
}