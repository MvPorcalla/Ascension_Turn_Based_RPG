// ════════════════════════════════════════════════════════════════════════
// Assets/Scripts/Modules/PotionSystem/Manager/PotionManager.cs
// ✅ COMPLETE REFACTOR: GameBootstrap pattern, no ServiceContainer, no singleton
// Manages potion usage, effects, and combat state
// ════════════════════════════════════════════════════════════════════════

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ascension.Core;
using Ascension.Data.SO.Item;
using Ascension.Character.Core;
using Ascension.Data.SO.Character;
using Ascension.Character.Manager;
using Ascension.PotionSystem.Data;

namespace Ascension.PotionSystem.Manager
{
    public class PotionManager : MonoBehaviour
    {
        #region Serialized Fields
        [Header("Runtime State")]
        [SerializeField] private bool isInCombat = false;
        
        [Header("Debug")]
        [SerializeField] private bool enableDebugLogs = true;
        #endregion
        
        #region Private Fields
        private CharacterManager _characterManager;
        private List<ActiveBuff> _activeBuffs = new List<ActiveBuff>();
        private List<ActiveHealOverTurn> _activeHealOverTurns = new List<ActiveHealOverTurn>();
        private bool _isInitialized = false;
        #endregion
        
        #region Properties
        public bool IsInCombat => isInCombat;
        public bool IsInitialized => _isInitialized;
        #endregion
        
        #region Initialization
        /// <summary>
        /// ✅ Called by GameBootstrap during initialization
        /// </summary>
        public void Init()
        {
            _characterManager = GameBootstrap.Character;
            
            if (_characterManager == null)
            {
                Debug.LogError("[PotionManager] CharacterManager not found in GameBootstrap!");
                return;
            }
            
            _isInitialized = true;
            Log("PotionManager initialized successfully");
        }
        #endregion
        
        #region Unity Callbacks
        private void Update()
        {
            if (!_isInitialized) return;
            UpdateActiveBuffs();
        }
        #endregion
        
        #region Public Methods - Potion Usage
        /// <summary>
        /// Use a potion (simplified - gets player automatically)
        /// </summary>
        public bool UsePotion(PotionSO potion)
        {
            if (!_isInitialized)
            {
                Debug.LogWarning("[PotionManager] Not initialized yet!");
                return false;
            }
            
            if (_characterManager == null || !_characterManager.HasActivePlayer)
            {
                Debug.LogWarning("[PotionManager] No active player!");
                return false;
            }
            
            if (!ValidatePotionUsage(potion))
            {
                return false;
            }
            
            if (!potion.CanUse(isInCombat))
            {
                Debug.LogWarning($"[PotionManager] Cannot use {potion.ItemName} in current state");
                return false;
            }

            CharacterStats characterStats = _characterManager.CurrentPlayer;
            CharacterBaseStatsSO baseStats = _characterManager.BaseStats;
            
            ApplyPotionEffects(potion, characterStats, baseStats);
            
            Log($"Used {potion.ItemName}");
            return true;
        }

        /// <summary>
        /// Use a potion (legacy signature - for backward compatibility)
        /// </summary>
        public bool UsePotion(PotionSO potion, CharacterStats characterStats, CharacterBaseStatsSO baseStats)
        {
            if (!ValidatePotionUsage(potion))
            {
                return false;
            }
            
            if (!potion.CanUse(isInCombat))
            {
                Debug.LogWarning($"[PotionManager] Cannot use {potion.ItemName} in current state");
                return false;
            }

            ApplyPotionEffects(potion, characterStats, baseStats);
            
            Log($"Used {potion.ItemName}");
            return true;
        }
        #endregion
        
        #region Public Methods - Turn Management
        /// <summary>
        /// Called at the start of each combat turn
        /// </summary>
        public void OnTurnStart()
        {
            if (!isInCombat) return;
            
            ProcessHealOverTurns();
            ProcessTurnBasedBuffs();
        }
        
