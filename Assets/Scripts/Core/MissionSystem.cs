using UnityEngine;
using System;
using System.Collections.Generic;

namespace ProtocolEMR.Core
{
    /// <summary>
    /// Objective type for mission tracking.
    /// </summary>
    public enum ObjectiveType
    {
        Tutorial,
        Puzzle,
        Combat,
        Discovery,
        Extraction,
        Custom
    }

    /// <summary>
    /// Represents a single mission objective.
    /// </summary>
    [System.Serializable]
    public class MissionObjective
    {
        public string objectiveId;
        public string description;
        public ObjectiveType objectiveType;
        public bool isComplete;
        public bool isOptional;
        public float progress; // 0.0 to 1.0
        public int requiredCount;
        public int currentCount;

        public MissionObjective(string id, string desc, ObjectiveType type, bool optional = false, int required = 1)
        {
            objectiveId = id;
            description = desc;
            objectiveType = type;
            isOptional = optional;
            requiredCount = required;
            currentCount = 0;
            progress = 0f;
            isComplete = false;
        }

        public void IncrementProgress(int amount = 1)
        {
            currentCount = Mathf.Clamp(currentCount + amount, 0, requiredCount);
            progress = (float)currentCount / requiredCount;
            
            if (currentCount >= requiredCount)
            {
                isComplete = true;
            }
        }

        public void SetProgress(float value)
        {
            progress = Mathf.Clamp01(value);
            currentCount = Mathf.RoundToInt(progress * requiredCount);
            
            if (progress >= 1f)
            {
                isComplete = true;
            }
        }

        public void Complete()
        {
            isComplete = true;
            progress = 1f;
            currentCount = requiredCount;
        }
    }

    /// <summary>
    /// Central mission tracking system for Protocol EMR.
    /// Manages mission objectives, progression, and save/load checkpoints.
    /// </summary>
    public class MissionSystem : MonoBehaviour
    {
        public static MissionSystem Instance { get; private set; }

        [Header("Mission Configuration")]
        [SerializeField] private bool autoInitialize = true;

        private List<MissionObjective> objectives = new List<MissionObjective>();
        private Dictionary<string, MissionObjective> objectiveDict = new Dictionary<string, MissionObjective>();
        private int completedObjectivesCount = 0;

        // Events
        public event Action<MissionObjective> OnObjectiveAdded;
        public event Action<MissionObjective> OnObjectiveCompleted;
        public event Action<MissionObjective, float> OnObjectiveProgressUpdated;
        public event Action OnAllObjectivesCompleted;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            if (autoInitialize)
            {
                InitializeDefaultObjectives();
            }
        }

        /// <summary>
        /// Initializes default mission objectives for the golden path.
        /// </summary>
        private void InitializeDefaultObjectives()
        {
            AddObjective("tutorial_movement", "Learn basic movement", ObjectiveType.Tutorial, false, 1);
            AddObjective("tutorial_interaction", "Interact with objects", ObjectiveType.Tutorial, false, 1);
            AddObjective("puzzle_solve", "Solve the first puzzle", ObjectiveType.Puzzle, false, 1);
            AddObjective("combat_encounter", "Survive the combat encounter", ObjectiveType.Combat, false, 1);
            AddObjective("extraction_reach", "Reach the extraction point", ObjectiveType.Extraction, false, 1);

            Debug.Log($"[MissionSystem] Initialized {objectives.Count} default objectives");
        }

        /// <summary>
        /// Adds a new mission objective.
        /// </summary>
        public void AddObjective(string id, string description, ObjectiveType type, bool optional = false, int requiredCount = 1)
        {
            if (objectiveDict.ContainsKey(id))
            {
                Debug.LogWarning($"[MissionSystem] Objective {id} already exists");
                return;
            }

            var objective = new MissionObjective(id, description, type, optional, requiredCount);
            objectives.Add(objective);
            objectiveDict[id] = objective;

            OnObjectiveAdded?.Invoke(objective);
            Debug.Log($"[MissionSystem] Added objective: {id} - {description}");
        }

        /// <summary>
        /// Completes a mission objective by ID.
        /// </summary>
        public void CompleteObjective(string objectiveId)
        {
            if (!objectiveDict.TryGetValue(objectiveId, out MissionObjective objective))
            {
                Debug.LogWarning($"[MissionSystem] Objective {objectiveId} not found");
                return;
            }

            if (objective.isComplete)
                return;

            objective.Complete();
            completedObjectivesCount++;

            OnObjectiveCompleted?.Invoke(objective);
            Debug.Log($"[MissionSystem] Completed objective: {objectiveId}");

            CheckAllObjectivesCompleted();
        }

