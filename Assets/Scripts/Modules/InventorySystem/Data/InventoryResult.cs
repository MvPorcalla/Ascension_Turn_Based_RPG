// ══════════════════════════════════════════════════════════════════
// Scripts/Modules/InventorySystem/Data/InventoryResult.cs
// Result pattern for inventory operations
// ══════════════════════════════════════════════════════════════════

using System;
using Ascension.Inventory.Enums;

namespace Ascension.Inventory.Data
{
    /// <summary>
    /// Result object for inventory operations
    /// Provides detailed feedback instead of just bool
    /// </summary>
    public class InventoryResult
    {
        public bool Success { get; private set; }
        public string Message { get; private set; }
        public ItemInstance AffectedItem { get; private set; }
        public InventoryErrorCode ErrorCode { get; private set; }

        private InventoryResult() { }

        // Static factory methods for clarity
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

        public static InventoryResult Fail(string message, InventoryErrorCode errorCode = InventoryErrorCode.Unknown)
        {
            return new InventoryResult
            {
                Success = false,
                Message = message,
                ErrorCode = errorCode
            };
        }

        public static InventoryResult DatabaseMissing()
        {
            return Fail("Database reference is required", InventoryErrorCode.DatabaseMissing);
        }

        public static InventoryResult ItemNotFound(string itemID)
        {
            return Fail($"Item not found: {itemID}", InventoryErrorCode.ItemNotFound);
        }

        public static InventoryResult NoSpace(ItemLocation location)
        {
            return Fail($"{location} is full", InventoryErrorCode.NoSpace);
        }

        public static InventoryResult InsufficientQuantity(string itemID, int required, int available)
        {
            return Fail($"Not enough {itemID}. Required: {required}, Available: {available}", 
                InventoryErrorCode.InsufficientQuantity);
        }
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
        Unknown = 99
    }
}