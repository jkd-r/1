using System;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace ProtocolEMR.Core.SaveSystem
{
    /// <summary>
    /// Validates save files using SHA-256 checksums and schema validation.
    /// Detects corruption and validates data integrity.
    /// </summary>
    public static class SaveFileValidator
    {
        /// <summary>
        /// Calculates SHA-256 checksum for the given data.
        /// </summary>
        public static string CalculateChecksum(string data)
        {
            try
            {
                using (SHA256 sha256 = SHA256.Create())
                {
                    byte[] bytes = Encoding.UTF8.GetBytes(data);
                    byte[] hash = sha256.ComputeHash(bytes);
                    return BitConverter.ToString(hash).Replace("-", "").ToLower();
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to calculate checksum: {e.Message}");
                return "";
            }
        }

        /// <summary>
        /// Validates that the checksum matches the data.
        /// </summary>
        public static bool ValidateChecksum(string data, string expectedChecksum)
        {
            if (string.IsNullOrEmpty(expectedChecksum))
            {
                return true;
            }

            string actualChecksum = CalculateChecksum(data);
            return actualChecksum.Equals(expectedChecksum, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Validates save data structure and logical consistency.
        /// </summary>
        public static bool ValidateSaveData(SaveData saveData, out string errorMessage)
        {
            errorMessage = "";

            if (saveData == null)
            {
                errorMessage = "Save data is null";
                return false;
            }

            if (string.IsNullOrEmpty(saveData.saveId))
            {
                errorMessage = "Save ID is missing";
                return false;
            }

            if (string.IsNullOrEmpty(saveData.profileId))
            {
                errorMessage = "Profile ID is missing";
                return false;
            }

            if (saveData.playerState == null)
            {
                errorMessage = "Player state is missing";
                return false;
            }

            if (saveData.playerState.health < 0 || saveData.playerState.health > 100)
            {
                errorMessage = $"Invalid player health: {saveData.playerState.health}";
                return false;
            }

            if (saveData.playerState.stamina < 0 || saveData.playerState.stamina > 100)
            {
                errorMessage = $"Invalid player stamina: {saveData.playerState.stamina}";
                return false;
            }

            if (saveData.playtimeSeconds < 0)
            {
                errorMessage = $"Invalid playtime: {saveData.playtimeSeconds}";
                return false;
            }

            return true;
        }

        /// <summary>
        /// Validates profile data structure.
        /// </summary>
        public static bool ValidateProfileData(ProfileData profileData, out string errorMessage)
        {
            errorMessage = "";

            if (profileData == null)
            {
                errorMessage = "Profile data is null";
                return false;
            }

            if (string.IsNullOrEmpty(profileData.profileId))
            {
                errorMessage = "Profile ID is missing";
                return false;
            }

            if (string.IsNullOrEmpty(profileData.profileName))
            {
                errorMessage = "Profile name is missing";
                return false;
            }

            if (profileData.totalPlaytime < 0)
            {
                errorMessage = $"Invalid total playtime: {profileData.totalPlaytime}";
                return false;
            }

            return true;
        }

        /// <summary>
        /// Attempts to repair corrupted save data with default values.
        /// </summary>
        public static SaveData RepairSaveData(SaveData corruptedData)
        {
            if (corruptedData == null)
            {
                return new SaveData();
            }

            if (corruptedData.playerState == null)
            {
                corruptedData.playerState = new PlayerState();
            }

            if (corruptedData.worldState == null)
            {
                corruptedData.worldState = new WorldState();
            }

            if (corruptedData.missionState == null)
            {
                corruptedData.missionState = new MissionState();
            }

            if (corruptedData.npcState == null)
            {
                corruptedData.npcState = new NPCState();
            }

            if (corruptedData.unlockedContent == null)
            {
                corruptedData.unlockedContent = new UnlockedContent();
            }

            if (corruptedData.sessionSettings == null)
            {
                corruptedData.sessionSettings = new SessionSettings();
            }

            if (corruptedData.playerState.health < 0 || corruptedData.playerState.health > 100)
            {
                corruptedData.playerState.health = 100f;
            }

            if (corruptedData.playerState.stamina < 0 || corruptedData.playerState.stamina > 100)
            {
                corruptedData.playerState.stamina = 100f;
            }

            Debug.LogWarning("Save data was repaired with default values");
            return corruptedData;
        }

        /// <summary>
        /// Checks if save version is compatible with current game version.
        /// </summary>
        public static bool IsVersionCompatible(string saveVersion, string currentVersion)
        {
            if (string.IsNullOrEmpty(saveVersion) || string.IsNullOrEmpty(currentVersion))
            {
                return true;
            }

            try
            {
                Version saveVer = Version.Parse(saveVersion);
                Version currentVer = Version.Parse(currentVersion);

                return saveVer.Major == currentVer.Major && saveVer.Minor <= currentVer.Minor;
            }
            catch
            {
                return true;
            }
        }
    }
}
