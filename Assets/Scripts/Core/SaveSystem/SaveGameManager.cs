using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using ProtocolEMR.Core.Player;

namespace ProtocolEMR.Core.SaveSystem
{
    /// <summary>
    /// Main save/load manager for Protocol EMR.
    /// Handles all save/load operations with async I/O, corruption protection, and backup management.
    /// Supports auto-save, manual saves, checkpoints, and quick-save functionality.
    /// </summary>
    public class SaveGameManager : MonoBehaviour
    {
        public static SaveGameManager Instance { get; private set; }

        [Header("Save Settings")]
        [SerializeField] private bool enableAutoSave = true;
        [SerializeField] private float autoSaveInterval = 300f; // 5 minutes
        [SerializeField] private bool enableEncryption = false;
        [SerializeField] private bool enableCompression = false;

        private const int MAX_MANUAL_SAVES = 10;
        private const int MAX_AUTO_SAVES = 3;
        private const int MAX_CHECKPOINTS = 5;

        private string savesDirectory;
        private SaveData currentSaveData;
        private float autoSaveTimer;
        private bool isSaving;
        private bool isLoading;

        public bool IsSaving => isSaving;
        public bool IsLoading => isLoading;
        public SaveData CurrentSaveData => currentSaveData;

        public event Action<SaveData> OnSaveCompleted;
        public event Action<SaveData> OnLoadCompleted;
        public event Action<string> OnSaveFailed;
        public event Action<string> OnLoadFailed;
        public event Action<float> OnSaveProgress;
        public event Action<float> OnLoadProgress;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            savesDirectory = Path.Combine(Application.persistentDataPath, "Saves");
            Directory.CreateDirectory(savesDirectory);

            currentSaveData = new SaveData();
            autoSaveTimer = 0f;
        }

        private void Update()
        {
            if (enableAutoSave && ProfileManager.Instance?.CurrentProfile != null)
            {
                autoSaveTimer += Time.unscaledDeltaTime;
                
                if (autoSaveTimer >= autoSaveInterval)
                {
                    autoSaveTimer = 0f;
                    AutoSave();
                }
            }
        }

        /// <summary>
        /// Saves the current game state to the specified slot.
        /// </summary>
        public void SaveGame(SaveSlotType slotType, int slotIndex = 0, string customName = "")
        {
            if (isSaving)
            {
                Debug.LogWarning("Save operation already in progress");
                return;
            }

            if (ProfileManager.Instance?.CurrentProfile == null)
            {
                Debug.LogError("Cannot save: No active profile");
                OnSaveFailed?.Invoke("No active profile");
                return;
            }

            StartCoroutine(SaveGameCoroutine(slotType, slotIndex, customName));
        }

        private IEnumerator SaveGameCoroutine(SaveSlotType slotType, int slotIndex, string customName)
        {
            isSaving = true;
            OnSaveProgress?.Invoke(0.1f);

            try
            {
                SaveData saveData = CaptureSaveData(slotType, slotIndex);
                OnSaveProgress?.Invoke(0.4f);

                string savePath = GetSavePath(saveData.saveId, saveData.profileId);
                
                if (File.Exists(savePath))
                {
                    BackupManager.CreateBackup(savePath);
                }
                OnSaveProgress?.Invoke(0.6f);

                yield return StartCoroutine(WriteSaveFileAsync(saveData, savePath));
                OnSaveProgress?.Invoke(0.9f);

                ProfileManager.Instance.CurrentProfile.currentSaveSlotId = saveData.saveId;
                ProfileManager.Instance.UpdateProfileStats(saveData.playerState.statistics);

                currentSaveData = saveData;

                OnSaveProgress?.Invoke(1.0f);
                OnSaveCompleted?.Invoke(saveData);

                string slotName = string.IsNullOrEmpty(customName) ? $"{slotType} {slotIndex}" : customName;
                Debug.Log($"Game saved to: {slotName}");
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to save game: {e.Message}");
                OnSaveFailed?.Invoke(e.Message);
            }
            finally
            {
                isSaving = false;
            }
        }

