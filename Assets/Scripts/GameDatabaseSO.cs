// ──────────────────────────────────────────────────
// GameDatabaseSO.cs
// Central database for all game items
// Provides categorized access and lookup by ID
// ──────────────────────────────────────────────────

using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[CreateAssetMenu(fileName = "DatabaseSO", menuName = "GameDatabase/DatabaseSO")]
public class GameDatabaseSO : ScriptableObject
{
    [Header("Items by Category (Inspector View)")]
    [SerializeField] private List<WeaponSO> weapons = new List<WeaponSO>();
    [SerializeField] private List<SkillSO> skills = new List<SkillSO>();
    [SerializeField] private List<PotionSO> potions = new List<PotionSO>();
    [SerializeField] private List<MaterialSO> materials = new List<MaterialSO>();
    [SerializeField] private List<ItemBaseSO> gear = new List<ItemBaseSO>();
    [SerializeField] private List<ItemBaseSO> misc = new List<ItemBaseSO>();

    // Cached dictionaries for fast lookup
    private Dictionary<string, ItemBaseSO> itemLookup;
    private List<ItemBaseSO> allItemsCache;

    public void Initialize()
    {
        // Build combined list from all categories
        allItemsCache = new List<ItemBaseSO>();
        allItemsCache.AddRange(weapons.Cast<ItemBaseSO>());
        allItemsCache.AddRange(skills.Cast<ItemBaseSO>());
        allItemsCache.AddRange(potions.Cast<ItemBaseSO>());
        allItemsCache.AddRange(materials.Cast<ItemBaseSO>());
        allItemsCache.AddRange(gear);
        allItemsCache.AddRange(misc);

        // Build item lookup
        itemLookup = new Dictionary<string, ItemBaseSO>();
        foreach (var item in allItemsCache)
        {
            if (item != null && !string.IsNullOrEmpty(item.itemID))
            {
                if (itemLookup.ContainsKey(item.itemID))
                {
                    Debug.LogWarning($"[GameDatabase] Duplicate itemID found: {item.itemID}");
                }
                else
                {
                    itemLookup[item.itemID] = item;
                }
            }
        }

        Debug.Log($"[GameDatabase] Initialized: {allItemsCache.Count} items " +
                  $"(Weapons: {weapons.Count}, Skills: {skills.Count}, " +
                  $"Potions: {potions.Count}, Materials: {materials.Count}, " +
                  $"Gear: {gear.Count}, Misc: {misc.Count})");
    }

    #region Item Queries

    /// <summary>
    /// Get item by ID (generic)
    /// </summary>
    public ItemBaseSO GetItem(string itemID)
    {
        if (itemLookup == null) Initialize();
        return itemLookup.TryGetValue(itemID, out var item) ? item : null;
    }

    /// <summary>
    /// Get all items (from all categories)
    /// </summary>
    public List<ItemBaseSO> GetAllItems()
    {
        if (allItemsCache == null) Initialize();
        return allItemsCache;
    }

    /// <summary>
    /// Get items by type
    /// </summary>
    public List<ItemBaseSO> GetItemsByType(ItemType type)
    {
        if (allItemsCache == null) Initialize();
        return allItemsCache.Where(item => item.itemType == type).ToList();
    }

    /// <summary>
    /// Get all weapons (typed)
    /// </summary>
    public List<WeaponSO> GetAllWeapons()
    {
        return new List<WeaponSO>(weapons);
    }

    /// <summary>
    /// Get all skills (typed)
    /// </summary>
    public List<SkillSO> GetAllSkills()
    {
        return new List<SkillSO>(skills);
    }

    /// <summary>
    /// Get all potions (typed)
    /// </summary>
    public List<PotionSO> GetAllPotions()
    {
        return new List<PotionSO>(potions);
    }

    /// <summary>
    /// Get all materials (typed)
    /// </summary>
    public List<MaterialSO> GetAllMaterials()
    {
        return new List<MaterialSO>(materials);
    }

    /// <summary>
    /// Get weapon by itemName
    /// </summary>
    public WeaponSO GetWeapon(string weaponName)
    {
        return weapons.FirstOrDefault(w => w.itemName == weaponName);
    }

    /// <summary>
    /// Get skill by itemName
    /// </summary>
    public SkillSO GetSkill(string skillName)
    {
        return skills.FirstOrDefault(s => s.itemName == skillName);
    }

    /// <summary>
    /// Get potion by itemName
    /// </summary>
    public PotionSO GetPotion(string potionName)
    {
        return potions.FirstOrDefault(p => p.itemName == potionName); // ✅ FIXED: use itemName
    }

    /// <summary>
    /// Get potions by type
    /// </summary>
    public List<PotionSO> GetPotionsByType(PotionType type)
    {
        return potions.Where(p => p.potionType == type).ToList();
    }

    /// <summary>
    /// Get materials by category
    /// </summary>
    public List<MaterialSO> GetMaterialsByCategory(MaterialCategory category)
    {
        return materials.Where(m => m.category == category).ToList();
    }

    /// <summary>
    /// Get skills compatible with a weapon type
    /// </summary>
    public List<SkillSO> GetSkillsForWeapon(WeaponType weaponType, SkillCategory category)
    {
        List<SkillSO> result = new List<SkillSO>();

        foreach (var skill in skills)
        {
            if (skill.category != category) continue;

            foreach (var compatibleType in skill.compatibleWeaponTypes)
            {
                if (compatibleType == weaponType)
                {
                    result.Add(skill);
                    break;
                }
            }
        }

        return result;
    }

    #endregion

    #region Editor Tools

