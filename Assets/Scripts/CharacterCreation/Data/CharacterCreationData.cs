// ════════════════════════════════════════════════════════════════════════
// Assets\Scripts\CharacterCreation\Data\CharacterCreationData.cs
// ✅ Data container with lifecycle helpers
// Stores temporary character creation state + initialization logic
// ════════════════════════════════════════════════════════════════════════

using System;
using Ascension.Character.Core;

namespace Ascension.CharacterCreation.Data
{
    /// <summary>
    /// Holds temporary character creation state
    /// Includes lifecycle helpers for initialization and reset
    /// Destroyed after character is created
    /// </summary>
    [Serializable]
    public class CharacterCreationData
    {
        #region Configuration
        public int totalPointsToAllocate = 50;
        #endregion

        #region State
        public CharacterAttributes currentAttributes;
        public CharacterDerivedStats previewStats;
        public int pointsSpent = 0;
        public string characterName = string.Empty;
        #endregion

        #region Properties
        public int PointsRemaining => totalPointsToAllocate - pointsSpent;
        public bool HasPointsToSpend => pointsSpent < totalPointsToAllocate;
        public bool CanReduce => pointsSpent > 0;
        public bool AllPointsAllocated => pointsSpent >= totalPointsToAllocate;
        #endregion

        #region Initialization
        public CharacterCreationData()
        {
            currentAttributes = new CharacterAttributes();
            previewStats = new CharacterDerivedStats();
        }
        #endregion
    }
}