using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using ProtocolEMR.Core.Procedural;
using System.IO;

namespace ProtocolEMR.Core.Editor
{
    /// <summary>
    /// Editor tools for authoring and validating procedural chunk definitions.
    /// </summary>
    public class ProceduralChunkEditorTools
    {
        private const string CHUNK_ASSET_PATH = "Assets/Resources/Procedural/Chunks/";
        private const string CHUNK_PREFAB_PATH = "Assets/Resources/Procedural/Prefabs/";

        [MenuItem("Protocol EMR/Procedural/Create New Chunk Definition")]
        public static void CreateNewChunkDefinition()
        {
            // Ensure directory exists
            EnsureDirectoryExists(CHUNK_ASSET_PATH);

            // Create a new chunk definition
            var chunkDef = ScriptableObject.CreateInstance<ProceduralChunkDefinition>();
            chunkDef.chunkId = "chunk_new_" + System.DateTime.Now.Ticks;
            chunkDef.displayName = "New Chunk";

            // Save the asset
            string assetPath = AssetDatabase.GenerateUniqueAssetPath(
                CHUNK_ASSET_PATH + "ChunkDef_" + chunkDef.chunkId + ".asset"
            );

            AssetDatabase.CreateAsset(chunkDef, assetPath);
            AssetDatabase.SaveAssets();
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = chunkDef;

            Debug.Log($"Created new chunk definition at: {assetPath}");
        }

        [MenuItem("Protocol EMR/Procedural/Validate All Chunks")]
        public static void ValidateAllChunks()
        {
            EnsureDirectoryExists(CHUNK_ASSET_PATH);

            var guids = AssetDatabase.FindAssets("t:ProceduralChunkDefinition", new[] { CHUNK_ASSET_PATH });
            int errorCount = 0;
            int warningCount = 0;

            Debug.Log($"Validating {guids.Length} chunk definitions...");

            foreach (var guid in guids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                var chunkDef = AssetDatabase.LoadAssetAtPath<ProceduralChunkDefinition>(assetPath);

                if (chunkDef == null)
                    continue;

                var errors = chunkDef.Validate();

                foreach (var error in errors)
                {
                    if (error.StartsWith("Warning:"))
                    {
                        Debug.LogWarning($"[{chunkDef.chunkId}] {error}", chunkDef);
                        warningCount++;
                    }
                    else
                    {
                        Debug.LogError($"[{chunkDef.chunkId}] {error}", chunkDef);
                        errorCount++;
                    }
                }
            }

            Debug.Log($"Validation complete: {errorCount} errors, {warningCount} warnings");
        }

        [MenuItem("Protocol EMR/Procedural/Generate Test Chunks")]
        public static void GenerateTestChunks()
        {
            EnsureDirectoryExists(CHUNK_ASSET_PATH);

            // Create Corridor chunk
            var corridorChunk = ScriptableObject.CreateInstance<ProceduralChunkDefinition>();
            corridorChunk.chunkId = "chunk_corridor";
            corridorChunk.displayName = "Corridor";
            corridorChunk.chunkSize = new Vector3(10f, 3f, 20f);
            corridorChunk.biomeType = "interior";
            corridorChunk.minLevel = 1;
            corridorChunk.maxLevel = 100;
            corridorChunk.selectionWeight = 0.8f;

            // Add connectors
            corridorChunk.connectors = new ChunkConnector[]
            {
                new ChunkConnector
                {
                    connectorTag = "corridor_start",
                    localPosition = new Vector3(0, 0, -10f),
                    connectionSize = new Vector3(10f, 3f, 2f),
                    allowInbound = true,
                    allowOutbound = false
                },
                new ChunkConnector
                {
                    connectorTag = "corridor_end",
                    localPosition = new Vector3(0, 0, 10f),
                    connectionSize = new Vector3(10f, 3f, 2f),
                    allowInbound = false,
                    allowOutbound = true
                }
            };

            // Add NPC zone
            corridorChunk.npcZoneTemplates = new ChunkNPCZoneTemplate[]
            {
                new ChunkNPCZoneTemplate
                {
                    zoneName = "patrol",
                    centerOffset = Vector3.zero,
                    zoneSize = new Vector3(8f, 2f, 18f),
                    minNPCs = 1,
                    maxNPCs = 2,
                    allowedTypes = new NPCType[] { NPCType.Patrol }
                }
            };

            SaveChunkDefinition(corridorChunk, "ChunkDef_Corridor");

            // Create Room chunk
            var roomChunk = ScriptableObject.CreateInstance<ProceduralChunkDefinition>();
            roomChunk.chunkId = "chunk_room";
            roomChunk.displayName = "Room";
            roomChunk.chunkSize = new Vector3(20f, 4f, 20f);
            roomChunk.biomeType = "interior";
            roomChunk.minLevel = 1;
            roomChunk.maxLevel = 100;
            roomChunk.selectionWeight = 0.6f;

            // Add connectors (room can connect from any side)
            roomChunk.connectors = new ChunkConnector[]
            {
                new ChunkConnector
                {
                    connectorTag = "room_north",
                    localPosition = new Vector3(0, 0, -10f),
                    connectionSize = new Vector3(10f, 3f, 2f)
                },
                new ChunkConnector
                {
                    connectorTag = "room_south",
                    localPosition = new Vector3(0, 0, 10f),
                    connectionSize = new Vector3(10f, 3f, 2f)
                },
                new ChunkConnector
                {
                    connectorTag = "room_east",
                    localPosition = new Vector3(10f, 0, 0),
                    connectionSize = new Vector3(2f, 3f, 10f)
                },
                new ChunkConnector
                {
                    connectorTag = "room_west",
                    localPosition = new Vector3(-10f, 0, 0),
                    connectionSize = new Vector3(2f, 3f, 10f)
                }
            };

            // Add NPC zones
            roomChunk.npcZoneTemplates = new ChunkNPCZoneTemplate[]
            {
                new ChunkNPCZoneTemplate
                {
                    zoneName = "guard_zone",
                    centerOffset = new Vector3(0, 0, 0),
                    zoneSize = new Vector3(15f, 3f, 15f),
                    minNPCs = 2,
                    maxNPCs = 4,
                    allowedTypes = new NPCType[] { NPCType.Guard, NPCType.Elite }
                }
            };

            SaveChunkDefinition(roomChunk, "ChunkDef_Room");

            // Create Vault chunk (terminal, no exit connectors)
            var vaultChunk = ScriptableObject.CreateInstance<ProceduralChunkDefinition>();
            vaultChunk.chunkId = "chunk_vault";
            vaultChunk.displayName = "Vault";
            vaultChunk.chunkSize = new Vector3(15f, 5f, 15f);
            vaultChunk.biomeType = "interior";
            vaultChunk.minLevel = 1;
            vaultChunk.maxLevel = 100;
            vaultChunk.selectionWeight = 0.3f;
            vaultChunk.allowDuplicates = false;

            // Only entrance connector
            vaultChunk.connectors = new ChunkConnector[]
            {
                new ChunkConnector
                {
                    connectorTag = "vault_entrance",
                    localPosition = new Vector3(0, 0, -7.5f),
                    connectionSize = new Vector3(10f, 4f, 2f),
                    allowInbound = true,
                    allowOutbound = false
                }
            };

            // Add objective zone
            vaultChunk.npcZoneTemplates = new ChunkNPCZoneTemplate[]
            {
                new ChunkNPCZoneTemplate
                {
                    zoneName = "elite_guard",
                    centerOffset = Vector3.zero,
                    zoneSize = new Vector3(12f, 4f, 12f),
                    minNPCs = 2,
                    maxNPCs = 3,
                    allowedTypes = new NPCType[] { NPCType.Elite },
                    difficultyMultiplier = 1.5f
                }
            };

            SaveChunkDefinition(vaultChunk, "ChunkDef_Vault");

            Debug.Log("Generated 3 test chunk definitions");
        }

