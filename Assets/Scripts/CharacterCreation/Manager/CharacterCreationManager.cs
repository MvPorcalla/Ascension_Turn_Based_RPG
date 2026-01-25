// ════════════════════════════════════════════════════════════════════════
// Assets\Scripts\CharacterCreation\Manager\CharacterCreationManager.cs
// ✅ Business logic for character creation (NO UI CODE!)
// Handles: validation, attribute allocation, character creation orchestration
// ════════════════════════════════════════════════════════════════════════

using System;
using UnityEngine;
using Ascension.Core;
using Ascension.Character.Core;              // if your CharacterAttributes, stats are here
using Ascension.CharacterCreation.Data;      // <-- instead of Ascension.Character.Data
using Ascension.Data.SO.Character;           // if ScriptableObjects are in Ascension.Data.SO.Character

namespace Ascension.CharacterCreation.Manager
{
    /// <summary>
    /// Manages character creation flow and business logic
    /// UI-agnostic - can be called from any UI implementation
    /// </summary>
    public class CharacterCreationManager : MonoBehaviour
    {
        #region Serialized Fields
        [Header("Configuration")]
        [SerializeField] private CharacterBaseStatsSO baseStats;

        [Header("Settings")]
        [SerializeField] private int minNameLength = 3;
        [SerializeField] private int maxNameLength = 20;
        [SerializeField] private bool enableDebugLogs = true;
        #endregion

        #region Private Fields
        private CharacterCreationData _creationData;
        private bool _isProcessing = false;
        #endregion

        #region Properties
        public CharacterCreationData CreationData => _creationData;
        public CharacterBaseStatsSO BaseStats => baseStats;
        public bool IsProcessing => _isProcessing;
        #endregion

        #region Lifecycle
        private void Awake()
        {
            if (baseStats == null)
            {
                Debug.LogError("[CharacterCreationManager] CharacterBaseStatsSO not assigned!", this);
                enabled = false;
                return;
            }

            _creationData = new CharacterCreationData();
            InitializeCreationData();

            Log("CharacterCreationManager initialized");
        }
        #endregion

        #region Private Helpers - Initialization
        /// <summary>
        /// Initialize creation data with base stats
        /// </summary>
        private void InitializeCreationData()
        {
            _creationData.currentAttributes = new CharacterAttributes(
                baseStats.startingSTR,
                baseStats.startingINT,
                baseStats.startingAGI,
                baseStats.startingEND,
                baseStats.startingWIS
            );

            _creationData.pointsSpent = 0;
            _creationData.characterName = string.Empty;
        }
        #endregion

        #region Public API - Attribute Modification
        /// <summary>
        /// Try to modify an attribute by a given amount
        /// Returns true if successful, false if invalid
        /// </summary>
        public bool TryModifyAttribute(AttributeType attributeType, int change)
        {
            if (_isProcessing)
            {
                LogWarning("Cannot modify attributes while processing character creation");
                return false;
            }

            // Get current value
            int currentValue = _creationData.currentAttributes.GetAttribute(attributeType);
            int minValue = GetBaseAttributeValue(attributeType);
            int newValue = currentValue + change;

            // Validation
            if (newValue < minValue)
            {
                Log($"Cannot reduce {attributeType} below base value ({minValue})");
                return false;
            }

            if (change > 0 && !_creationData.HasPointsToSpend)
            {
                Log("No points remaining to allocate");
                return false;
            }

            if (change < 0 && !_creationData.CanReduce)
            {
                Log("No points to remove");
                return false;
            }

            // Apply change
            _creationData.currentAttributes.SetAttribute(attributeType, newValue);
            
            // ✅ FIXED: Defensive clamping (don't trust validation alone)
            _creationData.pointsSpent = Mathf.Clamp(
                _creationData.pointsSpent + change,
                0,
                _creationData.totalPointsToAllocate
            );

            // Recalculate preview stats
            RecalculatePreviewStats();

            Log($"{attributeType} modified: {currentValue} → {newValue} (Points remaining: {_creationData.PointsRemaining})");
            return true;
        }

