// ══════════════════════════════════════════════════════════════════
// Assets\Scripts\Modules\StorageSystem\UI\EquippedGearPreviewUI.cs
// Simple read-only display of equipped gear in storage room
// ══════════════════════════════════════════════════════════════════

using UnityEngine;
using Ascension.Equipment.Manager;
using Ascension.Equipment.Enums;
using Ascension.Inventory.Manager;

namespace Ascension.Storage.UI
{
    /// <summary>
    /// Simple read-only display of equipped gear in storage room.
    /// </summary>
    public class EquippedGearPreviewUI : MonoBehaviour
    {
        [Header("Slot References (7 slots total)")]
        [SerializeField] private EquippedGearSlotUI weaponSlot;
        [SerializeField] private EquippedGearSlotUI helmetSlot;
        [SerializeField] private EquippedGearSlotUI chestSlot;
        [SerializeField] private EquippedGearSlotUI glovesSlot;
        [SerializeField] private EquippedGearSlotUI bootsSlot;
        [SerializeField] private EquippedGearSlotUI accessory1Slot;
        [SerializeField] private EquippedGearSlotUI accessory2Slot;

        [Header("Optional: Header")]
        [SerializeField] private TMPro.TextMeshProUGUI headerText;

        private EquipmentManager equipmentManager;
        private InventoryManager inventoryManager;

        private void Start()
        {
            equipmentManager = EquipmentManager.Instance;
            inventoryManager = InventoryManager.Instance;

            if (equipmentManager == null || inventoryManager == null)
            {
                Debug.LogError("[EquippedGearPreviewUI] Missing manager reference");
                return;
            }

            weaponSlot?.Initialize(GearSlotType.Weapon);
            helmetSlot?.Initialize(GearSlotType.Helmet);
            chestSlot?.Initialize(GearSlotType.Chest);
            glovesSlot?.Initialize(GearSlotType.Gloves);
            bootsSlot?.Initialize(GearSlotType.Boots);
            accessory1Slot?.Initialize(GearSlotType.Accessory1);
            accessory2Slot?.Initialize(GearSlotType.Accessory2);

            if (headerText != null)
                headerText.text = "Equipped Gear";

            equipmentManager.OnEquipmentChanged += RefreshDisplay;
            RefreshDisplay();
        }

        private void OnDestroy()
        {
            if (equipmentManager != null)
                equipmentManager.OnEquipmentChanged -= RefreshDisplay;
        }

        private void RefreshDisplay()
        {
            weaponSlot?.SetItem(equipmentManager.GetEquippedItemId(GearSlotType.Weapon), inventoryManager.Database);
            helmetSlot?.SetItem(equipmentManager.GetEquippedItemId(GearSlotType.Helmet), inventoryManager.Database);
            chestSlot?.SetItem(equipmentManager.GetEquippedItemId(GearSlotType.Chest), inventoryManager.Database);
            glovesSlot?.SetItem(equipmentManager.GetEquippedItemId(GearSlotType.Gloves), inventoryManager.Database);
            bootsSlot?.SetItem(equipmentManager.GetEquippedItemId(GearSlotType.Boots), inventoryManager.Database);
            accessory1Slot?.SetItem(equipmentManager.GetEquippedItemId(GearSlotType.Accessory1), inventoryManager.Database);
            accessory2Slot?.SetItem(equipmentManager.GetEquippedItemId(GearSlotType.Accessory2), inventoryManager.Database);
        }
    }
}
