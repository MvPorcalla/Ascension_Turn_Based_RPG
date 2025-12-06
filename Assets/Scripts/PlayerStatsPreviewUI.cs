// ════════════════════════════════════════════
// PlayerPreviewUI.cs
// Displays player derived stats in UI preview panel
// ════════════════════════════════════════════

using UnityEngine;
using TMPro;
using Ascension.Data.SO;
using Ascension.Systems;

namespace Ascension.UI
{
    public class PlayerPreviewUI : MonoBehaviour
    {
        #region Serialized Fields
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
        #endregion
        
        #region Unity Callbacks
        private void Awake()
        {
            ValidateReferences();
            
            if (debugMode)
                Debug.Log("[PlayerPreviewUI] Initialized");
        }
        #endregion
        
        #region Public Methods
        /// <summary>
        /// Update preview with current derived stats
        /// </summary>
        public void UpdateStats(PlayerDerivedStats stats)
        {
            if (stats == null)
            {
                Debug.LogWarning("[PlayerPreviewUI] Cannot update with null stats");
                return;
            }
            
            UpdateOffensiveStats(stats);
            UpdateDefensiveStats(stats);
        }
        
        /// <summary>
        /// Preview stats using temporary attributes (for allocation screen)
        /// </summary>
        public void PreviewStats(
            CharacterBaseStatsSO baseStats, 
            int level, 
            PlayerAttributes attributes, 
            PlayerItemStats itemStats, 
            WeaponSO weapon = null)
        {
            PlayerDerivedStats previewStats = new PlayerDerivedStats();
            previewStats.Recalculate(baseStats, level, attributes, itemStats, weapon);
            
            UpdateStats(previewStats);
        }
        
        /// <summary>
        /// Clear all stat displays
        /// </summary>
        public void ClearDisplay()
        {
            const string EMPTY_VALUE = "--";
            const string EMPTY_PERCENT = "--%";
            
            // Offensive
            SetStatText(adValueText, EMPTY_VALUE);
            SetStatText(apValueText, EMPTY_VALUE);
            SetStatText(critDamageValueText, EMPTY_PERCENT);
            SetStatText(critRateValueText, EMPTY_PERCENT);
            SetStatText(lethalityValueText, EMPTY_VALUE);
            SetStatText(penetrationValueText, EMPTY_PERCENT);
            
            // Defensive
            SetStatText(hpValueText, EMPTY_VALUE);
            SetStatText(lifestealValueText, EMPTY_PERCENT);
            SetStatText(attackSpeedValueText, EMPTY_VALUE);
            SetStatText(defenseValueText, EMPTY_VALUE);
            SetStatText(evasionValueText, EMPTY_PERCENT);
            SetStatText(tenacityValueText, EMPTY_PERCENT);
        }
        #endregion
        
        #region Private Methods
        private void UpdateOffensiveStats(PlayerDerivedStats stats)
        {
            SetStatText(adValueText, stats.AD.ToString("F1"));
            SetStatText(apValueText, stats.AP.ToString("F1"));
            SetStatText(critDamageValueText, FormatPercent(stats.CritDamage));
            SetStatText(critRateValueText, FormatPercent(stats.CritRate));
            SetStatText(lethalityValueText, stats.Lethality.ToString("F0"));
            SetStatText(penetrationValueText, FormatPercent(stats.Penetration));
        }
        
        private void UpdateDefensiveStats(PlayerDerivedStats stats)
        {
            SetStatText(hpValueText, stats.MaxHP.ToString("F0"));
            SetStatText(lifestealValueText, FormatPercent(stats.Lifesteal));
            SetStatText(attackSpeedValueText, stats.AttackSpeed.ToString("F2"));
            SetStatText(defenseValueText, stats.Defense.ToString("F1"));
            SetStatText(evasionValueText, FormatPercent(stats.Evasion));
            SetStatText(tenacityValueText, FormatPercent(stats.Tenacity));
        }
        
        private void SetStatText(TMP_Text textComponent, string value)
        {
            if (textComponent != null)
                textComponent.text = value;
        }
        
        private string FormatPercent(float value)
        {
            return value.ToString("F1") + "%";
        }
        
        private void ValidateReferences()
        {
            if (adValueText == null)
                Debug.LogWarning("[PlayerPreviewUI] AD text not assigned!", this);
            if (apValueText == null)
                Debug.LogWarning("[PlayerPreviewUI] AP text not assigned!", this);
            if (hpValueText == null)
                Debug.LogWarning("[PlayerPreviewUI] HP text not assigned!", this);
        }
        #endregion
    }
}