        /// <summary>
        /// Set combat state (in/out of combat)
        /// </summary>
        public void SetCombatState(bool inCombat)
        {
            isInCombat = inCombat;
            Log($"Combat state: {(inCombat ? "IN COMBAT" : "OUT OF COMBAT")}");
            
            if (!inCombat)
            {
                ClearTurnBasedEffects();
            }
        }
        #endregion
        
        #region Public Methods - Query Active Effects
        /// <summary>
        /// Get all active heal-over-turn effects
        /// </summary>
        public List<ActiveHealOverTurn> GetActiveHealOverTurns()
        {
            return new List<ActiveHealOverTurn>(_activeHealOverTurns);
        }
        
        /// <summary>
        /// Get all active buffs
        /// </summary>
        public List<ActiveBuff> GetActiveBuffs()
        {
            return new List<ActiveBuff>(_activeBuffs);
        }
        #endregion
        
        #region Private Methods - Validation
        private bool ValidatePotionUsage(PotionSO potion)
        {
            if (potion == null)
            {
                Debug.LogWarning("[PotionManager] Potion is null");
                return false;
            }
            
            return true;
        }
        #endregion
        
        #region Private Methods - Potion Effects
        private void ApplyPotionEffects(PotionSO potion, CharacterStats characterStats, CharacterBaseStatsSO baseStats)
        {
            if (potion.HealthRestore > 0)
            {
                ApplyHealthRestore(potion, characterStats);
            }
            
            if (potion.ManaRestore > 0)
            {
                ApplyManaRestore(potion);
            }
            
            if (potion.buffs != null && potion.buffs.Count > 0)
            {
                ApplyBuffs(potion.buffs, characterStats, baseStats);
            }
        }
        
        private void ApplyHealthRestore(PotionSO potion, CharacterStats characterStats)
        {
            float healAmount = potion.GetActualHealAmount(characterStats.MaxHP);
            
            switch (potion.durationType)
            {
                case DurationType.Instant:
                    HealPlayer(healAmount);
                    break;
                    
                case DurationType.RealTime:
                    StartCoroutine(HealOverTime(characterStats, healAmount, potion.restoreDuration));
                    break;
                    
                case DurationType.TurnBased:
                    AddHealOverTurn(potion.ItemName, characterStats, healAmount, potion.TurnDuration);
                    break;
            }
        }
        
        private void ApplyManaRestore(PotionSO potion)
        {
            const float TEMP_MAX_MANA = 100f;
            float manaAmount = potion.GetActualManaAmount(TEMP_MAX_MANA);
            Log($"Restored {manaAmount} mana (STUB - mana not implemented yet)");
        }
        
        private void ApplyBuffs(List<PotionBuff> buffs, CharacterStats characterStats, CharacterBaseStatsSO baseStats)
        {
            foreach (var buff in buffs)
            {
                ApplyBuff(buff, characterStats, baseStats);
            }
        }
        #endregion
        
        #region Private Methods - Healing
        /// <summary>
        /// ✅ FIXED: Use CharacterManager.Heal() instead of ApplyHeal()
        /// </summary>
        private void HealPlayer(float amount)
        {
            if (_characterManager == null || !_characterManager.HasActivePlayer)
            {
                Debug.LogWarning("[PotionManager] Cannot heal - no active player");
                return;
            }
            
            // ✅ Use the correct method name from CharacterManager
            _characterManager.Heal(amount);
            Log($"Healed {amount:F0} HP");
        }
        
        /// <summary>
        /// Heal over time (real-time, not turn-based)
        /// </summary>
        private IEnumerator HealOverTime(CharacterStats characterStats, float totalAmount, float duration)
        {
            float healPerSecond = totalAmount / duration;
            float elapsed = 0f;
            
            Log($"Healing {totalAmount:F0} HP over {duration}s ({healPerSecond:F1} HP/s)");
            
            while (elapsed < duration)
            {
                float deltaHeal = healPerSecond * Time.deltaTime;
                HealPlayer(deltaHeal);
                
                elapsed += Time.deltaTime;
                yield return null;
            }
            
            Log("Heal over time completed");
        }
        #endregion
        
