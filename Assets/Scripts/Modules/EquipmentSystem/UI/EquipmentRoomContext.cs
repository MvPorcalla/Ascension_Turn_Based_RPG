// ════════════════════════════════════════════
// Assets\Scripts\Modules\EquipmentSystem\UI\EquipmentRoomContext.cs
// Equipment Room behavior for GearPopup
// Located in: Scripts/Modules/EquipmentSystem/UI/Contexts/
// ════════════════════════════════════════════

using UnityEngine;
using Ascension.Data.SO.Item;
using Ascension.Equipment.Manager;
using Ascension.Equipment.Services;
using Ascension.Inventory.Data;
using Ascension.SharedUI.Popups;

namespace Ascension.Equipment.UI
{
    public class EquipmentRoomContext : IGearPopupContext
    {
        private GearSlotService _slotService;

        public EquipmentRoomContext()
        {
            _slotService = new GearSlotService();
        }

        public string GetButtonText(ItemBaseSO item, ItemInstance itemInstance)
        {
            bool isEquipped = Equipment.Manager.EquipmentManager.Instance?.IsItemEquipped(item.ItemID) ?? false;
            return isEquipped ? "UNEQUIP" : "EQUIP";
        }

        public bool OnButtonClicked(ItemBaseSO item, ItemInstance itemInstance)
        {
            if (item == null || EquipmentManager.Instance == null)
            {
                Debug.LogError("[EquipmentRoomContext] Missing references");
                return false;
            }

            bool isEquipped = Equipment.Manager.EquipmentManager.Instance?.IsItemEquipped(item.ItemID) ?? false;

            if (isEquipped)
            {
                return HandleUnequip(item);
            }
            else
            {
                return HandleEquip(item);
            }
        }

        public bool CanPerformAction(ItemBaseSO item, ItemInstance itemInstance)
        {
            return EquipmentManager.Instance != null;
        }

        private bool HandleEquip(ItemBaseSO item)
        {
            var slotType = _slotService.GetSlotForItem(item);

            if (!slotType.HasValue)
            {
                Debug.LogError($"[EquipmentRoomContext] Cannot determine slot for {item.ItemName}");
                return false;
            }

            bool success = EquipmentManager.Instance.EquipItem(item.ItemID, slotType.Value);

            if (success)
            {
                Debug.Log($"[EquipmentRoomContext] Equipped {item.ItemName}");
            }
            else
            {
                Debug.LogError($"[EquipmentRoomContext] Failed to equip {item.ItemName}");
            }

            return success;
        }

        private bool HandleUnequip(ItemBaseSO item)
        {
            var slotType = FindEquippedSlot(item.ItemID);

            if (!slotType.HasValue)
            {
                Debug.LogError($"[EquipmentRoomContext] Cannot find equipped slot for {item.ItemName}");
                return false;
            }

            bool success = EquipmentManager.Instance.UnequipSlot(slotType.Value);

            if (success)
            {
                Debug.Log($"[EquipmentRoomContext] Unequipped {item.ItemName}");
            }
            else
            {
                Debug.LogError($"[EquipmentRoomContext] Failed to unequip {item.ItemName}");
            }

            return success;
        }

        private Ascension.Equipment.Enums.GearSlotType? FindEquippedSlot(string itemId)
        {
            var equippedGear = EquipmentManager.Instance.EquippedGear;

            if (equippedGear.weaponId == itemId)
                return Ascension.Equipment.Enums.GearSlotType.Weapon;
            if (equippedGear.helmetId == itemId)
                return Ascension.Equipment.Enums.GearSlotType.Helmet;
            if (equippedGear.chestId == itemId)
                return Ascension.Equipment.Enums.GearSlotType.Chest;
            if (equippedGear.glovesId == itemId)
                return Ascension.Equipment.Enums.GearSlotType.Gloves;
            if (equippedGear.bootsId == itemId)
                return Ascension.Equipment.Enums.GearSlotType.Boots;
            if (equippedGear.accessory1Id == itemId)
                return Ascension.Equipment.Enums.GearSlotType.Accessory1;
            if (equippedGear.accessory2Id == itemId)
                return Ascension.Equipment.Enums.GearSlotType.Accessory2;

            return null;
        }
    }
}