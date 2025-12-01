// ──────────────────────────────────────────────────
// CharacterManager.cs
// Central manager for player character data
// Single source of truth for all player stats
// ─────────────────────────────────────────────────

using UnityEngine;
using System;

public class CharacterManager : MonoBehaviour
{
    public static CharacterManager Instance { get; private set; }

    [Header("References")]
    [SerializeField] private CharacterBaseStatsSO baseStats;

    [Header("Runtime Data")]
    private PlayerStats currentPlayer;
    private bool isInitialized = false;

    // Events for other systems to listen to
    public event Action<PlayerStats> OnPlayerLoaded;
    public event Action<PlayerStats> OnPlayerStatsChanged;
    public event Action<float, float> OnHealthChanged; // (current, max)
    public event Action<int> OnLevelUp; // (newLevel)
    public event Action<int> OnExperienceGained; // (amount)

    // Public accessors
    public PlayerStats CurrentPlayer => currentPlayer;
    public CharacterBaseStatsSO BaseStats => baseStats;
    public bool HasActivePlayer => isInitialized && currentPlayer != null;

    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    #region Player Initialization

    /// <summary>
    /// Create a new player character
    /// </summary>
    public void CreateNewPlayer(string playerName)
    {
        currentPlayer = new PlayerStats();
        currentPlayer.playerName = playerName;
        currentPlayer.Initialize(baseStats);
        
        isInitialized = true;

        Debug.Log($"[CharacterManager] Created new player: {playerName}");
        OnPlayerLoaded?.Invoke(currentPlayer);
        OnPlayerStatsChanged?.Invoke(currentPlayer);
    }

    /// <summary>
    /// Load player from JSON data
    /// </summary>
    public void LoadPlayer(PlayerStats loadedStats)
    {
        if (loadedStats == null)
        {
            Debug.LogError("[CharacterManager] Cannot load null player stats!");
            return;
        }

        currentPlayer = loadedStats;
        
        // Ensure stats are recalculated with current base stats
        currentPlayer.RecalculateStats(baseStats, fullHeal: false);
        
        isInitialized = true;

        Debug.Log($"[CharacterManager] Loaded player: {currentPlayer.playerName}");
        OnPlayerLoaded?.Invoke(currentPlayer);
        OnPlayerStatsChanged?.Invoke(currentPlayer);
    }

    /// <summary>
    /// Unload current player (for returning to menu, etc.)
    /// </summary>
    public void UnloadPlayer()
    {
        currentPlayer = null;
        isInitialized = false;
        Debug.Log("[CharacterManager] Player unloaded");
    }

    #endregion

    #region Player Actions

    /// <summary>
    /// Add experience to player
    /// </summary>
    public void AddExperience(int amount)
    {
        if (!HasActivePlayer) return;

        OnExperienceGained?.Invoke(amount);

        bool leveledUp = currentPlayer.AddExperience(amount, baseStats);

        if (leveledUp)
        {
            Debug.Log($"[CharacterManager] Level up! Now level {currentPlayer.Level}");
            OnLevelUp?.Invoke(currentPlayer.Level);
            OnHealthChanged?.Invoke(currentPlayer.CurrentHP, currentPlayer.MaxHP);
        }

        OnPlayerStatsChanged?.Invoke(currentPlayer);
    }

    /// <summary>
    /// Heal player
    /// </summary>
    public void Heal(float amount)
    {
        if (!HasActivePlayer) return;

        float oldHP = currentPlayer.CurrentHP;
        currentPlayer.combatRuntime.Heal(amount, currentPlayer.MaxHP);
        
        Debug.Log($"[CharacterManager] Healed {amount} HP ({oldHP:F0} → {currentPlayer.CurrentHP:F0})");
        OnHealthChanged?.Invoke(currentPlayer.CurrentHP, currentPlayer.MaxHP);
    }

    /// <summary>
    /// Apply heal to player (alias for Heal)
    /// </summary>
    public void ApplyHeal(float amount)
    {
        if (!HasActivePlayer) return;
        
        float oldHP = currentPlayer.CurrentHP;
        currentPlayer.combatRuntime.Heal(amount, currentPlayer.MaxHP);
        OnHealthChanged?.Invoke(currentPlayer.CurrentHP, currentPlayer.MaxHP);

        Debug.Log($"[CharacterManager] Healed {amount} HP ({oldHP:F0} → {currentPlayer.CurrentHP:F0})");
    }


