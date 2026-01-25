// ══════════════════════════════════════════════════
// CharacterAttributes.cs
// Holds base player attributes (STR, INT, etc.)
// ══════════════════════════════════════════════════

using System;

namespace Ascension.Character.Core
{
    [Serializable]
    public class CharacterAttributes
    {
        public int STR;
        public int INT;
        public int AGI;
        public int END;
        public int WIS;
        
        public CharacterAttributes() { }
        
        public CharacterAttributes(int str, int intelligence, int agi, int end, int wis)
        {
            STR = str;
            INT = intelligence;
            AGI = agi;
            END = end;
            WIS = wis;
        }
        
        public CharacterAttributes Clone()
        {
            return new CharacterAttributes(STR, INT, AGI, END, WIS);
        }
        
        public void CopyFrom(CharacterAttributes other)
        {
            STR = other.STR;
            INT = other.INT;
            AGI = other.AGI;
            END = other.END;
            WIS = other.WIS;
        }
        
        /// <summary>
        /// Get attribute by type (compile-time safe)
        /// </summary>
        public int GetAttribute(AttributeType type)
        {
            return type switch
            {
                AttributeType.STR => STR,
                AttributeType.INT => INT,
                AttributeType.AGI => AGI,
                AttributeType.END => END,
                AttributeType.WIS => WIS,
                _ => throw new ArgumentException($"Invalid attribute type: {type}")
            };
        }
        
        /// <summary>
        /// Set attribute by type (compile-time safe)
        /// </summary>
        public void SetAttribute(AttributeType type, int value)
        {
            switch (type)
            {
                case AttributeType.STR: STR = value; break;
                case AttributeType.INT: INT = value; break;
                case AttributeType.AGI: AGI = value; break;
                case AttributeType.END: END = value; break;
                case AttributeType.WIS: WIS = value; break;
                default: throw new ArgumentException($"Invalid attribute type: {type}");
            }
        }
    }
    
    // ════════════════════════════════════════════════════════════════
    // AttributeType Enum (moved here from Character.Stat namespace)
    // ════════════════════════════════════════════════════════════════
    
    public enum AttributeType
    {
        STR,
        INT,
        AGI,
        END,
        WIS
    }
}