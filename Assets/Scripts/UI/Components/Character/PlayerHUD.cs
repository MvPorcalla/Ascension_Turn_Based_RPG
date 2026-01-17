// ════════════════════════════════════════════════════════════════════════
// Assets/Scripts/UI/Components/Character/PlayerHud.cs
// ✅ FIXED: Removed immediate refresh in OnEnable() to prevent race condition
// ════════════════════════════════════════════════════════════════════════

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Ascension.Core;
using Ascension.Character.Stat;
using Ascension.Character.Manager;

namespace Ascension.UI
{
    public class PlayerHUD : MonoBehaviour
    {
        #region Serialized Fields
        [Header("Player Info")]
        [SerializeField] private TMP_Text playerNameText;
        [SerializeField] private TMP_Text levelText;

        [Header("Health Bar")]
        [SerializeField] private Image healthFill;
        [SerializeField] private TMP_Text healthText;

        [Header("EXP Bar")]
        [SerializeField] private Image expFill;
        [SerializeField] private TMP_Text expText;

        [Header("Settings")]
        [SerializeField] private bool smoothBars = true;
        [SerializeField] private float barLerpSpeed = 5f;
        #endregion

        #region Private Fields
        private float targetHealthFill;
        private float targetExpFill;
        #endregion

        #region Unity Callbacks
        private void OnEnable()
        {
            SubscribeToEvents();
            
            // ✅ CRITICAL FIX: Do NOT refresh immediately in OnEnable()
            // Reason: PlayerHUD instantiates BEFORE ServiceContainer.Start() runs
            // This means CharacterManager.Instance could be null when OnEnable fires
            // Instead, we wait for OnGameLoaded event which guarantees data exists
            
            // ❌ REMOVED: RefreshHUDSafe();
        }

        private void OnDisable()
        {
            UnsubscribeFromEvents();
        }

        private void Update()
        {
            if (smoothBars)
                AnimateBars();
        }
        #endregion

        #region Public Methods
        public void RefreshHUD()
        {
            var characterManager = CharacterManager.Instance;
            
            if (characterManager == null || !characterManager.HasActivePlayer)
            {
                HideHUD();
                return;
            }

            var player = characterManager.CurrentPlayer;
            UpdatePlayerInfo(player);
            UpdateHealthBar(player);
            UpdateExpBar(player);
        }
        
        private void HideHUD()
        {
            if (playerNameText != null) playerNameText.text = "";
            if (levelText != null) levelText.text = "";
            if (healthText != null) healthText.text = "";
            if (expText != null) expText.text = "";
            
            if (healthFill != null) healthFill.fillAmount = 0;
            if (expFill != null) expFill.fillAmount = 0;
        }
        #endregion

        #region Event Subscriptions
        /// <summary>
        /// ✅ Subscribe to GameEvents (static) instead of manager events
        /// Benefits:
        /// - No singleton dependency (survives manager destruction)
        /// - No null checks needed during unsubscribe
        /// - No memory leak risk
        /// </summary>
        private void SubscribeToEvents()
        {
            GameEvents.OnGameLoaded += HandleGameLoaded;
            GameEvents.OnStatsRecalculated += HandleStatsChanged;
            GameEvents.OnHealthChanged += HandleHealthChanged;
            GameEvents.OnLevelUp += HandleLevelUp;
            GameEvents.OnPlayerNameChanged += HandlePlayerNameChanged;
            
            Debug.Log("[PlayerHUD] Subscribed to GameEvents");
        }

        /// <summary>
        /// ✅ Static events are always safe to unsubscribe from
        /// No null checks needed, no memory leak risk
        /// </summary>
        private void UnsubscribeFromEvents()
        {
            GameEvents.OnGameLoaded -= HandleGameLoaded;
            GameEvents.OnStatsRecalculated -= HandleStatsChanged;
            GameEvents.OnHealthChanged -= HandleHealthChanged;
            GameEvents.OnLevelUp -= HandleLevelUp;
            GameEvents.OnPlayerNameChanged -= HandlePlayerNameChanged;
            
            Debug.Log("[PlayerHUD] Unsubscribed from GameEvents");
        }

        // ════════════════════════════════════════════════════════════════
        // Event Handlers
        // ════════════════════════════════════════════════════════════════
        
        /// <summary>
        /// ✅ CRITICAL: This is the ONLY safe place to do initial HUD refresh
        /// OnGameLoaded fires AFTER:
        /// 1. ServiceContainer.Start() completes
        /// 2. All managers are initialized
        /// 3. Character data is loaded
        /// 4. Bootstrap completes scene loading
        /// </summary>
        private void HandleGameLoaded(CharacterStats stats)
        {
            Debug.Log("[PlayerHUD] Game loaded, performing initial HUD refresh");
            RefreshHUD();
        }
        
        private void HandleStatsChanged(CharacterStats stats)
        {
            RefreshHUD();
        }
        
        private void HandleHealthChanged(float current, float max)
        {
            // Quick update for health changes (don't need full refresh)
            if (CharacterManager.Instance != null && CharacterManager.Instance.HasActivePlayer)
            {
                var player = CharacterManager.Instance.CurrentPlayer;
                UpdateHealthBar(player);
            }
        }
        
        private void HandleLevelUp(int level)
        {
            Debug.Log($"[PlayerHUD] Level up detected: {level}");
            RefreshHUD();
        }
        
        private void HandlePlayerNameChanged(string newName)
        {
            if (playerNameText != null)
                playerNameText.text = newName;
        }
        #endregion

        #region UI Updates
        private void UpdatePlayerInfo(CharacterStats player)
        {
            if (playerNameText != null)
                playerNameText.text = player.playerName;

            if (levelText != null)
                levelText.text = player.IsTranscended ? $"Lv.{player.Level} ★" : $"Lv.{player.Level}";
        }

        private void UpdateHealthBar(CharacterStats player)
        {
            if (player.MaxHP <= 0) return;

            targetHealthFill = player.CurrentHP / player.MaxHP;

            if (!smoothBars && healthFill != null)
                healthFill.fillAmount = targetHealthFill;

            if (healthText != null)
                healthText.text = $"{Mathf.CeilToInt(player.CurrentHP)}/{Mathf.CeilToInt(player.MaxHP)}";
        }

        private void UpdateExpBar(CharacterStats player)
        {
            if (player.levelSystem.expToNextLevel <= 0) return;

            targetExpFill = (float)player.CurrentEXP / player.levelSystem.expToNextLevel;

            if (!smoothBars && expFill != null)
                expFill.fillAmount = targetExpFill;

            if (expText != null)
                expText.text = $"{player.CurrentEXP}/{player.levelSystem.expToNextLevel}";
        }

        private void AnimateBars()
        {
            if (healthFill != null)
                healthFill.fillAmount = Mathf.Lerp(healthFill.fillAmount, targetHealthFill, barLerpSpeed * Time.deltaTime);

            if (expFill != null)
                expFill.fillAmount = Mathf.Lerp(expFill.fillAmount, targetExpFill, barLerpSpeed * Time.deltaTime);
        }
        #endregion
    }
}