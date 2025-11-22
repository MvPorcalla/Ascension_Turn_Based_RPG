RootFolder
├── SaveFolder
    ├── PlayerData
    │   ├── playerData.json
    │   └── backupFolder
    │       ├── playerBackup01.json
    │       ├── playerBackup02.json
    │       └── playerBackup03.json
    ├── Future save system...

│    
├──
└──

Canvas (Screen Space - Overlay, 1920x1080)
│
├── Panel_BG (optional full-screen background)
│   └── Image component (background color or sprite)
│
└── Panel_MainLayout (fills screen)
     └── AvatarCreationLayout (800x1000, anchored middle center)
         ├── Panel (image component)
         │    ├── HeaderGO
         │    │    └── Title_Attributes
         │    │
         │    ├── InputField_Name
         │    │    ├── TMP_InputField
         │    │    └── Placeholder ("Enter Name")
         │    │
         │    ├── Panel_Attributes (VerticalLayoutGroup)
         │    │    ├── Sub_HeaderGO
         │    │    │    └── Text_Title ( Attributes allocation )
         │    │    ├── Attribute_STR (HorizontalLayoutGroup)
         │    │    │    ├── Text_Attribut
         │    │    │    ├── Attribute_Value (TMP_Text)
         │    │    │    └── Attribute_Buttons (emptyGO)
         │    │    │         ├── Button_Minus ("-")
         │    │    │         │    └── Button component + TMP_Text
         │    │    │         └── Button_Plus ("+")
         │    │    │              └── Button component + TMP_Text
         │    │    │
         │    │    ├── Attribute_INT (same structure)
         │    │    ├── Attribute_AGI (same structure)
         │    │    ├── Attribute_END (same structure)
         │    │    ├── Attribute_WIS (same structure)
         │    │    ├── Spacer (10px)
         │    │    └── Points
         │    │        ├── PointsText (TMP_Text)
         │    │        └── PointsValue (TMP_Text)
         │    │    
         │    ├── Panel_CombatStats (VerticalLayoutGroup)
         │    │    ├── Header_ComabtStats
         │    │    │    └── Text_Title ( Combat stats )
         │    │    ├── Base_AD ("Attack Damage: 10")
         │    │    │    ├── Text_Label
         │    │    │    └── Text_Value (TMP_Text)
         │    │    ├── Base_AP ("Ability Power: 5")
         │    │    │    ├── Text_Label
         │    │    │    └── Text_Value (TMP_Text)
         │    │    ├── Base_CritDamage ("Crit Damage: 15%")
         │    │    │    ├── Text_Label
         │    │    │    └── Text_Value (TMP_Text)
         │    │    ├── Base_CritRate ("Crit Rate: 5%")
         │    │    │    ├── Text_Label
         │    │    │    └── Text_Value (TMP_Text)
         │    │    ├── Base_Lethality (" ")
         │    │    │    ├── Text_Label
         │    │    │    └── Text_Value (TMP_Text)
         │    │    ├── Base_PhysicalPen (" ")
         │    │    │    ├── Text_Label
         │    │    │    └── Text_Value (TMP_Text)
         │    │    ├── Base_MagicPen (" ")
         │    │    │    ├── Text_Label
         │    │    │    └── Text_Value (TMP_Text)
         │    │    ├── Base_HP ("HP: 100")
         │    │    │    ├── Text_Label
         │    │    │    └── Text_Value (TMP_Text)
         │    │    ├── Base_AR ("Armor: 5")
         │    │    │    ├── Text_Label
         │    │    │    └── Text_Value (TMP_Text)
         │    │    ├── Base_MR ("Magic Resist: 5")
         │    │    │    ├── Text_Label
         │    │    │    └── Text_Value (TMP_Text)
         │    │    ├── Base_Evasion ("Evasion: 2%")
         │    │    │    ├── Text_Label
         │    │    │    └── Text_Value (TMP_Text)
         │    │    └── Base_Tenacity ("Tenacity: 0%")
         │    │         ├── Text_Label
         │    │         └── Text_Value (TMP_Text)
         │    │
         │    └── Button_Confirm ("Confirm & Start")
         │         └── Button component + TMP_Text
         │
         ├── Editorbuttons
         │   ├── PlayerName
         │   ├── Health (500/500)
         │   ├── Exp (0/100)
         │   └── Level (Lv. 1)
         │
         └── Editorbuttons
             ├── AddExp_Button
             ├── LevelUp_Button
             ├── DealDamage_Button
             └── Heal_Button




