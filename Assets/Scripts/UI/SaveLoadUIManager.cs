using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ProtocolEMR.Core.SaveSystem;

namespace ProtocolEMR.UI
{
    /// <summary>
    /// UI manager for save/load menu interface.
    /// Displays save slots, handles user interactions, and provides visual feedback.
    /// </summary>
    public class SaveLoadUIManager : MonoBehaviour
    {
        public static SaveLoadUIManager Instance { get; private set; }

        [Header("UI References")]
        [SerializeField] private GameObject saveMenuPanel;
        [SerializeField] private GameObject loadMenuPanel;
        [SerializeField] private GameObject profileMenuPanel;
        [SerializeField] private GameObject saveSlotPrefab;
        [SerializeField] private Transform saveSlotContainer;
        [SerializeField] private TMP_Text statusText;
        [SerializeField] private Slider progressBar;
        [SerializeField] private GameObject progressPanel;

        [Header("Save Menu Elements")]
        [SerializeField] private TMP_InputField saveNameInput;
        [SerializeField] private Button saveButton;
        [SerializeField] private Button cancelButton;

        [Header("Profile Menu Elements")]
        [SerializeField] private TMP_InputField profileNameInput;
        [SerializeField] private TMP_Dropdown avatarDropdown;
        [SerializeField] private TMP_Dropdown difficultyDropdown;
        [SerializeField] private Button createProfileButton;
        [SerializeField] private Transform profileListContainer;
        [SerializeField] private GameObject profileItemPrefab;

        private SaveSlotType currentSlotType;
        private int currentSlotIndex;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;

            InitializeEventHandlers();
            HideAllMenus();
        }

        private void InitializeEventHandlers()
        {
            if (SaveGameManager.Instance != null)
            {
                SaveGameManager.Instance.OnSaveCompleted += OnSaveCompleted;
                SaveGameManager.Instance.OnLoadCompleted += OnLoadCompleted;
                SaveGameManager.Instance.OnSaveFailed += OnSaveFailed;
                SaveGameManager.Instance.OnLoadFailed += OnLoadFailed;
                SaveGameManager.Instance.OnSaveProgress += UpdateProgressBar;
                SaveGameManager.Instance.OnLoadProgress += UpdateProgressBar;
            }

            if (SaveTriggerSystem.Instance != null)
            {
                SaveTriggerSystem.Instance.OnAutoSaveTriggered += OnAutoSaveTriggered;
            }
        }

        private void OnDestroy()
        {
            if (SaveGameManager.Instance != null)
            {
                SaveGameManager.Instance.OnSaveCompleted -= OnSaveCompleted;
                SaveGameManager.Instance.OnLoadCompleted -= OnLoadCompleted;
                SaveGameManager.Instance.OnSaveFailed -= OnSaveFailed;
                SaveGameManager.Instance.OnLoadFailed -= OnLoadFailed;
                SaveGameManager.Instance.OnSaveProgress -= UpdateProgressBar;
                SaveGameManager.Instance.OnLoadProgress -= UpdateProgressBar;
            }

            if (SaveTriggerSystem.Instance != null)
            {
                SaveTriggerSystem.Instance.OnAutoSaveTriggered -= OnAutoSaveTriggered;
            }
        }

        /// <summary>
        /// Opens the save menu for manual saves.
        /// </summary>
        public void OpenSaveMenu()
        {
            if (ProfileManager.Instance?.CurrentProfile == null)
            {
                ShowNotification("No active profile. Please create a profile first.");
                return;
            }

            saveMenuPanel?.SetActive(true);
            loadMenuPanel?.SetActive(false);
            profileMenuPanel?.SetActive(false);

            currentSlotType = SaveSlotType.ManualSave;
            PopulateSaveSlots();
        }

        /// <summary>
        /// Opens the load menu.
        /// </summary>
        public void OpenLoadMenu()
        {
            if (ProfileManager.Instance?.CurrentProfile == null)
            {
                ShowNotification("No active profile. Please create a profile first.");
                return;
            }

            loadMenuPanel?.SetActive(true);
            saveMenuPanel?.SetActive(false);
            profileMenuPanel?.SetActive(false);

            PopulateSaveSlots();
        }

