using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;

namespace ProtocolEMR.Core.Procedural
{
    /// <summary>
    /// Centralized procedural seeding service for deterministic generation.
    /// Manages the run seed and provides typed sub-seeds for different systems.
    /// Ensures reproducible behavior for spaces, NPC population, and story beats.
    /// </summary>
    public class SeedManager : MonoBehaviour
    {
        public static SeedManager Instance { get; private set; }

        [Header("Seed Configuration")]
        [SerializeField] private bool useCustomSeed = false;
        [SerializeField] private int customSeed = 0;
        [SerializeField] private bool autoGenerateSeed = true;
        
        [Header("Debug")]
        [SerializeField] private bool logSeedUsage = false;
        [SerializeField] private KeyCode copySeedKey = KeyCode.F8;

        private int currentSeed;
        private Dictionary<string, int> scopeSeeds = new Dictionary<string, int>();
        private Dictionary<string, int> seedOffsets = new Dictionary<string, int>();
        private System.Random randomGenerator;
        private string savePath;

        // Well-known scope identifiers
        public const string SCOPE_CHUNKS = "chunks";
        public const string SCOPE_ENCOUNTERS = "encounters";
        public const string SCOPE_AUDIO = "audio";
        public const string SCOPE_NPCS = "npcs";
        public const string SCOPE_STORY = "story";
        public const string SCOPE_LOOT = "loot";
        public const string SCOPE_ENVIRONMENT = "environment";

        public int CurrentSeed => currentSeed;
        public System.Random RandomGenerator => randomGenerator;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            savePath = Path.Combine(Application.persistentDataPath, "procedural_seed.json");
            InitializeSeed();
        }

        private void Update()
        {
            if (Input.GetKeyDown(copySeedKey))
            {
                CopySeedToClipboard();
            }
        }

        /// <summary>
        /// Initializes the seed system.
        /// </summary>
        private void InitializeSeed()
        {
            if (useCustomSeed)
            {
                currentSeed = customSeed;
            }
            else if (File.Exists(savePath))
            {
                LoadSeed();
            }
            else if (autoGenerateSeed)
            {
                GenerateNewSeed();
            }
            else
            {
                currentSeed = (int)DateTime.Now.Ticks;
            }

            randomGenerator = new System.Random(currentSeed);
            InitializeScopeSeeds();

            Debug.Log($"SeedManager initialized with seed: {currentSeed}");
        }

        /// <summary>
        /// Initializes seeds for different scopes.
        /// </summary>
        private void InitializeScopeSeeds()
        {
            string[] scopes = { SCOPE_CHUNKS, SCOPE_ENCOUNTERS, SCOPE_AUDIO, SCOPE_NPCS, SCOPE_STORY, SCOPE_LOOT, SCOPE_ENVIRONMENT };
            
            foreach (string scope in scopes)
            {
                // Generate deterministic sub-seeds for each scope
                int scopeSeed = currentSeed;
                for (int i = 0; i < scope.Length; i++)
                {
                    scopeSeed = scopeSeed * 31 + scope[i];
                }
                
                scopeSeeds[scope] = scopeSeed;
                seedOffsets[scope] = 0;
            }
        }

        /// <summary>
        /// Generates a new random seed.
        /// </summary>
        public void GenerateNewSeed()
        {
            currentSeed = Mathf.Abs((int)(DateTime.Now.Ticks & 0x7FFFFFFF));
            randomGenerator = new System.Random(currentSeed);
            InitializeScopeSeeds();
            SaveSeed();
            
            Debug.Log($"Generated new seed: {currentSeed}");
        }

        /// <summary>
        /// Sets a specific seed and reinitializes the system.
        /// </summary>
        public void SetSeed(int seed)
        {
            currentSeed = Mathf.Abs(seed);
            randomGenerator = new System.Random(currentSeed);
            InitializeScopeSeeds();
            SaveSeed();
            
            Debug.Log($"Seed set to: {currentSeed}");
        }

        /// <summary>
        /// Gets a seed for a specific scope with optional offset.
        /// </summary>
        public int GetSeed(string scope, int offset = 0)
        {
            if (!scopeSeeds.ContainsKey(scope))
            {
                Debug.LogWarning($"Unknown scope: {scope}. Using global seed instead.");
                return currentSeed + offset;
            }

            int seed = scopeSeeds[scope] + seedOffsets[scope] + offset;
            
            if (logSeedUsage)
            {
                Debug.Log($"Seed requested for scope '{scope}' with offset {offset}: {seed}");
            }
            
            return seed;
        }

        /// <summary>
        /// Advances the offset for a specific scope.
        /// </summary>
        public void AdvanceScopeOffset(string scope, int advance = 1)
        {
            if (!seedOffsets.ContainsKey(scope))
            {
                seedOffsets[scope] = 0;
            }
            
            seedOffsets[scope] += advance;
            
            if (logSeedUsage)
            {
                Debug.Log($"Advanced scope '{scope}' offset to: {seedOffsets[scope]}");
            }
        }

        /// <summary>
        /// Gets a random number generator for a specific scope.
        /// </summary>
        public System.Random GetScopeRandom(string scope, int offset = 0)
        {
            int seed = GetSeed(scope, offset);
            return new System.Random(seed);
        }

