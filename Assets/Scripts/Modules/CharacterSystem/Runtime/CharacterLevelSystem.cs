// ════════════════════════════════════════════
// CharacterLevelSystem.cs
// Manages player leveling, EXP, and transcendence
// ════════════════════════════════════════════

using System;
using UnityEngine;
using Ascension.Data.SO.Character;

namespace Ascension.Character.Runtime
{
    [Serializable]
    public class CharacterLevelSystem
    {
        // ──────────────────────────────────────────────
        // Public Fields
        // ──────────────────────────────────────────────

        public int level = 1;
        public int currentEXP = 0;
        public int expToNextLevel = 100;
        public int unallocatedPoints = 0;
        public int transcendenceLevel = 0;
        public bool isTranscended = false;

        // ──────────────────────────────────────────────
        // Public Methods
        // ──────────────────────────────────────────────

        public void Initialize(CharacterBaseStatsSO baseStats)
        {
            level = 1;
            currentEXP = 0;
            expToNextLevel = CalculateEXPForLevel(2, baseStats);
            unallocatedPoints = 0;
            transcendenceLevel = 0;
            isTranscended = false;
        }

        /// <summary>
        /// Add EXP with level cap protection
        /// Returns number of levels gained
        /// </summary>
        public int AddExperience(int amount, CharacterBaseStatsSO baseStats)
        {
            // Check if at max level (hard cap at 1000 unless transcendence is enabled)
            int maxPossibleLevel = GetMaxPossibleLevel(baseStats);

            if (level >= maxPossibleLevel)
            {
                currentEXP = 0;
                Debug.Log($"[LevelSystem] Max level reached ({maxPossibleLevel}). No more EXP gain.");
                return 0;
            }

            currentEXP += amount;
            int levelsGained = 0;

            // Safety: Max 100 levels per EXP gain to prevent infinite loops
            const int MAX_LEVELS_PER_GAIN = 100;

            while (currentEXP >= expToNextLevel && level < maxPossibleLevel && levelsGained < MAX_LEVELS_PER_GAIN)
            {
                currentEXP -= expToNextLevel;
                LevelUp(baseStats);
                levelsGained++;
            }

            // Clamp EXP if max level reached
            if (level >= maxPossibleLevel)
            {
                currentEXP = 0;
                Debug.Log($"[LevelSystem] Reached max level {maxPossibleLevel}!");
            }

            if (levelsGained >= MAX_LEVELS_PER_GAIN)
            {
                currentEXP = Mathf.Min(currentEXP, expToNextLevel - 1);
                Debug.LogWarning($"[LevelSystem] EXP overflow detected! Gained {levelsGained} levels. Clamping EXP.");
            }

            return levelsGained;
        }

        /// <summary>
        /// Get maximum achievable level
        /// </summary>
        public int GetMaxPossibleLevel(CharacterBaseStatsSO baseStats)
        {
            if (!baseStats.enableTranscendence)
            {
                return baseStats.maxLevel;
            }

            return baseStats.maxLevel + baseStats.maxTranscendenceLevel;
        }

        /// <summary>
        /// Check if player is at max level
        /// </summary>
        public bool IsAtMaxLevel(CharacterBaseStatsSO baseStats)
        {
            return level >= GetMaxPossibleLevel(baseStats);
        }

        // ──────────────────────────────────────────────
        // Private Methods
        // ──────────────────────────────────────────────

        private void LevelUp(CharacterBaseStatsSO baseStats)
        {
            level++;
            unallocatedPoints += baseStats.pointsPerLevel;

            // Check for transcendence ONLY if enabled
            if (baseStats.enableTranscendence && level > baseStats.maxLevel)
            {
                if (!isTranscended)
                {
                    isTranscended = true;
                    transcendenceLevel = 1;
                    Debug.Log($"[LevelSystem] TRANSCENDENCE UNLOCKED! Transcendence Level: {transcendenceLevel}");
                }
                else
                {
                    transcendenceLevel++;
                    Debug.Log($"[LevelSystem] Transcendence increased to Level {transcendenceLevel}");
                }
            }

            expToNextLevel = CalculateEXPForLevel(level + 1, baseStats);
        }

        private int CalculateEXPForLevel(int targetLevel, CharacterBaseStatsSO baseStats)
        {
            if (targetLevel <= 1) return 0;

            int maxLevel = GetMaxPossibleLevel(baseStats);

            if (targetLevel > maxLevel)
            {
                return int.MaxValue;
            }

            // Normal leveling (1 - maxLevel)
            if (targetLevel <= baseStats.maxLevel)
            {
                return (int)(baseStats.baseEXPRequirement +
                             (targetLevel * baseStats.expLinearGrowth) +
                             Mathf.Pow(targetLevel, baseStats.expExponentGrowth));
            }

            // Transcendence leveling
            if (baseStats.enableTranscendence && targetLevel <= baseStats.maxLevel + baseStats.maxTranscendenceLevel)
            {
                int baseReq = CalculateEXPForLevel(baseStats.maxLevel, baseStats);
                int transcendLevel = targetLevel - baseStats.maxLevel;
                return (int)(baseReq * baseStats.transcendenceEXPMultiplier * transcendLevel);
            }

            return int.MaxValue;
        }
    }
}