// -------------------------------
// Usage Examples for Bootstrap Flow
// -------------------------------
// 
// GAME FLOW:
// 01_Bootstrap → (No Save?) → 02_AvatarCreation → 03_MainBase
//             → (Has Save?) → 03_MainBase
//
// Player always returns to 03_MainBase after playing
// 02_AvatarCreation only runs ONCE per save file
// -------------------------------

// ============================================
// UPDATED AvatarCreationManager.OnConfirmClicked()
// ============================================
private void OnConfirmClicked()
{
    if (string.IsNullOrWhiteSpace(nameInput.text))
    {
        Debug.LogWarning("Please enter a character name!");
        return;
    }
    
    if (pointsSpent < totalPointsToAllocate)
    {
        Debug.LogWarning($"You still have {totalPointsToAllocate - pointsSpent} points to allocate!");
        return;
    }
    
    currentStats.playerName = nameInput.text;
    
    // Use GameManager - save and go to MainBase
    GameManager.Instance.SetPlayerStats(currentStats);
    GameManager.Instance.SaveGame(0);
    GameManager.Instance.GoToMainBase(); // Goes to 03_MainBase
}

// ============================================
// UPDATED LevelUpManager.SavePlayerStats()
// ============================================
private void SavePlayerStats()
{
    // NEW: Use GameManager
    GameManager.Instance.SaveGame();
}

// ============================================
// EXAMPLE: MainBase Scene Controller
// ============================================
public class MainBaseController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_Text playerNameText;
    [SerializeField] private TMP_Text playerLevelText;
    [SerializeField] private Button settingsBtn;
    [SerializeField] private Button deleteCharacterBtn;
    
    private void Start()
    {
        // Verify player exists (should always exist if Bootstrap worked)
        if (!GameManager.Instance.HasActivePlayer)
        {
            Debug.LogError("No player! Restarting from Bootstrap...");
            SceneManager.LoadScene("01_Bootstrap");
            return;
        }
        
        // Display player info
        PlayerStats player = GameManager.Instance.CurrentPlayer;
        playerNameText.text = player.playerName;
        playerLevelText.text = $"Level {player.level}";
        
        // Setup buttons
        deleteCharacterBtn.onClick.AddListener(OnDeleteCharacter);
    }
    
    private void OnDeleteCharacter()
    {
        // Confirm dialog should be shown here
        GameManager.Instance.ResetAndCreateNewCharacter(0);
    }
    
    // Call this when returning from battle/dungeon/etc
    public void OnReturnToBase()
    {
        GameManager.Instance.SaveGame(); // Auto-save on return
    }
}

// ============================================
// EXAMPLE: Settings Panel (in MainBase)
// ============================================
public class SettingsPanel : MonoBehaviour
{
    [SerializeField] private Button restoreBackupBtn;
    [SerializeField] private Button resetGameBtn;
    
    private void Start()
    {
        restoreBackupBtn.onClick.AddListener(OnRestoreBackup);
        resetGameBtn.onClick.AddListener(OnResetGame);
    }
    
    private void OnRestoreBackup()
    {
        // If player thinks their save is bugged
        if (SaveManager.Instance.RestoreFromBackup(0))
        {
            GameManager.Instance.LoadGame(0);
            SceneManager.LoadScene("03_MainBase"); // Reload scene
        }
    }
    
    private void OnResetGame()
    {
        // Start completely fresh
        GameManager.Instance.ResetAndCreateNewCharacter(0);
    }
}

// ============================================
// EXAMPLE: Battle/Dungeon Scene
// ============================================
public class BattleManager : MonoBehaviour
{
    public void OnBattleWon(int expReward)
    {
        // Add exp through GameManager
        bool leveledUp = GameManager.Instance.AddExperience(expReward);
        
        if (leveledUp)
        {
            // Show level up UI
            // LevelUpManager handles the point allocation
        }
    }
    
    public void OnReturnToBase()
    {
        // Save progress and return
        GameManager.Instance.LoadSceneWithSave("03_MainBase");
    }
}