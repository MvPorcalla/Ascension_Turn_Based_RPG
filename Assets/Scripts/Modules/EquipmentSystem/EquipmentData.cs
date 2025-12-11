// ════════════════════════════════════════════
// EquipmentData.cs
// Serializable data for saving equipped items
// ════════════════════════════════════════════

using System;

[Serializable]
public class EquipmentData
{
    // Equipment slots
    public string equippedWeaponID;
    public string equippedHelmetID;
    public string equippedChestPlateID;
    public string equippedGlovesID;
    public string equippedBootsID;
    public string equippedAccessory1ID;
    public string equippedAccessory2ID;
    
    // Skills
    public string normalSkill1ID;
    public string normalSkill2ID;
    public string ultimateSkillID;
    
    // Hotbar
    public string hotbarItem1ID;
    public string hotbarItem2ID;
    public string hotbarItem3ID;
}