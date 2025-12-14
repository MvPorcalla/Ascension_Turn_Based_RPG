// ═══════════════════════════════════════════════════════════════════════════════
// Assets\Scripts\Core\IGameService.cs
// Contract for game services that require explicit initialization
// ═══════════════════════════════════════════════════════════════════════════════

namespace Ascension.Core
{
    /// <summary>
    /// Contract for game services that require explicit initialization.
    /// Services implementing this interface will be initialized in a controlled order
    /// by the ServiceContainer after auto-discovery.
    /// 
    /// IMPORTANT: Do NOT initialize dependencies in Awake() or Start().
    /// Wait for Initialize() to be called by ServiceContainer.
    /// 
    /// Usage:
    /// public class CharacterManager : MonoBehaviour, IGameService 
    /// { 
    ///     public void Initialize() 
    ///     {
    ///         // Inject dependencies here
    ///         // Subscribe to events here
    ///     }
    /// }
    /// </summary>
    public interface IGameService
    {
        /// <summary>
        /// Called by ServiceContainer after all services are discovered and registered.
        /// This is where you should inject dependencies and initialize state.
        /// </summary>
        void Initialize();
    }
}