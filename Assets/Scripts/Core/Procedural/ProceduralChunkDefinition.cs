using UnityEngine;
using System.Collections.Generic;
using ProtocolEMR.Core.AI;

namespace ProtocolEMR.Core.Procedural
{
    /// <summary>
    /// Defines connection points on a chunk for stitching multiple chunks together.
    /// </summary>
    [System.Serializable]
    public class ChunkConnector
    {
        [Tooltip("Unique identifier for this connector")]
        public string connectorTag = "default";

        [Tooltip("Local position relative to chunk center")]
        public Vector3 localPosition = Vector3.zero;

        [Tooltip("Rotation for aligned connections")]
        public Quaternion localRotation = Quaternion.identity;

        [Tooltip("Size of the connection portal")]
        public Vector3 connectionSize = Vector3.one * 5f;

        [Tooltip("Connector can face outward from chunk")]
        public bool allowOutbound = true;

        [Tooltip("Connector can accept inbound connections")]
        public bool allowInbound = true;

        /// <summary>
        /// Gets the world position of this connector given a chunk's transform.
        /// </summary>
        public Vector3 GetWorldPosition(Transform chunkTransform)
        {
            return chunkTransform.TransformPoint(localPosition);
        }

        /// <summary>
        /// Gets the world rotation of this connector.
        /// </summary>
        public Quaternion GetWorldRotation(Transform chunkTransform)
        {
            return chunkTransform.rotation * localRotation;
        }
    }

    /// <summary>
    /// Defines a hazard or obstacle within a chunk.
    /// </summary>
    [System.Serializable]
    public class ChunkHazard
    {
        public string hazardType = "obstacle";
        public Vector3 position = Vector3.zero;
        public float radius = 1f;
        public bool isDangerous = false;
    }

    /// <summary>
    /// Defines an NPC spawn zone template for chunks.
    /// </summary>
    [System.Serializable]
    public class ChunkNPCZoneTemplate
    {
        public string zoneName = "Zone_Unnamed";
        public Vector3 centerOffset = Vector3.zero;
        public Vector3 zoneSize = Vector3.one * 10f;
        public NPCType[] allowedTypes = { NPCType.Guard };
        public int minNPCs = 1;
        public int maxNPCs = 3;
        public float difficultyMultiplier = 1.0f;
    }

    /// <summary>
    /// Scriptable object that defines the structure and properties of a procedural chunk.
    /// </summary>
    [CreateAssetMenu(fileName = "ChunkDefinition", menuName = "Protocol EMR/Procedural/Chunk Definition")]
    public class ProceduralChunkDefinition : ScriptableObject
    {
        [Header("Identity")]
        [Tooltip("Unique identifier for this chunk type")]
        public string chunkId = "chunk_default";

        [Tooltip("Display name for the chunk")]
        public string displayName = "Default Chunk";

        [Tooltip("Chunk prefab to instantiate")]
        public GameObject chunkPrefab;

        [Header("Dimensions")]
        [Tooltip("Size of the chunk in world units")]
        public Vector3 chunkSize = Vector3.one * 20f;

        [Tooltip("Biome/environment type")]
        public string biomeType = "generic";

        [Header("Connections")]
        [Tooltip("Available connectors for stitching chunks together")]
        public ChunkConnector[] connectors = new ChunkConnector[0];

        [Header("Environmental Features")]
        [Tooltip("Hazards within this chunk")]
        public ChunkHazard[] hazards = new ChunkHazard[0];

        [Tooltip("Light fixtures or environmental interactions")]
        public string[] environmentalFeatures = new string[0];

        [Header("NPC Zones")]
        [Tooltip("NPC spawn zone templates for this chunk")]
        public ChunkNPCZoneTemplate[] npcZoneTemplates = new ChunkNPCZoneTemplate[0];

        [Header("Generation Parameters")]
        [Tooltip("Minimum level at which this chunk can appear")]
        public int minLevel = 1;

        [Tooltip("Maximum level at which this chunk can appear")]
        public int maxLevel = 100;

        [Tooltip("Probability weight for random selection (0-1)")]
        [Range(0f, 1f)]
        public float selectionWeight = 1.0f;

        [Tooltip("Whether this chunk can appear multiple times in a level")]
        public bool allowDuplicates = true;

        [Tooltip("Relative time cost for generation (affects pacing)")]
        [Range(0.1f, 5f)]
        public float generationCost = 1.0f;

        [Header("Debug")]
        [SerializeField] private bool showDebugInfo = false;

        /// <summary>
        /// Validates chunk definition for completeness and consistency.
        /// </summary>
        public List<string> Validate()
        {
            var errors = new List<string>();

            if (string.IsNullOrEmpty(chunkId))
                errors.Add("Chunk ID is empty");

            if (chunkPrefab == null)
                errors.Add("Chunk prefab is not assigned");

            if (chunkSize.x <= 0 || chunkSize.y <= 0 || chunkSize.z <= 0)
                errors.Add("Chunk size must be positive");

            if (connectors.Length == 0)
                errors.Add("Warning: Chunk has no connectors (will be terminal chunk)");

            if (minLevel > maxLevel)
                errors.Add("minLevel cannot be greater than maxLevel");

            foreach (var connector in connectors)
            {
                if (string.IsNullOrEmpty(connector.connectorTag))
                    errors.Add("Connector has empty tag");

                if (connector.connectionSize.magnitude <= 0)
                    errors.Add("Connector size must be positive");
            }

            return errors;
        }

        /// <summary>
        /// Gets a connector by tag.
        /// </summary>
        public ChunkConnector GetConnector(string tag)
        {
            foreach (var conn in connectors)
            {
                if (conn.connectorTag == tag)
                    return conn;
            }
            return null;
        }

        /// <summary>
        /// Gets all available connector tags.
        /// </summary>
        public string[] GetConnectorTags()
        {
            var tags = new string[connectors.Length];
            for (int i = 0; i < connectors.Length; i++)
            {
                tags[i] = connectors[i].connectorTag;
            }
            return tags;
        }
    }
}
