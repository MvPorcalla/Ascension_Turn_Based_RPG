// ════════════════════════════════════════════
// Assets\Scripts\AppFlow\SaveController.cs
// Manages game saving and loading operations
// ════════════════════════════════════════════

using UnityEngine;
using Ascension.Core;
using Ascension.Data.Save;
using Ascension.Character.Stat;
using Ascension.Character.Manager;
using Ascension.Inventory.Manager;
using Ascension.Inventory.Data;

namespace Ascension.App
{
    public class SaveController : MonoBehaviour, IGameService
    {
        #region Injected Dependencies
        private CharacterManager _characterManager;
        private SaveManager _saveManager;
        private InventoryManager _inventoryManager;
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
            
            bool success = _saveManager.SaveGame(charData, invData, eqData, sessionPlayTime);
            
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
            
            LoadPlayerFromSave(saveData);
            LoadInventoryFromSave(saveData);
            LoadEquipmentFromSave(saveData);
            
            OnLoadCompleted?.Invoke();
            Debug.Log("[SaveController] Game loaded successfully");
            
            return true;
        }

        public bool SaveExists()
        {
            return _saveManager != null && _saveManager.SaveExists();
        }

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
        /// <summary>
        /// ✅ FIXED: Maps CharacterStats attributes to save data with correct names
        /// </summary>
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
                
                // ✅ FIXED: Map to correct save data fields
                strength = stats.attributes.STR,
                agility = stats.attributes.AGI,      // Was: dexterity
                intelligence = stats.attributes.INT,
                endurance = stats.attributes.END,    // Was: vitality
                wisdom = stats.attributes.WIS        // Was: luck
            };
        }

        /// <summary>
        /// ✅ FIXED: Maps save data to CharacterStats attributes with correct names
        /// </summary>
        private void LoadPlayerFromSave(SaveData saveData)
        {
            CharacterStats stats = new CharacterStats();
            stats.Initialize(_characterManager.BaseStats);
            
            stats.playerName = saveData.characterData.playerName;
            stats.guildRank = "Unranked";
            
            stats.levelSystem.level = saveData.characterData.level;
            stats.levelSystem.currentEXP = saveData.characterData.currentExperience;
            stats.levelSystem.unallocatedPoints = saveData.characterData.attributePoints;
            
            // ✅ FIXED: Map from correct save data fields
            stats.attributes.STR = saveData.characterData.strength;
            stats.attributes.AGI = saveData.characterData.agility;      // Was: dexterity
            stats.attributes.INT = saveData.characterData.intelligence;
            stats.attributes.END = saveData.characterData.endurance;    // Was: vitality
            stats.attributes.WIS = saveData.characterData.wisdom;       // Was: luck
            
            stats.RecalculateStats(_characterManager.BaseStats, fullHeal: false);
            stats.combatRuntime.currentHP = saveData.characterData.currentHealth;
            
            _characterManager.LoadPlayer(stats);
            
            Debug.Log($"[SaveController] ✓ Loaded character: {stats.playerName}");
            Debug.Log($"[SaveController]   Attributes - STR:{stats.attributes.STR} AGI:{stats.attributes.AGI} INT:{stats.attributes.INT} END:{stats.attributes.END} WIS:{stats.attributes.WIS}");
        }
        #endregion

        #region Private Methods - Inventory Conversion
        private InventorySaveData ConvertToInventorySaveData()
        {
            if (_inventoryManager == null)
            {
                return new InventorySaveData { items = new ItemInstanceData[0], maxBagSlots = 12 };
            }

            BagInventoryData bagData = _inventoryManager.SaveInventory();
            
            ItemInstanceData[] itemArray = new ItemInstanceData[bagData.items.Count];
            
            for (int i = 0; i < bagData.items.Count; i++)
            {
                itemArray[i] = new ItemInstanceData
                {
                    itemId = bagData.items[i].itemID,
                    quantity = bagData.items[i].quantity
                };
            }
            
            return new InventorySaveData
            {
                maxBagSlots = bagData.maxBagSlots,
                items = itemArray
            };
        }

        private void LoadInventoryFromSave(SaveData saveData)
        {
            if (_inventoryManager != null && saveData.inventoryData != null)
            {
                BagInventoryData bagData = new BagInventoryData
                {
                    maxBagSlots = saveData.inventoryData.maxBagSlots,
                    items = new System.Collections.Generic.List<ItemInstance>()
                };
                
                foreach (var itemData in saveData.inventoryData.items)
                {
                    bagData.items.Add(new ItemInstance(itemData.itemId, itemData.quantity, false));
                }
                
                _inventoryManager.LoadInventory(bagData);
            }
        }
        #endregion

        #region Private Methods - Equipment Conversion
        private EquipmentSaveData ConvertToEquipmentSaveData()
        {
            // TODO: Implement when EquipmentManager ready
            return new EquipmentSaveData
            {
                weaponId = "",
                helmetId = "",
                chestId = "",
                glovesId = "",
                bootsId = ""
            };
        }

        private void LoadEquipmentFromSave(SaveData saveData)
        {
            // TODO: Implement when EquipmentManager ready
            if (saveData.equipmentData != null)
            {
                _characterManager.UpdateStatsFromEquipment();
            }
        }
        #endregion
    }
}