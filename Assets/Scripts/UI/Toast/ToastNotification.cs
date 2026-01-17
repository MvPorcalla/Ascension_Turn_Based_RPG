// ════════════════════════════════════════════
// Assets/Scripts/Modules/SharedUI/Notifications/ToastNotification.cs
// Individual toast notification component
// ════════════════════════════════════════════

using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Ascension.UI.Toast
{
    /// <summary>
    /// Individual toast notification that displays a message briefly.
    /// Managed by ToastManager - don't instantiate directly.
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public class ToastNotification : MonoBehaviour
    {
        #region Events
        public event Action<ToastNotification> OnToastComplete;
        #endregion

        #region Serialized Fields
        [Header("UI References")]
        [SerializeField] private Image background;
        [SerializeField] private TMP_Text messageText;
        [SerializeField] private Image icon;
        #endregion

        #region Private Fields
        private CanvasGroup canvasGroup;
        private RectTransform rectTransform;
        private Coroutine displayCoroutine;
        #endregion

        #region Properties
        public RectTransform RectTransform
        {
            get
            {
                if (rectTransform == null)
                    rectTransform = GetComponent<RectTransform>();
                return rectTransform;
            }
        }
        #endregion

        #region Unity Callbacks
        private void Awake()
        {
            canvasGroup = GetComponent<CanvasGroup>();
            rectTransform = GetComponent<RectTransform>();
            
            // Start invisible
            canvasGroup.alpha = 0f;
        }
        #endregion

        #region Public Methods

        /// <summary>
        /// Show toast with message and configuration
        /// </summary>
        public void Show(string message, ToastType type, ToastConfig config)
        {
            if (config == null)
            {
                Debug.LogError("[ToastNotification] Config is null!");
                return;
            }

            // Set message
            if (messageText != null)
            {
                messageText.text = message;
                messageText.color = config.textColor;
            }

            // Set background color
            if (background != null)
            {
                background.color = config.GetColorForType(type);
            }

            // Set icon (optional)
            if (icon != null)
            {
                Sprite iconSprite = config.GetIconForType(type);
                
                if (iconSprite != null)
                {
                    icon.sprite = iconSprite;
                    icon.enabled = true;
                }
                else
                {
                    icon.enabled = false;
                }
            }

            // Start display animation
            if (displayCoroutine != null)
            {
                StopCoroutine(displayCoroutine);
            }
            
            displayCoroutine = StartCoroutine(DisplaySequence(config));
        }

        /// <summary>
        /// Force dismiss this toast immediately
        /// </summary>
        public void Dismiss()
        {
            if (displayCoroutine != null)
            {
                StopCoroutine(displayCoroutine);
                displayCoroutine = null;
            }

            OnToastComplete?.Invoke(this);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Animation sequence: Fade in → Wait → Fade out
        /// </summary>
        private IEnumerator DisplaySequence(ToastConfig config)
        {
            // Phase 1: Fade In
            yield return FadeIn(config.animationSpeed);

            // Phase 2: Stay Visible
            yield return new WaitForSeconds(config.displayDuration);

            // Phase 3: Fade Out
            yield return FadeOut(config.animationSpeed);

            // Notify completion
            OnToastComplete?.Invoke(this);
        }

        private IEnumerator FadeIn(float speed)
        {
            float elapsed = 0f;
            
            while (elapsed < speed)
            {
                elapsed += Time.deltaTime;
                canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / speed);
                yield return null;
            }
            
            canvasGroup.alpha = 1f;
        }

        private IEnumerator FadeOut(float speed)
        {
            float elapsed = 0f;
            
            while (elapsed < speed)
            {
                elapsed += Time.deltaTime;
                canvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / speed);
                yield return null;
            }
            
            canvasGroup.alpha = 0f;
        }

        #endregion

        #region Cleanup
        private void OnDestroy()
        {
            if (displayCoroutine != null)
            {
                StopCoroutine(displayCoroutine);
            }
        }
        #endregion
    }
}