        /// <summary>
        /// Loads a game from the specified save file.
        /// </summary>
        public void LoadGame(string saveId, string profileId)
        {
            if (isLoading)
            {
                Debug.LogWarning("Load operation already in progress");
                return;
            }

            StartCoroutine(LoadGameCoroutine(saveId, profileId));
        }

        private IEnumerator LoadGameCoroutine(string saveId, string profileId)
        {
            isLoading = true;
            OnLoadProgress?.Invoke(0.1f);

            try
            {
                string savePath = GetSavePath(saveId, profileId);
                
                if (!File.Exists(savePath))
                {
                    Debug.LogError($"Save file not found: {savePath}");
                    OnLoadFailed?.Invoke("Save file not found");
                    yield break;
                }

                OnLoadProgress?.Invoke(0.3f);

                SaveData saveData = null;
                yield return StartCoroutine(ReadSaveFileAsync(savePath, result => saveData = result));

                if (saveData == null)
                {
                    Debug.LogError("Failed to load save data");
                    OnLoadFailed?.Invoke("Failed to read save file");
                    yield break;
                }

                OnLoadProgress?.Invoke(0.5f);

                if (!SaveFileValidator.ValidateSaveData(saveData, out string errorMessage))
                {
                    Debug.LogWarning($"Save data validation failed: {errorMessage}");
                    
                    if (BackupManager.RestoreFromBackup(savePath))
                    {
                        yield return StartCoroutine(ReadSaveFileAsync(savePath, result => saveData = result));
                    }
                    else
                    {
                        saveData = SaveFileValidator.RepairSaveData(saveData);
                    }
                }

                OnLoadProgress?.Invoke(0.7f);

                yield return StartCoroutine(ApplySaveData(saveData));

                OnLoadProgress?.Invoke(0.9f);

                currentSaveData = saveData;
                ProfileManager.Instance.SwitchToProfile(profileId);

                OnLoadProgress?.Invoke(1.0f);
                OnLoadCompleted?.Invoke(saveData);

                Debug.Log($"Game loaded: {saveData.locationName}");
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to load game: {e.Message}");
                OnLoadFailed?.Invoke(e.Message);
            }
            finally
            {
                isLoading = false;
            }
        }

        /// <summary>
        /// Performs an auto-save operation.
        /// </summary>
        public void AutoSave()
        {
            if (!enableAutoSave || isSaving) return;

            int autoSaveSlot = (int)(Time.realtimeSinceStartup / autoSaveInterval) % MAX_AUTO_SAVES;
            SaveGame(SaveSlotType.AutoSave, autoSaveSlot, "");
        }

        /// <summary>
        /// Performs a quick-save operation (F5 hotkey).
        /// </summary>
        public void QuickSave()
        {
            SaveGame(SaveSlotType.QuickSave, 0, "Quick Save");
        }

        /// <summary>
        /// Performs a quick-load operation (F9 hotkey).
        /// </summary>
        public void QuickLoad()
        {
            if (ProfileManager.Instance?.CurrentProfile == null) return;

            SaveSlotMetadata quickSave = GetSaveSlots(SaveSlotType.QuickSave).FirstOrDefault();
            if (quickSave != null)
            {
                LoadGame(quickSave.saveId, quickSave.profileId);
            }
        }

        /// <summary>
        /// Creates a checkpoint save before major events.
        /// </summary>
        public void CreateCheckpoint(string checkpointName)
        {
            int checkpointSlot = UnityEngine.Random.Range(0, MAX_CHECKPOINTS);
            SaveGame(SaveSlotType.Checkpoint, checkpointSlot, checkpointName);
        }

