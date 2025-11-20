using UnityEngine;
using System.Collections.Generic;
using ProtocolEMR.Core.Procedural;

namespace ProtocolEMR.Core.AI
{
    /// <summary>
    /// Zone configuration for NPC spawning.
    /// </summary>
    [System.Serializable]
    public class NPCSpawnZone
    {
        [Header("Zone Configuration")]
        public string zoneName = "Default Zone";
        public Vector3 zoneCenter = Vector3.zero;
        public Vector3 zoneSize = Vector3.one * 20f;
        
        [Header("Spawn Settings")]
        public NPCType[] allowedNPCTypes = { NPCType.Guard };
        public int minNPCs = 1;
        public int maxNPCs = 3;
        public float spawnDelay = 0f;
        public bool spawnOnLevelLoad = true;
        public bool spawnByTrigger = false;
        
        [Header("Difficulty Scaling")]
        public float difficultyMultiplier = 1.0f;
        public bool scaleWithProgress = true;
        
        [Header("Procedural Settings")]
        public bool useProceduralSeed = true;
        public int seedOffset = 0;
        
        [Header("Visual")]
        public Color gizmoColor = Color.green;
        public bool drawGizmos = true;

        // Runtime data
        private List<GameObject> spawnedNPCs = new List<GameObject>();
        private bool hasSpawned = false;

        public List<GameObject> SpawnedNPCs => spawnedNPCs;
        public bool HasSpawned => hasSpawned;

        /// <summary>
        /// Checks if a position is within this zone.
        /// </summary>
        public bool ContainsPosition(Vector3 position)
        {
            Vector3 localPos = position - zoneCenter;
            return Mathf.Abs(localPos.x) <= zoneSize.x * 0.5f &&
                   Mathf.Abs(localPos.y) <= zoneSize.y * 0.5f &&
                   Mathf.Abs(localPos.z) <= zoneSize.z * 0.5f;
        }

        /// <summary>
        /// Gets a random position within this zone.
        /// </summary>
        public Vector3 GetRandomPosition()
        {
            Vector3 randomPos = zoneCenter;
            
            // Use SeedManager for deterministic positioning
            if (SeedManager.Instance != null)
            {
                randomPos.x += SeedManager.Instance.GetRandomFloat(SeedManager.SCOPE_NPCS, seedOffset) * zoneSize.x - zoneSize.x * 0.5f;
                randomPos.y += SeedManager.Instance.GetRandomFloat(SeedManager.SCOPE_NPCS, seedOffset + 1) * zoneSize.y - zoneSize.y * 0.5f;
                randomPos.z += SeedManager.Instance.GetRandomFloat(SeedManager.SCOPE_NPCS, seedOffset + 2) * zoneSize.z - zoneSize.z * 0.5f;
            }
            else
            {
                // Fallback to Unity random
                randomPos.x += Random.Range(-zoneSize.x * 0.5f, zoneSize.x * 0.5f);
                randomPos.y += Random.Range(-zoneSize.y * 0.5f, zoneSize.y * 0.5f);
                randomPos.z += Random.Range(-zoneSize.z * 0.5f, zoneSize.z * 0.5f);
            }
            
            return randomPos;
        }

        /// <summary>
        /// Gets the number of NPCs to spawn based on zone settings.
        /// </summary>
        public int GetSpawnCount()
        {
            if (SeedManager.Instance != null)
            {
                return SeedManager.Instance.GetRandomInt(SeedManager.SCOPE_NPCS, minNPCs, maxNPCs + 1, seedOffset + 10);
            }
            return Random.Range(minNPCs, maxNPCs + 1);
        }

        /// <summary>
        /// Gets a random NPC type from allowed types.
        /// </summary>
        public NPCType GetRandomNPCType()
        {
            if (allowedNPCTypes.Length == 0)
                return NPCType.Guard;

            if (SeedManager.Instance != null)
            {
                return SeedManager.Instance.GetRandomItem(allowedNPCTypes, SeedManager.SCOPE_NPCS, seedOffset + 20);
            }
            return allowedNPCTypes[Random.Range(0, allowedNPCTypes.Length)];
        }

        /// <summary>
        /// Adds spawned NPC to tracking list.
        /// </summary>
        public void AddSpawnedNPC(GameObject npc)
        {
            if (npc != null && !spawnedNPCs.Contains(npc))
            {
                spawnedNPCs.Add(npc);
            }
        }

