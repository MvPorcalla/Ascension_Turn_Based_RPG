// ════════════════════════════════════════════════════════════════════════
// Assets/Scripts/Core/GameBootstrap.cs
// ✅ UPDATED: Added SaveScheduler for auto-save system
// ════════════════════════════════════════════════════════════════════════

using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Ascension.Character.Manager;
using Ascension.Inventory.Manager;
using Ascension.Equipment.Manager;
using Ascension.Skill.Manager;
using Ascension.CharacterCreation.Manager;
using Ascension.PotionSystem.Manager;

namespace Ascension.Core
{
    public class GameBootstrap : MonoBehaviour
    {
        #region Singleton (Self)
        public static GameBootstrap Instance { get; private set; }
        #endregion

        #region Serialized Fields
        [Header("Persistent UI")]
        [SerializeField] private GameObject persistentUICanvas;

        [Header("Settings")]
        [SerializeField] private float minimumLoadTime = 1f;
        [SerializeField] private bool showDebugLogs = true;
        #endregion

        #region Static Manager References
        public static SaveManager Save { get; private set; }
        public static SaveScheduler SaveScheduler { get; private set; } // ✅ NEW
        public static CharacterManager Character { get; private set; }
        public static InventoryManager Inventory { get; private set; }
        public static EquipmentManager Equipment { get; private set; }
        public static SkillLoadoutManager Skills { get; private set; }
        public static PotionManager Potion { get; private set; }
        public static SceneFlowManager SceneFlow { get; private set; }
        #endregion

        #region Initialization State
        private bool _isInitialized = false;
        public bool IsInitialized => _isInitialized;
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
            
            // ✅ CRITICAL: Make managers persistent across scene loads
            DontDestroyOnLoad(gameObject);
            
            if (persistentUICanvas != null)
            {
                DontDestroyOnLoad(persistentUICanvas);
            }
        }

        private IEnumerator Start()
        {
            float startTime = Time.time;
            Log("=== BOOTSTRAP START ===");

            // ✅ FIXED: Wrapped in try-catch to prevent broken game state
            try
            {
                SetupPersistentUI();
                DiscoverManagers();
                InitializeManagers();
                
                _isInitialized = true;
            }
            catch (Exception e)
            {
                LogError($"CRITICAL: Bootstrap initialization failed!\n{e.Message}\n{e.StackTrace}");
                
                // ✅ Show error to user (you can customize this)
                Debug.LogError("═══════════════════════════════════════════════════");
                Debug.LogError("GAME INITIALIZATION FAILED - CANNOT CONTINUE");
                Debug.LogError("Check console for details");
                Debug.LogError("═══════════════════════════════════════════════════");
                
                // Optionally: Load error scene or quit application
                #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
                #else
                Application.Quit();
                #endif
                
                yield break;
            }

            yield return EnsureMinimumLoadTime(startTime);

            string targetScene = DetermineStartingScene();

            // ✅ ALWAYS load additively - Bootstrap scene stays loaded forever
            yield return LoadSceneAsync(targetScene);

            Log($"=== BOOTSTRAP COMPLETE → {targetScene} ===");
        }
        #endregion

        #region Initialization Steps
        private void SetupPersistentUI()
        {
            if (persistentUICanvas != null)
            {
                Log("✓ Persistent UI ready (DontDestroyOnLoad)");
            }
            else
            {
                LogError("PersistentUICanvas not assigned in Inspector!");
                throw new InvalidOperationException("PersistentUICanvas is required but not assigned");
            }
        }

        private void DiscoverManagers()
        {
            Log("Discovering managers...");

            Save = GetComponentInChildren<SaveManager>();
            SaveScheduler = GetComponentInChildren<SaveScheduler>(); // ✅ NEW
            Character = GetComponentInChildren<CharacterManager>();
            Inventory = GetComponentInChildren<InventoryManager>();
            Equipment = GetComponentInChildren<EquipmentManager>();
            Skills = GetComponentInChildren<SkillLoadoutManager>();
            Potion = GetComponentInChildren<PotionManager>();
            SceneFlow = GetComponentInChildren<SceneFlowManager>();

            // ✅ FIXED: Validate all managers exist before continuing
            ValidateManagerReferences();

            Log("✓ All managers discovered");
        }