        #region Private Methods - Turn-Based Heal Over Turn
        private void AddHealOverTurn(string potionName, CharacterStats characterStats, float totalHeal, int turnCount)
        {
            float healPerTurn = totalHeal / turnCount;
            
            ActiveHealOverTurn hot = new ActiveHealOverTurn
            {
                potionName = potionName,
                characterStats = characterStats,
                healPerTurn = healPerTurn,
                totalHealRemaining = totalHeal,
                turnsRemaining = turnCount
            };
            
            _activeHealOverTurns.Add(hot);
            
            Log($"Applied Heal over Turn: {healPerTurn:F0} HP/turn for {turnCount} turns (Total: {totalHeal:F0})");
        }
        
        private void ProcessHealOverTurns()
        {
            for (int i = _activeHealOverTurns.Count - 1; i >= 0; i--)
            {
                var hot = _activeHealOverTurns[i];
                
                HealPlayer(hot.healPerTurn);
                
                hot.turnsRemaining--;
                hot.totalHealRemaining -= hot.healPerTurn;
                
                if (hot.turnsRemaining <= 0)
                {
                    Log($"Heal over Turn expired: {hot.potionName}");
                    _activeHealOverTurns.RemoveAt(i);
                }
            }
        }
        #endregion
        
        #region Private Methods - Buff System
        private void ApplyBuff(PotionBuff buff, CharacterStats characterStats, CharacterBaseStatsSO baseStats)
        {
            Log($"Applied buff: {buff.type} (+{buff.value}) for {buff.duration}");
            
            ActiveBuff activeBuff = new ActiveBuff
            {
                buffType = buff.type,
                value = buff.value,
                duration = buff.duration,
                remainingTime = buff.duration,
                durationType = buff.durationType
            };
            
            _activeBuffs.Add(activeBuff);
        }
        
        private void UpdateActiveBuffs()
        {
            for (int i = _activeBuffs.Count - 1; i >= 0; i--)
            {
                if (_activeBuffs[i].durationType == DurationType.RealTime)
                {
                    _activeBuffs[i].remainingTime -= Time.deltaTime;
                    
                    if (_activeBuffs[i].remainingTime <= 0)
                    {
                        Log($"Buff expired: {_activeBuffs[i].buffType}");
                        _activeBuffs.RemoveAt(i);
                    }
                }
            }
        }
        
        private void ProcessTurnBasedBuffs()
        {
            for (int i = _activeBuffs.Count - 1; i >= 0; i--)
            {
                if (_activeBuffs[i].durationType == DurationType.TurnBased)
                {
                    _activeBuffs[i].remainingTime--;
                    
                    if (_activeBuffs[i].remainingTime <= 0)
                    {
                        Log($"Turn-based buff expired: {_activeBuffs[i].buffType}");
                        _activeBuffs.RemoveAt(i);
                    }
                }
            }
        }
        #endregion
        
        #region Private Methods - Cleanup
        private void ClearTurnBasedEffects()
        {
            _activeHealOverTurns.Clear();
            Log("Cleared all turn-based effects (combat ended)");
        }
        #endregion
        
        #region Logging
        private void Log(string message)
        {
            if (enableDebugLogs)
                Debug.Log($"[PotionManager] {message}");
        }
        #endregion
        
        #region Debug Methods
        [ContextMenu("Debug: Enter Combat")]
        private void DebugEnterCombat() => SetCombatState(true);
        
        [ContextMenu("Debug: Exit Combat")]
        private void DebugExitCombat() => SetCombatState(false);
        
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
            Debug.Log($"Heal over Turns: {_activeHealOverTurns.Count}");
            
            foreach (var hot in _activeHealOverTurns)
            {
                Debug.Log($"  • {hot.potionName}: {hot.healPerTurn:F0} HP/turn for {hot.turnsRemaining} turns");
            }
            
            Debug.Log($"Active Buffs: {_activeBuffs.Count}");
            
            foreach (var buff in _activeBuffs)
            {
                Debug.Log($"  • {buff.buffType} (+{buff.value}) - {buff.remainingTime:F1} remaining");
            }
        }
        #endregion
    }
}