        /// <summary>
        /// Captures current game state into SaveData structure.
        /// </summary>
        private SaveData CaptureSaveData(SaveSlotType slotType, int slotIndex)
        {
            SaveData saveData = new SaveData
            {
                profileId = ProfileManager.Instance.CurrentProfile.profileId,
                saveSlotType = slotType,
                slotIndex = slotIndex,
                locationName = GetCurrentLocationName(),
                currentMissionName = GetCurrentMissionName(),
                playtimeSeconds = ProfileManager.Instance.CurrentProfile.totalPlaytime
            };

            CapturePlayerState(saveData.playerState);
            CaptureWorldState(saveData.worldState);
            CaptureMissionState(saveData.missionState);
            CaptureNPCState(saveData.npcState);
            CaptureSessionSettings(saveData.sessionSettings);

            return saveData;
        }

        private void CapturePlayerState(PlayerState playerState)
        {
            PlayerController player = FindObjectOfType<PlayerController>();
            if (player != null)
            {
                playerState.position = new Vector3Serializable(player.transform.position);
                playerState.rotation = new Vector3Serializable(player.transform.eulerAngles);
                playerState.health = 100f; // Default, will be updated by HealthSystem integration
                playerState.stamina = player.CurrentStamina;
            }

            HealthSystem healthSystem = FindObjectOfType<HealthSystem>();
            if (healthSystem != null)
            {
                playerState.health = healthSystem.CurrentHealth;
            }
        }

        private void CaptureWorldState(WorldState worldState)
        {
            worldState.currentScene = SceneManager.GetActiveScene().name;
            worldState.seed = UnityEngine.Random.seed;
        }

        private void CaptureMissionState(MissionState missionState)
        {
            // Placeholder - will be populated by mission system integration
        }

        private void CaptureNPCState(NPCState npcState)
        {
            // Placeholder - will be populated by NPC system integration
        }

        private void CaptureSessionSettings(SessionSettings sessionSettings)
        {
            if (Settings.SettingsManager.Instance != null)
            {
                var gameplaySettings = Settings.SettingsManager.Instance.GetGameplaySettings();
                sessionSettings.difficulty = gameplaySettings.difficulty.ToString();
            }
        }

        /// <summary>
        /// Applies loaded save data to the game state.
        /// </summary>
        private IEnumerator ApplySaveData(SaveData saveData)
        {
            if (saveData.worldState.currentScene != SceneManager.GetActiveScene().name)
            {
                AsyncOperation sceneLoad = SceneManager.LoadSceneAsync(saveData.worldState.currentScene);
                while (!sceneLoad.isDone)
                {
                    yield return null;
                }
            }

            yield return new WaitForSeconds(0.1f);

            ApplyPlayerState(saveData.playerState);
            ApplyWorldState(saveData.worldState);
            ApplyMissionState(saveData.missionState);
            ApplyNPCState(saveData.npcState);
            ApplySessionSettings(saveData.sessionSettings);
        }

        private void ApplyPlayerState(PlayerState playerState)
        {
            PlayerController player = FindObjectOfType<PlayerController>();
            if (player != null)
            {
                player.transform.position = playerState.position.ToVector3();
                player.transform.eulerAngles = playerState.rotation.ToVector3();
            }

            HealthSystem healthSystem = FindObjectOfType<HealthSystem>();
            if (healthSystem != null)
            {
                healthSystem.SetHealth(playerState.health);
            }
        }

        private void ApplyWorldState(WorldState worldState)
        {
            UnityEngine.Random.InitState(worldState.seed);
        }

        private void ApplyMissionState(MissionState missionState)
        {
            // Placeholder - will be populated by mission system integration
        }

        private void ApplyNPCState(NPCState npcState)
        {
            // Placeholder - will be populated by NPC system integration
        }

        private void ApplySessionSettings(SessionSettings sessionSettings)
        {
            // Placeholder - settings already applied from SettingsManager
        }

