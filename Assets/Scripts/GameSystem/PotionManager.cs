// ════════════════════════════════════════════
// PotionManager.cs
// Handles potion usage, effects, and turn-based HoT
// Supports percentage and flat healing values
// ════════════════════════════════════════════

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ascension.Data.SO.Item;
using Ascension.Character.Stat;
using Ascension.Data.SO.Character;
using Ascension.Character.Manager;

namespace Ascension.GameSystem
{
    public class PotionManager : MonoBehaviour
    {
        #region Singleton
        public static PotionManager Instance { get; private set; }
        #endregion
        
        #region Serialized Fields
        [Header("Runtime State")]
        [SerializeField] private bool isInCombat = false;
        #endregion
        
        #region Private Fields
        private List<ActiveBuff> activeBuffs = new List<ActiveBuff>();
        private List<ActiveHealOverTurn> activeHealOverTurns = new List<ActiveHealOverTurn>();
        #endregion
        
        #region Properties
        public bool IsInCombat => isInCombat;
        #endregion
        
        #region Unity Callbacks
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            Instance = this;
        }
        
        private void Update()
        {
            UpdateActiveBuffs();
        }
        #endregion
        
        #region Public Methods - Potion Usage
        public bool UsePotion(PotionSO potion, CharacterStats CharacterStats, CharacterBaseStatsSO baseStats)
        {
            if (!ValidatePotionUsage(potion, CharacterStats))
            {
                return false;
            }
            
            if (!potion.CanUse(isInCombat))
            {
                Debug.LogWarning($"[PotionManager] Cannot use {potion.ItemName} in current state");
                return false;
            }

            ApplyPotionEffects(potion, CharacterStats, baseStats);
            
            Debug.Log($"[PotionManager] Used {potion.ItemName}");
            return true;
        }
        #endregion
        
        #region Public Methods - Turn Management
        public void OnTurnStart()
        {
            if (!isInCombat) return;
            
            ProcessHealOverTurns();
            ProcessTurnBasedBuffs();
        }
        
        public void SetCombatState(bool inCombat)
        {
            isInCombat = inCombat;
            Debug.Log($"[PotionManager] Combat state: {(inCombat ? "IN COMBAT" : "OUT OF COMBAT")}");
            
            if (!inCombat)
            {
                ClearTurnBasedEffects();
            }
        }
        #endregion
        
        #region Public Methods - Query Active Effects
        public List<ActiveHealOverTurn> GetActiveHealOverTurns()
        {
            return new List<ActiveHealOverTurn>(activeHealOverTurns);
        }
        
        public List<ActiveBuff> GetActiveBuffs()
        {
            return new List<ActiveBuff>(activeBuffs);
        }
        #endregion
        
        #region Private Methods - Validation
        private bool ValidatePotionUsage(PotionSO potion, CharacterStats CharacterStats)
        {
            if (potion == null)
            {
                Debug.LogWarning("[PotionManager] Potion is null");
                return false;
            }
            
            if (CharacterStats == null)
            {
                Debug.LogWarning("[PotionManager] CharacterStats is null");
                return false;
            }
            
            return true;
        }
        #endregion
        
        #region Private Methods - Potion Effects
        private void ApplyPotionEffects(PotionSO potion, CharacterStats CharacterStats, CharacterBaseStatsSO baseStats)
        {
            if (potion.HealthRestore > 0)
            {
                ApplyHealthRestore(potion, CharacterStats);
            }
            
            if (potion.ManaRestore > 0)
            {
                ApplyManaRestore(potion);
            }
            
            if (potion.buffs != null && potion.buffs.Count > 0)
            {
                ApplyBuffs(potion.buffs, CharacterStats, baseStats);
            }
        }
        
        private void ApplyHealthRestore(PotionSO potion, CharacterStats CharacterStats)
        {
            float healAmount = potion.GetActualHealAmount(CharacterStats.MaxHP);
            
            switch (potion.durationType)
            {
                case DurationType.Instant:
                    HealPlayer(CharacterStats, healAmount);
                    break;
                    
                case DurationType.RealTime:
                    StartCoroutine(HealOverTime(CharacterStats, healAmount, potion.restoreDuration));
                    break;
                    
                case DurationType.TurnBased:
                    AddHealOverTurn(potion.ItemName, CharacterStats, healAmount, potion.TurnDuration);
                    break;
            }
        }
        
