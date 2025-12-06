// ──────────────────────────────────────────────────
// EquipmentRoomUI.cs
// Manages the Equipment Room UI and slot interactions
// ──────────────────────────────────────────────────

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;
using Ascension.Managers;
using Ascension.Data.SO;
using Ascension.UI;

public class EquipmentRoomUI : MonoBehaviour
{
    [Header("UI Panels")]
    [SerializeField] private GameObject equipmentRoomPanel;
    
    [Header("Player Preview")]
    [SerializeField] private PlayerPreviewUI playerPreview;
    
    [Header("Gear Slots")]
    [SerializeField] private EquipmentSlotUI weaponSlot;
    [SerializeField] private EquipmentSlotUI helmetSlot;
    [SerializeField] private EquipmentSlotUI chestPlateSlot;
    [SerializeField] private EquipmentSlotUI glovesSlot;
    [SerializeField] private EquipmentSlotUI bootsSlot;
    [SerializeField] private EquipmentSlotUI accessory1Slot;
    [SerializeField] private EquipmentSlotUI accessory2Slot;
    
    [Header("Skill Slots")]
    [SerializeField] private EquipmentSlotUI normalSkill1Slot;
    [SerializeField] private EquipmentSlotUI normalSkill2Slot;
    [SerializeField] private EquipmentSlotUI ultimateSkillSlot;
    
    [Header("HotBar Slots")]
    [SerializeField] private EquipmentSlotUI hotbarSlot1;
    [SerializeField] private EquipmentSlotUI hotbarSlot2;
    [SerializeField] private EquipmentSlotUI hotbarSlot3;
    
    [Header("Storage Section")]
    [SerializeField] private Transform storageContent;
    [SerializeField] private GameObject itemSlotPrefab;
    [SerializeField] private Button gearButton;
    [SerializeField] private Button abilitiesButton;
    [SerializeField] private GameObject gearSortButtons;
    [SerializeField] private GameObject abilitiesSortButtons;
    
    [Header("Popup")]
    [SerializeField] private GearInfoPopup gearInfoPopup;
    [SerializeField] private EquipmentRoomPotionPopup potionInfoPopup;
    // [SerializeField] private SkillPopup skillInfoPopup;
    
    [Header("Storage Mode")]
    private StorageMode currentStorageMode = StorageMode.Gear;
    private ItemType currentSortFilter = ItemType.Weapon;
    
    [Header("Cached")]
    private List<EquipmentStorageSlotUI> storageSlots = new List<EquipmentStorageSlotUI>(); // Changed
    private EquipmentSlotUI selectedSlot = null;
    
    [Header("Debug")]
    [SerializeField] private bool debugMode = false;

    private void Awake()
    {
        // Initialize slot references
        InitializeSlots();
        
        // Setup button listeners
        gearButton.onClick.AddListener(() => SwitchStorageMode(StorageMode.Gear));
        abilitiesButton.onClick.AddListener(() => SwitchStorageMode(StorageMode.Abilities));
    }

    private void OnEnable()
    {
        // Subscribe to equipment changes
        if (EquipmentManager.Instance != null)
            EquipmentManager.Instance.OnEquipmentChanged += RefreshAllSlots;
        
        // Initial refresh
        RefreshAllSlots();
        RefreshStorage();
        UpdatePlayerPreview();
    }

    private void OnDisable()
    {
        // Unsubscribe
        if (EquipmentManager.Instance != null)
            EquipmentManager.Instance.OnEquipmentChanged -= RefreshAllSlots;
    }

    #region Initialization
    
    private void InitializeSlots()
    {
        // Initialize gear slots
        weaponSlot.Initialize(EquipmentSlotType.Weapon, OnSlotClicked);
        helmetSlot.Initialize(EquipmentSlotType.Helmet, OnSlotClicked);
        chestPlateSlot.Initialize(EquipmentSlotType.ChestPlate, OnSlotClicked);
        glovesSlot.Initialize(EquipmentSlotType.Gloves, OnSlotClicked);
        bootsSlot.Initialize(EquipmentSlotType.Boots, OnSlotClicked);
        accessory1Slot.Initialize(EquipmentSlotType.Accessory1, OnSlotClicked);
        accessory2Slot.Initialize(EquipmentSlotType.Accessory2, OnSlotClicked);
        
        // Initialize skill slots
        normalSkill1Slot.Initialize(EquipmentSlotType.NormalSkill1, OnSlotClicked);
        normalSkill2Slot.Initialize(EquipmentSlotType.NormalSkill2, OnSlotClicked);
        ultimateSkillSlot.Initialize(EquipmentSlotType.UltimateSkill, OnSlotClicked);
        
        // Initialize hotbar slots
        hotbarSlot1.Initialize(EquipmentSlotType.Hotbar1, OnSlotClicked);
        hotbarSlot2.Initialize(EquipmentSlotType.Hotbar2, OnSlotClicked);
        hotbarSlot3.Initialize(EquipmentSlotType.Hotbar3, OnSlotClicked);
        
        if (debugMode)
            Debug.Log("[EquipmentRoomManager] Slots initialized");
    }
    
