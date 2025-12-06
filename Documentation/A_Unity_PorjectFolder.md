# Unity Project Folder Structure - Ascension (FINAL - INDUSTRY STANDARD)

```
Assets/
└── Scripts/
    │
    ├── Core/
    │   ├── Bootstrap.cs                        [Ascension.Core]
    │   └── GameSystemHub.cs                    [Ascension.Core]
    │
    ├── Manager/
    │   ├── CharacterManager.cs                 [Ascension.Manager]
    │   ├── GameManager.cs                      [Ascension.Manager]
    │   ├── SaveManager.cs                      [Ascension.Manager]
    │   └── InventoryManager.cs                 [Ascension.Manager]
    │
    ├── Character/
    │   ├── Stat/
    │   │   ├── CharacterStats.cs               [Ascension.Character.Stat]
    │   │   ├── CharacterAttributes.cs          [Ascension.Character.Stat]
    │   │   ├── CharacterItemStats.cs           [Ascension.Character.Stat]
    │   │   └── CharacterDerivedStats.cs        [Ascension.Character.Stat]
    │   │
    │   ├── Runtime/
    │   │   ├── CharacterCombatRuntime.cs       [Ascension.Character.Runtime]
    │   │   └── CharacterLevelSystem.cs         [Ascension.Character.Runtime]
    │   │
    │   └── Model/
    │       └── CharacterData.cs                [Ascension.Character.Model]
    │
    ├── Inventory/
    │   ├── Data/
    │   │   ├── BagInventory.cs                 [Ascension.Inventory.Data]
    │   │   ├── ItemInstance.cs                 [Ascension.Inventory.Data]
    │   │   └── BagInventoryData.cs             [Ascension.Inventory.Data]
    │   │
    │   └── Enum/
    │       └── InventoryEnums.cs               [Ascension.Inventory.Enum]
    │
    ├── GameSystem/
    │   └── PotionManager.cs                    [Ascension.GameSystem]
    │
    ├── UI/
    │   ├── Core/
    │   │   └── UIManager.cs                    [Ascension.UI.Core]
    │   │
    │   ├── HUD/
    │   │   ├── PlayerHUD.cs                    [Ascension.UI.HUD]
    │   │   ├── PlayerPreviewUI.cs              [Ascension.UI.HUD]
    │   │   └── ProfilePanelManager.cs          [Ascension.UI.HUD]
    │   │
    │   ├── Panel/
    │   │   ├── LevelUpManager.cs               [Ascension.UI.Panel]
    │   │   └── DisclaimerController.cs         [Ascension.UI.Panel]
    │   │
    │   ├── Creation/
    │   │   └── AvatarCreationManager.cs        [Ascension.UI.Creation]
    │   │
    │   └── Inventory/
    │       ├── StorageRoomUI.cs                [Ascension.UI.Inventory]
    │       ├── ItemSlotUI.cs                   [Ascension.UI.Inventory]
    │       ├── BuffLineUI.cs                   [Ascension.UI.Inventory]
    │       │
    │       └── Popup/
    │           ├── InventoryPotionPopup.cs     [Ascension.UI.Inventory.Popup]
    │           ├── InventoryItemPopup.cs       [Ascension.UI.Inventory.Popup]
    │           └── InventoryGearPopup.cs       [Ascension.UI.Inventory.Popup]
    │
    └── Data/
        ├── Model/
        │   └── SaveData.cs                     [Ascension.Data.Model]
        │
        ├── Enum/
        │   └── WeaponEnums.cs                  [Ascension.Data.Enum]
        │
        └── ScriptableObject/
            ├── Item/
            │   ├── ItemBaseSO.cs               [Ascension.Data.SO.Item]
            │   ├── WeaponSO.cs                 [Ascension.Data.SO.Item]
            │   ├── WeaponRaritySO.cs           [Ascension.Data.SO.Item]
            │   ├── GearSO.cs                   [Ascension.Data.SO.Item]
            │   ├── PotionSO.cs                 [Ascension.Data.SO.Item]
            │   ├── MaterialSO.cs               [Ascension.Data.SO.Item]
            │   └── AbilitySO.cs                [Ascension.Data.SO.Item]
            │
            ├── Character/
            │   └── CharacterBaseStatsSO.cs     [Ascension.Data.SO.Character]
            │
            └── Database/
                └── GameDatabaseSO.cs           [Ascension.Data.SO.Database]
```

---

## 🔥 KEY IMPROVEMENTS FROM CHATGPT'S FEEDBACK

### 1. ✅ **Singular Folder Names**
**Why:** Industry standard convention for better scanning and consistency.

❌ **Before:**
```
Managers/
Characters/
Systems/
```

✅ **After:**
```
Manager/
Character/
System/
```