        private static void SaveChunkDefinition(ProceduralChunkDefinition chunkDef, string fileName)
        {
            string assetPath = AssetDatabase.GenerateUniqueAssetPath(
                CHUNK_ASSET_PATH + fileName + ".asset"
            );

            AssetDatabase.CreateAsset(chunkDef, assetPath);
            AssetDatabase.SaveAssets();
        }

        private static void EnsureDirectoryExists(string path)
        {
            if (!AssetDatabase.IsValidFolder(path))
            {
                string parentPath = Path.GetDirectoryName(path.TrimEnd('/'));
                string folderName = Path.GetFileName(path.TrimEnd('/'));

                if (!AssetDatabase.IsValidFolder(parentPath))
                {
                    EnsureDirectoryExists(parentPath);
                }

                AssetDatabase.CreateFolder(parentPath, folderName);
            }
        }

        /// <summary>
        /// Stress test: generate chunk sequences from 100 different seeds.
        /// </summary>
        [MenuItem("Protocol EMR/Procedural/Stress Test Chunk Generation")]
        public static void StressTestChunkGeneration()
        {
            Debug.Log("Starting stress test of chunk generation...");

            var guids = AssetDatabase.FindAssets("t:ProceduralChunkDefinition", new[] { CHUNK_ASSET_PATH });
            
            if (guids.Length == 0)
            {
                Debug.LogWarning("No chunk definitions found. Run 'Generate Test Chunks' first.");
                return;
            }

            var availableChunks = new ProceduralChunkDefinition[guids.Length];
            for (int i = 0; i < guids.Length; i++)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guids[i]);
                availableChunks[i] = AssetDatabase.LoadAssetAtPath<ProceduralChunkDefinition>(assetPath);
            }

            int successCount = 0;
            int failureCount = 0;
            Dictionary<string, int> chunkUsage = new Dictionary<string, int>();

            for (int seed = 1; seed <= 100; seed++)
            {
                try
                {
                    var random = new System.Random(seed);
                    var sequence = new List<ProceduralChunkDefinition>();

                    // Generate a sequence of 8 chunks
                    for (int i = 0; i < 8; i++)
                    {
                        int chunkIndex = random.Next(availableChunks.Length);
                        ProceduralChunkDefinition chunk = availableChunks[chunkIndex];

                        float weight = (float)random.NextDouble();
                        if (weight <= chunk.selectionWeight)
                        {
                            sequence.Add(chunk);

                            // Track usage
                            if (!chunkUsage.ContainsKey(chunk.chunkId))
                                chunkUsage[chunk.chunkId] = 0;
                            chunkUsage[chunk.chunkId]++;
                        }
                    }

                    if (sequence.Count > 0)
                        successCount++;
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"Seed {seed} failed: {ex.Message}");
                    failureCount++;
                }
            }

            var sb = new System.Text.StringBuilder();
            sb.AppendLine("=== STRESS TEST RESULTS ===");
            sb.AppendLine($"Successful sequences: {successCount}/100");
            sb.AppendLine($"Failed sequences: {failureCount}/100");
            sb.AppendLine("\nChunk usage distribution:");

            foreach (var kvp in chunkUsage)
            {
                sb.AppendLine($"  {kvp.Key}: {kvp.Value} times");
            }

            Debug.Log(sb.ToString());
        }
    }
}
