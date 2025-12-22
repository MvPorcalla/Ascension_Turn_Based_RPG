// ════════════════════════════════════════════
// Assets\Scripts\Modules\EquipmentSystem\UI\EquipmentRoomUI.cs
// Main controller for Equipment Room UI
// ════════════════════════════════════════════

using UnityEngine;
using UnityEngine.UI;
using Ascension.Equipment.Enums;
using Ascension.Equipment.Manager;
using Ascension.Equipment.Services;
using Ascension.UI;
using Ascension.Character.Manager;
using Ascension.Character.Stat;

namespace Ascension.Equipment.UI
{
    public class EquipmentRoomUI : MonoBehaviour
    {
        #region Serialized Fields
        [Header("Sections")]
        [SerializeField] private PlayerPreviewUI playerPreview;
        [SerializeField] private EquipmentStorageUI storageUI;
        
        [Header("Gear Slots")]
        [SerializeField] private GearSlotUI weaponSlot;
        [SerializeField] private GearSlotUI helmetSlot;
        [SerializeField] private GearSlotUI chestSlot;
        [SerializeField] private GearSlotUI glovesSlot;
        [SerializeField] private GearSlotUI bootsSlot;
        [SerializeField] private GearSlotUI accessory1Slot;
        [SerializeField] private GearSlotUI accessory2Slot;
        
        [Header("Skill Slots")]
        [SerializeField] private SkillSlotUI normalSkill1Slot;
        [SerializeField] private SkillSlotUI normalSkill2Slot;
        [SerializeField] private SkillSlotUI ultimateSkillSlot;
        
        [Header("Navigation")]
        [SerializeField] private Button backButton;
        #endregion

        #region Private Fields
        private GearSlotService _slotService;
        #endregion

        #region Unity Callbacks
        private void Awake()
        {
            _slotService = new GearSlotService();
            SetupGearSlots();
            SetupSkillSlots();
            SetupButtons();
        }

        private void OnEnable()
        {
            SubscribeToEvents();
            RefreshUI();
        }

        private void OnDisable()
        {
            UnsubscribeFromEvents();
        }
        #endregion

        #region Setup
        private void SetupGearSlots()
        {
            // Subscribe to gear slot clicks
            if (weaponSlot != null)
                weaponSlot.OnSlotClicked += OnGearSlotClicked;
            
            if (helmetSlot != null)
                helmetSlot.OnSlotClicked += OnGearSlotClicked;
            
            if (chestSlot != null)
                chestSlot.OnSlotClicked += OnGearSlotClicked;
            
            if (glovesSlot != null)
                glovesSlot.OnSlotClicked += OnGearSlotClicked;
            
            if (bootsSlot != null)
                bootsSlot.OnSlotClicked += OnGearSlotClicked;
            
            if (accessory1Slot != null)
                accessory1Slot.OnSlotClicked += OnGearSlotClicked;
            
            if (accessory2Slot != null)
                accessory2Slot.OnSlotClicked += OnGearSlotClicked;
        }

        private void SetupSkillSlots()
        {
            // Subscribe to skill slot clicks
            if (normalSkill1Slot != null)
                normalSkill1Slot.OnSlotClicked += OnSkillSlotClicked;
            
            if (normalSkill2Slot != null)
                normalSkill2Slot.OnSlotClicked += OnSkillSlotClicked;
            
            if (ultimateSkillSlot != null)
                ultimateSkillSlot.OnSlotClicked += OnSkillSlotClicked;
        }

        private void SetupButtons()
        {
            if (backButton != null)
                backButton.onClick.AddListener(OnBackClicked);
        }

        private void SubscribeToEvents()
        {
            if (EquipmentManager.Instance != null)
            {
                EquipmentManager.Instance.OnEquipmentChanged += RefreshUI;
            }

            if (SkillLoadoutManager.Instance != null)
            {
                SkillLoadoutManager.Instance.OnLoadoutChanged += RefreshSkillSlots;
            }

            if (CharacterManager.Instance != null)
            {
                CharacterManager.Instance.OnCharacterStatsChanged += OnStatsChanged;
            }
        }

        private void UnsubscribeFromEvents()
        {
            if (EquipmentManager.Instance != null)
            {
                EquipmentManager.Instance.OnEquipmentChanged -= RefreshUI;
            }

            if (SkillLoadoutManager.Instance != null)
            {
                SkillLoadoutManager.Instance.OnLoadoutChanged -= RefreshSkillSlots;
            }

            if (CharacterManager.Instance != null)
            {
                CharacterManager.Instance.OnCharacterStatsChanged -= OnStatsChanged;
            }
        }
        #endregion

        #region UI Refresh
        private void RefreshUI()
        {
            RefreshPlayerPreview();
            RefreshAllGearSlots();
            RefreshSkillSlots();
        }

        private void RefreshPlayerPreview()
        {
            if (playerPreview == null || CharacterManager.Instance == null)
                return;

            var currentPlayer = CharacterManager.Instance.CurrentPlayer;
            if (currentPlayer == null || currentPlayer.derivedStats == null)
                return;

            // create a read-only snapshot / DTO
            CharacterDerivedStats previewStats = new CharacterDerivedStats(currentPlayer.derivedStats);

            playerPreview.UpdateStats(previewStats);
        }

        private void RefreshAllGearSlots()
        {
            weaponSlot?.RefreshSlot();
            helmetSlot?.RefreshSlot();
            chestSlot?.RefreshSlot();
            glovesSlot?.RefreshSlot();
            bootsSlot?.RefreshSlot();
            accessory1Slot?.RefreshSlot();
            accessory2Slot?.RefreshSlot();
        }

        private void RefreshSkillSlots()
        {
            normalSkill1Slot?.RefreshSlot();
            normalSkill2Slot?.RefreshSlot();
            ultimateSkillSlot?.RefreshSlot();
        }
        #endregion

        #region Event Handlers - Gear Slots
        private void OnGearSlotClicked(GearSlotType slotType)
        {
            Debug.Log($"[EquipmentRoomUI] Gear slot clicked: {slotType}");
            
            // Get filter for this slot
            EquipmentStorageFilter filter = _slotService.GetFilterForSlot(slotType);
            
            // Show filtered storage
            if (storageUI != null)
            {
                storageUI.ShowFilteredItems(filter);
            }
        }
        #endregion

        #region Event Handlers - Skill Slots
        private void OnSkillSlotClicked(SkillSlotType slotType)
        {
            Debug.Log($"[EquipmentRoomUI] Skill slot clicked: {slotType}");
            
            // Show abilities filter
            if (storageUI != null)
            {
                storageUI.ShowFilteredItems(EquipmentStorageFilter.Abilities);
            }
        }
        #endregion

        #region Event Handlers - Other
        private void OnStatsChanged(Character.Stat.CharacterStats stats)
        {
            RefreshPlayerPreview();
        }

        private void OnBackClicked()
        {
            Debug.Log("[EquipmentRoomUI] Back button clicked");
            gameObject.SetActive(false);
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Open equipment room and show specific filter
        /// </summary>
        public void OpenWithFilter(EquipmentStorageFilter filter)
        {
            gameObject.SetActive(true);
            
            if (storageUI != null)
            {
                storageUI.ShowFilteredItems(filter);
            }
        }
        #endregion

        #region Debug
        [ContextMenu("Force Refresh UI")]
        private void DebugForceRefresh()
        {
            RefreshUI();
        }
        #endregion
    }
}