        private void ApplyManaRestore(PotionSO potion)
        {
            const float TEMP_MAX_MANA = 100f;
            float manaAmount = potion.GetActualManaAmount(TEMP_MAX_MANA);
            Debug.Log($"[PotionManager] Restored {manaAmount} mana (STUB - mana not implemented yet)");
        }
        
        private void ApplyBuffs(List<PotionBuff> buffs, CharacterStats CharacterStats, CharacterBaseStatsSO baseStats)
        {
            foreach (var buff in buffs)
            {
                ApplyBuff(buff, CharacterStats, baseStats);
            }
        }
        #endregion
        
        #region Private Methods - Instant Healing
        private void HealPlayer(CharacterStats CharacterStats, float amount)
        {
            float oldHP = CharacterStats.CurrentHP;
            float maxHP = CharacterStats.MaxHP;
            float newHP = Mathf.Min(oldHP + amount, maxHP);
            float actualHealed = newHP - oldHP;
            
            CharacterStats.combatRuntime.currentHP = newHP;
            
            Debug.Log($"[PotionManager] Healed {actualHealed:F0} HP ({oldHP:F0} → {newHP:F0})");
            
            TriggerHealEvent(amount);
        }
        
        private void TriggerHealEvent(float amount)
        {
            if (CharacterManager.Instance != null)
            {
                CharacterManager.Instance.ApplyHeal(amount);
            }
        }
        #endregion
        
        #region Private Methods - Real-Time Heal Over Time
        private IEnumerator HealOverTime(CharacterStats CharacterStats, float totalAmount, float duration)
        {
            float healPerSecond = totalAmount / duration;
            float elapsed = 0f;
            
            Debug.Log($"[PotionManager] Healing {totalAmount:F0} HP over {duration}s ({healPerSecond:F1} HP/s)");
            
            while (elapsed < duration)
            {
                float deltaHeal = healPerSecond * Time.deltaTime;
                HealPlayer(CharacterStats, deltaHeal);
                
                elapsed += Time.deltaTime;
                yield return null;
            }
            
            Debug.Log("[PotionManager] Heal over time completed");
        }
        #endregion
        
        #region Private Methods - Turn-Based Heal Over Turn
        private void AddHealOverTurn(string potionName, CharacterStats CharacterStats, float totalHeal, int turnCount)
        {
            float healPerTurn = totalHeal / turnCount;
            
            ActiveHealOverTurn hot = new ActiveHealOverTurn
            {
                potionName = potionName,
                CharacterStats = CharacterStats,
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
                
                HealPlayer(hot.CharacterStats, hot.healPerTurn);
                
                hot.turnsRemaining--;
                hot.totalHealRemaining -= hot.healPerTurn;
                
                if (hot.turnsRemaining <= 0)
                {
                    Debug.Log($"[PotionManager] Heal over Turn expired: {hot.potionName}");
                    activeHealOverTurns.RemoveAt(i);
                }
            }
        }
        #endregion
        
        #region Private Methods - Buff System
        private void ApplyBuff(PotionBuff buff, CharacterStats CharacterStats, CharacterBaseStatsSO baseStats)
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
        }
        
        private void UpdateActiveBuffs()
        {
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
        #endregion
        
        #region Private Methods - Cleanup
        private void ClearTurnBasedEffects()
        {
            activeHealOverTurns.Clear();
        }
        #endregion
        
        #region Debug Methods
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
            Debug.Log("=== ACTIVE EFFECTS ===");
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
    
    // ════════════════════════════════════════════
    // Supporting Classes
    // ════════════════════════════════════════════
    
    [Serializable]
    public class ActiveBuff
    {
        public BuffType buffType;
        public float value;
        public float duration;
        public float remainingTime;
        public DurationType durationType = DurationType.RealTime;
    }
    
    [Serializable]
    public class ActiveHealOverTurn
    {
        public string potionName;
        public CharacterStats CharacterStats;
        public float healPerTurn;
        public float totalHealRemaining;
        public int turnsRemaining;
    }
}