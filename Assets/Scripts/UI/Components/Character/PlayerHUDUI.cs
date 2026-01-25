// ════════════════════════════════════════════════════════════════════════
// Assets/Scripts/UI/Components/Character/PlayerHUDUI.cs
// ✅ FIXED: Changed CurrentEXP → CurrentExp (correct casing)
// ✅ RENAMED: PlayerHUD → PlayerHUDUI
// ════════════════════════════════════════════════════════════════════════

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Ascension.Core;
using Ascension.Character.Core;
using Ascension.Character.Manager;

namespace Ascension.UI.Components.Character
{
    public class PlayerHUDUI : MonoBehaviour
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
            RefreshHUD(); // ✅ Immediate refresh on enable
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
            var characterManager = GameBootstrap.Character;
            
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
        private void SubscribeToEvents()
        {
            GameEvents.OnGameLoaded += HandleGameLoaded;
            GameEvents.OnStatsRecalculated += HandleStatsChanged;
            GameEvents.OnHealthChanged += HandleHealthChanged;
            GameEvents.OnLevelUp += HandleLevelUp;
            GameEvents.OnPlayerNameChanged += HandlePlayerNameChanged;
            GameEvents.OnExperienceGained += HandleExperienceGained;
            
            Debug.Log("[PlayerHUDUI] Subscribed to GameEvents");
        }

        private void UnsubscribeFromEvents()
        {
            GameEvents.OnGameLoaded -= HandleGameLoaded;
            GameEvents.OnStatsRecalculated -= HandleStatsChanged;
            GameEvents.OnHealthChanged -= HandleHealthChanged;
            GameEvents.OnLevelUp -= HandleLevelUp;
            GameEvents.OnPlayerNameChanged -= HandlePlayerNameChanged;
            GameEvents.OnExperienceGained -= HandleExperienceGained;
            
            Debug.Log("[PlayerHUDUI] Unsubscribed from GameEvents");
        }

        // ════════════════════════════════════════════════════════════════
        // Event Handlers
        // ════════════════════════════════════════════════════════════════
        
        private void HandleGameLoaded(CharacterStats stats)
        {
            Debug.Log("[PlayerHUDUI] Game loaded, performing initial HUD refresh");
            RefreshHUD();
        }
        
        private void HandleStatsChanged(CharacterStats stats)
        {
            RefreshHUD();
        }
        
        private void HandleHealthChanged(float current, float max)
        {
            var characterManager = GameBootstrap.Character;
            if (characterManager != null && characterManager.HasActivePlayer)
            {
                var player = characterManager.CurrentPlayer;
                UpdateHealthBar(player);
            }
        }
        
        private void HandleLevelUp(int level)
        {
            Debug.Log($"[PlayerHUDUI] Level up detected: {level}");
            RefreshHUD();
        }
        
        private void HandlePlayerNameChanged(string newName)
        {
            if (playerNameText != null)
                playerNameText.text = newName;
        }
        
        /// <summary>
        /// ✅ NEW: Handle experience gained event (updates EXP bar only)
        /// </summary>
        private void HandleExperienceGained(int gained, int newTotal)
        {
            var characterManager = GameBootstrap.Character;
            if (characterManager != null && characterManager.HasActivePlayer)
            {
                var player = characterManager.CurrentPlayer;
                UpdateExpBar(player);
            }
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

            // ✅ FIXED: Changed CurrentEXP → CurrentExp (correct property name)
            targetExpFill = (float)player.CurrentExp / player.levelSystem.expToNextLevel;

            if (!smoothBars && expFill != null)
                expFill.fillAmount = targetExpFill;

            if (expText != null)
            {
                // ✅ FIXED: Changed CurrentEXP → CurrentExp
                expText.text = $"{player.CurrentExp}/{player.levelSystem.expToNextLevel}";
            }
        }

        private void AnimateBars()
        {
            if (healthFill != null)
                healthFill.fillAmount = Mathf.Lerp(healthFill.fillAmount, targetHealthFill, barLerpSpeed * Time.deltaTime);

            if (expFill != null)
                expFill.fillAmount = Mathf.Lerp(expFill.fillAmount, targetExpFill, barLerpSpeed * Time.deltaTime);
        }
        #endregion

        #region Editor Helpers
#if UNITY_EDITOR
        [ContextMenu("Force Refresh HUD")]
        private void EditorForceRefresh()
        {
            if (Application.isPlaying)
            {
                RefreshHUD();
            }
            else
            {
                Debug.LogWarning("[PlayerHUDUI] Force Refresh only works in Play Mode");
            }
        }
#endif
        #endregion
    }
}