// ════════════════════════════════════════════
// Assets\Scripts\Modules\CharacterSystem\UI\PlayerHUD.cs
// Displays player info, health, and EXP; subscribes to CharacterManager events
// ════════════════════════════════════════════

using UnityEngine;
using UnityEngine.UI;
using TMPro;
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
        private void Start()
        {
            SubscribeToEvents();
            RefreshHUDSafe();
        }

        private void OnDestroy()
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
            if (!CharacterManager.Instance.HasActivePlayer) return;

            var player = CharacterManager.Instance.CurrentPlayer;
            UpdatePlayerInfo(player);
            UpdateHealthBar(player);
            UpdateExpBar(player);
        }
        #endregion

        #region Event Subscriptions
        private void SubscribeToEvents()
        {
            var cm = CharacterManager.Instance;
            if (cm == null)
            {
                Debug.LogWarning("[PlayerHUD] CharacterManager not found!");
                return;
            }

            cm.OnPlayerLoaded += HandlePlayerLoaded;              // ✅ Named method
            cm.OnCharacterStatsChanged += HandleStatsChanged;    // ✅ Named method
            cm.OnHealthChanged += HandleHealthChanged;           // ✅ Named method
            cm.OnLevelUp += HandleLevelUp;                       // ✅ Named method
        }

        private void UnsubscribeFromEvents()
        {
            var cm = CharacterManager.Instance;
            if (cm == null) return;  // ✅ Guard against null

            cm.OnPlayerLoaded -= HandlePlayerLoaded;
            cm.OnCharacterStatsChanged -= HandleStatsChanged;
            cm.OnHealthChanged -= HandleHealthChanged;
            cm.OnLevelUp -= HandleLevelUp;
        }

        // Event Handlers
        private void HandlePlayerLoaded(CharacterStats stats) => RefreshHUD();
        private void HandleStatsChanged(CharacterStats stats) => RefreshHUD();
        private void HandleHealthChanged(float current, float max) => RefreshHUDSafe();
        private void HandleLevelUp(int level) => RefreshHUDSafe();

        private void RefreshHUDSafe()
        {
            if (CharacterManager.Instance?.HasActivePlayer == true)
                RefreshHUD();
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
