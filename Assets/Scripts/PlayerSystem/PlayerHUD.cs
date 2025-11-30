// -------------------------------
// PlayerHUD.cs
// -------------------------------

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
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnPlayerLoaded += OnPlayerLoaded;
            GameManager.Instance.OnNewGameStarted += RefreshHUD;

            if (GameManager.Instance.HasActivePlayer)
                RefreshHUD();
        }
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnPlayerLoaded -= OnPlayerLoaded;
            GameManager.Instance.OnNewGameStarted -= RefreshHUD;
        }
    }

    private void Update()
    {
        if (smoothBars)
        {
            healthFill.fillAmount = Mathf.Lerp(healthFill.fillAmount, targetHealthFill, barLerpSpeed * Time.deltaTime);
            expFill.fillAmount = Mathf.Lerp(expFill.fillAmount, targetExpFill, barLerpSpeed * Time.deltaTime);
        }
    }

    private void OnPlayerLoaded(PlayerStats stats)
    {
        RefreshHUD();
    }

    #region Public API

    public void RefreshHUD()
    {
        if (!GameManager.Instance.HasActivePlayer)
            return;

        PlayerStats player = GameManager.Instance.CurrentPlayer;

        UpdatePlayerInfo(player);
        UpdateHealthBar(player);
        UpdateExpBar(player);
    }

    public void TakeDamage(float damage)
    {
        if (!GameManager.Instance.HasActivePlayer)
            return;

        PlayerStats player = GameManager.Instance.CurrentPlayer;
        player.combatRuntime.TakeDamage(damage, player.MaxHP);
        UpdateHealthBar(player);
    }

    public void Heal(float amount)
    {
        if (!GameManager.Instance.HasActivePlayer)
            return;

        PlayerStats player = GameManager.Instance.CurrentPlayer;
        player.combatRuntime.Heal(amount, player.MaxHP);
        UpdateHealthBar(player);
    }

    public void FullHeal()
    {
        if (!GameManager.Instance.HasActivePlayer)
            return;

        PlayerStats player = GameManager.Instance.CurrentPlayer;
        player.combatRuntime.currentHP = player.MaxHP;
        UpdateHealthBar(player);
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