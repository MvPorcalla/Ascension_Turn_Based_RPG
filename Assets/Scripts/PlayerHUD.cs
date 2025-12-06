// ════════════════════════════════════════════
// PlayerHUD.cs
// Player HUD display - subscribes to CharacterManager events
// ════════════════════════════════════════════

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Ascension.Managers;

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
            InitialRefresh();
        }

        private void OnDestroy()
        {
            UnsubscribeFromEvents();
        }

        private void Update()
        {
            if (smoothBars)
            {
                SmoothBarAnimations();
            }
        }
        #endregion

        #region Public Methods
        public void RefreshHUD()
        {
            if (!CharacterManager.Instance.HasActivePlayer)
                return;

            PlayerStats player = CharacterManager.Instance.CurrentPlayer;

            UpdatePlayerInfo(player);
            UpdateHealthBar(player);
            UpdateExpBar(player);
        }
        #endregion

        #region Private Methods - Events
        private void SubscribeToEvents()
        {
            if (CharacterManager.Instance == null)
            {
                Debug.LogWarning("[PlayerHUD] CharacterManager not found!");
                return;
            }

            CharacterManager.Instance.OnPlayerLoaded += OnPlayerLoaded;
            CharacterManager.Instance.OnPlayerStatsChanged += OnPlayerStatsChanged;
            CharacterManager.Instance.OnHealthChanged += OnHealthChanged;
            CharacterManager.Instance.OnLevelUp += OnLevelUp;
        }

        private void UnsubscribeFromEvents()
        {
            if (CharacterManager.Instance == null) return;

            CharacterManager.Instance.OnPlayerLoaded -= OnPlayerLoaded;
            CharacterManager.Instance.OnPlayerStatsChanged -= OnPlayerStatsChanged;
            CharacterManager.Instance.OnHealthChanged -= OnHealthChanged;
            CharacterManager.Instance.OnLevelUp -= OnLevelUp;
        }

        private void InitialRefresh()
        {
            if (CharacterManager.Instance?.HasActivePlayer == true)
            {
                RefreshHUD();
            }
        }

        private void OnPlayerLoaded(PlayerStats stats) => RefreshHUD();
        private void OnPlayerStatsChanged(PlayerStats stats) => RefreshHUD();
        private void OnHealthChanged(float current, float max) => UpdateHealthBar(CharacterManager.Instance.CurrentPlayer);
        
        private void OnLevelUp(int newLevel)
        {
            RefreshHUD();
            Debug.Log($"[PlayerHUD] Level up animation: {newLevel}");
        }
        #endregion

        #region Private Methods - UI Updates
        private void UpdatePlayerInfo(PlayerStats player)
        {
            if (playerNameText != null)
                playerNameText.text = player.playerName;

            if (levelText != null)
            {
                levelText.text = player.IsTranscended 
                    ? $"Lv.{player.Level} ★" 
                    : $"Lv.{player.Level}";
            }
        }

        private void UpdateHealthBar(PlayerStats player)
        {
            if (player.MaxHP <= 0) return;

            float ratio = player.CurrentHP / player.MaxHP;
            targetHealthFill = ratio;

            if (!smoothBars && healthFill != null)
                healthFill.fillAmount = ratio;

            if (healthText != null)
                healthText.text = $"{Mathf.CeilToInt(player.CurrentHP)}/{Mathf.CeilToInt(player.MaxHP)}";
        }

        private void UpdateExpBar(PlayerStats player)
        {
            if (player.levelSystem.expToNextLevel <= 0) return;

            float ratio = (float)player.CurrentEXP / player.levelSystem.expToNextLevel;
            targetExpFill = ratio;

            if (!smoothBars && expFill != null)
                expFill.fillAmount = ratio;

            if (expText != null)
                expText.text = $"{player.CurrentEXP}/{player.levelSystem.expToNextLevel}";
        }

        private void SmoothBarAnimations()
        {
            if (healthFill != null)
                healthFill.fillAmount = Mathf.Lerp(healthFill.fillAmount, targetHealthFill, barLerpSpeed * Time.deltaTime);

            if (expFill != null)
                expFill.fillAmount = Mathf.Lerp(expFill.fillAmount, targetExpFill, barLerpSpeed * Time.deltaTime);
        }
        #endregion
    }
}