using UnityEngine;
using System.IO;
using ProtocolEMR.Systems;

namespace ProtocolEMR.Core.Procedural
{
    /// <summary>
    /// Lightweight procedural state store for saving and loading seed data.
    /// Complements the main ObjectStateManager by handling procedural generation state.
    /// </summary>
    public class ProceduralStateStore : MonoBehaviour
    {
        public static ProceduralStateStore Instance { get; private set; }

        [Header("State Store Configuration")]
        [SerializeField] private string proceduralStateFileName = "procedural_state.json";
        [SerializeField] private bool autoSaveOnQuit = true;

        private string savePath;
        private ProceduralStateData currentState;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            savePath = Path.Combine(Application.persistentDataPath, proceduralStateFileName);
            currentState = new ProceduralStateData();

            Debug.Log("ProceduralStateStore initialized");
        }

        private void Start()
        {
            LoadProceduralState();
        }

        private void OnApplicationQuit()
        {
            if (autoSaveOnQuit)
            {
                SaveProceduralState();
            }
        }

        /// <summary>
        /// Saves the current procedural state from SeedManager.
        /// </summary>
        public void SaveProceduralState()
        {
            if (SeedManager.Instance == null)
            {
                Debug.LogWarning("SeedManager not available for saving procedural state");
                return;
            }

            try
            {
                // Capture current state from SeedManager
                currentState.seed = SeedManager.Instance.CurrentSeed;
                currentState.timestamp = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                // Capture scope offsets
                currentState.scopeOffsets.Clear();
                string[] scopes = { 
                    SeedManager.SCOPE_CHUNKS,
                    SeedManager.SCOPE_ENCOUNTERS,
                    SeedManager.SCOPE_AUDIO,
                    SeedManager.SCOPE_NPCS,
                    SeedManager.SCOPE_STORY,
                    SeedManager.SCOPE_LOOT,
                    SeedManager.SCOPE_ENVIRONMENT
                };

                foreach (string scope in scopes)
                {
                    currentState.scopeOffsets.Add(new ProceduralScopeOffset
                    {
                        scope = scope,
                        offset = SeedManager.Instance.GetScopeOffset(scope)
                    });
                }

                // Save to file
                string json = JsonUtility.ToJson(currentState, true);
                File.WriteAllText(savePath, json);

                Debug.Log($"Procedural state saved to: {savePath}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to save procedural state: {e.Message}");
            }
        }

        /// <summary>
        /// Loads procedural state and applies it to SeedManager.
        /// </summary>
        public void LoadProceduralState()
        {
            if (!File.Exists(savePath))
            {
                Debug.Log($"No procedural state file found at: {savePath}");
                return;
            }

            try
            {
                string json = File.ReadAllText(savePath);
                currentState = JsonUtility.FromJson<ProceduralStateData>(json);

                // Apply state to SeedManager if available
                if (SeedManager.Instance != null)
                {
                    SeedManager.Instance.SetSeed(currentState.seed);

                    // Restore scope offsets
                    foreach (var offset in currentState.scopeOffsets)
                    {
                        SeedManager.Instance.SetScopeOffset(offset.scope, offset.offset);
                    }

                    Debug.Log($"Procedural state loaded from: {savePath}");
                    Debug.Log($"Restored seed: {currentState.seed} from {currentState.timestamp}");
                }
                else
                {
                    Debug.LogWarning("SeedManager not available for loading procedural state");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to load procedural state: {e.Message}");
            }
        }

        /// <summary>
        /// Deletes the procedural state file.
        /// </summary>
        public void DeleteProceduralState()
        {
            if (File.Exists(savePath))
            {
                File.Delete(savePath);
                Debug.Log($"Procedural state file deleted: {savePath}");
            }
        }

        /// <summary>
        /// Gets the current procedural state data.
        /// </summary>
        public ProceduralStateData GetCurrentState()
        {
            return currentState;
        }

        /// <summary>
        /// Checks if procedural state file exists.
        /// </summary>
        public bool HasProceduralState()
        {
            return File.Exists(savePath);
        }

        /// <summary>
        /// Forces an immediate save of the current procedural state.
        /// </summary>
        public void ForceSave()
        {
            SaveProceduralState();
        }

        /// <summary>
        /// Forces an immediate load of procedural state.
        /// </summary>
        public void ForceLoad()
        {
            LoadProceduralState();
        }

        /// <summary>
        /// Resets the procedural state to defaults.
        /// </summary>
        public void ResetProceduralState()
        {
            currentState = new ProceduralStateData();
            
            if (SeedManager.Instance != null)
            {
                SeedManager.Instance.ResetScopeOffsets();
            }
            
            DeleteProceduralState();
            Debug.Log("Procedural state reset to defaults");
        }

        /// <summary>
        /// Gets debug information about the procedural state.
        /// </summary>
        public string GetDebugInfo()
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("PROCEDURAL STATE DEBUG INFO");
            sb.AppendLine($"Save Path: {savePath}");
            sb.AppendLine($"File Exists: {HasProceduralState()}");
            sb.AppendLine($"Current Seed: {currentState.seed}");
            sb.AppendLine($"Timestamp: {currentState.timestamp}");
            sb.AppendLine($"Scope Offsets Count: {currentState.scopeOffsets.Count}");
            
            foreach (var offset in currentState.scopeOffsets)
            {
                sb.AppendLine($"  {offset.scope}: {offset.offset}");
            }
            
            return sb.ToString();
        }
    }

    [System.Serializable]
    public class ProceduralStateData
    {
        public int seed;
        public string timestamp;
        public System.Collections.Generic.List<ProceduralScopeOffset> scopeOffsets = 
            new System.Collections.Generic.List<ProceduralScopeOffset>();
    }

    [System.Serializable]
    public class ProceduralScopeOffset
    {
        public string scope;
        public int offset;
    }
}