        /// <summary>
        /// Recalculate preview combat stats based on current attributes
        /// </summary>
        public void RecalculatePreviewStats()
        {
            _creationData.previewStats.Recalculate(
                baseStats,
                level: 1,
                _creationData.currentAttributes,
                new CharacterItemStats(), // No equipment at creation
                equippedWeapon: null
            );
        }
        #endregion

        #region Public API - Character Creation
        /// <summary>
        /// Validate character creation inputs
        /// Returns validation result with specific error message if failed
        /// </summary>
        public ValidationResult ValidateCharacterCreation(string characterName)
        {
            // Name validation
            if (string.IsNullOrWhiteSpace(characterName))
            {
                return ValidationResult.Fail("Please enter a character name!");
            }

            string trimmedName = characterName.Trim();

            if (trimmedName.Length < minNameLength)
            {
                return ValidationResult.Fail($"Name must be at least {minNameLength} characters!");
            }

            if (trimmedName.Length > maxNameLength)
            {
                return ValidationResult.Fail($"Name must be {maxNameLength} characters or less!");
            }

            // Points allocation validation
            if (!_creationData.AllPointsAllocated)
            {
                int remaining = _creationData.PointsRemaining;
                return ValidationResult.Fail(
                    $"You have {remaining} attribute point{(remaining > 1 ? "s" : "")} remaining!"
                );
            }

            // System validation
            if (GameBootstrap.Instance == null)
            {
                Debug.LogError("[CharacterCreationManager] GameBootstrap not found!");
                return ValidationResult.Fail("Game system not ready. Please restart.");
            }

            if (GameBootstrap.Character == null)
            {
                Debug.LogError("[CharacterCreationManager] CharacterManager not found!");
                return ValidationResult.Fail("Character system not ready. Please restart.");
            }

            if (GameBootstrap.Save == null)
            {
                Debug.LogError("[CharacterCreationManager] SaveManager not found!");
                return ValidationResult.Fail("Save system not ready. Please restart.");
            }

            if (GameBootstrap.SceneFlow == null)
            {
                Debug.LogError("[CharacterCreationManager] SceneFlowManager not found!");
                return ValidationResult.Fail("Scene system not ready. Please restart.");
            }

            return ValidationResult.Success();
        }

        /// <summary>
        /// Create and save the character, then transition to main game
        /// Returns creation result with error message if failed
        /// </summary>
        public CreationResult CreateCharacter(string characterName)
        {
            if (_isProcessing)
            {
                return CreationResult.Fail("Character creation already in progress!");
            }

            _isProcessing = true;

            try
            {
                // Step 1: Validate inputs
                var validationResult = ValidateCharacterCreation(characterName);
                if (!validationResult.IsValid)
                {
                    _isProcessing = false;
                    return CreationResult.Fail(validationResult.ErrorMessage);
                }

                string trimmedName = characterName.Trim();

                Log($"Creating character: {trimmedName}");
                Log($"  Attributes - STR={_creationData.currentAttributes.STR} " +
                    $"INT={_creationData.currentAttributes.INT} " +
                    $"AGI={_creationData.currentAttributes.AGI} " +
                    $"END={_creationData.currentAttributes.END} " +
                    $"WIS={_creationData.currentAttributes.WIS}");

                // Step 2: Create character stats
                CharacterStats newCharacter = new CharacterStats();
                newCharacter.Initialize(baseStats);
                newCharacter.playerName = trimmedName;
                newCharacter.attributes.CopyFrom(_creationData.currentAttributes);
                newCharacter.RecalculateStats(baseStats, fullHeal: true);

                Log($"  Combat Stats - HP={newCharacter.MaxHP:F0} AD={newCharacter.AD:F1} AP={newCharacter.AP:F1}");

                // Step 3: Load character into CharacterManager
                GameBootstrap.Character.LoadPlayer(newCharacter);

                // Step 4: Save character
                bool saveSuccess = GameBootstrap.Save.SaveGame(newCharacter, sessionPlayTime: 0f);

                if (!saveSuccess)
                {
                    Debug.LogError("[CharacterCreationManager] Failed to save character!");
                    _isProcessing = false;
                    return CreationResult.Fail("Failed to save character. Please try again.");
                }

                Log("✓ Character created and saved successfully!");

                // Step 5: Transition to main game (handled by caller after success)
                // Caller should call: GameBootstrap.SceneFlow.LoadMainScene("03_MainBase");

                return CreationResult.FromSuccess(newCharacter);
            }
            catch (Exception e)
            {
                Debug.LogError($"[CharacterCreationManager] Exception during character creation: {e.Message}\n{e.StackTrace}");
                _isProcessing = false;
                return CreationResult.Fail("An unexpected error occurred. Please try again.");
            }
        }

