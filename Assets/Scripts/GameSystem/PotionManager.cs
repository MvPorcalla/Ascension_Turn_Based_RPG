// ════════════════════════════════════════════
// PotionManager.cs
// REFACTORED: Injects CharacterManager dependency
// ════════════════════════════════════════════

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ascension.Core;
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
        
        #region Injected Dependencies
        private CharacterManager _characterManager;
        #endregion
        
        #region Serialized Fields
        [Header("Runtime State")]
        [SerializeField] private bool isInCombat = false;
        #endregion
        
        #region Private Fields
        private List<ActiveBuff> _activeBuffs = new List<ActiveBuff>();
        private List<ActiveHealOverTurn> _activeHealOverTurns = new List<ActiveHealOverTurn>();
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

        private void Start()
        {
            InjectDependencies();
        }
        
        private void Update()
        {
            UpdateActiveBuffs();
        }
        #endregion
        
        #region Dependency Injection
        private void InjectDependencies()
        {
            ServiceContainer container = ServiceContainer.Instance;
            
            if (container == null || !container.IsInitialized)
            {
                Debug.LogWarning("[PotionManager] ServiceContainer not ready, retrying...");
                Invoke(nameof(InjectDependencies), 0.1f);
                return;
            }

            try
            {
                _characterManager = container.GetRequired<CharacterManager>();
                Debug.Log("[PotionManager] Dependencies injected successfully");
            }
            catch (InvalidOperationException e)
            {
                Debug.LogError($"[PotionManager] Failed to inject dependencies: {e.Message}");
            }
        }
        #endregion
        
        #region Public Methods - Potion Usage
        public bool UsePotion(PotionSO potion, CharacterStats characterStats, CharacterBaseStatsSO baseStats)
        {
            if (!ValidatePotionUsage(potion, characterStats))
            {
                return false;
            }
            
            if (!potion.CanUse(isInCombat))
            {
                Debug.LogWarning($"[PotionManager] Cannot use {potion.ItemName} in current state");
                return false;
            }

            ApplyPotionEffects(potion, characterStats, baseStats);
            
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
            return new List<ActiveHealOverTurn>(_activeHealOverTurns);
        }
        
        public List<ActiveBuff> GetActiveBuffs()
        {
            return new List<ActiveBuff>(_activeBuffs);
        }
        #endregion
        
        #region Private Methods - Validation
        private bool ValidatePotionUsage(PotionSO potion, CharacterStats characterStats)
        {
            if (potion == null)
            {
                Debug.LogWarning("[PotionManager] Potion is null");
                return false;
            }
            
            if (characterStats == null)
            {
                Debug.LogWarning("[PotionManager] CharacterStats is null");
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
                    HealPlayer(characterStats, healAmount);
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
            Debug.Log($"[PotionManager] Restored {manaAmount} mana (STUB - mana not implemented yet)");
        }
        
        private void ApplyBuffs(List<PotionBuff> buffs, CharacterStats characterStats, CharacterBaseStatsSO baseStats)
        {
            foreach (var buff in buffs)
            {
                ApplyBuff(buff, characterStats, baseStats);
            }
        }
        #endregion
        
        #region Private Methods - Instant Healing
        private void HealPlayer(CharacterStats characterStats, float amount)
        {
            float oldHP = characterStats.CurrentHP;
            float maxHP = characterStats.MaxHP;
            float newHP = Mathf.Min(oldHP + amount, maxHP);
            float actualHealed = newHP - oldHP;
            
            characterStats.combatRuntime.currentHP = newHP;
            
            Debug.Log($"[PotionManager] Healed {actualHealed:F0} HP ({oldHP:F0} → {newHP:F0})");
            
            TriggerHealEvent(amount);
        }
        
        private void TriggerHealEvent(float amount)
        {
            if (_characterManager != null)
            {
                _characterManager.ApplyHeal(amount);
            }
        }
        #endregion
        
        #region Private Methods - Real-Time Heal Over Time
        private IEnumerator HealOverTime(CharacterStats characterStats, float totalAmount, float duration)
        {
            float healPerSecond = totalAmount / duration;
            float elapsed = 0f;
            
            Debug.Log($"[PotionManager] Healing {totalAmount:F0} HP over {duration}s ({healPerSecond:F1} HP/s)");
            
            while (elapsed < duration)
            {
                float deltaHeal = healPerSecond * Time.deltaTime;
                HealPlayer(characterStats, deltaHeal);
                
                elapsed += Time.deltaTime;
                yield return null;
            }
            
            Debug.Log("[PotionManager] Heal over time completed");
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
            
            Debug.Log($"[PotionManager] Applied Heal over Turn: {healPerTurn:F0} HP/turn for {turnCount} turns (Total: {totalHeal:F0})");
        }
        
        private void ProcessHealOverTurns()
        {
            for (int i = _activeHealOverTurns.Count - 1; i >= 0; i--)
            {
                var hot = _activeHealOverTurns[i];
                
                HealPlayer(hot.characterStats, hot.healPerTurn);
                
                hot.turnsRemaining--;
                hot.totalHealRemaining -= hot.healPerTurn;
                
                if (hot.turnsRemaining <= 0)
                {
                    Debug.Log($"[PotionManager] Heal over Turn expired: {hot.potionName}");
                    _activeHealOverTurns.RemoveAt(i);
                }
            }
        }
        #endregion
        
        #region Private Methods - Buff System
        private void ApplyBuff(PotionBuff buff, CharacterStats characterStats, CharacterBaseStatsSO baseStats)
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
                        Debug.Log($"[PotionManager] Buff expired: {_activeBuffs[i].buffType}");
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
                        Debug.Log($"[PotionManager] Turn-based buff expired: {_activeBuffs[i].buffType}");
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
        public CharacterStats characterStats;
        public float healPerTurn;
        public float totalHealRemaining;
        public int turnsRemaining;
    }
}