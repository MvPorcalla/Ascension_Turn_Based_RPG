// ──────────────────────────────────────────────────
// PlayerCombatRuntime.cs
// Manages runtime combat state for the player
// ──────────────────────────────────────────────────

using System;
using UnityEngine;

[Serializable]
public class PlayerCombatRuntime
{
    public float currentHP;
    
    // Future: Add these when needed
    // public List<StatusEffect> activeEffects;
    // public Dictionary<string, float> skillCooldowns;
    // public float currentShield;
    // public int combo;
    
    public void Initialize(float maxHP)
    {
        currentHP = maxHP;
    }
    
    /// <summary>
    /// Update HP when max HP changes (from equipment, level up, etc.)
    /// Maintains HP percentage if not a full heal
    /// </summary>
    public void OnMaxHPChanged(float oldMaxHP, float newMaxHP, bool fullHeal = false)
    {
        if (fullHeal || oldMaxHP <= 0)
        {
            currentHP = newMaxHP;
        }
        else
        {
            // Maintain HP percentage
            float hpPercent = currentHP / oldMaxHP;
            currentHP = newMaxHP * hpPercent;
            currentHP = Mathf.Clamp(currentHP, 0, newMaxHP);
        }
    }
    
    /// <summary>
    /// Get HP as a percentage (0-1)
    /// </summary>
    public float GetHPPercent(float maxHP)
    {
        if (maxHP <= 0) return 1f;
        return Mathf.Clamp01(currentHP / maxHP);
    }
    
    /// <summary>
    /// Apply damage
    /// </summary>
    public void TakeDamage(float amount, float maxHP)
    {
        currentHP = Mathf.Max(0, currentHP - amount);
    }
    
    /// <summary>
    /// Apply healing
    /// </summary>
    public void Heal(float amount, float maxHP)
    {
        currentHP = Mathf.Min(maxHP, currentHP + amount);
    }
    
    /// <summary>
    /// Check if dead
    /// </summary>
    public bool IsDead() => currentHP <= 0;
}