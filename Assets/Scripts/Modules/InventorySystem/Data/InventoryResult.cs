// ══════════════════════════════════════════════════════════════════
// Scripts/Modules/InventorySystem/Data/InventoryResult.cs
// Result pattern for inventory operations
// ══════════════════════════════════════════════════════════════════

using System;
using Ascension.Inventory.Enums;

namespace Ascension.Inventory.Data
{
    /// <summary>
    /// Result object for inventory operations.
    /// Provides detailed feedback instead of just bool.
    /// </summary>
    public class InventoryResult
    {
        public bool Success { get; private set; }
        public string Message { get; private set; }
        public ItemInstance AffectedItem { get; private set; }
        public InventoryErrorCode ErrorCode { get; private set; }

        private InventoryResult() { }

        #region Static Factory Methods

        /// <summary>
        /// Create a successful result
        /// </summary>
        public static InventoryResult Ok(ItemInstance item = null, string message = null)
        {
            return new InventoryResult
            {
                Success = true,
                AffectedItem = item,
                Message = message ?? "Operation successful",
                ErrorCode = InventoryErrorCode.None
            };
        }

        /// <summary>
        /// Create a failed result
        /// </summary>
        public static InventoryResult Fail(string message, InventoryErrorCode errorCode = InventoryErrorCode.Unknown)
        {
            return new InventoryResult
            {
                Success = false,
                Message = message,
                ErrorCode = errorCode
            };
        }

        /// <summary>
        /// Database reference missing error
        /// </summary>
        public static InventoryResult DatabaseMissing()
        {
            return Fail("Database reference is required", InventoryErrorCode.DatabaseMissing);
        }

        /// <summary>
        /// Item not found in database
        /// </summary>
        public static InventoryResult ItemNotFound(string itemID)
        {
            return Fail($"Item not found: {itemID}", InventoryErrorCode.ItemNotFound);
        }

        /// <summary>
        /// Location is full
        /// </summary>
        public static InventoryResult NoSpace(ItemLocation location)
        {
            return Fail($"{location} is full", InventoryErrorCode.NoSpace);
        }

        /// <summary>
        /// Not enough items to complete operation
        /// </summary>
        public static InventoryResult InsufficientQuantity(string itemID, int required, int available)
        {
            return Fail($"Not enough {itemID}. Required: {required}, Available: {available}", 
                InventoryErrorCode.InsufficientQuantity);
        }

        #endregion

        #region Fluent API

        /// <summary>
        /// ✅ NEW: Override the message (useful for custom feedback)
        /// </summary>
        /// <example>
        /// var result = inventory.AddItem("potion", 5)
        ///     .WithMessage("Health potions added!");
        /// </example>
        public InventoryResult WithMessage(string message)
        {
            Message = message;
            return this;
        }

        /// <summary>
        /// ✅ NEW: Override the affected item (useful for chaining)
        /// </summary>
        public InventoryResult WithItem(ItemInstance item)
        {
            AffectedItem = item;
            return this;
        }

        #endregion

        #region Error Code Checks

        /// <summary>
        /// ✅ NEW: Check if error is NoSpace
        /// </summary>
        /// <example>
        /// if (result.IsNoSpace())
        /// {
        ///     UI.ShowUpgradeDialog();
        /// }
        /// </example>
        public bool IsNoSpace() => ErrorCode == InventoryErrorCode.NoSpace;

        /// <summary>
        /// ✅ NEW: Check if error is ItemNotFound
        /// </summary>
        public bool IsItemNotFound() => ErrorCode == InventoryErrorCode.ItemNotFound;

        /// <summary>
        /// ✅ NEW: Check if error is InsufficientQuantity
        /// </summary>
        public bool IsInsufficientQuantity() => ErrorCode == InventoryErrorCode.InsufficientQuantity;

        /// <summary>
        /// ✅ NEW: Check if error is DatabaseMissing
        /// </summary>
        public bool IsDatabaseMissing() => ErrorCode == InventoryErrorCode.DatabaseMissing;

        /// <summary>
        /// ✅ NEW: Check if error is AlreadyInLocation
        /// </summary>
        public bool IsAlreadyInLocation() => ErrorCode == InventoryErrorCode.AlreadyInLocation;