        /// <summary>
        /// Removes NPC from tracking list.
        /// </summary>
        public void RemoveSpawnedNPC(GameObject npc)
        {
            spawnedNPCs.Remove(npc);
        }

        /// <summary>
        /// Clears all spawned NPCs.
        /// </summary>
        public void ClearSpawnedNPCs()
        {
            spawnedNPCs.Clear();
        }

        /// <summary>
        /// Draws zone visualization in editor.
        /// </summary>
        public void DrawGizmos()
        {
            if (!drawGizmos)
                return;

            Gizmos.color = new Color(gizmoColor.r, gizmoColor.g, gizmoColor.b, 0.3f);
            Gizmos.DrawCube(zoneCenter, zoneSize);
            
            Gizmos.color = gizmoColor;
            Gizmos.DrawWireCube(zoneCenter, zoneSize);
        }
    }

    /// <summary>
    /// NPC spawner for procedural generation and zone-based spawning.
    /// </summary>
    public class NPCSpawner : MonoBehaviour
    {
        [Header("Spawner Configuration")]
        [SerializeField] private GameObject[] npcPrefabs;
        [SerializeField] private NPCSpawnZone[] spawnZones;
        [SerializeField] private Transform npcParent;
        
        [Header("Global Settings")]
        [SerializeField] private bool spawnOnStart = true;
        [SerializeField] private bool useGlobalSeed = false;
        [SerializeField] private int globalSeed = 12345;
        [SerializeField] private float maxNPCsPerLevel = 20;
        [SerializeField] private bool enforcePopulationLimit = true;
        
        [Header("Difficulty Scaling")]
        [SerializeField] private bool enableDifficultyScaling = true;
        [SerializeField] private AnimationCurve difficultyCurve = AnimationCurve.Linear(1f, 0.5f, 10f, 2.0f);
        
        [Header("Debug")]
        [SerializeField] private bool drawDebugGizmos = true;
        [SerializeField] private bool showSpawnInfo = true;

        // Runtime data
        private List<GameObject> allSpawnedNPCs = new List<GameObject>();
        private Dictionary<NPCType, GameObject> npcPrefabMap = new Dictionary<NPCType, GameObject>();
        private int currentLevel = 1;

        // Properties
        public int SpawnedNPCCount => allSpawnedNPCs.Count;
        public List<GameObject> AllSpawnedNPCs => allSpawnedNPCs;
        public NPCSpawnZone[] SpawnZones => spawnZones;

        private void Start()
        {
            InitializeSpawner();
            
            if (spawnOnStart)
            {
                SpawnAllZones();
            }
        }

        /// <summary>
        /// Initializes the spawner system.
        /// </summary>
        private void InitializeSpawner()
        {
            // Create NPC prefab mapping
            BuildNPCPrefabMap();
            
            // Create parent transform if not assigned
            if (npcParent == null)
            {
                GameObject parentObj = new GameObject("Spawned NPCs");
                parentObj.transform.SetParent(transform);
                npcParent = parentObj.transform;
            }

            // Initialize random seed - now handled by SeedManager
            if (useGlobalSeed && SeedManager.Instance != null)
            {
                SeedManager.Instance.SetSeed(globalSeed);
            }
            else if (useGlobalSeed)
            {
                // Fallback if SeedManager not available
                Random.InitState(globalSeed);
            }

            Debug.Log("NPC Spawner initialized");
        }

        /// <summary>
        /// Builds mapping from NPC types to prefabs.
        /// </summary>
        private void BuildNPCPrefabMap()
        {
            npcPrefabMap.Clear();
            
            foreach (GameObject prefab in npcPrefabs)
            {
                if (prefab == null) continue;

                NPCController controller = prefab.GetComponent<NPCController>();
                if (controller != null)
                {
                    npcPrefabMap[controller.Type] = prefab;
                }
            }

            Debug.Log($"Mapped {npcPrefabMap.Count} NPC types to prefabs");
        }

        /// <summary>
        /// Spawns NPCs for all zones.
        /// </summary>
        public void SpawnAllZones()
        {
            foreach (var zone in spawnZones)
            {
                SpawnZone(zone);
            }
            
            Debug.Log($"Spawned {allSpawnedNPCs.Count} NPCs across {spawnZones.Length} zones");
        }

