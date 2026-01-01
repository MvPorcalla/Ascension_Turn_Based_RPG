// ════════════════════════════════════════════
// Assets\Scripts\AppFlow\SaveController.cs
// Save and load game state management
// ════════════════════════════════════════════

using System;
using UnityEngine;
using Ascension.Core;
using Ascension.Data.Save;
using Ascension.Character.Stat;
using Ascension.Character.Manager;
using Ascension.Inventory.Manager;
using Ascension.Inventory.Data;
using Ascension.Inventory.Enums;
using Ascension.Equipment.Manager;
using Ascension.Inventory.Config;

namespace Ascension.App
{
    public class SaveController : MonoBehaviour, IGameService
    {
        #region Injected Dependencies
        private CharacterManager _characterManager;
        private SaveManager _saveManager;
        private InventoryManager _inventoryManager;
        private EquipmentManager _equipmentManager;
        private SkillLoadoutManager _skillLoadoutManager;
        #endregion

        #region Events
        public event System.Action OnSaveCompleted;
        public event System.Action OnLoadCompleted;
        #endregion

        #region Initialization
        public void Initialize()
        {
            InjectDependencies();
        }

        private void InjectDependencies()
        {
            var container = ServiceContainer.Instance;
            
            if (container == null)
            {
                Debug.LogError("[SaveController] ServiceContainer not available!");
                return;
            }

            _characterManager = container.Get<CharacterManager>();
            _saveManager = container.Get<SaveManager>();
            _inventoryManager = container.Get<InventoryManager>();
            _equipmentManager = container.Get<EquipmentManager>();
            _skillLoadoutManager = container.Get<SkillLoadoutManager>();

            ValidateDependencies();
        }

        private void ValidateDependencies()
        {
            if (_characterManager == null)
                Debug.LogError("[SaveController] CharacterManager not found!");
            
            if (_saveManager == null)
                Debug.LogError("[SaveController] SaveManager not found!");
            
            if (_inventoryManager == null)
                Debug.LogError("[SaveController] InventoryManager not found!");
            
            if (_equipmentManager == null)
                Debug.LogError("[SaveController] EquipmentManager not found!");
            
            if (_skillLoadoutManager == null)
                Debug.LogError("[SaveController] SkillLoadoutManager not found!");
        }
        #endregion

        #region Public Methods - Save/Load
        public bool SaveGame(float sessionPlayTime)
        {
            if (_characterManager?.CurrentPlayer == null)
            {
                Debug.LogError("[SaveController] No active player to save!");
                return false;
            }

            CharacterSaveData charData = ConvertToCharacterSaveData(_characterManager.CurrentPlayer);
            InventorySaveData invData = ConvertToInventorySaveData();
            EquipmentSaveData eqData = ConvertToEquipmentSaveData();
            SkillLoadoutSaveData skillData = ConvertToSkillLoadoutSaveData();
            
            bool success = _saveManager.SaveGame(
                charData, 
                invData, 
                eqData, 
                skillData,
                sessionPlayTime
            );
            
            if (success)
            {
                OnSaveCompleted?.Invoke();
                Debug.Log("[SaveController] Game saved successfully");
            }
            
            return success;
        }

        public bool LoadGame()
        {
            SaveData saveData = _saveManager.LoadGame();
            
            if (saveData == null)
            {
                Debug.LogError("[SaveController] Failed to load save data");
                return false;
            }
            
            // Load order matters
            LoadInventoryFromSave(saveData);
            LoadEquipmentFromSave(saveData);
            LoadPlayerFromSave(saveData);
            LoadSkillLoadoutFromSave(saveData);
            
            OnLoadCompleted?.Invoke();
            Debug.Log("[SaveController] Game loaded successfully");
            
            return true;
        }

        public bool SaveExists() => _saveManager != null && _saveManager.SaveExists();

