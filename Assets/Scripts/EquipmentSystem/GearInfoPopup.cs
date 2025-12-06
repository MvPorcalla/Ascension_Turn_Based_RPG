// GearInfoPopup.cs
// Popup UI for displaying detailed gear or weapon information

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using Ascension.Data.SO;

public class GearInfoPopup : MonoBehaviour
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
        closeButton?.onClick.AddListener(Hide);
        equipButton?.onClick.AddListener(OnEquipButtonClicked);
        Hide();
    }

    public void Show(ItemBaseSO item, bool isEquipped, Action<ItemBaseSO, bool> equipCallback)
    {
        currentItem = item;
        isCurrentlyEquipped = isEquipped;
        onEquipCallback = equipCallback;

        UpdateDisplay();
        popupContainer?.SetActive(true);
    }

    public void Hide()
    {
        popupContainer?.SetActive(false);
        currentItem = null;
        onEquipCallback = null;
    }

    private void UpdateDisplay()
    {
        if (currentItem == null) return;

        itemNameText.text = currentItem.ItemName;
        if (itemImage != null) itemImage.sprite = currentItem.Icon;

        ClearPanel(statPanelContent);
        ClearPanel(effectPanelContent);

        if (currentItem is WeaponSO weapon) DisplayWeaponStats(weapon);
        else if (currentItem is GearSO gear) DisplayGearStats(gear);

        descriptionText.text = currentItem.Description;
        equipButtonLabel.text = isCurrentlyEquipped ? "UNEQUIP" : "EQUIP";
    }

    private void DisplayWeaponStats(WeaponSO weapon)
    {
        if (weapon.BonusAD > 0) AddStat("Attack Damage", $"+{weapon.BonusAD}");
        if (weapon.BonusAP > 0) AddStat("Ability Power", $"+{weapon.BonusAP}");
        if (weapon.BonusHP > 0) AddStat("Health", $"+{weapon.BonusHP}");
        if (weapon.BonusDefense > 0) AddStat("Defense", $"+{weapon.BonusDefense}");
        if (weapon.BonusAttackSpeed > 0) AddStat("Attack Speed", $"+{weapon.BonusAttackSpeed}");
        if (weapon.BonusCritRate > 0) AddStat("Crit Rate", $"+{weapon.BonusCritRate}%");
        if (weapon.BonusCritDamage > 0) AddStat("Crit Damage", $"+{weapon.BonusCritDamage}%");
        if (weapon.BonusEvasion > 0) AddStat("Evasion", $"+{weapon.BonusEvasion}%");
        if (weapon.BonusTenacity > 0) AddStat("Tenacity", $"+{weapon.BonusTenacity}%");
        if (weapon.BonusLethality > 0) AddStat("Lethality", $"+{weapon.BonusLethality}");
        if (weapon.BonusPenetration > 0) AddStat("Penetration", $"+{weapon.BonusPenetration}%");
        if (weapon.BonusLifesteal > 0) AddStat("Lifesteal", $"+{weapon.BonusLifesteal}%");

        AddStat("Type", weapon.WeaponType.ToString());
        AddStat("Range", weapon.AttackRangeType.ToString());

        if (weapon.BonusStats != null && weapon.BonusStats.Count > 0)
        {
            AddEffect("<b>Bonus Stats:</b>");
            foreach (var bonus in weapon.BonusStats) AddEffect(bonus.GetDisplayText());
        }

        if (weapon.DefaultWeaponSkill != null)
        {
            AddEffect($"[Ability] : {weapon.DefaultWeaponSkill.AbilityName}");
        }
    }

    private void DisplayGearStats(GearSO gear)
    {
        if (gear.BonusHP > 0) AddStat("Health", "+" + gear.BonusHP);
        if (gear.BonusDefense > 0) AddStat("Defense", "+" + gear.BonusDefense);
        if (gear.BonusAD > 0) AddStat("Attack Damage", "+" + gear.BonusAD);
        if (gear.BonusAP > 0) AddStat("Ability Power", "+" + gear.BonusAP);
        if (gear.BonusAttackSpeed > 0) AddStat("Attack Speed", "+" + gear.BonusAttackSpeed);
        if (gear.BonusCritRate > 0) AddStat("Crit Rate", "+" + gear.BonusCritRate + "%");
        if (gear.BonusCritDamage > 0) AddStat("Crit Damage", "+" + gear.BonusCritDamage + "%");
        if (gear.BonusEvasion > 0) AddStat("Evasion", "+" + gear.BonusEvasion + "%");
        if (gear.BonusTenacity > 0) AddStat("Tenacity", "+" + gear.BonusTenacity + "%");
        if (gear.BonusLethality > 0) AddStat("Lethality", "+" + gear.BonusLethality);
        if (gear.BonusPenetration > 0) AddStat("Penetration", "+" + gear.BonusPenetration + "%");
        if (gear.BonusLifesteal > 0) AddStat("Lifesteal", "+" + gear.BonusLifesteal + "%");

        if (gear.BonusStats != null && gear.BonusStats.Count > 0)
        {
            foreach (var bonus in gear.BonusStats) AddEffect("Bonus: " + bonus.GetDisplayText());
        }
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
        foreach (Transform child in panel) Destroy(child.gameObject);
    }

    private void OnEquipButtonClicked()
    {
        onEquipCallback?.Invoke(currentItem, isCurrentlyEquipped);
    }
}
