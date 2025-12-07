# Refactor: **Ascension — Minimal, Opinionated Coding Standard**

No fluff. Same rules, tighter layout, easier to follow. Use this as your single-source-of-truth for generating Unity C# code for Ascension.

---

# 1 — Big Picture (single sentence)

One class per file, feature-grouped namespaces and folders, readable/robust Unity patterns for turn-based 2D games.

---

# 2 — Project / Folder / Namespace Mapping

Map folders to namespaces. Keep feature grouping strict.

Folder → Namespace (examples)

* `Scripts/Core` → `Ascension.Core`
* `Scripts/Manager` → `Ascension.Manager`
* `Scripts/CharacterSystem/Manager` → `Ascension.Character.Manager`
* `Scripts/CharacterSystem/Stat` → `Ascension.Character.Stat`
* `Scripts/InventorySystem/Manager` → `Ascension.Inventory.Manager`
* `Scripts/CombatSystem/Manager` → `Ascension.Combat.Manager`
* `Scripts/CombatSystem/Unit` → `Ascension.Combat.Unit`
* `Scripts/Data/ScriptableObject/Combat` → `Ascension.Data.SO.Combat`
* `Scripts/UI` → `Ascension.UI.*`

Always keep one class per file; filename must equal class name.

---

# 3 — File header + formatting

Every file starts with:

```csharp
// ════════════════════════════════════════════
// ClassName.cs
// Short purpose (one line)
// ════════════════════════════════════════════
```

Indent: 4 spaces. Braces on new lines. No magic numbers; prefer constants. Use `Awake` for component init, `Start` for cross-object init. Minimize `Update()`.

---

# 4 — Naming & style rules (short)

* Classes / Methods: `PascalCase`
* Private fields: `camelCase`
* Inspector fields: `[SerializeField] private`
* Constants: `UPPER_SNAKE_CASE`
* Interfaces: `IName`
* Events: `OnPascalCase`
* Fewer than 80 lines → no regions. Over 80 → use the region order: Serialized, Private, Properties, Unity callbacks, Public, Private, Events.

---

# 5 — Patterns to always use

* **State machine** for battle states (enum in `Ascension.Combat.Manager`).
* **ScriptableObjects** for data (units, abilities, items).
* **Event-driven** communication between systems (static events or dedicated event bus).
* **Turn queue**: build, sort by initiative, dequeue and process.
* **Pooling** for effects/UI; virtualization for long lists.

---

# 6 — Unity best-practices (short)

* Cache components in `Awake`.
* Subscribe/unsubscribe in `OnEnable`/`OnDisable`.
* Validate references and disable component (or log error) on missing critical refs.
* Use coroutines for turn sequences — no heavy Update loops.
* Use `Destroy(gameObject)` only for duplicates in singletons.

---

# 7 — Error handling / logs

* `Debug.LogError` for fatal/invalid setup.
* `Debug.LogWarning` for recoverable issues.
* `Debug.Log` sparingly for important state changes.

---

# 8 — Generation checklist (to enforce)

* [ ] Header present
* [ ] Proper feature-grouped namespace
* [ ] One class per file + matching filename
* [ ] No magic numbers
* [ ] Null checks for critical refs
* [ ] Event hooks used for inter-system comms
* [ ] Methods single-responsibility
* [ ] Regions only when >80 lines

---

# 9 — Minimal examples

## CombatUnit (trimmed)

```csharp
// ════════════════════════════════════════════
// CombatUnit.cs
// Lightweight unit runtime wrapper
// ════════════════════════════════════════════

using UnityEngine;
using Ascension.Data.SO.Combat;

namespace Ascension.Combat.Unit
{
    public class CombatUnit : MonoBehaviour, IDamageable
    {
        [SerializeField] private UnitDataSO unitData;
        private int currentHealth;
        public int CurrentHealth => currentHealth;
        public UnitDataSO Data => unitData;

        void Awake() {
            if (unitData == null) {
                Debug.LogError("UnitData missing", this);
                enabled = false; return;
            }
            currentHealth = unitData.maxHealth;
        }

        public void TakeDamage(int dmg) {
            if (currentHealth <= 0) return;
            currentHealth = Mathf.Max(0, currentHealth - dmg);
            if (currentHealth == 0) Die();
        }

        private void Die() {
            // small responsibilities: update state, raise event
            OnUnitDeath?.Invoke(this);
        }

        public static event System.Action<CombatUnit> OnUnitDeath;
    }
}
```

## BattleManager (trimmed)

```csharp
// ════════════════════════════════════════════
// BattleManager.cs
// Orchestrates turn queue and battle state
// ════════════════════════════════════════════

using UnityEngine;
using System.Collections.Generic;
using Ascension.Combat.Unit;

namespace Ascension.Combat.Manager
{
    public enum BattleState { PlayerTurn, EnemyTurn, Victory, Defeat }

    public class BattleManager : MonoBehaviour
    {
        public static BattleManager Instance { get; private set; }
        [SerializeField] private List<CombatUnit> playerUnits;
        [SerializeField] private List<CombatUnit> enemyUnits;

        private Queue<CombatUnit> turnQueue;

        void Awake() {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            turnQueue = new Queue<CombatUnit>();
        }

        void Start() {
            InitializeBattle();
        }

        private void InitializeBattle() {
            BuildTurnQueue();
            StartPlayerTurn();
        }

        private void BuildTurnQueue() {
            var all = new List<CombatUnit>();
            all.AddRange(playerUnits);
            all.AddRange(enemyUnits);
            all.Sort((a,b) => b.Data.initiative.CompareTo(a.Data.initiative));
            foreach (var u in all) turnQueue.Enqueue(u);
        }

        public void EndTurn() {
            if (turnQueue.Count == 0) BuildTurnQueue();
            var next = turnQueue.Dequeue();
            OnUnitTurnStart?.Invoke(next);
        }

        public static event System.Action<CombatUnit> OnUnitTurnStart;
        public static event System.Action<BattleState> OnTurnStart;

        private void StartPlayerTurn() {
            OnTurnStart?.Invoke(BattleState.PlayerTurn);
        }
    }
}
```

---

# 10 — Quick dos & don'ts

Do:

* Group features, keep namespaces predictable.
* Use ScriptableObjects for data.
* Build small, testable systems.

Don't:

* Dump many responsibilities into one manager.
* Use `GetComponent` in `Update`.
* Mix unrelated systems in one namespace.

---
