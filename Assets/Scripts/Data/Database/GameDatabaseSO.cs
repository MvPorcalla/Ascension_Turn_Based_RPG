// ════════════════════════════════════════════
// GameDatabaseSO.cs
// Central database for all game items, providing categorized access and fast lookup by ID
// ════════════════════════════════════════════

using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Ascension.Data.Enums;

namespace Ascension.Data.SO.Database
{
    [CreateAssetMenu(fileName = "DatabaseSO", menuName = "GameDatabase/DatabaseSO")]
    public class GameDatabaseSO : ScriptableObject
    {
        #region Serialized Fields
        [Header("Items by Category (Inspector View)")]
        [SerializeField] private List<WeaponSO> weapons = new List<WeaponSO>();
        [SerializeField] private List<AbilitySO> abilities = new List<AbilitySO>();   // Renamed FROM SkillSO
        [SerializeField] private List<PotionSO> potions = new List<PotionSO>();
        [SerializeField] private List<MaterialSO> materials = new List<MaterialSO>();
        [SerializeField] private List<ItemBaseSO> gear = new List<ItemBaseSO>();
        [SerializeField] private List<ItemBaseSO> misc = new List<ItemBaseSO>();
        #endregion

        #region Private Fields
        private Dictionary<string, ItemBaseSO> itemLookup;
        private List<ItemBaseSO> allItemsCache;
        #endregion

        #region Public Methods
        public void Initialize()
        {
            BuildAllItemsCache();
            BuildItemLookup();
            LogInitialization();
        }

        public ItemBaseSO GetItem(string itemID)
        {
            if (itemLookup == null) Initialize();
            itemLookup.TryGetValue(itemID, out var item);
            return item;
        }

        public List<ItemBaseSO> GetAllItems()
        {
            if (allItemsCache == null) Initialize();
            return allItemsCache;
        }

        public List<ItemBaseSO> GetItemsByType(ItemType type)
        {
            if (allItemsCache == null) Initialize();
            return allItemsCache.Where(item => item.ItemType == type).ToList();
        }

        public List<WeaponSO> GetAllWeapons() => new List<WeaponSO>(weapons);
        public List<AbilitySO> GetAllAbilities() => new List<AbilitySO>(abilities);
        public List<PotionSO> GetAllPotions() => new List<PotionSO>(potions);
        public List<MaterialSO> GetAllMaterials() => new List<MaterialSO>(materials);

        public WeaponSO GetWeapon(string weaponName) => weapons.FirstOrDefault(w => w.ItemName == weaponName);
        public AbilitySO GetAbility(string abilityName) => abilities.FirstOrDefault(a => a.ItemName == abilityName);
        public PotionSO GetPotion(string potionName) => potions.FirstOrDefault(p => p.ItemName == potionName);

        public List<PotionSO> GetPotionsByType(PotionType type) => potions.Where(p => p.potionType == type).ToList();

        public List<MaterialSO> GetMaterialsByCategory(MaterialCategory category) =>
            materials.Where(m => m.category == category).ToList();

        public List<AbilitySO> GetAbilitiesForWeapon(WeaponType weaponType, AbilityCategory category)
        {
            return abilities
                .Where(a => a.Category == category && a.CompatibleWeaponTypes.Contains(weaponType))
                .ToList();
        }
        #endregion

        #region Editor Tools
#if UNITY_EDITOR
        [ContextMenu("Auto-Populate All Categories")]
        public void AutoPopulateAllCategories()
        {
            ClearAllLists();

            ItemBaseSO[] items = Resources.LoadAll<ItemBaseSO>("");

            foreach (var item in items)
            {
                if (item == null || item == this) continue;

                switch (item)
                {
                    case WeaponSO weapon: weapons.Add(weapon); break;
                    case AbilitySO ability: abilities.Add(ability); break;
                    case PotionSO potion: potions.Add(potion); break;
                    case MaterialSO material: materials.Add(material); break;
                    default:
                        switch (item.ItemType)
                        {
                            case ItemType.Consumable: misc.Add(item); break;
                            case ItemType.Gear: gear.Add(item); break;
                            case ItemType.Material:
                            case ItemType.Misc: misc.Add(item); break;
                        }
                        break;
                }
            }

            weapons = weapons.OrderBy(w => w.ItemName).ToList();
            abilities = abilities.OrderBy(a => a.ItemName).ToList();
            potions = potions.OrderBy(p => p.ItemName).ToList();
            materials = materials.OrderBy(m => m.ItemName).ToList();
            gear = gear.OrderBy(g => g.ItemName).ToList();
            misc = misc.OrderBy(m => m.ItemName).ToList();

            UnityEditor.EditorUtility.SetDirty(this);
            UnityEditor.AssetDatabase.SaveAssets();

            Debug.Log($"[GameDatabase] Auto-populated {GetAllItems().Count} items");
            PrintCategorizedCount();
        }

