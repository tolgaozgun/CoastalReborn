using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Core.Interfaces;

namespace Core.Services
{
    /// <summary>
    /// Implementation of the save service.
    /// </summary>
    public class SaveService : MonoBehaviour, ISaveService
    {
        [Header("Save Configuration")]
        [SerializeField] private string saveFolder = "Saves";
        [SerializeField] private bool enableAutoSave = true;
        [SerializeField] private int autoSaveIntervalMinutes = 5;

        private string savePath;
        private DateTime lastSaveTime;
        private CancellationTokenSource autoSaveCancellationTokenSource;

        public bool HasSaveData => Directory.Exists(savePath) && Directory.GetFiles(savePath, "*.save").Length > 0;
        public DateTime LastSaveTime => lastSaveTime;

        public event Action OnGameSaved;
        public event Action OnGameLoaded;

        private void Awake()
        {
            savePath = Path.Combine(Application.persistentDataPath, saveFolder);
            Directory.CreateDirectory(savePath);

            if (enableAutoSave)
            {
                SetAutoSave(true, autoSaveIntervalMinutes);
            }
        }

        private void OnDestroy()
        {
            autoSaveCancellationTokenSource?.Cancel();
            autoSaveCancellationTokenSource?.Dispose();
        }

        public async Task<bool> SaveGameAsync(string saveSlot = "default", CancellationToken cancellationToken = default)
        {
            try
            {
                string filePath = GetSaveFilePath(saveSlot);
                SaveData saveData = new SaveData
                {
                    SaveSlot = saveSlot,
                    SaveTime = DateTime.Now,
                    Version = "1.0.0"
                };

                // TODO: Collect actual game data
                // saveData.PlayerData = GetPlayerData();
                // saveData.WorldData = GetWorldData();

                string jsonData = JsonUtility.ToJson(saveData, true);
                await File.WriteAllTextAsync(filePath, jsonData, cancellationToken);

                lastSaveTime = DateTime.Now;
                OnGameSaved?.Invoke();

                Debug.Log($"Game saved to slot '{saveSlot}'");
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to save game: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> LoadGameAsync(string saveSlot = "default", CancellationToken cancellationToken = default)
        {
            try
            {
                string filePath = GetSaveFilePath(saveSlot);
                if (!File.Exists(filePath))
                {
                    Debug.LogWarning($"Save file not found for slot '{saveSlot}'");
                    return false;
                }

                string jsonData = await File.ReadAllTextAsync(filePath, cancellationToken);
                SaveData saveData = JsonUtility.FromJson<SaveData>(jsonData);

                // TODO: Apply loaded game data
                // ApplyPlayerData(saveData.PlayerData);
                // ApplyWorldData(saveData.WorldData);

                OnGameLoaded?.Invoke();
                Debug.Log($"Game loaded from slot '{saveSlot}'");
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to load game: {ex.Message}");
                return false;
            }
        }

        public bool DeleteSaveGame(string saveSlot = "default")
        {
            try
            {
                string filePath = GetSaveFilePath(saveSlot);
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    Debug.Log($"Save file deleted for slot '{saveSlot}'");
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to delete save game: {ex.Message}");
                return false;
            }
        }

        public SaveSlotInfo[] GetSaveSlotInfos()
        {
            // TODO: Implement save slot info collection
            return new SaveSlotInfo[0];
        }

        public bool SaveSlotExists(string saveSlot)
        {
            string filePath = GetSaveFilePath(saveSlot);
            return File.Exists(filePath);
        }

        public void CreateSaveSlot(string saveSlot)
        {
            // Save slots are created automatically when saving
        }

        public async Task<bool> AutoSaveAsync()
        {
            return await SaveGameAsync("autosave");
        }

        public void SetAutoSave(bool enabled, int intervalMinutes = 5)
        {
            autoSaveCancellationTokenSource?.Cancel();

            if (enabled)
            {
                autoSaveCancellationTokenSource = new CancellationTokenSource();
                StartAutoSaveLoop(intervalMinutes, autoSaveCancellationTokenSource.Token);
            }
        }

        private async void StartAutoSaveLoop(int intervalMinutes, CancellationToken cancellationToken)
        {
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    await Task.Delay(TimeSpan.FromMinutes(intervalMinutes), cancellationToken);
                    if (!cancellationToken.IsCancellationRequested)
                    {
                        await AutoSaveAsync();
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // Expected when cancellation is requested
            }
        }

        private string GetSaveFilePath(string saveSlot)
        {
            return Path.Combine(savePath, $"{saveSlot}.save");
        }
    }

    [Serializable]
    public class SaveData
    {
        public string SaveSlot;
        public DateTime SaveTime;
        public string Version;
        public float PlaytimeHours;
        // TODO: Add actual save data fields
    }
}