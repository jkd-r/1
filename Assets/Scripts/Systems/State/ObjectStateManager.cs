using UnityEngine;
using System.Collections.Generic;
using System;
using System.IO;

namespace ProtocolEMR.Systems
{
    /// <summary>
    /// Manages the state of interactable objects in the world.
    /// Saves and loads door states, pickup states, terminal states, etc.
    /// </summary>
    public class ObjectStateManager : MonoBehaviour
    {
        [Serializable]
        public class ObjectState
        {
            public string objectId;
            public string objectType;
            public bool hasBeenInteracted;
            public bool isLocked;
            public bool isOpen;
            public bool hasBeenPickedUp;
            public Vector3 position;
            public string customData;
        }

        [Serializable]
        public class WorldState
        {
            public List<ObjectState> objectStates = new List<ObjectState>();
            public ProceduralState proceduralState = new ProceduralState();
        }

        [SerializeField] private string stateFileName = "world_state.json";
        private Dictionary<string, ObjectState> objectStates = new Dictionary<string, ObjectState>();
        private ProceduralState proceduralState = new ProceduralState();
        private string savePath;

        private static ObjectStateManager instance;
        public static ObjectStateManager Instance
        {
            get
            {
                if (instance == null)
                {
                    GameObject managerObject = new GameObject("ObjectStateManager");
                    instance = managerObject.AddComponent<ObjectStateManager>();
                }
                return instance;
            }
        }

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }
            instance = this;

            savePath = Path.Combine(Application.persistentDataPath, stateFileName);
        }

        /// <summary>
        /// Records the state of an object.
        /// </summary>
        public void RecordObjectState(string objectId, string objectType, ObjectState state)
        {
            state.objectId = objectId;
            state.objectType = objectType;
            objectStates[objectId] = state;
        }

        /// <summary>
        /// Gets the state of an object.
        /// </summary>
        public ObjectState GetObjectState(string objectId)
        {
            if (objectStates.TryGetValue(objectId, out var state))
            {
                return state;
            }
            return null;
        }

        /// <summary>
        /// Checks if an object has been interacted with.
        /// </summary>
        public bool HasObjectBeenInteracted(string objectId)
        {
            var state = GetObjectState(objectId);
            return state != null && state.hasBeenInteracted;
        }

        /// <summary>
        /// Records that a door is locked/unlocked.
        /// </summary>
        public void RecordDoorState(string doorId, bool isLocked, bool isOpen)
        {
            var state = new ObjectState
            {
                objectId = doorId,
                objectType = "Door",
                isLocked = isLocked,
                isOpen = isOpen,
                hasBeenInteracted = true
            };
            objectStates[doorId] = state;
        }

        /// <summary>
        /// Records that an item has been picked up.
        /// </summary>
        public void RecordItemPickup(string itemId, string itemName)
        {
            var state = new ObjectState
            {
                objectId = itemId,
                objectType = "Item",
                hasBeenPickedUp = true,
                hasBeenInteracted = true,
                customData = itemName
            };
            objectStates[itemId] = state;
        }

        /// <summary>
        /// Records that a terminal has been accessed.
        /// </summary>
        public void RecordTerminalAccess(string terminalId, bool hasBeenUsed)
        {
            var state = GetObjectState(terminalId);
            if (state == null)
            {
                state = new ObjectState { objectId = terminalId, objectType = "Terminal" };
            }
            state.hasBeenInteracted = hasBeenUsed;
            objectStates[terminalId] = state;
        }

        /// <summary>
        /// Saves all object states to file.
        /// </summary>
        public void SaveWorldState()
        {
            var worldState = new WorldState();
            foreach (var state in objectStates.Values)
            {
                worldState.objectStates.Add(state);
            }

            // Include procedural state
            worldState.proceduralState = proceduralState;

            string json = JsonUtility.ToJson(worldState, true);
            File.WriteAllText(savePath, json);

            Debug.Log($"World state saved to {savePath}");
        }

        /// <summary>
        /// Loads all object states from file.
        /// </summary>
        public void LoadWorldState()
        {
            if (!File.Exists(savePath))
            {
                Debug.LogWarning($"State file not found: {savePath}");
                return;
            }

            string json = File.ReadAllText(savePath);
            var worldState = JsonUtility.FromJson<WorldState>(json);

            objectStates.Clear();
            foreach (var state in worldState.objectStates)
            {
                objectStates[state.objectId] = state;
            }

            // Load procedural state
            if (worldState.proceduralState != null)
            {
                proceduralState = worldState.proceduralState;
                
                // Restore seed to SeedManager if available
                if (ProtocolEMR.Core.Procedural.SeedManager.Instance != null)
                {
                    ProtocolEMR.Core.Procedural.SeedManager.Instance.SetSeed(proceduralState.seed);
                    
                    // Restore scope offsets
                    foreach (var offset in proceduralState.scopeOffsets)
                    {
                        ProtocolEMR.Core.Procedural.SeedManager.Instance.SetScopeOffset(offset.scope, offset.offset);
                    }
                }
            }

            Debug.Log($"World state loaded from {savePath}");
        }

        /// <summary>
        /// Clears all recorded states.
        /// </summary>
        public void ClearAllStates()
        {
            objectStates.Clear();
        }

        /// <summary>
        /// Deletes the saved state file.
        /// </summary>
        public void DeleteSavedState()
        {
            if (File.Exists(savePath))
            {
                File.Delete(savePath);
                Debug.Log($"State file deleted: {savePath}");
            }
        }

        /// <summary>
        /// Gets all recorded object states.
        /// </summary>
        public List<ObjectState> GetAllObjectStates()
        {
            return new List<ObjectState>(objectStates.Values);
        }

        /// <summary>
        /// Records the current procedural state from SeedManager.
        /// </summary>
        public void RecordProceduralState()
        {
            if (ProtocolEMR.Core.Procedural.SeedManager.Instance != null)
            {
                proceduralState.seed = ProtocolEMR.Core.Procedural.SeedManager.Instance.CurrentSeed;
                
                proceduralState.scopeOffsets.Clear();
                string[] scopes = { 
                    ProtocolEMR.Core.Procedural.SeedManager.SCOPE_CHUNKS,
                    ProtocolEMR.Core.Procedural.SeedManager.SCOPE_ENCOUNTERS,
                    ProtocolEMR.Core.Procedural.SeedManager.SCOPE_AUDIO,
                    ProtocolEMR.Core.Procedural.SeedManager.SCOPE_NPCS,
                    ProtocolEMR.Core.Procedural.SeedManager.SCOPE_STORY,
                    ProtocolEMR.Core.Procedural.SeedManager.SCOPE_LOOT,
                    ProtocolEMR.Core.Procedural.SeedManager.SCOPE_ENVIRONMENT
                };
                
                foreach (string scope in scopes)
                {
                    proceduralState.scopeOffsets.Add(new ScopeOffsetState
                    {
                        scope = scope,
                        offset = ProtocolEMR.Core.Procedural.SeedManager.Instance.GetScopeOffset(scope)
                    });
                }
            }
        }

        /// <summary>
        /// Gets the current procedural state.
        /// </summary>
        public ProceduralState GetProceduralState()
        {
            return proceduralState;
        }
    }

    [Serializable]
    public class ProceduralState
    {
        public int seed;
        public List<ScopeOffsetState> scopeOffsets = new List<ScopeOffsetState>();
    }

    [Serializable]
    public class ScopeOffsetState
    {
        public string scope;
        public int offset;
    }
}
