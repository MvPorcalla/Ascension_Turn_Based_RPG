// ──────────────────────────────────────────────────
// PlayerPreviewUI.cs
// Displays player derived stats in the UI preview panel
// ──────────────────────────────────────────────────

using UnityEngine;
using TMPro;

public class PlayerPreviewUI : MonoBehaviour
{
    [Header("Offensive Stats")]
    [SerializeField] private TMP_Text adValueText;
    [SerializeField] private TMP_Text apValueText;
    [SerializeField] private TMP_Text critDamageValueText;
    [SerializeField] private TMP_Text critRateValueText;
    [SerializeField] private TMP_Text lethalityValueText;
    [SerializeField] private TMP_Text penetrationValueText;
    
    [Header("Defensive Stats")]
    [SerializeField] private TMP_Text hpValueText;
    [SerializeField] private TMP_Text lifestealValueText;
    [SerializeField] private TMP_Text attackSpeedValueText;
    [SerializeField] private TMP_Text defenseValueText;
    [SerializeField] private TMP_Text evasionValueText;
    [SerializeField] private TMP_Text tenacityValueText;
    
    [Header("Debug")]
    [SerializeField] private bool debugMode = false;
    
    private void Awake()
    {
        if (debugMode)
            Debug.Log("[PlayerPreviewUI] Awake called - UI ready");
    }
    
    /// <summary>
    /// Update the preview with derived stats
    /// </summary>
    public void UpdateStats(PlayerDerivedStats stats)
    {
        if (stats == null)
        {
            Debug.LogWarning("[PlayerPreviewUI] Attempted to update with null stats");
            return;
        }
        
        // Offensive stats
        SetText(adValueText, stats.AD.ToString("F1"));
        SetText(apValueText, stats.AP.ToString("F1"));
        SetText(critDamageValueText, stats.CritDamage.ToString("F1") + "%");
        SetText(critRateValueText, stats.CritRate.ToString("F1") + "%");
        SetText(lethalityValueText, stats.Lethality.ToString("F0"));
        SetText(penetrationValueText, stats.Penetration.ToString("F1") + "%");
        
        // Defensive stats
        SetText(hpValueText, stats.MaxHP.ToString("F0"));
        SetText(lifestealValueText, stats.Lifesteal.ToString("F1") + "%");
        SetText(attackSpeedValueText, stats.AttackSpeed.ToString("F2"));
        SetText(defenseValueText, stats.Defense.ToString("F1"));
        SetText(evasionValueText, stats.Evasion.ToString("F1") + "%");
        SetText(tenacityValueText, stats.Tenacity.ToString("F1") + "%");
    }
    
    /// <summary>
    /// Preview stats using temporary attributes (for allocation screen)
    /// </summary>
    public void PreviewStats(CharacterBaseStatsSO baseStats, int level, PlayerAttributes attributes, PlayerItemStats itemStats, WeaponSO weapon = null)
    {
        PlayerDerivedStats previewStats = new PlayerDerivedStats();
        previewStats.Recalculate(baseStats, level, attributes, itemStats, weapon);
        UpdateStats(previewStats);
    }
    
    /// <summary>
    /// Clear all stat displays
    /// </summary>
    public void Clear()
    {
        SetText(adValueText, "--");
        SetText(apValueText, "--");
        SetText(critDamageValueText, "--%");
        SetText(critRateValueText, "--%");
        SetText(lethalityValueText, "--");
        SetText(penetrationValueText, "--%");
        SetText(hpValueText, "--");
        SetText(lifestealValueText, "--%");
        SetText(attackSpeedValueText, "--");
        SetText(defenseValueText, "--");
        SetText(evasionValueText, "--%");
        SetText(tenacityValueText, "--%");
    }
    
    private void SetText(TMP_Text textComponent, string value)
    {
        if (textComponent != null)
            textComponent.text = value;
    }
}