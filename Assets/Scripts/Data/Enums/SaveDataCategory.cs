// ════════════════════════════════════════════════════════════════════════
// Assets/Scripts/Data/Enums/SaveDataCategory.cs
// Categorizes what type of data has changed (for dirty flag system)
// ════════════════════════════════════════════════════════════════════════

using System;

namespace Ascension.Data.Enums
{
    /// <summary>
    /// Flags for tracking which categories of save data have changed
    /// Use bitwise operations to track multiple categories at once
    /// Example: dirtyFlags |= SaveDataCategory.Character | SaveDataCategory.Inventory;
    /// </summary>
    [Flags]
    public enum SaveDataCategory
    {
        None = 0,
        
        /// <summary>Character stats, level, HP, attributes</summary>
        Character = 1 << 0,
        
        /// <summary>Items, gold, currency</summary>
        Inventory = 1 << 1,
        
        /// <summary>Equipped gear slots</summary>
        Equipment = 1 << 2,
        
        /// <summary>Skill loadout and unlocked abilities</summary>
        Skills = 1 << 3,
        
        /// <summary>Quest progress, completed objectives</summary>
        Quest = 1 << 4,
        
        /// <summary>Room state, unlocks, discovered areas</summary>
        World = 1 << 5,
        
        /// <summary>Mark all categories as dirty (force full save)</summary>
        All = Character | Inventory | Equipment | Skills | Quest | World
    }
}