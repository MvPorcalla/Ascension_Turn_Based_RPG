// ──────────────────────────────────────────────────
// PotionManager.cs (Updated)
// Handles potion usage, effects, and cooldowns
// Integrated with CharacterManager for event-driven updates
// ─────────────────────────────────────────────────

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PotionManager : MonoBehaviour
{
    public static PotionManager Instance { get; private set; }

    [Header("Settings")]
    [SerializeField] private bool allowPotionsDuringCooldown = false;

    [Header("Runtime State")]
    private float potionCooldownRemaining = 0f;
    private bool isInCombat = false;
    
    // Active buff tracking (for future buff system)
    private List<ActiveBuff> activeBuffs = new List<ActiveBuff>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        // Update cooldown
        if (potionCooldownRemaining > 0)
        {
            potionCooldownRemaining -= Time.deltaTime;
        }

        // Update active buffs
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
            return false;
        }

        // Check cooldown
        if (potionCooldownRemaining > 0 && !allowPotionsDuringCooldown)
        {
            Debug.Log($"[PotionManager] Potion on cooldown: {potionCooldownRemaining:F1}s remaining");
            return false;
        }

        // Apply potion effects
        ApplyPotionEffects(potion, playerStats, baseStats);

        // Start cooldown
        if (potion.cooldown > 0)
        {
            potionCooldownRemaining = potion.cooldown;
        }

        Debug.Log($"[PotionManager] Used {potion.itemName}");
        return true;
    }

    /// <summary>
    /// Set combat state
    /// </summary>
    public void SetCombatState(bool inCombat)
    {
        isInCombat = inCombat;
        Debug.Log($"[PotionManager] Combat state: {(inCombat ? "IN COMBAT" : "OUT OF COMBAT")}");
    }

    /// <summary>
    /// Get remaining cooldown time
    /// </summary>
    public float GetCooldownRemaining()
    {
        return Mathf.Max(0, potionCooldownRemaining);
    }

    /// <summary>
    /// Check if potion cooldown is active
    /// </summary>
    public bool IsOnCooldown()
    {
        return potionCooldownRemaining > 0;
    }

    #endregion

    #region Potion Effects

    private void ApplyPotionEffects(PotionSO potion, PlayerStats playerStats, CharacterBaseStatsSO baseStats)
    {
        // Apply health restore
        if (potion.healthRestore > 0)
        {
            if (potion.restoreDuration > 0)
            {
                // Heal over time
                StartCoroutine(HealOverTime(playerStats, potion.healthRestore, potion.restoreDuration));
            }
            else
            {
                // Instant heal
                HealPlayer(playerStats, potion.healthRestore);
            }
        }

        // Apply mana restore (TODO: Add mana to PlayerStats)
        if (potion.manaRestore > 0)
        {
            Debug.Log($"[PotionManager] Restored {potion.manaRestore} mana (STUB - mana not implemented yet)");
            // TODO: Implement when PlayerStats has mana
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

    private void HealPlayer(PlayerStats playerStats, float amount)
    {
        float oldHP = playerStats.CurrentHP;
        float maxHP = playerStats.MaxHP;
        
        // Calculate new HP
        float newHP = Mathf.Min(oldHP + amount, maxHP);
        float actualHealed = newHP - oldHP;

        // Apply healing
        playerStats.combatRuntime.currentHP = newHP;

        Debug.Log($"[PotionManager] Healed {actualHealed} HP ({oldHP:F0} → {newHP:F0})");

        // ✅ Trigger event through CharacterManager
        if (CharacterManager.Instance != null)
        {
            CharacterManager.Instance.ApplyHeal(amount);
        }
    }

    private IEnumerator HealOverTime(PlayerStats playerStats, float totalAmount, float duration)
    {
        float healPerSecond = totalAmount / duration;
        float elapsed = 0f;

        Debug.Log($"[PotionManager] Healing {totalAmount} HP over {duration}s ({healPerSecond:F1} HP/s)");

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

    #region Buff System (Stub)

    private void ApplyBuff(PotionBuff buff, PlayerStats playerStats, CharacterBaseStatsSO baseStats)
    {
        Debug.Log($"[PotionManager] Applied buff: {buff.type} (+{buff.value}) for {buff.duration}s");

        // Create active buff
        ActiveBuff activeBuff = new ActiveBuff
        {
            buffType = buff.type,
            value = buff.value,
            duration = buff.duration,
            remainingTime = buff.duration
        };

        activeBuffs.Add(activeBuff);

        // TODO: Implement actual buff effects on PlayerStats
        // You'll need to modify PlayerStats to support temporary buffs
        // For now, just track them
    }

    private void UpdateActiveBuffs()
    {
        for (int i = activeBuffs.Count - 1; i >= 0; i--)
        {
            activeBuffs[i].remainingTime -= Time.deltaTime;

            if (activeBuffs[i].remainingTime <= 0)
            {
                Debug.Log($"[PotionManager] Buff expired: {activeBuffs[i].buffType}");
                activeBuffs.RemoveAt(i);
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

    [ContextMenu("Debug: Print Active Buffs")]
    private void DebugPrintBuffs()
    {
        if (activeBuffs.Count == 0)
        {
            Debug.Log("[PotionManager] No active buffs");
            return;
        }

        Debug.Log($"=== ACTIVE BUFFS ({activeBuffs.Count}) ===");
        foreach (var buff in activeBuffs)
        {
            Debug.Log($"  {buff.buffType} (+{buff.value}) - {buff.remainingTime:F1}s remaining");
        }
    }

    #endregion
}

// ──────────────────────────────────────────────────
// Supporting Class
// ──────────────────────────────────────────────────

[System.Serializable]
public class ActiveBuff
{
    public BuffType buffType;
    public float value;
    public float duration;
    public float remainingTime;
}