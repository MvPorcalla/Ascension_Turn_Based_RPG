// ════════════════════════════════════════════
// GameManager.cs
// Central game controller - uses GameSystemHub for system access
// Handles: Game flow, saves, scene management
// ════════════════════════════════════════════

using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using Ascension.Data.Models;
using Ascension.Core;

namespace Ascension.Managers
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
        public PlayerStats CurrentPlayer => Hub?.CharacterManager?.CurrentPlayer;
        public CharacterBaseStatsSO BaseStats => Hub?.CharacterManager?.BaseStats;
        public bool HasActivePlayer => Hub?.CharacterManager != null && Hub.CharacterManager.HasActivePlayer;
        #endregion
        
        #region Events
        public event Action<PlayerStats> OnPlayerLoaded;
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

            Hub.CharacterManager.CreateNewPlayer("Adventurer");
            
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
            
            PlayerStats playerToSave = Hub.CharacterManager.GetPlayerDataForSave();
            BagInventoryData inventoryData = Hub.InventoryManager.SaveInventory();
            EquipmentData equipmentData = GetEquipmentData();
            
            bool success = Hub.SaveManager.SaveGame(playerToSave, inventoryData, equipmentData, sessionPlayTime);
            
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

            SaveData saveData = Hub.SaveManager.LoadGame();
            
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
            return Hub?.SaveManager != null && Hub.SaveManager.SaveExists();
        }
        
        public bool DeleteSave()
        {
            if (Hub?.SaveManager == null) return false;
            
            bool success = Hub.SaveManager.DeleteSave();
            
            if (success && Hub.CharacterManager != null)
            {
                Hub.CharacterManager.UnloadPlayer();
            }
            
            return success;
        }
        #endregion
        
        #region Public Methods - Player Actions (Delegate to CharacterManager)
        public bool AddExperience(int amount)
        {
            if (!HasActivePlayer || Hub.CharacterManager == null)
                return false;
            
            int oldLevel = CurrentPlayer.Level;
            Hub.CharacterManager.AddExperience(amount);
            
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
            Hub?.CharacterManager?.Heal(amount);
        }

        public void TakeDamage(float amount)
        {
            Hub?.CharacterManager?.TakeDamage(amount);
        }

        public void FullHeal()
        {
            Hub?.CharacterManager?.FullHeal();
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
        
        #region Private Methods
        private void WaitForSystemsAndSubscribe()
        {
            if (Hub == null || !Hub.IsInitialized)
            {
                Debug.LogWarning("[GameManager] Waiting for GameSystemHub...");
                Invoke(nameof(WaitForSystemsAndSubscribe), 0.1f);
                return;
            }
            
            SubscribeToCharacterManager();
        }
        
        private void SubscribeToCharacterManager()
        {
            if (Hub?.CharacterManager != null)
            {
                Hub.CharacterManager.OnPlayerLoaded += OnCharacterLoaded;
                Debug.Log("[GameManager] Subscribed to CharacterManager");
            }
            else
            {
                Debug.LogWarning("[GameManager] CharacterManager not found!");
            }
        }

        private void UnsubscribeFromCharacterManager()
        {
            if (Hub?.CharacterManager != null)
            {
                Hub.CharacterManager.OnPlayerLoaded -= OnCharacterLoaded;
            }
        }
        
        private void OnCharacterLoaded(PlayerStats stats)
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
            PlayerStats loadedPlayer = saveData.playerData.ToPlayerStats(Hub.CharacterManager.BaseStats);
            Hub.CharacterManager.LoadPlayer(loadedPlayer);
        }
        
        private void LoadInventoryFromSave(SaveData saveData)
        {
            if (Hub.InventoryManager != null && saveData.inventoryData != null)
            {
                Hub.InventoryManager.LoadInventory(saveData.inventoryData);
            }
        }
        
        private void LoadEquipmentFromSave(SaveData saveData)
        {
            if (Hub.EquipmentManager != null && saveData.equipmentData != null)
            {
                Hub.EquipmentManager.LoadEquipment(saveData.equipmentData);
                // Update CharacterManager stats after loading equipment
                Hub.CharacterManager.UpdateStatsFromEquipment();
            }
        }
        
        private EquipmentData GetEquipmentData()
        {
            if (Hub.EquipmentManager != null)
            {
                return Hub.EquipmentManager.SaveEquipment();
            }
            return new EquipmentData(); // Empty equipment data
        }
        #endregion
    }
}