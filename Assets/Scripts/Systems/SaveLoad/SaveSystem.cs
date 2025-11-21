using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using ProtocolEMR.Core;
using ProtocolEMR.Core.Dialogue;

namespace ProtocolEMR.Systems.SaveLoad
{
    /// <summary>
    /// Centralized save/load facade for Sprint 6 deliverables.
    /// Handles slot selection, profile persistence, encryption, and orchestration across subsystems.
    /// </summary>
    public class SaveSystem : MonoBehaviour
    {
        private static SaveSystem instance;
        public static SaveSystem Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = UnityEngine.Object.FindObjectOfType<SaveSystem>();
                    if (instance == null)
                    {
                        var go = new GameObject("SaveSystem");
                        instance = go.AddComponent<SaveSystem>();
                    }
                }

                return instance;
            }
        }

        [Header("Persistence")]
        [SerializeField] private bool encryptSaves = true;
        [SerializeField] private string encryptionSecret = "Protocol-EMR-Sprint6";
        [SerializeField] private string savesFolderName = "Saves";
        [SerializeField] private string profilesFolderName = "Profiles";
        [SerializeField] private string defaultProfileId = SaveSystemConstants.DefaultProfileId;

        [Header("Auto Save")]
        [SerializeField] private bool autoSaveOnInterval = false;
        [SerializeField] private float autoSaveIntervalSeconds = 300f;
        [SerializeField] private bool autoSaveOnQuit = true;
        [SerializeField] private SaveSlotType autoSaveSlotType = SaveSlotType.Auto;
        [SerializeField] private int autoSaveSlotIndex = 0;

        [Header("References")]
        [SerializeField] private GameObject playerOverride;

        public event Action<SaveData> OnBeforeSave;
        public event Action<SaveData> OnAfterSave;
        public event Action<SaveData> OnAfterLoad;

        private string savesRootPath;
        private string profilesRootPath;
        private float lastAutoSaveTimestamp;
        private bool isSaving;
        private bool isLoading;
        private AccountProfileData activeProfile;

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);

            InitializePaths();
            LoadOrCreateDefaultProfile();
        }

        private void Update()
        {
            if (!autoSaveOnInterval || isSaving)
                return;

            if (autoSaveIntervalSeconds <= 0f)
                return;

            if (Time.unscaledTime - lastAutoSaveTimestamp >= autoSaveIntervalSeconds)
            {
                SaveGame(autoSaveSlotType, autoSaveSlotIndex);
                lastAutoSaveTimestamp = Time.unscaledTime;
            }
        }

        private void OnApplicationQuit()
        {
            if (autoSaveOnQuit)
            {
                SaveGame(autoSaveSlotType, autoSaveSlotIndex);
            }
        }

        #region Public API

        public bool SaveGame(SaveSlotType slotType = SaveSlotType.Manual, int slotIndex = 0)
        {
            if (isSaving)
            {
                Debug.LogWarning("[SaveSystem] Save already in progress");
                return false;
            }

            var descriptor = new SaveSlotDescriptor(slotType, slotIndex);

            try
            {
                isSaving = true;
                var saveData = BuildSaveData(descriptor);
                OnBeforeSave?.Invoke(saveData);

                string directory = GetProfileSaveDirectory(saveData.metadata.profileId);
                Directory.CreateDirectory(directory);

                string fileName = GetSaveFileName(descriptor, saveData.metadata.saveId);
                string filePath = Path.Combine(directory, fileName);

                byte[] payload = EncodeSaveData(saveData, descriptor);
                File.WriteAllBytes(filePath, payload);

                PersistActiveProfile();

                OnAfterSave?.Invoke(saveData);
                Debug.Log($"[SaveSystem] Saved game to {filePath}");
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SaveSystem] Failed to save game: {ex.Message}\n{ex.StackTrace}");
                return false;
            }
            finally
            {
                isSaving = false;
            }
        }

        public Coroutine LoadGameAsync(SaveSlotType slotType, int slotIndex, bool reloadScene = true)
        {
            string filePath = FindLatestSaveFile(slotType, slotIndex);
            if (string.IsNullOrEmpty(filePath))
            {
                Debug.LogWarning($"[SaveSystem] No save file found for {slotType} slot {slotIndex}");
                return null;
            }

            var descriptor = new SaveSlotDescriptor(slotType, slotIndex);
            return StartCoroutine(LoadRoutine(filePath, descriptor, reloadScene));
        }

        public AccountProfileData GetActiveProfile()
        {
            return activeProfile?.Clone();
        }

        public void SetActiveProfile(string profileId)
        {
            if (string.IsNullOrEmpty(profileId))
            {
                Debug.LogWarning("[SaveSystem] Profile ID cannot be empty");
                return;
            }

            activeProfile = LoadProfile(profileId) ?? CreateProfile(profileId, "Protocol Agent");
            PersistActiveProfile();
        }

        public List<SaveMetadata> ListSavesForActiveProfile()
        {
            var results = new List<SaveMetadata>();
            string directory = GetProfileSaveDirectory(activeProfile?.profileId);
            if (!Directory.Exists(directory))
                return results;

            foreach (var file in Directory.GetFiles(directory, "*.sav"))
            {
                var descriptor = SaveSlotDescriptor.FromFileName(Path.GetFileName(file));
                if (LoadSystem.TryReadMetadata(file, descriptor, encryptSaves ? encryptionSecret : null, activeProfile, out var metadata))
                {
                    results.Add(metadata);
                }
            }

            return results.OrderByDescending(m => ParseTimestamp(m.timestampUtc)).ToList();
        }

        #endregion

        #region Internal Save/Load

        private SaveData BuildSaveData(SaveSlotDescriptor descriptor)
        {
            var save = new SaveData();
            save.account = activeProfile?.Clone() ?? new AccountProfileData();
            save.metadata.Stamp(descriptor, save.account);

            save.player = PlayerProgressionData.Capture(playerOverride);
            save.npcState = NPCStateSerializer.CaptureState();
            save.world = WorldStateManager.CaptureWorldState();
            save.metadata.currentScene = SceneManager.GetActiveScene().name;
            if (save.world?.blackboard != null)
            {
                save.metadata.locationName = $"Chunk {save.world.blackboard.currentChunkId}";
            }
            save.dialogue = CaptureDialogueState();
            save.metadata.playtimeSeconds = save.player.playtimeSeconds;

            if (MissionSystem.Instance != null)
            {
                save.missionStateJson = MissionSystem.Instance.SerializeMissionState();
            }

            UpdateProfileStats(save);
            return save;
        }

        private DialogueStateSnapshot CaptureDialogueState()
        {
            var snapshot = new DialogueStateSnapshot();
            var manager = UnknownDialogueManager.Instance;
            if (manager == null)
                return snapshot;

            var history = manager.GetMessageHistory();
            if (history != null)
            {
                snapshot.history = new List<MessageHistory>(history);
            }

            var cooldowns = manager.GetMessageCooldownSnapshot();
            if (cooldowns != null)
            {
                foreach (var kvp in cooldowns)
                {
                    snapshot.cooldowns.Add(new NamedFloatValue { key = kvp.Key, value = kvp.Value });
                }
            }

            snapshot.lastMessageTimestamp = manager.GetLastMessageTimestamp();
            snapshot.playerStyleProfile = CloneProfile(manager.GetPlayerStyleProfile());
            return snapshot;
        }

        private byte[] EncodeSaveData(SaveData saveData, SaveSlotDescriptor descriptor)
        {
            saveData.metadata.encrypted = encryptSaves;
            string json = JsonUtility.ToJson(saveData, true);
            byte[] plainBytes = Encoding.UTF8.GetBytes(json);
            saveData.metadata.checksum = EncryptionUtility.ComputeHash(plainBytes);
            saveData.metadata.encrypted = encryptSaves;

            json = JsonUtility.ToJson(saveData, true);
            plainBytes = Encoding.UTF8.GetBytes(json);

            if (!encryptSaves)
            {
                return plainBytes;
            }

            byte[] key = EncryptionUtility.DeriveKey(encryptionSecret, saveData.metadata.profileId);
            byte[] iv = EncryptionUtility.DeriveIV(encryptionSecret, descriptor.GetFilePrefix());
            return EncryptionUtility.Encrypt(plainBytes, key, iv);
        }

        private IEnumerator LoadRoutine(string filePath, SaveSlotDescriptor descriptor, bool reloadScene)
        {
            if (isLoading)
            {
                Debug.LogWarning("[SaveSystem] Load already in progress");
                yield break;
            }

            isLoading = true;

            if (!LoadSystem.TryReadSave(filePath, descriptor, encryptSaves ? encryptionSecret : null, activeProfile, out var saveData, out var error))
            {
                Debug.LogError($"[SaveSystem] Failed to read save: {error}");
                isLoading = false;
                yield break;
            }

            var options = new LoadOptions
            {
                reloadScene = reloadScene,
                playerOverride = playerOverride
            };

            yield return LoadSystem.ApplySaveData(saveData, options);

            activeProfile = saveData.account ?? activeProfile;
            PersistActiveProfile();

            OnAfterLoad?.Invoke(saveData);
            isLoading = false;
        }

        #endregion

        #region Profiles

        private void InitializePaths()
        {
            savesRootPath = Path.Combine(Application.persistentDataPath, savesFolderName);
            profilesRootPath = Path.Combine(Application.persistentDataPath, profilesFolderName);
            Directory.CreateDirectory(savesRootPath);
            Directory.CreateDirectory(profilesRootPath);
        }

        private void LoadOrCreateDefaultProfile()
        {
            if (!string.IsNullOrEmpty(defaultProfileId))
            {
                activeProfile = LoadProfile(defaultProfileId);
            }

            if (activeProfile == null)
            {
                activeProfile = CreateProfile(defaultProfileId, "Protocol Agent");
            }

            PersistActiveProfile();
        }

        private AccountProfileData CreateProfile(string profileId, string profileName)
        {
            var profile = new AccountProfileData
            {
                profileId = string.IsNullOrEmpty(profileId) ? Guid.NewGuid().ToString() : profileId,
                profileName = string.IsNullOrEmpty(profileName) ? "Agent" : profileName,
                avatarIcon = "Icon01"
            };
            profile.Touch();
            SaveProfile(profile);
            return profile;
        }

        private AccountProfileData LoadProfile(string profileId)
        {
            string path = GetProfilePath(profileId);
            if (!File.Exists(path))
                return null;

            try
            {
                string json = File.ReadAllText(path);
                var profile = JsonUtility.FromJson<AccountProfileData>(json);
                profile.profileId = string.IsNullOrEmpty(profile.profileId) ? profileId : profile.profileId;
                return profile;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SaveSystem] Failed to load profile {profileId}: {ex.Message}");
                return null;
            }
        }

        private void PersistActiveProfile()
        {
            if (activeProfile == null)
                return;

            activeProfile.Touch();
            SaveProfile(activeProfile);
        }

        private void SaveProfile(AccountProfileData profile)
        {
            if (profile == null)
                return;

            string path = GetProfilePath(profile.profileId);
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            string json = JsonUtility.ToJson(profile, true);
            File.WriteAllText(path, json);
        }

        private string GetProfilePath(string profileId)
        {
            string safeId = string.IsNullOrEmpty(profileId) ? SaveSystemConstants.DefaultProfileId : profileId;
            return Path.Combine(profilesRootPath, $"profile_{safeId}.json");
        }

        private string GetProfileSaveDirectory(string profileId)
        {
            string safeId = string.IsNullOrEmpty(profileId) ? SaveSystemConstants.DefaultProfileId : profileId;
            return Path.Combine(savesRootPath, safeId);
        }

        private string GetSaveFileName(SaveSlotDescriptor descriptor, string saveId)
        {
            return $"{descriptor.GetFilePrefix()}_{saveId}.sav";
        }

        private string FindLatestSaveFile(SaveSlotType slotType, int slotIndex)
        {
            string directory = GetProfileSaveDirectory(activeProfile?.profileId);
            if (!Directory.Exists(directory))
                return null;

            string prefix = new SaveSlotDescriptor(slotType, slotIndex).GetFilePrefix();
            var files = Directory.GetFiles(directory, $"{prefix}_*.sav");
            if (files.Length == 0)
                return null;

            return files.OrderByDescending(File.GetLastWriteTimeUtc).FirstOrDefault();
        }

        private void UpdateProfileStats(SaveData save)
        {
            if (save?.account == null)
                return;

            save.account.Touch();
            save.account.totalPlaytimeSeconds += Mathf.Max(0f, save.player?.playtimeSeconds ?? 0f);

            if (activeProfile != null && string.Equals(activeProfile.profileId, save.account.profileId, StringComparison.Ordinal))
            {
                activeProfile.totalPlaytimeSeconds = save.account.totalPlaytimeSeconds;
                activeProfile.lastPlayedAtUtc = save.account.lastPlayedAtUtc;
            }
        }

        private PlayerStyleProfile CloneProfile(PlayerStyleProfile profile)
        {
            if (profile == null)
                return null;

            return JsonUtility.FromJson<PlayerStyleProfile>(JsonUtility.ToJson(profile));
        }

        private DateTime ParseTimestamp(string value)
        {
            if (DateTime.TryParse(value, null, System.Globalization.DateTimeStyles.RoundtripKind, out var result))
            {
                return result;
            }

            return DateTime.MinValue;
        }

        #endregion
    }
}
