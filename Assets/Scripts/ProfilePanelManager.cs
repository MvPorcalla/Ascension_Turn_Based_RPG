// // -------------------------------
// // ProfilePanelManager.cs (Cleaned for merged stats)
// // -------------------------------

// using UnityEngine;
// using UnityEngine.UI;
// using TMPro;

// public class ProfilePanelManager : MonoBehaviour
// {
//     [Header("Player Info")]
//     [SerializeField] private TMP_Text playerNameText;
//     [SerializeField] private TMP_Text playerLevelText;
//     [SerializeField] private TMP_Text guildRankText;

//     [Header("Combat Stats Display")]
//     [SerializeField] private TMP_Text adValueText;
//     [SerializeField] private TMP_Text apValueText;
//     [SerializeField] private TMP_Text critDamageValueText;
//     [SerializeField] private TMP_Text critRateValueText;
//     [SerializeField] private TMP_Text lethalityValueText;
//     [SerializeField] private TMP_Text penetrationValueText; // merged from physical + magic pen
//     [SerializeField] private TMP_Text hpValueText;
//     [SerializeField] private TMP_Text defenseValueText;     // merged from armor + MR
//     [SerializeField] private TMP_Text lifestealValueText;
//     [SerializeField] private TMP_Text evasionValueText;
//     [SerializeField] private TMP_Text tenacityValueText;

//     [Header("Attribute Display")]
//     [SerializeField] private TMP_Text strValueText;
//     [SerializeField] private TMP_Text intValueText;
//     [SerializeField] private TMP_Text agiValueText;
//     [SerializeField] private TMP_Text endValueText;
//     [SerializeField] private TMP_Text wisValueText;
//     [SerializeField] private TMP_Text pointsValueText;

//     [Header("Attribute Buttons")]
//     [SerializeField] private Button strMinusBtn;
//     [SerializeField] private Button strPlusBtn;
//     [SerializeField] private Button intMinusBtn;
//     [SerializeField] private Button intPlusBtn;
//     [SerializeField] private Button agiMinusBtn;
//     [SerializeField] private Button agiPlusBtn;
//     [SerializeField] private Button endMinusBtn;
//     [SerializeField] private Button endPlusBtn;
//     [SerializeField] private Button wisMinusBtn;
//     [SerializeField] private Button wisPlusBtn;

//     [Header("Action Buttons")]
//     [SerializeField] private Button confirmButton;

//     // Temp allocation tracking
//     private int tempSTR, tempINT, tempAGI, tempEND, tempWIS;
//     private int tempPointsSpent = 0;

//     private CharacterBaseStatsSO BaseStats => GameManager.Instance.BaseStats;
//     private PlayerStats Player => GameManager.Instance.CurrentPlayer;

//     private void Start() => SetupButtons();

//     private void OnEnable()
//     {
//         if (GameManager.Instance != null && GameManager.Instance.HasActivePlayer)
//         {
//             InitializeTempValues();
//             RefreshUI();
//         }
//     }

//     private void SetupButtons()
//     {
//         // Attribute buttons
//         strMinusBtn.onClick.AddListener(() => ModifyTempAttribute(ref tempSTR, -1));
//         strPlusBtn.onClick.AddListener(() => ModifyTempAttribute(ref tempSTR, 1));
//         intMinusBtn.onClick.AddListener(() => ModifyTempAttribute(ref tempINT, -1));
//         intPlusBtn.onClick.AddListener(() => ModifyTempAttribute(ref tempINT, 1));
//         agiMinusBtn.onClick.AddListener(() => ModifyTempAttribute(ref tempAGI, -1));
//         agiPlusBtn.onClick.AddListener(() => ModifyTempAttribute(ref tempAGI, 1));
//         endMinusBtn.onClick.AddListener(() => ModifyTempAttribute(ref tempEND, -1));
//         endPlusBtn.onClick.AddListener(() => ModifyTempAttribute(ref tempEND, 1));
//         wisMinusBtn.onClick.AddListener(() => ModifyTempAttribute(ref tempWIS, -1));
//         wisPlusBtn.onClick.AddListener(() => ModifyTempAttribute(ref tempWIS, 1));

//         // Confirm button
//         confirmButton.onClick.AddListener(OnConfirmClicked);
//     }

//     private void InitializeTempValues()
//     {
//         tempSTR = Player.STR;
//         tempINT = Player.INT;
//         tempAGI = Player.AGI;
//         tempEND = Player.END;
//         tempWIS = Player.WIS;
//         tempPointsSpent = 0;
//     }

//     private void ModifyTempAttribute(ref int tempAttribute, int change)
//     {
//         if (change > 0)
//         {
//             if (tempPointsSpent >= Player.unallocatedPoints) return;
//             tempAttribute++;
//             tempPointsSpent++;
//         }
//         else if (change < 0)
//         {
//             int originalValue = GetOriginalValue(ref tempAttribute);
//             if (tempAttribute <= originalValue) return;
//             tempAttribute--;
//             tempPointsSpent--;
//         }

//         RefreshUI();
//     }

//     private int GetOriginalValue(ref int tempAttribute)
//     {
//         if (ReferenceEquals(tempAttribute, tempSTR)) return Player.STR;
//         if (ReferenceEquals(tempAttribute, tempINT)) return Player.INT;
//         if (ReferenceEquals(tempAttribute, tempAGI)) return Player.AGI;
//         if (ReferenceEquals(tempAttribute, tempEND)) return Player.END;
//         if (ReferenceEquals(tempAttribute, tempWIS)) return Player.WIS;
//         return 0;
//     }