        /// <summary>
        /// Gets a random float in range [0, 1] for a specific scope.
        /// </summary>
        public float GetRandomFloat(string scope, int offset = 0)
        {
            var rng = GetScopeRandom(scope, offset);
            return (float)rng.NextDouble();
        }

        /// <summary>
        /// Gets a random integer in range [min, max) for a specific scope.
        /// </summary>
        public int GetRandomInt(string scope, int min, int max, int offset = 0)
        {
            var rng = GetScopeRandom(scope, offset);
            return rng.Next(min, max);
        }

        /// <summary>
        /// Gets a random boolean for a specific scope.
        /// </summary>
        public bool GetRandomBool(string scope, int offset = 0)
        {
            var rng = GetScopeRandom(scope, offset);
            return rng.Next(0, 2) == 1;
        }

        /// <summary>
        /// Gets a random item from an array for a specific scope.
        /// </summary>
        public T GetRandomItem<T>(T[] array, string scope, int offset = 0)
        {
            if (array == null || array.Length == 0)
                return default(T);
            
            int index = GetRandomInt(scope, 0, array.Length, offset);
            return array[index];
        }

        /// <summary>
        /// Gets a random item from a list for a specific scope.
        /// </summary>
        public T GetRandomItem<T>(List<T> list, string scope, int offset = 0)
        {
            if (list == null || list.Count == 0)
                return default(T);
            
            int index = GetRandomInt(scope, 0, list.Count, offset);
            return list[index];
        }

        /// <summary>
        /// Copies the current seed to clipboard.
        /// </summary>
        public void CopySeedToClipboard()
        {
#if UNITY_EDITOR
            UnityEditor.EditorGUI.systemCopyBuffer = currentSeed.ToString();
            Debug.Log($"Seed {currentSeed} copied to clipboard");
#elif UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
            TextEditor te = new TextEditor();
            te.text = currentSeed.ToString();
            te.SelectAll();
            te.Copy();
            Debug.Log($"Seed {currentSeed} copied to clipboard");
#else
            Debug.Log($"Seed: {currentSeed} (clipboard not supported on this platform)");
#endif
        }

        /// <summary>
        /// Saves the current seed to file.
        /// </summary>
        public void SaveSeed()
        {
            try
            {
                var seedData = new SeedData
                {
                    seed = currentSeed,
                    scopeOffsets = new List<ScopeOffset>(seedOffsets.Count)
                };

                foreach (var kvp in seedOffsets)
                {
                    seedData.scopeOffsets.Add(new ScopeOffset
                    {
                        scope = kvp.Key,
                        offset = kvp.Value
                    });
                }

                string json = JsonUtility.ToJson(seedData, true);
                File.WriteAllText(savePath, json);

                Debug.Log($"Seed saved to: {savePath}");
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to save seed: {e.Message}");
            }
        }

        /// <summary>
        /// Loads the seed from file.
        /// </summary>
        public void LoadSeed()
        {
            try
            {
                if (!File.Exists(savePath))
                {
                    Debug.LogWarning($"Seed file not found: {savePath}");
                    return;
                }

                string json = File.ReadAllText(savePath);
                var seedData = JsonUtility.FromJson<SeedData>(json);

                currentSeed = seedData.seed;
                seedOffsets.Clear();

                foreach (var offset in seedData.scopeOffsets)
                {
                    seedOffsets[offset.scope] = offset.offset;
                }

                randomGenerator = new System.Random(currentSeed);
                InitializeScopeSeeds();

                // Restore offsets after initialization
                foreach (var kvp in seedOffsets)
                {
                    if (scopeSeeds.ContainsKey(kvp.Key))
                    {
                        seedOffsets[kvp.Key] = kvp.Value;
                    }
                }

                Debug.Log($"Seed loaded from: {savePath}");
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to load seed: {e.Message}");
                GenerateNewSeed();
            }
        }

        /// <summary>
        /// Resets all scope offsets to zero.
        /// </summary>
        public void ResetScopeOffsets()
        {
            foreach (string key in new List<string>(seedOffsets.Keys))
            {
                seedOffsets[key] = 0;
            }
            
            Debug.Log("All scope offsets reset to zero");
        }

        /// <summary>
        /// Gets the current offset for a scope.
        /// </summary>
        public int GetScopeOffset(string scope)
        {
            return seedOffsets.ContainsKey(scope) ? seedOffsets[scope] : 0;
        }

        /// <summary>
        /// Sets the offset for a scope.
        /// </summary>
        public void SetScopeOffset(string scope, int offset)
        {
            seedOffsets[scope] = offset;
            
            if (logSeedUsage)
            {
                Debug.Log($"Set scope '{scope}' offset to: {offset}");
            }
        }

        /// <summary>
        /// Gets debug information about the seed system.
        /// </summary>
        public string GetDebugInfo()
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"Current Seed: {currentSeed}");
            sb.AppendLine("Scope Offsets:");
            
            foreach (var kvp in seedOffsets)
            {
                sb.AppendLine($"  {kvp.Key}: {kvp.Value}");
            }
            
            return sb.ToString();
        }

        private void OnDestroy()
        {
            SaveSeed();
        }
    }

    [Serializable]
    public class SeedData
    {
        public int seed;
        public List<ScopeOffset> scopeOffsets = new List<ScopeOffset>();
    }

    [Serializable]
    public class ScopeOffset
    {
        public string scope;
        public int offset;
    }
}