        /// <summary>
        /// Opens the profile menu.
        /// </summary>
        public void OpenProfileMenu()
        {
            profileMenuPanel?.SetActive(true);
            saveMenuPanel?.SetActive(false);
            loadMenuPanel?.SetActive(false);

            PopulateProfileList();
        }

        /// <summary>
        /// Hides all menus.
        /// </summary>
        public void HideAllMenus()
        {
            saveMenuPanel?.SetActive(false);
            loadMenuPanel?.SetActive(false);
            profileMenuPanel?.SetActive(false);
            progressPanel?.SetActive(false);
        }

        /// <summary>
        /// Populates the save slot list with existing saves.
        /// </summary>
        private void PopulateSaveSlots()
        {
            if (saveSlotContainer == null) return;

            // Clear existing slots
            foreach (Transform child in saveSlotContainer)
            {
                Destroy(child.gameObject);
            }

            // Get save slots for current profile
            List<SaveSlotMetadata> slots = SaveGameManager.Instance.GetSaveSlots();

            foreach (SaveSlotMetadata slot in slots)
            {
                CreateSaveSlotUI(slot);
            }

            // Add empty slots for manual saves
            if (currentSlotType == SaveSlotType.ManualSave)
            {
                int existingManualSaves = slots.FindAll(s => s.slotType == SaveSlotType.ManualSave).Count;
                for (int i = existingManualSaves; i < 10; i++)
                {
                    CreateEmptySaveSlotUI(i);
                }
            }
        }

        private void CreateSaveSlotUI(SaveSlotMetadata slot)
        {
            if (saveSlotPrefab == null || saveSlotContainer == null) return;

            GameObject slotObj = Instantiate(saveSlotPrefab, saveSlotContainer);
            
            // Populate slot UI elements (simplified - extend based on prefab structure)
            TMP_Text slotText = slotObj.GetComponentInChildren<TMP_Text>();
            if (slotText != null)
            {
                string formattedTime = System.DateTime.Parse(slot.timestamp).ToLocalTime().ToString("g");
                string playtime = System.TimeSpan.FromSeconds(slot.playtime).ToString(@"hh\:mm\:ss");
                
                slotText.text = $"{slot.slotType} {slot.slotIndex}\n" +
                               $"Location: {slot.locationName}\n" +
                               $"Time: {formattedTime}\n" +
                               $"Playtime: {playtime}\n" +
                               $"Difficulty: {slot.difficulty}";
            }

            Button loadButton = slotObj.GetComponent<Button>();
            if (loadButton != null)
            {
                loadButton.onClick.AddListener(() => LoadSave(slot.saveId, slot.profileId));
            }
        }

        private void CreateEmptySaveSlotUI(int slotIndex)
        {
            if (saveSlotPrefab == null || saveSlotContainer == null) return;

            GameObject slotObj = Instantiate(saveSlotPrefab, saveSlotContainer);
            
            TMP_Text slotText = slotObj.GetComponentInChildren<TMP_Text>();
            if (slotText != null)
            {
                slotText.text = $"Empty Slot {slotIndex}";
            }

            Button saveButton = slotObj.GetComponent<Button>();
            if (saveButton != null)
            {
                saveButton.onClick.AddListener(() => SaveToSlot(slotIndex));
            }
        }

        /// <summary>
        /// Saves to the specified slot.
        /// </summary>
        private void SaveToSlot(int slotIndex)
        {
            string customName = saveNameInput?.text ?? "";
            currentSlotIndex = slotIndex;

            if (SaveGameManager.Instance != null)
            {
                ShowProgressPanel("Saving...");
                SaveGameManager.Instance.SaveGame(SaveSlotType.ManualSave, slotIndex, customName);
            }
        }

        /// <summary>
        /// Loads the specified save.
        /// </summary>
        private void LoadSave(string saveId, string profileId)
        {
            if (SaveGameManager.Instance != null)
            {
                ShowProgressPanel("Loading...");
                SaveGameManager.Instance.LoadGame(saveId, profileId);
            }
        }

