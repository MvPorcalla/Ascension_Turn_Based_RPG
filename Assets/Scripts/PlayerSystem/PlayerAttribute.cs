// -------------------------------
// PlayerAttributes.cs
// Pure data class for character attributes
// -------------------------------

using System;

[Serializable]
public class PlayerAttributes
{
    public int STR;
    public int INT;
    public int AGI;
    public int END;
    public int WIS;
    
    public PlayerAttributes() { }
    
    public PlayerAttributes(int str, int intelligence, int agi, int end, int wis)
    {
        STR = str;
        INT = intelligence;
        AGI = agi;
        END = end;
        WIS = wis;
    }
    
    public PlayerAttributes Clone()
    {
        return new PlayerAttributes(STR, INT, AGI, END, WIS);
    }
    
    public void CopyFrom(PlayerAttributes other)
    {
        STR = other.STR;
        INT = other.INT;
        AGI = other.AGI;
        END = other.END;
        WIS = other.WIS;
    }
}