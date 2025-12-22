// ════════════════════════════════════════════
// Assets\Scripts\Modules\EquipmentSystem\UI\SkillSlotUI.cs
// UI component for individual skill slot - Mobile-Friendly (renamed from HotbarSlotUI)
// ════════════════════════════════════════════

using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Ascension.Data.SO.Item;
using Ascension.Equipment.Enums;
using Ascension.Equipment.Manager;

namespace Ascension.Equipment.UI
{
    /// <summary>
    /// UI component for individual skill loadout slot
    /// Mobile-friendly: Single tap to interact
    /// </summary>
    public class SkillSlotUI : MonoBehaviour
    {
        #region Serialized Fields
        [Header("Slot Info")]
        [SerializeField] private SkillSlotType slotType;
        
        [Header("UI Components")]
        [SerializeField] private Button slotButton;
        [SerializeField] private Image slotBackground;
        [SerializeField] private Image skillIcon;
        [SerializeField] private TMP_Text slotNameText;
        [SerializeField] private GameObject emptyIndicator;
        
        [Header("Colors")]
        [SerializeField] private Color emptySlotColor = new Color(0.3f, 0.3f, 0.3f, 1f);
        [SerializeField] private Color normalSkillColor = new Color(0.2f, 0.5f, 0.8f, 1f);
        [SerializeField] private Color ultimateSkillColor = new Color(0.8f, 0.3f, 0.2f, 1f);
        #endregion

        #region Properties
        public SkillSlotType SlotType => slotType;
        public bool IsEmpty => string.IsNullOrEmpty(GetAssignedSkillId());
        public bool IsUltimateSlot => slotType == SkillSlotType.UltimateSkill;
        #endregion

        #region Events
        /// <summary>
        /// Fired when slot is tapped/clicked
        /// </summary>
        public event Action<SkillSlotType> OnSlotClicked;
        #endregion

        #region Unity Callbacks
        private void Awake()
        {
            SetupButton();
            SetSlotName();
        }

        private void OnEnable()
        {
            SubscribeToEvents();
            RefreshSlot();
        }

        private void OnDisable()
        {
            UnsubscribeFromEvents();
        }

        private void SubscribeToEvents()
        {
            if (SkillLoadoutManager.Instance != null)
            {
                SkillLoadoutManager.Instance.OnLoadoutChanged += RefreshSlot;
            }
        }

        private void UnsubscribeFromEvents()
        {
            if (SkillLoadoutManager.Instance != null)
            {
                SkillLoadoutManager.Instance.OnLoadoutChanged -= RefreshSlot;
            }
        }
        #endregion

        #region Setup
        private void SetupButton()
        {
            if (slotButton != null)
            {
                slotButton.onClick.AddListener(HandleSlotClick);
            }
        }

        private void SetSlotName()
        {
            if (slotNameText != null)
            {
                slotNameText.text = GetSlotDisplayName();
            }
        }

        private string GetSlotDisplayName()
        {
            return slotType switch
            {
                SkillSlotType.NormalSkill1 => "Skill 1",
                SkillSlotType.NormalSkill2 => "Skill 2",
                SkillSlotType.UltimateSkill => "Ultimate",
                _ => slotType.ToString()
            };
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Refresh slot display based on assigned skill
        /// </summary>
        public void RefreshSlot()
        {
            string assignedSkillId = GetAssignedSkillId();

            if (string.IsNullOrEmpty(assignedSkillId))
            {
                ShowEmptySlot();
            }
            else
            {
                ShowAssignedSkill(assignedSkillId);
            }
        }

        /// <summary>
        /// Get currently assigned skill ID in this slot
        /// </summary>
        public string GetAssignedSkillId()
        {
            if (SkillLoadoutManager.Instance == null)
                return string.Empty;

            return SkillLoadoutManager.Instance.GetSlotSkillId(slotType);
        }
        #endregion

        #region Private Methods - Display
        private void ShowEmptySlot()
        {
            // Show empty indicator
            if (emptyIndicator != null)
                emptyIndicator.SetActive(true);

            // Hide skill icon
            if (skillIcon != null)
            {
                skillIcon.enabled = false;
                skillIcon.sprite = null;
            }

            // Set empty slot color based on type
            if (slotBackground != null)
            {
                if (IsUltimateSlot)
                    slotBackground.color = ultimateSkillColor * 0.5f; // Dimmed ultimate
                else
                    slotBackground.color = normalSkillColor * 0.5f; // Dimmed normal
            }
        }

        private void ShowAssignedSkill(string skillId)
        {
            AbilitySO skill = GetSkillData(skillId);

            if (skill == null)
            {
                Debug.LogWarning($"[SkillSlotUI] Skill not found: {skillId}");
                ShowEmptySlot();
                return;
            }

            // Hide empty indicator
            if (emptyIndicator != null)
                emptyIndicator.SetActive(false);

            // Show skill icon
            if (skillIcon != null && skill.Icon != null)
            {
                skillIcon.sprite = skill.Icon;
                skillIcon.enabled = true;
                skillIcon.color = Color.white;
            }

            // Set filled slot color
            if (slotBackground != null)
            {
                if (IsUltimateSlot)
                    slotBackground.color = ultimateSkillColor;
                else
                    slotBackground.color = normalSkillColor;
            }
        }

        private AbilitySO GetSkillData(string skillId)
        {
            if (SkillLoadoutManager.Instance != null)
            {
                return SkillLoadoutManager.Instance.GetSkill(skillId);
            }
            return null;
        }
        #endregion

        #region Event Handlers
        private void HandleSlotClick()
        {
            OnSlotClicked?.Invoke(slotType);
            Debug.Log($"[SkillSlotUI] Tapped {slotType} slot");
        }
        #endregion

        #region Debug
        [ContextMenu("Force Refresh")]
        private void DebugForceRefresh()
        {
            RefreshSlot();
        }
        #endregion
    }
}