        /// <summary>
        /// Spawns NPCs for a specific zone.
        /// </summary>
        public void SpawnZone(NPCSpawnZone zone)
        {
            if (zone.HasSpawned || !zone.spawnOnLevelLoad)
                return;

            if (enforcePopulationLimit && allSpawnedNPCs.Count >= maxNPCsPerLevel)
            {
                Debug.LogWarning("Maximum NPC population limit reached");
                return;
            }

            int spawnCount = zone.GetSpawnCount();
            spawnCount = Mathf.Min(spawnCount, maxNPCsPerLevel - allSpawnedNPCs.Count);

            for (int i = 0; i < spawnCount; i++)
            {
                SpawnNPCInZone(zone);
            }

            zone.hasSpawned = true;
        }

        /// <summary>
        /// Spawns a single NPC in a zone.
        /// </summary>
        private GameObject SpawnNPCInZone(NPCSpawnZone zone)
        {
            NPCType npcType = zone.GetRandomNPCType();
            Vector3 spawnPosition = GetValidSpawnPosition(zone);
            
            if (spawnPosition == Vector3.zero)
            {
                Debug.LogWarning($"Could not find valid spawn position in zone {zone.zoneName}");
                return null;
            }

            GameObject npcPrefab = GetNPCPrefab(npcType);
            if (npcPrefab == null)
            {
                Debug.LogWarning($"No prefab found for NPC type {npcType}");
                return null;
            }

            GameObject npc = Instantiate(npcPrefab, spawnPosition, Quaternion.identity, npcParent);
            NPCController controller = npc.GetComponent<NPCController>();
            
            if (controller != null)
            {
                ConfigureNPC(controller, zone);
            }

            allSpawnedNPCs.Add(npc);
            zone.AddSpawnedNPC(npc);

            return npc;
        }

        /// <summary>
        /// Gets a valid spawn position within a zone.
        /// </summary>
        private Vector3 GetValidSpawnPosition(NPCSpawnZone zone)
        {
            int maxAttempts = 10;
            
            for (int attempt = 0; attempt < maxAttempts; attempt++)
            {
                Vector3 candidatePos = zone.GetRandomPosition();
                
                // Check if position is on NavMesh
                UnityEngine.AI.NavMeshHit hit;
                if (UnityEngine.AI.NavMesh.SamplePosition(candidatePos, out hit, 2.0f, UnityEngine.AI.NavMesh.AllAreas))
                {
                    // Check if position is clear of other NPCs
                    if (IsPositionClear(hit.position))
                    {
                        return hit.position;
                    }
                }
            }

            return Vector3.zero;
        }

        /// <summary>
        /// Checks if a position is clear of other NPCs.
        /// </summary>
        private bool IsPositionClear(Vector3 position)
        {
            float checkRadius = 2.0f;
            Collider[] colliders = Physics.OverlapSphere(position, checkRadius);
            
            foreach (var collider in colliders)
            {
                NPCController npc = collider.GetComponent<NPCController>();
                if (npc != null)
                {
                    return false;
                }
            }
            
            return true;
        }

        /// <summary>
        /// Gets prefab for specific NPC type.
        /// </summary>
        private GameObject GetNPCPrefab(NPCType npcType)
        {
            if (npcPrefabMap.ContainsKey(npcType))
            {
                return npcPrefabMap[npcType];
            }

            // Fallback to first available prefab
            return npcPrefabs.Length > 0 ? npcPrefabs[0] : null;
        }

        /// <summary>
        /// Configures NPC with zone-specific settings.
        /// </summary>
        private void ConfigureNPC(NPCController controller, NPCSpawnZone zone)
        {
            // Apply procedural seed using SeedManager
            if (zone.useProceduralSeed && SeedManager.Instance != null)
            {
                int npcIndex = allSpawnedNPCs.Count;
                SeedManager.Instance.AdvanceScopeOffset(SeedManager.SCOPE_NPCS, zone.seedOffset + npcIndex);
            }
            else if (zone.useProceduralSeed)
            {
                // Fallback if SeedManager not available
                int seed = globalSeed + zone.seedOffset + GetInstanceID() + allSpawnedNPCs.Count;
                Random.InitState(seed);
            }

            // Apply difficulty scaling
            if (enableDifficultyScaling)
            {
                ApplyDifficultyScaling(controller, zone);
            }

            // Set up patrol points if needed
            if (zone.zoneSize.magnitude > 30f)
            {
                GeneratePatrolForNPC(controller, zone);
            }
        }