**Benefit:** Cleaner hierarchy, matches Unity conventions (Animation, Mesh, Material — all singular)

---

### 3. ✅ **Models vs Stats Separation**
**Why:** Separate serialization concerns from runtime logic.

```
Character/
  ├── Stat/         → Runtime stat calculations
  ├── Runtime/      → Combat/buff state
  └── Model/        → Serialization DTOs (save data)
```

**Pattern:**
- `CharacterStats` → runtime gameplay logic
- `CharacterData` → serializable save/load model

---

### 4. ✅ **Enum Consistency**
**Fixed:** All enum folders now singular: `Enum/` not `Enums/`

```
Data/Enum/              → Ascension.Data.Enum
Inventory/Enum/         → Ascension.Inventory.Enum
```

---

### 5. ✅ **Naming Consistency: Runtime over Systems**
**Decision:** Use `Runtime` for stateful gameplay logic, reserve `System` for stateless utilities.

```
Character/Runtime/      → Character combat/buff state
System/                 → Standalone systems (Potion, Audio, etc.)
```

---

## 📦 Assembly Definition Files (.asmdef) - RECOMMENDED

**Why:** Faster compile times + enforced dependencies.

Create these files:

```
Scripts/
├── Ascension.Core.asmdef
├── Ascension.Manager.asmdef
├── Ascension.Character.asmdef
├── Ascension.Inventory.asmdef
├── Ascension.System.asmdef
├── Ascension.UI.asmdef
└── Ascension.Data.asmdef
```

**Dependencies:**
```
Ascension.UI → depends on → Ascension.Manager, Ascension.Character
Ascension.Manager → depends on → Ascension.Data, Ascension.Character
Ascension.Character → depends on → Ascension.Data
```

**Benefit:** 
- ✅ Changes to UI don't recompile Character code
- ✅ Prevents circular dependencies
- ✅ 50-80% faster iteration compile times

---

## 🎯 Complete Namespace Reference (FINAL)

| Folder                | Namespace                         | Type          |
|-----------------------|-----------------------------------|---------------|
| `Core/`               | `Ascension.Core`                  | Bootstrap     |
| `Manager/`            | `Ascension.Manager`               | Singletons    |
| `Character/Stat/`     | `Ascension.Character.Stat`        | Runtime stats |
| `Character/Runtime/`  | `Ascension.Character.Runtime`     | Combat/buffs  |
| `Character/Model/`    | `Ascension.Character.Model`       | Save data     |
| `Inventory/Data/`     | `Ascension.Inventory.Data`        | Bag logic     |
| `Inventory/Enum/`     | `Ascension.Inventory.Enum`        | Enums         |
| `GameSystem/`         | `Ascension.GameSystem`            | Utilities     |
| `UI/Core/`            | `Ascension.UI.Core`               | Main UI       |
| `UI/HUD/`             | `Ascension.UI.HUD`                | Overlays      |
| `UI/Panel/`           | `Ascension.UI.Panel`              | Modals        |
| `UI/Inventory/`       | `Ascension.UI.Inventory`          | Inv UI        |
| `UI/Inventory/Popup/` | `Ascension.UI.Inventory.Popup`    | Popups        |
| `Data/Model/`         | `Ascension.Data.Model`            | DTOs          |
| `Data/Enum/`          | `Ascension.Data.Enum`             | Enums         |
| `Data/SO/Item/`       | `Ascension.Data.SO.Item`          | Item SOs      |
| `Data/SO/Character/`  | `Ascension.Data.SO.Character`     | Char SOs      |
| `Data/SO/Database/`   | `Ascension.Data.SO.Database`      | DB SOs        |

---

## 📋 Migration Checklist (UPDATED)

### Phase 1: Folder Rename
```bash
# Singular names
Managers/ → Manager/
Characters/ → Character/
Systems/ → GameSystem/
Models/ → Model/
Enums/ → Enum/
ScriptableObjects/ → ScriptableObject/
Panels/ → Panel/
Popups/ → Popup/
```

### Phase 2: SO Organization
```bash
# Group ScriptableObjects by feature
ScriptableObject/
  ├── Item/       (move all item SOs here)
  ├── Character/  (move CharacterBaseStatsSO)
  └── Database/   (move GameDatabaseSO)
```

### Phase 3: Namespace Updates
- [ ] Update all folder-based namespaces to singular
- [ ] Update SO namespaces with feature grouping
- [ ] Update all `using` statements

### Phase 4: Assembly Definitions (Optional but Recommended)
- [ ] Create .asmdef files per major folder
- [ ] Set up dependency chain
- [ ] Test compilation

---

Place one `.asmdef` file in each major folder. Unity will handle the rest.

**Result:** 50-80% faster compile times when changing UI without touching Character code.