        /// <summary>
        /// Updates objective progress.
        /// </summary>
        public void UpdateObjectiveProgress(string objectiveId, float progress)
        {
            if (!objectiveDict.TryGetValue(objectiveId, out MissionObjective objective))
            {
                Debug.LogWarning($"[MissionSystem] Objective {objectiveId} not found");
                return;
            }

            objective.SetProgress(progress);
            OnObjectiveProgressUpdated?.Invoke(objective, progress);

            if (objective.isComplete && completedObjectivesCount < objectives.Count)
            {
                completedObjectivesCount++;
                OnObjectiveCompleted?.Invoke(objective);
                CheckAllObjectivesCompleted();
            }
        }

        /// <summary>
        /// Increments objective progress by count.
        /// </summary>
        public void IncrementObjective(string objectiveId, int amount = 1)
        {
            if (!objectiveDict.TryGetValue(objectiveId, out MissionObjective objective))
            {
                Debug.LogWarning($"[MissionSystem] Objective {objectiveId} not found");
                return;
            }

            bool wasComplete = objective.isComplete;
            objective.IncrementProgress(amount);

            OnObjectiveProgressUpdated?.Invoke(objective, objective.progress);

            if (!wasComplete && objective.isComplete)
            {
                completedObjectivesCount++;
                OnObjectiveCompleted?.Invoke(objective);
                CheckAllObjectivesCompleted();
            }
        }

        /// <summary>
        /// Gets an objective by ID.
        /// </summary>
        public MissionObjective GetObjective(string objectiveId)
        {
            objectiveDict.TryGetValue(objectiveId, out MissionObjective objective);
            return objective;
        }

        /// <summary>
        /// Gets all objectives.
        /// </summary>
        public List<MissionObjective> GetAllObjectives()
        {
            return new List<MissionObjective>(objectives);
        }

        /// <summary>
        /// Gets completion progress (0.0 to 1.0).
        /// </summary>
        public float GetCompletionProgress()
        {
            if (objectives.Count == 0)
                return 0f;

            int requiredCount = 0;
            int completedCount = 0;

            foreach (var objective in objectives)
            {
                if (!objective.isOptional)
                {
                    requiredCount++;
                    if (objective.isComplete)
                        completedCount++;
                }
            }

            return requiredCount > 0 ? (float)completedCount / requiredCount : 0f;
        }

        /// <summary>
        /// Checks if all required objectives are completed.
        /// </summary>
        private void CheckAllObjectivesCompleted()
        {
            foreach (var objective in objectives)
            {
                if (!objective.isOptional && !objective.isComplete)
                    return;
            }

            OnAllObjectivesCompleted?.Invoke();
            Debug.Log("[MissionSystem] All objectives completed!");
        }

        /// <summary>
        /// Resets all objectives.
        /// </summary>
        public void ResetObjectives()
        {
            objectives.Clear();
            objectiveDict.Clear();
            completedObjectivesCount = 0;

            if (autoInitialize)
            {
                InitializeDefaultObjectives();
            }

            Debug.Log("[MissionSystem] Objectives reset");
        }

        /// <summary>
        /// Gets mission state for save/load.
        /// </summary>
        public string SerializeMissionState()
        {
            var data = new MissionSaveData
            {
                objectives = objectives
            };
            return JsonUtility.ToJson(data, true);
        }

        /// <summary>
        /// Loads mission state from save data.
        /// </summary>
        public void DeserializeMissionState(string json)
        {
            try
            {
                var data = JsonUtility.FromJson<MissionSaveData>(json);
                
                objectives.Clear();
                objectiveDict.Clear();
                completedObjectivesCount = 0;

                foreach (var objective in data.objectives)
                {
                    objectives.Add(objective);
                    objectiveDict[objective.objectiveId] = objective;
                    
                    if (objective.isComplete)
                        completedObjectivesCount++;
                }

                Debug.Log($"[MissionSystem] Loaded {objectives.Count} objectives from save data");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[MissionSystem] Failed to deserialize mission state: {ex.Message}");
            }
        }

        [System.Serializable]
        private class MissionSaveData
        {
            public List<MissionObjective> objectives;
        }
    }
}
