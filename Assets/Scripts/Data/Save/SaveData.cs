// ════════════════════════════════════════════
// Assets\Scripts\Data\Save\SaveData.cs
// Root save data structure for game persistence
// ════════════════════════════════════════════

using System;

namespace Ascension.Data.Save
{
    /// <summary>
    /// Root save data container
    /// </summary>
    [Serializable]
    public class SaveData
    {
        public SaveMetaData metaData;
        public CharacterSaveData characterData;
        public InventorySaveData inventoryData;
        public EquipmentSaveData equipmentData;
        public SkillLoadoutSaveData skillLoadoutData;
    }

    // ════════════════════════════════════════════
    // SaveMetaData - Pure data, no logic
    // ════════════════════════════════════════════

    /// <summary>
    /// Metadata about the save file
    /// All timestamps and version info set by SaveManager
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

    // ════════════════════════════════════════════
    // CharacterSaveData - Pure serializable data
    // ════════════════════════════════════════════

    /// <summary>
    /// Character state snapshot for persistence
    /// Maps to CharacterStats but remains serialization-friendly
    /// </summary>
    [Serializable]
    public class CharacterSaveData
    {
        public string playerName;
        public int level;
        public int currentExperience;
        public float currentHealth;
        public float currentMana;
        public int attributePoints;
        
        // Base Attributes (matching CharacterAttributes)
        public int strength;      // STR - Strength
        public int agility;       // AGI - Agility
        public int intelligence;  // INT - Intelligence
        public int endurance;     // END - Endurance
        public int wisdom;        // WIS - Wisdom
    }

    // ════════════════════════════════════════════
    // InventorySaveData - Pure serializable data
    // ✅ UPDATED: Added maxPocketSlots and maxStorageSlots
    // ════════════════════════════════════════════

    /// <summary>
    /// Inventory snapshot for persistence
    /// </summary>
    [Serializable]
    public class InventorySaveData
    {
        public ItemInstanceData[] items;
        public int maxBagSlots = 12;
        public int maxPocketSlots = 6;
        public int maxStorageSlots = 60;
    }

    /// <summary>
    /// Single item instance with location flags
    /// </summary>
    [Serializable]
    public class ItemInstanceData
    {
        public string itemId;
        public int quantity;
        
        // Location flags for proper persistence
        public bool isInBag;
        public bool isInPocket;
        // public bool isEquipped;
    }

    // ════════════════════════════════════════════
    // EquipmentSaveData - Pure serializable data
    // ════════════════════════════════════════════

    /// <summary>
    /// Equipped items snapshot for persistence
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

    // ════════════════════════════════════════════
    // SkillLoadoutSaveData - Pure serializable data
    // ════════════════════════════════════════════

    /// <summary>
    /// Skill loadout configuration snapshot for persistence
    /// </summary>
    [Serializable]
    public class SkillLoadoutSaveData
    {
        // Skill slots only (consumables removed)
        public string normalSkill1Id;
        public string normalSkill2Id;
        public string ultimateSkillId;
    }
}