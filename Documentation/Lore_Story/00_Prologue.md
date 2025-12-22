**Prologue Cutscene: Player Backstory**
(Narrated in third person)

Once upon a Tuesday—which, unsurprisingly, was the worst day of the week—a perfectly average Office Worker was trudging home, dragging the weight of unpaid bills, endless meetings, existential dread, and a lifetime streak of romantic failures.

And then… **Bam!!!.**

He got hit. Not metaphorically. Not emotionally.
Literally. By a truck.
Heh—yeah, the motherfucker got the full Truck-kun treatment.

But here’s the kicker: **he wasn’t supposed to die.**
The Reaper? Yes, that’s actually a job...
They messed up. 
Apparently, someone else was on the “to-die” list, but our unlucky hero won the cosmic accident lottery.

This guy just can’t catch a break.

So the universe, with its terrible sense of humor, offered two options:

1. **Reincarnate in his own world in a random body.** Could be a bird, a worm, or a sentient rock. Roll the dice.
2. **Get isekai’d into a new world by replacing someone already dying.** A hunter who survived a brutal fight, broke every bone except for one, and was one sneeze away from death.

Naturally, he chose the second one. Because sure, that’s *logical enough* for someone who already died by truck.

And just like that, our barely-clever poor soul woke up inside a stranger’s shattered body, in a world that didn’t care about his salary, office politics, or personal grievances. A world with towers that apparently go up forever, monsters that are way too enthusiastic about killing him, and a destiny that sounded suspiciously like “become a god.”


Maybe, just maybe, in this world he could stop being a wizarding virgin… learn a thing or two… and, who knows, finally meet someone who wasn’t imaginary.

But hey… what could go wrong?

**Spoiler: everything.**

**Cutscene end.**

---

====================================================================================

Sup Pips! I’ve been stuck on this shitty game I’m making for 3 weeks now and could really use some opinions, ideas, or brutal honesty. Anything helps

---

I’m currently working on the player’s backstory. Not sure if it’s good or just straight-up cringe. TBH i fell like my writing is cringe.

**Prologue Cutscene: Player Backstory**

Once upon a Tuesday—which, unsurprisingly, was the worst day of the week—a perfectly average Office Worker was trudging home, dragging the weight of unpaid bills, endless meetings, existential dread, and a lifetime streak of romantic failures.

And then… Bam!!!.

He got hit. Not metaphorically. Not emotionally.
Literally. By a truck.
Heh—yeah, the motherfucker got the full Truck-kun treatment.

But here’s the kicker: he wasn’t supposed to die.
The Reaper? Yes, that’s actually a job...
They messed up. 
Apparently, someone else was on the “to-die” list, but our unlucky hero won the cosmic accident lottery.

This guy just can’t catch a break.

So the universe, with its terrible sense of humor, offered two options:

1. Reincarnate in his own world in a random body. Could be a bird, a worm, or a sentient rock. Roll the dice.
2. Get isekai’d into a new world by replacing someone already dying. A hunter who survived a brutal fight, broke every bone except for one, and was one sneeze away from death.

Naturally, he chose the second one. Because sure, that’s logical enough for someone who already died by truck.

And just like that, our barely-clever poor soul woke up inside a stranger’s shattered body, in a world that didn’t care about his salary, office politics, or personal grievances. A world with towers that apparently go up forever, monsters that are way too enthusiastic about killing him, and a destiny that sounded suspiciously like “become a god.”


Maybe, just maybe, in this world he could stop being a wizarding virgin… learn a thing or two… and, who knows, finally meet someone who wasn’t imaginary.

But hey… what could go wrong?

Spoiler: everything.

**Cutscene end.**

---

Man, it took me a full week to balance and finalize the stats formula on the character (that shit is brutal my tiny brain hurts)
I want to gather some opinions on this piece-of-shit stats system.

**Attributes: for customizing Characters**

* STR → Affects: Attack Damage
* INT → Affects: Ability Power
* AGI → Affects: Attack Speed, Crit Rate, Evasion
* WIS → Affects: Health (HP), Defense
* END → Affects: Tenacity, Defense
* CHR → Removed (might add it back later for Market Negotiation Mechanics)

**Derived Stats:**

* Attack Damage
* Ability Power
* Health
* Defense – combined from Physical & Magical Defense (AR + MR)
* Attack Speed – determines turn order
* Crit Rate – attribute contribution capped at 60%; weapons/gear can add more
* Crit Damage – from Weapon and Gear
* Evasion – affects Dodge Chance; attribute contribution capped at 40%; weapons/gear can add more
* Tenacity – affects CC Resistance; attribute contribution capped at 40%; weapons/gear can add more
* Lethality – flat Penetration; from Weapon and Gear
* Penetration – % Penetration; from Weapon and Gear, combined Physical & Magical Pen, capped at 80%
* Lifesteal – from Weapon and Gear


* Is this too complicated?
* Do these stats make sense at a glance?
* Are any derived stats unclear or unnecessary?
* Do any attributes feel too strong or too weak?
* Is the attribute → derived stat mapping clear?

Any opinion helps :>

---


What do you think of this battle system before I commit to these mechanics, to save myself some headache when implementing it in C#.

Battle System
When the player enters a room or floor, they immediately enter battle mode, and the UI switches to the Battle HUD.
Combat is all enemies in the room versus the player and up to 3 NPC party members in the lineup.
Players can select enemies to attack based on the skill's maximum target limit.
Team composition matters.
Currently considering only female NPCs for… reasons 🤔
THB balancing this is tricky — don’t want enemies one-shotting player, but also don’t want floors to feel impossible.

---

Battle Area Types
1. Tower of Ascension — 10 rooms per floor, last room has a boss.
2. Dungeon — farming areas that reset if you leave early, but better rewards if cleared.
3. Gates — only appear on city areas, 2–3 floors, boss on final floor, failing gives penalty debuffs.

---

Boss Types
There are different boss types with unique mechanics. For example: 
- a Berserker-type boss that goes berserk when its HP hits a threshold, 
- a Necromancer-type boss that summons minions, 
- a Spellcaster-type boss that bombards player with multiple powerful skills,
- and many more.

---

Weapon & Skill System
Players can equip weapons. Each weapon type (e.g., sword, bow, staff, axe, dagger) has its own unique embedded skill and three skill slots: two normal skills and one ultimate.
Each skill is restricted to its weapon type. If you equip a sword, you can only equip sword-type skills.
So the setup is: 1 default weapon skill + 2 normal skills + 1 ultimate.
Default skills have no per-turn cooldown—they function as your basic attack. Normal and ultimate skills have per-turn cooldowns. (Example: if an ultimate has a 6-turn cooldown, once used, it becomes unavailable for the next 6 turns.)
Each skill has a max target cap, meaning you can only select up to that number of targets. For example, a Cleave skill with a cap of 3 lets you target 3 enemies. (This mechanic is important for the battle system.)

---

Titles & Traits (No Job Classes)
This game doesn’t use traditional job classes because I don’t want to restrict players from experimenting with different builds. Instead, players can equip Titles and Traits (I’ll be adding tons of them with various buffs and stat bonuses).
These function as the game’s “class system,” but they’re fully interchangeable, letting players mix and match however they want. (Also at the Temple in city you can do full reset of your stats for freedom of experimentation)

---

TBH, there’s still tons of content I want to add:
Blacksmith crafting with a mini-game (hit the green area on a slider to affect gear rarity)
Potion brewing using the same mini-game mechanic
VN-style recruitment for NPC party members
Full world story lore to explore
Market/Blackmarket system that changes based on in-game time
Reputation building and much more juicy content