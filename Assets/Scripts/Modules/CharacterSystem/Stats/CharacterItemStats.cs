// ════════════════════════════════════════════
// CharacterItemStats.cs
// Holds item-based bonuses and provides flexible stat access
// ════════════════════════════════════════════

using System;
using System.Collections.Generic;
using UnityEngine;
using Ascension.Data.Enums;

namespace Ascension.Character.Stat
{
    [Serializable]
    public class CharacterItemStats
    {
        // ──────────────────────────────────────────────
        // Serialized Fields
        // ──────────────────────────────────────────────

        [SerializeField] private float ad;
        [SerializeField] private float ap;
        [SerializeField] private float hp;
        [SerializeField] private float defense;
        [SerializeField] private float attackSpeed;
        [SerializeField] private float critRate;
        [SerializeField] private float critDamage;
        [SerializeField] private float evasion;
        [SerializeField] private float tenacity;
        [SerializeField] private float lethality;
        [SerializeField] private float penetration;
        [SerializeField] private float lifesteal;

        // ──────────────────────────────────────────────
        // Private Fields
        // ──────────────────────────────────────────────

        private Dictionary<BonusStatType, float> statsCache;

        // ──────────────────────────────────────────────
        // Constructors
        // ──────────────────────────────────────────────

        public CharacterItemStats()
        {
            BuildCache();
        }

        // ──────────────────────────────────────────────
        // Public Methods
        // ──────────────────────────────────────────────

        public void AddStat(BonusStatType type, float value)
        {
            switch (type)
            {
                case BonusStatType.AttackDamage: ad += value; break;
                case BonusStatType.AbilityPower: ap += value; break;
                case BonusStatType.Health: hp += value; break;
                case BonusStatType.Defense: defense += value; break;
                case BonusStatType.AttackSpeed: attackSpeed += value; break;
                case BonusStatType.CritRate: critRate += value; break;
                case BonusStatType.CritDamage: critDamage += value; break;
                case BonusStatType.Evasion: evasion += value; break;
                case BonusStatType.Tenacity: tenacity += value; break;
                case BonusStatType.Lethality: lethality += value; break;
                case BonusStatType.Penetration: penetration += value; break;
                case BonusStatType.Lifesteal: lifesteal += value; break;
            }
        }

        public float GetStat(BonusStatType type)
        {
            return type switch
            {
                BonusStatType.AttackDamage => ad,
                BonusStatType.AbilityPower => ap,
                BonusStatType.Health => hp,
                BonusStatType.Defense => defense,
                BonusStatType.AttackSpeed => attackSpeed,
                BonusStatType.CritRate => critRate,
                BonusStatType.CritDamage => critDamage,
                BonusStatType.Evasion => evasion,
                BonusStatType.Tenacity => tenacity,
                BonusStatType.Lethality => lethality,
                BonusStatType.Penetration => penetration,
                BonusStatType.Lifesteal => lifesteal,
                _ => 0f
            };
        }

        public Dictionary<BonusStatType, float> GetAllStats()
        {
            BuildCache();
            return new Dictionary<BonusStatType, float>(statsCache);
        }

        public CharacterItemStats Clone()
        {
            return new CharacterItemStats
            {
                ad = this.ad,
                ap = this.ap,
                hp = this.hp,
                defense = this.defense,
                attackSpeed = this.attackSpeed,
                critRate = this.critRate,
                critDamage = this.critDamage,
                evasion = this.evasion,
                tenacity = this.tenacity,
                lethality = this.lethality,
                penetration = this.penetration,
                lifesteal = this.lifesteal
            };
        }

        public void Reset()
        {
            ad = ap = hp = defense = attackSpeed = 0;
            critRate = critDamage = evasion = tenacity = 0;
            lethality = penetration = lifesteal = 0;
        }

        // ──────────────────────────────────────────────
        // Properties (Direct access)
        // ──────────────────────────────────────────────

        public float AD => ad;
        public float AP => ap;
        public float HP => hp;
        public float Defense => defense;
        public float AttackSpeed => attackSpeed;
        public float CritRate => critRate;
        public float CritDamage => critDamage;
        public float Evasion => evasion;
        public float Tenacity => tenacity;
        public float Lethality => lethality;
        public float Penetration => penetration;
        public float Lifesteal => lifesteal;

        // ──────────────────────────────────────────────
        // Private Methods
        // ──────────────────────────────────────────────

        private void BuildCache()
        {
            statsCache = new Dictionary<BonusStatType, float>
            {
                { BonusStatType.AttackDamage, ad },
                { BonusStatType.AbilityPower, ap },
                { BonusStatType.Health, hp },
                { BonusStatType.Defense, defense },
                { BonusStatType.AttackSpeed, attackSpeed },
                { BonusStatType.CritRate, critRate },
                { BonusStatType.CritDamage, critDamage },
                { BonusStatType.Evasion, evasion },
                { BonusStatType.Tenacity, tenacity },
                { BonusStatType.Lethality, lethality },
                { BonusStatType.Penetration, penetration },
                { BonusStatType.Lifesteal, lifesteal }
            };
        }
    }
}
