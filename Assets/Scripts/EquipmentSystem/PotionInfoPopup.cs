using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using Ascension.Data.SO;

public class EquipmentRoomPotionPopup : MonoBehaviour
{
    [Header("Popup Container")]
    [SerializeField] private GameObject popupContainer;

    [Header("Header")]
    [SerializeField] private TMP_Text potionName;
    [SerializeField] private Button closeButton;

    [Header("Potion Display")]
    [SerializeField] private Image potionIcon;
    [SerializeField] private TMP_Text potionTypeLabel;
    [SerializeField] private TMP_Text potionTypeValue;

    [Header("Buff Effects")]
    [SerializeField] private Transform buffEffectContent;
    [SerializeField] private GameObject buffTypePrefab;

    [Header("Description")]
    [SerializeField] private TMP_Text potionDescription;

    [Header("Action Buttons")]
    [SerializeField] private Button equipButton;

    private PotionSO currentPotion;
    private Action<PotionSO> onEquipToggleCallback;
    private bool isEquipped;

    private void Awake()
    {
        closeButton?.onClick.AddListener(ClosePopup);
        equipButton?.onClick.AddListener(OnEquipButtonClicked);
        popupContainer.SetActive(false);
    }

    public void ShowPotion(PotionSO potion, Action<PotionSO> equipCallback, bool equipped)
    {
        currentPotion = potion;
        onEquipToggleCallback = equipCallback;
        isEquipped = equipped;

        popupContainer.SetActive(true);

        // Header
        potionName.text = potion.ItemName;

        // Icon
        if (potionIcon != null)
        {
            if (potion.Icon != null)
            {
                potionIcon.sprite = potion.Icon;
                potionIcon.enabled = true;
            }
            else
            {
                potionIcon.sprite = null;
                potionIcon.enabled = false;
            }
        }

        // Potion Type
        if (potionTypeLabel != null) potionTypeLabel.text = "Potion Type";
        if (potionTypeValue != null)
        {
            potionTypeValue.text = potion.potionType.ToString();
        }

        // Buff Effects
        DisplayBuffEffects(potion);

        // Description
        if (potionDescription != null)
            potionDescription.text = potion.Description;

        // Equip Button Label
        UpdateEquipButton();
    }

    private void DisplayBuffEffects(PotionSO potion)
    {
        if (buffEffectContent == null || buffTypePrefab == null) return;

        // Clear existing
        foreach (Transform child in buffEffectContent)
            Destroy(child.gameObject);

        if (potion.buffs == null || potion.buffs.Count == 0) return;

        foreach (var buff in potion.buffs)
        {
            GameObject buffObj = Instantiate(buffTypePrefab, buffEffectContent);

            TMP_Text labelText = buffObj.transform.Find("TextLabel")?.GetComponent<TMP_Text>();
            TMP_Text valueText = buffObj.transform.Find("textValue")?.GetComponent<TMP_Text>();
            TMP_Text durationText = buffObj.transform.Find("textDuration")?.GetComponent<TMP_Text>();

            if (labelText != null) labelText.text = GetBuffLabel(buff.type);
            if (valueText != null) valueText.text = GetBuffValueText(buff.type, buff.value);
            if (durationText != null) durationText.text = GetBuffDurationText(buff.durationType, buff.duration);
        }
    }

    private string GetBuffLabel(BuffType buffType)
    {
        return buffType switch
        {
            BuffType.AttackDamage => "Attack Damage",
            BuffType.AbilityPower => "Ability Power",
            BuffType.Defense => "Defense",
            BuffType.Speed => "Movement Speed",
            BuffType.CritRate => "Crit Rate",
            BuffType.AttackSpeed => "Attack Speed",
            BuffType.Regeneration => "Regeneration",
            BuffType.Resistance => "Resistance",
            BuffType.Invisibility => "Invisibility",
            BuffType.Invulnerability => "Invulnerability",
            _ => "Unknown Buff"
        };
    }

    private string GetBuffValueText(BuffType buffType, float value)
    {
        if (buffType == BuffType.Speed || 
            buffType == BuffType.CritRate || 
            buffType == BuffType.AttackSpeed || 
            buffType == BuffType.Resistance)
        {
            return $"+{value}%";
        }

        if (buffType == BuffType.Invisibility || buffType == BuffType.Invulnerability)
        {
            return "Active";
        }

        return $"+{value}";
    }

    private string GetBuffDurationText(DurationType durationType, float duration)
    {
        return durationType switch
        {
            DurationType.Instant => "Instant",
            DurationType.RealTime => $"{duration:F1}s",
            DurationType.TurnBased => $"{Mathf.RoundToInt(duration)} turns",
            _ => "Unknown"
        };
    }

    private void UpdateEquipButton()
    {
        if (equipButton == null) return;
        TMP_Text text = equipButton.GetComponentInChildren<TMP_Text>();
        if (text != null)
            text.text = isEquipped ? "Unequip" : "Equip";
    }

    private void OnEquipButtonClicked()
    {
        if (currentPotion == null) return;

        isEquipped = !isEquipped;
        UpdateEquipButton();
        onEquipToggleCallback?.Invoke(currentPotion);
    }

    private void ClosePopup()
    {
        popupContainer.SetActive(false);
        currentPotion = null;
        onEquipToggleCallback = null;
    }
}