        [ContextMenu("Print Categorized Counts")]
        public void PrintCategorizedCount()
        {
            Debug.Log("=== ITEM BREAKDOWN ===");
            Debug.Log($"Weapons: {weapons.Count}");
            Debug.Log($"Abilities: {abilities.Count}");
            Debug.Log($"Potions: {potions.Count}");
            Debug.Log($"Materials: {materials.Count}");
            Debug.Log($"Gear: {gear.Count}");
            Debug.Log($"Misc: {misc.Count}");
            Debug.Log($"Total: {GetAllItems().Count}");
        }

        [ContextMenu("Test Database")]
        public void TestDatabase()
        {
            Initialize();

            DebugLogCategory("WEAPONS", weapons);
            DebugLogCategory("ABILITIES", abilities);
            DebugLogCategory("POTIONS", potions);
            DebugLogCategory("MATERIALS", materials);

            PrintCategorizedCount();
        }

        [ContextMenu("Debug: Check Item Exists")]
        public void DebugCheckItem()
        {
            Initialize();
            string testID = "weapon_iron_bow";
            ItemBaseSO item = GetItem(testID);

            if (item != null)
                Debug.Log($"✅ FOUND: {item.ItemName}");
            else
                Debug.LogError($"❌ NOT FOUND: {testID} in database! Total items: {GetAllItems().Count}");
        }

        [ContextMenu("Validate: Check for Duplicates")]
        public void ValidateDuplicates()
        {
            if (allItemsCache == null) Initialize();

            HashSet<string> seenIDs = new HashSet<string>();
            List<string> duplicates = new List<string>();

            foreach (var item in allItemsCache)
            {
                if (string.IsNullOrEmpty(item.ItemID))
                {
                    Debug.LogWarning($"Item '{item.ItemName}' has no itemID!");
                    continue;
                }

                if (!seenIDs.Add(item.ItemID)) duplicates.Add(item.ItemID);
            }

            if (duplicates.Count > 0)
            {
                Debug.LogError($"[GameDatabase] Found {duplicates.Count} duplicate itemIDs:");
                foreach (var dup in duplicates) Debug.LogError($"  - {dup}");
            }
            else
            {
                Debug.Log($"✅ [GameDatabase] All {allItemsCache.Count} items have unique IDs.");
            }
        }
#endif
        #endregion

        #region Private Methods
        private void BuildAllItemsCache()
        {
            allItemsCache = new List<ItemBaseSO>();
            allItemsCache.AddRange(weapons.Cast<ItemBaseSO>());
            allItemsCache.AddRange(abilities.Cast<ItemBaseSO>());
            allItemsCache.AddRange(potions.Cast<ItemBaseSO>());
            allItemsCache.AddRange(materials.Cast<ItemBaseSO>());
            allItemsCache.AddRange(gear);
            allItemsCache.AddRange(misc);
        }

        private void BuildItemLookup()
        {
            itemLookup = new Dictionary<string, ItemBaseSO>();

            foreach (var item in allItemsCache)
            {
                if (item == null || string.IsNullOrEmpty(item.ItemID)) continue;

                if (!itemLookup.ContainsKey(item.ItemID))
                    itemLookup[item.ItemID] = item;
                else
                    Debug.LogWarning($"Duplicate itemID found: {item.ItemID}");
            }
        }

        private void ClearAllLists()
        {
            weapons.Clear();
            abilities.Clear();
            potions.Clear();
            materials.Clear();
            gear.Clear();
            misc.Clear();
        }

        private void LogInitialization()
        {
            Debug.Log($"[GameDatabase] Initialized: {allItemsCache.Count} items " +
                      $"(Weapons: {weapons.Count}, Abilities: {abilities.Count}, " +
                      $"Potions: {potions.Count}, Materials: {materials.Count}, " +
                      $"Gear: {gear.Count}, Misc: {misc.Count})");
        }

        private void DebugLogCategory<T>(string categoryName, List<T> items) where T : ItemBaseSO
        {
            Debug.Log($"=== {categoryName} ({items.Count}) ===");
            foreach (var item in items)
            {
                Debug.Log($"  {item.ItemName} - ID: {item.ItemID}");
            }
        }
        #endregion
    }
}
