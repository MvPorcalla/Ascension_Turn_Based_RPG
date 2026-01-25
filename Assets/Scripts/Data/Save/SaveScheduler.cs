// ════════════════════════════════════════════════════════════════════════
// Assets/Scripts/Core/SaveScheduler.cs
// ✅ NEW: Handles save timing, debouncing, and dirty flag tracking
// Attach to: GameBootstrap GameObject (as a child component)
// ════════════════════════════════════════════════════════════════════════

using UnityEngine;
using Ascension.Core;
using Ascension.Data.Enums;

namespace Ascension.Core
{
    /// <summary>
    /// Manages when and how saves are triggered
    /// - Event-based saves (inventory change, level up, etc.)
    /// - Scene-based saves (entering new room, exiting storage)
    /// - Safety timer (backup auto-save every 45s if dirty)
    /// - Debouncing (prevents spam saves within 2 seconds)
    /// </summary>
    public class SaveScheduler : MonoBehaviour
    {
        #region Serialized Fields
        [Header("Save Timing Settings")]
        [SerializeField] private float autoSaveInterval = 45f; // Safety net timer
        [SerializeField] private float minSaveInterval = 2f;   // Debounce time
        
        [Header("Platform Settings")]
        [Tooltip("Enable aggressive save on mobile (app pause/quit)")]
        [SerializeField] private bool enableMobileSafety = true;
        
        [Header("Debug")]
        [SerializeField] private bool enableDebugLogs = true;
        [SerializeField] private bool logSaveTriggers = true;
        #endregion
        
        #region Private Fields
        private SaveDataCategory _dirtyFlags = SaveDataCategory.None;
        private float _lastSaveTime = 0f;
        private float _timeSinceLastChange = 0f;
        private bool _saveRequested = false;
        private bool _isInitialized = false;
        
        // Combat safety
        private bool _isInCombatTurn = false;
        private bool _isInAnimation = false;
        #endregion
        
        #region Properties
        public bool IsDirty => _dirtyFlags != SaveDataCategory.None;
        public bool CanSaveNow => !_isInCombatTurn && !_isInAnimation;
        #endregion
        
        #region Initialization
        /// <summary>
        /// Called by GameBootstrap during initialization
        /// </summary>
        public void Init()
        {
            SubscribeToEvents();
            _lastSaveTime = Time.time;
            _isInitialized = true;
            
            Log("SaveScheduler initialized");
            Log($"Auto-save interval: {autoSaveInterval}s, Min interval: {minSaveInterval}s");
        }
        
        private void OnDestroy()
        {
            UnsubscribeFromEvents();
        }
        #endregion
        
        #region Unity Callbacks
        private void Update()
        {
            if (!_isInitialized) return;
            
            UpdateTimers();
            ProcessSaveRequests();
        }
        
        private void LateUpdate()
        {
            if (!_isInitialized) return;
            
            // Process queued saves at end of frame
            if (_saveRequested && CanSaveNow)
            {
                ExecuteSave(forceSave: false);
                _saveRequested = false;
            }
        }
        
        /// <summary>
        /// ✅ CRITICAL: Save on app pause/quit (mobile safety)
        /// </summary>
        private void OnApplicationPause(bool pauseStatus)
        {
            if (!enableMobileSafety) return;
            
            if (pauseStatus && IsDirty) // App going to background
            {
                Log("App pausing - forcing save");
                ExecuteSave(forceSave: true);
            }
        }
        
        /// <summary>
        /// ✅ CRITICAL: Save on app quit
        /// </summary>
        private void OnApplicationQuit()
        {
            if (IsDirty)
            {
                Log("App quitting - forcing save");
                ExecuteSave(forceSave: true);
            }
        }
        #endregion
        
        #region Event Subscriptions
        private void SubscribeToEvents()
        {
            // ═══════════════════════════════════════════════════════════
            // TIER 1: Critical Events (force save, ignore debounce)
            // ═══════════════════════════════════════════════════════════
            GameEvents.OnLevelUp += OnLevelUp;
            GameEvents.OnGearEquipped += OnEquipmentChanged;
            GameEvents.OnGearUnequipped += OnEquipmentUnequipped;
            GameEvents.OnSkillLoadoutChanged += OnSkillLoadoutChanged;
            
            // ═══════════════════════════════════════════════════════════
            // TIER 2: Important Events (respect debounce)
            // ═══════════════════════════════════════════════════════════
            GameEvents.OnItemAdded += OnItemAdded;
            GameEvents.OnItemRemoved += OnItemRemoved;
            GameEvents.OnExperienceGained += OnExperienceGained;
            GameEvents.OnHealthChanged += OnHealthChanged;
            
            // ═══════════════════════════════════════════════════════════
            // Scene Events (high priority)
            // ═══════════════════════════════════════════════════════════
            GameEvents.OnSceneChanging += OnSceneChanging;
            
            Log("Subscribed to save trigger events");
        }
        
