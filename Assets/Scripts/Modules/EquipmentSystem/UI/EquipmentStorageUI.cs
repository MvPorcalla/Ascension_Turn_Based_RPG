// ════════════════════════════════════════════
// Assets\Scripts\Modules\EquipmentSystem\UI\EquipmentStorageUI.cs
// Filtered storage display for equipment room
// ════════════════════════════════════════════

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Ascension.Data.SO.Item;
using Ascension.Equipment.Enums;
using Ascension.Equipment.Manager;
using Ascension.Equipment.Services;
using Ascension.Inventory.Manager;
using Ascension.Inventory.UI;
using Ascension.SharedUI.Popups;

namespace Ascension.Equipment.UI
{
    public class EquipmentStorageUI : MonoBehaviour
    {
        #region Serialized Fields
        [Header("UI References")]
        [SerializeField] private Transform storageContent;
        [SerializeField] private GameObject itemSlotPrefab;
        
        [Header("Filter Buttons")]
        [SerializeField] private Button gearButton;
        [SerializeField] private Button abilitiesButton;
        
        [Header("Gear Sort Buttons")]
        [SerializeField] private Button allGearButton;
        [SerializeField] private Button weaponsButton;
        [SerializeField] private Button helmetsButton;
        [SerializeField] private Button chestsButton;
        [SerializeField] private Button glovesButton;
        [SerializeField] private Button bootsButton;
        [SerializeField] private Button accessoriesButton;
        #endregion

        #region Private Fields
        private EquipmentStorageFilter _currentFilter = EquipmentStorageFilter.All;
        private List<GameObject> _storageSlots = new List<GameObject>();
        private GearSlotService _slotService;
        #endregion

        #region Unity Callbacks
        private void Awake()
        {
            _slotService = new GearSlotService();
            SetupButtons();
        }

        private void OnEnable()
        {
            SubscribeToEvents();
            RefreshStorage();
        }

        private void OnDisable()
        {
            UnsubscribeFromEvents();
        }
        #endregion

        #region Setup
        private void SetupButtons()
        {
            // Main filter buttons
            if (gearButton != null)
                gearButton.onClick.AddListener(() => SetMainFilter(EquipmentStorageFilter.All));
            
            if (abilitiesButton != null)
                abilitiesButton.onClick.AddListener(() => SetMainFilter(EquipmentStorageFilter.Abilities));

            // Gear sort buttons
            if (allGearButton != null)
                allGearButton.onClick.AddListener(() => SetFilter(EquipmentStorageFilter.All));
            
            if (weaponsButton != null)
                weaponsButton.onClick.AddListener(() => SetFilter(EquipmentStorageFilter.Weapons));
            
            if (helmetsButton != null)
                helmetsButton.onClick.AddListener(() => SetFilter(EquipmentStorageFilter.Helmets));
            
            if (chestsButton != null)
                chestsButton.onClick.AddListener(() => SetFilter(EquipmentStorageFilter.Chests));
            
            if (glovesButton != null)
                glovesButton.onClick.AddListener(() => SetFilter(EquipmentStorageFilter.Gloves));
            
            if (bootsButton != null)
                bootsButton.onClick.AddListener(() => SetFilter(EquipmentStorageFilter.Boots));
            
            if (accessoriesButton != null)
                accessoriesButton.onClick.AddListener(() => SetFilter(EquipmentStorageFilter.Accessories));
        }

        private void SubscribeToEvents()
        {
            if (InventoryManager.Instance != null)
            {
                InventoryManager.Instance.Inventory.OnInventoryChanged += RefreshStorage;
            }

            if (EquipmentManager.Instance != null)
            {
                EquipmentManager.Instance.OnEquipmentChanged += RefreshStorage;
            }
        }

