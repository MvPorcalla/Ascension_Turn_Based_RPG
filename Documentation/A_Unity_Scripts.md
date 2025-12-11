# ASCENSION REFACTOR AGENT INSTRUCTIONS (FEATURE-GROUPED) - FINAL

You are refactoring a Unity C# project using FEATURE-BASED organization with layer separation.

---

## 🎯 Complete Namespace Reference (FEATURE-GROUPED FINAL)

| Folder Path                           | Namespace                         | Type          |
|---------------------------------------|-----------------------------------|---------------|
| `Core/`                               | `Ascension.Core`                  | Bootstrap     |
| `Manager/`                            | `Ascension.Manager`               | Global Mgrs   |
| `CharacterSystem/Manager/`            | `Ascension.Character.Manager`     | Char Manager  |
| `CharacterSystem/Stat/`               | `Ascension.Character.Stat`        | Runtime stats |
| `CharacterSystem/Runtime/`            | `Ascension.Character.Runtime`     | Combat/buffs  |
| `CharacterSystem/Model/`              | `Ascension.Character.Model`       | Save data     |
| `CharacterSystem/UI/`                 | `Ascension.Character.UI`          | Char UI       |
| `InventorySystem/Manager/`            | `Ascension.Inventory.Manager`     | Inv Manager   |
| `InventorySystem/Data/`               | `Ascension.Inventory.Data`        | Bag logic     |
| `InventorySystem/Enum/`               | `Ascension.Inventory.Enum`        | Enums         |
| `InventorySystem/UI/`                 | `Ascension.Inventory.UI`          | Inv UI        |
| `InventorySystem/UI/Popup/`           | `Ascension.Inventory.UI.Popup`    | Inv Popups    |
| `GameSystem/`                         | `Ascension.GameSystem`            | Game Systems  |
| `UI/Core/`                            | `Ascension.UI.Core`               | Global UI     |
| `UI/Panel/`                           | `Ascension.UI.Panel`              | Panels        |
| `Data/Model/`                         | `Ascension.Data.Model`            | DTOs          |
| `Data/Enum/`                          | `Ascension.Data.Enum`             | Enums         |
| `Data/ScriptableObject/Item/`         | `Ascension.Data.SO.Item`          | Item SOs      |
| `Data/ScriptableObject/Character/`    | `Ascension.Data.SO.Character`     | Char SOs      |
| `Data/ScriptableObject/Database/`     | `Ascension.Data.SO.Database`      | DB SOs        |

---

## RULE 1: FOLDER STRUCTURE

Create this exact folder structure (NO additional subfolders):

```
Scripts/
├── AppFlow/                                // High-level orchestration
│   ├── Ascension.Appflow.asmdef
│   ├── GameManager.cs                      // References Manager + Character + Inventory
│   └── SaveManager.cs                      // References Manager + Character + Inventory
│
├── Core/                                   // Core engine / bootstrap
│   ├── Ascension.Core.asmdef
│   ├── Bootstrap.cs
│   └── GameSystemHub.cs
│
├── Manager/                                // Pure manager logic, no cross-module calls
│   ├── Ascension.Manager.asmdef
│   └── Model/
│       └── SaveData.cs                     // Only data structures, no references to Character/Inventory
│
├── CharacterSystem/                        // Character module
│   ├── Ascension.Character.asmdef          // not included for now
│   ├── Manager/
│   │   └── CharacterManager.cs
│   ├── Stats/
│   │   ├── CharacterStats.cs
│   │   ├── CharacterAttributes.cs
│   │   ├── CharacterItemStats.cs
│   │   └── CharacterDerivedStats.cs
│   ├── Runtime/
│   │   ├── CharacterCombatRuntime.cs
│   │   └── CharacterLevelSystem.cs
│   ├── Model/
│   │   └── CharacterData.cs
│   └── UI/
│       ├── PlayerHUD.cs
│       ├── PlayerPreviewUI.cs
│       ├── ProfilePanelManager.cs
│       ├── LevelUpManager.cs
│       └── CharacterCreationManager.cs
│
├── InventorySystem/                        // Inventory module
│   ├── Ascension.Inventory.asmdef          // not included for now
│   ├── Manager/
│   │   └── InventoryManager.cs
│   ├── Data/
│   │   ├── BagInventory.cs
│   │   ├── ItemInstance.cs
│   │   └── BagInventoryData.cs
│   ├── Enum/
│   │   └── InventoryEnums.cs
│   ├── Popup/
│   │   ├── InventoryPotionPopup.cs
│   │   ├── InventoryItemPopup.cs
│   │   └── InventoryGearPopup.cs
│   └── UI/
│       ├── StorageRoomUI.cs
│       ├── ItemSlotUI.cs
│       └── BuffLineUI.cs

│
├── GameSystem/                             // Game-wide systems, optional for cross-module logic
│   ├── Ascension.GameSystem.asmdef         // not included for now
│   └── PotionManager.cs
├── UI/                                     // UI module
│   ├── Ascension.UI.asmdef
│   ├── Core/
│   │   └── UIManager.cs
│   └── Panel/
│       └── DisclaimerController.cs
│
├── Data/                                   // Pure data / scriptable objects
    ├── Ascension.Data.asmdef               // not included for now
    ├── Enums/
    │   └── WeaponEnums.cs
    └── ScriptableObject/
        ├── Item/
        │   ├── ItemBaseSO.cs
        │   ├── WeaponSO.cs
        │   ├── WeaponRaritySO.cs
        │   ├── GearSO.cs
        │   ├── PotionSO.cs
        │   ├── MaterialSO.cs
        │   └── AbilitySO.cs
        ├── Character/
        │   └── CharacterBaseStatsSO.cs
        └── Database/
            └── GameDatabaseSO.cs

```