    #endregion

    #region Slot Management
    
    private void RefreshAllSlots()
    {
        if (EquipmentManager.Instance == null) return;
        
        // Refresh gear slots
        weaponSlot.SetItem(EquipmentManager.Instance.GetEquippedWeapon());
        helmetSlot.SetItem(EquipmentManager.Instance.GetEquippedHelmet());
        chestPlateSlot.SetItem(EquipmentManager.Instance.GetEquippedChestPlate());
        glovesSlot.SetItem(EquipmentManager.Instance.GetEquippedGloves());
        bootsSlot.SetItem(EquipmentManager.Instance.GetEquippedBoots());
        accessory1Slot.SetItem(EquipmentManager.Instance.GetEquippedAccessory1());
        accessory2Slot.SetItem(EquipmentManager.Instance.GetEquippedAccessory2());
        
        // Refresh skill slots
        normalSkill1Slot.SetItem(EquipmentManager.Instance.GetNormalSkill1());
        normalSkill2Slot.SetItem(EquipmentManager.Instance.GetNormalSkill2());
        ultimateSkillSlot.SetItem(EquipmentManager.Instance.GetUltimateSkill());
        
        // Refresh hotbar slots
        hotbarSlot1.SetItem(EquipmentManager.Instance.GetHotbarItem1());
        hotbarSlot2.SetItem(EquipmentManager.Instance.GetHotbarItem2());
        hotbarSlot3.SetItem(EquipmentManager.Instance.GetHotbarItem3());
        
        // Refresh storage to update equipped indicators
        RefreshStorage();
    }
    
    private void OnSlotClicked(EquipmentSlotUI slot)
    {
        selectedSlot = slot;
        
        // If slot has item, show popup with equip/unequip option
        if (slot.HasItem())
        {
            ShowGearInfoPopup(slot.GetItem(), true);
        }
        else
        {
            // Filter storage to show compatible items
            FilterStorageForSlot(slot);
        }
        
        if (debugMode)
            Debug.Log($"[EquipmentRoomManager] Slot clicked: {slot.SlotType}");
    }
    
    #endregion

    #region Storage Management
    
    private void SwitchStorageMode(StorageMode mode)
    {
        currentStorageMode = mode;
        
        // Update button states
        gearButton.interactable = (mode != StorageMode.Gear);
        abilitiesButton.interactable = (mode != StorageMode.Abilities);
        
        // Show appropriate sort buttons
        gearSortButtons.SetActive(mode == StorageMode.Gear);
        abilitiesSortButtons.SetActive(mode == StorageMode.Abilities);
        
        // Reset filter
        if (mode == StorageMode.Gear)
            currentSortFilter = ItemType.Weapon;
        else
            currentSortFilter = ItemType.Ability;
        
        RefreshStorage();
        
        if (debugMode)
            Debug.Log($"[EquipmentRoomManager] Switched to {mode} mode");
    }
    
    public void SetSortFilter(string filterName)
    {
        // Parse filter name to ItemType
        switch (filterName)
        {
            case "Weapon":
                currentSortFilter = ItemType.Weapon;
                break;
            case "Gear":
                currentSortFilter = ItemType.Gear;
                break;
            case "Consumable":
                currentSortFilter = ItemType.Consumable;
                break;
            case "Ability":
                currentSortFilter = ItemType.Ability;
                break;
            case "All":
                currentSortFilter = ItemType.Misc; // Use Misc as "All" indicator
                break;
        }
        
        RefreshStorage();
    }
    
