// ──────────────────────────────────────────────────
// GearInfoPopUp.cs
// Reusable popup for displaying item info with equip/unequip
// ──────────────────────────────────────────────────

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class GearInfoPopUp : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject popupContainer;
    [SerializeField] private TMP_Text itemNameText;
    [SerializeField] private Image itemImage;
    [SerializeField] private Transform statPanelContent;
    [SerializeField] private Transform effectPanelContent;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private Button closeButton;
    [SerializeField] private Button equipButton;
    [SerializeField] private TMP_Text equipButtonLabel;
    
    [Header("Prefabs")]
    [SerializeField] private GameObject itemBonusStatPrefab;
    [SerializeField] private GameObject itemEffectPrefab;
    
    [Header("State")]
    private ItemBaseSO currentItem;
    private bool isCurrentlyEquipped;
    private Action<ItemBaseSO, bool> onEquipCallback;

    private void Awake()
    {
        if (closeButton != null)
            closeButton.onClick.AddListener(Hide);
        
        if (equipButton != null)
            equipButton.onClick.AddListener(OnEquipButtonClicked);
        
        Hide();
    }

    public void Show(ItemBaseSO item, bool isEquipped, Action<ItemBaseSO, bool> equipCallback)
    {
        currentItem = item;
        isCurrentlyEquipped = isEquipped;
        onEquipCallback = equipCallback;
        
        // Update UI
        UpdateDisplay();
        
        // Show popup
        if (popupContainer != null)
            popupContainer.SetActive(true);
    }

    public void Hide()
    {
        if (popupContainer != null)
            popupContainer.SetActive(false);
        
        currentItem = null;
        onEquipCallback = null;
    }

    private void UpdateDisplay()
    {
        if (currentItem == null) return;
        
        // Item name
        if (itemNameText != null)
            itemNameText.text = currentItem.itemName;
        
        // Item image
        if (itemImage != null)
            itemImage.sprite = currentItem.icon;
        
        // Clear previous stats/effects
        ClearPanel(statPanelContent);
        ClearPanel(effectPanelContent);
        
        // Display based on item type
        if (currentItem is WeaponSO weapon)
        {
            DisplayWeaponStats(weapon);
        }
        else if (currentItem is GearSO gear)
        {
            DisplayGearStats(gear);
        }
        else if (currentItem is SkillSO skill)
        {
            DisplaySkillInfo(skill);
        }
        else if (currentItem is PotionSO potion)
        {
            DisplayPotionInfo(potion);
        }
        
        // Description
        if (descriptionText != null)
            descriptionText.text = currentItem.description;
        
        // Equip button
        if (equipButtonLabel != null)
            equipButtonLabel.text = isCurrentlyEquipped ? "UNEQUIP" : "EQUIP";
    }

    private void DisplayWeaponStats(WeaponSO weapon)
    {
        // Base stats - use bonusAD/bonusAP properties directly (already includes rarity multiplier)
        if (weapon.bonusAD > 0)
            AddStat("Attack Damage", weapon.bonusAD.ToString("F1"));
        
        if (weapon.bonusAP > 0)
            AddStat("Ability Power", weapon.bonusAP.ToString("F1"));
        
        if (weapon.bonusHP > 0)
            AddStat("Health", "+" + weapon.bonusHP.ToString("F0"));
        
        if (weapon.bonusDefense > 0)
            AddStat("Defense", "+" + weapon.bonusDefense.ToString("F1"));
        
        if (weapon.bonusAttackSpeed > 0)
            AddStat("Attack Speed", weapon.bonusAttackSpeed.ToString("F2"));
        
        if (weapon.bonusCritRate > 0)
            AddStat("Crit Rate", weapon.bonusCritRate.ToString("F1") + "%");
        
        if (weapon.bonusCritDamage > 0)
            AddStat("Crit Damage", weapon.bonusCritDamage.ToString("F1") + "%");
        
        if (weapon.bonusEvasion > 0)
            AddStat("Evasion", weapon.bonusEvasion.ToString("F1") + "%");
        
        if (weapon.bonusTenacity > 0)
            AddStat("Tenacity", weapon.bonusTenacity.ToString("F1") + "%");
        
        if (weapon.bonusLethality > 0)
            AddStat("Lethality", weapon.bonusLethality.ToString("F0"));
        
        if (weapon.bonusPenetration > 0)
            AddStat("Penetration", weapon.bonusPenetration.ToString("F1") + "%");
        
        if (weapon.bonusLifesteal > 0)
            AddStat("Lifesteal", weapon.bonusLifesteal.ToString("F1") + "%");
        
        // Weapon type info
        AddStat("Type", weapon.weaponType.ToString());
        AddStat("Range", weapon.attackRangeType.ToString());
        
        // Rolled bonus stats (if any)
        if (weapon.bonusStats != null && weapon.bonusStats.Count > 0)
        {
            AddEffect("<b>Bonus Stats:</b>");
            foreach (var bonus in weapon.bonusStats)
            {
                AddEffect(bonus.GetDisplayText());
            }
        }
        
        // Default weapon skill
        if (weapon.defaultWeaponSkill != null)
        {
            AddEffect($"<color=#ffd93d>⚔ {weapon.defaultWeaponSkill.skillName}</color>");
        }
    }

    private void DisplayGearStats(GearSO gear)
    {
        // Base stats using your GearSO properties
        if (gear.bonusHP > 0)
            AddStat("Health", "+" + gear.bonusHP.ToString("F0"));
        
        if (gear.bonusDefense > 0)
            AddStat("Defense", "+" + gear.bonusDefense.ToString("F1"));
        
        if (gear.bonusAD > 0)
            AddStat("Attack Damage", "+" + gear.bonusAD.ToString("F1"));
        
        if (gear.bonusAP > 0)
            AddStat("Ability Power", "+" + gear.bonusAP.ToString("F1"));
        
        if (gear.bonusAttackSpeed > 0)
            AddStat("Attack Speed", "+" + gear.bonusAttackSpeed.ToString("F2"));
        
        if (gear.bonusCritRate > 0)
            AddStat("Crit Rate", "+" + gear.bonusCritRate.ToString("F1") + "%");
        
        if (gear.bonusCritDamage > 0)
            AddStat("Crit Damage", "+" + gear.bonusCritDamage.ToString("F1") + "%");
        
        if (gear.bonusEvasion > 0)
            AddStat("Evasion", "+" + gear.bonusEvasion.ToString("F1") + "%");
        
        if (gear.bonusTenacity > 0)
            AddStat("Tenacity", "+" + gear.bonusTenacity.ToString("F1") + "%");
        
        if (gear.bonusLethality > 0)
            AddStat("Lethality", "+" + gear.bonusLethality.ToString("F0"));
        
        if (gear.bonusPenetration > 0)
            AddStat("Penetration", "+" + gear.bonusPenetration.ToString("F1") + "%");
        
        if (gear.bonusLifesteal > 0)
            AddStat("Lifesteal", "+" + gear.bonusLifesteal.ToString("F1") + "%");
        
        // Bonus stats from rolls
        if (gear.bonusStats != null && gear.bonusStats.Count > 0)
        {
            foreach (var bonus in gear.bonusStats)
            {
                AddEffect("Bonus: " + bonus.GetDisplayText());
            }
        }
    }

    private void DisplaySkillInfo(SkillSO skill)
    {
        // Skill stats
        AddStat("Category", skill.category.ToString());
        AddStat("Damage Type", skill.damageType.ToString());
        AddStat("Base Damage", skill.baseDamage.ToString("F0"));
        
        if (skill.adRatio > 0)
            AddStat("AD Ratio", (skill.adRatio * 100).ToString("F0") + "%");
        
        if (skill.apRatio > 0)
            AddStat("AP Ratio", (skill.apRatio * 100).ToString("F0") + "%");
        
        AddStat("Target", skill.targetType.ToString());
        
        if (skill.targetType == TargetType.Multi)
            AddStat("Max Targets", skill.maxTargets.ToString());
        
        if (skill.turnCooldown > 0)
            AddStat("Cooldown", skill.turnCooldown + " turns");
        
        AddStat("Can Crit", skill.canCrit ? "Yes" : "No");
        
        // Weapon requirements
        if (skill.compatibleWeaponTypes != null && skill.compatibleWeaponTypes.Length > 0)
        {
            string weapons = string.Join(", ", skill.compatibleWeaponTypes);
            AddEffect("Requires: " + weapons);
        }
    }
    
    private void DisplayPotionInfo(PotionSO potion)
    {
        // Potion type
        AddStat("Type", potion.potionType.ToString());
        
        // Restore effects
        if (potion.healthRestore > 0)
        {
            string restoreText = potion.restoreType == RestoreType.Percentage 
                ? $"{potion.healthRestorePercent}% HP" 
                : $"{potion.healthRestoreFlat} HP";
            
            if (potion.durationType == DurationType.Instant)
            {
                AddStat("Restores", restoreText);
            }
            else if (potion.durationType == DurationType.TurnBased)
            {
                AddStat("Restores", $"{restoreText} over {potion.TurnDuration} turns");
            }
            else
            {
                AddStat("Restores", $"{restoreText} over {potion.restoreDuration:F1}s");
            }
        }
        
        if (potion.manaRestore > 0)
        {
            string restoreText = potion.restoreType == RestoreType.Percentage 
                ? $"{potion.manaRestorePercent}% Mana" 
                : $"{potion.manaRestoreFlat} Mana";
            AddStat("Mana", restoreText);
        }
        
        // Buffs
        if (potion.buffs != null && potion.buffs.Count > 0)
        {
            foreach (var buff in potion.buffs)
            {
                AddEffect(buff.GetDescription());
            }
        }
        
        // Usage restrictions
        if (!potion.canUseInCombat)
            AddEffect("Cannot use in combat");
        if (!potion.canUseOutOfCombat)
            AddEffect("Combat only");
    }

    private void AddStat(string label, string value)
    {
        if (itemBonusStatPrefab == null || statPanelContent == null) return;
        
        GameObject statObj = Instantiate(itemBonusStatPrefab, statPanelContent);
        
        TMP_Text labelText = statObj.transform.Find("Text_Label")?.GetComponent<TMP_Text>();
        TMP_Text valueText = statObj.transform.Find("Text_value")?.GetComponent<TMP_Text>();
        
        if (labelText != null) labelText.text = label + ":";
        if (valueText != null) valueText.text = value;
    }

    private void AddEffect(string effectText)
    {
        if (itemEffectPrefab == null || effectPanelContent == null) return;
        
        GameObject effectObj = Instantiate(itemEffectPrefab, effectPanelContent);
        
        TMP_Text text = effectObj.transform.Find("Text")?.GetComponent<TMP_Text>();
        if (text != null) text.text = "• " + effectText;
    }

    private void ClearPanel(Transform panel)
    {
        if (panel == null) return;
        
        foreach (Transform child in panel)
        {
            Destroy(child.gameObject);
        }
    }

    private void OnEquipButtonClicked()
    {
        onEquipCallback?.Invoke(currentItem, isCurrentlyEquipped);
    }

    private string GetStatName(BonusStatType statType)
    {
        switch (statType)
        {
            case BonusStatType.AttackDamage: return "Attack Damage";
            case BonusStatType.AbilityPower: return "Ability Power";
            case BonusStatType.Health: return "Health";
            case BonusStatType.Defense: return "Defense";
            case BonusStatType.AttackSpeed: return "Attack Speed";
            case BonusStatType.CritRate: return "Crit Rate";
            case BonusStatType.CritDamage: return "Crit Damage";
            case BonusStatType.Evasion: return "Evasion";
            case BonusStatType.Tenacity: return "Tenacity";
            case BonusStatType.Lethality: return "Lethality";
            case BonusStatType.Penetration: return "Penetration";
            case BonusStatType.Lifesteal: return "Lifesteal";
            default: return statType.ToString();
        }
    }

    private string FormatStatValue(WeaponBonusStat bonus)
    {
        switch (bonus.statType)
        {
            case BonusStatType.AttackDamage:
            case BonusStatType.AbilityPower:
            case BonusStatType.Defense:
            case BonusStatType.AttackSpeed:
                return "+" + bonus.value.ToString("F1");
            
            case BonusStatType.Health:
            case BonusStatType.Lethality:
                return "+" + bonus.value.ToString("F0");
            
            case BonusStatType.CritRate:
            case BonusStatType.CritDamage:
            case BonusStatType.Evasion:
            case BonusStatType.Tenacity:
            case BonusStatType.Penetration:
            case BonusStatType.Lifesteal:
                return "+" + bonus.value.ToString("F1") + "%";
            
            default:
                return bonus.value.ToString("F1");
        }
    }
}