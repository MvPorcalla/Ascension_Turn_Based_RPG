// ════════════════════════════════════════════
// Assets/Scripts/Modules/SharedUI/Notifications/ToastManager.cs
// Centralized manager for toast notifications
// ════════════════════════════════════════════

using System.Collections.Generic;
using UnityEngine;
using Ascension.SharedUI.Popups;

namespace Ascension.SharedUI.Notifications
{
    /// <summary>
    /// Manages toast notifications throughout the game.
    /// Automatically subscribes to PopupActionHandler events.
    /// 
    /// Usage: ToastManager.Instance.ShowSuccess("Item equipped!");
    /// </summary>
    public class ToastManager : MonoBehaviour
    {
        #region Singleton
        public static ToastManager Instance { get; private set; }
        #endregion

        #region Serialized Fields
        [Header("Configuration")]
        [SerializeField] private ToastConfig config;

        [Header("Prefab")]
        [SerializeField] private GameObject toastPrefab;

        [Header("Container")]
        [Tooltip("Where toasts appear (usually top-center of screen)")]
        [SerializeField] private RectTransform toastContainer;

        [Header("Auto-Subscribe to Popup Events")]
        [Tooltip("Automatically show toasts for popup actions?")]
        [SerializeField] private bool subscribeToPopupEvents = true;
        #endregion

        #region Private Fields
        private Queue<ToastNotification> toastPool = new Queue<ToastNotification>();
        private List<ToastNotification> activeToasts = new List<ToastNotification>();
        #endregion

        #region Unity Callbacks
        private void Awake()
        {
            InitializeSingleton();
            ValidateReferences();
            PrewarmPool();
            
            if (subscribeToPopupEvents)
            {
                SubscribeToPopupEvents();
            }
        }

        private void OnDestroy()
        {
            if (subscribeToPopupEvents)
            {
                UnsubscribeFromPopupEvents();
            }
        }

        private void InitializeSingleton()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            // DontDestroyOnLoad(gameObject); // ✅ Uncomment if toasts needed across scenes
        }

        private void ValidateReferences()
        {
            if (config == null)
            {
                Debug.LogError("[ToastManager] ToastConfig not assigned!");
            }

            if (toastPrefab == null)
            {
                Debug.LogError("[ToastManager] Toast prefab not assigned!");
            }

            if (toastContainer == null)
            {
                Debug.LogError("[ToastManager] Toast container not assigned!");
            }
        }

        private void PrewarmPool()
        {
            // Create a small pool to avoid runtime instantiation lag
            for (int i = 0; i < 3; i++)
            {
                CreateToastInPool();
            }
        }
        #endregion

        #region Public API - Simple Methods

        /// <summary>
        /// Show a success toast (green)
        /// </summary>
        public void ShowSuccess(string message)
        {
            ShowToast(message, ToastType.Success);
        }

        /// <summary>
        /// Show an error toast (red)
        /// </summary>
        public void ShowError(string message)
        {
            ShowToast(message, ToastType.Error);
        }

        /// <summary>
        /// Show a warning toast (orange)
        /// </summary>
        public void ShowWarning(string message)
        {
            ShowToast(message, ToastType.Warning);
        }

        /// <summary>
        /// Show an info toast (blue)
        /// </summary>
        public void ShowInfo(string message)
        {
            ShowToast(message, ToastType.Info);
        }

        /// <summary>
        /// Show a toast with specific type
        /// </summary>
        public void ShowToast(string message, ToastType type)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                Debug.LogWarning("[ToastManager] Attempted to show empty toast");
                return;
            }

            if (config == null || toastPrefab == null || toastContainer == null)
            {
                Debug.LogWarning($"[ToastManager] Cannot show toast - missing references. Message: {message}");
                return;
            }

            // Get toast from pool
            ToastNotification toast = GetToast();
            
            if (toast == null)
            {
                Debug.LogError("[ToastManager] Failed to get toast from pool");
                return;
            }

            // Add to active list
            activeToasts.Add(toast);

            // Position toast
            RepositionToasts();

            // Show toast
            toast.Show(message, type, config);

            // Subscribe to completion
            toast.OnToastComplete += OnToastCompleted;
        }

        /// <summary>
        /// Clear all active toasts immediately
        /// </summary>
        public void ClearAll()
        {
            foreach (var toast in activeToasts)
            {
                toast.OnToastComplete -= OnToastCompleted;
                ReturnToast(toast);
            }
            
            activeToasts.Clear();
        }

        #endregion

        #region Popup Event Integration

        private void SubscribeToPopupEvents()
        {
            if (PopupActionHandler.Instance != null)
            {
                PopupActionHandler.Instance.OnActionCompleted += HandlePopupSuccess;
                PopupActionHandler.Instance.OnActionFailed += HandlePopupError;
                
                Debug.Log("[ToastManager] Subscribed to PopupActionHandler events");
            }
            else
            {
                Debug.LogWarning("[ToastManager] PopupActionHandler.Instance is null - cannot subscribe");
            }
        }

        private void UnsubscribeFromPopupEvents()
        {
            if (PopupActionHandler.Instance != null)
            {
                PopupActionHandler.Instance.OnActionCompleted -= HandlePopupSuccess;
                PopupActionHandler.Instance.OnActionFailed -= HandlePopupError;
            }
        }

        private void HandlePopupSuccess(string message)
        {
            ShowSuccess(message);
        }

        private void HandlePopupError(string message)
        {
            ShowError(message);
        }

        #endregion

        #region Toast Pool Management

        private ToastNotification GetToast()
        {
            // Check if we hit max visible limit
            if (activeToasts.Count >= config.maxVisibleToasts)
            {
                // Remove oldest toast
                ToastNotification oldest = activeToasts[0];
                oldest.Dismiss();
            }

            // Get from pool or create new
            if (toastPool.Count > 0)
            {
                ToastNotification toast = toastPool.Dequeue();
                toast.gameObject.SetActive(true);
                return toast;
            }
            else
            {
                return CreateToastInPool();
            }
        }

        private void ReturnToast(ToastNotification toast)
        {
            if (toast == null) return;

            toast.gameObject.SetActive(false);
            toast.OnToastComplete -= OnToastCompleted;
            toastPool.Enqueue(toast);
        }

        private ToastNotification CreateToastInPool()
        {
            if (toastPrefab == null || toastContainer == null) return null;

            GameObject obj = Instantiate(toastPrefab, toastContainer);
            obj.SetActive(false);

            ToastNotification toast = obj.GetComponent<ToastNotification>();
            
            if (toast == null)
            {
                Debug.LogError("[ToastManager] Toast prefab missing ToastNotification component!");
                Destroy(obj);
                return null;
            }

            toastPool.Enqueue(toast);
            return toast;
        }

        #endregion

        #region Toast Positioning

        private void RepositionToasts()
        {
            float yOffset = 0f;

            // Stack toasts from top to bottom
            for (int i = activeToasts.Count - 1; i >= 0; i--)
            {
                ToastNotification toast = activeToasts[i];
                
                if (toast != null && toast.RectTransform != null)
                {
                    RectTransform rt = toast.RectTransform;
                    
                    // Position at top with offset
                    rt.anchoredPosition = new Vector2(0f, -yOffset);
                    
                    // Increment offset for next toast
                    yOffset += rt.sizeDelta.y + config.toastSpacing;
                }
            }
        }

        private void OnToastCompleted(ToastNotification toast)
        {
            if (toast == null) return;

            // Remove from active list
            activeToasts.Remove(toast);

            // Return to pool
            ReturnToast(toast);

            // Reposition remaining toasts
            RepositionToasts();
        }

        #endregion
    }
}