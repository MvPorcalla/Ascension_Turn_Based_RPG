## **Game Flow Map**

### **00_Disclaimer**

* **Scene:** Disclaimer UI (`UI/Panel/DisclaimerController.cs`)
* **Purpose:** Show legal/disclaimer info.
* **Flow:**

  1. Player clicks **Agree / Proceed**.
  2. SceneController → `LoadScene("01_Bootstrap")`.

---

### **01_Bootstrap**

* **Scene:** Bootstrap (`AppFlow/Bootstrap.cs`)
* **Purpose:** Initialize game systems & services.
* **Flow:**

  1. `Bootstrap` initializes `ServiceContainer`.
  2. Auto-register all `IGameService` components:

     * `SaveManager`
     * `PlayerStateController`
     * `GameManager`
  3. Check if a **save file exists** via `SaveManager.SaveExists()`.

     * **Yes:** Load `SaveData` and proceed to `03_GameBase`.
     * **No:** Load `02_CharacterCreation`.

---

### **02_CharacterCreation**

* **Scene:** Character creation UI (`AppFlow/SceneController.cs` + `CharacterSystem/UI/CharacterCreationManager.cs`)

* **Purpose:** Let player create a new character before starting.

* **Flow:**

  1. Player sets name, appearance, initial stats, etc.
  2. Confirm creation → Save data via `SaveManager.SaveGame()`.
  3. Update `PlayerStateController` with the new character.
  4. Proceed to `03_GameBase`.

* **Notes:**

  * No existing save is needed here.
  * Ensure `CharacterCreationManager` only handles UI + input; persistence goes to `SaveManager`.

---

### **03_GameBase**

* **Scene:** Main gameplay (`AppFlow/GameManager.cs` + modules active)
* **Purpose:** Start the actual game.
* **Flow:**

  1. Load player data from `SaveManager` if exists.
  2. Initialize all gameplay modules:

     * `CharacterManager`
     * `InventoryManager`
     * `EquipmentSystem`
     * Any runtime systems (combat, leveling, etc.)
  3. UI modules initialize:

     * `PlayerHUD`
     * `ProfilePanel`
  4. Game loop runs; all save/load actions go through `SaveManager`.
  5. Scene transitions:

     * Save/Load points
     * Optional mini-game or menu transitions

---

### **Additional Notes**

* Scene transitions are handled **only by `SceneController`**.
* `PlayerStateController` keeps track of:

  * Current session
  * CanSave() status
  * Flags like `HasCreatedCharacter`
* `SaveManager` is the single source of truth for serialization — nothing else writes save files directly.
* Future expansion (e.g., new game modes or optional scenes) can be added between Bootstrap and GameBase.

---

## Script Flow

Bootstrap.Start()
    ↓
ServiceContainer.Awake()
    └─ Auto-discovers all IGameService components
    ↓
ServiceContainer.Start()
    └─ Initializes services in order:
        1. SaveManager.Initialize()
        2. CharacterManager.Initialize()
        3. InventoryManager.Initialize()
        4. PlayerStateController.Initialize()
        5. SaveController.Initialize()
        6. SceneController.Initialize()
        7. GameManager.Initialize()
    ↓
    └─ Fires OnAllSystemsReady event
    ↓
Bootstrap continues
    └─ Checks if save exists
        ├─ YES → LoadGame() → MainBase
        └─ NO → CharacterCreation
    ↓
CharacterCreationManager loads
    └─ User customizes character
    └─ OnConfirmClicked()
        └─ GameManager.StartNewGame() ← CREATES PLAYER HERE
        └─ Apply customizations
        └─ Save and proceed to MainBase

---

## 📊 Dependency Tree

ServiceContainer (initializes everything)
    ↓
SaveManager (no dependencies) ← IGameService ✅
    ↓
CharacterManager (depends on SaveManager) ← IGameService ✅
    ↓
InventoryManager (depends on CharacterManager) ← IGameService ✅
    ↓
GameManager (depends on all controllers) ← IGameService ✅
    ↓
PotionManager (depends on CharacterManager) ← NOT IGameService ✅
    ↓
UI Components (depend on managers) ← NOT IGameService ✅

---

## Service Container System

Scene: 01_Bootstrap (or any scene with ServiceContainer)
├── GameSystem (GameObject) (Component: ServiceContainer.cs)
│   ├── GameManager (Component: GameManager.cs)
│   ├── PlayerStateController (Component: PlayerStateController.cs)
│   ├── SaveController (Component: SaveController.cs)
│   ├── SceneController (Component: SceneController.cs)
│   ├── SaveManager (Component: SaveManager.cs)
│   ├── CharacterManager (Component: CharacterManager.cs)
│   ├── PotionManager (Component: PotionManager.cs)
│   ├── InventoryManager (Component: InventoryManager.cs)
│   └── EquipmentManager (Component: EquipmentManager.cs)