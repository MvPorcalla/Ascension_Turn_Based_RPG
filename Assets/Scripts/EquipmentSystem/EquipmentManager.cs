// ──────────────────────────────────────────────────
// EquipmentManager.cs
// Manages currently equipped items (weapons, gear, skills, consumables)
// Persists across scenes
// ──────────────────────────────────────────────────

using UnityEngine;
using System;
using System.Collections.Generic;
using Ascension.GameSystem;
using Ascension.Data.SO.Item;
using Ascension.Character.Stat;
using Ascension.Data.Enums;

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
    [SerializeField] private AbilitySO normalSkill1;
    [SerializeField] private AbilitySO normalSkill2;
    [SerializeField] private AbilitySO ultimateSkill;
    
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
    
    public AbilitySO GetNormalSkill1() => normalSkill1;
    public AbilitySO GetNormalSkill2() => normalSkill2;
    public AbilitySO GetUltimateSkill() => ultimateSkill;
    
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
            Debug.Log($"[EquipmentManager] Equipped weapon: {weapon.WeaponName}");
        
        return true;
    }
    
    public void UnequipWeapon()
    {
        if (equippedWeapon == null) return;
        
        if (debugMode)
            Debug.Log($"[EquipmentManager] Unequipped weapon: {equippedWeapon.WeaponName}");
        
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
            Debug.LogWarning($"[EquipmentManager] {gear.GearType} cannot be equipped in {slotType} slot");
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
            Debug.Log($"[EquipmentManager] Equipped {gear.GearName} in {slotType}");
        
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
            Debug.Log($"[EquipmentManager] Unequipped {currentGear.GearName} from {slotType}");
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
                return gear.GearType == GearType.Helmet;
            case GearSlotType.ChestPlate:
                return gear.GearType == GearType.ChestPlate;
            case GearSlotType.Gloves:
                return gear.GearType == GearType.Gloves;
            case GearSlotType.Boots:
                return gear.GearType == GearType.Boots;
            case GearSlotType.Accessory1:
            case GearSlotType.Accessory2:
                return gear.GearType == GearType.Accessory;
            default:
                return false;
        }
    }
    
    #endregion

    #region Equip/Unequip Skills
    
    public bool EquipSkill(string itemID, SkillSlotType slotType)
    {
        ItemBaseSO item = GetItemFromInventory(itemID);
        if (item == null || !(item is AbilitySO skill))
        {
            Debug.LogWarning($"[EquipmentManager] Item {itemID} is not a skill");
            return false;
        }
        
        // Validate skill category matches slot
        if (!IsSkillCompatibleWithSlot(skill, slotType))
        {
            Debug.LogWarning($"[EquipmentManager] {skill.Category} ability cannot be equipped in {slotType} slot");
            return false;
        }
        
        // Check weapon compatibility
        if (skill.CompatibleWeaponTypes != null && skill.CompatibleWeaponTypes.Length > 0)
        {
            if (equippedWeapon == null || !IsSkillCompatibleWithWeapon(skill, equippedWeapon))
            {
                Debug.LogWarning($"[EquipmentManager] Ability {skill.AbilityName} requires compatible weapon");
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
            Debug.Log($"[EquipmentManager] Equipped ability: {skill.AbilityName}");
        
        return true;
    }
    
    public void UnequipSkill(SkillSlotType slotType)
    {
        AbilitySO currentSkill = GetEquippedSkill(slotType);
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
            Debug.Log($"[EquipmentManager] Unequipped ability: {currentSkill.AbilityName}");
    }
    
    public AbilitySO GetEquippedSkill(SkillSlotType slotType)
    {
        switch (slotType)
        {
            case SkillSlotType.Normal1: return normalSkill1;
            case SkillSlotType.Normal2: return normalSkill2;
            case SkillSlotType.Ultimate: return ultimateSkill;
            default: return null;
        }
    }
    
    private bool IsSkillCompatibleWithSlot(AbilitySO skill, SkillSlotType slotType)
    {
        switch (slotType)
        {
            case SkillSlotType.Normal1:
            case SkillSlotType.Normal2:
                return skill.Category == AbilityCategory.Normal || skill.Category == AbilityCategory.Weapon;
            case SkillSlotType.Ultimate:
                return skill.Category == AbilityCategory.Ultimate;
            default:
                return false;
        }
    }
    
    private bool IsSkillCompatibleWithWeapon(AbilitySO skill, WeaponSO weapon)
    {
        if (skill.CompatibleWeaponTypes == null || skill.CompatibleWeaponTypes.Length == 0)
            return true;
        
        foreach (var weaponType in skill.CompatibleWeaponTypes)
        {
            if (weapon.WeaponType == weaponType)
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
            Debug.Log($"[EquipmentManager] Equipped {item.ItemName} in hotbar slot {slotIndex}");
        
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
            Debug.Log($"[EquipmentManager] Unequipped {current.ItemName} from hotbar slot {slotIndex}");
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

    #region Save/Load (NEW)

    public EquipmentData SaveEquipment()
    {
        return new EquipmentData
        {
            equippedWeaponID = equippedWeapon?.ItemID,
            equippedHelmetID = equippedHelmet?.ItemID,
            equippedChestPlateID = equippedChestPlate?.ItemID,
            equippedGlovesID = equippedGloves?.ItemID,
            equippedBootsID = equippedBoots?.ItemID,
            equippedAccessory1ID = equippedAccessory1?.ItemID,
            equippedAccessory2ID = equippedAccessory2?.ItemID,
            normalSkill1ID = normalSkill1?.ItemID,
            normalSkill2ID = normalSkill2?.ItemID,
            ultimateSkillID = ultimateSkill?.ItemID,
            hotbarItem1ID = hotbarItem1?.ItemID,
            hotbarItem2ID = hotbarItem2?.ItemID,
            hotbarItem3ID = hotbarItem3?.ItemID
        };
    }

    public void LoadEquipment(EquipmentData data)
    {
        if (data == null) return;
        
        // Load weapon
        if (!string.IsNullOrEmpty(data.equippedWeaponID))
            EquipWeapon(data.equippedWeaponID);
        
        // Load gear
        if (!string.IsNullOrEmpty(data.equippedHelmetID))
            EquipGear(data.equippedHelmetID, GearSlotType.Helmet);
        if (!string.IsNullOrEmpty(data.equippedChestPlateID))
            EquipGear(data.equippedChestPlateID, GearSlotType.ChestPlate);
        if (!string.IsNullOrEmpty(data.equippedGlovesID))
            EquipGear(data.equippedGlovesID, GearSlotType.Gloves);
        if (!string.IsNullOrEmpty(data.equippedBootsID))
            EquipGear(data.equippedBootsID, GearSlotType.Boots);
        if (!string.IsNullOrEmpty(data.equippedAccessory1ID))
            EquipGear(data.equippedAccessory1ID, GearSlotType.Accessory1);
        if (!string.IsNullOrEmpty(data.equippedAccessory2ID))
            EquipGear(data.equippedAccessory2ID, GearSlotType.Accessory2);
        
        // Load skills
        if (!string.IsNullOrEmpty(data.normalSkill1ID))
            EquipSkill(data.normalSkill1ID, SkillSlotType.Normal1);
        if (!string.IsNullOrEmpty(data.normalSkill2ID))
            EquipSkill(data.normalSkill2ID, SkillSlotType.Normal2);
        if (!string.IsNullOrEmpty(data.ultimateSkillID))
            EquipSkill(data.ultimateSkillID, SkillSlotType.Ultimate);
        
        // Load hotbar
        if (!string.IsNullOrEmpty(data.hotbarItem1ID))
            EquipHotbarItem(data.hotbarItem1ID, 1);
        if (!string.IsNullOrEmpty(data.hotbarItem2ID))
            EquipHotbarItem(data.hotbarItem2ID, 2);
        if (!string.IsNullOrEmpty(data.hotbarItem3ID))
            EquipHotbarItem(data.hotbarItem3ID, 3);
        
        Debug.Log("[EquipmentManager] Equipment loaded from save");
    }

    #endregion

    #region Utility
    
    /// <summary>
    /// Check if an item is currently equipped
    /// </summary>
    public bool IsItemEquipped(string itemID)
    {
        if (equippedWeapon != null && equippedWeapon.ItemID == itemID) return true;
        if (equippedHelmet != null && equippedHelmet.ItemID == itemID) return true;
        if (equippedChestPlate != null && equippedChestPlate.ItemID == itemID) return true;
        if (equippedGloves != null && equippedGloves.ItemID == itemID) return true;
        if (equippedBoots != null && equippedBoots.ItemID == itemID) return true;
        if (equippedAccessory1 != null && equippedAccessory1.ItemID == itemID) return true;
        if (equippedAccessory2 != null && equippedAccessory2.ItemID == itemID) return true;
        if (normalSkill1 != null && normalSkill1.ItemID == itemID) return true;
        if (normalSkill2 != null && normalSkill2.ItemID == itemID) return true;
        if (ultimateSkill != null && ultimateSkill.ItemID == itemID) return true;
        if (hotbarItem1 != null && hotbarItem1.ItemID == itemID) return true;
        if (hotbarItem2 != null && hotbarItem2.ItemID == itemID) return true;
        if (hotbarItem3 != null && hotbarItem3.ItemID == itemID) return true;
        
        return false;
    }
    
    /// <summary>
    /// Get total item stats from all equipped gear
    /// </summary>
    public CharacterItemStats GetTotalItemStats()
    {
        CharacterItemStats stats = new CharacterItemStats();

        // Add weapon stats using public properties
        if (equippedWeapon != null)
        {
            stats.AddStat(BonusStatType.AttackDamage, equippedWeapon.BonusAD);
            stats.AddStat(BonusStatType.AbilityPower, equippedWeapon.BonusAP);
            stats.AddStat(BonusStatType.Health, equippedWeapon.BonusHP);
            stats.AddStat(BonusStatType.Defense, equippedWeapon.BonusDefense);
            stats.AddStat(BonusStatType.AttackSpeed, equippedWeapon.BonusAttackSpeed);
            stats.AddStat(BonusStatType.CritRate, equippedWeapon.BonusCritRate);
            stats.AddStat(BonusStatType.CritDamage, equippedWeapon.BonusCritDamage);
            stats.AddStat(BonusStatType.Evasion, equippedWeapon.BonusEvasion);
            stats.AddStat(BonusStatType.Tenacity, equippedWeapon.BonusTenacity);
            stats.AddStat(BonusStatType.Lethality, equippedWeapon.BonusLethality);
            stats.AddStat(BonusStatType.Penetration, equippedWeapon.BonusPenetration);
            stats.AddStat(BonusStatType.Lifesteal, equippedWeapon.BonusLifesteal);
        }

        // Add gear stats using public properties
        AddGearStats(equippedHelmet, stats);
        AddGearStats(equippedChestPlate, stats);
        AddGearStats(equippedGloves, stats);
        AddGearStats(equippedBoots, stats);
        AddGearStats(equippedAccessory1, stats);
        AddGearStats(equippedAccessory2, stats);

        return stats;
    }

    private void AddGearStats(GearSO gear, CharacterItemStats stats)
    {
        if (gear == null) return;

        if (gear.BonusHP > 0) stats.AddStat(BonusStatType.Health, gear.BonusHP);
        if (gear.BonusDefense > 0) stats.AddStat(BonusStatType.Defense, gear.BonusDefense);
        if (gear.BonusAD > 0) stats.AddStat(BonusStatType.AttackDamage, gear.BonusAD);
        if (gear.BonusAP > 0) stats.AddStat(BonusStatType.AbilityPower, gear.BonusAP);
        if (gear.BonusAttackSpeed > 0) stats.AddStat(BonusStatType.AttackSpeed, gear.BonusAttackSpeed);
        if (gear.BonusCritRate > 0) stats.AddStat(BonusStatType.CritRate, gear.BonusCritRate);
        if (gear.BonusCritDamage > 0) stats.AddStat(BonusStatType.CritDamage, gear.BonusCritDamage);
        if (gear.BonusEvasion > 0) stats.AddStat(BonusStatType.Evasion, gear.BonusEvasion);
        if (gear.BonusTenacity > 0) stats.AddStat(BonusStatType.Tenacity, gear.BonusTenacity);
        if (gear.BonusLethality > 0) stats.AddStat(BonusStatType.Lethality, gear.BonusLethality);
        if (gear.BonusPenetration > 0) stats.AddStat(BonusStatType.Penetration, gear.BonusPenetration);
        if (gear.BonusLifesteal > 0) stats.AddStat(BonusStatType.Lifesteal, gear.BonusLifesteal);
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
        Debug.Log($"Weapon: {(equippedWeapon != null ? equippedWeapon.WeaponName : "None")}");
        Debug.Log($"Helmet: {(equippedHelmet != null ? equippedHelmet.GearName : "None")}");
        Debug.Log($"ChestPlate: {(equippedChestPlate != null ? equippedChestPlate.GearName : "None")}");
        Debug.Log($"Gloves: {(equippedGloves != null ? equippedGloves.GearName : "None")}");
        Debug.Log($"Boots: {(equippedBoots != null ? equippedBoots.GearName : "None")}");
        Debug.Log($"Accessory1: {(equippedAccessory1 != null ? equippedAccessory1.GearName : "None")}");
        Debug.Log($"Accessory2: {(equippedAccessory2 != null ? equippedAccessory2.GearName : "None")}");
        Debug.Log($"Normal Skill 1: {(normalSkill1 != null ? normalSkill1.AbilityName : "None")}");
        Debug.Log($"Normal Skill 2: {(normalSkill2 != null ? normalSkill2.AbilityName : "None")}");
        Debug.Log($"Ultimate: {(ultimateSkill != null ? ultimateSkill.AbilityName : "None")}");
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