using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace ProtocolEMR.Core.SaveSystem
{
    /// <summary>
    /// Manages player profiles including creation, switching, deletion, and persistence.
    /// Supports up to 10 profiles per user with local storage.
    /// Profiles are stored as JSON files in the persistent data path.
    /// </summary>
    public class ProfileManager : MonoBehaviour
    {
        public static ProfileManager Instance { get; private set; }

        private const int MAX_PROFILES = 10;
        private const int RECENT_PROFILES_COUNT = 3;

        private ProfileData currentProfile;
        private List<ProfileData> allProfiles;
        private string profilesDirectory;

        public ProfileData CurrentProfile => currentProfile;
        public List<ProfileData> AllProfiles => allProfiles;

        public event Action<ProfileData> OnProfileCreated;
        public event Action<ProfileData> OnProfileSwitched;
        public event Action<string> OnProfileDeleted;
        public event Action<ProfileData> OnProfileUpdated;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            profilesDirectory = Path.Combine(Application.persistentDataPath, "Profiles");
            Directory.CreateDirectory(profilesDirectory);

            allProfiles = new List<ProfileData>();
            LoadAllProfiles();
        }

        /// <summary>
        /// Creates a new player profile with the specified name and settings.
        /// </summary>
        public ProfileData CreateProfile(string profileName, int avatarIcon = 0, string difficulty = "Normal")
        {
            if (allProfiles.Count >= MAX_PROFILES)
            {
                Debug.LogError($"Cannot create profile: Maximum of {MAX_PROFILES} profiles reached");
                return null;
            }

            if (string.IsNullOrWhiteSpace(profileName))
            {
                Debug.LogError("Cannot create profile: Profile name cannot be empty");
                return null;
            }

            if (profileName.Length > 20)
            {
                profileName = profileName.Substring(0, 20);
            }

            if (ProfileNameExists(profileName))
            {
                Debug.LogError($"Cannot create profile: Profile name '{profileName}' already exists");
                return null;
            }

            ProfileData newProfile = new ProfileData
            {
                profileName = profileName,
                avatarIcon = avatarIcon,
                difficulty = difficulty
            };

            SaveProfile(newProfile);
            allProfiles.Add(newProfile);

            OnProfileCreated?.Invoke(newProfile);
            Debug.Log($"Profile created: {profileName} (ID: {newProfile.profileId})");

            return newProfile;
        }

        /// <summary>
        /// Switches to the specified profile, making it the current active profile.
        /// </summary>
        public bool SwitchToProfile(string profileId)
        {
            ProfileData profile = allProfiles.FirstOrDefault(p => p.profileId == profileId);
            
            if (profile == null)
            {
                Debug.LogError($"Cannot switch to profile: Profile ID {profileId} not found");
                return false;
            }

            currentProfile = profile;
            currentProfile.lastPlayedDate = DateTime.UtcNow.ToString("o");
            SaveProfile(currentProfile);

            OnProfileSwitched?.Invoke(currentProfile);
            Debug.Log($"Switched to profile: {currentProfile.profileName}");

            return true;
        }

        /// <summary>
        /// Deletes the specified profile and all associated save files.
        /// </summary>
        public bool DeleteProfile(string profileId)
        {
            ProfileData profile = allProfiles.FirstOrDefault(p => p.profileId == profileId);
            
            if (profile == null)
            {
                Debug.LogError($"Cannot delete profile: Profile ID {profileId} not found");
                return false;
            }

            if (currentProfile != null && currentProfile.profileId == profileId)
            {
                currentProfile = null;
            }

            string profilePath = GetProfilePath(profileId);
            if (File.Exists(profilePath))
            {
                File.Delete(profilePath);
            }

            if (SaveGameManager.Instance != null)
            {
                SaveGameManager.Instance.DeleteAllSavesForProfile(profileId);
            }

            allProfiles.Remove(profile);

            OnProfileDeleted?.Invoke(profileId);
            Debug.Log($"Profile deleted: {profile.profileName}");

            return true;
        }

        /// <summary>
        /// Updates the current profile with new data and saves it.
        /// </summary>
        public void UpdateCurrentProfile(ProfileData updatedProfile)
        {
            if (currentProfile == null)
            {
                Debug.LogError("Cannot update profile: No current profile");
                return;
            }

            currentProfile = updatedProfile;
            SaveProfile(currentProfile);

            OnProfileUpdated?.Invoke(currentProfile);
        }

        /// <summary>
        /// Gets the most recently played profiles for quick access.
        /// </summary>
        public List<ProfileData> GetRecentProfiles()
        {
            return allProfiles
                .OrderByDescending(p => DateTime.Parse(p.lastPlayedDate))
                .Take(RECENT_PROFILES_COUNT)
                .ToList();
        }

        /// <summary>
        /// Saves a profile to disk.
        /// </summary>
        private void SaveProfile(ProfileData profile)
        {
            try
            {
                profile.checksum = SaveFileValidator.CalculateChecksum(JsonUtility.ToJson(profile, false));
                
                string json = JsonUtility.ToJson(profile, true);
                string path = GetProfilePath(profile.profileId);
                File.WriteAllText(path, json);
                
                Debug.Log($"Profile saved: {profile.profileName}");
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to save profile: {e.Message}");
            }
        }

        /// <summary>
        /// Loads all profiles from disk.
        /// </summary>
        private void LoadAllProfiles()
        {
            allProfiles.Clear();

            try
            {
                string[] profileFiles = Directory.GetFiles(profilesDirectory, "profile_*.json");
                
                foreach (string filePath in profileFiles)
                {
                    try
                    {
                        string json = File.ReadAllText(filePath);
                        ProfileData profile = JsonUtility.FromJson<ProfileData>(json);
                        
                        if (SaveFileValidator.ValidateChecksum(json, profile.checksum))
                        {
                            allProfiles.Add(profile);
                        }
                        else
                        {
                            Debug.LogWarning($"Profile checksum mismatch: {filePath}");
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"Failed to load profile from {filePath}: {e.Message}");
                    }
                }

                Debug.Log($"Loaded {allProfiles.Count} profiles");
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to load profiles: {e.Message}");
            }
        }

        /// <summary>
        /// Checks if a profile name already exists.
        /// </summary>
        private bool ProfileNameExists(string profileName)
        {
            return allProfiles.Any(p => p.profileName.Equals(profileName, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Gets the file path for a profile.
        /// </summary>
        private string GetProfilePath(string profileId)
        {
            return Path.Combine(profilesDirectory, $"profile_{profileId}.json");
        }

        /// <summary>
        /// Updates profile playtime and statistics.
        /// </summary>
        public void UpdateProfilePlaytime(float deltaTime)
        {
            if (currentProfile != null)
            {
                currentProfile.totalPlaytime += deltaTime;
                currentProfile.cumulativeStats.totalPlaytime += deltaTime;
            }
        }

        /// <summary>
        /// Updates profile statistics from save data.
        /// </summary>
        public void UpdateProfileStats(PlayerStatistics stats)
        {
            if (currentProfile == null) return;

            currentProfile.cumulativeStats.totalDeaths = stats.deaths;
            currentProfile.cumulativeStats.totalNPCsEncountered = stats.npcsEncountered;
            currentProfile.cumulativeStats.totalCollectiblesFound = stats.collectiblesFound;
            currentProfile.cumulativeStats.totalCombatEncounters = stats.combatEncounters;
            currentProfile.cumulativeStats.totalPuzzlesSolved = stats.puzzlesSolved;
            currentProfile.cumulativeStats.totalSecretsFound = stats.secretsFound;
            currentProfile.cumulativeStats.totalMissionsCompleted = stats.missionsCompleted;

            SaveProfile(currentProfile);
        }
    }
}