    /// <summary>
    /// Damage player
    /// </summary>
    public void TakeDamage(float amount)
    {
        if (!HasActivePlayer) return;

        float oldHP = currentPlayer.CurrentHP;
        currentPlayer.combatRuntime.TakeDamage(amount, currentPlayer.MaxHP);
        
        Debug.Log($"[CharacterManager] Took {amount} damage ({oldHP:F0} → {currentPlayer.CurrentHP:F0})");
        OnHealthChanged?.Invoke(currentPlayer.CurrentHP, currentPlayer.MaxHP);

        // Check for death
        if (currentPlayer.CurrentHP <= 0)
        {
            OnPlayerDeath();
        }
    }

    /// <summary>
    /// Full heal player
    /// </summary>
    public void FullHeal()
    {
        if (!HasActivePlayer) return;

        currentPlayer.combatRuntime.currentHP = currentPlayer.MaxHP;
        Debug.Log("[CharacterManager] Full heal applied");
        OnHealthChanged?.Invoke(currentPlayer.CurrentHP, currentPlayer.MaxHP);
    }

    /// <summary>
    /// Equip weapon
    /// </summary>
    public void EquipWeapon(WeaponSO weapon)
    {
        if (!HasActivePlayer) return;

        currentPlayer.EquipWeapon(weapon, baseStats);
        OnPlayerStatsChanged?.Invoke(currentPlayer);
    }

    /// <summary>
    /// Unequip weapon
    /// </summary>
    public void UnequipWeapon()
    {
        if (!HasActivePlayer) return;

        currentPlayer.UnequipWeapon(baseStats);
        OnPlayerStatsChanged?.Invoke(currentPlayer);
    }

    /// <summary>
    /// Set guild rank
    /// </summary>
    public void SetGuildRank(string rank)
    {
        if (!HasActivePlayer) return;

        currentPlayer.SetGuildRank(rank);
        OnPlayerStatsChanged?.Invoke(currentPlayer);
    }

    /// <summary>
    /// Allocate attribute point
    /// </summary>
    public bool AllocateAttributePoint(string attributeName)
    {
        if (!HasActivePlayer) return false;

        if (currentPlayer.UnallocatedPoints <= 0)
        {
            Debug.LogWarning("[CharacterManager] No unallocated points available!");
            return false;
        }

        currentPlayer.ModifyAttribute(attributeName, 1, baseStats);
        currentPlayer.levelSystem.unallocatedPoints--;
        
        Debug.Log($"[CharacterManager] Allocated point to {attributeName}");
        OnPlayerStatsChanged?.Invoke(currentPlayer);
        return true;
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Recalculate all player stats (call after equipment/buff changes)
    /// </summary>
    public void RecalculateStats()
    {
        if (!HasActivePlayer) return;

        currentPlayer.RecalculateStats(baseStats, fullHeal: false);
        OnPlayerStatsChanged?.Invoke(currentPlayer);
    }

    /// <summary>
    /// Get player data ready for JSON serialization
    /// </summary>
    public PlayerStats GetPlayerDataForSave()
    {
        return currentPlayer;
    }

    /// <summary>
    /// Handle player death
    /// </summary>
    private void OnPlayerDeath()
    {
        Debug.Log("[CharacterManager] Player has died!");
        // TODO: Implement death logic (respawn, game over, etc.)
    }

    #endregion

    #region Debug Tools

    [ContextMenu("Debug: Print Player Stats")]
    private void DebugPrintStats()
    {
        if (!HasActivePlayer)
        {
            Debug.Log("[CharacterManager] No active player");
            return;
        }

        Debug.Log("=== PLAYER STATS ===");
        Debug.Log($"Name: {currentPlayer.playerName}");
        Debug.Log($"Level: {currentPlayer.Level}");
        Debug.Log($"HP: {currentPlayer.CurrentHP}/{currentPlayer.MaxHP}");
        Debug.Log($"AD: {currentPlayer.AD}");
        Debug.Log($"AP: {currentPlayer.AP}");
        Debug.Log($"Attack Speed: {currentPlayer.AttackSpeed}");
        Debug.Log($"STR: {currentPlayer.attributes.STR}");
        Debug.Log($"INT: {currentPlayer.attributes.INT}");
        Debug.Log($"AGI: {currentPlayer.attributes.AGI}");
        Debug.Log($"END: {currentPlayer.attributes.END}");
        Debug.Log($"WIS: {currentPlayer.attributes.WIS}");
    }

    [ContextMenu("Debug: Add 100 EXP")]
    private void DebugAddExp()
    {
        AddExperience(100);
    }

    [ContextMenu("Debug: Damage 50 HP")]
    private void DebugDamage()
    {
        TakeDamage(50);
    }

    [ContextMenu("Debug: Heal 50 HP")]
    private void DebugHeal()
    {
        Heal(50);
    }

    [ContextMenu("Debug: Full Heal")]
    private void DebugFullHeal()
    {
        FullHeal();
    }

    #endregion
}