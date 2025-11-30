// -------------------------------
// GameDatabase.cs (Updated with PotionSO)
// -------------------------------
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[CreateAssetMenu(fileName = "GameDatabase", menuName = "Game/Database")]
public class GameDatabase : ScriptableObject
{
    [Header("Items by Category (Inspector View)")]
    [SerializeField] private List<WeaponSO> weapons = new List<WeaponSO>();
    [SerializeField] private List<SkillSO> skills = new List<SkillSO>();
    [SerializeField] private List<PotionSO> potions = new List<PotionSO>(); // Potions category 🆕 NEW
    [SerializeField] private List<ItemBaseSO> gear = new List<ItemBaseSO>(); // Helmets, chest, gloves, boots, accessories
    [SerializeField] private List<ItemBaseSO> materials = new List<ItemBaseSO>();
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
        allItemsCache.AddRange(potions.Cast<ItemBaseSO>()); // 🔧 CHANGED
        allItemsCache.AddRange(materials);
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
                  $"Potions: {potions.Count}, Materials: {materials.Count}, " + // 🔧 CHANGED
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
    /// Get all potions (typed) 🆕 NEW
    /// </summary>
    public List<PotionSO> GetAllPotions()
    {
        return new List<PotionSO>(potions);
    }

    /// <summary>
    /// Get weapon by name
    /// </summary>
    public WeaponSO GetWeapon(string weaponName)
    {
        return weapons.FirstOrDefault(w => w.weaponName == weaponName);
    }

    /// <summary>
    /// Get skill by name
    /// </summary>
    public SkillSO GetSkill(string skillName)
    {
        return skills.FirstOrDefault(s => s.skillName == skillName);
    }

    /// <summary>
    /// Get potion by name 🆕 NEW
    /// </summary>
    public PotionSO GetPotion(string potionName)
    {
        return potions.FirstOrDefault(p => p.potionName == potionName);
    }

    /// <summary>
    /// Get potions by type 🆕 NEW
    /// </summary>
    public List<PotionSO> GetPotionsByType(PotionType type)
    {
        return potions.Where(p => p.potionType == type).ToList();
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
            else if (item is PotionSO potion) // 🆕 NEW - check for PotionSO
            {
                potions.Add(potion);
            }
            else
            {
                switch (item.itemType)
                {
                    case ItemType.Consumable:
                        // Generic consumables (if not PotionSO) go to misc
                        misc.Add(item);
                        break;
                    case ItemType.Material:
                        materials.Add(item);
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

        // Sort each category
        weapons = weapons.OrderBy(w => w.weaponName).ToList();
        skills = skills.OrderBy(s => s.skillName).ToList();
        potions = potions.OrderBy(p => p.potionName).ToList(); // 🔧 CHANGED
        materials = materials.OrderBy(i => i.itemName).ToList();
        gear = gear.OrderBy(i => i.itemName).ToList();
        misc = misc.OrderBy(i => i.itemName).ToList();

        #if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
        UnityEditor.AssetDatabase.SaveAssets();
        #endif

        int total = weapons.Count + skills.Count + potions.Count + // 🔧 CHANGED
                    materials.Count + gear.Count + misc.Count;
        
        Debug.Log($"[GameDatabase] Auto-populated {total} items:");
        PrintCategorizedCount();
    }

    private void ClearAllLists()
    {
        weapons.Clear();
        skills.Clear();
        potions.Clear(); // 🔧 CHANGED
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
        Debug.Log($"Potions: {potions.Count}"); // 🔧 CHANGED
        Debug.Log($"Materials: {materials.Count}");
        Debug.Log($"Gear: {gear.Count}");
        Debug.Log($"Misc: {misc.Count}");
        
        int total = weapons.Count + skills.Count + potions.Count + // 🔧 CHANGED
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
            Debug.Log($"  {weapon.weaponName} [{weapon.weaponType}] - ID: {weapon.itemID}");
        }
        
        Debug.Log($"=== SKILLS ({skills.Count}) ===");
        foreach (var skill in skills)
        {
            Debug.Log($"  {skill.skillName} [{skill.category}] - ID: {skill.itemID}");
        }
        
        Debug.Log($"=== POTIONS ({potions.Count}) ==="); // 🔧 CHANGED
        foreach (var potion in potions)
        {
            Debug.Log($"  {potion.potionName} [{potion.potionType}] - ID: {potion.itemID}");
        }
        
        Debug.Log($"=== MATERIALS ({materials.Count}) ===");
        foreach (var item in materials)
        {
            Debug.Log($"  {item.itemName} - ID: {item.itemID}");
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