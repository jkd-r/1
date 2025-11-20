using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.AI;

namespace ProtocolEMR.Core.Procedural
{
    /// <summary>
    /// Represents a chunk instance in the generated level.
    /// </summary>
    public class GeneratedChunkInstance
    {
        public ProceduralChunkDefinition definition;
        public GameObject instance;
        public Vector3 worldPosition;
        public Quaternion worldRotation;
        public NavMeshSurface navMeshSurface;
        public List<NPCSpawnZone> npcZones = new List<NPCSpawnZone>();
    }

    /// <summary>
    /// Runtime level assembly pipeline that consumes the seed service, stitches chunks,
    /// and provisions NPC spawn zones within performance budget.
    /// </summary>
    public class ProceduralLevelBuilder : MonoBehaviour
    {
        [Header("Chunk Configuration")]
        [SerializeField] private ProceduralChunkDefinition[] availableChunks;
        [SerializeField] private ProceduralChunkDefinition startingChunk;

        [Header("Generation Settings")]
        [SerializeField] private int targetChunkCount = 8;
        [SerializeField] private float maxGenerationTimeSeconds = 8f;
        [SerializeField] private bool bakeNavMeshAsync = true;
        [SerializeField] private int navMeshAgentTypeID = 0;

        [Header("NPC Integration")]
        [SerializeField] private NPCSpawner npcSpawner;
        [SerializeField] private float maxNPCsPerLevel = 30f;

        [Header("Debug")]
        [SerializeField] private bool logGenerationMetrics = true;
        [SerializeField] private bool drawDebugGizmos = false;

        private List<GeneratedChunkInstance> generatedChunks = new List<GeneratedChunkInstance>();
        private Transform chunksParent;
        private float generationStartTime;
        private bool isGenerating = false;
        private bool generationComplete = false;

        public List<GeneratedChunkInstance> GeneratedChunks => generatedChunks;
        public bool IsGenerating => isGenerating;
        public bool GenerationComplete => generationComplete;

        private void Start()
        {
            // Auto-generate level on scene load
            StartCoroutine(GenerateLevelCoroutine());
        }

        /// <summary>
        /// Generates a new procedural level.
        /// </summary>
        public void GenerateLevel()
        {
            StopAllCoroutines();
            StartCoroutine(GenerateLevelCoroutine());
        }

        /// <summary>
        /// Coroutine that generates the level with time budget.
        /// </summary>
        private IEnumerator GenerateLevelCoroutine()
        {
            if (isGenerating)
                yield break;

            isGenerating = true;
            generationStartTime = Time.realtimeSinceStartup;
            generationComplete = false;
            generatedChunks.Clear();

            try
            {
                // Create parent for chunks
                GameObject parent = new GameObject("Procedural Level");
                chunksParent = parent.transform;

                // Generate initial chunk graph using seed
                var chunkGraph = GenerateChunkGraph();
                
                yield return null; // Allow frame to process

                // Build the level by instantiating and connecting chunks
                float buildStartTime = Time.realtimeSinceStartup;
                yield return StartCoroutine(BuildLevelFromGraphCoroutine(chunkGraph));
                float buildTime = Time.realtimeSinceStartup - buildStartTime;

                if (logGenerationMetrics)
                {
                    Debug.Log($"[LevelBuilder] Built {generatedChunks.Count} chunks in {buildTime:F2}s");
                }

                // Bake NavMesh asynchronously
                if (bakeNavMeshAsync)
                {
                    float navStartTime = Time.realtimeSinceStartup;
                    yield return StartCoroutine(BakeNavMeshAsyncCoroutine());
                    float navTime = Time.realtimeSinceStartup - navStartTime;

                    if (logGenerationMetrics)
                    {
                        Debug.Log($"[LevelBuilder] NavMesh baking completed in {navTime:F2}s");
                    }
                }

                // Provision NPC spawn zones
                float npcStartTime = Time.realtimeSinceStartup;
                ProvisionNPCZones();
                float npcTime = Time.realtimeSinceStartup - npcStartTime;

                if (logGenerationMetrics)
                {
                    Debug.Log($"[LevelBuilder] NPC zones provisioned in {npcTime:F2}s");
                }

                // Log final metrics
                float totalTime = Time.realtimeSinceStartup - generationStartTime;
                LogGenerationMetrics(totalTime, buildTime, navTime, npcTime);

                generationComplete = true;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[LevelBuilder] Error during level generation: {ex.Message}\n{ex.StackTrace}");
            }
            finally
            {
                isGenerating = false;
            }
        }

