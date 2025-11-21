using System;
using System.Threading;
using System.Threading.Tasks;

namespace Core.Interfaces
{
    /// <summary>
    /// Service for managing game save/load operations and persistent data.
    /// </summary>
    public interface ISaveService
    {
        /// <summary>
        /// Check if save data exists.
        /// </summary>
        bool HasSaveData { get; }

        /// <summary>
        /// Get the last save time.
        /// </summary>
        DateTime LastSaveTime { get; }

        /// <summary>
        /// Event fired when game is saved.
        /// </summary>
        event Action OnGameSaved;

        /// <summary>
        /// Event fired when game is loaded.
        /// </summary>
        event Action OnGameLoaded;

        /// <summary>
        /// Save the game state.
        /// </summary>
        /// <param name="saveSlot">Save slot identifier</param>
        /// <param name="cancellationToken">Optional cancellation token</param>
        /// <returns>True if save successful</returns>
        Task<bool> SaveGameAsync(string saveSlot = "default", CancellationToken cancellationToken = default);

        /// <summary>
        /// Load the game state.
        /// </summary>
        /// <param name="saveSlot">Save slot identifier</param>
        /// <param name="cancellationToken">Optional cancellation token</param>
        /// <returns>True if load successful</returns>
        Task<bool> LoadGameAsync(string saveSlot = "default", CancellationToken cancellationToken = default);

        /// <summary>
        /// Delete a save game.
        /// </summary>
        /// <param name="saveSlot">Save slot identifier</param>
        /// <returns>True if deletion successful</returns>
        bool DeleteSaveGame(string saveSlot = "default");

        /// <summary>
        /// Get information about all save slots.
        /// </summary>
        /// <returns>Array of save slot information</returns>
        SaveSlotInfo[] GetSaveSlotInfos();

        /// <summary>
        /// Check if a save slot exists.
        /// </summary>
        /// <param name="saveSlot">Save slot identifier</param>
        /// <returns>True if slot exists</returns>
        bool SaveSlotExists(string saveSlot);

        /// <summary>
        /// Create a new save slot.
        /// </summary>
        /// <param name="saveSlot">Save slot identifier</param>
        void CreateSaveSlot(string saveSlot);

        /// <summary>
        /// Auto-save the current game state.
        /// </summary>
        /// <returns>True if auto-save successful</returns>
        Task<bool> AutoSaveAsync();

        /// <summary>
        /// Enable or disable auto-save.
        /// </summary>
        /// <param name="enabled">Whether auto-save is enabled</param>
        /// <param name="intervalMinutes">Auto-save interval in minutes</param>
        void SetAutoSave(bool enabled, int intervalMinutes = 5);
    }

    /// <summary>
    /// Information about a save slot.
    /// </summary>
    public struct SaveSlotInfo
    {
        public string SlotName;
        public DateTime SaveTime;
        public float PlaytimeHours;
        public string Description;
        public int SaveVersion;
    }
}