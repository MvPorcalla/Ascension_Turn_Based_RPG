// ════════════════════════════════════════════
// Assets\Scripts\Modules\EquipmentSystem\Data\EquipmentTransaction.cs
// Transaction pattern for atomic multi-step equipment operations
// ════════════════════════════════════════════

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Ascension.Equipment.Data
{
    /// <summary>
    /// Transaction pattern for atomic equipment operations.
    /// If any step fails, all previous steps are automatically rolled back.
    /// 
    /// Usage:
    /// using var transaction = new EquipmentTransaction();
    /// transaction.AddRollback(() => { /* undo action */ });
    /// // ... do work ...
    /// transaction.Commit(); // Success - no rollback
    /// </summary>
    public class EquipmentTransaction : IDisposable
    {
        private readonly List<Action> _rollbackActions = new();
        private bool _committed = false;
        private readonly string _operationName;

        public EquipmentTransaction(string operationName = "Equipment Operation")
        {
            _operationName = operationName;
            Debug.Log($"[Transaction] Started: {_operationName}");
        }

        /// <summary>
        /// Register a rollback action. Actions are executed in reverse order (LIFO).
        /// </summary>
        public void AddRollback(Action rollback)
        {
            if (rollback == null)
            {
                Debug.LogWarning("[Transaction] Attempted to add null rollback action");
                return;
            }

            // Insert at beginning for LIFO execution
            _rollbackActions.Insert(0, rollback);
            Debug.Log($"[Transaction] Registered rollback #{_rollbackActions.Count}");
        }

        /// <summary>
        /// Mark transaction as successful. Prevents rollback on Dispose.
        /// </summary>
        public void Commit()
        {
            _committed = true;
            Debug.Log($"[Transaction] Committed: {_operationName} (no rollback needed)");
            _rollbackActions.Clear();
        }

        /// <summary>
        /// Automatically called when transaction goes out of scope.
        /// Rolls back all actions if not committed.
        /// </summary>
        public void Dispose()
        {
            if (!_committed && _rollbackActions.Count > 0)
            {
                Debug.LogWarning($"[Transaction] Rolling back {_rollbackActions.Count} operations for: {_operationName}");
                
                int successfulRollbacks = 0;
                int failedRollbacks = 0;

                foreach (var rollback in _rollbackActions)
                {
                    try
                    {
                        rollback();
                        successfulRollbacks++;
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"[Transaction] Rollback action failed: {e.Message}\n{e.StackTrace}");
                        failedRollbacks++;
                    }
                }

                Debug.Log($"[Transaction] Rollback complete: {successfulRollbacks} successful, {failedRollbacks} failed");
            }
            else if (_committed)
            {
                // Already committed - no action needed
            }
            else
            {
                // No rollback actions - this is fine
                Debug.Log($"[Transaction] Disposed: {_operationName} (no rollback actions)");
            }
        }

        /// <summary>
        /// Manually trigger rollback (normally automatic via Dispose)
        /// </summary>
        public void Rollback()
        {
            if (_committed)
            {
                Debug.LogWarning("[Transaction] Cannot rollback - transaction already committed");
                return;
            }

            Dispose();
        }
    }
}