        /// <summary>
        /// Generates chunk graph using SeedManager.
        /// </summary>
        private List<ProceduralChunkDefinition> GenerateChunkGraph()
        {
            var graph = new List<ProceduralChunkDefinition>();
            
            if (SeedManager.Instance == null)
            {
                Debug.LogWarning("[LevelBuilder] SeedManager not available, using fallback");
                return graph;
            }

            // Start with the starting chunk
            if (startingChunk != null)
            {
                graph.Add(startingChunk);
            }

            // Generate remaining chunks deterministically
            int chunksGenerated = 1;
            int maxAttempts = targetChunkCount * 3; // Prevent infinite loops
            int attempt = 0;

            while (chunksGenerated < targetChunkCount && attempt < maxAttempts)
            {
                attempt++;

                // Select a random chunk based on seed
                int chunkIndex = SeedManager.Instance.GetRandomInt(
                    SeedManager.SCOPE_CHUNKS,
                    0,
                    availableChunks.Length,
                    chunksGenerated
                );

                if (chunkIndex >= 0 && chunkIndex < availableChunks.Length)
                {
                    ProceduralChunkDefinition selectedChunk = availableChunks[chunkIndex];
                    
                    // Apply selection weight filtering
                    float weight = SeedManager.Instance.GetRandomFloat(
                        SeedManager.SCOPE_CHUNKS,
                        chunksGenerated + 100
                    );

                    if (weight <= selectedChunk.selectionWeight && 
                        (selectedChunk.allowDuplicates || !graph.Contains(selectedChunk)))
                    {
                        graph.Add(selectedChunk);
                        chunksGenerated++;
                    }
                }

                SeedManager.Instance.AdvanceScopeOffset(SeedManager.SCOPE_CHUNKS, 1);
            }

            return graph;
        }

        /// <summary>
        /// Builds the level from the chunk graph with time budget awareness.
        /// </summary>
        private IEnumerator BuildLevelFromGraphCoroutine(List<ProceduralChunkDefinition> chunkGraph)
        {
            Vector3 currentPosition = Vector3.zero;
            Quaternion currentRotation = Quaternion.identity;

            for (int i = 0; i < chunkGraph.Count; i++)
            {
                ProceduralChunkDefinition chunkDef = chunkGraph[i];

                if (chunkDef == null || chunkDef.chunkPrefab == null)
                {
                    Debug.LogWarning($"[LevelBuilder] Skipping invalid chunk definition at index {i}");
                    continue;
                }

                // Check time budget
                float elapsedTime = Time.realtimeSinceStartup - generationStartTime;
                if (elapsedTime > maxGenerationTimeSeconds * 0.7f) // Reserve 30% for NavMesh and NPC zones
                {
                    if (logGenerationMetrics)
                    {
                        Debug.LogWarning($"[LevelBuilder] Time budget exhausted. Generated {i} of {chunkGraph.Count} chunks.");
                    }
                    break;
                }

                // Instantiate chunk
                GameObject chunkInstance = Instantiate(
                    chunkDef.chunkPrefab,
                    currentPosition,
                    currentRotation,
                    chunksParent
                );

                chunkInstance.name = $"{chunkDef.chunkId}_{generatedChunks.Count}";

                var generatedChunk = new GeneratedChunkInstance
                {
                    definition = chunkDef,
                    instance = chunkInstance,
                    worldPosition = currentPosition,
                    worldRotation = currentRotation
                };

                // Setup NavMesh surface for this chunk
                if (bakeNavMeshAsync)
                {
                    var navMeshSurface = chunkInstance.AddComponent<NavMeshSurface>();
                    navMeshSurface.agentTypeID = navMeshAgentTypeID;
                    navMeshSurface.collectObjects = CollectObjects.Children;
                    navMeshSurface.useGeometry = NavMeshCollectGeometry.RenderMeshes;
                    generatedChunk.navMeshSurface = navMeshSurface;
                }

                generatedChunks.Add(generatedChunk);

                // Calculate next position based on connectors
                if (i < chunkGraph.Count - 1)
                {
                    ProceduralChunkDefinition nextChunkDef = chunkGraph[i + 1];
                    if (nextChunkDef != null)
                    {
                        CalculateNextChunkPosition(chunkDef, nextChunkDef, ref currentPosition, ref currentRotation);
                    }
                }

                // Spread work across frames
                if (i % 2 == 0)
                    yield return null;
            }
        }

        /// <summary>
        /// Calculates the position and rotation for the next chunk based on connectors.
        /// </summary>
        private void CalculateNextChunkPosition(
            ProceduralChunkDefinition currentDef,
            ProceduralChunkDefinition nextDef,
            ref Vector3 position,
            ref Quaternion rotation)
        {
            // Simple linear progression along X axis
            // In a more advanced system, this would use connector alignment
            position.x += currentDef.chunkSize.x + nextDef.chunkSize.x * 0.5f;
        }

