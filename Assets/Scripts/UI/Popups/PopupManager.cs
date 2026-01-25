// ══════════════════════════════════════════════════════════════════
// Assets/Scripts/UI/Popups/PopupManager.cs
// ✅ FIXED: Added comprehensive debugging
// ══════════════════════════════════════════════════════════════════

using UnityEngine;
using Ascension.Data.SO.Item;
using Ascension.Inventory.Data;
using Ascension.Inventory.Enums;

namespace Ascension.UI.Popups
{
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

        [Header("Debug")]
        [SerializeField] private bool enableDebugLogs = true;
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
                Debug.LogWarning("[PopupManager] Duplicate instance destroyed");
                Destroy(gameObject);
                return;
            }
            Instance = this;
            Log("PopupManager initialized");
        }

        private void ValidatePopups()
        {
            if (gearPopup == null)
                Debug.LogError("[PopupManager] GearPopup not assigned!");
            else
                Log($"GearPopup found: {gearPopup.name}");
            
            if (itemPopup == null)
                Debug.LogError("[PopupManager] ItemPopup not assigned!");
            else
                Log($"ItemPopup found: {itemPopup.name}");
            
            if (potionPopup == null)
                Debug.LogError("[PopupManager] PotionPopup not assigned!");
            else
                Log($"PotionPopup found: {potionPopup.name}");
        }
        #endregion

        #region Public API - Smart Routing

        public void ShowItemPopup(ItemBaseSO itemData, ItemInstance itemInstance, PopupContext context)
        {
            Log($"ShowItemPopup called: {itemData?.ItemName ?? "NULL"}, Type: {itemData?.GetType().Name ?? "NULL"}");

            if (itemData == null || itemInstance == null)
            {
                Debug.LogError("[PopupManager] Cannot show popup - null data");
                return;
            }

            CloseCurrentPopup();

            // ✅ FIXED: Check specific types BEFORE IsStackable
            if (itemData is PotionSO potion)
            {
                Log($"Routing to PotionPopup: {itemData.ItemName}");
                ShowPotionPopup(potion, itemInstance, context);
            }
            else if (itemData is WeaponSO || itemData is GearSO)
            {
                Log($"Routing to GearPopup: {itemData.ItemName}");
                ShowGearPopup(itemData, itemInstance, context);
            }
            else if (itemData.IsStackable)
            {
                Log($"Routing to ItemPopup (stackable): {itemData.ItemName}");
                ShowStackableItemPopup(itemData, itemInstance, context);
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
            Log($"ShowGearPopup: {itemData.ItemName}");

            if (gearPopup == null)
            {
                Debug.LogError("[PopupManager] GearPopup is null!");
                return;
            }

            CloseCurrentPopup();
            
            Log("Calling gearPopup.Show()");
            gearPopup.Show(itemData, itemInstance, context);
            currentActivePopup = gearPopup;
            
            Log($"GearPopup active: {gearPopup.gameObject.activeSelf}");
        }

        public void ShowStackableItemPopup(ItemBaseSO itemData, ItemInstance itemInstance, PopupContext context)
        {
            Log($"ShowStackableItemPopup: {itemData.ItemName}");

            if (itemPopup == null)
            {
                Debug.LogError("[PopupManager] ItemPopup is null!");
                return;
            }

            CloseCurrentPopup();
            
            Log("Calling itemPopup.ShowItem()");
            itemPopup.ShowItem(itemData, itemInstance, context);
            currentActivePopup = itemPopup;
            
            Log($"ItemPopup active: {itemPopup.gameObject.activeSelf}");
        }

        public void ShowPotionPopup(PotionSO potionData, ItemInstance itemInstance, PopupContext context)
        {
            Log($"ShowPotionPopup: {potionData.ItemName}");

            if (potionPopup == null)
            {
                Debug.LogError("[PopupManager] PotionPopup is null!");
                return;
            }

            CloseCurrentPopup();
            
            Log("Calling potionPopup.ShowPotion()");
            potionPopup.ShowPotion(potionData, itemInstance, context);
            currentActivePopup = potionPopup;
            
            Log($"PotionPopup active: {potionPopup.gameObject.activeSelf}");
        }

        #endregion

        #region Popup Management

        public void CloseCurrentPopup()
        {
            if (currentActivePopup == null)
            {
                Log("CloseCurrentPopup: No popup open");
                return;
            }

            Log($"Closing popup: {currentActivePopup.GetType().Name}");

            if (currentActivePopup is GearPopup gear)
                gear.Hide();
            else if (currentActivePopup is ItemPopup item)
                item.Hide();
            else if (currentActivePopup is PotionPopup potion)
                potion.Hide();

            currentActivePopup = null;
            Log("Popup closed");
        }

        public bool IsAnyPopupOpen() => currentActivePopup != null;

        #endregion

        #region Logging
        private void Log(string message)
        {
            if (enableDebugLogs)
                Debug.Log($"[PopupManager] {message}");
        }
        #endregion
    }
}