        private void UnsubscribeFromEvents()
        {
            GameEvents.OnLevelUp -= OnLevelUp;
            GameEvents.OnGearEquipped -= OnEquipmentChanged;
            GameEvents.OnGearUnequipped -= OnEquipmentUnequipped;
            GameEvents.OnSkillLoadoutChanged -= OnSkillLoadoutChanged;
            GameEvents.OnItemAdded -= OnItemAdded;
            GameEvents.OnItemRemoved -= OnItemRemoved;
            GameEvents.OnExperienceGained -= OnExperienceGained;
            GameEvents.OnHealthChanged -= OnHealthChanged;
            GameEvents.OnSceneChanging -= OnSceneChanging;
        }
        #endregion
        
        #region Event Handlers
        
        // ═══════════════════════════════════════════════════════════════
        // TIER 1: Critical Events (force save immediately)
        // ═══════════════════════════════════════════════════════════════
        
        private void OnLevelUp(int newLevel)
        {
            LogTrigger($"Level up to {newLevel}");
            MarkDirty(SaveDataCategory.Character);
            RequestSave(forceSave: true);
        }
        
        private void OnEquipmentChanged(Equipment.Enums.GearSlotType slot, Data.SO.Item.ItemBaseSO item)
        {
            LogTrigger($"Equipped {item.ItemName} to {slot}");
            MarkDirty(SaveDataCategory.Equipment | SaveDataCategory.Character);
            RequestSave(forceSave: true);
        }
        
        private void OnEquipmentUnequipped(Equipment.Enums.GearSlotType slot)
        {
            LogTrigger($"Unequipped {slot}");
            MarkDirty(SaveDataCategory.Equipment | SaveDataCategory.Character);
            RequestSave(forceSave: true);
        }
        
        private void OnSkillLoadoutChanged()
        {
            LogTrigger("Skill loadout changed");
            MarkDirty(SaveDataCategory.Skills);
            RequestSave(forceSave: true);
        }
        
        // ═══════════════════════════════════════════════════════════════
        // TIER 2: Important Events (respect debounce)
        // ═══════════════════════════════════════════════════════════════
        
        private void OnItemAdded(Inventory.Data.ItemInstance item)
        {
            LogTrigger($"Item added: {item.itemID}");
            MarkDirty(SaveDataCategory.Inventory);
            RequestSave(forceSave: false);
        }
        
        private void OnItemRemoved(Inventory.Data.ItemInstance item)
        {
            LogTrigger($"Item removed: {item.itemID}");
            MarkDirty(SaveDataCategory.Inventory);
            RequestSave(forceSave: false);
        }
        
        private void OnExperienceGained(int gained, int newTotal)
        {
            LogTrigger($"EXP gained: +{gained}");
            MarkDirty(SaveDataCategory.Character);
            RequestSave(forceSave: false);
        }
        
        private void OnHealthChanged(float current, float max)
        {
            // Only save if significant HP change (> 10% of max)
            float percentChange = Mathf.Abs(current - max) / max;
            if (percentChange > 0.1f)
            {
                LogTrigger($"HP changed: {current:F0}/{max:F0}");
                MarkDirty(SaveDataCategory.Character);
                RequestSave(forceSave: false);
            }
        }
        
        // ═══════════════════════════════════════════════════════════════
        // Scene Events (high priority)
        // ═══════════════════════════════════════════════════════════════
        
        private void OnSceneChanging(string sceneName)
        {
            LogTrigger($"Scene changing to {sceneName}");
            MarkDirty(SaveDataCategory.All); // Mark everything dirty on scene change
            RequestSave(forceSave: true);
        }
        
        #endregion
        
        #region Public API
        
        /// <summary>
        /// Manually mark data as dirty (for external systems)
        /// </summary>
        public void MarkDirty(SaveDataCategory category)
        {
            _dirtyFlags |= category;
            _timeSinceLastChange = 0f;
        }
        
        /// <summary>
        /// Manually request a save
        /// </summary>
        public void RequestSave(bool forceSave = false)
        {
            if (forceSave)
            {
                ExecuteSave(forceSave: true);
            }
            else
            {
                _saveRequested = true;
            }
        }
        
        /// <summary>
        /// Set combat turn state (prevents saves during active turns)
        /// </summary>
        public void SetCombatTurnState(bool inTurn)
        {
            _isInCombatTurn = inTurn;
            Log($"Combat turn: {(inTurn ? "ACTIVE" : "INACTIVE")}");
        }
        
        /// <summary>
        /// Set animation state (prevents saves during animations)
        /// </summary>
        public void SetAnimationState(bool inAnimation)
        {
            _isInAnimation = inAnimation;
        }
        
        /// <summary>
        /// Force save immediately (for UI buttons, quit game, etc.)
        /// </summary>
        public void ForceSaveNow()
        {
            if (IsDirty)
            {
                ExecuteSave(forceSave: true);
            }
            else
            {
                Log("No dirty data - skipping force save");
            }
        }
        
