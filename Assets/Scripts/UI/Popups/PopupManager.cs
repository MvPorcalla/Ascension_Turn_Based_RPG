// ════════════════════════════════════════════
// Assets/Scripts/Modules/SharedUI/Popups/PopupManager.cs
// Centralized manager for all popup types
// ════════════════════════════════════════════

using UnityEngine;
using Ascension.Data.SO.Item;
using Ascension.Inventory.Data;
using Ascension.Inventory.Enums;

namespace Ascension.UI.Popups
{
    /// <summary>
    /// Central access point for all popup types.
    /// Ensures only one popup is active at a time.
    /// </summary>
    public class PopupManager : MonoBehaviour
    {
        #region Singleton
        public static PopupManager Instance { get; private set; }
        #endregion

        #region Serialized Fields
        [Header("Popup References")]
        [SerializeField] private GearPopup gearPopup;
        [SerializeField] private ItemPopup itemPopup;
        [SerializeField] private PotionPopup potionPopup;

        #endregion

        #region Private Fields
        private MonoBehaviour currentActivePopup;
        #endregion

        #region Unity Callbacks
        private void Awake()
        {
            InitializeSingleton();
            ValidatePopups();
        }

        private void InitializeSingleton()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void ValidatePopups()
        {
            if (gearPopup == null)
                Debug.LogError("[PopupManager] GearPopup not assigned!");
            
            if (itemPopup == null)
                Debug.LogError("[PopupManager] ItemPopup not assigned!");
            
            if (potionPopup == null)
                Debug.LogError("[PopupManager] PotionPopup not assigned!");
        }
        #endregion

        #region Public API - Smart Routing

        /// <summary>
        /// Smart popup opener - automatically routes to correct popup type
        /// </summary>
        public void ShowItemPopup(ItemBaseSO itemData, ItemInstance itemInstance, PopupContext context)
        {
            if (itemData == null || itemInstance == null)
            {
                Debug.LogError("[PopupManager] Cannot show popup - null data");
                return;
            }

            CloseCurrentPopup();

            // Route to appropriate popup based on item type
            if (itemData is PotionSO potion)
            {
                ShowPotionPopup(potion, itemInstance, context);
            }
            else if (itemData.IsStackable)
            {
                ShowStackableItemPopup(itemData, itemInstance, context);
            }
            else if (itemData is WeaponSO || itemData is GearSO)
            {
                ShowGearPopup(itemData, itemInstance, context);
            }
            else
            {
                Debug.LogWarning($"[PopupManager] Unknown item type: {itemData.GetType().Name}");
            }
        }

        #endregion

        #region Public API - Specific Popups

        public void ShowGearPopup(ItemBaseSO itemData, ItemInstance itemInstance, PopupContext context)
        {
            if (gearPopup == null) return;

            CloseCurrentPopup();
            gearPopup.Show(itemData, itemInstance, context);
            currentActivePopup = gearPopup;
        }

        public void ShowStackableItemPopup(ItemBaseSO itemData, ItemInstance itemInstance, PopupContext context)
        {
            if (itemPopup == null) return;

            CloseCurrentPopup();
            itemPopup.ShowItem(itemData, itemInstance, context);
            currentActivePopup = itemPopup;
        }

        public void ShowPotionPopup(PotionSO potionData, ItemInstance itemInstance, PopupContext context)
        {
            if (potionPopup == null) return;

            CloseCurrentPopup();
            potionPopup.ShowPotion(potionData, itemInstance, context);
            currentActivePopup = potionPopup;
        }

        #endregion

        #region Popup Management

        public void CloseCurrentPopup()
        {
            if (currentActivePopup == null) return;

            if (currentActivePopup is GearPopup gear)
                gear.Hide();
            else if (currentActivePopup is ItemPopup item)
                item.Hide();
            else if (currentActivePopup is PotionPopup potion)
                potion.Hide();

            currentActivePopup = null;
        }

        public bool IsAnyPopupOpen() => currentActivePopup != null;

        #endregion
    }
}