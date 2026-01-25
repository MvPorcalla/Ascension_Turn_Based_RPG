// ════════════════════════════════════════════════════════════════════════
// Assets/Scripts/Modules/PotionSystem/Data/ActiveBuff.cs
// Data class for active potion buffs
// ════════════════════════════════════════════════════════════════════════

using System;
using Ascension.Data.SO.Item;

namespace Ascension.PotionSystem.Data
{
    [Serializable]
    public class ActiveBuff
    {
        public BuffType buffType;
        public float value;
        public float duration;
        public float remainingTime;
        public DurationType durationType = DurationType.RealTime;
    }
}