---

## RULE 2: FILE LOCATION MAP

Move files to these EXACT locations:

```
GameManager.cs                  → AppFlow/                                                      ✅
SaveManager.cs                  → AppFlow/                                                      ✅

Bootstrap.cs                    → Core/                                                         ✅
GameSystemHub.cs                → Core/                                                         ✅

SaveData.cs                     → Manager/Model/                                                ✅

CharacterManager.cs             → CharacterSystem/Manager/                                      ✅
PlayerStats.cs                  → CharacterSystem/Stat/ (rename: CharacterStats.cs)             ✅
PlayerAttributes.cs             → CharacterSystem/Stat/ (rename: CharacterAttributes.cs)        ✅
PlayerItemStats.cs              → CharacterSystem/Stat/ (rename: CharacterItemStats.cs)         ✅
PlayerDerivedStats.cs           → CharacterSystem/Stat/ (rename: CharacterDerivedStats.cs)      ✅
PlayerCombatRuntime.cs          → CharacterSystem/Runtime/ (rename: CharacterCombatRuntime.cs)  ✅
PlayerLevelSystem.cs            → CharacterSystem/Runtime/ (rename: CharacterLevelSystem.cs)    ✅
PlayerData.cs                   → CharacterSystem/Model/ (rename: CharacterData.cs)             ✅
PlayerHUD.cs                    → CharacterSystem/UI/                                           ✅
PlayerPreviewUI.cs              → CharacterSystem/UI/                                           ✅
ProfilePanelManager.cs          → CharacterSystem/UI/                                           ✅
LevelUpManager.cs               → CharacterSystem/UI/                                           ✅
CharacterCreationManager.cs        → CharacterSystem/UI/                                        ✅

InventoryManager.cs             → InventorySystem/Manager/                                      
BagInventory.cs                 → InventorySystem/Data/                                         
ItemInstance.cs                 → InventorySystem/Data/                                                                 
BagInventoryData.cs             → InventorySystem/Data/                                         
InventoryEnums.cs               → InventorySystem/Enums/                                         
StorageRoomUI.cs                → InventorySystem/UI/                                           
ItemSlotUI.cs                   → InventorySystem/UI/                                           
BuffLineUI.cs                   → InventorySystem/UI/                                           
InventoryPotionPopup.cs         → InventorySystem/UI/Popup/                                     
InventoryItemPopup.cs           → InventorySystem/UI/Popup/                                     
InventoryGearPopup.cs           → InventorySystem/UI/Popup/                                     

**EquipmentSystem** "on going work"
- 
- 
- 
- 
- 
- 

PotionManager.cs                → GameSystem/                                                   - rename to CombatSystem; rework not to be dependent to inventory system

UIManager.cs                    → UI/Core/                                                      
DisclaimerController.cs         → UI/Panel/                                                     

WeaponEnums.cs                  → Data/Enums/                                                   ✅
ItemBaseSO.cs                   → Data/ScriptableObject/Item/                                   ✅
WeaponSO.cs                     → Data/ScriptableObject/Item/                                   ✅
WeaponRaritySO.cs               → Data/ScriptableObject/Item/                                   ✅
GearSO.cs                       → Data/ScriptableObject/Item/                                   ✅
PotionSO.cs                     → Data/ScriptableObject/Item/                                   ✅
MaterialSO.cs                   → Data/ScriptableObject/Item/                                   ✅
AbilitySO.cs                    → Data/ScriptableObject/Item/                                   ✅
CharacterBaseStatsSO.cs         → Data/ScriptableObject/Character/                              ✅
GameDatabaseSO.cs               → Data/ScriptableObject/Database/                               ✅
```

---

## RULE 3: NAMESPACE MAP

Apply these namespaces to files based on their folder:

