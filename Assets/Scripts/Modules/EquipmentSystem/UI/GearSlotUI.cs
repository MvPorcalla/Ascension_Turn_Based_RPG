// ════════════════════════════════════════════
// Assets\Scripts\Modules\EquipmentSystem\UI\GearSlotUI.cs
// UI component for individual equipment slot - Mobile-Friendly
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
    /// UI component for individual equipment slot
    /// Mobile-friendly: Single tap to interact
    /// </summary>
    public class GearSlotUI : MonoBehaviour
    {
        #region Serialized Fields
        [Header("Slot Info")]
        [SerializeField] private GearSlotType slotType;
        
        [Header("UI Components")]
        [SerializeField] private Button slotButton;
        [SerializeField] private Image slotBackground;
        [SerializeField] private Image itemIcon;
        [SerializeField] private Image rarityBorder;
        [SerializeField] private TMP_Text slotNameText;
        [SerializeField] private GameObject emptyIndicator;
        
        [Header("Colors")]
        [SerializeField] private Color emptySlotColor = new Color(0.3f, 0.3f, 0.3f, 1f);
        [SerializeField] private Color filledSlotColor = new Color(0.5f, 0.5f, 0.5f, 1f);
        #endregion

        #region Properties
        public GearSlotType SlotType => slotType;
        public bool IsEmpty => EquipmentManager.Instance?.IsSlotEmpty(slotType) ?? true;
        #endregion

        #region Events
        /// <summary>
        /// Fired when slot is tapped/clicked
        /// </summary>
        public event Action<GearSlotType> OnSlotClicked;
        
        // ✅ FIX: Remove unused event to eliminate warning
        // For mobile, we only need single tap interaction
        // If you need long-press later, add OnSlotLongPressed instead
        #endregion

        #region Unity Callbacks
        private void Awake()
        {
            SetupButton();
            SetSlotName();
        }

        private void OnEnable()
        {
            if (EquipmentManager.Instance != null)
            {
                EquipmentManager.Instance.OnEquipmentChanged += RefreshSlot;
            }
            
            RefreshSlot();
        }

        private void OnDisable()
        {
            if (EquipmentManager.Instance != null)
            {
                EquipmentManager.Instance.OnEquipmentChanged -= RefreshSlot;
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
                slotNameText.text = slotType.ToString();
            }
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Refresh slot display based on equipped item
        /// </summary>
        public void RefreshSlot()
        {
            if (EquipmentManager.Instance == null)
            {
                ShowEmptySlot();
                return;
            }

            string equippedItemId = EquipmentManager.Instance.GetEquippedItemId(slotType);

            if (string.IsNullOrEmpty(equippedItemId))
            {
                ShowEmptySlot();
            }
            else
            {
                ShowEquippedItem(equippedItemId);
            }
        }

        /// <summary>
        /// Get currently equipped item in this slot
        /// </summary>
        public string GetEquippedItemId()
        {
            return EquipmentManager.Instance?.GetEquippedItemId(slotType);
        }
        #endregion

        #region Private Methods - Display
        private void ShowEmptySlot()
        {
            // Show empty indicator
            if (emptyIndicator != null)
                emptyIndicator.SetActive(true);

            // Hide item icon
            if (itemIcon != null)
            {
                itemIcon.enabled = false;
                itemIcon.sprite = null;
            }

            // Hide rarity border
            if (rarityBorder != null)
                rarityBorder.enabled = false;

            // Set empty slot color
            if (slotBackground != null)
                slotBackground.color = emptySlotColor;
        }

        private void ShowEquippedItem(string itemId)
        {
            ItemBaseSO item = EquipmentManager.Instance.Database.GetItem(itemId);

            if (item == null)
            {
                Debug.LogWarning($"[GearSlotUI] Item not found in database: {itemId}");
                ShowEmptySlot();
                return;
            }

            // Hide empty indicator
            if (emptyIndicator != null)
                emptyIndicator.SetActive(false);

            // Show item icon
            if (itemIcon != null && item.Icon != null)
            {
                itemIcon.sprite = item.Icon;
                itemIcon.enabled = true;
                itemIcon.color = Color.white;
            }

            // Show rarity border
            if (rarityBorder != null)
            {
                rarityBorder.enabled = true;
                rarityBorder.color = GetRarityColor(item.Rarity);
            }

            // Set filled slot color
            if (slotBackground != null)
                slotBackground.color = filledSlotColor;
        }

        private Color GetRarityColor(Rarity rarity)
        {
            return rarity switch
            {
                Rarity.Common => new Color(0.7f, 0.7f, 0.7f, 1f),      // Gray
                Rarity.Rare => new Color(0.2f, 0.5f, 1f, 1f),          // Blue
                Rarity.Epic => new Color(0.6f, 0.2f, 1f, 1f),          // Purple
                Rarity.Legendary => new Color(1f, 0.75f, 0.1f, 1f),    // Gold
                Rarity.Mythic => new Color(1f, 0.2f, 0.2f, 1f),        // Red
                _ => Color.white
            };
        }
        #endregion

        #region Event Handlers
        private void HandleSlotClick()
        {
            OnSlotClicked?.Invoke(slotType);
            Debug.Log($"[GearSlotUI] Tapped {slotType} slot");
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