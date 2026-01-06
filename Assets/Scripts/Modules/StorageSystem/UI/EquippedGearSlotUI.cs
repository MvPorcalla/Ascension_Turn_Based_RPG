// ══════════════════════════════════════════════════════════════════
// Assets\Scripts\Modules\StorageSystem\UI\EquippedGearSlotUI.cs
// Individual equipped gear slot – display only, no interaction
// ══════════════════════════════════════════════════════════════════

using UnityEngine;
using UnityEngine.UI;
using Ascension.Equipment.Enums;
using Ascension.Data.SO.Database;

namespace Ascension.Storage.UI
{
    /// <summary>
    /// Individual equipped gear slot – display only, no interaction
    /// </summary>
    public class EquippedGearSlotUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Image iconImage;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private GameObject emptyOverlay;

        [Header("Optional Label")]
        [SerializeField] private TMPro.TextMeshProUGUI labelText;

        [Header("Visual Feedback")]
        [SerializeField] private Color emptySlotColor = new Color(0.2f, 0.2f, 0.2f, 0.5f);
        [SerializeField] private Color filledSlotColor = Color.white;

        private GearSlotType slotType;

        public void Initialize(GearSlotType type)
        {
            slotType = type;

            if (labelText != null)
                labelText.text = type.ToDisplayName();

            SetItem(null, null);
        }

        public void SetItem(string itemId, GameDatabaseSO database)
        {
            bool hasItem = !string.IsNullOrEmpty(itemId);

            if (emptyOverlay != null)
                emptyOverlay.SetActive(!hasItem);

            if (backgroundImage != null)
                backgroundImage.color = hasItem ? filledSlotColor : emptySlotColor;

            if (!hasItem || database == null)
            {
                ClearIcon();
                return;
            }

            var itemData = database.GetItem(itemId);
            if (itemData != null && iconImage != null)
            {
                iconImage.sprite = itemData.Icon;
                iconImage.enabled = true;
                iconImage.color = Color.white;
            }
            else
            {
                ClearIcon();
            }
        }

        private void ClearIcon()
        {
            if (iconImage != null)
                iconImage.enabled = false;
        }
    }
}