    // Refresh storage items based on current mode and filter
    private void RefreshStorage()
    {
        if (InventoryManager.Instance == null) return;
        
        // Clear existing slots
        foreach (var slot in storageSlots)
        {
            Destroy(slot.gameObject);
        }
        storageSlots.Clear();
        
        // Get filtered items
        List<ItemBaseSO> items = GetFilteredItems();
        
        // Create item slots
        foreach (var item in items)
        {
            GameObject slotObj = Instantiate(itemSlotPrefab, storageContent);
            EquipmentStorageSlotUI slotUI = slotObj.GetComponent<EquipmentStorageSlotUI>(); // Changed
            
            if (slotUI != null)
            {
                slotUI.SetItem(item);
                slotUI.SetEquippedIndicator(EquipmentManager.Instance.IsItemEquipped(item.ItemID));
                slotUI.OnClick += () => OnStorageItemClicked(item);

                storageSlots.Add(slotUI);
            }
        }
        
        if (debugMode)
            Debug.Log($"[EquipmentRoomManager] Storage refreshed: {items.Count} items");
    }
    
    private List<ItemBaseSO> GetFilteredItems()
    {
        List<ItemBaseSO> filtered = new List<ItemBaseSO>();
        
        if (InventoryManager.Instance == null) return filtered;
        
        // Get all items from inventory
        var allInventoryItems = InventoryManager.Instance.Inventory.allItems;
        
        foreach (var inventoryItem in allInventoryItems)
        {
            // Get the actual ItemBaseSO from database
            ItemBaseSO item = InventoryManager.Instance.Database.GetItem(inventoryItem.itemID);
            if (item == null) continue;
            
            // Filter by storage mode
            if (currentStorageMode == StorageMode.Gear)
            {
                // Show weapons, gear, and potions
                bool isGearMode = item.ItemType == ItemType.Weapon || 
                                  item.ItemType == ItemType.Gear || 
                                  item is PotionSO;
                
                if (!isGearMode) continue;
                
                // Apply sort filter
                if (currentSortFilter == ItemType.Weapon && item.ItemType != ItemType.Weapon) continue;
                if (currentSortFilter == ItemType.Gear && item.ItemType != ItemType.Gear) continue;
                if (currentSortFilter == ItemType.Consumable && !(item is PotionSO)) continue;
                // ItemType.Misc is used as "All" indicator - don't filter
            }
            else // Abilities mode
            {
                // Show only Abilities
                if (item.ItemType != ItemType.Ability) continue;
            }
            
            // Filter by selected slot compatibility
            if (selectedSlot != null && !IsItemCompatibleWithSlot(item, selectedSlot))
                continue;
            
            filtered.Add(item);
        }
        
        return filtered;
    }
    
    private void FilterStorageForSlot(EquipmentSlotUI slot)
    {
        // Auto-switch storage mode based on slot type
        switch (slot.SlotType)
        {
            case EquipmentSlotType.NormalSkill1:
            case EquipmentSlotType.NormalSkill2:
            case EquipmentSlotType.UltimateSkill:
                SwitchStorageMode(StorageMode.Abilities);
                break;
            default:
                SwitchStorageMode(StorageMode.Gear);
                break;
        }
        
        RefreshStorage();
    }
    
    private bool IsItemCompatibleWithSlot(ItemBaseSO item, EquipmentSlotUI slot)
    {
        switch (slot.SlotType)
        {
            case EquipmentSlotType.Weapon:
                return item.ItemType == ItemType.Weapon;

            case EquipmentSlotType.Helmet:
                return item is GearSO gear && gear.GearType == GearType.Helmet;

            case EquipmentSlotType.ChestPlate:
                return item is GearSO gear2 && gear2.GearType == GearType.ChestPlate;

            case EquipmentSlotType.Gloves:
                return item is GearSO gear3 && gear3.GearType == GearType.Gloves;

            case EquipmentSlotType.Boots:
                return item is GearSO gear4 && gear4.GearType == GearType.Boots;

            case EquipmentSlotType.Accessory1:
            case EquipmentSlotType.Accessory2:
                return item is GearSO gear5 && gear5.GearType == GearType.Accessory;

            case EquipmentSlotType.NormalSkill1:
            case EquipmentSlotType.NormalSkill2:
                return item is AbilitySO skill &&
                    (skill.Category == AbilityCategory.Normal || skill.Category == AbilityCategory.Weapon);

            case EquipmentSlotType.UltimateSkill:
                return item is AbilitySO skill2 && skill2.Category == AbilityCategory.Ultimate;

            case EquipmentSlotType.Hotbar1:
            case EquipmentSlotType.Hotbar2:
            case EquipmentSlotType.Hotbar3:
                return item is PotionSO;

            default:
                return false;
        }
    }
    