        /// <summary>
        /// ✅ NEW: Check if error is InvalidOperation
        /// </summary>
        public bool IsInvalidOperation() => ErrorCode == InventoryErrorCode.InvalidOperation;

        #endregion

        #region Critical Operations

        /// <summary>
        /// ✅ NEW: Throw exception if operation failed (for critical operations)
        /// Use this when the operation MUST succeed or the game state is invalid.
        /// </summary>
        /// <example>
        /// // Quest-critical item that must be added
        /// inventory.AddItem("quest_key_item", 1)
        ///     .ThrowIfFailed();
        /// </example>
        /// <exception cref="InvalidOperationException">Thrown if Success is false</exception>
        public InventoryResult ThrowIfFailed()
        {
            if (!Success)
            {
                throw new InvalidOperationException(
                    $"Inventory operation failed: {Message} (ErrorCode: {ErrorCode})"
                );
            }
            return this;
        }

        /// <summary>
        /// ✅ NEW: Execute action only if operation succeeded
        /// </summary>
        /// <example>
        /// inventory.AddItem("gold", 100)
        ///     .OnSuccess(result => UI.ShowMessage($"Added {result.AffectedItem.quantity} gold!"));
        /// </example>
        public InventoryResult OnSuccess(Action<InventoryResult> action)
        {
            if (Success)
            {
                action?.Invoke(this);
            }
            return this;
        }

        /// <summary>
        /// ✅ NEW: Execute action only if operation failed
        /// </summary>
        /// <example>
        /// inventory.RemoveItem(item, 5)
        ///     .OnFailure(result => UI.ShowError(result.Message));
        /// </example>
        public InventoryResult OnFailure(Action<InventoryResult> action)
        {
            if (!Success)
            {
                action?.Invoke(this);
            }
            return this;
        }

        #endregion

        #region Logging Helpers

        /// <summary>
        /// ✅ NEW: Log result to console (useful for debugging)
        /// </summary>
        public InventoryResult Log()
        {
            if (Success)
            {
                UnityEngine.Debug.Log($"[InventoryResult] ✅ {Message}");
            }
            else
            {
                UnityEngine.Debug.LogWarning($"[InventoryResult] ❌ {Message} (ErrorCode: {ErrorCode})");
            }
            return this;
        }

        #endregion

        #region Conversion Helpers

        /// <summary>
        /// ✅ NEW: Convert to nullable AffectedItem (returns null if failed)
        /// </summary>
        /// <example>
        /// var item = inventory.AddItem("potion", 1).GetItemOrNull();
        /// if (item != null) { /* use item */ }
        /// </example>
        public ItemInstance GetItemOrNull()
        {
            return Success ? AffectedItem : null;
        }

        /// <summary>
        /// ✅ NEW: Get item or throw exception
        /// </summary>
        public ItemInstance GetItemOrThrow()
        {
            ThrowIfFailed();
            return AffectedItem;
        }

        #endregion

        #region New Factory Methods
        
        /// <summary>
        /// Bag is full (world loot should stay in dungeon panel)
        /// </summary>
        public static InventoryResult BagFull()
        {
            return Fail("Bag is full", InventoryErrorCode.BagFull);
        }
        
        /// <summary>
        /// Storage is full (rare, but possible)
        /// </summary>
        public static InventoryResult StorageFull()
        {
            return Fail("Storage is full", InventoryErrorCode.StorageFull);
        }
        
        #endregion
        
        #region New Error Checks
        
        /// <summary>
        /// Check if error is BagFull specifically
        /// </summary>
        public bool IsBagFull() => ErrorCode == InventoryErrorCode.BagFull;
        
        /// <summary>
        /// Check if error is StorageFull specifically
        /// </summary>
        public bool IsStorageFull() => ErrorCode == InventoryErrorCode.StorageFull;
        
        #endregion

    }

    /// <summary>
    /// Error codes for programmatic error handling
    /// </summary>
    public enum InventoryErrorCode
    {
        None = 0,
        DatabaseMissing = 1,
        ItemNotFound = 2,
        NoSpace = 3,
        InsufficientQuantity = 4,
        InvalidOperation = 5,
        AlreadyInLocation = 6,
        BagFull = 7,                // Bag specifically is full
        StorageFull = 8,            // Storage specifically is full
        Unknown = 99
    }
}