    [ContextMenu("Auto-Populate All Categories")]
    public void AutoPopulateAllCategories()
    {
        ClearAllLists();

        // Load all ItemBaseSO from Resources folder
        ItemBaseSO[] items = Resources.LoadAll<ItemBaseSO>("");

        // Categorize each item
        foreach (var item in items)
        {
            if (item == null || item == this) continue;

            if (item is WeaponSO weapon)
            {
                weapons.Add(weapon);
            }
            else if (item is SkillSO skill)
            {
                skills.Add(skill);
            }
            else if (item is PotionSO potion)
            {
                potions.Add(potion);
            }
            else if (item is MaterialSO material) // ✅ Check for MaterialSO
            {
                materials.Add(material);
            }
            else
            {
                switch (item.itemType)
                {
                    case ItemType.Consumable:
                        misc.Add(item);
                        break;
                    case ItemType.Material:
                        // If not MaterialSO but marked as material, still add
                        Debug.LogWarning($"Item {item.itemName} is ItemType.Material but not MaterialSO");
                        misc.Add(item);
                        break;
                    case ItemType.Gear:
                        gear.Add(item);
                        break;
                    case ItemType.Misc:
                        misc.Add(item);
                        break;
                }
            }
        }

        // Sort each category by itemName
        weapons = weapons.OrderBy(w => w.itemName).ToList();
        skills = skills.OrderBy(s => s.itemName).ToList();
        potions = potions.OrderBy(p => p.itemName).ToList(); // ✅ FIXED
        materials = materials.OrderBy(m => m.itemName).ToList();
        gear = gear.OrderBy(i => i.itemName).ToList();
        misc = misc.OrderBy(i => i.itemName).ToList();

        #if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
        UnityEditor.AssetDatabase.SaveAssets();
        #endif

        int total = weapons.Count + skills.Count + potions.Count +
                    materials.Count + gear.Count + misc.Count;
        
        Debug.Log($"[GameDatabase] Auto-populated {total} items:");
        PrintCategorizedCount();
    }

    private void ClearAllLists()
    {
        weapons.Clear();
        skills.Clear();
        potions.Clear();
        materials.Clear();
        gear.Clear();
        misc.Clear();
    }

    [ContextMenu("Print Categorized Counts")]
    public void PrintCategorizedCount()
    {
        Debug.Log("=== ITEM BREAKDOWN ===");
        Debug.Log($"Weapons: {weapons.Count}");
        Debug.Log($"Skills: {skills.Count}");
        Debug.Log($"Potions: {potions.Count}");
        Debug.Log($"Materials: {materials.Count}");
        Debug.Log($"Gear: {gear.Count}");
        Debug.Log($"Misc: {misc.Count}");
        
        int total = weapons.Count + skills.Count + potions.Count +
                    materials.Count + gear.Count + misc.Count;
        Debug.Log($"Total: {total}");
    }

    [ContextMenu("Test Database")]
    public void TestDatabase()
    {
        Initialize();
        
        Debug.Log($"=== WEAPONS ({weapons.Count}) ===");
        foreach (var weapon in weapons)
        {
            Debug.Log($"  {weapon.itemName} - ID: {weapon.itemID}");
        }
        
        Debug.Log($"=== SKILLS ({skills.Count}) ===");
        foreach (var skill in skills)
        {
            Debug.Log($"  {skill.itemName} - ID: {skill.itemID}");
        }
        
        Debug.Log($"=== POTIONS ({potions.Count}) ===");
        foreach (var potion in potions)
        {
            Debug.Log($"  {potion.itemName} [{potion.potionType}] - ID: {potion.itemID}");
        }
        
        Debug.Log($"=== MATERIALS ({materials.Count}) ===");
        foreach (var material in materials)
        {
            Debug.Log($"  {material.itemName} [{material.category}] - ID: {material.itemID}");
        }

        PrintCategorizedCount();
    }
    
    [ContextMenu("Debug: Check Item Exists")]
    public void DebugCheckItem()
    {
        Initialize();
        
        Debug.Log("=== Checking if 'weapon_iron_bow' exists ===");
        ItemBaseSO item = GetItem("weapon_iron_bow");
        
        if (item != null)
        {
            Debug.Log($"✅ FOUND: {item.itemName}");
        }
        else
        {
            Debug.LogError("❌ NOT FOUND: weapon_iron_bow is not in the database!");
            Debug.Log($"Total items in database: {GetAllItems().Count}");
            Debug.Log("List of all itemIDs:");
            foreach (var i in GetAllItems())
            {
                Debug.Log($"  - {i.itemID} ({i.itemName})");
            }
        }
    }

    [ContextMenu("Validate: Check for Duplicates")]
    public void ValidateDuplicates()
    {
        if (allItemsCache == null) Initialize();
        
        HashSet<string> seenIDs = new HashSet<string>();
        List<string> duplicates = new List<string>();

        foreach (var item in allItemsCache)
        {
            if (string.IsNullOrEmpty(item.itemID))
            {
                Debug.LogWarning($"[GameDatabase] Item '{item.itemName}' has no itemID!");
                continue;
            }

            if (seenIDs.Contains(item.itemID))
            {
                duplicates.Add(item.itemID);
            }
            else
            {
                seenIDs.Add(item.itemID);
            }
        }

        if (duplicates.Count > 0)
        {
            Debug.LogError($"[GameDatabase] Found {duplicates.Count} duplicate itemIDs:");
            foreach (var dup in duplicates)
            {
                Debug.LogError($"  - {dup}");
            }
        }
        else
        {
            Debug.Log($"✅ [GameDatabase] No duplicates found! All {allItemsCache.Count} items have unique IDs.");
        }
    }

    #endregion
}