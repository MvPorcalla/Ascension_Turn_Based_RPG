// ──────────────────────────────────────────────────
// EquipmentManager.cs
// Manages currently equipped items (weapons, gear, skills, consumables)
// Persists across scenes
// ──────────────────────────────────────────────────

using UnityEngine;
using System;
using System.Collections.Generic;

public class EquipmentManager : MonoBehaviour
{
    public static EquipmentManager Instance { get; private set; }
    
    [Header("Currently Equipped")]
    [SerializeField] private WeaponSO equippedWeapon;
    [SerializeField] private GearSO equippedHelmet;
    [SerializeField] private GearSO equippedChestPlate;
    [SerializeField] private GearSO equippedGloves;
    [SerializeField] private GearSO equippedBoots;
    [SerializeField] private GearSO equippedAccessory1;
    [SerializeField] private GearSO equippedAccessory2;
    
    [Header("Skills")]
    [SerializeField] private SkillSO normalSkill1;
    [SerializeField] private SkillSO normalSkill2;
    [SerializeField] private SkillSO ultimateSkill;
    
    [Header("HotBar Items (Consumables for Combat)")]
    [SerializeField] private ItemBaseSO hotbarItem1;
    [SerializeField] private ItemBaseSO hotbarItem2;
    [SerializeField] private ItemBaseSO hotbarItem3;
    
    // event for when equipment changes
    public event Action OnEquipmentChanged;
    
    [Header("Debug")]
    [SerializeField] private bool debugMode = false;

