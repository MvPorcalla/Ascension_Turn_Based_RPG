// ═══════════════════════════════════════════════════════════════════════════════
// Assets\Scripts\Core\GameSystemHub.cs
// Central coordinator for all game systems
// Uses generic Component storage - actual types resolved by GameCore layer
// ═══════════════════════════════════════════════════════════════════════════════

using UnityEngine;

namespace Ascension.Core
{
    public class GameSystemHub : MonoBehaviour
    {
        #region Singleton
        public static GameSystemHub Instance { get; private set; }
        #endregion

        #region System References (Generic Components)
        private Component characterManager;
        private Component saveManager;
        private Component inventoryManager;
        private Component equipmentManager;
        private Component potionManager;
        private Component gameManager;
        #endregion

        #region Initialization State
        private bool isInitialized = false;
        public bool IsInitialized => isInitialized;
        #endregion

        #region Unity Callbacks
        private void Awake()
        {
            if (!InitializeSingleton())
                return;

            // Delay finding systems slightly to ensure they initialize first
            Invoke(nameof(FindSystemsFromChildren), 0.05f);
        }

        private void Start()
        {
            isInitialized = true;
            Log($"Hub ready - {CountSystems()} systems found");
        }
        #endregion

        #region Initialization
        private bool InitializeSingleton()
        {
            if (Instance != null && Instance != this)
            {
                Debug.LogWarning("[GameSystemHub] Duplicate instance found, destroying...");
                Destroy(gameObject);
                return false;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
            Log("Initialized");
            return true;
        }

        private void FindSystemsFromChildren()
        {
            Log("Finding systems from children...");

            characterManager = FindManagerByName("CharacterManager");
            saveManager = FindManagerByName("SaveManager");
            inventoryManager = FindManagerByName("InventoryManager");
            equipmentManager = FindManagerByName("EquipmentManager");
            potionManager = FindManagerByName("PotionManager");
            gameManager = FindManagerByName("GameManager");

            LogSystemStatus();
        }

        private Component FindManagerByName(string typeName)
        {
            Component[] components = GetComponentsInChildren<Component>(true);
            
            foreach (Component comp in components)
            {
                if (comp.GetType().Name == typeName)
                {
                    Log($"✓ Found {typeName}");
                    return comp;
                }
            }
            
            Log($"✗ {typeName} not found");
            return null;
        }
        #endregion

        #region Public API
        /// <summary>
        /// Get a system by type - called from higher-level code (GameCore)
        /// </summary>
        public T GetSystem<T>() where T : Component
        {
            string typeName = typeof(T).Name;
            
            switch (typeName)
            {
                case "CharacterManager": return characterManager as T;
                case "SaveManager": return saveManager as T;
                case "InventoryManager": return inventoryManager as T;
                case "EquipmentManager": return equipmentManager as T;
                case "PotionManager": return potionManager as T;
                case "GameManager": return gameManager as T;
                default: return null;
            }
        }

        /// <summary>
        /// Check if all critical systems are ready
        /// </summary>
        public bool AreAllSystemsReady()
        {
            return isInitialized &&
                   characterManager != null &&
                   saveManager != null &&
                   inventoryManager != null;
        }

        /// <summary>
        /// Get detailed system status for debugging
        /// </summary>
        public string GetSystemStatus()
        {
            return $"=== GAME SYSTEMS STATUS ===\n" +
                   $"Hub Initialized: {(isInitialized ? "✓" : "✗")}\n" +
                   $"CharacterManager: {GetStatusIcon(characterManager)}\n" +
                   $"SaveManager: {GetStatusIcon(saveManager)}\n" +
                   $"InventoryManager: {GetStatusIcon(inventoryManager)}\n" +
                   $"EquipmentManager: {GetStatusIcon(equipmentManager)}\n" +
                   $"PotionManager: {GetStatusIcon(potionManager)}\n" +
                   $"GameManager: {GetStatusIcon(gameManager)}";
        }
        #endregion

        #region Private Helpers
        private int CountSystems()
        {
            int count = 0;
            if (characterManager != null) count++;
            if (saveManager != null) count++;
            if (inventoryManager != null) count++;
            if (equipmentManager != null) count++;
            if (potionManager != null) count++;
            if (gameManager != null) count++;
            return count;
        }

        private string GetStatusIcon(Object system)
        {
            return system != null ? "✓ Ready" : "✗ Missing";
        }

        private void LogSystemStatus()
        {
            Log($"CharacterManager: {GetStatusIcon(characterManager)}");
            Log($"SaveManager: {GetStatusIcon(saveManager)}");
            Log($"InventoryManager: {GetStatusIcon(inventoryManager)}");
            Log($"EquipmentManager: {GetStatusIcon(equipmentManager)}");
            Log($"PotionManager: {GetStatusIcon(potionManager)}");
            Log($"GameManager: {GetStatusIcon(gameManager)}");
        }

        private void Log(string message)
        {
            Debug.Log($"[GameSystemHub] {message}");
        }
        #endregion

        #region Debug Tools
        [ContextMenu("Print System Status")]
        private void DebugPrintStatus()
        {
            Debug.Log(GetSystemStatus());
        }

        [ContextMenu("Count Systems")]
        private void DebugCountSystems()
        {
            Debug.Log($"Systems found: {CountSystems()}/6");
        }
        #endregion
    }
}