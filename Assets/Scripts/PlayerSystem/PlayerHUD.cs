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
        player.currentHP = Mathf.Max(0, player.currentHP - damage);
        UpdateHealthBar(player);
    }

    public void Heal(float amount)
    {
        if (!GameManager.Instance.HasActivePlayer)
            return;

        PlayerStats player = GameManager.Instance.CurrentPlayer;
        player.currentHP = Mathf.Min(player.HP, player.currentHP + amount);
        UpdateHealthBar(player);
    }

    public void FullHeal()
    {
        if (!GameManager.Instance.HasActivePlayer)
            return;

        PlayerStats player = GameManager.Instance.CurrentPlayer;
        player.currentHP = player.HP;
        UpdateHealthBar(player);
    }

    #endregion

    #region Private Updates

    private void UpdatePlayerInfo(PlayerStats player)
    {
        if (playerNameText != null)
            playerNameText.text = player.playerName;

        if (levelText != null)
            levelText.text = $"Lv.{player.level}";
    }

    private void UpdateHealthBar(PlayerStats player)
    {
        if (player.HP <= 0) return;

        float ratio = player.currentHP / player.HP;
        targetHealthFill = ratio;

        if (!smoothBars)
            healthFill.fillAmount = ratio;

        if (healthText != null)
            healthText.text = $"{Mathf.CeilToInt(player.currentHP)}/{Mathf.CeilToInt(player.HP)}";
    }

    private void UpdateExpBar(PlayerStats player)
    {
        if (player.expToNextLevel <= 0) return;

        float ratio = (float)player.currentEXP / player.expToNextLevel;
        targetExpFill = ratio;

        if (!smoothBars)
            expFill.fillAmount = ratio;

        if (expText != null)
            expText.text = $"{player.currentEXP}/{player.expToNextLevel}";
    }

    #endregion
}