        /// <summary>
        /// Applies difficulty scaling to NPC parameters.
        /// </summary>
        private void ApplyDifficultyScaling(NPCController controller, NPCSpawnZone zone)
        {
            float difficultyMultiplier = difficultyCurve.Evaluate(currentLevel) * zone.difficultyMultiplier;
            
            NPCParameters parameters = controller.Parameters;
            parameters.maxHealth *= difficultyMultiplier;
            parameters.damagePerHit *= difficultyMultiplier;
            parameters.perceptionRange *= Mathf.Lerp(0.7f, 1.3f, difficultyMultiplier);
            parameters.reactionTime *= Mathf.Lerp(1.5f, 0.7f, difficultyMultiplier);
        }

        /// <summary>
        /// Generates patrol points for NPC within zone.
        /// </summary>
        private void GeneratePatrolForNPC(NPCController controller, NPCSpawnZone zone)
        {
            List<Vector3> patrolPoints = new List<Vector3>();
            int pointCount;
            
            if (SeedManager.Instance != null)
            {
                pointCount = SeedManager.Instance.GetRandomInt(SeedManager.SCOPE_NPCS, 3, 7, zone.seedOffset + 30);
            }
            else
            {
                pointCount = Random.Range(3, 6);
            }
            
            for (int i = 0; i < pointCount; i++)
            {
                Vector3 point = zone.GetRandomPosition();
                
                UnityEngine.AI.NavMeshHit hit;
                if (UnityEngine.AI.NavMesh.SamplePosition(point, out hit, 5.0f, UnityEngine.AI.NavMesh.AllAreas))
                {
                    patrolPoints.Add(hit.position);
                }
            }

            // Set patrol points through reflection or public method
            // This would need to be implemented in NPCController
        }

        /// <summary>
        /// Destroys all spawned NPCs.
        /// </summary>
        public void DespawnAllNPCs()
        {
            foreach (var npc in allSpawnedNPCs)
            {
                if (npc != null)
                {
                    DestroyImmediate(npc);
                }
            }
            
            allSpawnedNPCs.Clear();
            
            foreach (var zone in spawnZones)
            {
                zone.ClearSpawnedNPCs();
                zone.hasSpawned = false;
            }
        }

        /// <summary>
        /// Spawns NPCs for triggered zones.
        /// </summary>
        public void TriggerZoneSpawn(string zoneName)
        {
            foreach (var zone in spawnZones)
            {
                if (zone.zoneName == zoneName && zone.spawnByTrigger)
                {
                    SpawnZone(zone);
                }
            }
        }

        /// <summary>
        /// Gets NPCs in specific zone.
        /// </summary>
        public List<GameObject> GetNPCsInZone(NPCSpawnZone zone)
        {
            return new List<GameObject>(zone.SpawnedNPCs);
        }

        /// <summary>
        /// Gets all NPCs of specific type.
        /// </summary>
        public List<GameObject> GetNPCsOfType(NPCType npcType)
        {
            List<GameObject> npcs = new List<GameObject>();
            
            foreach (var npc in allSpawnedNPCs)
            {
                if (npc == null) continue;
                
                NPCController controller = npc.GetComponent<NPCController>();
                if (controller != null && controller.Type == npcType)
                {
                    npcs.Add(npc);
                }
            }
            
            return npcs;
        }

        /// <summary>
        /// Sets current level for difficulty scaling.
        /// </summary>
        public void SetCurrentLevel(int level)
        {
            currentLevel = Mathf.Max(1, level);
        }

        /// <summary>
        /// Gets population percentage.
        /// </summary>
        public float GetPopulationPercentage()
        {
            return (float)allSpawnedNPCs.Count / maxNPCsPerLevel;
        }

        private void OnDrawGizmos()
        {
            if (!drawDebugGizmos)
                return;

            // Draw all spawn zones
            foreach (var zone in spawnZones)
            {
                zone.DrawGizmos();
            }

            // Draw spawn info
            if (showSpawnInfo)
            {
                Vector3 infoPos = transform.position + Vector3.up * 2f;
                Gizmos.color = Color.white;
                UnityEditor.Handles.Label(infoPos, $"NPCs: {allSpawnedNPCs.Count}/{maxNPCsPerLevel}");
            }
        }

        private void OnValidate()
        {
            maxNPCsPerLevel = Mathf.Max(1, maxNPCsPerLevel);
            globalSeed = Mathf.Max(0, globalSeed);
        }
    }
}