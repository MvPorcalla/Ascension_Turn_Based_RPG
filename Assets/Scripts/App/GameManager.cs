// ════════════════════════════════════════════
// Assets\Scripts\AppFlow\GameManager.cs
// Central game controller - uses GameSystemHub for system access
// Handles: Game flow, saves, scene management
// ════════════════════════════════════════════

using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using Ascension.Core;
using Ascension.Manager.Model;
using Ascension.Character.Stat;
using Ascension.Character.Manager;
using Ascension.Inventory.Manager;
using Ascension.Inventory.Data;  // ✅ ADDED: For BagInventoryData
using Ascension.Data.SO.Character;

namespace Ascension.App
{
    public class GameManager : MonoBehaviour
    {
        #region Singleton
        public static GameManager Instance { get; private set; }
        #endregion
        
        #region Serialized Fields
        [Header("Runtime Data (Read Only)")]
        [SerializeField] private float sessionPlayTime = 0f;
        [SerializeField] private bool isAvatarCreationComplete = false;
        #endregion
        
        #region Properties (Through Hub)
        private GameSystemHub Hub => GameSystemHub.Instance;
        private CharacterManager CharManager => Hub?.GetSystem<CharacterManager>();
        private SaveManager SManager => Hub?.GetSystem<SaveManager>();
        private InventoryManager InvManager => Hub?.GetSystem<InventoryManager>();
        private EquipmentManager EqManager => Hub?.GetSystem<EquipmentManager>();
        
        public CharacterStats CurrentPlayer => CharManager?.CurrentPlayer;
        public CharacterBaseStatsSO BaseStats => CharManager?.BaseStats;
        public bool HasActivePlayer => CharManager != null && CharManager.HasActivePlayer;
        #endregion
        
        #region Events
        public event Action<CharacterStats> OnPlayerLoaded;
        public event Action OnPlayerSaved;
        public event Action OnNewGameStarted;
        #endregion
        
        #region Unity Callbacks
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            Instance = this;
        }

        private void Start()
        {
            WaitForSystemsAndSubscribe();
        }

        private void OnDestroy()
        {
            UnsubscribeFromCharacterManager();
        }
        