        /// <summary>
        /// ✅ NEW: Validate all required managers are present
        /// </summary>
        private void ValidateManagerReferences()
        {
            bool hasErrors = false;

            if (Save == null)
            {
                LogError("SaveManager not found in children!");
                hasErrors = true;
            }

            if (SaveScheduler == null)
            {
                LogError("SaveScheduler not found in children!");
                hasErrors = true;
            }

            if (Character == null)
            {
                LogError("CharacterManager not found in children!");
                hasErrors = true;
            }

            if (Inventory == null)
            {
                LogError("InventoryManager not found in children!");
                hasErrors = true;
            }

            if (Equipment == null)
            {
                LogError("EquipmentManager not found in children!");
                hasErrors = true;
            }

            if (Skills == null)
            {
                LogError("SkillLoadoutManager not found in children!");
                hasErrors = true;
            }

            if (Potion == null)
            {
                LogError("PotionManager not found in children!");
                hasErrors = true;
            }

            if (SceneFlow == null)
            {
                LogError("SceneFlowManager not found in children!");
                hasErrors = true;
            }

            if (hasErrors)
            {
                throw new InvalidOperationException(
                    "One or more required managers are missing! " +
                    "Check that all manager components are attached to Bootstrap children."
                );
            }
        }

        /// <summary>
        /// ✅ FIXED: Now validates each Init() call and stops on failure
        /// </summary>
        private void InitializeManagers()
        {
            Log("Initializing managers...");

            try
            {
                // ✅ Initialize in dependency order
                Save.Init();
                Log("  ✓ SaveManager");

                Character.Init();
                Log("  ✓ CharacterManager");

                Inventory.Init();
                Log("  ✓ InventoryManager");

                Equipment.Init();
                Log("  ✓ EquipmentManager");

                Skills.Init();
                Log("  ✓ SkillLoadoutManager");

                Potion.Init();
                Log("  ✓ PotionManager");

                SceneFlow.Init();
                Log("  ✓ SceneFlowManager");

                // ✅ Initialize SaveScheduler LAST (after all other managers)
                SaveScheduler.Init();
                Log("  ✓ SaveScheduler");

                Log("✓ All managers initialized successfully");
            }
            catch (Exception e)
            {
                LogError($"Manager initialization failed: {e.Message}");
                throw; // Re-throw to be caught by Start()
            }
        }

        private string DetermineStartingScene()
        {
            if (!Save.SaveExists())
            {
                Log("No save found → AvatarCreation");
                return "02_AvatarCreation";
            }

            try
            {
                if (Save.LoadGame())
                {
                    Log($"Save loaded → MainBase (Player: {Character.CurrentPlayer?.playerName})");
                    return "03_MainBase";
                }
            }
            catch (Exception e)
            {
                LogError($"Save corrupted: {e.Message}");
                Log("Falling back to AvatarCreation");
            }

            return "02_AvatarCreation";
        }

        private IEnumerator LoadSceneAsync(string sceneName)
        {
            // ✅ ALWAYS ADDITIVE - Bootstrap scene never unloads
            Log($"Loading {sceneName} (Additive - Bootstrap persists)");
            
            AsyncOperation op = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);

            if (op == null)
            {
                LogError($"Failed to start loading scene: {sceneName}");
                yield break;
            }

            yield return op;

            Scene scene = SceneManager.GetSceneByName(sceneName);
            if (scene.IsValid() && scene.isLoaded)
            {
                SceneManager.SetActiveScene(scene);
                SceneFlow.SetCurrentMainScene(sceneName);
                Log($"✓ Scene loaded and active: {sceneName}");
            }
            else
            {
                LogError($"Scene invalid after load: {sceneName}");
            }
        }

        private IEnumerator EnsureMinimumLoadTime(float startTime)
        {
            float elapsed = Time.time - startTime;
            if (elapsed < minimumLoadTime)
            {
                float remaining = minimumLoadTime - elapsed;
                Log($"Waiting {remaining:F2}s to meet minimum load time");
                yield return new WaitForSeconds(remaining);
            }
        }
        #endregion

        #region Helper Methods
        private void Log(string message)
        {
            if (showDebugLogs) Debug.Log($"[Bootstrap] {message}");
        }

        private void LogError(string message)
        {
            Debug.LogError($"[Bootstrap] {message}");
        }
        #endregion

        #region Editor Utilities
#if UNITY_EDITOR
        [ContextMenu("Validate Bootstrap Setup")]
        private void ValidateSetup()
        {
            Debug.Log("=== BOOTSTRAP VALIDATION ===");

            // Check persistent UI
            if (persistentUICanvas == null)
                Debug.LogError("❌ PersistentUICanvas not assigned!", this);
            else
                Debug.Log("✓ PersistentUICanvas assigned");

            // Check managers
            DiscoverManagers();
            
            Debug.Log("=== VALIDATION COMPLETE ===");
        }
#endif
        #endregion
    }
}