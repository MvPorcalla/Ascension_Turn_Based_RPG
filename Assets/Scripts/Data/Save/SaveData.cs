// ════════════════════════════════════════════════════════════════════════
// Assets/Scripts/Data/Save/SaveData.cs
// ✅ REFACTORED: Full DTO pattern - clean separation of runtime vs save data
// ════════════════════════════════════════════════════════════════════════

using System;
using Ascension.Inventory.Config;

namespace Ascension.Data.Save
{
    /// <summary>
    /// Main save file container - all game state in one place
    /// Uses DTOs (Data Transfer Objects) for clean separation from runtime classes
    /// </summary>
    [Serializable]
    public class SaveData
    {
        public SaveMetaData metaData;
        public CharacterSaveData characterData;     // ✅ DTO - not runtime CharacterStats
        public InventorySaveData inventoryData;
        public EquipmentSaveData equipmentData;
        public SkillLoadoutSaveData skillLoadoutData;
    }

    // ════════════════════════════════════════════════════════════════════════
    // METADATA DTO
    // ════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Save file metadata (version, timestamps, playtime)
    /// </summary>
    [Serializable]
    public class SaveMetaData
    {
        public string saveVersion;
        public string createdTime;
        public string lastSaveTime;
        public float totalPlayTimeSeconds;
        public int saveCount;
    }

    // ════════════════════════════════════════════════════════════════════════
    // CHARACTER DTO
    // ════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// ✅ Character data transfer object
    /// Separates runtime CharacterStats from save file structure
    /// Benefits: Version migration, backward compatibility, data validation
    /// </summary>
    [Serializable]
    public class CharacterSaveData
    {
        // ─── Identity ───
        public string playerName;

        // ─── Level & Progression ───
        public int level;
        public int currentExperience;
        public int attributePoints;

        // ─── Core Attributes ───
        public int strength;
        public int agility;
        public int intelligence;
        public int endurance;
        public int wisdom;

        // ─── Combat Runtime State ───
        public float currentHealth;
        public float currentMana;

        // ─── Future Expansion ───
        // public string currentTitle;
        // public int prestigeLevel;
        // public long totalGoldEarned;
    }

    // ════════════════════════════════════════════════════════════════════════
    // INVENTORY DTO
    // ════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Inventory save data - items across all locations
    /// </summary>
    [Serializable]
    public class InventorySaveData
    {
        public ItemInstanceData[] items;
        public int maxBagSlots = InventoryConfig.DEFAULT_BAG_SLOTS;
        public int maxStorageSlots = InventoryConfig.DEFAULT_STORAGE_SLOTS;
    }

    /// <summary>
    /// ✅ Individual item instance DTO
    /// location values: 0=Storage, 2=Bag, 3=Equipped
    /// previousLocation: -1=None/null, 0=Storage, 2=Bag
    /// </summary>
    [Serializable]
    public class ItemInstanceData
    {
        public string itemId;
        public int quantity;
        public int location;          // Current location (enum cast to int)
        public int previousLocation;  // Where item was before equipping (-1 = none)
    }

    // ════════════════════════════════════════════════════════════════════════
    // EQUIPMENT DTO
    // ════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Equipment save data - equipped item IDs per slot
    /// </summary>
    [Serializable]
    public class EquipmentSaveData
    {
        public string weaponId;
        public string helmetId;
        public string chestId;
        public string glovesId;
        public string bootsId;
        public string accessory1Id;
        public string accessory2Id;
    }

    // ════════════════════════════════════════════════════════════════════════
    // SKILL LOADOUT DTO
    // ════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Skill loadout save data - equipped skill IDs per slot
    /// </summary>
    [Serializable]
    public class SkillLoadoutSaveData
    {
        public string normalSkill1Id;
        public string normalSkill2Id;
        public string ultimateSkillId;
    }
}