        /// <summary>
        /// Complete character creation and transition to main game
        /// Call this after CreateCharacter() succeeds
        /// </summary>
        public void CompleteCreation()
        {
            if (GameBootstrap.SceneFlow != null)
            {
                // ✅ FIXED: Use constant instead of hard-coded string
                GameBootstrap.SceneFlow.LoadMainScene(Core.SceneFlowManager.SCENE_MAIN_BASE);
            }
            else
            {
                Debug.LogError("[CharacterCreationManager] Cannot transition - SceneFlowManager not found!");
            }
        }
        #endregion

        #region Public API - Reset
        /// <summary>
        /// Reset all attributes to base values
        /// </summary>
        public void ResetAttributes()
        {
            if (_isProcessing)
            {
                LogWarning("Cannot reset while processing");
                return;
            }

            InitializeCreationData();
            RecalculatePreviewStats();

            Log("Character creation data reset to defaults");
        }
        #endregion

        #region Private Helpers
        private int GetBaseAttributeValue(AttributeType attributeType)
        {
            return attributeType switch
            {
                AttributeType.STR => baseStats.startingSTR,
                AttributeType.INT => baseStats.startingINT,
                AttributeType.AGI => baseStats.startingAGI,
                AttributeType.END => baseStats.startingEND,
                AttributeType.WIS => baseStats.startingWIS,
                _ => 0
            };
        }

        private void Log(string message)
        {
            if (enableDebugLogs)
                Debug.Log($"[CharacterCreation] {message}");
        }

        private void LogWarning(string message)
        {
            Debug.LogWarning($"[CharacterCreation] {message}");
        }
        #endregion
    }

    // ════════════════════════════════════════════════════════════════════════
    // RESULT TYPES
    // ════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Result of validation check
    /// </summary>
    public struct ValidationResult
    {
        public bool IsValid { get; }
        public string ErrorMessage { get; }

        private ValidationResult(bool isValid, string errorMessage = "")
        {
            IsValid = isValid;
            ErrorMessage = errorMessage;
        }

        public static ValidationResult Success() => new ValidationResult(true);
        public static ValidationResult Fail(string message) => new ValidationResult(false, message);
    }

    /// <summary>
    /// Result of character creation attempt
    /// </summary>
    public struct CreationResult
    {
        public bool Success { get; }
        public string ErrorMessage { get; }
        public CharacterStats Character { get; }

        private CreationResult(bool success, CharacterStats character = null, string errorMessage = "")
        {
            Success = success;
            Character = character;
            ErrorMessage = errorMessage;
        }

        // ✅ Renamed static factory method to avoid conflict with property
        public static CreationResult FromSuccess(CharacterStats character) => new CreationResult(true, character);
        public static CreationResult Fail(string message) => new CreationResult(false, errorMessage: message);
    }

}