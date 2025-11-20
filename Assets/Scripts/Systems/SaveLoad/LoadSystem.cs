using System;
using System.Collections;
using System.Collections.Generic;
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
    /// Helper functions for reading and applying save data.
    /// </summary>
    public static class LoadSystem
    {
        public static bool TryReadSave(string filePath, SaveSlotDescriptor descriptor, string encryptionSecret, AccountProfileData profile, out SaveData saveData, out string error)
        {
            saveData = null;
            error = string.Empty;

            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
            {
                error = "Save file not found";
                return false;
            }

            try
            {
                byte[] bytes = File.ReadAllBytes(filePath);
                byte[] payload = bytes;

                if (!string.IsNullOrEmpty(encryptionSecret))
                {
                    string profileId = profile?.profileId ?? SaveSystemConstants.DefaultProfileId;
                    string slotToken = descriptor?.GetFilePrefix() ?? SaveSlotType.Manual.ToString().ToLowerInvariant();
                    byte[] key = EncryptionUtility.DeriveKey(encryptionSecret, profileId);
                    byte[] iv = EncryptionUtility.DeriveIV(encryptionSecret, slotToken);
                    payload = EncryptionUtility.Decrypt(bytes, key, iv);
                }

                string json = Encoding.UTF8.GetString(payload);
                saveData = JsonUtility.FromJson<SaveData>(json);

                if (saveData == null)
                {
                    error = "Failed to deserialize save data";
                    return false;
                }

                string expectedHash = saveData.metadata?.checksum ?? string.Empty;
                string actualHash = EncryptionUtility.ComputeHash(Encoding.UTF8.GetBytes(JsonUtility.ToJson(saveData, true)));
                if (!string.IsNullOrEmpty(expectedHash) && !string.Equals(expectedHash, actualHash, StringComparison.OrdinalIgnoreCase))
                {
                    error = "Save checksum mismatch";
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return false;
            }
        }

        public static bool TryReadMetadata(string filePath, SaveSlotDescriptor descriptor, string encryptionSecret, AccountProfileData profile, out SaveMetadata metadata)
        {
            metadata = null;
            bool success = TryReadSave(filePath, descriptor, encryptionSecret, profile, out var data, out var readError);
            if (success && data != null)
            {
                metadata = data.metadata;
                return true;
            }

            if (!string.IsNullOrEmpty(readError))
            {
                Debug.LogWarning($"[LoadSystem] Metadata read failed for {filePath}: {readError}");
            }

            return false;
        }

        public static IEnumerator ApplySaveData(SaveData data, LoadOptions options)
        {
            if (data == null)
                yield break;

            if (options == null)
            {
                options = new LoadOptions();
            }

            if (options.reloadScene && !string.IsNullOrEmpty(data.metadata?.currentScene))
            {
                var currentScene = SceneManager.GetActiveScene();
                if (!string.Equals(currentScene.name, data.metadata.currentScene, StringComparison.Ordinal))
                {
                    AsyncOperation load = SceneManager.LoadSceneAsync(data.metadata.currentScene);
                    while (!load.isDone)
                    {
                        yield return null;
                    }
                }
            }

            WorldStateManager.ApplyWorldState(data.world);
            NPCStateSerializer.ApplyState(data.npcState);
            data.player?.Apply(options.playerOverride);
            RestoreDialogueState(data.dialogue);

            if (!string.IsNullOrEmpty(data.missionStateJson) && MissionSystem.Instance != null)
            {
                MissionSystem.Instance.DeserializeMissionState(data.missionStateJson);
            }
        }

        private static void RestoreDialogueState(DialogueStateSnapshot snapshot)
        {
            var manager = UnknownDialogueManager.Instance;
            if (manager == null || snapshot == null)
                return;

            var history = snapshot.history ?? new List<MessageHistory>();
            var cooldowns = new Dictionary<string, float>(StringComparer.Ordinal);
            if (snapshot.cooldowns != null)
            {
                foreach (var entry in snapshot.cooldowns)
                {
                    if (entry != null && !string.IsNullOrEmpty(entry.key))
                    {
                        cooldowns[entry.key] = entry.value;
                    }
                }
            }

            var profile = snapshot.playerStyleProfile != null
                ? JsonUtility.FromJson<PlayerStyleProfile>(JsonUtility.ToJson(snapshot.playerStyleProfile))
                : new PlayerStyleProfile();

            manager.RestoreDialogueState(history, cooldowns, snapshot.lastMessageTimestamp, profile);
        }
    }

    /// <summary>
    /// Options controlling how save data is applied.
    /// </summary>
    public class LoadOptions
    {
        public bool reloadScene = true;
        public GameObject playerOverride;
    }
}
