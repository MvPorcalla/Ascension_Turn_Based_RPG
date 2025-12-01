// ──────────────────────────────────────────────────
// PlayerHUD.cs
// Manages the player's HUD display (health, EXP, level, etc.)
// Now subscribes to CharacterManager events
// ──────────────────────────────────────────────────

using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerHUD : MonoBehaviour
{
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

    // Target values for smooth lerping
    private float targetHealthFill;
    private float targetExpFill;

    private void Start()
    {
        // Subscribe to CharacterManager events
        if (CharacterManager.Instance != null)
        {
            CharacterManager.Instance.OnPlayerLoaded += OnPlayerLoaded;
            CharacterManager.Instance.OnPlayerStatsChanged += OnPlayerStatsChanged;
            CharacterManager.Instance.OnHealthChanged += OnHealthChanged;
            CharacterManager.Instance.OnLevelUp += OnLevelUp;

            // Initial refresh if player already loaded
            if (CharacterManager.Instance.HasActivePlayer)
            {
                RefreshHUD();
            }
        }
        else
        {
            Debug.LogWarning("[PlayerHUD] CharacterManager not found!");
        }
    }

    private void OnDestroy()
    {
        // Unsubscribe from events
        if (CharacterManager.Instance != null)
        {
            CharacterManager.Instance.OnPlayerLoaded -= OnPlayerLoaded;
            CharacterManager.Instance.OnPlayerStatsChanged -= OnPlayerStatsChanged;
            CharacterManager.Instance.OnHealthChanged -= OnHealthChanged;
            CharacterManager.Instance.OnLevelUp -= OnLevelUp;
        }
    }

    private void Update()
    {
        // Smooth bar animations
        if (smoothBars)
        {
            healthFill.fillAmount = Mathf.Lerp(healthFill.fillAmount, targetHealthFill, barLerpSpeed * Time.deltaTime);
            expFill.fillAmount = Mathf.Lerp(expFill.fillAmount, targetExpFill, barLerpSpeed * Time.deltaTime);
        }
    }

    #region Event Handlers

    private void OnPlayerLoaded(PlayerStats stats)
    {
        RefreshHUD();
    }

    private void OnPlayerStatsChanged(PlayerStats stats)
    {
        RefreshHUD();
    }

    private void OnHealthChanged(float current, float max)
    {
        UpdateHealthBar(CharacterManager.Instance.CurrentPlayer);
    }

    private void OnLevelUp(int newLevel)
    {
        RefreshHUD();
        // TODO: Add level up visual effect/sound
        Debug.Log($"[PlayerHUD] Level up animation: {newLevel}");
    }

    #endregion

    #region Public API

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

    #region Private Updates

    private void UpdatePlayerInfo(PlayerStats player)
    {
        if (playerNameText != null)
            playerNameText.text = player.playerName;

        if (levelText != null)
        {
            if (player.IsTranscended)
                levelText.text = $"Lv.{player.Level} ★";
            else
                levelText.text = $"Lv.{player.Level}";
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

    #endregion
}