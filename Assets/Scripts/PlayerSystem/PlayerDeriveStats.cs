// -------------------------------
// PlayerDerivedStats.cs - CORRECTED
// -------------------------------

using System;
using UnityEngine;

[Serializable]
public class PlayerDerivedStats
{
    // Final calculated stats
    public float AD;
    public float AP;
    public float MaxHP;
    public float Defense;
    public float AttackSpeed;
    public float CritRate;
    public float CritDamage;
    public float Evasion;
    public float Tenacity;
    public float Lethality;
    public float Penetration;
    public float Lifesteal;
    
    // Cache flag to avoid unnecessary recalculations
    [NonSerialized] private bool isDirty = true;
    
    public void MarkDirty() => isDirty = true;
    public bool IsDirty() => isDirty;
    
    /// <summary>
    /// Calculate all derived stats with weapon support and soft caps
    /// </summary>
    public void Calculate(
        CharacterBaseStatsSO baseStats,
        int level,
        PlayerAttributes attributes,
        PlayerItemStats itemStats,
        WeaponSO equippedWeapon = null)
    {
        if (!isDirty) return;
        
        int levelBonus = level - 1;
        
        // Apply soft caps to attributes
        float effectiveSTR = ApplySoftCap(attributes.STR, baseStats.attributeSoftCap, baseStats.postSoftCapEfficiency);
        float effectiveINT = ApplySoftCap(attributes.INT, baseStats.attributeSoftCap, baseStats.postSoftCapEfficiency);
        float effectiveAGI = ApplySoftCap(attributes.AGI, baseStats.attributeSoftCap, baseStats.postSoftCapEfficiency);
        float effectiveEND = ApplySoftCap(attributes.END, baseStats.attributeSoftCap, baseStats.postSoftCapEfficiency);
        float effectiveWIS = ApplySoftCap(attributes.WIS, baseStats.attributeSoftCap, baseStats.postSoftCapEfficiency);
        
        // === ATTACK DAMAGE (Weapon-Based) ===
        float baseAD = baseStats.BaseAD;
        float weaponAD = equippedWeapon != null ? equippedWeapon.bonusAD : 0f;
        float weaponScaling = weaponAD * (effectiveSTR * baseStats.weaponSTRScaling);
        float attributeAD = effectiveSTR * baseStats.STRtoAD;
        
        AD = baseAD + weaponAD + weaponScaling + attributeAD + itemStats.AD;
        
        // === ABILITY POWER (Weapon-Based) ===
        float baseAP = baseStats.BaseAP;
        float weaponAP = equippedWeapon != null ? equippedWeapon.bonusAP : 0f;
        float weaponAPScaling = weaponAP * (effectiveINT * baseStats.weaponINTScaling);
        float attributeAP = effectiveINT * baseStats.INTtoAP;
        
        AP = baseAP + weaponAP + weaponAPScaling + attributeAP + itemStats.AP;
        
        // === HEALTH (Flat from attributes) ===
        float levelHP = baseStats.HPPerLevel * levelBonus;
        float attributeHP = effectiveEND * baseStats.ENDtoHP;
        float weaponHP = equippedWeapon != null ? equippedWeapon.bonusHP : 0f;
        
        MaxHP = baseStats.BaseHP + levelHP + attributeHP + weaponHP + itemStats.HP;
        
        // === DEFENSE ===
        float levelDefense = baseStats.DefensePerLevel * levelBonus;
        float weaponDefense = equippedWeapon != null ? equippedWeapon.bonusDefense : 0f;
        
        Defense = baseStats.BaseDefense + levelDefense +
                  (effectiveEND * baseStats.ENDtoDefense) +
                  (effectiveWIS * baseStats.WIStoDefense) +
                  weaponDefense +
                  itemStats.Defense;
        
        // === ATTACK SPEED (Turn Order) ===
        AttackSpeed = baseStats.BaseAttackSpeed + 
                      (effectiveAGI * baseStats.AGItoAttackSpeed) + 
                      itemStats.AttackSpeed;
        
        // === CRITICAL RATE (Tiered Cap) ===
        float levelEvasion = baseStats.EvasionPerLevel * levelBonus;
        float baseCritRate = baseStats.BaseCritRate + (effectiveAGI * baseStats.AGItoCritRate);
        
        // Add weapon crit rate (before base cap)
        if (equippedWeapon != null)
            baseCritRate += equippedWeapon.bonusCritRate;
        
        baseCritRate = Mathf.Min(baseCritRate, baseStats.baseCritRateCap);
        
        CritRate = baseCritRate + itemStats.CritRate;
        CritRate = Mathf.Min(CritRate, baseStats.totalCritRateCap);
        
        // === CRITICAL DAMAGE ===
        float weaponCritDamage = equippedWeapon != null ? equippedWeapon.bonusCritDamage : 0f;
        CritDamage = baseStats.BaseCritDamage + weaponCritDamage + itemStats.CritDamage;
        
        // === EVASION (Tiered Cap) ===
        float baseEvasion = baseStats.BaseEvasion + levelEvasion + (effectiveAGI * baseStats.AGItoEvasion);
        
        // Add weapon evasion (before base cap)
        if (equippedWeapon != null)
            baseEvasion += equippedWeapon.bonusEvasion;
        
        baseEvasion = Mathf.Min(baseEvasion, baseStats.baseEvasionCap);
        
        Evasion = baseEvasion + itemStats.Evasion;
        Evasion = Mathf.Min(Evasion, baseStats.totalEvasionCap);

        // === TENACITY (Tiered Cap) ===
        float levelTenacity = baseStats.TenacityPerLevel * levelBonus;
        float baseTenacity = baseStats.BaseTenacity + levelTenacity + (effectiveWIS * baseStats.WIStoTenacity);

        // Add weapon tenacity (before base cap)
        if (equippedWeapon != null)
            baseTenacity += equippedWeapon.bonusTenacity;

        // Apply base cap
        baseTenacity = Mathf.Min(baseTenacity, baseStats.baseTenacityCap);

        // Add item tenacity and apply total cap
        Tenacity = baseTenacity + itemStats.Tenacity;
        Tenacity = Mathf.Min(Tenacity, baseStats.totalTenacityCap);
        
        // === ITEM-ONLY STATS (Weapon can also provide these) ===
        float weaponLethality = equippedWeapon != null ? equippedWeapon.bonusLethality : 0f;
        float weaponPenetration = equippedWeapon != null ? equippedWeapon.bonusPenetration : 0f;
        float weaponLifesteal = equippedWeapon != null ? equippedWeapon.bonusLifesteal : 0f;
        
        Lethality = weaponLethality + itemStats.Lethality;
        Penetration = Mathf.Min(weaponPenetration + itemStats.Penetration, baseStats.maxPenetration);
        Lifesteal = weaponLifesteal + itemStats.Lifesteal;
        
        isDirty = false;
    }
    
    /// <summary>
    /// Apply soft cap to attribute
    /// </summary>
    private float ApplySoftCap(int stat, int softCap, float postCapEfficiency)
    {
        if (stat <= softCap) return stat;
        
        float basePortion = softCap;
        float excessPortion = (stat - softCap) * postCapEfficiency;
        return basePortion + excessPortion;
    }
    
    /// <summary>
    /// Force recalculation and return self (for chaining)
    /// </summary>
    public PlayerDerivedStats Recalculate(
        CharacterBaseStatsSO baseStats,
        int level,
        PlayerAttributes attributes,
        PlayerItemStats itemStats,
        WeaponSO equippedWeapon = null)
    {
        isDirty = true;
        Calculate(baseStats, level, attributes, itemStats, equippedWeapon);
        return this;
    }
}