        #endregion
        
        #region Private Methods - Save Execution
        
        private void UpdateTimers()
        {
            _timeSinceLastChange += Time.deltaTime;
            
            // Safety timer: Auto-save every X seconds if dirty
            if (IsDirty && (Time.time - _lastSaveTime) >= autoSaveInterval)
            {
                LogTrigger($"Safety timer ({autoSaveInterval}s)");
                ExecuteSave(forceSave: false);
            }
        }
        
        private void ProcessSaveRequests()
        {
            // This runs in Update() - LateUpdate() will handle queued saves
        }
        
        /// <summary>
        /// Execute the actual save operation
        /// </summary>
        private void ExecuteSave(bool forceSave)
        {
            // Safety check 1: Not in combat/animation (unless forced)
            if (!forceSave && !CanSaveNow)
            {
                Log("Save blocked - in combat turn or animation");
                return;
            }
            
            // Safety check 2: Debounce (unless forced)
            if (!forceSave && (Time.time - _lastSaveTime) < minSaveInterval)
            {
                Log($"Save debounced - waiting {minSaveInterval - (Time.time - _lastSaveTime):F1}s");
                return;
            }
            
            // Safety check 3: Must have dirty data
            if (!IsDirty)
            {
                return;
            }
            
            // Validate managers
            if (GameBootstrap.Save == null || GameBootstrap.Character == null)
            {
                Debug.LogError("[SaveScheduler] Required managers not available!");
                return;
            }
            
            if (!GameBootstrap.Character.HasActivePlayer)
            {
                Debug.LogWarning("[SaveScheduler] No active player - cannot save");
                return;
            }
            
            // Calculate play time since last save
            float sessionPlayTime = Time.time - _lastSaveTime;
            
            // Execute save
            bool success = GameBootstrap.Save.SaveGame(
                GameBootstrap.Character.CurrentPlayer,
                sessionPlayTime
            );
            
            if (success)
            {
                LogSave();
                
                // Clear dirty flags
                _dirtyFlags = SaveDataCategory.None;
                _lastSaveTime = Time.time;
                _timeSinceLastChange = 0f;
            }
            else
            {
                Debug.LogError("[SaveScheduler] Save failed!");
            }
        }
        
        #endregion
        
        #region Logging
        
        private void Log(string message)
        {
            if (enableDebugLogs)
                Debug.Log($"[SaveScheduler] {message}");
        }
        
        private void LogTrigger(string trigger)
        {
            if (logSaveTriggers)
                Debug.Log($"[SaveScheduler] 💾 Trigger: {trigger}");
        }
        
        private void LogSave()
        {
            string categories = GetDirtyCategoriesString();
            Log($"✅ SAVED - Categories: {categories}");
        }
        
        private string GetDirtyCategoriesString()
        {
            if (_dirtyFlags == SaveDataCategory.None)
                return "None";
            
            if (_dirtyFlags == SaveDataCategory.All)
                return "All";
            
            System.Collections.Generic.List<string> parts = new System.Collections.Generic.List<string>();
            
            if ((_dirtyFlags & SaveDataCategory.Character) != 0) parts.Add("Character");
            if ((_dirtyFlags & SaveDataCategory.Inventory) != 0) parts.Add("Inventory");
            if ((_dirtyFlags & SaveDataCategory.Equipment) != 0) parts.Add("Equipment");
            if ((_dirtyFlags & SaveDataCategory.Skills) != 0) parts.Add("Skills");
            if ((_dirtyFlags & SaveDataCategory.Quest) != 0) parts.Add("Quest");
            if ((_dirtyFlags & SaveDataCategory.World) != 0) parts.Add("World");
            
            return string.Join(", ", parts.ToArray());
        }
        
        #endregion
        
        #region Debug Methods
        
        [ContextMenu("Debug: Print Save State")]
        private void DebugPrintState()
        {
            Debug.Log("=== SAVE SCHEDULER STATE ===");
            Debug.Log($"Is Dirty: {IsDirty}");
            Debug.Log($"Dirty Categories: {GetDirtyCategoriesString()}");
            Debug.Log($"Last Save: {Time.time - _lastSaveTime:F1}s ago");
            Debug.Log($"Time Since Change: {_timeSinceLastChange:F1}s");
            Debug.Log($"Can Save Now: {CanSaveNow}");
            Debug.Log($"In Combat Turn: {_isInCombatTurn}");
            Debug.Log($"In Animation: {_isInAnimation}");
        }
        
        [ContextMenu("Debug: Force Save Now")]
        private void DebugForceSave()
        {
            if (Application.isPlaying)
            {
                ForceSaveNow();
            }
        }
        
        [ContextMenu("Debug: Mark All Dirty")]
        private void DebugMarkDirty()
        {
            if (Application.isPlaying)
            {
                MarkDirty(SaveDataCategory.All);
                Debug.Log("[SaveScheduler] Marked all categories dirty");
            }
        }
        
        #endregion
    }
}