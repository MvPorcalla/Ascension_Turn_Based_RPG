# GameBootstrap Initialization Flow

**Purpose**
Defines the complete startup sequence of the game, including manager discovery, validation, initialization order, and initial scene loading.
This document is the **single source of truth** for how the game boots.

---

## Overview

* `GameBootstrap` is the **root initializer**
* It **never unloads**
* All core managers are discovered, validated, and initialized here
* Scene loading is handled **additively**
* Other systems must access managers **only through `GameBootstrap`**

---

## Initialization Sequence (Execution Order)

### 1. Singleton Setup

**Lifecycle:** `Awake()`

**Responsibilities:**

* Ensure only one `GameBootstrap` instance exists
* Persist core systems across scene loads

**Steps:**

* Check existing instance (prevent duplicates)
* `DontDestroyOnLoad(this)`
* `DontDestroyOnLoad(PersistentUI)`

---

### 2. Manager Discovery

**Lifecycle:** `Start() → DiscoverManagers()`

**Responsibilities:**

* Locate all required managers under the Bootstrap hierarchy

**Managers Discovered:**

* `SaveManager`
* `CharacterManager`
* `InventoryManager`
* `EquipmentManager`
* `SkillLoadoutManager`
* `SceneFlowManager`

**Method Used:**

* `GetComponentInChildren<T>()`

---

### 3. Validation

**Lifecycle:** `Start() → ValidateManagerReferences()`

**Responsibilities:**

* Ensure all required managers were found

**Rules:**

* All manager references must be non-null
* Missing managers throw a hard error
* Game must not continue in a broken state

---

### 4. Manager Initialization

**Lifecycle:** `Start() → InitializeManagers()`

Managers are initialized **in strict order** to satisfy dependencies.

| Order | Manager             | Dependencies           |
| ----- | ------------------- | ---------------------- |
| 1     | SaveManager         | None                   |
| 2     | CharacterManager    | `CharacterBaseStatsSO` |
| 3     | InventoryManager    | `GameDatabaseSO`       |
| 4     | EquipmentManager    | Character + Inventory  |
| 5     | SkillLoadoutManager | None (stub)            |
| 6     | SceneFlowManager    | `SceneManifest`        |

⚠️ **Order matters. Do not reorder without updating dependencies.**

---

### 5. Determine Starting Scene

**Lifecycle:** `Start() → DetermineStartingScene()`

**Decision Logic:**

* If no save exists → `AvatarCreation`
* If save exists and loads successfully → `MainBase`
* If save is corrupted → fallback to `AvatarCreation`

This logic prevents soft-locks caused by broken save data.

---

### 6. Load Initial Scene

**Lifecycle:** `Start() → LoadSceneAsync()`

**Responsibilities:**

* Load the chosen scene **additively**
* Keep Bootstrap alive
* Set correct active scene
* Notify UI and dependent systems

**Steps:**

* Load scene additively
* `SetActiveScene()`
* `SceneFlow.SetCurrentMainScene()`
* `GameEvents.TriggerSceneLoaded()`

---

## Manager Dependency Order (Summary)

Managers must be initialized in this exact order:

1. **SaveManager**
   Dependencies: None

2. **CharacterManager**
   Dependencies: CharacterBaseStatsSO (serialized)

3. **InventoryManager**
   Dependencies: GameDatabaseSO (serialized)

4. **EquipmentManager**
   Dependencies: Character + Inventory + Database

5. **SkillLoadoutManager**
   Dependencies: None (stub)

6. **SceneFlowManager**
   Dependencies: SceneManifest (serialized)

---

## Critical Implementation Notes

* `EquipmentManager` **must** initialize after Character and Inventory
* `SceneFlowManager` lives in **Core/**, not Managers (infrastructure-level)
* `GameBootstrap` **never unloads**
* All managers are accessed through `GameBootstrap` static references

---

## Correct Access Pattern

### ✅ Correct

```csharp
GameBootstrap.Character.CreateNewPlayer("Hero");
GameBootstrap.Inventory.AddToBag("sword_001", 1);
GameBootstrap.Equipment.EquipItem("sword_001");
GameBootstrap.SceneFlow.LoadMainScene("03_MainBase");
```

### ❌ Incorrect (Do Not Use)

```csharp
CharacterManager.Instance.CreateNewPlayer();
SceneFlowManager.Instance.LoadMainScene();
```

**Reason:**
Direct singleton access bypasses initialization guarantees and breaks boot order safety.

---

## Final Notes

* If something breaks during startup, check this document first
* Any new manager must:

  1. Be discovered
  2. Be validated
  3. Declare dependencies
  4. Be placed correctly in init order

If it’s not documented here, it’s not officially part of the boot flow.
