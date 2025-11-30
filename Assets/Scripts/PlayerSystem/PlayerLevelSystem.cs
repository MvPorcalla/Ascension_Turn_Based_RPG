// -------------------------------
// PlayerLevelSystem.cs
// Handles leveling, EXP, and transcendence
// -------------------------------

using System;
using UnityEngine;

[Serializable]
public class PlayerLevelSystem
{
    public int level = 1;
    public int currentEXP = 0;
    public int expToNextLevel = 100;
    public int unallocatedPoints = 0;
    public int transcendenceLevel = 0;
    public bool isTranscended = false;
    
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
            // At max level - stop gaining EXP
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
        
        // If we hit max level during this gain, dump excess EXP
        if (level >= maxPossibleLevel)
        {
            currentEXP = 0;
            Debug.Log($"[LevelSystem] Reached max level {maxPossibleLevel}!");
        }
        
        // If still over EXP after max levels, clamp it
        if (levelsGained >= MAX_LEVELS_PER_GAIN)
        {
            currentEXP = Mathf.Min(currentEXP, expToNextLevel - 1);
            Debug.LogWarning($"[LevelSystem] EXP overflow detected! Gained {levelsGained} levels. Clamping EXP.");
        }
        
        return levelsGained;
    }
    
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
    
    /// <summary>
    /// Get maximum achievable level
    /// </summary>
    public int GetMaxPossibleLevel(CharacterBaseStatsSO baseStats)
    {
        // If transcendence is disabled, hard cap at 1000
        if (!baseStats.enableTranscendence)
        {
            return baseStats.maxLevel;
        }
        
        // If transcendence is enabled, allow up to maxLevel + maxTranscendenceLevel
        return baseStats.maxLevel + baseStats.maxTranscendenceLevel;
    }
    
    /// <summary>
    /// Check if player is at max level
    /// </summary>
    public bool IsAtMaxLevel(CharacterBaseStatsSO baseStats)
    {
        return level >= GetMaxPossibleLevel(baseStats);
    }
    
    private int CalculateEXPForLevel(int targetLevel, CharacterBaseStatsSO baseStats)
    {
        if (targetLevel <= 1) return 0;
        
        int maxLevel = GetMaxPossibleLevel(baseStats);
        
        // If target is beyond max, return max value (prevents leveling)
        if (targetLevel > maxLevel)
        {
            return int.MaxValue;
        }
        
        // Normal leveling (1-1000)
        if (targetLevel <= baseStats.maxLevel)
        {
            return (int)(baseStats.baseEXPRequirement + 
                        (targetLevel * baseStats.expLinearGrowth) + 
                        Mathf.Pow(targetLevel, baseStats.expExponentGrowth));
        }
        
        // Transcendence leveling (1001+) - ONLY if enabled
        if (baseStats.enableTranscendence && targetLevel <= baseStats.maxLevel + baseStats.maxTranscendenceLevel)
        {
            int baseReq = CalculateEXPForLevel(baseStats.maxLevel, baseStats);
            int transcendLevel = targetLevel - baseStats.maxLevel;
            return (int)(baseReq * baseStats.transcendenceEXPMultiplier * transcendLevel);
        }
        
        return int.MaxValue;
    }
}