    #endregion

    #region Item Selection & Equipping
    
    private void OnStorageItemClicked(ItemBaseSO item)
    {
        // Show appropriate popup
        if (item is PotionSO potion)
        {
            if (potionInfoPopup != null)
            {
                bool isEquipped = EquipmentManager.Instance.IsItemEquipped(potion.ItemID);
                potionInfoPopup.ShowPotion(potion, null, isEquipped);
            }
        }
        else
        {
            ShowGearInfoPopup(item, false);
        }

        if (debugMode)
            Debug.Log($"[EquipmentRoomManager] Storage item clicked: {item.ItemName}");
    }

    
    private void ShowGearInfoPopup(ItemBaseSO item, bool isEquipped)
    {
        if (gearInfoPopup == null)
        {
            Debug.LogError("[EquipmentRoomManager] GearInfoPopup is not assigned!");
            return;
        }
        
        gearInfoPopup.Show(item, isEquipped, OnEquipButtonClicked);
    }
    
    private void OnEquipButtonClicked(ItemBaseSO item, bool currentlyEquipped)
    {
        if (currentlyEquipped)
        {
            // Unequip
            UnequipItem(item);
        }
        else
        {
            // Equip to selected slot or auto-detect slot
            EquipItem(item);
        }
        
        // Close popup
        gearInfoPopup.Hide();
        
        // Clear selection
        selectedSlot = null;
        
        // Refresh
        RefreshAllSlots();
        UpdatePlayerPreview();
    }
    
    private void EquipItem(ItemBaseSO item)
    {
        if (EquipmentManager.Instance == null) return;
        
        // If no slot selected, try to auto-detect
        if (selectedSlot == null)
        {
            selectedSlot = GetAutoSlotForItem(item);
        }
        
        if (selectedSlot == null)
        {
            Debug.LogWarning("[EquipmentRoomManager] No compatible slot found");
            return;
        }
        
        // Equip based on slot type
        switch (selectedSlot.SlotType)
        {
            case EquipmentSlotType.Weapon:
                EquipmentManager.Instance.EquipWeapon(item.ItemID);
                break;
            
            case EquipmentSlotType.Helmet:
                EquipmentManager.Instance.EquipGear(item.ItemID, GearSlotType.Helmet);
                break;
            case EquipmentSlotType.ChestPlate:
                EquipmentManager.Instance.EquipGear(item.ItemID, GearSlotType.ChestPlate);
                break;
            case EquipmentSlotType.Gloves:
                EquipmentManager.Instance.EquipGear(item.ItemID, GearSlotType.Gloves);
                break;
            case EquipmentSlotType.Boots:
                EquipmentManager.Instance.EquipGear(item.ItemID, GearSlotType.Boots);
                break;
            case EquipmentSlotType.Accessory1:
                EquipmentManager.Instance.EquipGear(item.ItemID, GearSlotType.Accessory1);
                break;
            case EquipmentSlotType.Accessory2:
                EquipmentManager.Instance.EquipGear(item.ItemID, GearSlotType.Accessory2);
                break;
            
            case EquipmentSlotType.NormalSkill1:
                EquipmentManager.Instance.EquipSkill(item.ItemID, SkillSlotType.Normal1);
                break;
            case EquipmentSlotType.NormalSkill2:
                EquipmentManager.Instance.EquipSkill(item.ItemID, SkillSlotType.Normal2);
                break;
            case EquipmentSlotType.UltimateSkill:
                EquipmentManager.Instance.EquipSkill(item.ItemID, SkillSlotType.Ultimate);
                break;
            
            case EquipmentSlotType.Hotbar1:
                EquipmentManager.Instance.EquipHotbarItem(item.ItemID, 1);
                break;
            case EquipmentSlotType.Hotbar2:
                EquipmentManager.Instance.EquipHotbarItem(item.ItemID, 2);
                break;
            case EquipmentSlotType.Hotbar3:
                EquipmentManager.Instance.EquipHotbarItem(item.ItemID, 3);
                break;
        }
    }
    