        /// <summary>
        /// Bakes NavMesh for all chunks asynchronously.
        /// </summary>
        private IEnumerator BakeNavMeshAsyncCoroutine()
        {
            for (int i = 0; i < generatedChunks.Count; i++)
            {
                var chunk = generatedChunks[i];
                
                if (chunk.navMeshSurface == null)
                    continue;

                // Bake the NavMesh
                chunk.navMeshSurface.BuildNavMesh();

                // Check time budget periodically
                if (i % 3 == 0)
                {
                    float elapsedTime = Time.realtimeSinceStartup - generationStartTime;
                    if (elapsedTime > maxGenerationTimeSeconds * 0.9f)
                    {
                        Debug.LogWarning($"[LevelBuilder] Time budget exhausted during NavMesh baking. Remaining chunks use fallback nav.");
                        break;
                    }
                    
                    yield return null;
                }
            }
        }

        /// <summary>
        /// Provisions NPC spawn zones from chunk templates.
        /// </summary>
        private void ProvisionNPCZones()
        {
            if (npcSpawner == null)
            {
                Debug.LogWarning("[LevelBuilder] NPCSpawner not assigned");
                return;
            }

            var allNPCZones = new List<NPCSpawnZone>();
            int seedOffsetCounter = 0;

            foreach (var chunk in generatedChunks)
            {
                if (chunk.definition.npcZoneTemplates.Length == 0)
                    continue;

                foreach (var template in chunk.definition.npcZoneTemplates)
                {
                    var zone = new NPCSpawnZone
                    {
                        zoneName = $"{chunk.definition.chunkId}_{template.zoneName}",
                        zoneCenter = chunk.instance.transform.TransformPoint(template.centerOffset),
                        zoneSize = template.zoneSize,
                        allowedNPCTypes = template.allowedTypes,
                        minNPCs = template.minNPCs,
                        maxNPCs = template.maxNPCs,
                        difficultyMultiplier = template.difficultyMultiplier,
                        useProceduralSeed = true,
                        seedOffset = seedOffsetCounter,
                        spawnOnLevelLoad = true
                    };

                    allNPCZones.Add(zone);
                    chunk.npcZones.Add(zone);

                    seedOffsetCounter += 10; // Space out seed offsets for independent sequences
                }
            }

            // Update NPCSpawner with dynamic zones
            npcSpawner.SetDynamicSpawnZones(allNPCZones.ToArray());

            if (logGenerationMetrics)
            {
                Debug.Log($"[LevelBuilder] Provisioned {allNPCZones.Count} NPC zones");
            }
        }

        /// <summary>
        /// Logs generation metrics for telemetry and debugging.
        /// </summary>
        private void LogGenerationMetrics(float totalTime, float buildTime, float navTime, float npcTime)
        {
            if (!logGenerationMetrics)
                return;

            var sb = new System.Text.StringBuilder();
            sb.AppendLine("=== LEVEL GENERATION METRICS ===");
            sb.AppendLine($"Total Time: {totalTime:F3}s (Budget: {maxGenerationTimeSeconds:F3}s)");
            sb.AppendLine($"Chunk Building: {buildTime:F3}s ({buildTime / totalTime * 100:F1}%)");
            sb.AppendLine($"NavMesh Baking: {navTime:F3}s ({navTime / totalTime * 100:F1}%)");
            sb.AppendLine($"NPC Provisioning: {npcTime:F3}s ({npcTime / totalTime * 100:F1}%)");
            sb.AppendLine($"Chunks Generated: {generatedChunks.Count}");
            sb.AppendLine($"Budget Remaining: {Mathf.Max(0, maxGenerationTimeSeconds - totalTime):F3}s");

            if (SeedManager.Instance != null)
            {
                sb.AppendLine($"Seed: {SeedManager.Instance.CurrentSeed}");
            }

            Debug.Log(sb.ToString());
        }

        /// <summary>
        /// Clears the generated level.
        /// </summary>
        public void ClearLevel()
        {
            if (chunksParent != null)
            {
                Destroy(chunksParent.gameObject);
                chunksParent = null;
            }

            generatedChunks.Clear();
            generationComplete = false;
        }

        private void OnDrawGizmos()
        {
            if (!drawDebugGizmos || !generationComplete)
                return;

            foreach (var chunk in generatedChunks)
            {
                if (chunk.instance == null)
                    continue;

                // Draw chunk bounds
                Gizmos.color = new Color(0.2f, 0.8f, 0.2f, 0.3f);
                Gizmos.DrawCube(chunk.worldPosition, chunk.definition.chunkSize);

                // Draw NPC zones
                Gizmos.color = new Color(0.8f, 0.2f, 0.2f, 0.2f);
                foreach (var zone in chunk.npcZones)
                {
                    Gizmos.DrawCube(zone.zoneCenter, zone.zoneSize);
                }
            }
        }
    }
}
