using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.AI;
using ProtocolEMR.Core.AI;

namespace ProtocolEMR.Systems.SaveLoad
{
    /// <summary>
    /// Extracts and restores NPC state for persistence.
    /// </summary>
    public static class NPCStateSerializer
    {
        private static readonly FieldInfo CurrentHealthField = typeof(NPCController)
            .GetField("currentHealth", BindingFlags.NonPublic | BindingFlags.Instance);

        private static readonly FieldInfo CurrentStaminaField = typeof(NPCController)
            .GetField("currentStamina", BindingFlags.NonPublic | BindingFlags.Instance);

        public static NPCStateCollection CaptureState()
        {
            var collection = new NPCStateCollection();
            var controllers = GetAllNPCControllers();

            foreach (var npc in controllers)
            {
                if (npc == null)
                    continue;

                var snapshot = new NPCStateData
                {
                    npcId = BuildPersistentId(npc),
                    npcName = npc.name,
                    npcType = npc.Type,
                    currentState = npc.CurrentState,
                    position = SerializableVector3.FromVector3(npc.transform.position),
                    rotation = SerializableQuaternion.FromQuaternion(npc.transform.rotation),
                    health = npc.CurrentHealth,
                    stamina = npc.CurrentStamina,
                    isDead = npc.CurrentState == NPCState.Dead
                };

                var perception = npc.Perception;
                if (perception != null)
                {
                    var perceptionData = perception.Data;
                    snapshot.canSeePlayer = perceptionData.canSeePlayer;
                    snapshot.canHearPlayer = perceptionData.canHearPlayer;
                    snapshot.lastKnownPlayerPosition = SerializableVector3.FromVector3(perceptionData.lastKnownPlayerPosition);
                    snapshot.alertness = perceptionData.alertness;
                }

                collection.entries.Add(snapshot);
            }

            return collection;
        }

        public static void ApplyState(NPCStateCollection collection)
        {
            if (collection == null || collection.entries.Count == 0)
                return;

            var lookup = new Dictionary<string, NPCStateData>(StringComparer.OrdinalIgnoreCase);
            foreach (var entry in collection.entries)
            {
                if (!string.IsNullOrEmpty(entry.npcId))
                {
                    lookup[entry.npcId] = entry;
                }
            }

            var controllers = GetAllNPCControllers();
            foreach (var npc in controllers)
            {
                if (npc == null)
                    continue;

                string id = BuildPersistentId(npc);
                if (!lookup.TryGetValue(id, out var snapshot))
                    continue;

                ApplySnapshot(npc, snapshot);
            }
        }

        private static void ApplySnapshot(NPCController npc, NPCStateData snapshot)
        {
            if (npc == null || snapshot == null)
                return;

            Vector3 position = snapshot.position.ToVector3();
            Quaternion rotation = snapshot.rotation.ToQuaternion();

            var agent = npc.GetComponent<NavMeshAgent>();
            bool reEnableAgent = agent != null && agent.enabled;

            if (reEnableAgent)
            {
                agent.enabled = false;
            }

            npc.transform.SetPositionAndRotation(position, rotation);

            if (reEnableAgent)
            {
                agent.enabled = true;
                agent.Warp(position);
            }

            float targetHealth = Mathf.Clamp(snapshot.health, 0f, npc.Parameters.maxHealth);
            float targetStamina = Mathf.Clamp(snapshot.stamina, 0f, npc.Parameters.maxStamina);

            CurrentHealthField?.SetValue(npc, targetHealth);
            CurrentStaminaField?.SetValue(npc, targetStamina);

            var desiredState = snapshot.isDead ? NPCState.Dead : snapshot.currentState;
            npc.SetState(desiredState);
        }

        private static List<NPCController> GetAllNPCControllers()
        {
            if (NPCManager.Instance != null)
            {
                return new List<NPCController>(NPCManager.Instance.AllNPCs);
            }

            return new List<NPCController>(UnityEngine.Object.FindObjectsOfType<NPCController>());
        }

        private static string BuildPersistentId(NPCController npc)
        {
            if (npc == null)
                return string.Empty;

            string sceneName = npc.gameObject.scene.IsValid() ? npc.gameObject.scene.name : "unspecified";
            string path = GetHierarchyPath(npc.transform);
            return $"{sceneName}:{path}";
        }

        private static string GetHierarchyPath(Transform root)
        {
            if (root == null)
                return string.Empty;

            var stack = new Stack<string>();
            var current = root;
            while (current != null)
            {
                stack.Push(current.name);
                current = current.parent;
            }

            return string.Join("/", stack);
        }
    }

    /// <summary>
    /// Collection wrapper for NPC state snapshots.
    /// </summary>
    [Serializable]
    public class NPCStateCollection
    {
        public List<NPCStateData> entries = new List<NPCStateData>();
    }

    /// <summary>
    /// Serializable NPC state information.
    /// </summary>
    [Serializable]
    public class NPCStateData
    {
        public string npcId;
        public string npcName;
        public NPCType npcType;
        public NPCState currentState;
        public SerializableVector3 position;
        public SerializableQuaternion rotation;
        public float health;
        public float stamina;
        public bool isDead;
        public bool canSeePlayer;
        public bool canHearPlayer;
        public SerializableVector3 lastKnownPlayerPosition;
        public float alertness;
    }
}
