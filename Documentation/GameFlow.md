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



# DisclaimerLayout

Always load DisclaimerPanel first

Canvas (Screen Space - Overlay, 1920x1080)
│
├── BackgroundLayer (optional full-screen background)
│   └── Image component (background color or sprite)
│
└── ScreenDisclaimer (fills screen)
│    ├── DisclaimerPanel
│    │    ├── Header
│    │    │    └── Title
│    │    ├── Disclainer
│    │    │    └── DisclaimerText
│    │    ├── Acknowledge
│    │    │    ├── Toggle (UI)
│    │    │    ├── DisclaimerText
│    │    │    └── ToSButton
│    │    └── ActionButton
│    │         ├── AgreeButton
│    │         └── ExitButton
│    │
│    └── ToSPanel
│         ├── Header
│         │    ├── Title
│         ├── TermsOfServer
│         │    ├── viewport...
│         ├── ActionButton
│         │    ├── AgreeButton
│         │    ├── BackButton (bak to DisclaimerPanel)

help me create script for my Discalimer
Disclaimer make them check the toggel first before allowing the agree button to be clickable, the exit is always clicakble
also make the Term of Service as button that open a panel called TOSpanel

Disclaimer -> agree -> 01_Bootstrap
Disclaimer -> exit -> quit

