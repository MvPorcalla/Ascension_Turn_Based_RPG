// ════════════════════════════════════════════
// Assets\Scripts\Modules\EquipmentSystem\Data\SkillLoadout.cs
// Pure data class for skill loadout configuration (renamed from HotbarLoadout)
// ════════════════════════════════════════════

using System;

namespace Ascension.Equipment.Data
{
    /// <summary>
    /// Represents the player's skill loadout
    /// Pure data structure - no logic
    /// </summary>
    [Serializable]
    public class SkillLoadout
    {
        // Skill slots
        public string normalSkill1Id = string.Empty;
        public string normalSkill2Id = string.Empty;
        public string ultimateSkillId = string.Empty;

        /// <summary>
        /// Check if a skill is assigned to any skill slot
        /// </summary>
        public bool IsSkillAssigned(string skillId)
        {
            if (string.IsNullOrEmpty(skillId))
                return false;

            return normalSkill1Id == skillId ||
                   normalSkill2Id == skillId ||
                   ultimateSkillId == skillId;
        }

        /// <summary>
        /// Clear all skill slots
        /// </summary>
        public void ClearAll()
        {
            normalSkill1Id = string.Empty;
            normalSkill2Id = string.Empty;
            ultimateSkillId = string.Empty;
        }

        /// <summary>
        /// Get count of assigned skill slots
        /// </summary>
        public int GetAssignedSkillCount()
        {
            int count = 0;
            if (!string.IsNullOrEmpty(normalSkill1Id)) count++;
            if (!string.IsNullOrEmpty(normalSkill2Id)) count++;
            if (!string.IsNullOrEmpty(ultimateSkillId)) count++;
            return count;
        }
    }
}