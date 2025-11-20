using System;
using System.Collections.Generic;
using UnityEngine;
using ProtocolEMR.Core.Dialogue;

namespace ProtocolEMR.Systems.SaveLoad
{
    /// <summary>
    /// Complete snapshot of the game state for persistence.
    /// </summary>
    [Serializable]
    public class SaveData
    {
        public SaveMetadata metadata = new SaveMetadata();
        public AccountProfileData account = new AccountProfileData();
        public PlayerProgressionData player = new PlayerProgressionData();
        public NPCStateCollection npcState = new NPCStateCollection();
        public WorldStateSnapshot world = new WorldStateSnapshot();
        public DialogueStateSnapshot dialogue = new DialogueStateSnapshot();
        public string missionStateJson = string.Empty;
    }

    /// <summary>
    /// Metadata for a save slot.
    /// </summary>
    [Serializable]
    public class SaveMetadata
    {
        public string saveId = Guid.NewGuid().ToString();
        public string profileId = SaveSystemConstants.DefaultProfileId;
        public string profileName = "Default";
        public SaveSlotType slotType = SaveSlotType.Manual;
        public int slotIndex = 0;
        public string timestampUtc = DateTime.UtcNow.ToString("o");
        public string version = SaveSystemConstants.SaveFormatVersion;
        public string gameVersion = Application.version;
        public string currentScene = string.Empty;
        public string locationName = string.Empty;
        public float playtimeSeconds = 0f;
        public bool encrypted = true;
        public string checksum = string.Empty;

        public void Stamp(SaveSlotDescriptor descriptor, AccountProfileData profile)
        {
            if (descriptor != null)
            {
                slotType = descriptor.slotType;
                slotIndex = descriptor.slotIndex;
            }

            if (profile != null)
            {
                profileId = profile.profileId;
                profileName = profile.profileName;
            }

            timestampUtc = DateTime.UtcNow.ToString("o");
            version = SaveSystemConstants.SaveFormatVersion;
            gameVersion = Application.version;
        }
    }

    /// <summary>
    /// Descriptor for a save slot target.
    /// </summary>
    [Serializable]
    public class SaveSlotDescriptor
    {
        public SaveSlotType slotType = SaveSlotType.Manual;
        public int slotIndex = 0;
        public string customName = string.Empty;

        public SaveSlotDescriptor()
        {
        }

        public SaveSlotDescriptor(SaveSlotType type, int index, string label = "")
        {
            slotType = type;
            slotIndex = Mathf.Max(0, index);
            customName = label ?? string.Empty;
        }

        public string GetFilePrefix()
        {
            return $"{slotType.ToString().ToLowerInvariant()}_{Mathf.Max(0, slotIndex)}";
        }

        public static SaveSlotDescriptor FromFileName(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                return new SaveSlotDescriptor();
            }

            var trimmed = fileName.Replace(".sav", string.Empty);
            var parts = trimmed.Split('_');
            if (parts.Length < 2)
            {
                return new SaveSlotDescriptor();
            }

            if (!Enum.TryParse(parts[0], true, out SaveSlotType parsedType))
            {
                parsedType = SaveSlotType.Manual;
            }

            int parsedIndex = 0;
            int.TryParse(parts[1], out parsedIndex);

            return new SaveSlotDescriptor(parsedType, parsedIndex);
        }

        public bool Matches(SaveMetadata metadata)
        {
            if (metadata == null) return false;
            return metadata.slotType == slotType && metadata.slotIndex == slotIndex;
        }

        public override string ToString()
        {
            return $"{slotType} Slot #{slotIndex}";
        }
    }

    /// <summary>
    /// Accepted slot types for the sprint 6 save system.
    /// </summary>
    public enum SaveSlotType
    {
        Manual,
        Auto,
        Quick,
        Checkpoint
    }

    /// <summary>
    /// Account/profile level data that is stored alongside saves and in standalone profile files.
    /// </summary>
    [Serializable]
    public class AccountProfileData
    {
        public string profileId = SaveSystemConstants.DefaultProfileId;
        public string profileName = "Default Profile";
        public string avatarIcon = "Icon01";
        public string createdAtUtc = DateTime.UtcNow.ToString("o");
        public string lastPlayedAtUtc = DateTime.UtcNow.ToString("o");
        public float totalPlaytimeSeconds = 0f;
        public int highestMissionCompleted = 0;
        public int totalMissionsCompleted = 0;
        public int totalAchievementsUnlocked = 0;
        public int totalDeaths = 0;
        public int totalNPCsEncountered = 0;
        public int totalCollectiblesFound = 0;
        public int totalCombatEncounters = 0;
        public int totalPuzzlesSolved = 0;

        public void Touch()
        {
            if (string.IsNullOrEmpty(createdAtUtc))
            {
                createdAtUtc = DateTime.UtcNow.ToString("o");
            }

            lastPlayedAtUtc = DateTime.UtcNow.ToString("o");

            if (string.IsNullOrEmpty(profileId))
            {
                profileId = Guid.NewGuid().ToString();
            }
        }

        public AccountProfileData Clone()
        {
            return JsonUtility.FromJson<AccountProfileData>(JsonUtility.ToJson(this));
        }
    }

    /// <summary>
    /// Serializable vector helper for JSON saves.
    /// </summary>
    [Serializable]
    public struct SerializableVector3
    {
        public float x;
        public float y;
        public float z;

        public static SerializableVector3 FromVector3(Vector3 value)
        {
            return new SerializableVector3 { x = value.x, y = value.y, z = value.z };
        }

        public Vector3 ToVector3()
        {
            return new Vector3(x, y, z);
        }
    }

    /// <summary>
    /// Serializable quaternion helper for JSON saves.
    /// </summary>
    [Serializable]
    public struct SerializableQuaternion
    {
        public float x;
        public float y;
        public float z;
        public float w;

        public static SerializableQuaternion FromQuaternion(Quaternion value)
        {
            return new SerializableQuaternion { x = value.x, y = value.y, z = value.z, w = value.w };
        }

        public Quaternion ToQuaternion()
        {
            if (Mathf.Approximately(x, 0f) && Mathf.Approximately(y, 0f) && Mathf.Approximately(z, 0f) && Mathf.Approximately(w, 0f))
            {
                return Quaternion.identity;
            }

            return new Quaternion(x, y, z, w);
        }
    }

    /// <summary>
    /// Stores dialogue system state.
    /// </summary>
    [Serializable]
    public class DialogueStateSnapshot
    {
        public List<MessageHistory> history = new List<MessageHistory>();
        public List<NamedFloatValue> cooldowns = new List<NamedFloatValue>();
        public float lastMessageTimestamp;
        public PlayerStyleProfile playerStyleProfile;
    }

    /// <summary>
    /// Named float value used to serialize dictionaries.
    /// </summary>
    [Serializable]
    public class NamedFloatValue
    {
        public string key;
        public float value;
    }

    /// <summary>
    /// Constants shared across save/load implementation.
    /// </summary>
    public static class SaveSystemConstants
    {
        public const string DefaultProfileId = "default_profile";
        public const string SaveFormatVersion = "1.0.0"; // Sprint 6 baseline
    }
}
