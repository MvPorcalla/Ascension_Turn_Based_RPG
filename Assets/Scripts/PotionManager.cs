// ──────────────────────────────────────────────────
// PotionManager.cs (Percentage-Based Update)
// Handles potion usage, effects, and turn-based HoT
// Supports both percentage and flat healing values
// ─────────────────────────────────────────────────

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PotionManager : MonoBehaviour
{
    public static PotionManager Instance { get; private set; }

    [Header("Runtime State")]
    private bool isInCombat = false;
    
    // Active effects
    private List<ActiveBuff> activeBuffs = new List<ActiveBuff>();
    private List<ActiveHealOverTurn> activeHealOverTurns = new List<ActiveHealOverTurn>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        // Update real-time buffs
        UpdateActiveBuffs();
    }

    #region Public API

    /// <summary>
    /// Use a potion on the player
    /// </summary>
    public bool UsePotion(PotionSO potion, PlayerStats playerStats, CharacterBaseStatsSO baseStats)
    {
        if (potion == null || playerStats == null)
        {
            Debug.LogWarning("[PotionManager] Invalid potion or player stats");
            return false;
        }

        // Check if potion can be used
        if (!potion.CanUse(isInCombat))
        {
            Debug.LogWarning($"[PotionManager] Cannot use {potion.itemName} in current state");
            return false;
        }

        // Apply potion effects
        ApplyPotionEffects(potion, playerStats, baseStats);

        Debug.Log($"[PotionManager] Used {potion.itemName}");
        return true;
    }

    /// <summary>
    /// Called at the start of each turn in combat
    /// Processes turn-based heal-over-time effects
    /// </summary>
    public void OnTurnStart()
    {
        if (!isInCombat) return;

        ProcessHealOverTurns();
        ProcessTurnBasedBuffs();
    }

    /// <summary>
    /// Set combat state
    /// </summary>
    public void SetCombatState(bool inCombat)
    {
        isInCombat = inCombat;
        Debug.Log($"[PotionManager] Combat state: {(inCombat ? "IN COMBAT" : "OUT OF COMBAT")}");

        // Clear turn-based effects when leaving combat
        if (!inCombat)
        {
            activeHealOverTurns.Clear();
        }
    }

    /// <summary>
    /// Check if currently in combat
    /// </summary>
    public bool IsInCombat()
    {
        return isInCombat;
    }

    /// <summary>
    /// Get active heal-over-turn effects
    /// </summary>
    public List<ActiveHealOverTurn> GetActiveHealOverTurns()
    {
        return new List<ActiveHealOverTurn>(activeHealOverTurns);
    }

    #endregion

    #region Potion Effects

    private void ApplyPotionEffects(PotionSO potion, PlayerStats playerStats, CharacterBaseStatsSO baseStats)
    {
        // Apply health restore
        if (potion.healthRestore > 0)
        {
            // Calculate actual heal amount based on percentage or flat
            float healAmount = potion.GetActualHealAmount(playerStats.MaxHP);

            switch (potion.durationType)
            {
                case DurationType.Instant:
                    // Instant heal
                    HealPlayer(playerStats, healAmount);
                    break;

                case DurationType.RealTime:
                    // Heal over time (real-time)
                    StartCoroutine(HealOverTime(playerStats, healAmount, potion.restoreDuration));
                    break;

                case DurationType.TurnBased:
                    // Heal over turns
                    AddHealOverTurn(potion.itemName, playerStats, healAmount, potion.TurnDuration);
                    break;
            }
        }

        // Apply mana restore (TODO: Add mana to PlayerStats)
        if (potion.manaRestore > 0)
        {
            float manaAmount = potion.GetActualManaAmount(100f); // TODO: Use actual max mana
            Debug.Log($"[PotionManager] Restored {manaAmount} mana (STUB - mana not implemented yet)");
        }

        // Apply buffs
        if (potion.buffs != null && potion.buffs.Count > 0)
        {
            foreach (var buff in potion.buffs)
            {
                ApplyBuff(buff, playerStats, baseStats);
            }
        }
    }

    #endregion

    #region Instant Healing

    private void HealPlayer(PlayerStats playerStats, float amount)
    {
        float oldHP = playerStats.CurrentHP;
        float maxHP = playerStats.MaxHP;
        
        // Calculate new HP
        float newHP = Mathf.Min(oldHP + amount, maxHP);
        float actualHealed = newHP - oldHP;

        // Apply healing
        playerStats.combatRuntime.currentHP = newHP;

        Debug.Log($"[PotionManager] Healed {actualHealed:F0} HP ({oldHP:F0} → {newHP:F0})");

        // Trigger event through CharacterManager
        if (CharacterManager.Instance != null)
        {
            CharacterManager.Instance.ApplyHeal(amount);
        }
    }

    #endregion

    #region Real-Time Heal Over Time

    private IEnumerator HealOverTime(PlayerStats playerStats, float totalAmount, float duration)
    {
        float healPerSecond = totalAmount / duration;
        float elapsed = 0f;

        Debug.Log($"[PotionManager] Healing {totalAmount:F0} HP over {duration}s ({healPerSecond:F1} HP/s)");

        while (elapsed < duration)
        {
            float deltaHeal = healPerSecond * Time.deltaTime;
            HealPlayer(playerStats, deltaHeal);

            elapsed += Time.deltaTime;
            yield return null;
        }

        Debug.Log($"[PotionManager] Heal over time completed");
    }

    #endregion

    #region Turn-Based Heal Over Turn

    private void AddHealOverTurn(string potionName, PlayerStats playerStats, float totalHeal, int turnCount)
    {
        float healPerTurn = totalHeal / turnCount;

        ActiveHealOverTurn hot = new ActiveHealOverTurn
        {
            potionName = potionName,
            playerStats = playerStats,
            healPerTurn = healPerTurn,
            totalHealRemaining = totalHeal,
            turnsRemaining = turnCount
        };

        activeHealOverTurns.Add(hot);

        Debug.Log($"[PotionManager] Applied Heal over Turn: {healPerTurn:F0} HP/turn for {turnCount} turns (Total: {totalHeal:F0})");
    }

    private void ProcessHealOverTurns()
    {
        for (int i = activeHealOverTurns.Count - 1; i >= 0; i--)
        {
            var hot = activeHealOverTurns[i];

            // Apply heal for this turn
            HealPlayer(hot.playerStats, hot.healPerTurn);

            // Update remaining
            hot.turnsRemaining--;
            hot.totalHealRemaining -= hot.healPerTurn;

            // Remove if expired
            if (hot.turnsRemaining <= 0)
            {
                Debug.Log($"[PotionManager] Heal over Turn expired: {hot.potionName}");
                activeHealOverTurns.RemoveAt(i);
            }
        }
    }

    #endregion

    #region Buff System

    private void ApplyBuff(PotionBuff buff, PlayerStats playerStats, CharacterBaseStatsSO baseStats)
    {
        Debug.Log($"[PotionManager] Applied buff: {buff.type} (+{buff.value}) for {buff.duration}");

        ActiveBuff activeBuff = new ActiveBuff
        {
            buffType = buff.type,
            value = buff.value,
            duration = buff.duration,
            remainingTime = buff.duration,
            durationType = buff.durationType
        };

        activeBuffs.Add(activeBuff);

        // TODO: Implement actual buff effects on PlayerStats
    }

    private void UpdateActiveBuffs()
    {
        // Update real-time buffs only
        for (int i = activeBuffs.Count - 1; i >= 0; i--)
        {
            if (activeBuffs[i].durationType == DurationType.RealTime)
            {
                activeBuffs[i].remainingTime -= Time.deltaTime;

                if (activeBuffs[i].remainingTime <= 0)
                {
                    Debug.Log($"[PotionManager] Buff expired: {activeBuffs[i].buffType}");
                    activeBuffs.RemoveAt(i);
                }
            }
        }
    }

    private void ProcessTurnBasedBuffs()
    {
        // Process turn-based buffs
        for (int i = activeBuffs.Count - 1; i >= 0; i--)
        {
            if (activeBuffs[i].durationType == DurationType.TurnBased)
            {
                activeBuffs[i].remainingTime--;

                if (activeBuffs[i].remainingTime <= 0)
                {
                    Debug.Log($"[PotionManager] Turn-based buff expired: {activeBuffs[i].buffType}");
                    activeBuffs.RemoveAt(i);
                }
            }
        }
    }

    /// <summary>
    /// Get all active buffs
    /// </summary>
    public List<ActiveBuff> GetActiveBuffs()
    {
        return new List<ActiveBuff>(activeBuffs);
    }

    #endregion

    #region Debug

    [ContextMenu("Debug: Enter Combat")]
    private void DebugEnterCombat()
    {
        SetCombatState(true);
    }

    [ContextMenu("Debug: Exit Combat")]
    private void DebugExitCombat()
    {
        SetCombatState(false);
    }

    [ContextMenu("Debug: Trigger Turn")]
    private void DebugTriggerTurn()
    {
        OnTurnStart();
        Debug.Log("[PotionManager] Turn triggered manually");
    }

    [ContextMenu("Debug: Print Active Effects")]
    private void DebugPrintEffects()
    {
        Debug.Log($"=== ACTIVE EFFECTS ===");
        Debug.Log($"Heal over Turns: {activeHealOverTurns.Count}");
        foreach (var hot in activeHealOverTurns)
        {
            Debug.Log($"  • {hot.potionName}: {hot.healPerTurn:F0} HP/turn for {hot.turnsRemaining} turns");
        }

        Debug.Log($"Active Buffs: {activeBuffs.Count}");
        foreach (var buff in activeBuffs)
        {
            Debug.Log($"  • {buff.buffType} (+{buff.value}) - {buff.remainingTime:F1} remaining");
        }
    }

    #endregion
}

// ──────────────────────────────────────────────────
// Supporting Classes
// ──────────────────────────────────────────────────

[System.Serializable]
public class ActiveBuff
{
    public BuffType buffType;
    public float value;
    public float duration;
    public float remainingTime;
    public DurationType durationType = DurationType.RealTime;
}

[System.Serializable]
public class ActiveHealOverTurn
{
    public string potionName;
    public PlayerStats playerStats;
    public float healPerTurn;
    public float totalHealRemaining;
    public int turnsRemaining;
}