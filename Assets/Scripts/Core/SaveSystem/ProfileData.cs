using System;
using System.Collections.Generic;

namespace ProtocolEMR.Core.SaveSystem
{
    /// <summary>
    /// Player profile data structure.
    /// Contains profile metadata, cumulative statistics, and settings.
    /// Each profile can have multiple save files.
    /// </summary>
    [Serializable]
    public class ProfileData
    {
        public string profileId;
        public string profileName;
        public string creationDate;
        public string lastPlayedDate;
        public float totalPlaytime;
        public string currentSaveSlotId;
        
        // Profile settings
        public int avatarIcon;
        public string difficulty;
        
        // Progress summary
        public int highestMissionCompleted;
        public int totalMissionsCompleted;
        public int totalAchievements;
        
        // Cumulative statistics
        public CumulativeStats cumulativeStats;
        
        // Metadata
        public string version;
        public string checksum;

        public ProfileData()
        {
            profileId = Guid.NewGuid().ToString();
            profileName = "Player";
            creationDate = DateTime.UtcNow.ToString("o");
            lastPlayedDate = DateTime.UtcNow.ToString("o");
            totalPlaytime = 0f;
            currentSaveSlotId = "";
            
            avatarIcon = 0;
            difficulty = "Normal";
            
            highestMissionCompleted = 0;
            totalMissionsCompleted = 0;
            totalAchievements = 0;
            
            cumulativeStats = new CumulativeStats();
            
            version = "1.0.0";
            checksum = "";
        }
    }

    [Serializable]
    public class CumulativeStats
    {
        public float totalPlaytime;
        public int totalDeaths;
        public int totalNPCsEncountered;
        public int totalCollectiblesFound;
        public int totalCombatEncounters;
        public int totalPuzzlesSolved;
        public int totalSecretsFound;
        public int totalMissionsCompleted;

        public CumulativeStats()
        {
            totalPlaytime = 0f;
            totalDeaths = 0;
            totalNPCsEncountered = 0;
            totalCollectiblesFound = 0;
            totalCombatEncounters = 0;
            totalPuzzlesSolved = 0;
            totalSecretsFound = 0;
            totalMissionsCompleted = 0;
        }
    }

    /// <summary>
    /// Save slot metadata for display in save/load menus.
    /// </summary>
    [Serializable]
    public class SaveSlotMetadata
    {
        public string saveId;
        public string profileId;
        public SaveSlotType slotType;
        public int slotIndex;
        public string timestamp;
        public string locationName;
        public string missionName;
        public float playtime;
        public string difficulty;
        public string screenshotPath;

        public SaveSlotMetadata()
        {
            saveId = "";
            profileId = "";
            slotType = SaveSlotType.ManualSave;
            slotIndex = 0;
            timestamp = "";
            locationName = "";
            missionName = "";
            playtime = 0f;
            difficulty = "Normal";
            screenshotPath = "";
        }
    }
}
