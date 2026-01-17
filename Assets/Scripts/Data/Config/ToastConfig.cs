// ════════════════════════════════════════════
// Assets/Scripts/Modules/SharedUI/Notifications/ToastConfig.cs
// Centralized configuration for toast notifications
// ════════════════════════════════════════════

using UnityEngine;

namespace Ascension.UI.Toast
{
    /// <summary>
    /// Configuration for toast notification appearance and behavior.
    /// Create via: Assets → Create → UI/Notifications → Toast Config
    /// </summary>
    [CreateAssetMenu(fileName = "ToastConfig", menuName = "UI/Configuration/Toast Config")]
    public class ToastConfig : ScriptableObject
    {
        #region Display Settings
        [Header("━━━ DISPLAY SETTINGS ━━━")]
        [Space(5)]
        
        [Tooltip("How long toast stays visible (seconds)")]
        [Range(1f, 10f)]
        public float displayDuration = 3f;

        [Tooltip("Animation speed for fade in/out")]
        [Range(0.1f, 2f)]
        public float animationSpeed = 0.5f;

        [Tooltip("Maximum number of toasts visible at once")]
        [Range(1, 10)]
        public int maxVisibleToasts = 3;

        [Tooltip("Vertical spacing between stacked toasts")]
        [Range(5f, 50f)]
        public float toastSpacing = 10f;

        #endregion

        #region Colors

        [Space(10)]
        [Header("━━━ TOAST COLORS ━━━")]
        [Space(5)]
        
        [Tooltip("Background color for success messages")]
        public Color successColor = new Color(0.2f, 0.8f, 0.2f, 0.9f); // Green

        [Tooltip("Background color for error messages")]
        public Color errorColor = new Color(0.8f, 0.2f, 0.2f, 0.9f); // Red

        [Tooltip("Background color for warning messages")]
        public Color warningColor = new Color(0.9f, 0.7f, 0.2f, 0.9f); // Orange

        [Tooltip("Background color for info messages")]
        public Color infoColor = new Color(0.2f, 0.5f, 0.8f, 0.9f); // Blue

        [Tooltip("Text color (usually white)")]
        public Color textColor = Color.white;

        #endregion

        #region Icons (Optional)

        [Space(10)]
        [Header("━━━ ICONS (Optional) ━━━")]
        [Space(5)]
        
        [Tooltip("Icon for success messages")]
        public Sprite successIcon;

        [Tooltip("Icon for error messages")]
        public Sprite errorIcon;

        [Tooltip("Icon for warning messages")]
        public Sprite warningIcon;

        [Tooltip("Icon for info messages")]
        public Sprite infoIcon;

        #endregion

        #region Sound (Future)

        [Space(10)]
        [Header("━━━ SOUND EFFECTS (Future) ━━━")]
        [Space(5)]
        
        [Tooltip("Sound when toast appears")]
        public AudioClip popSound;

        #endregion

        #region Helper Methods

        /// <summary>
        /// Get color for toast type
        /// </summary>
        public Color GetColorForType(ToastType type)
        {
            return type switch
            {
                ToastType.Success => successColor,
                ToastType.Error => errorColor,
                ToastType.Warning => warningColor,
                ToastType.Info => infoColor,
                _ => infoColor
            };
        }

        /// <summary>
        /// Get icon for toast type (returns null if not set)
        /// </summary>
        public Sprite GetIconForType(ToastType type)
        {
            return type switch
            {
                ToastType.Success => successIcon,
                ToastType.Error => errorIcon,
                ToastType.Warning => warningIcon,
                ToastType.Info => infoIcon,
                _ => null
            };
        }

        #endregion

        #region Validation

        private void OnValidate()
        {
            if (displayDuration < 1f) displayDuration = 1f;
            if (animationSpeed < 0.1f) animationSpeed = 0.1f;
            if (maxVisibleToasts < 1) maxVisibleToasts = 1;
            if (toastSpacing < 5f) toastSpacing = 5f;
        }

        #endregion
    }

    /// <summary>
    /// Types of toast notifications
    /// </summary>
    public enum ToastType
    {
        Success,    // Green - "Equipped Iron Sword"
        Error,      // Red - "Inventory full"
        Warning,    // Orange - "Low health"
        Info        // Blue - "Quest updated"
    }
}