// ═══════════════════════════════════════════════════════════════════════════════
// Assets\Scripts\Core\ServiceContainer.cs
// ✅ FIXED: Added SceneFlowManager to initialization order
// ═══════════════════════════════════════════════════════════════════════════════

using System;
using System.Collections.Generic;
using UnityEngine;
using Ascension.App;
using Ascension.Equipment.Manager;

namespace Ascension.Core
{
    public class ServiceContainer : MonoBehaviour
    {
        #region Singleton
        public static ServiceContainer Instance { get; private set; }
        #endregion

        #region Service Storage
        private readonly Dictionary<Type, Component> _services = new Dictionary<Type, Component>();
        #endregion

        #region Initialization State
        private bool _isDiscoveryComplete = false;
        private bool _isInitialized = false;
        
        public bool IsInitialized => _isInitialized;
        #endregion

        #region Events
        public event Action OnAllSystemsReady;
        #endregion

        #region Unity Callbacks
        private void Awake()
        {
            if (!InitializeSingleton())
                return;

            // Phase 1: Auto-discover all services
            AutoDiscoverServices();
            _isDiscoveryComplete = true;
            
            Log("Discovery complete, waiting for Start() to initialize services...");
        }

        private void Start()
        {
            if (!_isDiscoveryComplete)
            {
                LogError("Discovery not complete in Start()!");
                return;
            }

            // Phase 2: Initialize all services in order
            InitializeAllServices();
            
            // Phase 3: Validate critical services
            ValidateCriticalServices();
            
            // Phase 4: Mark as ready
            _isInitialized = true;
            
            Log($"✓ All systems initialized - {_services.Count} services ready");
            
            // Phase 5: Fire event
            OnAllSystemsReady?.Invoke();
        }
        #endregion

