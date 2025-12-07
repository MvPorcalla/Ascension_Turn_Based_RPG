// ──────────────────────────────────────────────────
// CharacterAttribute.cs
// Holds base player attributes (STR, INT, etc.)
// ──────────────────────────────────────────────────

using System;

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
}