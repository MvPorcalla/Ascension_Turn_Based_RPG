┌─────────────────────────────────────────────────────────────┐
│                     01_Bootstrap                            │
│  • Initialize all singletons                                │
│  • Check for existing save                                  │
└─────────────────┬───────────────────────┬───────────────────┘
                  │                       │
           No Save Found            Save Found
                  │                       │
                  ▼                       ▼
┌─────────────────────────────┐  ┌─────────────────────────────┐
│    02_AvatarCreation        │  │       03_MainBase           │
│  • One-time character setup │  │  • Main hub / landing page  │
│  • Allocate starting points │  │  • Access dungeons, shop,   │
│  • Save & go to MainBase    │  │    inventory, etc.          │
└──────────────┬──────────────┘  └─────────────────────────────┘
               │                              ▲
               └──────────────────────────────┘
                    (Never returns here)



00_Disclaimer (first launch only)
01_Bootstrap (initialization)
02_TitleScreen
    ├─ If player has save: Continue → Mainbase
    └─ If new game:
         a. PrologueCutscene
         b. AvatarCreation
         c. Mainbase
03_PrologueCutscene (Only if New Game) 
04_AvatarCreation (Only if New Game) 
05_Mainbase (Game)

0..... 5 more scene


// ════════════════════════════════════════════════════════════════════════
// ASSEMBLY DEFINITION FILES (.asmdef) - COMPLETE SETUP FOR ASCENSION
// Place each .asmdef file in the corresponding folder root
// ════════════════════════════════════════════════════════════════════════

// ────────────────────────────────────────────────────────────────────────
// FILE: Scripts/Data/Ascension.Data.asmdef
// PURPOSE: Core data structures - NO DEPENDENCIES (foundation layer)
// ────────────────────────────────────────────────────────────────────────
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

