// ══════════════════════════════════════════════════════════════════
// Assets/Scripts/UI/Components/Inventory/EquipmentSlotUI.cs
// ✅ REUSABLE: Single labeled equipment slot (Weapon, Helmet, etc.)
// Used by EquippedGearPreview in both Storage scene and InventoryPanel
// ══════════════════════════════════════════════════════════════════

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using Ascension.Equipment.Enums;
using Ascension.Equipment.Manager;
using Ascension.Inventory.Manager;
using Ascension.Inventory.Enums;
using Ascension.UI.Popups;

namespace Ascension.UI.Components.Inventory
{
    /// <summary>
    /// Displays a single equipment slot with label (e.g., "Weapon", "Helmet")
    /// Automatically updates when equipment changes
    /// Click to show popup (context-aware based on scene)
    /// </summary>
    public class EquipmentSlotUI : MonoBehaviour
    {
        #region Serialized Fields

        [Header("Slot Configuration")]
        [SerializeField] private GearSlotType slotType;

        [Header("UI References")]
        [SerializeField] private Button slotButton;
        [SerializeField] private Image iconImage;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private GameObject emptyOverlay;
        [SerializeField] private TMP_Text labelText;

        [Header("Visual Feedback")]
        [SerializeField] private Color emptySlotColor = new Color(0.2f, 0.2f, 0.2f, 0.5f);
        [SerializeField] private Color filledSlotColor = Color.white;

        #endregion

        #region Private Fields
        private string currentItemId;
        private EquipmentManager equipmentManager;
        private InventoryManager inventoryManager;
        private bool isInitialized = false;
        #endregion

        #region Unity Callbacks
        private void OnEnable()
        {
            if (!isInitialized)
            {
                Initialize();
            }
            else
            {
                // Re-subscribe on enable (in case we were disabled)
                SubscribeToEvents();
                RefreshSlot();
            }
        }

        private void OnDisable()
        {
            // Only unsubscribe, don't cleanup (we might be re-enabled)
            UnsubscribeFromEvents();
        }

        private void OnDestroy()
        {
            // Full cleanup on destroy
            CleanupButton();
        }
        #endregion

        #region Initialization
        private void Initialize()
        {
            // Get manager references
            equipmentManager = EquipmentManager.Instance;
            inventoryManager = InventoryManager.Instance;

            if (equipmentManager == null)
            {
                Debug.LogError("[EquipmentSlotUI] EquipmentManager not found!");
                return;
            }

            if (inventoryManager == null)
            {
                Debug.LogError("[EquipmentSlotUI] InventoryManager not found!");
                return;
            }

            // Setup UI
            SetupLabel();
            SetupButton();

            // Subscribe to events
            SubscribeToEvents();

            // Initial display
            RefreshSlot();
            
            isInitialized = true;
        }

        private void SetupLabel()
        {
            if (labelText != null)
            {
                labelText.text = slotType.ToDisplayName();
            }
        }

        private void SetupButton()
        {
            if (slotButton != null)
            {
                slotButton.onClick.AddListener(OnSlotClicked);
            }
        }

        private void CleanupButton()
        {
            if (slotButton != null)
            {
                slotButton.onClick.RemoveListener(OnSlotClicked);
            }
        }
        #endregion

        #region Event Management
        private void SubscribeToEvents()
        {
            if (equipmentManager != null)
            {
                equipmentManager.OnEquipmentChanged += RefreshSlot;
            }
        }

        private void UnsubscribeFromEvents()
        {
            if (equipmentManager != null)
            {
                equipmentManager.OnEquipmentChanged -= RefreshSlot;
            }
        }
        #endregion

        #region Display Logic
        /// <summary>
        /// Refresh slot display (called on equipment change)
        /// </summary>
        private void RefreshSlot()
        {
            if (equipmentManager == null || inventoryManager == null)
                return;

            // Get equipped item ID for this slot
            currentItemId = equipmentManager.GetEquippedItemId(slotType);
            bool hasItem = !string.IsNullOrEmpty(currentItemId);

            // Update visuals
            UpdateEmptyOverlay(hasItem);
            UpdateBackground(hasItem);
            UpdateIcon(hasItem);
            UpdateButton(hasItem);
        }

        private void UpdateEmptyOverlay(bool hasItem)
        {
            if (emptyOverlay != null)
            {
                emptyOverlay.SetActive(!hasItem);
            }
        }

        private void UpdateBackground(bool hasItem)
        {
            if (backgroundImage != null)
            {
                backgroundImage.color = hasItem ? filledSlotColor : emptySlotColor;
            }
        }

        private void UpdateIcon(bool hasItem)
        {
            if (iconImage == null)
                return;

            if (!hasItem)
            {
                iconImage.sprite = null;
                iconImage.enabled = false;
                return;
            }

            var itemData = inventoryManager.Database.GetItem(currentItemId);
            if (itemData != null && itemData.Icon != null)
            {
                iconImage.sprite = itemData.Icon;
                iconImage.enabled = true;
                iconImage.color = Color.white;
            }
            else
            {
                iconImage.sprite = null;
                iconImage.enabled = false;
            }
        }

        private void UpdateButton(bool hasItem)
        {
            if (slotButton != null)
            {
                slotButton.interactable = hasItem;
            }
        }
        #endregion

        #region Click Handler
        /// <summary>
        /// Handle slot click - show popup with context
        /// </summary>
        private void OnSlotClicked()
        {
            if (string.IsNullOrEmpty(currentItemId))
            {
                Debug.LogWarning($"[EquipmentSlotUI] Slot {slotType} is empty");
                return;
            }

            if (inventoryManager?.Database == null)
            {
                Debug.LogError("[EquipmentSlotUI] InventoryManager not available");
                return;
            }

            // Get item data
            var itemData = inventoryManager.Database.GetItem(currentItemId);
            if (itemData == null)
            {
                Debug.LogError($"[EquipmentSlotUI] Item data not found: {currentItemId}");
                return;
            }

            // Get item instance
            var itemInstance = inventoryManager.Inventory.allItems
                .FirstOrDefault(i => i.itemID == currentItemId && i.location == ItemLocation.Equipped);

            if (itemInstance == null)
            {
                Debug.LogError($"[EquipmentSlotUI] ItemInstance not found: {currentItemId}");
                return;
            }

            // Show popup with context
            PopupContext context = GetPopupContext();
            PopupManager.Instance?.ShowItemPopup(itemData, itemInstance, context);
        }

        /// <summary>
        /// Get popup context based on configuration
        /// </summary>
        private PopupContext GetPopupContext()
        {
            // Always return EquippedGear context for equipment slots
            // (Can only unequip, regardless of scene)
            return PopupContext.FromEquippedGear();
        }
        #endregion

        #region Public API
        /// <summary>
        /// Force refresh slot display (useful for manual updates)
        /// </summary>
        public void ForceRefresh()
        {
            RefreshSlot();
        }

        /// <summary>
        /// Check if this slot is empty
        /// </summary>
        public bool IsEmpty()
        {
            return string.IsNullOrEmpty(currentItemId);
        }

        /// <summary>
        /// Get current item ID in this slot
        /// </summary>
        public string GetItemId()
        {
            return currentItemId;
        }
        #endregion

        #region Editor Helpers
#if UNITY_EDITOR
        [ContextMenu("Force Refresh")]
        private void EditorForceRefresh()
        {
            if (Application.isPlaying)
            {
                RefreshSlot();
            }
            else
            {
                Debug.LogWarning("[EquipmentSlotUI] Force Refresh only works in Play Mode");
            }
        }
#endif
        #endregion
    }
}