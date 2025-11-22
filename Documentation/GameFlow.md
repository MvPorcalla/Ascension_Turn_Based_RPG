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