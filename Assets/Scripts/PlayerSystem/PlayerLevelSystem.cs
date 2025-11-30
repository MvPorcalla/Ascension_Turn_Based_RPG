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
    /// Add EXP with overflow protection
    /// Returns number of levels gained
    /// </summary>
    public int AddExperience(int amount, CharacterBaseStatsSO baseStats)
    {
        currentEXP += amount;
        
        int maxPossibleLevel = GetMaxPossibleLevel(baseStats);
        int levelsGained = 0;
        
        // Safety: Max 100 levels per EXP gain to prevent infinite loops
        const int MAX_LEVELS_PER_GAIN = 100;
        
        while (currentEXP >= expToNextLevel && level < maxPossibleLevel && levelsGained < MAX_LEVELS_PER_GAIN)
        {
            currentEXP -= expToNextLevel;
            LevelUp(baseStats);
            levelsGained++;
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
        
        // Check for transcendence
        if (level > baseStats.maxLevel && baseStats.enableTranscendence)
        {
            if (!isTranscended)
            {
                isTranscended = true;
                transcendenceLevel = 1;
                Debug.Log($"[LevelSystem] TRANSCENDENCE UNLOCKED! Level: {transcendenceLevel}");
            }
            else
            {
                transcendenceLevel++;
            }
        }
        
        expToNextLevel = CalculateEXPForLevel(level + 1, baseStats);
    }
    
    public int GetMaxPossibleLevel(CharacterBaseStatsSO baseStats)
    {
        int max = baseStats.maxLevel;
        if (baseStats.enableTranscendence)
            max += baseStats.maxTranscendenceLevel;
        return max;
    }
    
    private int CalculateEXPForLevel(int targetLevel, CharacterBaseStatsSO baseStats)
    {
        if (targetLevel <= 1) return 0;
        
        // Normal leveling
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