        public bool DeleteSave()
        {
            if (_saveManager == null) return false;
            
            bool success = _saveManager.DeleteSave();
            
            if (success && _characterManager != null)
            {
                _characterManager.UnloadPlayer();
            }
            
            return success;
        }
        #endregion

        #region Private Methods - Character Conversion
        private CharacterSaveData ConvertToCharacterSaveData(CharacterStats stats)
        {
            return new CharacterSaveData
            {
                playerName = stats.playerName,
                level = stats.Level,
                currentExperience = stats.CurrentEXP,
                currentHealth = stats.CurrentHP,
                currentMana = 0f,
                attributePoints = stats.UnallocatedPoints,
                
                strength = stats.attributes.STR,
                agility = stats.attributes.AGI,
                intelligence = stats.attributes.INT,
                endurance = stats.attributes.END,
                wisdom = stats.attributes.WIS
            };
        }

        private void LoadPlayerFromSave(SaveData saveData)
        {
            CharacterStats stats = new CharacterStats();
            stats.Initialize(_characterManager.BaseStats);
            
            stats.playerName = saveData.characterData.playerName;
            stats.guildRank = "Unranked";
            
            stats.levelSystem.level = saveData.characterData.level;
            stats.levelSystem.currentEXP = saveData.characterData.currentExperience;
            stats.levelSystem.unallocatedPoints = saveData.characterData.attributePoints;
            
            stats.attributes.STR = saveData.characterData.strength;
            stats.attributes.AGI = saveData.characterData.agility;
            stats.attributes.INT = saveData.characterData.intelligence;
            stats.attributes.END = saveData.characterData.endurance;
            stats.attributes.WIS = saveData.characterData.wisdom;
            
            stats.RecalculateStats(_characterManager.BaseStats, fullHeal: false);
            stats.combatRuntime.currentHP = saveData.characterData.currentHealth;
            
            _characterManager.LoadPlayer(stats);
            
            Debug.Log($"[SaveController] ✓ Loaded character: {stats.playerName}");
        }
        #endregion

        #region Private Methods - Inventory Conversion (✅ MIGRATED)
        
        /// <summary>
        /// ✅ MIGRATED: Convert ItemInstance (enum) → ItemInstanceData (int)
        /// </summary>
        private InventorySaveData ConvertToInventorySaveData()
        {
            if (_inventoryManager == null)
            {
                return new InventorySaveData 
                { 
                    items = new ItemInstanceData[0], 
                    maxBagSlots = 12,
                    maxPocketSlots = 6,
                    maxStorageSlots = 60
                };
            }

            InventoryCoreData bagData = _inventoryManager.SaveInventory();

            if (bagData == null || bagData.items == null)
            {
                Debug.LogError("[SaveController] Failed to get inventory data");
                return new InventorySaveData 
                { 
                    items = Array.Empty<ItemInstanceData>(),
                    maxBagSlots = InventoryConfig.DEFAULT_BAG_SLOTS,
                    maxPocketSlots = InventoryConfig.DEFAULT_POCKET_SLOTS,
                    maxStorageSlots = InventoryConfig.DEFAULT_STORAGE_SLOTS
                };
            }
            
            ItemInstanceData[] itemArray = new ItemInstanceData[bagData.items.Count];
            
            for (int i = 0; i < bagData.items.Count; i++)
            {
                ItemInstance item = bagData.items[i];
                
                // ✅ MIGRATED: enum → int conversion
                itemArray[i] = new ItemInstanceData
                {
                    itemId = item.itemID,
                    quantity = item.quantity,
                    location = (int)item.location  // ✅ Convert enum to int
                };
            }
            
            return new InventorySaveData
            {
                items = itemArray,
                maxBagSlots = bagData.maxBagSlots,
                maxPocketSlots = bagData.maxPocketSlots,
                maxStorageSlots = bagData.maxStorageSlots
            };
        }