        private void UnsubscribeFromEvents()
        {
            if (InventoryManager.Instance != null)
            {
                InventoryManager.Instance.Inventory.OnInventoryChanged -= RefreshStorage;
            }

            if (EquipmentManager.Instance != null)
            {
                EquipmentManager.Instance.OnEquipmentChanged -= RefreshStorage;
            }
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Show items matching a specific filter
        /// </summary>
        public void ShowFilteredItems(EquipmentStorageFilter filter)
        {
            SetFilter(filter);
        }
        #endregion

        #region Private Methods - Filtering
        private void SetMainFilter(EquipmentStorageFilter filter)
        {
            _currentFilter = filter;
            RefreshStorage();
        }

        private void SetFilter(EquipmentStorageFilter filter)
        {
            _currentFilter = filter;
            RefreshStorage();
            Debug.Log($"[EquipmentStorageUI] Filter set to: {filter}");
        }
        #endregion

        #region Private Methods - Display
        private void RefreshStorage()
        {
            ClearStorageSlots();
            
            if (InventoryManager.Instance == null || EquipmentManager.Instance == null)
            {
                Debug.LogWarning("[EquipmentStorageUI] Managers not available");
                return;
            }

            var filteredItems = GetFilteredItems();
            
            foreach (var item in filteredItems)
            {
                CreateItemSlot(item);
            }

            Debug.Log($"[EquipmentStorageUI] Showing {filteredItems.Count} items with filter: {_currentFilter}");
        }

        private void ClearStorageSlots()
        {
            foreach (var slot in _storageSlots)
            {
                Destroy(slot);
            }
            _storageSlots.Clear();
        }

        private List<Inventory.Data.ItemInstance> GetFilteredItems()
        {
            var storageItems = InventoryManager.Instance.Inventory.GetStorageItems();
            var filteredItems = new List<Inventory.Data.ItemInstance>();

            foreach (var item in storageItems)
            {
                ItemBaseSO itemData = EquipmentManager.Instance.Database.GetItem(item.itemID);
                
                if (itemData != null && _slotService.MatchesFilter(itemData, _currentFilter))
                {
                    filteredItems.Add(item);
                }
            }

            return filteredItems;
        }

        private void CreateItemSlot(Inventory.Data.ItemInstance item)
        {
            GameObject slotObj = Instantiate(itemSlotPrefab, storageContent);
            ItemSlotUI slotUI = slotObj.GetComponent<ItemSlotUI>();

            if (slotUI != null)
            {
                ItemBaseSO itemData = EquipmentManager.Instance.Database.GetItem(item.itemID);
                
                if (itemData != null)
                {
                    slotUI.Setup(itemData, item, () => OnItemClicked(item));
                }
            }

            _storageSlots.Add(slotObj);
        }
        #endregion

        #region Event Handlers
        /// <summary>
        /// Handle item click based on type
        /// </summary>
        private void OnItemClicked(Inventory.Data.ItemInstance item)
        {
            ItemBaseSO itemData = EquipmentManager.Instance.Database.GetItem(item.itemID);
            
            if (itemData == null)
            {
                Debug.LogError($"[EquipmentStorageUI] Item not found in database: {item.itemID}");
                return;
            }

            // Show popup for weapons and gear
            if (itemData is WeaponSO || itemData is GearSO)
            {
                var context = new EquipmentRoomContext();
                GearPopup.Instance.Show(itemData, item, context);
            }
            // Show skill popup for abilities
            else if (itemData is AbilitySO ability)
            {
                ShowSkillPopup(ability, item);
            }
        }

        /// <summary>
        /// Show skill popup for abilities
        /// TODO: Implement SkillAssignmentPopup in future
        /// </summary>
        private void ShowSkillPopup(AbilitySO skill, Inventory.Data.ItemInstance item)
        {
            Debug.Log($"[EquipmentStorageUI] Skill clicked: {skill.AbilityName}");
            // TODO: Create and show SkillAssignmentPopup
            Debug.LogWarning("[EquipmentStorageUI] Skill assignment popup not yet implemented");
        }
        #endregion
    }
}