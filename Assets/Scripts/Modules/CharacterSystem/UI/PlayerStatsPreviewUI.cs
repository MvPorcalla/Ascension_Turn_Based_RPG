// ════════════════════════════════════════════
// Assets\Scripts\Modules\CharacterSystem\UI\PlayerStatsPreviewUI.cs
// Displays player derived stats in the preview panel
// ════════════════════════════════════════════

using UnityEngine;
using TMPro;
using Ascension.GameSystem;
using Ascension.Data.SO.Item;
using Ascension.Character.Stat;
using Ascension.Data.SO.Character;

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
        public void UpdateStats(CharacterDerivedStats stats)
        {
            if (stats == null)
            {
                Debug.LogWarning("[PlayerPreviewUI] Cannot update with null stats");
                return;
            }

            UpdateOffensive(stats);
            UpdateDefensive(stats);
        }

        public void PreviewStats(
            CharacterBaseStatsSO baseStats,
            int level,
            CharacterAttributes attributes,
            CharacterItemStats itemStats,
            WeaponSO weapon = null)
        {
            var preview = new CharacterDerivedStats();
            preview.Recalculate(baseStats, level, attributes, itemStats, weapon);
            UpdateStats(preview);
        }

        public void ClearDisplay()
        {
            const string EMPTY_VALUE = "--";
            const string EMPTY_PERCENT = "--%";

            // Offensive
            Set(adValueText, EMPTY_VALUE);
            Set(apValueText, EMPTY_VALUE);
            Set(critDamageValueText, EMPTY_PERCENT);
            Set(critRateValueText, EMPTY_PERCENT);
            Set(lethalityValueText, EMPTY_VALUE);
            Set(penetrationValueText, EMPTY_PERCENT);

            // Defensive
            Set(hpValueText, EMPTY_VALUE);
            Set(lifestealValueText, EMPTY_PERCENT);
            Set(attackSpeedValueText, EMPTY_VALUE);
            Set(defenseValueText, EMPTY_VALUE);
            Set(evasionValueText, EMPTY_PERCENT);
            Set(tenacityValueText, EMPTY_PERCENT);
        }
        #endregion

        #region Private Methods
        private void UpdateOffensive(CharacterDerivedStats stats)
        {
            Set(adValueText, stats.AD.ToString("F1"));
            Set(apValueText, stats.AP.ToString("F1"));
            Set(critDamageValueText, Percent(stats.CritDamage));
            Set(critRateValueText, Percent(stats.CritRate));
            Set(lethalityValueText, stats.Lethality.ToString("F0"));
            Set(penetrationValueText, Percent(stats.Penetration));
        }

        private void UpdateDefensive(CharacterDerivedStats stats)
        {
            Set(hpValueText, stats.MaxHP.ToString("F0"));
            Set(lifestealValueText, Percent(stats.Lifesteal));
            Set(attackSpeedValueText, stats.AttackSpeed.ToString("F2"));
            Set(defenseValueText, stats.Defense.ToString("F1"));
            Set(evasionValueText, Percent(stats.Evasion));
            Set(tenacityValueText, Percent(stats.Tenacity));
        }

        private void Set(TMP_Text text, string value)
        {
            if (text != null)
                text.text = value;
        }

        private string Percent(float value) => value.ToString("F1") + "%";

        private void ValidateReferences()
        {
            if (adValueText == null) Debug.LogWarning("[PlayerPreviewUI] AD text not assigned!", this);
            if (apValueText == null) Debug.LogWarning("[PlayerPreviewUI] AP text not assigned!", this);
            if (hpValueText == null) Debug.LogWarning("[PlayerPreviewUI] HP text not assigned!", this);
        }
        #endregion
    }
}