        /// <summary>
        /// Populates the profile list.
        /// </summary>
        private void PopulateProfileList()
        {
            if (profileListContainer == null) return;

            // Clear existing profile items
            foreach (Transform child in profileListContainer)
            {
                Destroy(child.gameObject);
            }

            if (ProfileManager.Instance == null) return;

            List<ProfileData> profiles = ProfileManager.Instance.AllProfiles;

            foreach (ProfileData profile in profiles)
            {
                CreateProfileItemUI(profile);
            }
        }

        private void CreateProfileItemUI(ProfileData profile)
        {
            if (profileItemPrefab == null || profileListContainer == null) return;

            GameObject profileObj = Instantiate(profileItemPrefab, profileListContainer);
            
            TMP_Text profileText = profileObj.GetComponentInChildren<TMP_Text>();
            if (profileText != null)
            {
                string lastPlayed = System.DateTime.Parse(profile.lastPlayedDate).ToLocalTime().ToString("g");
                string playtime = System.TimeSpan.FromSeconds(profile.totalPlaytime).ToString(@"hh\:mm\:ss");
                
                profileText.text = $"{profile.profileName}\n" +
                                  $"Last Played: {lastPlayed}\n" +
                                  $"Playtime: {playtime}\n" +
                                  $"Missions: {profile.totalMissionsCompleted}";
            }

            Button selectButton = profileObj.GetComponent<Button>();
            if (selectButton != null)
            {
                selectButton.onClick.AddListener(() => SelectProfile(profile.profileId));
            }
        }

        /// <summary>
        /// Creates a new profile.
        /// </summary>
        public void CreateNewProfile()
        {
            if (ProfileManager.Instance == null) return;

            string profileName = profileNameInput?.text ?? "Player";
            int avatarIcon = avatarDropdown?.value ?? 0;
            string difficulty = difficultyDropdown?.options[difficultyDropdown.value].text ?? "Normal";

            ProfileData profile = ProfileManager.Instance.CreateProfile(profileName, avatarIcon, difficulty);
            
            if (profile != null)
            {
                ShowNotification($"Profile created: {profileName}");
                PopulateProfileList();
            }
            else
            {
                ShowNotification("Failed to create profile");
            }
        }

        /// <summary>
        /// Selects a profile and switches to it.
        /// </summary>
        private void SelectProfile(string profileId)
        {
            if (ProfileManager.Instance != null)
            {
                if (ProfileManager.Instance.SwitchToProfile(profileId))
                {
                    ShowNotification($"Switched to profile: {ProfileManager.Instance.CurrentProfile.profileName}");
                    HideAllMenus();
                }
            }
        }

        /// <summary>
        /// Shows the progress panel with a message.
        /// </summary>
        private void ShowProgressPanel(string message)
        {
            if (progressPanel != null)
            {
                progressPanel.SetActive(true);
            }

            if (statusText != null)
            {
                statusText.text = message;
            }

            if (progressBar != null)
            {
                progressBar.value = 0f;
            }
        }

        /// <summary>
        /// Updates the progress bar.
        /// </summary>
        private void UpdateProgressBar(float progress)
        {
            if (progressBar != null)
            {
                progressBar.value = progress;
            }
        }

        private void OnSaveCompleted(SaveData saveData)
        {
            ShowNotification($"Game saved to {saveData.saveSlotType} {saveData.slotIndex}");
            progressPanel?.SetActive(false);
            HideAllMenus();
        }

        private void OnLoadCompleted(SaveData saveData)
        {
            ShowNotification($"Game loaded: {saveData.locationName}");
            progressPanel?.SetActive(false);
            HideAllMenus();
        }

        private void OnSaveFailed(string error)
        {
            ShowNotification($"Save failed: {error}");
            progressPanel?.SetActive(false);
        }

        private void OnLoadFailed(string error)
        {
            ShowNotification($"Load failed: {error}");
            progressPanel?.SetActive(false);
        }

        private void OnAutoSaveTriggered(string reason)
        {
            ShowNotification($"Auto-saving... ({reason})", 1f);
        }

        private void ShowNotification(string message, float duration = 3f)
        {
            if (NotificationManager.Instance != null)
            {
                NotificationManager.Instance.ShowNotification(message);
            }
            else
            {
                Debug.Log($"[Notification] {message}");
            }
        }
    }
}