    private void UnequipItem(ItemBaseSO item)
    {
        if (EquipmentManager.Instance == null) return;
        
        // Find which slot this item is in and unequip
        if (item is WeaponSO)
        {
            EquipmentManager.Instance.UnequipWeapon();
        }
        else if (item is GearSO gear)
        {
            // Find which gear slot it's in
            if (EquipmentManager.Instance.GetEquippedHelmet() == gear)
                EquipmentManager.Instance.UnequipGear(GearSlotType.Helmet);
            else if (EquipmentManager.Instance.GetEquippedChestPlate() == gear)
                EquipmentManager.Instance.UnequipGear(GearSlotType.ChestPlate);
            else if (EquipmentManager.Instance.GetEquippedGloves() == gear)
                EquipmentManager.Instance.UnequipGear(GearSlotType.Gloves);
            else if (EquipmentManager.Instance.GetEquippedBoots() == gear)
                EquipmentManager.Instance.UnequipGear(GearSlotType.Boots);
            else if (EquipmentManager.Instance.GetEquippedAccessory1() == gear)
                EquipmentManager.Instance.UnequipGear(GearSlotType.Accessory1);
            else if (EquipmentManager.Instance.GetEquippedAccessory2() == gear)
                EquipmentManager.Instance.UnequipGear(GearSlotType.Accessory2);
        }
        else if (item is AbilitySO skill)
        {
            if (EquipmentManager.Instance.GetNormalSkill1() == skill)
                EquipmentManager.Instance.UnequipSkill(SkillSlotType.Normal1);
            else if (EquipmentManager.Instance.GetNormalSkill2() == skill)
                EquipmentManager.Instance.UnequipSkill(SkillSlotType.Normal2);
            else if (EquipmentManager.Instance.GetUltimateSkill() == skill)
                EquipmentManager.Instance.UnequipSkill(SkillSlotType.Ultimate);
        }
        else if (item.ItemType == ItemType.Consumable)
        {
            if (EquipmentManager.Instance.GetHotbarItem1() == item)
                EquipmentManager.Instance.UnequipHotbarItem(1);
            else if (EquipmentManager.Instance.GetHotbarItem2() == item)
                EquipmentManager.Instance.UnequipHotbarItem(2);
            else if (EquipmentManager.Instance.GetHotbarItem3() == item)
                EquipmentManager.Instance.UnequipHotbarItem(3);
        }
    }
    
    private EquipmentSlotUI GetAutoSlotForItem(ItemBaseSO item)
    {
        if (item is WeaponSO)
            return weaponSlot;

        if (item is GearSO gear)
        {
            switch (gear.GearType)
            {
                case GearType.Helmet: return helmetSlot;
                case GearType.ChestPlate: return chestPlateSlot;
                case GearType.Gloves: return glovesSlot;
                case GearType.Boots: return bootsSlot;
                case GearType.Accessory:
                    if (!accessory1Slot.HasItem()) return accessory1Slot;
                    if (!accessory2Slot.HasItem()) return accessory2Slot;
                    return accessory1Slot; // Replace first if both full
            }
        }

        if (item is AbilitySO skill)
        {
            switch (skill.Category)
            {
                case AbilityCategory.Normal:
                case AbilityCategory.Weapon:
                    if (!normalSkill1Slot.HasItem()) return normalSkill1Slot;
                    if (!normalSkill2Slot.HasItem()) return normalSkill2Slot;
                    return normalSkill1Slot;
                case AbilityCategory.Ultimate:
                    return ultimateSkillSlot;
            }
        }

        if (item.ItemType == ItemType.Consumable)
        {
            if (!hotbarSlot1.HasItem()) return hotbarSlot1;
            if (!hotbarSlot2.HasItem()) return hotbarSlot2;
            if (!hotbarSlot3.HasItem()) return hotbarSlot3;
            return hotbarSlot1;
        }

        return null;
    }
    
    #endregion

    #region Player Preview
    
    private void UpdatePlayerPreview()
    {
        if (playerPreview == null) return;
        if (EquipmentManager.Instance == null) return;
        
        // TODO: Get actual player stats
        // For now, just recalculate from equipment
        PlayerItemStats itemStats = EquipmentManager.Instance.GetTotalItemStats();
        
        // You'll need to get these from PlayerDataManager or similar
        // playerPreview.PreviewStats(baseStats, level, attributes, itemStats, equippedWeapon);
        
        if (debugMode)
            Debug.Log("[EquipmentRoomManager] Player preview updated");
    }
    
    #endregion
}

public enum StorageMode
{
    Gear,
    Abilities
}

public enum EquipmentSlotType
{
    Weapon,
    Helmet,
    ChestPlate,
    Gloves,
    Boots,
    Accessory1,
    Accessory2,
    NormalSkill1,
    NormalSkill2,
    UltimateSkill,
    Hotbar1,
    Hotbar2,
    Hotbar3
}