```
AppFlow/*                               → namespace Ascension.AppFlow
Core/*                                  → namespace Ascension.Core
Manager/*                               → namespace Ascension.Manager
Manager/Model/*                         → namespace Ascension.Manager.Model

CharacterSystem/Manager/*               → namespace Ascension.Character.Manager
CharacterSystem/Stat/*                  → namespace Ascension.Character.Stat
CharacterSystem/Runtime/*               → namespace Ascension.Character.Runtime
CharacterSystem/Model/*                 → namespace Ascension.Character.Model
CharacterSystem/UI/*                    → namespace Ascension.Character.UI

InventorySystem/Manager/*               → namespace Ascension.Inventory.Manager
InventorySystem/Data/*                  → namespace Ascension.Inventory.Data
InventorySystem/Enums/*                  → namespace Ascension.Inventory.Enums
InventorySystem/UI/*                    → namespace Ascension.Inventory.UI
InventorySystem/Popup/*              → namespace Ascension.Inventory.Popup

GameSystem/*                            → namespace Ascension.GameSystem

UI/Core/*                               → namespace Ascension.UI.Core
UI/Panel/*                              → namespace Ascension.UI.Panel

Data/Enums/*                             → namespace Ascension.Data.Enums
Data/ScriptableObject/Item/*            → namespace Ascension.Data.SO.Item
Data/ScriptableObject/Character/*       → namespace Ascension.Data.SO.Character
Data/ScriptableObject/Database/*        → namespace Ascension.Data.SO.Database
```

---

## RULE 4: CLASS RENAMES

Rename these classes inside the files:

```
PlayerStats           → CharacterStats
PlayerAttributes      → CharacterAttributes
PlayerItemStats       → CharacterItemStats
PlayerDerivedStats    → CharacterDerivedStats
PlayerCombatRuntime   → CharacterCombatRuntime
PlayerLevelSystem     → CharacterLevelSystem
PlayerData            → CharacterData
```

---

## RULE 5: NAMESPACE REPLACEMENT IN USING STATEMENTS

Replace old using statements with new ones throughout ALL files:

```
OLD                                 NEW
using Ascension.Managers;       → using Ascension.Manager;
                                → OR using Ascension.Character.Manager;
                                → OR using Ascension.Inventory.Manager;

using Ascension.Systems;        → using Ascension.GameSystem;

using Ascension.Data.Models;    → using Ascension.Data.Model;

using Ascension.UI;             → using Ascension.UI.Core;
                                → OR using Ascension.Character.UI;
                                → OR using Ascension.Inventory.UI;
```

---

## RULE 6: ASSEMBLY DEFINITIONS

Create these 7 .asmdef files with exact content:

### File: `Scripts/Data/Ascension.Data.asmdef`
```json
{
    "name": "Ascension.Data",
    "rootNamespace": "Ascension.Data",
    "references": [],
    "includePlatforms": [],
    "excludePlatforms": [],
    "allowUnsafeCode": false,
    "overrideReferences": false,
    "precompiledReferences": [],
    "autoReferenced": true,
    "defineConstraints": [],
    "versionDefines": [],
    "noEngineReferences": false
}
```

### File: `Scripts/CharacterSystem/Ascension.Character.asmdef`
```json
{
    "name": "Ascension.Character",
    "rootNamespace": "Ascension.Character",
    "references": ["Ascension.Data"],
    "includePlatforms": [],
    "excludePlatforms": [],
    "allowUnsafeCode": false,
    "overrideReferences": false,
    "precompiledReferences": [],
    "autoReferenced": true,
    "defineConstraints": [],
    "versionDefines": [],
    "noEngineReferences": false
}

```

### File: `Scripts/InventorySystem/Ascension.Inventory.asmdef`
```json
{
    "name": "Ascension.Inventory",
    "rootNamespace": "Ascension.Inventory",
    "references": ["Ascension.Data", "Ascension.Character"],
    "includePlatforms": [],
    "excludePlatforms": [],
    "allowUnsafeCode": false,
    "overrideReferences": false,
    "precompiledReferences": [],
    "autoReferenced": true,
    "defineConstraints": [],
    "versionDefines": [],
    "noEngineReferences": false
}
```

### File: `Scripts/GameSystem/Ascension.GameSystem.asmdef`
```json
{
    "name": "Ascension.GameSystem",
    "rootNamespace": "Ascension.GameSystem",
    "references": ["Ascension.Data", "Ascension.Character"],
    "includePlatforms": [],
    "excludePlatforms": [],
    "allowUnsafeCode": false,
    "overrideReferences": false,
    "precompiledReferences": [],
    "autoReferenced": true,
    "defineConstraints": [],
    "versionDefines": [],
    "noEngineReferences": false
}
```

