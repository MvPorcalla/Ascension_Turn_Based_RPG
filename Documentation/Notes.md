# Ascension: Turn-Based RPG

**Engine:** Unity 2022.3.62f2 LTS (2D)
**Platform Target:** Mobile first (potentially PC later)
**Version Control:** GitHub

---

## Setup Instructions

1. **Clone the repository**

```bash
git clone https://github.com/MvPorcalla/Ascension_Turn_Based_RPG.git
```

2. **Open the project in Unity**

   * Launch Unity Hub.
   * Click `Add` and select the folder where you cloned the repository.
   * Open the project using Unity 2022.3.62f2 LTS.

3. **Check Scripting Runtime / API Compatibility**

   * Go to `Edit → Project Settings → Player → Other Settings`.
   * Ensure **Scripting Runtime Version** is `.NET 4.x Equivalent`.

4. **Install required packages**

   * Open `Window → Package Manager`.
   * Make sure **TextMeshPro (TMP)** is installed.
   * (Optional) You may later add **Addressables** for asset management.

5. **Version Control Best Practices**

   * This project uses Git. `.gitignore` is already set up for Unity.
   * Commit often with clear messages.
   * Push branches to GitHub for backup and collaboration.

6. **Running the project**

   * Open the starting scene (`MainScene` or equivalent).
   * Press `Play` in the Unity Editor to test.

---

## Quick Development Notes

### Weapons

* Each weapon has **1 default weapon skill**, **2 normal skills**, and **1 ultimate skill**.
* Weapons belong to a **category** to determine which skills can be equipped.
* **Attribute bonuses**: STR, INT, AGI, END, WIS (added to character’s base attributes).
* **Item-only bonuses**: Crit Damage, Lethality, Magic Penetration, Physical Penetration (applied directly to derived stats).

### Skills

* Skills can only be equipped on compatible weapon types.
* **Skill cooldowns** are measured in turns.
* Default weapon skills **do not have cooldowns**.
* Normal skills have **short cooldowns** (2–5 turns).
* Ultimate skills have **long cooldowns**.
* Skills have **max target** specifying how many enemies they hit per turn.
* Target types include Single, Multi, AllEnemies, Self, AllAllies.

### Combat & Gameplay

* Turn-based system: each action counts as a turn.
* Quests can guide the player in weapon selection and skill unlocking.

### Battle System

* When entering a dungeon floor or battle room, the player faces **all enemies on that floor simultaneously**.
* Combat is **turn-based**: each player action counts as a turn.
* **Attack mechanics:**

  * When the player attacks, all enemies that are alive will counterattack according to their AI.
  * Skills can hit multiple enemies depending on the skill’s **max target** value.
* Strategy comes from **skill choice and targeting**, as each skill may hit single, multiple, or all enemies.

### Notes

* Mobile-first design, but PC support may be added later.
* No mana system currently; skills are only limited by cooldowns.
* Project uses **TMP** for text rendering.

---