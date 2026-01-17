// ════════════════════════════════════════════
// Assets\Scripts\CharacterSystem\UI\LevelUpManager.cs
// Manages the level-up UI and attribute allocation
// ════════════════════════════════════════════

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Ascension.Character.Stat;
using Ascension.Data.SO.Character;
using Ascension.Character.Manager;
using Ascension.App;

namespace Ascension.Character.UI
{
    public class LevelUpManager : MonoBehaviour
    {
        #region Serialized Fields
        [Header("Panel")]
        [SerializeField] private GameObject levelUpPanel;

        [Header("Level Info")]
        [SerializeField] private TMP_Text levelText;
        [SerializeField] private TMP_Text pointsRemainingText;
        [SerializeField] private TMP_Text transcendenceText;

        [Header("Attribute Buttons")]
        [SerializeField] private Button strPlusBtn;
        [SerializeField] private Button intPlusBtn;
        [SerializeField] private Button agiPlusBtn;
        [SerializeField] private Button endPlusBtn;
        [SerializeField] private Button wisPlusBtn;

        [Header("Attribute Display")]
        [SerializeField] private TMP_Text strText;
        [SerializeField] private TMP_Text intText;
        [SerializeField] private TMP_Text agiText;
        [SerializeField] private TMP_Text endText;
        [SerializeField] private TMP_Text wisText;

        [Header("Combat Stats Preview")]
        [SerializeField] private TMP_Text adText;
        [SerializeField] private TMP_Text apText;
        [SerializeField] private TMP_Text hpText;
        [SerializeField] private TMP_Text attackSpeedText;
        [SerializeField] private TMP_Text defenseText;
        [SerializeField] private TMP_Text critRateText;
        [SerializeField] private TMP_Text evasionText;

        [Header("UI Buttons")]
        [SerializeField] private Button confirmButton;
        [SerializeField] private Button closeButton;
        #endregion

        #region Private Fields
        private CharacterStats characterStats;
        private CharacterAttributes tempAttributes;
        private int tempPointsSpent;
        private readonly CharacterDerivedStats previewStats = new CharacterDerivedStats();

        private CharacterBaseStatsSO BaseStats => CharacterManager.Instance?.BaseStats;
        #endregion

        #region Unity Callbacks
        private void Start()
        {
            levelUpPanel?.SetActive(false);
            SetupButtons();
        }
        #endregion

        #region Public Methods
        public void OpenLevelUpPanel(CharacterStats stats)
        {
            if (stats.UnallocatedPoints <= 0)
            {
                Debug.Log("No points to allocate!");
                return;
            }

            characterStats = stats;
            tempAttributes = characterStats.attributes.Clone();
            tempPointsSpent = 0;

            levelUpPanel?.SetActive(true);
            UpdateUI();
        }

        public void GainExperience(int exp)
        {
            if (!CharacterManager.Instance.HasActivePlayer)
            {
                Debug.LogError("No active player!");
                return;
            }

            characterStats = CharacterManager.Instance.CurrentPlayer;
            int oldLevel = characterStats.Level;

            CharacterManager.Instance.AddExperience(exp);
            GameManager.Instance.SaveGame();

            if (characterStats.Level > oldLevel)
            {
                Debug.Log($"LEVEL UP! Now level {characterStats.Level}");
                OpenLevelUpPanel(characterStats);
            }
        }
        #endregion

        #region Private Methods
        private void SetupButtons()
        {
            strPlusBtn.onClick.AddListener(() => AddPoint("STR"));
            intPlusBtn.onClick.AddListener(() => AddPoint("INT"));
            agiPlusBtn.onClick.AddListener(() => AddPoint("AGI"));
            endPlusBtn.onClick.AddListener(() => AddPoint("END"));
            wisPlusBtn.onClick.AddListener(() => AddPoint("WIS"));

            confirmButton.onClick.AddListener(OnConfirmClicked);
            closeButton?.onClick.AddListener(OnCloseClicked);
        }

        private void AddPoint(string attribute)
        {
            if (tempPointsSpent >= characterStats.UnallocatedPoints) return;

            switch (attribute)
            {
                case "STR": tempAttributes.STR++; break;
                case "INT": tempAttributes.INT++; break;
                case "AGI": tempAttributes.AGI++; break;
                case "END": tempAttributes.END++; break;
                case "WIS": tempAttributes.WIS++; break;
            }

            tempPointsSpent++;
            UpdateUI();
        }

