// ═══════════════════════════════════════════════════════════════════════════════
// Assets\Scripts\Core\ServiceContainer.cs
// Hybrid DI + Service Locator - Type-safe system registry
// ═══════════════════════════════════════════════════════════════════════════════

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Ascension.Core
{
    /// <summary>
    /// Central service registry using dictionary-based lookups (O(1) performance)
    /// Supports both constructor injection and runtime resolution
    /// </summary>
    public class ServiceContainer : MonoBehaviour
    {
        #region Singleton
        public static ServiceContainer Instance { get; private set; }
        #endregion

        #region System Storage
        // Type-safe dictionary: O(1) lookups vs switch statement
        private readonly Dictionary<Type, Component> _services = new Dictionary<Type, Component>();
        #endregion

        #region Initialization State
        private bool _isInitialized = false;
        public bool IsInitialized => _isInitialized;
        #endregion

        #region Unity Callbacks
        private void Awake()
        {
            if (!InitializeSingleton())
                return;

            AutoDiscoverServices();
        }

        private void Start()
        {
            ValidateCriticalServices();
            _isInitialized = true;
            Log($"Container ready - {_services.Count} services registered");
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

        /// <summary>
        /// Automatically find and register all services from child objects
        /// No more manual string matching!
        /// </summary>
        private void AutoDiscoverServices()
        {
            Log("Auto-discovering services...");

            // Get all components on children
            Component[] allComponents = GetComponentsInChildren<Component>(true);

            // Register known service types
            RegisterServicesOfType<Ascension.App.GameManager>(allComponents);
            RegisterServicesOfType<Ascension.App.SaveManager>(allComponents);
            RegisterServicesOfType<Ascension.Character.Manager.CharacterManager>(allComponents);
            RegisterServicesOfType<Ascension.Inventory.Manager.InventoryManager>(allComponents);
            RegisterServicesOfType<Ascension.GameSystem.PotionManager>(allComponents);
            
            // TODO: Add EquipmentManager when ready
            // RegisterServicesOfType<Ascension.Equipment.Manager.EquipmentManager>(allComponents);

            LogRegisteredServices();
        }

        private void RegisterServicesOfType<T>(Component[] components) where T : Component
        {
            foreach (Component comp in components)
            {
                if (comp is T service)
                {
                    Register(service);
                    return; // Only register first instance
                }
            }
        }
        #endregion

        #region Public API - Registration
        /// <summary>
        /// Manually register a service (for edge cases or testing)
        /// </summary>
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

        /// <summary>
        /// Unregister a service
        /// </summary>
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
        /// <summary>
        /// Get a service by type - O(1) dictionary lookup
        /// Returns null if not found (caller should handle)
        /// </summary>
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

        /// <summary>
        /// Get service with runtime type check
        /// Throws exception if not found (use for critical dependencies)
        /// </summary>
        public T GetRequired<T>() where T : Component
        {
            T service = Get<T>();
            
            if (service == null)
            {
                throw new InvalidOperationException($"Required service not found: {typeof(T).Name}");
            }

            return service;
        }

        /// <summary>
        /// Check if service exists without retrieving it
        /// </summary>
        public bool Has<T>() where T : Component
        {
            return _services.ContainsKey(typeof(T));
        }
        #endregion

        #region Public API - Validation
        /// <summary>
        /// Validate that critical services are registered
        /// </summary>
        private void ValidateCriticalServices()
        {
            bool allPresent = true;

            allPresent &= ValidateService<Ascension.App.GameManager>();
            allPresent &= ValidateService<Ascension.App.SaveManager>();
            allPresent &= ValidateService<Ascension.Character.Manager.CharacterManager>();
            allPresent &= ValidateService<Ascension.Inventory.Manager.InventoryManager>();

            if (!allPresent)
            {
                LogError("CRITICAL SERVICES MISSING! Check hierarchy.");
            }
        }

        private bool ValidateService<T>() where T : Component
        {
            if (!Has<T>())
            {
                LogError($"Critical service missing: {typeof(T).Name}");
                return false;
            }
            return true;
        }

        /// <summary>
        /// Get detailed status for debugging
        /// </summary>
        public string GetSystemStatus()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.AppendLine("=== SERVICE CONTAINER STATUS ===");
            sb.AppendLine($"Initialized: {(_isInitialized ? "✓" : "✗")}");
            sb.AppendLine($"Total Services: {_services.Count}");
            sb.AppendLine("\nRegistered Services:");

            foreach (var kvp in _services)
            {
                string status = kvp.Value != null ? "✓ Ready" : "✗ Null";
                sb.AppendLine($"  {kvp.Key.Name}: {status}");
            }

            return sb.ToString();
        }
        #endregion

        #region Private Helpers
        private void LogRegisteredServices()
        {
            Log("=== Registered Services ===");
            foreach (var kvp in _services)
            {
                Log($"  {kvp.Key.Name}: {(kvp.Value != null ? "✓" : "✗")}");
            }
        }

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