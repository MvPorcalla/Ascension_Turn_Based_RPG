
---

# Ascension: Turn-Based RPG

**Ascension** is a portrait-mode, mobile-first 2D turn-based RPG built in Unity 2022.3 LTS. Players can fully customize their characters’ stats, abilities, and equipment, creating unique builds instead of following fixed templates.

Combat is 1 vs multiple enemies per room (5–20). Turn order is determined by Attack Speed. Skills have per-turn cooldowns and define their maximum number of selectable targets. The skill system is equipment-driven: equipped weapons unlock specific skills. Skills unlock via level and stat requirements rather than classes, and core stats (STR, AGI, END, WIS) are fully respeccable. Gear comes in rarity tiers. The theme is fantasy isekai, and the UI is tap-based.

## Features
- **Flexible Character Builds** – allocate points into Strength, Intelligence, Agility, Endurance, and Wisdom to define playstyle
- **Modular Inventory & Equipment System** – manage weapons, armor, and consumables with intuitive UI
- **Equipment-Driven Skill System** – skills are unlocked and usable based on equipped weapons, enabling dynamic, build-specific combat strategies
- **Turn-Based Combat System** – tactical battles with dynamic actions
- **Storage & Loadout UI** – optimized for mobile and scalable to PC
- **Clean, Modular Code** – architecture designed for maintainability and future expansion

## About This Project
This is a personal prototype exploring **modular game architecture, clean UI/UX design, and scalable inventory systems**. It demonstrates my skills in **Unity development, C# programming, and game system design**, with a focus on creating **polished, player-friendly interfaces**.

---

## Contact & Permissions

Hi there! 👋  

If you'd like to **use, reference, or contribute** to this project, please feel free to reach out. I’m happy to discuss ideas, collaborations, or just chat about the project.  

You can contact me via:

- Email: scryptid1@gmail.com  
- GitHub: [MvPorcalla](https://github.com/MvPorcalla)  

Please note: All rights reserved. Do not redistribute or claim this work as your own without permission.

---

**Engine:** Unity 2022.3.62f2 LTS (2D)
**Platform Target:** Mobile (primary), potentially PC later
**Version Control:** GitHub

## Setup Instructions

1. **Clone the repository**

```bash
git clone https://github.com/MvPorcalla/Ascension_Turn_Based_RPG.git
```

2. **Open the project in Unity**

   * Launch Unity Hub.
   * Click `Add` and select the folder where you cloned the repository.
   * Open the project using Unity 2022.3.62f2 LTS.

3. **Check Scripting Runtime / API Compatibility** (optional, default is fine)

   * Go to `Edit → Project Settings → Player → Other Settings`.
   * Ensure **Scripting Runtime Version** is `.NET 4.x Equivalent`.

4. **Install required packages**

   * Open `Window → Package Manager`.
   * Make sure **TextMeshPro (TMP)** is installed.
   * (Optional) You may later add **Addressables** for asset management.

5. **Version Control Best Practices**

   * This project uses Git. `.gitignore` is already set up for Unity.
   * Make commits often with clear messages.
   * Push branches to GitHub for backup and collaboration.

6. **Running the project**

   * Press `Play` in the Unity Editor. It doesn't matter which scene you're currently in — the game will always start at 00_Disclaimer.

---