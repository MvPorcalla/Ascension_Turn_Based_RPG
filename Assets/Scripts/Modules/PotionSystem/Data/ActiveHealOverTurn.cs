// ════════════════════════════════════════════════════════════════════════
// Assets/Scripts/Modules/PotionSystem/Data/ActiveHealOverTurn.cs
// Data class for active heal-over-turn effects
// ════════════════════════════════════════════════════════════════════════

using System;
using Ascension.Character.Core;

namespace Ascension.PotionSystem.Data
{
    [Serializable]
    public class ActiveHealOverTurn
    {
        public string potionName;
        public CharacterStats characterStats;
        public float healPerTurn;
        public float totalHealRemaining;
        public int turnsRemaining;
    }
}