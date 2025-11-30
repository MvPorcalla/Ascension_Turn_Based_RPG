// -------------------------------
// PlayerStatsPreviewUI.cs - Reusable Player Stats Display
// -------------------------------
using UnityEngine;
using TMPro;

/// <summary>
/// Reusable component for displaying player combat stats.
/// Automatically finds UI elements based on standardized prefab structure.
/// </summary>
public class PlayerStatsPreviewUI : MonoBehaviour
{
    [Header("Auto-Find Elements")]
    [Tooltip("If true, automatically finds UI elements on Start")]
    [SerializeField] private bool autoFindElements = true;

    [Header("Offensive Stats (Auto-Assigned)")]
    [SerializeField] private TMP_Text adValue;
    [SerializeField] private TMP_Text apValue;
    [SerializeField] private TMP_Text critDamageValue;
    [SerializeField] private TMP_Text critRateValue;
    [SerializeField] private TMP_Text lethalityValue;
    [SerializeField] private TMP_Text penetrationValue;

    [Header("Defensive Stats (Auto-Assigned)")]
    [SerializeField] private TMP_Text hpValue;
    [SerializeField] private TMP_Text lifestealValue;
    [SerializeField] private TMP_Text attackSpeedValue; // If you add it later
    [SerializeField] private TMP_Text defenseValue;
    [SerializeField] private TMP_Text evasionValue;
    [SerializeField] private TMP_Text tenacityValue;

    private void Start()
    {
        if (autoFindElements)
        {
            FindUIElements();
        }
    }

    /// <summary>
    /// Automatically find all UI elements based on hierarchy path
    /// </summary>
    [ContextMenu("Find UI Elements")]
    public void FindUIElements()
    {
        // Offensive Stats
        adValue = FindTextByPath("OffensiveStats/Base_AD/AD_value");
        apValue = FindTextByPath("OffensiveStats/Base_AP/AP_value");
        critDamageValue = FindTextByPath("OffensiveStats/Base_CritDamage/CritDamage_value");
        critRateValue = FindTextByPath("OffensiveStats/Base_CritRate/CritRate_value");
        lethalityValue = FindTextByPath("OffensiveStats/Base_Lethality/Lethality_value");
        penetrationValue = FindTextByPath("OffensiveStats/Base_Penetration/Penetration_value");

        // Defensive Stats
        hpValue = FindTextByPath("DefensiveStats/Base_HP/HP_value");
        lifestealValue = FindTextByPath("DefensiveStats/Base_Lifesteal/Lifesteal_value");
        defenseValue = FindTextByPath("DefensiveStats/Base_Defense/Defense_value");
        evasionValue = FindTextByPath("DefensiveStats/Base_Evasion/Evasion_value");
        tenacityValue = FindTextByPath("DefensiveStats/Base_Tenacity/Tenacity_value");

        Debug.Log("[PlayerStatsPreviewUI] UI elements found!");
    }

    /// <summary>
    /// Update display with current player stats
    /// </summary>
    public void UpdateStats(PlayerStats stats)
    {
        if (stats == null)
        {
            Debug.LogWarning("[PlayerStatsPreviewUI] PlayerStats is null!");
            return;
        }

        // Offensive Stats
        SetText(adValue, stats.derivedStats.AD, "F1");
        SetText(apValue, stats.derivedStats.AP, "F1");
        SetText(critDamageValue, stats.derivedStats.CritDamage, "F1", "%");
        SetText(critRateValue, stats.derivedStats.CritRate, "F1", "%");
        SetText(lethalityValue, stats.derivedStats.Lethality, "F0");
        SetText(penetrationValue, stats.derivedStats.Penetration, "F1", "%");

        // Defensive Stats
        SetText(hpValue, stats.derivedStats.MaxHP, "F0");
        SetText(lifestealValue, stats.derivedStats.Lifesteal, "F1", "%");
        SetText(defenseValue, stats.derivedStats.Defense, "F1");
        SetText(evasionValue, stats.derivedStats.Evasion, "F1", "%");
        SetText(tenacityValue, stats.derivedStats.Tenacity, "F1", "%");

        // Optional: Attack Speed
        if (attackSpeedValue != null)
            SetText(attackSpeedValue, stats.derivedStats.AttackSpeed, "F1");
    }

    /// <summary>
    /// Update display with GameManager's current player
    /// </summary>
    public void UpdateStats()
    {
        if (GameManager.Instance != null && GameManager.Instance.CurrentPlayer != null)
        {
            UpdateStats(GameManager.Instance.CurrentPlayer);
        }
        else
        {
            Debug.LogWarning("[PlayerStatsPreviewUI] No active player found!");
            ClearStats();
        }
    }

    /// <summary>
    /// Clear all stat displays
    /// </summary>
    public void ClearStats()
    {
        SetText(adValue, "0");
        SetText(apValue, "0");
        SetText(critDamageValue, "0%");
        SetText(critRateValue, "0%");
        SetText(lethalityValue, "0");
        SetText(penetrationValue, "0%");
        SetText(hpValue, "0");
        SetText(lifestealValue, "0%");
        SetText(defenseValue, "0");
        SetText(evasionValue, "0%");
        SetText(tenacityValue, "0%");
    }

    #region Helper Methods

    /// <summary>
    /// Find TMP_Text component by hierarchy path
    /// </summary>
    private TMP_Text FindTextByPath(string path)
    {
        Transform found = transform.Find(path);
        if (found != null)
        {
            return found.GetComponent<TMP_Text>();
        }
        
        Debug.LogWarning($"[PlayerStatsPreviewUI] Could not find: {path}");
        return null;
    }

    /// <summary>
    /// Safely set text with formatting
    /// </summary>
    private void SetText(TMP_Text textComponent, float value, string format = "F0", string suffix = "")
    {
        if (textComponent != null)
        {
            textComponent.text = value.ToString(format) + suffix;
        }
    }

    /// <summary>
    /// Safely set text directly
    /// </summary>
    private void SetText(TMP_Text textComponent, string value)
    {
        if (textComponent != null)
        {
            textComponent.text = value;
        }
    }

    #endregion

    #region Editor Helpers

#if UNITY_EDITOR
    [ContextMenu("Test: Update with Current Player")]
    private void TestUpdateStats()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning("Test only works in Play Mode!");
            return;
        }

        UpdateStats();
    }

    [ContextMenu("Test: Fill with Sample Data")]
    private void TestFillSampleData()
    {
        SetText(adValue, "250.5");
        SetText(apValue, "180.2");
        SetText(critDamageValue, "200%");
        SetText(critRateValue, "45.5%");
        SetText(lethalityValue, "25");
        SetText(penetrationValue, "30%");
        SetText(hpValue, "2500");
        SetText(lifestealValue, "15%");
        SetText(defenseValue, "120.5");
        SetText(evasionValue, "25.5%");
        SetText(tenacityValue, "40%");

        Debug.Log("[PlayerStatsPreviewUI] Sample data filled!");
    }
#endif

    #endregion
}