### File: `Scripts/Manager/Ascension.Manager.asmdef`
```json
{
    "name": "Ascension.Manager",
    "rootNamespace": "Ascension.Manager",
    "references": [
        "Ascension.Data",
        "Ascension.Character",
        "Ascension.Inventory",
        "Ascension.GameSystem"
    ],
    "includePlatforms": [],
    "excludePlatforms": [],
    "allowUnsafeCode": false,
    "overrideReferences": false,
    "precompiledReferences": [],
    "autoReferenced": true,
    "defineConstraints": [],
    "versionDefines": [],
    "noEngineReferences": false
}
```

### File: `Scripts/Core/Ascension.Core.asmdef`
```json
{
    "name": "Ascension.Core",
    "rootNamespace": "Ascension.Core",
    "references": [
        "Ascension.Manager",
        "Ascension.Character",
        "Ascension.Inventory",
        "Ascension.GameSystem"
    ],
    "includePlatforms": [],
    "excludePlatforms": [],
    "allowUnsafeCode": false,
    "overrideReferences": false,
    "precompiledReferences": [],
    "autoReferenced": true,
    "defineConstraints": [],
    "versionDefines": [],
    "noEngineReferences": false
}
```

### File: `Scripts/UI/Ascension.UI.asmdef`
```json
{
    "name": "Ascension.UI",
    "rootNamespace": "Ascension.UI",
    "references": [
        "Ascension.Manager",
        "Ascension.Character",
        "Ascension.Inventory",
        "Ascension.Data"
    ],
    "includePlatforms": [],
    "excludePlatforms": [],
    "allowUnsafeCode": false,
    "overrideReferences": false,
    "precompiledReferences": [],
    "autoReferenced": true,
    "defineConstraints": [],
    "versionDefines": [],
    "noEngineReferences": false
}
```

---

## EXECUTION ORDER

1. ✅ Create folder structure (RULE 1)
2. ✅ Move and rename files (RULE 2)
3. ✅ Update class names inside files (RULE 4)
4. ✅ Add namespace declarations to all files (RULE 3)
5. ✅ Fix using statements (RULE 5)
6. ✅ Create .asmdef files (RULE 6)
7. ✅ Compile and fix errors

---

## EXAMPLE TRANSFORMATIONS

### Example 1: CharacterManager.cs

**Before:**
```csharp
using Ascension.Data.Models;
using Ascension.Data.SO;

namespace Ascension.Managers
{
    public class CharacterManager
    {
        // code
    }
}
```

**After:**
```csharp
using Ascension.Data.Model;
using Ascension.Data.SO;

namespace Ascension.Character.Manager
{
    public class CharacterManager
    {
        // code
    }
}
```

### Example 2: StorageRoomUI.cs

**Before:**
```csharp
using UnityEngine;
using Ascension.Managers;

public class StorageRoomUI : MonoBehaviour
{
    // code
}
```

**After:**
```csharp
using UnityEngine;
using Ascension.Inventory.Manager;

namespace Ascension.Inventory.UI
{
    public class StorageRoomUI : MonoBehaviour
    {
        // code
    }
}
```

### Example 3: WeaponEnums.cs

**Before:**
```csharp
public enum WeaponType
{
    // values
}
```

**After:**
```csharp
namespace Ascension.Data.Enum
{
    public enum WeaponType
    {
        // values
    }
}
```

### Example 4: AbilitySO.cs

**Before:**
```csharp
using Ascension.Systems;

namespace Ascension.Data.SO
{
    public class AbilitySO : ScriptableObject
    {
        // code
    }
}
```

**After:**
```csharp
using Ascension.GameSystem;

namespace Ascension.Data.SO.Item
{
    public class AbilitySO : ScriptableObject
    {
        // code
    }
}
```

---

## VALIDATION CHECKLIST

After refactor, verify ALL of these:
- [ ] All files moved to correct folders per RULE 2
- [ ] All namespaces applied per RULE 3
- [ ] All Player* classes renamed to Character* per RULE 4
- [ ] All using statements updated per RULE 5
- [ ] All 7 .asmdef files created per RULE 6
- [ ] Project compiles without errors
- [ ] Character system files in CharacterSystem/ folder
- [ ] Inventory system files in InventorySystem/ folder
- [ ] No files with missing namespaces
- [ ] No references to old namespaces (Managers, Systems, Models, UI)

---

## CRITICAL RULES

1. NEVER modify existing code logic, only structure and namespaces
2. ALWAYS wrap classes in namespace blocks (even enums)
3. ALWAYS rename Player* classes to Character*
4. NEVER create circular dependencies in .asmdef
5. ALWAYS test compilation after each phase
6. Group related features (Character, Inventory) in their own system folders
7. Keep shared infrastructure (Core, Manager, Data, UI) at root level
8. NO additional subfolders beyond what's in RULE 1 (no HUD/, no Panel/ inside systems)