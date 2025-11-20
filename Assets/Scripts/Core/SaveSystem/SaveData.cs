using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProtocolEMR.Core.SaveSystem
{
    /// <summary>
    /// Complete save file data structure for Protocol EMR.
    /// Contains all player state, world state, mission progress, and metadata.
    /// Serialized to JSON format with checksum validation.
    /// </summary>
    [Serializable]
    public class SaveData
    {
        // Metadata
        public string saveId;
        public string profileId;
        public SaveSlotType saveSlotType;
        public int slotIndex;
        public string timestamp;
        public string saveVersion;
        public string gameVersion;
        
        // Preview metadata
        public string locationName;
        public string currentMissionName;
        public float playtimeSeconds;
        public string screenshotPath;
        
        // Core game state
        public PlayerState playerState;
        public WorldState worldState;
        public MissionState missionState;
        public NPCState npcState;
        public UnlockedContent unlockedContent;
        public SessionSettings sessionSettings;
        
        // Integrity
        public string checksum;
        public bool compressed;
        public bool encrypted;

        public SaveData()
        {
            saveId = Guid.NewGuid().ToString();
            timestamp = DateTime.UtcNow.ToString("o");
            saveVersion = "1.0.0";
            gameVersion = Application.version;
            
            playerState = new PlayerState();
            worldState = new WorldState();
            missionState = new MissionState();
            npcState = new NPCState();
            unlockedContent = new UnlockedContent();
            sessionSettings = new SessionSettings();
        }
    }

    [Serializable]
    public class PlayerState
    {
        public Vector3Serializable position;
        public Vector3Serializable rotation;
        public float health;
        public float stamina;
        public string currentWeapon;
        public List<string> equippedItems;
        public InventoryState inventory;
        public PlayerStatistics statistics;

        public PlayerState()
        {
            position = new Vector3Serializable();
            rotation = new Vector3Serializable();
            health = 100f;
            stamina = 100f;
            currentWeapon = "";
            equippedItems = new List<string>();
            inventory = new InventoryState();
            statistics = new PlayerStatistics();
        }
    }

    [Serializable]
    public class InventoryState
    {
        public List<InventoryItem> items;
        public int capacity;
        public int currency;

        public InventoryState()
        {
            items = new List<InventoryItem>();
            capacity = 20;
            currency = 0;
        }
    }

    [Serializable]
    public class InventoryItem
    {
        public string itemId;
        public int quantity;
        public ItemMetadata metadata;

        public InventoryItem()
        {
            metadata = new ItemMetadata();
        }
    }

    [Serializable]
    public class ItemMetadata
    {
        public float durability;
        public int ammo;
        public string customization;

        public ItemMetadata()
        {
            durability = 1.0f;
            ammo = 0;
            customization = "";
        }
    }

    [Serializable]
    public class PlayerStatistics
    {
        public float totalPlaytime;
        public int missionsCompleted;
        public int npcsEncountered;
        public int combatEncounters;
        public int deaths;
        public int collectiblesFound;
        public int puzzlesSolved;
        public int stealthKills;
        public int alertsTriggered;
        public int secretsFound;

        public PlayerStatistics()
        {
            totalPlaytime = 0f;
            missionsCompleted = 0;
            npcsEncountered = 0;
            combatEncounters = 0;
            deaths = 0;
            collectiblesFound = 0;
            puzzlesSolved = 0;
            stealthKills = 0;
            alertsTriggered = 0;
            secretsFound = 0;
        }
    }

    [Serializable]
    public class WorldState
    {
        public string currentScene;
        public int seed;
        public List<ObjectState> objectStates;
        public List<DoorState> doorStates;
        public List<string> collectiblesFound;
        public EnvironmentState environmentState;
        public List<string> locationsDiscovered;
        public List<PuzzleState> puzzlesSolved;
        public List<WorldEvent> worldEvents;

        public WorldState()
        {
            currentScene = "";
            seed = 0;
            objectStates = new List<ObjectState>();
            doorStates = new List<DoorState>();
            collectiblesFound = new List<string>();
            environmentState = new EnvironmentState();
            locationsDiscovered = new List<string>();
            puzzlesSolved = new List<PuzzleState>();
            worldEvents = new List<WorldEvent>();
        }
    }

    [Serializable]
    public class ObjectState
    {
        public string objectId;
        public string state;
        public string customData;
    }

    [Serializable]
    public class DoorState
    {
        public string doorId;
        public bool isLocked;
        public bool isOpen;
        public string keyRequired;
    }

    [Serializable]
    public class EnvironmentState
    {
        public float timeOfDay;
        public string lightingState;
        public List<string> hazardsActive;

        public EnvironmentState()
        {
            timeOfDay = 12f;
            lightingState = "Normal";
            hazardsActive = new List<string>();
        }
    }

    [Serializable]
    public class PuzzleState
    {
        public string puzzleId;
        public string solvedTimestamp;
        public int attempts;
        public float solutionTime;
    }

    [Serializable]
    public class WorldEvent
    {
        public string eventId;
        public string timestamp;
        public bool playerTriggered;
    }

    [Serializable]
    public class MissionState
    {
        public List<ActiveMission> activeMissions;
        public List<string> completedMissions;
        public List<string> availableMissions;
        public List<string> failedMissions;
        public Dictionary<string, string> missionChoices;

        public MissionState()
        {
            activeMissions = new List<ActiveMission>();
            completedMissions = new List<string>();
            availableMissions = new List<string>();
            failedMissions = new List<string>();
            missionChoices = new Dictionary<string, string>();
        }
    }

    [Serializable]
    public class ActiveMission
    {
        public string missionId;
        public List<MissionObjective> objectives;
        public string startTimestamp;

        public ActiveMission()
        {
            objectives = new List<MissionObjective>();
            startTimestamp = DateTime.UtcNow.ToString("o");
        }
    }

    [Serializable]
    public class MissionObjective
    {
        public string objectiveId;
        public string status;
        public float progress;
    }

    [Serializable]
    public class NPCState
    {
        public List<EncounteredNPC> encounteredNPCs;
        public float globalHostilityLevel;
        public AlertState alertStates;

        public NPCState()
        {
            encounteredNPCs = new List<EncounteredNPC>();
            globalHostilityLevel = 0f;
            alertStates = new AlertState();
        }
    }

    [Serializable]
    public class EncounteredNPC
    {
        public string npcId;
        public string status;
        public Vector3Serializable lastKnownPosition;
        public int relationshipLevel;
        public List<string> dialogueFlags;
        public string questGiverState;

        public EncounteredNPC()
        {
            lastKnownPosition = new Vector3Serializable();
            relationshipLevel = 0;
            dialogueFlags = new List<string>();
            questGiverState = "Available";
        }
    }

    [Serializable]
    public class AlertState
    {
        public string currentAlertLevel;
        public float alertTimer;

        public AlertState()
        {
            currentAlertLevel = "None";
            alertTimer = 0f;
        }
    }

    [Serializable]
    public class UnlockedContent
    {
        public List<string> cosmetics;
        public List<string> alternativePaths;
        public List<string> loreEntries;
        public List<string> abilities;

        public UnlockedContent()
        {
            cosmetics = new List<string>();
            alternativePaths = new List<string>();
            loreEntries = new List<string>();
            abilities = new List<string>();
        }
    }

    [Serializable]
    public class SessionSettings
    {
        public string difficulty;
        public bool activeHints;
        public bool subtitlesEnabled;

        public SessionSettings()
        {
            difficulty = "Normal";
            activeHints = true;
            subtitlesEnabled = true;
        }
    }

    [Serializable]
    public class Vector3Serializable
    {
        public float x;
        public float y;
        public float z;

        public Vector3Serializable()
        {
            x = 0f;
            y = 0f;
            z = 0f;
        }

        public Vector3Serializable(Vector3 vector)
        {
            x = vector.x;
            y = vector.y;
            z = vector.z;
        }

        public Vector3 ToVector3()
        {
            return new Vector3(x, y, z);
        }
    }

    public enum SaveSlotType
    {
        AutoSave,
        ManualSave,
        Checkpoint,
        QuickSave
    }
}