// ────────────────────────────────────────────────────────────────────────
// FILE: Scripts/Character/Ascension.Character.asmdef
// PURPOSE: Character systems - DEPENDS ON: Data
// ────────────────────────────────────────────────────────────────────────
{
    "name": "Ascension.Character",
    "rootNamespace": "Ascension.Character",
    "references": [
        "GUID:your-data-guid-here"
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

// ────────────────────────────────────────────────────────────────────────
// FILE: Scripts/Inventory/Ascension.Inventory.asmdef
// PURPOSE: Inventory system - DEPENDS ON: Data
// ────────────────────────────────────────────────────────────────────────
{
    "name": "Ascension.Inventory",
    "rootNamespace": "Ascension.Inventory",
    "references": [
        "GUID:your-data-guid-here"
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

// ────────────────────────────────────────────────────────────────────────
// FILE: Scripts/GameSystem/Ascension.GameSystem.asmdef
// PURPOSE: Game systems - DEPENDS ON: Data, Character
// ────────────────────────────────────────────────────────────────────────
{
    "name": "Ascension.GameSystem",
    "rootNamespace": "Ascension.GameSystem",
    "references": [
        "GUID:your-data-guid-here",
        "GUID:your-character-guid-here"
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

// ────────────────────────────────────────────────────────────────────────
// FILE: Scripts/Manager/Ascension.Manager.asmdef
// PURPOSE: System managers - DEPENDS ON: Data, Character, Inventory, GameSystem
// ────────────────────────────────────────────────────────────────────────
{
    "name": "Ascension.Manager",
    "rootNamespace": "Ascension.Manager",
    "references": [
        "GUID:your-data-guid-here",
        "GUID:your-character-guid-here",
        "GUID:your-inventory-guid-here",
        "GUID:your-gamesystem-guid-here"
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

// ────────────────────────────────────────────────────────────────────────
// FILE: Scripts/Core/Ascension.Core.asmdef
// PURPOSE: Application core - DEPENDS ON: Manager (orchestrates everything)
// ────────────────────────────────────────────────────────────────────────
{
    "name": "Ascension.Core",
    "rootNamespace": "Ascension.Core",
    "references": [
        "GUID:your-manager-guid-here",
        "GUID:your-gamesystem-guid-here"
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

// ────────────────────────────────────────────────────────────────────────
// FILE: Scripts/UI/Ascension.UI.asmdef
// PURPOSE: All UI components - DEPENDS ON: Manager, Character, Inventory, Data
// ────────────────────────────────────────────────────────────────────────
{
    "name": "Ascension.UI",
    "rootNamespace": "Ascension.UI",
    "references": [
        "GUID:your-manager-guid-here",
        "GUID:your-character-guid-here",
        "GUID:your-inventory-guid-here",
        "GUID:your-data-guid-here"
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

// ════════════════════════════════════════════════════════════════════════
// DEPENDENCY GRAPH (Bottom-up, cleanest architecture)
// ════════════════════════════════════════════════════════════════════════

/*
LAYER 1 (Foundation - no dependencies):
└── Data

LAYER 2 (Domain Logic):
├── Character ──→ Data
└── Inventory ──→ Data

LAYER 3 (Systems):
└── GameSystem ──→ Data, Character

LAYER 4 (Controllers):
└── Manager ──→ Data, Character, Inventory, GameSystem

LAYER 5 (Orchestration):
└── Core ──→ Manager, GameSystem

LAYER 6 (Presentation):
└── UI ──→ Manager, Character, Inventory, Data

RULES:
✅ Lower layers NEVER reference higher layers
✅ UI can only talk to Managers (not directly to Character/Inventory)
✅ Core orchestrates but doesn't contain business logic
*/

// ════════════════════════════════════════════════════════════════════════
// SETUP INSTRUCTIONS
// ════════════════════════════════════════════════════════════════════════

/*
STEP 1: Create .asmdef files in order
────────────────────────────────────────
1. Create Ascension.Data.asmdef first (no dependencies)
2. Create Character, Inventory (depend on Data)
3. Create GameSystem (depends on Data, Character)
4. Create Manager (depends on everything below it)
5. Create Core (depends on Manager)
6. Create UI last (depends on Manager layer)

STEP 2: Let Unity generate GUIDs
────────────────────────────────────────
- When you create .asmdef via Unity Editor, GUIDs are auto-generated
- If creating manually, Unity assigns GUIDs on import
- You can find GUIDs by opening the .asmdef in a text editor after creation

STEP 3: Reference by name (easier method)
────────────────────────────────────────
Instead of GUIDs, you can use assembly names:

{
    "name": "Ascension.UI",
    "references": [
        "Ascension.Manager",
        "Ascension.Character",
        "Ascension.Data"
    ]
}

Unity will resolve names to GUIDs automatically.

STEP 4: Verify setup
────────────────────────────────────────
1. Select any .asmdef file in Unity
2. Inspector shows "Assembly Definition References"
3. Drag-and-drop dependencies or use dropdown
4. Check "Auto Referenced" is ON (allows other assemblies to see it)

STEP 5: Test compilation
────────────────────────────────────────
1. Change a UI script
2. Watch Console - should only recompile Ascension.UI.dll
3. Character/Manager assemblies should NOT recompile
*/

// ════════════════════════════════════════════════════════════════════════
// COMMON ERRORS & FIXES
// ════════════════════════════════════════════════════════════════════════

/*
ERROR: "The type or namespace 'X' could not be found"
FIX: Add missing assembly reference in .asmdef dependencies

ERROR: Circular dependency detected
FIX: Restructure - lower layers can't depend on higher layers

ERROR: Changes to any script recompile everything
FIX: Check all folders have .asmdef files, ensure proper references

ERROR: Scripts in folder not found by other assemblies
FIX: Set "Auto Referenced" to true in .asmdef inspector
*/

// ════════════════════════════════════════════════════════════════════════
// PERFORMANCE EXPECTATIONS
// ════════════════════════════════════════════════════════════════════════

/*
BEFORE .asmdef:
- Change 1 UI script → Recompile ALL 200 scripts → 15-30 seconds

AFTER .asmdef:
- Change 1 UI script → Recompile ONLY UI assembly → 2-5 seconds

IMPROVEMENT: 70-85% faster iteration times
*/