        private void Update()
        {
            if (HasActivePlayer)
            {
                sessionPlayTime += Time.unscaledDeltaTime;
            }
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus && CanSave())
            {
                SaveGame();
            }
        }
        
        private void OnApplicationQuit()
        {
            if (CanSave())
            {
                SaveGame();
            }
        }
        #endregion
        
        #region Public Methods - Game Flow
        public void StartNewGame()
        {
            if (!ValidateSystemsReady())
                return;

            CharManager.CreateNewPlayer("Adventurer");
            
            sessionPlayTime = 0f;
            isAvatarCreationComplete = false;
            
            Debug.Log("[GameManager] New game started");
            OnNewGameStarted?.Invoke();
        }
        
        public void SetPlayerName(string playerName)
        {
            if (!HasActivePlayer) return;
            
            CurrentPlayer.playerName = playerName;
            Debug.Log($"[GameManager] Player name set: {playerName}");
        }

        public void CompleteAvatarCreation()
        {
            isAvatarCreationComplete = true;
            Debug.Log("[GameManager] Avatar creation completed - saves enabled");
        }
        
        public bool SaveGame()
        {
            if (!CanSave())
            {
                Debug.LogWarning("[GameManager] Save blocked - conditions not met");
                return false;
            }
            
            // Convert real data to save data format
            CharacterSaveData charSaveData = ConvertToCharacterSaveData(CurrentPlayer);
            InventorySaveData invSaveData = ConvertToInventorySaveData();
            EquipmentSaveData eqSaveData = ConvertToEquipmentSaveData();
            
            bool success = SManager.SaveGame(charSaveData, invSaveData, eqSaveData, sessionPlayTime);
            
            if (success)
            {
                OnPlayerSaved?.Invoke();
            }
            
            return success;
        }

        public bool LoadGame()
        {
            if (!ValidateSystemsReady())
                return false;

            SaveData saveData = SManager.LoadGame();
            
            if (saveData == null)
            {
                Debug.LogError("[GameManager] Failed to load save");
                return false;
            }
            
            LoadPlayerFromSave(saveData);
            LoadInventoryFromSave(saveData);
            LoadEquipmentFromSave(saveData);
            
            sessionPlayTime = 0f;
            isAvatarCreationComplete = true;
            
            Debug.Log($"[GameManager] Loaded: {CurrentPlayer.playerName} (Lv.{CurrentPlayer.Level})");
            
            return true;
        }
        
        public bool SaveExists()
        {
            return SManager != null && SManager.SaveExists();
        }
        
        public bool DeleteSave()
        {
            if (SManager == null) return false;
            
            bool success = SManager.DeleteSave();
            
            if (success && CharManager != null)
            {
                CharManager.UnloadPlayer();
            }
            
            return success;
        }
        #endregion
        
        #region Public Methods - Player Actions
        public bool AddExperience(int amount)
        {
            if (!HasActivePlayer || CharManager == null)
                return false;
            
            int oldLevel = CurrentPlayer.Level;
            CharManager.AddExperience(amount);
            
            bool leveledUp = CurrentPlayer.Level > oldLevel;
            
            if (leveledUp)
            {
                Debug.Log($"[GameManager] Level up! Now level {CurrentPlayer.Level}");
                SaveGame();
            }
            
            return leveledUp;
        }

        public void Heal(float amount)
        {
            CharManager?.Heal(amount);
        }

        public void TakeDamage(float amount)
        {
            CharManager?.TakeDamage(amount);
        }

        public void FullHeal()
        {
            CharManager?.FullHeal();
        }
        #endregion
        
        #region Public Methods - Scene Management
        public void LoadScene(string sceneName)
        {
            SceneManager.LoadScene(sceneName);
        }
        
        public void LoadSceneWithSave(string sceneName)
        {
            SaveGame();
            SceneManager.LoadScene(sceneName);
        }
        
        public void GoToMainBase()
        {
            LoadScene("03_MainBase");
        }
        
        public void ResetAndCreateNewCharacter()
        {
            DeleteSave();
            StartNewGame();
            LoadScene("02_AvatarCreation");
        }
        
        public void ReturnToMainBase()
        {
            SaveGame();
            LoadScene("03_MainBase");
        }

        public void OnActivityCompleted()
        {
            SaveGame();
            Debug.Log("[GameManager] Progress saved after activity");
        }
        #endregion
        
        #region Private Methods - Conversion
        private CharacterSaveData ConvertToCharacterSaveData(CharacterStats stats)
        {
            return new CharacterSaveData
            {
                playerName = stats.playerName,
                level = stats.Level,
                currentExperience = stats.CurrentEXP,          // ✅ FIXED: Was CurrentExperience
                currentHealth = stats.CurrentHP,               // ✅ FIXED: Was CurrentHealth
                currentMana = 0f,                              // ✅ FIXED: You don't have mana yet
                attributePoints = stats.UnallocatedPoints,     // ✅ FIXED: Was AttributePoints
                strength = stats.attributes.STR,               // ✅ FIXED: Was Attributes.Strength
                dexterity = stats.attributes.AGI,              // ✅ FIXED: Was Attributes.Dexterity
                intelligence = stats.attributes.INT,           // ✅ FIXED: Was Attributes.Intelligence
                vitality = stats.attributes.END,               // ✅ FIXED: Was Attributes.Vitality
                luck = stats.attributes.WIS                    // ✅ FIXED: Was Attributes.Luck (mapped to WIS)
            };
        }

        private InventorySaveData ConvertToInventorySaveData()
        {
            if (InvManager == null)
            {
                return new InventorySaveData { items = new ItemInstanceData[0], maxBagSlots = 12 };
            }

            BagInventoryData bagData = InvManager.SaveInventory();
            
            // Convert BagInventoryData to InventorySaveData
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

        private EquipmentSaveData ConvertToEquipmentSaveData()
        {
            if (EqManager == null)
            {
                // Return empty equipment data
                return new EquipmentSaveData
                {
                    weaponId = "",
                    helmetId = "",
                    chestId = "",
                    glovesId = "",
                    bootsId = ""
                };
            }

            // TODO: Implement when EquipmentManager exists
            return new EquipmentSaveData();
        }
        #endregion
        
        #region Private Methods - Loading
        private void WaitForSystemsAndSubscribe()
        {
            if (Hub == null || !Hub.IsInitialized)
            {
                Debug.LogWarning("[GameManager] Waiting for GameSystemHub...");
                Invoke(nameof(WaitForSystemsAndSubscribe), 0.1f);
                return;
            }
            
            // ✅ ADD THIS: Wait for CharacterManager specifically
            if (CharManager == null)
            {
                Debug.LogWarning("[GameManager] Waiting for CharacterManager...");
                Invoke(nameof(WaitForSystemsAndSubscribe), 0.1f);
                return;
            }
            
            SubscribeToCharacterManager();
        }
        
        private void SubscribeToCharacterManager()
        {
            if (CharManager != null)
            {
                CharManager.OnPlayerLoaded += OnCharacterLoaded;
                Debug.Log("[GameManager] Subscribed to CharacterManager");
            }
            else
            {
                Debug.LogWarning("[GameManager] CharacterManager not found!");
            }
        }

        private void UnsubscribeFromCharacterManager()
        {
            if (CharManager != null)
            {
                CharManager.OnPlayerLoaded -= OnCharacterLoaded;
            }
        }
        
        private void OnCharacterLoaded(CharacterStats stats)
        {
            OnPlayerLoaded?.Invoke(stats);
        }
        
        private bool ValidateSystemsReady()
        {
            if (Hub == null || !Hub.AreAllSystemsReady())
            {
                Debug.LogError("[GameManager] Systems not ready!");
                return false;
            }
            return true;
        }
        
        private bool CanSave()
        {
            if (!HasActivePlayer)
            {
                Debug.LogError("[GameManager] No active player to save!");
                return false;
            }
            
            if (!isAvatarCreationComplete)
            {
                Debug.LogWarning("[GameManager] Save blocked: Avatar creation not complete");
                return false;
            }
            
            return true;
        }
        
        private void LoadPlayerFromSave(SaveData saveData)
        {
            // Create new character stats
            CharacterStats stats = new CharacterStats();
            stats.Initialize(CharManager.BaseStats);
            
            // Restore saved values
            stats.playerName = saveData.characterData.playerName;
            stats.guildRank = "Unranked"; // TODO: Save/load guild rank
            
            // Restore level
            stats.levelSystem.level = saveData.characterData.level;
            stats.levelSystem.currentEXP = saveData.characterData.currentExperience;
            stats.levelSystem.unallocatedPoints = saveData.characterData.attributePoints;
            
            // Restore attributes
            stats.attributes.STR = saveData.characterData.strength;
            stats.attributes.AGI = saveData.characterData.dexterity;
            stats.attributes.INT = saveData.characterData.intelligence;
            stats.attributes.END = saveData.characterData.vitality;
            stats.attributes.WIS = saveData.characterData.luck;
            
            // Recalculate stats
            stats.RecalculateStats(CharManager.BaseStats, fullHeal: false);
            
            // Restore HP
            stats.combatRuntime.currentHP = saveData.characterData.currentHealth;
            
            // Load player
            CharManager.LoadPlayer(stats);
        }
        
        private void LoadInventoryFromSave(SaveData saveData)
        {
            if (InvManager != null && saveData.inventoryData != null)
            {
                // Convert InventorySaveData back to BagInventoryData
                BagInventoryData bagData = new BagInventoryData();
                bagData.maxBagSlots = saveData.inventoryData.maxBagSlots;
                bagData.items = new System.Collections.Generic.List<ItemInstance>();
                
                foreach (var itemData in saveData.inventoryData.items)
                {
                    bagData.items.Add(new ItemInstance(itemData.itemId, itemData.quantity, false));
                }
                
                InvManager.LoadInventory(bagData);
            }
        }
        
        private void LoadEquipmentFromSave(SaveData saveData)
        {
            if (EqManager != null && saveData.equipmentData != null)
            {
                // TODO: Implement when EquipmentManager exists
                // EqManager.LoadEquipment(convertedData);
                CharManager.UpdateStatsFromEquipment();
            }
        }
        #endregion
    }
}