        /// <summary>
        /// ✅ MIGRATED: Convert ItemInstanceData (int) → ItemInstance (enum)
        /// </summary>
        private void LoadInventoryFromSave(SaveData saveData)
        {
            if (_inventoryManager == null)
            {
                Debug.LogWarning("[SaveController] InventoryManager not available");
                return;
            }

            if (saveData.inventoryData == null)
            {
                Debug.LogWarning("[SaveController] No inventory data in save");
                return;
            }

            InventoryCoreData bagData = new InventoryCoreData
            {
                maxBagSlots = saveData.inventoryData.maxBagSlots,
                maxPocketSlots = saveData.inventoryData.maxPocketSlots,
                maxStorageSlots = saveData.inventoryData.maxStorageSlots,
                items = new System.Collections.Generic.List<ItemInstance>()
            };
            
            // ✅ MIGRATED: int → enum conversion
            foreach (var itemData in saveData.inventoryData.items)
            {
                ItemLocation location = (ItemLocation)itemData.location;  // ✅ Convert int to enum
                
                ItemInstance item = new ItemInstance(
                    itemData.itemId, 
                    itemData.quantity, 
                    location  // ✅ Pass enum directly
                );
                
                bagData.items.Add(item);
            }
            
            _inventoryManager.LoadInventory(bagData);

            Debug.Log($"[SaveController] ✓ Loaded inventory: {bagData.items.Count} items");
            
            // ✅ Log distribution
            int bagCount = 0, pocketCount = 0, storageCount = 0;
            foreach (var item in bagData.items)
            {
                switch (item.location)
                {
                    case ItemLocation.Bag: bagCount++; break;
                    case ItemLocation.Pocket: pocketCount++; break;
                    case ItemLocation.Storage: storageCount++; break;
                }
            }
            Debug.Log($"[SaveController]   Distribution - Bag:{bagCount} Pocket:{pocketCount} Storage:{storageCount}");
        }
        #endregion

        #region Private Methods - Equipment Conversion
        private EquipmentSaveData ConvertToEquipmentSaveData()
        {
            if (_equipmentManager != null)
            {
                return _equipmentManager.SaveEquipment();
            }

            return new EquipmentSaveData
            {
                weaponId = string.Empty,
                helmetId = string.Empty,
                chestId = string.Empty,
                glovesId = string.Empty,
                bootsId = string.Empty,
                accessory1Id = string.Empty,
                accessory2Id = string.Empty
            };
        }

        private void LoadEquipmentFromSave(SaveData saveData)
        {
            if (_equipmentManager == null)
            {
                Debug.LogWarning("[SaveController] EquipmentManager not available");
                return;
            }

            if (saveData.equipmentData == null)
            {
                Debug.LogWarning("[SaveController] No equipment data in save");
                return;
            }

            _equipmentManager.LoadEquipment(saveData.equipmentData);
            
            if (_characterManager != null)
            {
                _characterManager.UpdateStatsFromEquipment();
            }
            
            Debug.Log("[SaveController] Equipment loaded and stats updated");
        }
        #endregion

        #region Private Methods - Skill Loadout Conversion
        private SkillLoadoutSaveData ConvertToSkillLoadoutSaveData()
        {
            if (_skillLoadoutManager != null)
            {
                return _skillLoadoutManager.SaveSkillLoadout();
            }

            return new SkillLoadoutSaveData
            {
                normalSkill1Id = string.Empty,
                normalSkill2Id = string.Empty,
                ultimateSkillId = string.Empty
            };
        }

        private void LoadSkillLoadoutFromSave(SaveData saveData)
        {
            if (_skillLoadoutManager == null)
            {
                Debug.LogWarning("[SaveController] SkillLoadoutManager not available");
                return;
            }

            if (saveData.skillLoadoutData == null)
            {
                Debug.LogWarning("[SaveController] No skill loadout data in save");
                return;
            }

            _skillLoadoutManager.LoadSkillLoadout(saveData.skillLoadoutData);
            
            Debug.Log("[SaveController] Skill loadout loaded");
        }
        #endregion
    }
}