        /// <summary>
        /// Writes save data to file asynchronously.
        /// </summary>
        private IEnumerator WriteSaveFileAsync(SaveData saveData, string path)
        {
            string json = JsonUtility.ToJson(saveData, true);
            saveData.checksum = SaveFileValidator.CalculateChecksum(json);
            
            json = JsonUtility.ToJson(saveData, true);

            yield return null;

            File.WriteAllText(path, json);
        }

        /// <summary>
        /// Reads save data from file asynchronously.
        /// </summary>
        private IEnumerator ReadSaveFileAsync(string path, Action<SaveData> callback)
        {
            string json = File.ReadAllText(path);
            yield return null;

            SaveData saveData = JsonUtility.FromJson<SaveData>(json);
            
            if (!SaveFileValidator.ValidateChecksum(json, saveData.checksum))
            {
                Debug.LogWarning("Save file checksum mismatch");
            }

            callback?.Invoke(saveData);
        }

        /// <summary>
        /// Gets all save slots for the current profile.
        /// </summary>
        public List<SaveSlotMetadata> GetSaveSlots(SaveSlotType? slotTypeFilter = null)
        {
            if (ProfileManager.Instance?.CurrentProfile == null)
            {
                return new List<SaveSlotMetadata>();
            }

            string profileId = ProfileManager.Instance.CurrentProfile.profileId;
            List<SaveSlotMetadata> slots = new List<SaveSlotMetadata>();

            try
            {
                string[] saveFiles = Directory.GetFiles(savesDirectory, $"*_{profileId}_*.sav");

                foreach (string filePath in saveFiles)
                {
                    try
                    {
                        string json = File.ReadAllText(filePath);
                        SaveData saveData = JsonUtility.FromJson<SaveData>(json);

                        if (slotTypeFilter == null || saveData.saveSlotType == slotTypeFilter)
                        {
                            SaveSlotMetadata metadata = new SaveSlotMetadata
                            {
                                saveId = saveData.saveId,
                                profileId = saveData.profileId,
                                slotType = saveData.saveSlotType,
                                slotIndex = saveData.slotIndex,
                                timestamp = saveData.timestamp,
                                locationName = saveData.locationName,
                                missionName = saveData.currentMissionName,
                                playtime = saveData.playtimeSeconds,
                                difficulty = saveData.sessionSettings.difficulty,
                                screenshotPath = saveData.screenshotPath
                            };

                            slots.Add(metadata);
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"Failed to read save metadata from {filePath}: {e.Message}");
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to get save slots: {e.Message}");
            }

            return slots.OrderByDescending(s => DateTime.Parse(s.timestamp)).ToList();
        }

        /// <summary>
        /// Deletes a specific save file.
        /// </summary>
        public bool DeleteSave(string saveId, string profileId)
        {
            try
            {
                string savePath = GetSavePath(saveId, profileId);
                
                if (File.Exists(savePath))
                {
                    File.Delete(savePath);
                    Debug.Log($"Save deleted: {saveId}");
                    return true;
                }

                return false;
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to delete save: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// Deletes all saves for a specific profile.
        /// </summary>
        public void DeleteAllSavesForProfile(string profileId)
        {
            try
            {
                string[] saveFiles = Directory.GetFiles(savesDirectory, $"*_{profileId}_*.sav");
                
                foreach (string filePath in saveFiles)
                {
                    File.Delete(filePath);
                }

                Debug.Log($"Deleted {saveFiles.Length} saves for profile: {profileId}");
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to delete saves for profile: {e.Message}");
            }
        }

        private string GetSavePath(string saveId, string profileId)
        {
            return Path.Combine(savesDirectory, $"{saveId}_{profileId}_{DateTime.UtcNow:yyyyMMddHHmmss}.sav");
        }

        private string GetCurrentLocationName()
        {
            return SceneManager.GetActiveScene().name;
        }

        private string GetCurrentMissionName()
        {
            return "Mission_Unknown"; // Placeholder
        }
    }
}
