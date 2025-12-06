// ════════════════════════════════════════════════════════════════════════
// GameSystemHub.cs
// Central coordinator for all game systems
// Place as parent GameObject with all managers as children
// ════════════════════════════════════════════════════════════════════════

using UnityEngine;
using Ascension.Managers;
using Ascension.Systems;

namespace Ascension.Core
{
    public class GameSystemHub : MonoBehaviour
    {
        #region Singleton
        public static GameSystemHub Instance { get; private set; }
        #endregion

        #region System References
        public CharacterManager CharacterManager { get; private set; }
        public SaveManager SaveManager { get; private set; }
        public InventoryManager InventoryManager { get; private set; }
        public EquipmentManager EquipmentManager { get; private set; }
        public PotionManager PotionManager { get; private set; }
        public GameManager GameManager { get; private set; }
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

            FindSystemsFromChildren();
        }

        private void Start()
        {
            ValidateCriticalSystems();
            ConnectSystemEvents();
            
            isInitialized = true;
            Log($"All systems ready ({CountSystems()}/6 initialized)");
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

            CharacterManager = GetComponentInChildren<CharacterManager>();
            SaveManager = GetComponentInChildren<SaveManager>();
            InventoryManager = GetComponentInChildren<InventoryManager>();
            EquipmentManager = GetComponentInChildren<EquipmentManager>();
            PotionManager = GetComponentInChildren<PotionManager>();
            GameManager = GetComponentInChildren<GameManager>();

            LogSystemStatus();
        }

        private void ValidateCriticalSystems()
        {
            if (CharacterManager == null)
                Debug.LogError("[GameSystemHub] CRITICAL: CharacterManager missing!");
            
            if (SaveManager == null)
                Debug.LogError("[GameSystemHub] CRITICAL: SaveManager missing!");
            
            if (InventoryManager == null)
                Debug.LogWarning("[GameSystemHub] InventoryManager missing");
            
            if (EquipmentManager == null)
                Debug.LogWarning("[GameSystemHub] EquipmentManager missing");
        }

        private void ConnectSystemEvents()
        {
            Log("Connecting system events...");

            // Equipment → Character stats
            if (EquipmentManager != null && CharacterManager != null)
            {
                EquipmentManager.OnEquipmentChanged += OnEquipmentChanged;
                Log("✓ EquipmentManager → CharacterManager connected");
            }

            // Add more event connections here as systems grow
        }
        #endregion

        #region Event Handlers
        private void OnEquipmentChanged()
        {
            if (CharacterManager.HasActivePlayer)
            {
                CharacterManager.UpdateStatsFromEquipment();
            }
        }
        #endregion

        #region Public API
        /// <summary>
        /// Check if all critical systems are initialized and ready
        /// </summary>
        public bool AreAllSystemsReady()
        {
            return isInitialized &&
                   CharacterManager != null &&
                   SaveManager != null &&
                   //EquipmentManager != null &&
                   InventoryManager != null;
        }

        /// <summary>
        /// Get detailed system status for debugging
        /// </summary>
        public string GetSystemStatus()
        {
            return $"=== GAME SYSTEMS STATUS ===\n" +
                   $"Hub Initialized: {(isInitialized ? "✓" : "✗")}\n" +
                   $"CharacterManager: {GetStatusIcon(CharacterManager)}\n" +
                   $"SaveManager: {GetStatusIcon(SaveManager)}\n" +
                   $"InventoryManager: {GetStatusIcon(InventoryManager)}\n" +
                   // $"EquipmentManager: {GetStatusIcon(EquipmentManager)}\n" +
                   $"PotionManager: {GetStatusIcon(PotionManager)}\n" +
                   $"GameManager: {GetStatusIcon(GameManager)}";
        }
        #endregion

        #region Private Helpers
        private int CountSystems()
        {
            int count = 0;
            if (CharacterManager != null) count++;
            if (SaveManager != null) count++;
            if (InventoryManager != null) count++;
            // if (EquipmentManager != null) count++;
            if (PotionManager != null) count++;
            if (GameManager != null) count++;
            return count;
        }

        private string GetStatusIcon(Object system)
        {
            return system != null ? "✓ Ready" : "✗ Missing";
        }

        private void LogSystemStatus()
        {
            Log($"CharacterManager: {GetStatusIcon(CharacterManager)}");
            Log($"SaveManager: {GetStatusIcon(SaveManager)}");
            Log($"InventoryManager: {GetStatusIcon(InventoryManager)}");
            // Log($"EquipmentManager: {GetStatusIcon(EquipmentManager)}");
            Log($"PotionManager: {GetStatusIcon(PotionManager)}");
            Log($"GameManager: {GetStatusIcon(GameManager)}");
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

        [ContextMenu("Validate All Systems")]
        private void DebugValidateAllSystems()
        {
            ValidateCriticalSystems();
            Debug.Log($"Systems Ready: {AreAllSystemsReady()}");
        }
        #endregion

        #region Cleanup
        private void OnDestroy()
        {
            // Unsubscribe from events
            // if (EquipmentManager != null)
            // {
            //     EquipmentManager.OnEquipmentChanged -= OnEquipmentChanged;
            // }
        }
        #endregion
    }
}