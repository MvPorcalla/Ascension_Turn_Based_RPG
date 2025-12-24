# Modular Game Architecture Documentation

This document outlines the recommended architecture for a modular Unity game project using asmdefs, ensuring clean dependencies, scalability, and modularity.

---

## 1. Overview

The project is organized as *modular systems* coordinated by a *GameHub* and managed by a *GameManager*. Each system is encapsulated in its own assembly definition (.asmdef) for independence and reusability.

### Key Concepts

* *Modules / Systems*: Self-contained game logic (CharacterSystem, InventorySystem, CombatSystem, etc.).
* *GameHub*: Orchestrates modules, handles communication and coordination.
* *GameManager*: Central brain of the game, manages global state and high-level flow.
* *SaveManager*: Handles game persistence.

---

## 2. Layered Architecture

### Core Layer

* *Contents*: Utilities, SaveManager, logging, configuration.
* *Assembly*: Ascension.Core (.asmdef)
* *Purpose*: Provides foundational services to all other layers.

### System Modules Layer

* *Contents*: CharacterSystem, InventorySystem, CombatSystem, EquipmentSystem, MarketSystem, etc.
* *Assembly*: One asmdef per module (e.g., Ascension.Character, Ascension.Inventory).
* *Purpose*: Encapsulates independent game logic.
* *Dependencies*: Can reference Core only; should not reference other modules directly.

### Orchestrator Layer

* *Contents*: GameHub
* *Assembly*: Ascension.Hub (.asmdef)
* *Purpose*: Coordinates all modules, handles inter-module communication.
* *Dependencies*: References Core + all System Modules.

### Central Brain Layer

* *Contents*: GameManager
* *Assembly*: Ascension.Manager (.asmdef)
* *Purpose*: Holds global game state, manages high-level flow, interacts with GameHub.
* *Dependencies*: References Core + GameHub.

### Persistence Layer

* *Contents*: SaveManager
* *Purpose*: Handles saving/loading game data.
* *Dependencies*: Can be accessed by modules via Core interfaces.

---

## 3. Dependency Flow

Core (SaveManager, utilities)
      ↓
System Modules (Character, Inventory, Combat, etc.)
      ↓
GameHub (Orchestrator of modules)
      ↓
GameManager (Central Brain)

*Rules:*

1. Modules do not reference other modules directly.
2. GameHub references all modules.
3. GameManager references GameHub (not modules).
4. SaveManager can be used by modules or GameManager via Core interfaces.

---

## 4. asmdef Layout

Scripts/
├── Core/                → Ascension.Core.asmdef
│   └── SaveManager.cs
├── Manager/             → Ascension.Manager.asmdef
│   └── GameManager.cs
├── Hub/                 → Ascension.Hub.asmdef
│   └── GameHub.cs
├── CharacterSystem/     → Ascension.Character.asmdef
├── InventorySystem/     → Ascension.Inventory.asmdef
├── CombatSystem/        → Ascension.Combat.asmdef
├── EquipmentSystem/     → Ascension.Equipment.asmdef
└── MarketSystem/        → Ascension.Market.asmdef

*Notes:*

* Only create asmdefs for folders containing substantial code to avoid unnecessary overhead.
* Keep dependencies explicit and minimal.
* Avoid circular references by following the dependency flow rules.

---

## 5. Diagram


                ┌───────────────┐
                │  GameManager  │   ← brain / flow control
                └───────┬───────┘
                        │
                        ▼
               ┌──────────────────┐
               │ ServiceContainer │  ← registry / locator
               └────────┬─────────┘
                        │
   ┌────────────────────┼────────────────────┼────────────── more Modules..
   │                    │                    │
   ▼                    ▼                    ▼
CharacterManager   InventoryManager     SaveManager
   │                    │                    │
   └────── data ────────┴────────── data ────┘

This diagram illustrates module orchestration and allowed dependencies.

---

**Cross-Module Communication Rule:**

All game modules (CharacterSystem, InventorySystem, CombatSystem, etc.) are **independent**. If a module needs functionality from another module:

1. It **requests the target system from `ServiceContainer`**.
2. It calls the required method on that system.
3. The result is returned to the requesting module.

**Example:**

* InventorySystem wants to use a potion to heal a player or apply a buff.
* InventorySystem **does not directly call** `PotionManager`.
* Instead, it asks `ServiceContainer` for `PotionManager` and calls `UsePotion(...)`.
* Any effect (healing, buff) is applied through `PotionManager` and the results flow back to InventorySystem.

This ensures **loose coupling**, **centralized orchestration**, and **easy swapping/testing** of modules without breaking dependencies.

---

*Conclusion:*
This architecture ensures:

* Modularity and reusability of systems.
* Clear dependency flow with no circular references.
* Scalability as new modules can be added without breaking existing code.
* Easy asmdef management for Unity projects.