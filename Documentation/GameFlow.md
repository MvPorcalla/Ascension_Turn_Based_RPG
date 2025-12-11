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


        ┌──────────────────────────────────┐
        │           UI Layer               │
        │   (Ascension.UI)                 │
        └────────────┬─────────────────────┘
                     │ depends on
        ┌────────────▼─────────────────────┐
        │        Manager Layer             │
        │   (Ascension.Manager)            │
        └────────────┬─────────────────────┘
                     │ depends on
        ┌────────────▼─────────────────────┐
        │    Character | Inventory |       │
        │    GameSystem                    │
        └────────────┬─────────────────────┘
                     │ depends on
        ┌────────────▼─────────────────────┐
        │         Data Layer               │
        │    (Ascension.Data)              │
        │    ScriptableObjects, Models     │
        └──────────────────────────────────┘


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
