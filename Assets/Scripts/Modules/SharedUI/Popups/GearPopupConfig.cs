// ════════════════════════════════════════════
// Assets\Scripts\Modules\SharedUI\Popups\GearPopupConfig.cs
// ✅ MINIMAL: Only button labels (no error messages)
// ════════════════════════════════════════════

using UnityEngine;

namespace Ascension.SharedUI.Popups
{
    /// <summary>
    /// Minimal configuration for GearPopup - ONLY button labels.
    /// Error messages are hardcoded in GearPopup.cs since they rarely change.
    /// 
    /// Create via: Assets → Create → UI → GearPopup Config
    /// </summary>
    [CreateAssetMenu(fileName = "GearPopupConfig", menuName = "UI/GearPopup Config")]
    public class GearPopupConfig : ScriptableObject
    {
        #region Button Text

        [Header("Button Labels")]
        [Tooltip("Text shown when item is NOT equipped")]
        public string buttonEquip = "Equip";

        [Tooltip("Text shown when item IS equipped")]
        public string buttonUnequip = "Unequip";

        [Tooltip("Text for moving item from Bag to Storage")]
        public string buttonStore = "Store";

        [Tooltip("Text for moving item from Storage to Bag")]
        public string buttonTake = "Take";

        [Tooltip("Generic move button text")]
        public string buttonMove = "Move";

        #endregion

        #region Display Settings

        [Header("Display Settings")]
        [Tooltip("How long error messages stay visible (in seconds)")]
        [Range(1f, 10f)]
        public float errorDisplayDuration = 3f;

        [Tooltip("Color for error text displayed to player")]
        public Color errorTextColor = Color.red;

        #endregion

        #region Validation

        private void OnValidate()
        {
            // Ensure no empty strings
            if (string.IsNullOrWhiteSpace(buttonEquip)) 
                buttonEquip = "Equip";
            
            if (string.IsNullOrWhiteSpace(buttonUnequip)) 
                buttonUnequip = "Unequip";
            
            if (string.IsNullOrWhiteSpace(buttonStore)) 
                buttonStore = "Store";
            
            if (string.IsNullOrWhiteSpace(buttonTake)) 
                buttonTake = "Take";

            if (string.IsNullOrWhiteSpace(buttonMove))
                buttonMove = "Move";

            // Ensure duration is reasonable
            if (errorDisplayDuration < 1f) 
                errorDisplayDuration = 1f;
        }

        #endregion
    }
}