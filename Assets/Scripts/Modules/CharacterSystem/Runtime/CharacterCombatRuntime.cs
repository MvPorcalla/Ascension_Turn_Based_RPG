// ════════════════════════════════════════════
// CharacterCombatRuntime.cs
// Manages runtime combat state for the player
// ✅ UPDATED: Added MP placeholder for future implementation
// ════════════════════════════════════════════

using System;
using UnityEngine;

namespace Ascension.Character.Runtime
{
    [Serializable]
    public class CharacterCombatRuntime
    {
        // ──────────────────────────────────────────────
        // Public Fields
        // ──────────────────────────────────────────────

        public float currentHP;
        
        /// <summary>
        /// ✅ MP Placeholder - always 0 until mana system is implemented
        /// Included for save/load compatibility but not yet used in gameplay
        /// </summary>
        public float currentMP = 0f;

        // Future: Add these when needed
        // public List<StatusEffect> activeEffects;
        // public Dictionary<string, float> skillCooldowns;
        // public float currentShield;
        // public int combo;

        // ──────────────────────────────────────────────
        // Public Methods
        // ──────────────────────────────────────────────

        public void Initialize(float maxHP)
        {
            currentHP = maxHP;
            currentMP = 0f; // ✅ Initialize MP to 0 (unused for now)
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
        
        // ══════════════════════════════════════════════════════════════
        // ✅ MP SYSTEM (Future Implementation)
        // ══════════════════════════════════════════════════════════════
        
        /// <summary>
        /// ✅ TODO: Implement mana system
        /// For now, MP is always 0
        /// </summary>
        public void RestoreMP(float amount, float maxMP)
        {
            // TODO: Implement when mana system is ready
            // currentMP = Mathf.Min(maxMP, currentMP + amount);
        }
        
        /// <summary>
        /// ✅ TODO: Implement mana consumption
        /// </summary>
        public bool ConsumeMP(float amount)
        {
            // TODO: Implement when mana system is ready
            // if (currentMP < amount) return false;
            // currentMP -= amount;
            // return true;
            return true; // Always succeed for now (no MP cost)
        }
    }
}