//     private void RefreshUI()
//     {
//         UpdatePlayerInfo();
//         UpdateCombatStatsPreview();
//         UpdateAttributeDisplay();
//         UpdateButtonStates();
//     }

//     private void UpdatePlayerInfo()
//     {
//         if (playerNameText) playerNameText.text = Player.playerName;
//         if (playerLevelText) playerLevelText.text = $"Lv.{Player.level}";
//         if (guildRankText) guildRankText.text = "Unranked"; // Placeholder
//     }

//     private void UpdateCombatStatsPreview()
//     {
//         PlayerStats preview = new PlayerStats
//         {
//             level = Player.level,
//             STR = tempSTR,
//             INT = tempINT,
//             AGI = tempAGI,
//             END = tempEND,
//             WIS = tempWIS,
//             ItemAD = Player.ItemAD,
//             ItemAP = Player.ItemAP,
//             ItemHP = Player.ItemHP,
//             ItemDefense = Player.ItemDefense,
//             ItemCritRate = Player.ItemCritRate,
//             ItemCritDamage = Player.ItemCritDamage,
//             ItemEvasion = Player.ItemEvasion,
//             ItemTenacity = Player.ItemTenacity,
//             ItemLethality = Player.ItemLethality,
//             ItemPenetration = Player.ItemPenetration,
//             ItemLifesteal = Player.ItemLifesteal
//         };

//         preview.CalculateCombatStats(BaseStats);

//         if (adValueText) adValueText.text = preview.AD.ToString("F1");
//         if (apValueText) apValueText.text = preview.AP.ToString("F1");
//         if (critDamageValueText) critDamageValueText.text = preview.CritDamage.ToString("F1") + "%";
//         if (critRateValueText) critRateValueText.text = preview.CritRate.ToString("F1") + "%";
//         if (lethalityValueText) lethalityValueText.text = preview.Lethality.ToString("F0");
//         if (penetrationValueText) penetrationValueText.text = preview.Penetration.ToString("F1") + "%";
//         if (hpValueText) hpValueText.text = preview.HP.ToString("F0");
//         if (defenseValueText) defenseValueText.text = preview.Defense.ToString("F1");
//         if (lifestealValueText) lifestealValueText.text = preview.Lifesteal.ToString("F1") + "%";
//         if (evasionValueText) evasionValueText.text = preview.Evasion.ToString("F1") + "%";
//         if (tenacityValueText) tenacityValueText.text = preview.Tenacity.ToString("F1") + "%";
//     }

//     private void UpdateAttributeDisplay()
//     {
//         if (strValueText) strValueText.text = FormatAttributeText(Player.STR, tempSTR);
//         if (intValueText) intValueText.text = FormatAttributeText(Player.INT, tempINT);
//         if (agiValueText) agiValueText.text = FormatAttributeText(Player.AGI, tempAGI);
//         if (endValueText) endValueText.text = FormatAttributeText(Player.END, tempEND);
//         if (wisValueText) wisValueText.text = FormatAttributeText(Player.WIS, tempWIS);

//         int pointsRemaining = Player.unallocatedPoints - tempPointsSpent;
//         if (pointsValueText)
//         {
//             pointsValueText.text = pointsRemaining.ToString();
//             pointsValueText.color = pointsRemaining > 0 ? Color.green : Color.white;
//         }
//     }

//     private string FormatAttributeText(int original, int temp)
//     {
//         return temp > original ? $"{temp} <color=green>(+{temp - original})</color>" : temp.ToString();
//     }

//     private void UpdateButtonStates()
//     {
//         bool hasPointsToSpend = tempPointsSpent < Player.unallocatedPoints;
//         bool hasPointsAllocated = tempPointsSpent > 0;

//         strPlusBtn.interactable = hasPointsToSpend;
//         intPlusBtn.interactable = hasPointsToSpend;
//         agiPlusBtn.interactable = hasPointsToSpend;
//         endPlusBtn.interactable = hasPointsToSpend;
//         wisPlusBtn.interactable = hasPointsToSpend;

//         strMinusBtn.interactable = tempSTR > Player.STR;
//         intMinusBtn.interactable = tempINT > Player.INT;
//         agiMinusBtn.interactable = tempAGI > Player.AGI;
//         endMinusBtn.interactable = tempEND > Player.END;
//         wisMinusBtn.interactable = tempWIS > Player.WIS;

//         confirmButton.interactable = hasPointsAllocated;
//     }

//     private void OnConfirmClicked()
//     {
//         if (tempPointsSpent <= 0) return;

//         Player.STR = tempSTR;
//         Player.INT = tempINT;
//         Player.AGI = tempAGI;
//         Player.END = tempEND;
//         Player.WIS = tempWIS;
//         Player.unallocatedPoints -= tempPointsSpent;

//         Player.RecalculateStats(BaseStats, fullHeal: false);

//         GameManager.Instance.SaveGame();
//         tempPointsSpent = 0;
//         RefreshUI();

//         PlayerHUD hud = FindObjectOfType<PlayerHUD>();
//         if (hud != null) hud.RefreshHUD();

//         Debug.Log("[ProfilePanel] Stats allocated and saved!");
//     }
// }