        #region Initialization
        private bool InitializeSingleton()
        {
            if (Instance != null && Instance != this)
            {
                Debug.LogWarning("[ServiceContainer] Duplicate instance, destroying...");
                Destroy(gameObject);
                return false;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
            return true;
        }

        private void AutoDiscoverServices()
        {
            Log("Auto-discovering services...");

            Component[] allComponents = GetComponentsInChildren<Component>(true);
            int discoveredCount = 0;

            foreach (Component comp in allComponents)
            {
                if (comp is IGameService)
                {
                    Type serviceType = comp.GetType();
                    
                    if (serviceType == typeof(ServiceContainer))
                        continue;

                    if (!_services.ContainsKey(serviceType))
                    {
                        _services[serviceType] = comp;
                        Log($"✓ Discovered: {serviceType.Name}");
                        discoveredCount++;
                    }
                    else
                    {
                        LogWarning($"Duplicate service found: {serviceType.Name} (ignored)");
                    }
                }
            }

            Log($"Discovery complete: {discoveredCount} services found");
        }

        /// <summary>
        /// ✅ FIXED: Added SceneFlowManager to initialization order
        /// </summary>
        private void InitializeAllServices()
        {
            Log("Initializing services in order...");

            // Define initialization order (critical services first)
            Type[] initOrder = new Type[]
            {
                typeof(SaveManager),                                    // Must be first (no dependencies)
                typeof(Ascension.Character.Manager.CharacterManager),   // Depends on SaveManager
                typeof(Ascension.Inventory.Manager.InventoryManager),   // Depends on CharacterManager
                typeof(EquipmentManager),                               // Depends on InventoryManager
                typeof(SkillLoadoutManager),                            // Depends on EquipmentManager (skills from weapon)
                typeof(Ascension.App.GameManager),                      // Orchestrator (depends on all)
                typeof(SceneFlowManager)                                // ✅ ADDED: Scene flow manager
            };

            // Initialize services in order
            foreach (Type serviceType in initOrder)
            {
                if (_services.TryGetValue(serviceType, out Component service))
                {
                    if (service is IGameService gameService)
                    {
                        try
                        {
                            gameService.Initialize();
                            Log($"✓ Initialized: {serviceType.Name}");
                        }
                        catch (Exception e)
                        {
                            LogError($"Failed to initialize {serviceType.Name}: {e.Message}\n{e.StackTrace}");
                        }
                    }
                }
                else
                {
                    LogWarning($"Service not found for initialization: {serviceType.Name}");
                }
            }

            // Initialize any remaining services not in the explicit list
            InitializeRemainingServices(initOrder);
        }

        private void InitializeRemainingServices(Type[] alreadyInitialized)
        {
            HashSet<Type> initializedSet = new HashSet<Type>(alreadyInitialized);

            foreach (var kvp in _services)
            {
                if (!initializedSet.Contains(kvp.Key))
                {
                    if (kvp.Value is IGameService gameService)
                    {
                        try
                        {
                            gameService.Initialize();
                            Log($"✓ Initialized (late): {kvp.Key.Name}");
                        }
                        catch (Exception e)
                        {
                            LogError($"Failed to initialize {kvp.Key.Name}: {e.Message}");
                        }
                    }
                }
            }
        }
        #endregion

        #region Public API - Registration
        public void Register<T>(T service) where T : Component
        {
            Type serviceType = typeof(T);
            
            if (_services.ContainsKey(serviceType))
            {
                LogWarning($"Service {serviceType.Name} already registered, replacing...");
            }

            _services[serviceType] = service;
            Log($"✓ Registered: {serviceType.Name}");
        }

        public void Unregister<T>() where T : Component
        {
            Type serviceType = typeof(T);
            
            if (_services.Remove(serviceType))
            {
                Log($"✗ Unregistered: {serviceType.Name}");
            }
        }
        #endregion

        #region Public API - Resolution
        public T Get<T>() where T : Component
        {
            Type serviceType = typeof(T);
            
            if (_services.TryGetValue(serviceType, out Component service))
            {
                return service as T;
            }

            LogWarning($"Service not found: {serviceType.Name}");
            return null;
        }

        public T GetRequired<T>() where T : Component
        {
            T service = Get<T>();
            
            if (service == null)
            {
                throw new InvalidOperationException($"Required service not found: {typeof(T).Name}");
            }

            return service;
        }

        public bool Has<T>() where T : Component
        {
            return _services.ContainsKey(typeof(T));
        }
        #endregion

        #region Validation
        private void ValidateCriticalServices()
        {
            bool allPresent = true;

            allPresent &= ValidateService<Ascension.App.GameManager>("GameManager");
            allPresent &= ValidateService<SaveManager>("SaveManager");
            allPresent &= ValidateService<Ascension.Character.Manager.CharacterManager>("CharacterManager");
            allPresent &= ValidateService<Ascension.Inventory.Manager.InventoryManager>("InventoryManager");
            allPresent &= ValidateService<SceneFlowManager>("SceneFlowManager"); // ✅ ADDED

            if (!allPresent)
            {
                LogError("CRITICAL SERVICES MISSING! Check hierarchy and IGameService implementation.");
            }
            else
            {
                Log("✓ All critical services validated");
            }
        }

        private bool ValidateService<T>(string serviceName) where T : Component
        {
            if (!Has<T>())
            {
                LogError($"Critical service missing: {serviceName}");
                return false;
            }
            return true;
        }

        public string GetSystemStatus()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.AppendLine("=== SERVICE CONTAINER STATUS ===");
            sb.AppendLine($"Discovery Complete: {(_isDiscoveryComplete ? "✓" : "✗")}");
            sb.AppendLine($"Initialized: {(_isInitialized ? "✓" : "✗")}");
            sb.AppendLine($"Total Services: {_services.Count}");
            sb.AppendLine("\nRegistered Services:");

            foreach (var kvp in _services)
            {
                string status = kvp.Value != null ? "✓ Ready" : "✗ Null";
                string hasInterface = kvp.Value is IGameService ? " [IGameService]" : "";
                sb.AppendLine($"  {kvp.Key.Name}: {status}{hasInterface}");
            }

            return sb.ToString();
        }
        #endregion

        #region Logging
        private void Log(string message)
        {
            Debug.Log($"[ServiceContainer] {message}");
        }

        private void LogWarning(string message)
        {
            Debug.LogWarning($"[ServiceContainer] {message}");
        }

        private void LogError(string message)
        {
            Debug.LogError($"[ServiceContainer] {message}");
        }
        #endregion

        #region Debug Tools
        [ContextMenu("Print System Status")]
        private void DebugPrintStatus()
        {
            Debug.Log(GetSystemStatus());
        }

        [ContextMenu("Clear All Services")]
        private void DebugClearServices()
        {
            _services.Clear();
            Log("All services cleared");
        }
        #endregion
    }
}