    private void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        
        if (debugMode)
            Debug.Log("[EquipmentManager] Initialized");
    }

    #region Equipment Getters
    
    public WeaponSO GetEquippedWeapon() => equippedWeapon;
    public GearSO GetEquippedHelmet() => equippedHelmet;
    public GearSO GetEquippedChestPlate() => equippedChestPlate;
    public GearSO GetEquippedGloves() => equippedGloves;
    public GearSO GetEquippedBoots() => equippedBoots;
    public GearSO GetEquippedAccessory1() => equippedAccessory1;
    public GearSO GetEquippedAccessory2() => equippedAccessory2;
    
    public SkillSO GetNormalSkill1() => normalSkill1;
    public SkillSO GetNormalSkill2() => normalSkill2;
    public SkillSO GetUltimateSkill() => ultimateSkill;
    
    public ItemBaseSO GetHotbarItem1() => hotbarItem1;
    public ItemBaseSO GetHotbarItem2() => hotbarItem2;
    public ItemBaseSO GetHotbarItem3() => hotbarItem3;
    
    #endregion

    #region Equip/Unequip Weapons
    
    public bool EquipWeapon(string itemID)
    {
        ItemBaseSO item = GetItemFromInventory(itemID);
        if (item == null || !(item is WeaponSO weapon))
        {
            Debug.LogWarning($"[EquipmentManager] Item {itemID} is not a weapon");
            return false;
        }
        
        // Unequip current weapon
        if (equippedWeapon != null)
        {
            UnequipWeapon();
        }
        
        equippedWeapon = weapon;
        OnEquipmentChanged?.Invoke();
        
        if (debugMode)
            Debug.Log($"[EquipmentManager] Equipped weapon: {weapon.weaponName}");
        
        return true;
    }
    
    public void UnequipWeapon()
    {
        if (equippedWeapon == null) return;
        
        if (debugMode)
            Debug.Log($"[EquipmentManager] Unequipped weapon: {equippedWeapon.weaponName}");
        
        equippedWeapon = null;
        OnEquipmentChanged?.Invoke();
    }
    
    #endregion

    #region Equip/Unequip Gear
    
    public bool EquipGear(string itemID, GearSlotType slotType)
    {
        ItemBaseSO item = GetItemFromInventory(itemID);
        if (item == null || !(item is GearSO gear))
        {
            Debug.LogWarning($"[EquipmentManager] Item {itemID} is not gear");
            return false;
        }
        
        // Validate gear type matches slot
        if (!IsGearCompatibleWithSlot(gear, slotType))
        {
            Debug.LogWarning($"[EquipmentManager] {gear.gearType} cannot be equipped in {slotType} slot");
            return false;
        }
        
        // Unequip current item in slot
        UnequipGear(slotType);
        
        // Equip new gear
        switch (slotType)
        {
            case GearSlotType.Helmet:
                equippedHelmet = gear;
                break;
            case GearSlotType.ChestPlate:
                equippedChestPlate = gear;
                break;
            case GearSlotType.Gloves:
                equippedGloves = gear;
                break;
            case GearSlotType.Boots:
                equippedBoots = gear;
                break;
            case GearSlotType.Accessory1:
                equippedAccessory1 = gear;
                break;
            case GearSlotType.Accessory2:
                equippedAccessory2 = gear;
                break;
        }
        
        OnEquipmentChanged?.Invoke();
        
        if (debugMode)
            Debug.Log($"[EquipmentManager] Equipped {gear.gearName} in {slotType}");
        
        return true;
    }
    
    public void UnequipGear(GearSlotType slotType)
    {
        GearSO currentGear = GetEquippedGear(slotType);
        if (currentGear == null) return;
        
        switch (slotType)
        {
            case GearSlotType.Helmet:
                equippedHelmet = null;
                break;
            case GearSlotType.ChestPlate:
                equippedChestPlate = null;
                break;
            case GearSlotType.Gloves:
                equippedGloves = null;
                break;
            case GearSlotType.Boots:
                equippedBoots = null;
                break;
            case GearSlotType.Accessory1:
                equippedAccessory1 = null;
                break;
            case GearSlotType.Accessory2:
                equippedAccessory2 = null;
                break;
        }
        
        OnEquipmentChanged?.Invoke();
        
        if (debugMode)
            Debug.Log($"[EquipmentManager] Unequipped {currentGear.gearName} from {slotType}");
    }
    
    public GearSO GetEquippedGear(GearSlotType slotType)
    {
        switch (slotType)
        {
            case GearSlotType.Helmet: return equippedHelmet;
            case GearSlotType.ChestPlate: return equippedChestPlate;
            case GearSlotType.Gloves: return equippedGloves;
            case GearSlotType.Boots: return equippedBoots;
            case GearSlotType.Accessory1: return equippedAccessory1;
            case GearSlotType.Accessory2: return equippedAccessory2;
            default: return null;
        }
    }
    
    private bool IsGearCompatibleWithSlot(GearSO gear, GearSlotType slotType)
    {
        switch (slotType)
        {
            case GearSlotType.Helmet:
                return gear.gearType == GearType.Helmet;
            case GearSlotType.ChestPlate:
                return gear.gearType == GearType.ChestPlate;
            case GearSlotType.Gloves:
                return gear.gearType == GearType.Gloves;
            case GearSlotType.Boots:
                return gear.gearType == GearType.Boots;
            case GearSlotType.Accessory1:
            case GearSlotType.Accessory2:
                return gear.gearType == GearType.Accessory;
            default:
                return false;
        }
    }
    
    #endregion

    #region Equip/Unequip Skills
    
    public bool EquipSkill(string itemID, SkillSlotType slotType)
    {
        ItemBaseSO item = GetItemFromInventory(itemID);
        if (item == null || !(item is SkillSO skill))
        {
            Debug.LogWarning($"[EquipmentManager] Item {itemID} is not a skill");
            return false;
        }
        
        // Validate skill category matches slot
        if (!IsSkillCompatibleWithSlot(skill, slotType))
        {
            Debug.LogWarning($"[EquipmentManager] {skill.category} skill cannot be equipped in {slotType} slot");
            return false;
        }
        
        // Check weapon compatibility
        if (skill.compatibleWeaponTypes != null && skill.compatibleWeaponTypes.Length > 0)
        {
            if (equippedWeapon == null || !IsSkillCompatibleWithWeapon(skill, equippedWeapon))
            {
                Debug.LogWarning($"[EquipmentManager] Skill {skill.skillName} requires compatible weapon");
                return false;
            }
        }
        
        // Unequip current skill
        UnequipSkill(slotType);
        
        // Equip new skill
        switch (slotType)
        {
            case SkillSlotType.Normal1:
                normalSkill1 = skill;
                break;
            case SkillSlotType.Normal2:
                normalSkill2 = skill;
                break;
            case SkillSlotType.Ultimate:
                ultimateSkill = skill;
                break;
        }
        
        OnEquipmentChanged?.Invoke();
        
        if (debugMode)
            Debug.Log($"[EquipmentManager] Equipped skill: {skill.skillName}");
        
        return true;
    }
    
    public void UnequipSkill(SkillSlotType slotType)
    {
        SkillSO currentSkill = GetEquippedSkill(slotType);
        if (currentSkill == null) return;
        
        switch (slotType)
        {
            case SkillSlotType.Normal1:
                normalSkill1 = null;
                break;
            case SkillSlotType.Normal2:
                normalSkill2 = null;
                break;
            case SkillSlotType.Ultimate:
                ultimateSkill = null;
                break;
        }
        
        OnEquipmentChanged?.Invoke();
        
        if (debugMode)
            Debug.Log($"[EquipmentManager] Unequipped skill: {currentSkill.skillName}");
    }
    
    public SkillSO GetEquippedSkill(SkillSlotType slotType)
    {
        switch (slotType)
        {
            case SkillSlotType.Normal1: return normalSkill1;
            case SkillSlotType.Normal2: return normalSkill2;
            case SkillSlotType.Ultimate: return ultimateSkill;
            default: return null;
        }
    }
    
    private bool IsSkillCompatibleWithSlot(SkillSO skill, SkillSlotType slotType)
    {
        switch (slotType)
        {
            case SkillSlotType.Normal1:
            case SkillSlotType.Normal2:
                return skill.category == SkillCategory.Normal || skill.category == SkillCategory.Weapon;
            case SkillSlotType.Ultimate:
                return skill.category == SkillCategory.Ultimate;
            default:
                return false;
        }
    }
    
    private bool IsSkillCompatibleWithWeapon(SkillSO skill, WeaponSO weapon)
    {
        if (skill.compatibleWeaponTypes == null || skill.compatibleWeaponTypes.Length == 0)
            return true;
        
        foreach (var weaponType in skill.compatibleWeaponTypes)
        {
            if (weapon.weaponType == weaponType)
                return true;
        }
        
        return false;
    }
    
    #endregion

    #region Equip/Unequip HotBar Items
    
    public bool EquipHotbarItem(string itemID, int slotIndex)
    {
        if (slotIndex < 1 || slotIndex > 3)
        {
            Debug.LogWarning($"[EquipmentManager] Invalid hotbar slot: {slotIndex}");
            return false;
        }
        
        ItemBaseSO item = GetItemFromInventory(itemID);
        if (item == null)
        {
            Debug.LogWarning($"[EquipmentManager] Item {itemID} not found");
            return false;
        }
        
        // Only potions (consumables) allowed in hotbar
        if (!(item is PotionSO))
        {
            Debug.LogWarning($"[EquipmentManager] Only potions can be equipped in hotbar");
            return false;
        }
        
        // Unequip current item
        UnequipHotbarItem(slotIndex);
        
        // Equip new item
        switch (slotIndex)
        {
            case 1:
                hotbarItem1 = item;
                break;
            case 2:
                hotbarItem2 = item;
                break;
            case 3:
                hotbarItem3 = item;
                break;
        }
        
        OnEquipmentChanged?.Invoke();
        
        if (debugMode)
            Debug.Log($"[EquipmentManager] Equipped {item.itemName} in hotbar slot {slotIndex}");
        
        return true;
    }
    
    public void UnequipHotbarItem(int slotIndex)
    {
        ItemBaseSO current = GetHotbarItem(slotIndex);
        if (current == null) return;
        
        switch (slotIndex)
        {
            case 1:
                hotbarItem1 = null;
                break;
            case 2:
                hotbarItem2 = null;
                break;
            case 3:
                hotbarItem3 = null;
                break;
        }
        
        OnEquipmentChanged?.Invoke();
        
        if (debugMode)
            Debug.Log($"[EquipmentManager] Unequipped {current.itemName} from hotbar slot {slotIndex}");
    }
    
    public ItemBaseSO GetHotbarItem(int slotIndex)
    {
        switch (slotIndex)
        {
            case 1: return hotbarItem1;
            case 2: return hotbarItem2;
            case 3: return hotbarItem3;
            default: return null;
        }
    }
    
    #endregion

    #region Utility
    
    /// <summary>
    /// Check if an item is currently equipped
    /// </summary>
    public bool IsItemEquipped(string itemID)
    {
        if (equippedWeapon != null && equippedWeapon.itemID == itemID) return true;
        if (equippedHelmet != null && equippedHelmet.itemID == itemID) return true;
        if (equippedChestPlate != null && equippedChestPlate.itemID == itemID) return true;
        if (equippedGloves != null && equippedGloves.itemID == itemID) return true;
        if (equippedBoots != null && equippedBoots.itemID == itemID) return true;
        if (equippedAccessory1 != null && equippedAccessory1.itemID == itemID) return true;
        if (equippedAccessory2 != null && equippedAccessory2.itemID == itemID) return true;
        if (normalSkill1 != null && normalSkill1.itemID == itemID) return true;
        if (normalSkill2 != null && normalSkill2.itemID == itemID) return true;
        if (ultimateSkill != null && ultimateSkill.itemID == itemID) return true;
        if (hotbarItem1 != null && hotbarItem1.itemID == itemID) return true;
        if (hotbarItem2 != null && hotbarItem2.itemID == itemID) return true;
        if (hotbarItem3 != null && hotbarItem3.itemID == itemID) return true;
        
        return false;
    }
    
    /// <summary>
    /// Get total item stats from all equipped gear
    /// </summary>
    public PlayerItemStats GetTotalItemStats()
    {
        PlayerItemStats stats = new PlayerItemStats();
        
        // Add weapon stats using AddStat method
        if (equippedWeapon != null)
        {
            stats.AddStat(BonusStatType.AttackDamage, equippedWeapon.bonusAD);
            stats.AddStat(BonusStatType.AbilityPower, equippedWeapon.bonusAP);
            stats.AddStat(BonusStatType.Health, equippedWeapon.bonusHP);
            stats.AddStat(BonusStatType.Defense, equippedWeapon.bonusDefense);
            stats.AddStat(BonusStatType.AttackSpeed, equippedWeapon.bonusAttackSpeed);
            stats.AddStat(BonusStatType.CritRate, equippedWeapon.bonusCritRate);
            stats.AddStat(BonusStatType.CritDamage, equippedWeapon.bonusCritDamage);
            stats.AddStat(BonusStatType.Evasion, equippedWeapon.bonusEvasion);
            stats.AddStat(BonusStatType.Tenacity, equippedWeapon.bonusTenacity);
            stats.AddStat(BonusStatType.Lethality, equippedWeapon.bonusLethality);
            stats.AddStat(BonusStatType.Penetration, equippedWeapon.bonusPenetration);
            stats.AddStat(BonusStatType.Lifesteal, equippedWeapon.bonusLifesteal);
        }
        
        // Add gear stats
        AddGearStats(equippedHelmet, stats);
        AddGearStats(equippedChestPlate, stats);
        AddGearStats(equippedGloves, stats);
        AddGearStats(equippedBoots, stats);
        AddGearStats(equippedAccessory1, stats);
        AddGearStats(equippedAccessory2, stats);
        
        return stats;
    }

    private void AddGearStats(GearSO gear, PlayerItemStats stats)
    {
        if (gear == null) return;
        
        // Use AddStat method instead of direct assignment
        if (gear.bonusHP > 0) stats.AddStat(BonusStatType.Health, gear.bonusHP);
        if (gear.bonusDefense > 0) stats.AddStat(BonusStatType.Defense, gear.bonusDefense);
        if (gear.bonusAD > 0) stats.AddStat(BonusStatType.AttackDamage, gear.bonusAD);
        if (gear.bonusAP > 0) stats.AddStat(BonusStatType.AbilityPower, gear.bonusAP);
        if (gear.bonusAttackSpeed > 0) stats.AddStat(BonusStatType.AttackSpeed, gear.bonusAttackSpeed);
        if (gear.bonusCritRate > 0) stats.AddStat(BonusStatType.CritRate, gear.bonusCritRate);
        if (gear.bonusCritDamage > 0) stats.AddStat(BonusStatType.CritDamage, gear.bonusCritDamage);
        if (gear.bonusEvasion > 0) stats.AddStat(BonusStatType.Evasion, gear.bonusEvasion);
        if (gear.bonusTenacity > 0) stats.AddStat(BonusStatType.Tenacity, gear.bonusTenacity);
        if (gear.bonusLethality > 0) stats.AddStat(BonusStatType.Lethality, gear.bonusLethality);
        if (gear.bonusPenetration > 0) stats.AddStat(BonusStatType.Penetration, gear.bonusPenetration);
        if (gear.bonusLifesteal > 0) stats.AddStat(BonusStatType.Lifesteal, gear.bonusLifesteal);
    }
    
    private ItemBaseSO GetItemFromInventory(string itemID)
    {
        if (InventoryManager.Instance == null) return null;
        return InventoryManager.Instance.Database.GetItem(itemID);
    }
    
    #endregion

    #region Debug
    
    [ContextMenu("Print All Equipment")]
    private void DebugPrintEquipment()
    {
        Debug.Log("=== CURRENT EQUIPMENT ===");
        Debug.Log($"Weapon: {(equippedWeapon != null ? equippedWeapon.weaponName : "None")}");
        Debug.Log($"Helmet: {(equippedHelmet != null ? equippedHelmet.gearName : "None")}");
        Debug.Log($"ChestPlate: {(equippedChestPlate != null ? equippedChestPlate.gearName : "None")}");
        Debug.Log($"Gloves: {(equippedGloves != null ? equippedGloves.gearName : "None")}");
        Debug.Log($"Boots: {(equippedBoots != null ? equippedBoots.gearName : "None")}");
        Debug.Log($"Accessory1: {(equippedAccessory1 != null ? equippedAccessory1.gearName : "None")}");
        Debug.Log($"Accessory2: {(equippedAccessory2 != null ? equippedAccessory2.gearName : "None")}");
        Debug.Log($"Normal Skill 1: {(normalSkill1 != null ? normalSkill1.skillName : "None")}");
        Debug.Log($"Normal Skill 2: {(normalSkill2 != null ? normalSkill2.skillName : "None")}");
        Debug.Log($"Ultimate: {(ultimateSkill != null ? ultimateSkill.skillName : "None")}");
    }
    
    #endregion
}

public enum GearSlotType
{
    Helmet,
    ChestPlate,
    Gloves,
    Boots,
    Accessory1,
    Accessory2
}

public enum SkillSlotType
{
    Normal1,
    Normal2,
    Ultimate
}