// ══════════════════════════════════════════════════
// CharacterAttributes.cs
// Holds base player attributes (STR, INT, etc.)
// ══════════════════════════════════════════════════

using System;
using Ascension.Character.Stat;

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
    
    // ✅ NEW: Get attribute by type (compile-time safe)
    public int GetAttribute(AttributeType type)
    {
        switch (type)
        {
            case AttributeType.STR: return STR;
            case AttributeType.INT: return INT;
            case AttributeType.AGI: return AGI;
            case AttributeType.END: return END;
            case AttributeType.WIS: return WIS;
            default: throw new ArgumentException($"Invalid attribute type: {type}");
        }
    }
    
    // ✅ NEW: Set attribute by type (compile-time safe)
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