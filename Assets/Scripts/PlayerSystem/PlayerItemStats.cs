// ──────────────────────────────────────────────────
// PlayerItemStats.cs
// Holds item-based stat bonuses for the player
// ──────────────────────────────────────────────────

using System;

[Serializable]
public class PlayerItemStats
{
    public float AD;
    public float AP;
    public float HP;
    public float Defense;
    public float AttackSpeed;
    public float CritRate;
    public float CritDamage;
    public float Evasion;
    public float Tenacity;
    public float Lethality;
    public float Penetration;
    public float Lifesteal;
    
    public PlayerItemStats() { }
    
    public PlayerItemStats Clone()
    {
        return new PlayerItemStats
        {
            AD = this.AD,
            AP = this.AP,
            HP = this.HP,
            Defense = this.Defense,
            AttackSpeed = this.AttackSpeed,
            CritRate = this.CritRate,
            CritDamage = this.CritDamage,
            Evasion = this.Evasion,
            Tenacity = this.Tenacity,
            Lethality = this.Lethality,
            Penetration = this.Penetration,
            Lifesteal = this.Lifesteal
        };
    }
    
    public void Reset()
    {
        AD = 0;
        AP = 0;
        HP = 0;
        Defense = 0;
        AttackSpeed = 0;
        CritRate = 0;
        CritDamage = 0;
        Evasion = 0;
        Tenacity = 0;
        Lethality = 0;
        Penetration = 0;
        Lifesteal = 0;
    }
}