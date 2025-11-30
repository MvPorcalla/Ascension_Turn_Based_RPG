# Weapon Rarity System - Setup Guide

## 📋 System Overview

Your weapon rarity system uses **two separate mechanics**:

1. **Stat Multiplier**: Multiplies ALL base stats by a value
2. **Bonus Stat Slots**: Adds random bonus stats on top

This gives you maximum flexibility - rarities can have high multipliers with few bonuses, or low multipliers with many bonuses!

---

## 🎯 Recommended Rarity Configuration

### **Common (White)**
```
Stat Multiplier: 1.0x (baseline)
Bonus Stat Slots: 0
Min/Max Roll: 0% / 0%
Color: RGB(200, 200, 200) - Light Gray
Min Crafting Hits: 0
```
**Use**: Basic weapons, starter gear

### **Rare (Blue)**
```
Stat Multiplier: 1.3x
Bonus Stat Slots: 1
Min/Max Roll: 10% / 25%
Color: RGB(100, 150, 255) - Blue
Min Crafting Hits: 3
```
**Use**: Mid-tier weapons, common drops

### **Epic (Purple)**
```
Stat Multiplier: 1.6x
Bonus Stat Slots: 2
Min/Max Roll: 15% / 35%
Color: RGB(200, 100, 255) - Purple
Min Crafting Hits: 5
```
**Use**: High-tier weapons, rare drops

### **Legendary (Orange)**
```
Stat Multiplier: 2.0x
Bonus Stat Slots: 3
Min/Max Roll: 20% / 50%
Color: RGB(255, 150, 50) - Orange
Min Crafting Hits: 7
```
**Use**: End-game weapons, boss drops

### **Mythic (Red/Gold)**
```
Stat Multiplier: 2.5x
Bonus Stat Slots: 4
Min/Max Roll: 30% / 80%
Color: RGB(255, 50, 50) - Red OR RGB(255, 215, 0) - Gold
Min Crafting Hits: 9-10 (Perfect)
```
**Use**: Ultimate weapons, perfect crafts, special events

---

## 📦 Unity Setup Steps

### **Step 1: Create Rarity Assets**

Right-click in Project:
```
Create → Game → Weapon Rarity
```

Create 5 rarity assets:
- `Rarity_Common.asset`
- `Rarity_Rare.asset`
- `Rarity_Epic.asset`
- `Rarity_Legendary.asset`
- `Rarity_Mythic.asset`

Configure each with the values above.

---

### **Step 2: Create Example Weapon**

Right-click in Project:
```
Create → Game → Weapon
```

**Example: Iron Sword (Common)**
```
Weapon Name: Iron Sword
Weapon Type: Sword
Attack Range: Melee

Rarity Config: [Drag Rarity_Common here]

Base Stats:
├─ Base AD: 50
├─ Base AP: 0
├─ Base HP: 0
├─ Base Defense: 0
└─ Base Attack Speed: 5
```

**Final Stats**: 50 AD, 5 Attack Speed (1.0x multiplier, 0 bonus stats)

---

**Example: Legendary Flamebrand**
```
Weapon Name: Flamebrand
Weapon Type: Sword
Attack Range: Melee

Rarity Config: [Drag Rarity_Legendary here]

Base Stats:
├─ Base AD: 100
├─ Base AP: 50
├─ Base HP: 0
├─ Base Defense: 10
└─ Base Attack Speed: 8
```

**After Rolling Bonus Stats** (right-click weapon → "Roll Bonus Stats"):
- Base Stats × 2.0 = 200 AD, 100 AP, 20 Defense, 16 Attack Speed
- Plus 3 random bonus stats (20%-50% of base values)

Possible result:
```
+200 AD (base × 2.0)
+100 AP (base × 2.0)
+20 Defense (base × 2.0)
+16 Attack Speed (base × 2.0)

Bonus Stats:
• +35 Attack Damage (35% of 100)
• +4.2% Crit Rate (42% of 10 base)
• +8 Lifesteal (40% of 20 base)
```

---

## 🔧 How Stats Are Calculated

### **Formula:**
```
Final Stat = (Base Stat × Rarity Multiplier) + Bonus Stat Value
```

### **Example Breakdown:**

**Iron Sword (Common)**
- Base AD: 50
- Multiplier: 1.0x
- Bonus Slots: 0
- **Final AD: 50**

**Flamebrand (Legendary, with lucky rolls)**
- Base AD: 100
- Multiplier: 2.0x
- Rolled Bonus: +35 AD
- **Final AD: 235** (100 × 2.0 + 35)

---

## 🎮 Future: Blacksmith Crafting

When you implement the blacksmith minigame:

```csharp
// Pseudo-code for crafting
int successfulHits = PlayBlacksmithMinigame();

WeaponRaritySO achievedRarity = DetermineRarity(successfulHits);
// 0-2 hits = Common
// 3-4 hits = Rare
// 5-6 hits = Epic
// 7-8 hits = Legendary
// 9 hits = Mythic
// 10 hits (Perfect) = Heroic

WeaponSO craftedWeapon = CreateWeaponInstance(baseWeapon, achievedRarity);
craftedWeapon.RollBonusStats();

// Player gets the crafted weapon with rolled stats!
```

---

## 🎨 Visual Customization

Each rarity can have:
- **Custom Color**: For UI highlighting
- **Particle Effects**: Glow, sparkles, aura
- **Drop Sound**: Different audio per rarity
- **Icon**: Special border/badge

Add these in the `WeaponRaritySO` asset!

---

## ✅ Testing Your Weapon

Select your weapon in the Inspector, then:

1. **Right-click** → `"Roll Bonus Stats"` - Generates random bonuses
2. **Right-click** → `"Print Weapon Stats"` - Shows full calculation
3. **Play Mode** → Right-click → `"Test: Equip Weapon"` - Equips and shows player stats

---

## 📊 Balance Recommendations

### **Multiplier vs Bonus Slots Trade-off:**

**High Multiplier, Low Slots** (Consistent Power)
- Legendary: 2.5x multiplier, 1 bonus slot
- Reliable, predictable power

**Low Multiplier, High Slots** (Variable Power)
- Legendary: 1.5x multiplier, 5 bonus slots
- More RNG, potentially higher ceiling

**Your Current Setup (Balanced):**
- Scales both multiplier AND slots
- Common: 1.0x, 0 slots → Heroic: 3.0x, 5 slots
- Good progression curve!

---

## 🚀 Advanced: Weighted Bonus Stats

Future enhancement - make certain stats more likely:

```csharp
// In WeaponRaritySO
public BonusStatWeight[] statWeights;

[Serializable]
public class BonusStatWeight
{
    public BonusStatType statType;
    public float weight; // Higher = more likely
}
```

Example:
- Attack Damage: 40% chance
- Crit Rate: 30% chance
- Lifesteal: 10% chance

This lets you control loot tables per rarity!

---

## 📝 Summary

✅ **Clean Separation**: Base stats vs rarity vs bonuses  
✅ **Flexible**: Multipliers + random slots  
✅ **Scalable**: Easy to add new rarities  
✅ **Future-Proof**: Ready for crafting system  
✅ **Debug-Friendly**: Context menu helpers  

Your WeaponSO is now solid and flexible! 🎯