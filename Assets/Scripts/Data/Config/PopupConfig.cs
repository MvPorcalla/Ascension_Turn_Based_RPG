// ════════════════════════════════════════════
// Assets\Scripts\Modules\SharedUI\Popups\PopupConfig.cs
// Unified configuration for all popup types
// ════════════════════════════════════════════

using UnityEngine;
using Ascension.Data.SO.Item;
using Ascension.Inventory.Enums;

namespace Ascension.UI.Popups
{
    /// <summary>
    /// Unified configuration for all popup types (Gear, Item, Potion).
    /// Single ScriptableObject manages all popup UI text and settings.
    /// 
    /// Create via: Assets → Create → UI/Popups → Popup Config
    /// </summary>
    [CreateAssetMenu(fileName = "PopupConfig", menuName = "UI/Configuration/Popup Config")]
    public class PopupConfig : ScriptableObject
    {
        #region Equipment Actions (GearPopup)
        
        [Header("━━━ GEAR POPUP (Weapons/Armor) ━━━")]
        [Space(5)]
        
        [Header("Equipment Actions")]
        [Tooltip("Text shown when item is NOT equipped")]
        public string buttonEquip = "Equip";

        [Tooltip("Text shown when item IS equipped")]
        public string buttonUnequip = "Unequip";

        #endregion

        #region Movement Actions (All Popups)

        [Space(10)]
        [Header("━━━ SHARED MOVEMENT ACTIONS ━━━")]
        [Space(5)]
        
        [Header("Movement Actions")]
        [Tooltip("Text for moving item from Storage to Bag")]
        public string buttonAddToBag = "Add to Bag";
        
        [Tooltip("Text for moving item from Bag to Storage")]
        public string buttonStore = "Store";

        [Tooltip("Alternative: 'Take' (from storage)")]
        public string buttonTake = "Take";

        [Tooltip("Generic move button text (fallback)")]
        public string buttonMove = "Move";

        #endregion

        #region Consumption Actions (PotionPopup)

        [Space(10)]
        [Header("━━━ POTION POPUP (Consumables) ━━━")]
        [Space(5)]
        
        [Header("Consumption Actions")]
        [Tooltip("Text for using/consuming a potion")]
        public string buttonUse = "Use";

        [Tooltip("Text for using multiple potions")]
        public string buttonUseMultiple = "Use All";

        #endregion

        #region Display Labels

        [Space(10)]
        [Header("Display Labels")]
        [Tooltip("Label for quantity display")]
        public string labelQuantity = "Quantity:";

        [Tooltip("Label for potion type display")]
        public string labelPotionType = "Potion Type";

        #endregion

        #region Potion Type Names

        [Space(10)]
        [Header("Potion Type Display Names")]
        public string nameHealthPotion = "Health Potion";
        public string nameManaPotion = "Mana Potion";
        public string nameBuffPotion = "Buff Potion";
        public string nameElixir = "Elixir";
        public string nameAntidote = "Antidote";
        public string nameRejuvenation = "Rejuvenation";
        public string nameUtility = "Utility";
        public string nameUnknown = "Unknown";

        #endregion

        #region Quantity Controls

        [Space(10)]
        [Header("━━━ QUANTITY SETTINGS ━━━")]
        [Space(5)]
        
        [Header("Quantity Control Settings")]
        [Tooltip("Small increment/decrement (±1 button)")]
        [Range(1, 10)]
        public int quickAdjustSmall = 1;

        [Tooltip("Large increment/decrement (±5 button)")]
        [Range(1, 100)]
        public int quickAdjustLarge = 5;

        #endregion

        #region Error Display

        [Space(10)]
        [Header("━━━ ERROR DISPLAY SETTINGS ━━━")]
        [Space(5)]
        
        [Header("Error Display")]
        [Tooltip("How long error messages stay visible (in seconds)")]
        [Range(1f, 10f)]
        public float errorDisplayDuration = 3f;

        [Tooltip("Color for error text")]
        public Color errorTextColor = Color.red;

        #endregion

        #region Helper Methods

        /// <summary>
        /// Get display name for a potion type
        /// </summary>
        public string GetPotionTypeName(PotionType type)
        {
            return type switch
            {
                PotionType.HealthPotion => nameHealthPotion,
                PotionType.ManaPotion => nameManaPotion,
                PotionType.BuffPotion => nameBuffPotion,
                PotionType.Elixir => nameElixir,
                PotionType.Antidote => nameAntidote,
                PotionType.Rejuvenation => nameRejuvenation,
                PotionType.Utility => nameUtility,
                _ => nameUnknown
            };
        }

        /// <summary>
        /// Get appropriate movement button text based on item location
        /// </summary>
        public string GetMoveButtonText(ItemLocation location, bool useTakeInsteadOfAdd = false)
        {
            return location switch
            {
                ItemLocation.Bag => buttonStore,
                ItemLocation.Storage => useTakeInsteadOfAdd ? buttonTake : buttonAddToBag,
                _ => buttonMove
            };
        }

        #endregion

        #region Validation

        private void OnValidate()
        {
            // Equipment actions
            if (string.IsNullOrWhiteSpace(buttonEquip)) buttonEquip = "Equip";
            if (string.IsNullOrWhiteSpace(buttonUnequip)) buttonUnequip = "Unequip";

            // Movement actions
            if (string.IsNullOrWhiteSpace(buttonAddToBag)) buttonAddToBag = "Add to Bag";
            if (string.IsNullOrWhiteSpace(buttonStore)) buttonStore = "Store";
            if (string.IsNullOrWhiteSpace(buttonTake)) buttonTake = "Take";
            if (string.IsNullOrWhiteSpace(buttonMove)) buttonMove = "Move";

            // Consumption actions
            if (string.IsNullOrWhiteSpace(buttonUse)) buttonUse = "Use";
            if (string.IsNullOrWhiteSpace(buttonUseMultiple)) buttonUseMultiple = "Use All";

            // Labels
            if (string.IsNullOrWhiteSpace(labelQuantity)) labelQuantity = "Quantity:";
            if (string.IsNullOrWhiteSpace(labelPotionType)) labelPotionType = "Potion Type";

            // Potion type names
            if (string.IsNullOrWhiteSpace(nameHealthPotion)) nameHealthPotion = "Health Potion";
            if (string.IsNullOrWhiteSpace(nameManaPotion)) nameManaPotion = "Mana Potion";
            if (string.IsNullOrWhiteSpace(nameBuffPotion)) nameBuffPotion = "Buff Potion";
            if (string.IsNullOrWhiteSpace(nameElixir)) nameElixir = "Elixir";
            if (string.IsNullOrWhiteSpace(nameAntidote)) nameAntidote = "Antidote";
            if (string.IsNullOrWhiteSpace(nameRejuvenation)) nameRejuvenation = "Rejuvenation";
            if (string.IsNullOrWhiteSpace(nameUtility)) nameUtility = "Utility";
            if (string.IsNullOrWhiteSpace(nameUnknown)) nameUnknown = "Unknown";

            // Numeric validations
            if (quickAdjustSmall < 1) quickAdjustSmall = 1;
            if (quickAdjustLarge < 1) quickAdjustLarge = 5;
            if (errorDisplayDuration < 1f) errorDisplayDuration = 1f;
        }

        #endregion
    }
}