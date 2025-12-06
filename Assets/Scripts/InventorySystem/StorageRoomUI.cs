// ──────────────────────────────────────────────────
// StorageRoomUI.cs
// Manages the Storage Room UI for item management
// ──────────────────────────────────────────────────

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using Ascension.Managers;
using Ascension.Data.SO;

public class StorageRoomUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Transform inventoryContent; // Bag content
    [SerializeField] private Transform pocketContent; // Pocket content
    [SerializeField] private Transform storageContent;
    [SerializeField] private GameObject itemSlotPrefab;
    [SerializeField] private GameObject emptySlotPrefab; // For empty bag slots

    [Header("Filter Buttons")]
    [SerializeField] private Button allItemsButton;
    [SerializeField] private Button weaponButton;
    [SerializeField] private Button gearButton;
    [SerializeField] private Button potionButton;
    [SerializeField] private Button materialsButton;
    [SerializeField] private Button miscButton;

    [Header("Popups")]
    [SerializeField] private InventoryItemPopup itemPopup; // For stackable items (materials, misc)
    [SerializeField] private InventoryPotionPopup potionPopup; // For potions
    [SerializeField] private InventoryGearPopup gearPopup; // For weapons, Gear

    [Header("Quick Actions")]
    [SerializeField] private Button storeAllButton;
    [SerializeField] private Button backButton;

    private ItemType? currentFilter = null;

    private List<GameObject> inventorySlots = new List<GameObject>();
    private List<GameObject> pocketSlots = new List<GameObject>();
    private List<GameObject> storageSlots = new List<GameObject>();

    private void Start()
    {
        // Quick actions
        storeAllButton.onClick.AddListener(OnStoreAllBagClicked);
        backButton.onClick.AddListener(OnBackClicked);
        
        // Filter buttons
        allItemsButton.onClick.AddListener(() => SetFilter(null));
        weaponButton.onClick.AddListener(() => SetFilter(ItemType.Weapon));
        gearButton.onClick.AddListener(() => SetFilter(ItemType.Gear));
        potionButton.onClick.AddListener(() => SetFilter(ItemType.Consumable));
        materialsButton.onClick.AddListener(() => SetFilter(ItemType.Material));
        miscButton.onClick.AddListener(() => SetFilter(ItemType.Misc));

        // Subscribe to inventory changes
        InventoryManager.Instance.Inventory.OnInventoryChanged += RefreshUI;
        InventoryManager.Instance.OnInventoryLoaded += RefreshUI;

        RefreshUI();
    }

    private void OnDestroy()
    {
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.Inventory.OnInventoryChanged -= RefreshUI;
            InventoryManager.Instance.OnInventoryLoaded -= RefreshUI;
        }
    }

    #region UI Refresh

    private void RefreshUI()
    {
        RefreshInventorySection();
        RefreshPocketSection();
        RefreshStorageSection();
    }

    private void RefreshInventorySection()
    {
        foreach (var slot in inventorySlots)
            Destroy(slot);
        inventorySlots.Clear();

        var bagItems = InventoryManager.Instance.Inventory.GetBagItems();
        int maxSlots = InventoryManager.Instance.Inventory.maxBagSlots;

        foreach (var item in bagItems)
        {
            GameObject slotObj = Instantiate(itemSlotPrefab, inventoryContent);
            ItemSlotUI slotUI = slotObj.GetComponent<ItemSlotUI>();
            
            ItemBaseSO itemData = InventoryManager.Instance.Database.GetItem(item.itemID);
            slotUI.Setup(itemData, item, () => OnItemClicked(item, ItemLocation.Bag));

            inventorySlots.Add(slotObj);
        }

        int emptySlots = maxSlots - bagItems.Count;
        for (int i = 0; i < emptySlots; i++)
        {
            GameObject emptySlot = Instantiate(emptySlotPrefab, inventoryContent);
            inventorySlots.Add(emptySlot);
        }
    }

    private void RefreshPocketSection()
    {
        foreach (var slot in pocketSlots)
            Destroy(slot);
        pocketSlots.Clear();

        var pocketItems = InventoryManager.Instance.Inventory.GetPocketItems();
        int maxSlots = InventoryManager.Instance.Inventory.maxPocketSlots;

        foreach (var item in pocketItems)
        {
            GameObject slotObj = Instantiate(itemSlotPrefab, pocketContent);
            ItemSlotUI slotUI = slotObj.GetComponent<ItemSlotUI>();
            
            ItemBaseSO itemData = InventoryManager.Instance.Database.GetItem(item.itemID);
            slotUI.Setup(itemData, item, () => OnItemClicked(item, ItemLocation.Pocket));

            pocketSlots.Add(slotObj);
        }

        int emptySlots = maxSlots - pocketItems.Count;
        for (int i = 0; i < emptySlots; i++)
        {
            GameObject emptySlot = Instantiate(emptySlotPrefab, pocketContent);
            pocketSlots.Add(emptySlot);
        }
    }

    private void RefreshStorageSection()
    {
        foreach (var slot in storageSlots)
            Destroy(slot);
        storageSlots.Clear();

        var storageItems = InventoryManager.Instance.Inventory.GetStorageItemsByType(
            currentFilter, 
            InventoryManager.Instance.Database
        );

        foreach (var item in storageItems)
        {
            GameObject slotObj = Instantiate(itemSlotPrefab, storageContent);
            ItemSlotUI slotUI = slotObj.GetComponent<ItemSlotUI>();
            
            ItemBaseSO itemData = InventoryManager.Instance.Database.GetItem(item.itemID);
            slotUI.Setup(itemData, item, () => OnItemClicked(item, ItemLocation.Storage));

            storageSlots.Add(slotObj);
        }
    }

    #endregion

    #region Filter System

    private void SetFilter(ItemType? filterType)
    {
        currentFilter = filterType;
        RefreshStorageSection();
        UpdateFilterButtonStates();
    }

    private void UpdateFilterButtonStates()
    {
        // TODO: Add visual feedback for active filter button
    }

    #endregion

    #region Item Click Handler

    private void OnItemClicked(ItemInstance item, ItemLocation fromLocation)
    {
        ItemBaseSO itemData = InventoryManager.Instance.Database.GetItem(item.itemID);

        // Route to appropriate popup based on item type
        if (itemData is PotionSO potion)
        {
            // Use dedicated potion popup
            potionPopup.ShowPotion(potion, item, fromLocation);
        }
        else if (itemData.IsStackable)
        {
            // Use generic item popup for other stackables (materials, misc, ingredients)
            itemPopup.ShowItem(itemData, item, fromLocation);
        }
        else
        {
            // Use gear popup for non-stackable items (weapons, Gear)
            gearPopup.ShowGear(itemData, item, fromLocation);
        }
    }

    #endregion

    #region Quick Actions

    private void OnStoreAllBagClicked()
    {
        InventoryManager.Instance.Inventory.StoreAllItems();
        Debug.Log("All bag items stored!");
    }

    private void OnBackClicked()
    {
        Debug.Log("Back button clicked");
        gameObject.SetActive(false);
    }

    #endregion
}