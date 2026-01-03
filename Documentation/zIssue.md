WeaponSO.cs

Observations / Possible Improvements

ID generation

itemID = $"weapon_{name.ToLower().Replace(" ", "_")}" in GenerateItemID() could collide if names are duplicated. Consider appending a GUID or unique suffix.

Random rolls

UnityEngine.Random.Range is fine, but consider injecting a random seed if you want reproducible rolls for testing.

Bonus calculation

Currently, CalculateBonusStat() multiplies baseValue by rarity and then adds rolled bonuses. Make sure this aligns with your intended formula. Some games multiply both base + bonus by rarity instead.

Stat formatting

FormatStatText uses the same color for everything (#ffffff). You could optionally color-code stats for readability (red for AD, blue for AP, green for HP, etc.).

SO references

rarityConfig and defaultWeaponSkill are SO references. Ensure these are set in the editor or validated at runtime; missing references will cause silent bugs.

Thread safety / runtime modifications

Rolling bonus stats modifies the SO’s bonusStats list directly. If multiple game sessions share the same SO instance, this could create side effects. Usually, you clone the SO or apply rolls in a runtime data container instead of modifying the asset itself.

---

# 🎯 Task: Add Equipped Item Indicators to Inventory UI

## Context
I have a mobile-first, portrait-mode 2D turn-based RPG in Unity with separate InventorySystem and EquipmentSystem. Items are stored in inventory, and equipped state is tracked separately in EquipmentManager.

## Current Architecture
- **InventorySystem**: Tracks item locations (Bag/Pocket/Storage) and quantities
- **EquipmentSystem**: Tracks which items are equipped via EquipmentSaveData (weaponId, helmetId, etc.)
- **ItemInstance**: Does NOT have `isEquipped` flag (by design - single source of truth)

## Goal
Add visual indicators to show which items are currently equipped when viewing the inventory UI.

## Requirements

### 1. Visual Design (Choose One or Combine)
- **Option A**: Small [E] badge in top-left corner of item slot
- **Option B**: Golden/colored border around equipped items
- **Option C**: Both badge + border for extra visibility

================================================================================================================

# Equipment room VIsual UI

┌──────────────────────────────────────────────┐
│ [Back]          Equipment Room               │
└──────────────────────────────────────────────┘

┌──────────────────────────┬───────────────────┐
│ Player Stats (Core)      │ Gear Section      │
│ ATK  DEF  HP  CRIT       │ [Helm] [Chest]    │
│ [+ Advanced]             │ [Glov] [Boot ]    │
│                          │ [Acc ] [Acc ]     │
└──────────────────────────┴───────────────────┘

┌──────────────────────────────────────────────┐
│ Loadout (ACTIVE)                             │
│ Weapon | Skill 1 | Skill 2 | Ultimate        │
└──────────────────────────────────────────────┘

┌──────────────────────────────────────────────┐
│ Storage (Filtered Pool)                      │
│ [Weapons] [Gear] [Skills] [Potions?] [Sort]  │
│ ┌──┐ ┌──┐ ┌──┐ ┌──┐                          │
│ └──┘ └──┘ └──┘ └──┘   Scrollable Grid        │
└──────────────────────────────────────────────┘


================================================================================================================

TODO: Fix the Accessory Equipment it bugging it should be able to equip 2 but it only equip 1

================================================================================================================


ask me question first or script you want to see for full context before proceeding to code

### prompt (only separate abilities)

> Refactor my save logic so that abilities are stored in their own `abilitiesData` section instead of being mixed into inventory items.
> Keep `inventoryData.items` as a single flat array (do NOT separate by category).

### Expected Ouput

```json
{
  "metaData": {
    "saveVersion": "1.1",
    "createdTime": "2025-12-15 00:26:50",
    "lastSaveTime": "2025-12-23 14:10:00",
    "totalPlayTimeSeconds": 3720.25,
    "saveCount": 86
  },

  "characterData": {
    "playerName": "Medarru",
    "level": 2,
    "currentExperience": 120,
    "currentHealth": 220.0,
    "currentMana": 50.0,
    "attributePoints": 5,
    "strength": 30,
    "agility": 6,
    "intelligence": 2,
    "endurance": 11,
    "wisdom": 10
  },

  "inventoryData": {
    "items": [
      { "itemId": "weapon_iron_sword", "quantity": 1, "location": 0 },
      { "itemId": "gear_leather_helmet", "quantity": 1, "location": 0 },
      { "itemId": "gear_leather_chestplate", "quantity": 1, "location": 0 },
      { "itemId": "gear_leather_gloves", "quantity": 1, "location": 0 },
      { "itemId": "gear_leather_boots", "quantity": 1, "location": 0 },

      { "itemId": "gear_accessory_iron_ring", "quantity": 1, "location": 0 },
      { "itemId": "gear_accessory_iron_bracelet", "quantity": 1, "location": 0 },

      { "itemId": "potion_minor_health_potion", "quantity": 5, "location": 0 },
      { "itemId": "potion_minor_health_potion", "quantity": 7, "location": 1 },
      { "itemId": "potion_minor_health_potion", "quantity": 6, "location": 2 },

      { "itemId": "material_iron_ore", "quantity": 12, "location": 0 },
      { "itemId": "material_wood", "quantity": 25, "location": 0 }
    ],
    "maxBagSlots": 12,
    "maxPocketSlots": 6,
    "maxStorageSlots": 60
  },

  "abilitiesData": {
    "unlocked": [
      "ability_fireball",
      "ability_heal"
    ],
    "equipped": [
      "ability_fireball"
    ]
  },

  "equipmentData": {
    "weaponId": "weapon_iron_sword",
    "helmetId": "gear_leather_helmet",
    "chestId": "gear_leather_chestplate",
    "glovesId": "gear_leather_gloves",
    "bootsId": "gear_leather_boots",
    "accessory1Id": "gear_accessory_iron_ring",
    "accessory2Id": "gear_accessory_iron_bracelet"
  },

  "skillLoadoutData": {
    "normalSkill1Id": "ability_fireball",
    "normalSkill2Id": "",
    "ultimateSkillId": ""
  }
}
```

---

============================================================

4. ItemQueryService: Performance Optimization ⚠️
Current Code:
csharppublic List<ItemInstance> GetBagItems(List<ItemInstance> allItems)
{
    return allItems.Where(item =>
        item.location == ItemLocation.Bag &&
        !IsSkill(item.itemID)
    ).ToList();
}
Problem: Every call does a full O(n) scan with string prefix check.
Optimization (if needed):
csharp// Option 1: Cache results (invalidate on change)
private Dictionary<ItemLocation, List<ItemInstance>> _cachedByLocation;

public List<ItemInstance> GetBagItems(List<ItemInstance> allItems)
{
    if (_cachedByLocation == null || _isDirty)
    {
        RebuildCache(allItems);
    }
    return _cachedByLocation[ItemLocation.Bag];
}

// Option 2: Pre-filter skills once
private HashSet<string> _skillItemIDs = new HashSet<string>();

private bool IsSkill(string itemID)
{
    return _skillItemIDs.Contains(itemID);  // O(1) instead of O(k) where k = prefix length
}
But honestly: Your current approach is fine for <1000 items. Only optimize if profiler shows issues.

-------------------------------------------------

5. Missing Transaction Support ⚠️
Scenario: What if you want to:

Remove 5 iron ore
Remove 2 wood
Add 1 sword

But step 2 fails because you don't have enough wood?
Current behavior: Step 1 already removed the iron! Now your inventory is inconsistent.
Solution: Transaction pattern:
csharppublic class InventoryTransaction
{
    private List<Action> _operations = new List<Action>();
    private List<Action> _rollbacks = new List<Action>();
    
    public InventoryTransaction RemoveItem(string itemID, int quantity)
    {
        _operations.Add(() => {
            var result = inventory.RemoveItem(itemID, quantity);
            if (!result.Success) throw new Exception(result.Message);
            
            // Store rollback action
            _rollbacks.Add(() => inventory.AddItem(itemID, quantity));
        });
        return this;
    }
    
    public InventoryResult Commit()
    {
        try
        {
            foreach (var op in _operations)
                op();
            
            return InventoryResult.Ok();
        }
        catch (Exception ex)
        {
            // Rollback all operations
            for (int i = _rollbacks.Count - 1; i >= 0; i--)
                _rollbacks[i]();
            
            return InventoryResult.Fail(ex.Message);
        }
    }
}

// Usage:
var transaction = new InventoryTransaction()
    .RemoveItem("iron_ore", 5)
    .RemoveItem("wood", 2)
    .AddItem("sword_iron", 1);

var result = transaction.Commit();
This is advanced - only add if you have complex crafting/trading systems.




==========================

Add this to your InventoryManager:
csharp[ContextMenu("Run All Tests")]
public void RunAllTests()
{
    TestAddRemove();
    TestStacking();
    TestMoveBetweenLocations();
    TestCapacityUpgrades();
    Debug.Log("✅ All inventory tests passed!");
}