        private void UpdateUI()
        {
            UpdateLevelInfo();
            UpdatePointsRemaining();
            UpdateAttributeDisplays();
            UpdateButtonStates();
            PreviewCombatStats();
        }

        private void UpdateLevelInfo()
        {
            if (levelText != null)
                levelText.text = characterStats.IsTranscended
                    ? $"Level {characterStats.Level} (T{characterStats.TranscendenceLevel})"
                    : $"Level {characterStats.Level}";

            if (transcendenceText != null)
            {
                transcendenceText.gameObject.SetActive(characterStats.IsTranscended);
                if (characterStats.IsTranscended)
                    transcendenceText.text = $"<color=gold>Transcendence Level {characterStats.TranscendenceLevel}</color>";
            }
        }

        private void UpdatePointsRemaining()
        {
            if (pointsRemainingText == null) return;

            int remaining = characterStats.UnallocatedPoints - tempPointsSpent;
            pointsRemainingText.text = $"Points: {remaining}";
            pointsRemainingText.color = remaining > 0 ? Color.green : Color.red;
        }

        private void UpdateAttributeDisplays()
        {
            UpdateAttributeText(strText, tempAttributes.STR, characterStats.attributes.STR, "STR");
            UpdateAttributeText(intText, tempAttributes.INT, characterStats.attributes.INT, "INT");
            UpdateAttributeText(agiText, tempAttributes.AGI, characterStats.attributes.AGI, "AGI");
            UpdateAttributeText(endText, tempAttributes.END, characterStats.attributes.END, "END");
            UpdateAttributeText(wisText, tempAttributes.WIS, characterStats.attributes.WIS, "WIS");
        }

        private void UpdateAttributeText(TMP_Text text, int tempValue, int currentValue, string label)
        {
            if (text == null) return;
            string change = tempValue > currentValue ? $" <color=green>(+{tempValue - currentValue})</color>" : "";
            text.text = $"{label}: {tempValue}{change}";
        }

        private void UpdateButtonStates()
        {
            bool canAdd = tempPointsSpent < characterStats.UnallocatedPoints;
            strPlusBtn.interactable = canAdd;
            intPlusBtn.interactable = canAdd;
            agiPlusBtn.interactable = canAdd;
            endPlusBtn.interactable = canAdd;
            wisPlusBtn.interactable = canAdd;
        }

        private void PreviewCombatStats()
        {
            if (BaseStats == null) return;

            previewStats.Recalculate(BaseStats, characterStats.Level, tempAttributes,
                                    characterStats.itemStats, characterStats.equippedWeapon);

            SetText(adText, $"AD: {previewStats.AD:F1}");
            SetText(apText, $"AP: {previewStats.AP:F1}");
            SetText(hpText, $"HP: {previewStats.MaxHP:F0}");
            SetText(attackSpeedText, $"Speed: {previewStats.AttackSpeed:F1}");
            SetText(defenseText, $"Defense: {previewStats.Defense:F1}");
            SetText(critRateText, $"Crit: {previewStats.CritRate:F1}%");
            SetText(evasionText, $"Evasion: {previewStats.Evasion:F1}%");
        }

        private void SetText(TMP_Text text, string value)
        {
            if (text != null) text.text = value;
        }

        private void OnConfirmClicked()
        {
            if (tempPointsSpent < characterStats.UnallocatedPoints)
            {
                Debug.LogWarning($"You still have {characterStats.UnallocatedPoints - tempPointsSpent} points to spend!");
                return;
            }

            characterStats.attributes.CopyFrom(tempAttributes);
            characterStats.levelSystem.unallocatedPoints = 0;

            if (CharacterManager.Instance != null)
            {
                CharacterManager.Instance.RecalculateStats();
            }
            else
            {
                characterStats.RecalculateStats(BaseStats, false);
            }

            GameManager.Instance.SaveGame();
            levelUpPanel?.SetActive(false);
            Debug.Log("Level up points allocated!");
        }

        private void OnCloseClicked()
        {
            if (characterStats.UnallocatedPoints > 0)
            {
                Debug.LogWarning("You must allocate all points before closing!");
                return;
            }

            levelUpPanel?.SetActive(false);
        }
        #endregion
    }
}
