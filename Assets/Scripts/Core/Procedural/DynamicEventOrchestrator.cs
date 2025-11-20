using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using ProtocolEMR.Core.Dialogue;
using ProtocolEMR.Core.AI;

namespace ProtocolEMR.Core.Procedural
{
    /// <summary>
    /// Active instance of a dynamic event.
    /// </summary>
    public class ActiveDynamicEvent
    {
        public string eventId;
        public ProceduralEventProfile profile;
        public int chunkId;
        public float startTime;
        public float duration;
        public bool isComplete;
        public int progress;
        public List<GameObject> spawnedObjects = new List<GameObject>();
        
        public ActiveDynamicEvent(ProceduralEventProfile profile, int chunkId)
        {
            this.eventId = $"{profile.eventId}_{Time.time}";
            this.profile = profile;
            this.chunkId = chunkId;
            this.startTime = Time.time;
            this.duration = profile.eventDuration;
            this.isComplete = false;
            this.progress = 0;
        }
        
        public float ElapsedTime => Time.time - startTime;
        public float RemainingTime => Mathf.Max(0, duration - ElapsedTime);
        public bool HasExpired => duration > 0 && ElapsedTime >= duration;
    }

    /// <summary>
    /// Orchestrates dynamic procedural events based on world state.
    /// Schedules ambient, combat, and puzzle events per chunk using deterministic seeds.
    /// Integrates with DifficultyManager, NPCManager, and Unknown dialogue system.
    /// </summary>
    public class DynamicEventOrchestrator : MonoBehaviour
    {
        public static DynamicEventOrchestrator Instance { get; private set; }

        [Header("Configuration")]
        [SerializeField] private List<ProceduralEventProfile> eventProfiles = new List<ProceduralEventProfile>();
        [SerializeField] private bool enableOrchestration = true;
        
        [Header("Timing")]
        [SerializeField] private float eventCheckInterval = 5f;
        [SerializeField] private float minTimeBetweenEvents = 10f;
        
        [Header("Performance")]
        [SerializeField] private bool enablePerformanceLogging = false;
        [SerializeField] private float maxSchedulingTimeMs = 1f;
        
        [Header("Debug")]
        [SerializeField] private bool showDebugInfo = false;
        [SerializeField] private bool visualizeUpcomingEvents = false;
        [SerializeField] private KeyCode debugToggleKey = KeyCode.F9;

        private WorldStateBlackboard worldState;
        private List<ActiveDynamicEvent> activeEvents = new List<ActiveDynamicEvent>();
        private Dictionary<string, float> eventCooldowns = new Dictionary<string, float>();
        private Dictionary<int, float> chunkCooldowns = new Dictionary<int, float>();
        private Dictionary<string, int> eventOccurrences = new Dictionary<string, int>();
        private float lastEventTime = 0f;
        private float lastCheckTime = 0f;
        private Queue<Action> pendingDialogueActions = new Queue<Action>();
        
        // Scoped seed for deterministic event scheduling
        private const string SCOPE_EVENTS = "dynamic_events";

        public WorldStateBlackboard WorldState => worldState;
        public List<ActiveDynamicEvent> ActiveEvents => activeEvents;
        public bool EnableOrchestration { get => enableOrchestration; set => enableOrchestration = value; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            InitializeOrchestrator();
        }

        private void InitializeOrchestrator()
        {
            worldState = new WorldStateBlackboard();
            LoadEventProfiles();
            
            Debug.Log($"DynamicEventOrchestrator initialized with {eventProfiles.Count} event profiles");
        }

        private void LoadEventProfiles()
        {
            if (eventProfiles.Count == 0)
            {
                // Load from Resources folder if not assigned
                var loadedProfiles = Resources.LoadAll<ProceduralEventProfile>("Events");
                eventProfiles.AddRange(loadedProfiles);
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(debugToggleKey))
            {
                showDebugInfo = !showDebugInfo;
                visualizeUpcomingEvents = showDebugInfo;
            }

            if (!enableOrchestration) return;

            float startTime = enablePerformanceLogging ? Time.realtimeSinceStartup : 0f;

            // Update active events
            UpdateActiveEvents();

            // Process pending dialogue events (batch processing to prevent spam)
            ProcessPendingDialogueEvents();

            // Check for new events periodically
            if (Time.time - lastCheckTime >= eventCheckInterval)
            {
                CheckAndScheduleEvents();
                lastCheckTime = Time.time;
            }

            if (enablePerformanceLogging)
            {
                float elapsedMs = (Time.realtimeSinceStartup - startTime) * 1000f;
                if (elapsedMs > maxSchedulingTimeMs)
                {
                    Debug.LogWarning($"DynamicEventOrchestrator scheduling exceeded target: {elapsedMs:F3}ms");
                }
            }
        }

        private void UpdateActiveEvents()
        {
            for (int i = activeEvents.Count - 1; i >= 0; i--)
            {
                var evt = activeEvents[i];
                
                // Check if event has expired or is complete
                if (evt.HasExpired || evt.isComplete)
                {
                    EndEvent(evt);
                    activeEvents.RemoveAt(i);
                }
            }
        }

        private void ProcessPendingDialogueEvents()
        {
            // Process only one dialogue action per frame to prevent spam
            if (pendingDialogueActions.Count > 0 && UnknownDialogueManager.Instance != null)
            {
                var action = pendingDialogueActions.Dequeue();
                action?.Invoke();
            }
        }

        private void CheckAndScheduleEvents()
        {
            if (Time.time - lastEventTime < minTimeBetweenEvents)
                return;

            // Clean up expired cooldowns
            CleanupCooldowns();

            // Get eligible events for current world state
            var eligibleEvents = GetEligibleEvents();
            
            if (eligibleEvents.Count == 0)
                return;

            // Select event using weighted random selection with deterministic seed
            var selectedProfile = SelectEventDeterministic(eligibleEvents);
            
            if (selectedProfile != null)
            {
                SpawnEvent(selectedProfile);
            }
        }

        private List<ProceduralEventProfile> GetEligibleEvents()
        {
            var eligible = new List<ProceduralEventProfile>();
            int difficultyLevel = DifficultyManager.Instance != null ? (int)DifficultyManager.Instance.CurrentDifficulty : 1;
            int currentChunk = worldState.CurrentChunkId;

            foreach (var profile in eventProfiles)
            {
                // Check if event can spawn
                if (!profile.CanSpawn(worldState, difficultyLevel))
                    continue;

                // Check event cooldown
                if (eventCooldowns.ContainsKey(profile.eventId) && Time.time < eventCooldowns[profile.eventId])
                    continue;

                // Check chunk cooldown
                if (chunkCooldowns.ContainsKey(currentChunk) && Time.time < chunkCooldowns[currentChunk])
                    continue;

                // Check max occurrences
                if (profile.maxOccurrences > 0)
                {
                    int occurrences = eventOccurrences.ContainsKey(profile.eventId) ? eventOccurrences[profile.eventId] : 0;
                    if (occurrences >= profile.maxOccurrences)
                        continue;
                }

                // Check if event is already active
                if (activeEvents.Any(e => e.profile.eventId == profile.eventId && !profile.canRepeat))
                    continue;

                eligible.Add(profile);
            }

            return eligible;
        }

        private ProceduralEventProfile SelectEventDeterministic(List<ProceduralEventProfile> eligible)
        {
            if (eligible.Count == 0)
                return null;

            // Use SeedManager for deterministic selection
            if (SeedManager.Instance != null)
            {
                // Advance scope offset for this check
                SeedManager.Instance.AdvanceScopeOffset(SCOPE_EVENTS, 1);
                
                // Calculate total weight
                float totalWeight = eligible.Sum(e => e.spawnWeight);
                float randomValue = SeedManager.Instance.GetRandomFloat(SCOPE_EVENTS, 0) * totalWeight;
                
                float cumulative = 0f;
                foreach (var profile in eligible)
                {
                    cumulative += profile.spawnWeight;
                    if (randomValue <= cumulative)
                    {
                        return profile;
                    }
                }
            }
            
            // Fallback to Unity random
            float totalWeightFallback = eligible.Sum(e => e.spawnWeight);
            float randomFallback = UnityEngine.Random.Range(0f, totalWeightFallback);
            float cumulativeFallback = 0f;
            
            foreach (var profile in eligible)
            {
                cumulativeFallback += profile.spawnWeight;
                if (randomFallback <= cumulativeFallback)
                {
                    return profile;
                }
            }

            return eligible[eligible.Count - 1];
        }

        private void SpawnEvent(ProceduralEventProfile profile)
        {
            var evt = new ActiveDynamicEvent(profile, worldState.CurrentChunkId);
            activeEvents.Add(evt);
            
            // Update cooldowns
            eventCooldowns[profile.eventId] = Time.time + profile.eventCooldown;
            chunkCooldowns[worldState.CurrentChunkId] = Time.time + profile.chunkEventCooldown;
            lastEventTime = Time.time;
            
            // Track occurrences
            if (!eventOccurrences.ContainsKey(profile.eventId))
                eventOccurrences[profile.eventId] = 0;
            eventOccurrences[profile.eventId]++;

            // Update world state
            var chunkState = worldState.GetChunkState(worldState.CurrentChunkId);
            chunkState.hasActiveEvent = true;
            chunkState.activeEventId = evt.eventId;

            // Execute event-specific logic
            ExecuteEventLogic(evt);

            // Trigger Unknown dialogue
            if (profile.triggerDialogueOnStart)
            {
                TriggerEventDialogue(evt, true);
            }

            if (enablePerformanceLogging)
            {
                Debug.Log($"Spawned event: {profile.eventName} ({profile.eventType}) in chunk {worldState.CurrentChunkId}");
            }
        }

        private void ExecuteEventLogic(ActiveDynamicEvent evt)
        {
            switch (evt.profile.eventType)
            {
                case DynamicEventType.Combat:
                    ExecuteCombatEvent(evt);
                    break;
                case DynamicEventType.Puzzle:
                    ExecutePuzzleEvent(evt);
                    break;
                case DynamicEventType.Ambient:
                    ExecuteAmbientEvent(evt);
                    break;
                case DynamicEventType.Discovery:
                    ExecuteDiscoveryEvent(evt);
                    break;
                case DynamicEventType.Ambush:
                    ExecuteAmbushEvent(evt);
                    break;
            }
        }

        private void ExecuteCombatEvent(ActiveDynamicEvent evt)
        {
            // Spawn NPCs using NPCManager if available
            if (NPCManager.Instance != null)
            {
                // Get spawn position near player
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                {
                    Vector3 spawnCenter = player.transform.position + player.transform.forward * 15f;
                    
                    // Use SeedManager for deterministic NPC spawning
                    int seed = SeedManager.Instance != null ? 
                        SeedManager.Instance.GetSeed(SeedManager.SCOPE_ENCOUNTERS, evt.chunkId) : 
                        UnityEngine.Random.Range(0, 10000);
                    
                    // Track NPCs for this event (would need NPCManager integration)
                    // This is a placeholder for actual spawn logic
                    Debug.Log($"Combat event: Spawning {evt.profile.npcSpawnCount} NPCs at chunk {evt.chunkId}");
                }
            }
        }

        private void ExecutePuzzleEvent(ActiveDynamicEvent evt)
        {
            // Placeholder for puzzle spawning logic
            Debug.Log($"Puzzle event: Complexity {evt.profile.puzzleComplexity} at chunk {evt.chunkId}");
        }

        private void ExecuteAmbientEvent(ActiveDynamicEvent evt)
        {
            // Placeholder for ambient effects (audio, VFX, etc.)
            Debug.Log($"Ambient event: Intensity {evt.profile.ambientIntensity} at chunk {evt.chunkId}");
        }

        private void ExecuteDiscoveryEvent(ActiveDynamicEvent evt)
        {
            // Placeholder for discovery spawning logic
            Debug.Log($"Discovery event at chunk {evt.chunkId}");
        }

        private void ExecuteAmbushEvent(ActiveDynamicEvent evt)
        {
            // Similar to combat but with element of surprise
            Debug.Log($"Ambush event at chunk {evt.chunkId}");
        }

        private void EndEvent(ActiveDynamicEvent evt)
        {
            // Clean up spawned objects
            foreach (var obj in evt.spawnedObjects)
            {
                if (obj != null)
                {
                    Destroy(obj);
                }
            }

            // Update world state
            var chunkState = worldState.GetChunkState(evt.chunkId);
            chunkState.hasActiveEvent = false;
            chunkState.activeEventId = string.Empty;
            chunkState.completedEventIds.Add(evt.eventId);

            // Trigger dialogue on end
            if (evt.profile.triggerDialogueOnEnd)
            {
                TriggerEventDialogue(evt, false);
            }

            if (enablePerformanceLogging)
            {
                Debug.Log($"Ended event: {evt.profile.eventName} after {evt.ElapsedTime:F1}s");
            }
        }

        private void TriggerEventDialogue(ActiveDynamicEvent evt, bool isStart)
        {
            var context = evt.profile.GetDialogueContext(worldState);
            
            // Queue dialogue trigger to prevent same-frame spam
            pendingDialogueActions.Enqueue(() =>
            {
                if (isStart)
                {
                    UnknownDialogueTriggers.TriggerDynamicEventStarted(
                        evt.eventId,
                        evt.profile.eventType.ToString(),
                        evt.chunkId,
                        worldState.GetChunkThreatLevel(evt.chunkId),
                        context
                    );
                }
                else
                {
                    UnknownDialogueTriggers.TriggerDynamicEventResolved(
                        evt.eventId,
                        true,
                        evt.ElapsedTime,
                        context
                    );
                }
            });
        }

        private void CleanupCooldowns()
        {
            var expiredEventCooldowns = eventCooldowns.Where(kvp => Time.time >= kvp.Value).Select(kvp => kvp.Key).ToList();
            foreach (var key in expiredEventCooldowns)
            {
                eventCooldowns.Remove(key);
            }

            var expiredChunkCooldowns = chunkCooldowns.Where(kvp => Time.time >= kvp.Value).Select(kvp => kvp.Key).ToList();
            foreach (var key in expiredChunkCooldowns)
            {
                chunkCooldowns.Remove(key);
            }
        }

        // Public API for external systems

        public void SetChunk(int chunkId, Vector3 position)
        {
            worldState.SetCurrentChunk(chunkId, position);
        }

        public void SetThreatLevel(int chunkId, float threatLevel)
        {
            worldState.SetChunkThreatLevel(chunkId, threatLevel);
        }

        public void RecordPlayerAction(PlayerActionType actionType)
        {
            worldState.RecordPlayerAction(actionType);
        }

        public void SetMissionFlag(string flagName, bool value)
        {
            worldState.SetMissionFlag(flagName, value);
        }

        public void SetMissionPhase(int phase)
        {
            worldState.SetMissionPhase(phase);
        }

        public void SetGameProgress(float progress)
        {
            worldState.SetGameProgress(progress);
        }

        public void CompleteEvent(string eventId, bool success)
        {
            var evt = activeEvents.FirstOrDefault(e => e.eventId == eventId);
            if (evt != null)
            {
                evt.isComplete = true;
                
                // Trigger dialogue for completion
                UnknownDialogueTriggers.TriggerDynamicEventResolved(
                    eventId,
                    success,
                    evt.ElapsedTime
                );
            }
        }

        public void TriggerEventMilestone(string eventId, string milestoneName, int progress)
        {
            var evt = activeEvents.FirstOrDefault(e => e.eventId == eventId);
            if (evt != null)
            {
                evt.progress = progress;
                UnknownDialogueTriggers.TriggerDynamicEventMilestone(eventId, milestoneName, progress);
            }
        }

        public void AddEventProfile(ProceduralEventProfile profile)
        {
            if (!eventProfiles.Contains(profile))
            {
                eventProfiles.Add(profile);
            }
        }

        public void RemoveEventProfile(ProceduralEventProfile profile)
        {
            eventProfiles.Remove(profile);
        }

        private void OnGUI()
        {
            if (!showDebugInfo) return;

            GUIStyle style = new GUIStyle(GUI.skin.box);
            style.normal.textColor = Color.white;
            style.fontSize = 12;
            style.alignment = TextAnchor.UpperLeft;

            string debugText = GetDebugInfo();
            Vector2 size = style.CalcSize(new GUIContent(debugText));
            Rect rect = new Rect(Screen.width - size.x - 20, 20, size.x + 10, size.y + 10);

            GUI.Box(rect, debugText, style);
        }

        private void OnDrawGizmos()
        {
            if (!visualizeUpcomingEvents || !Application.isPlaying) return;

            foreach (var evt in activeEvents)
            {
                Color color = evt.profile.eventType switch
                {
                    DynamicEventType.Combat => Color.red,
                    DynamicEventType.Puzzle => Color.blue,
                    DynamicEventType.Ambient => Color.green,
                    _ => Color.yellow
                };

                Gizmos.color = color;
                Gizmos.DrawWireSphere(worldState.CurrentChunkPosition, 5f);
                Gizmos.DrawLine(worldState.CurrentChunkPosition, worldState.CurrentChunkPosition + Vector3.up * 10f);
            }
        }

        private string GetDebugInfo()
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("<b>DYNAMIC EVENT ORCHESTRATOR</b>");
            sb.AppendLine($"Orchestration: {(enableOrchestration ? "ON" : "OFF")}");
            sb.AppendLine($"Active Events: {activeEvents.Count}");
            sb.AppendLine($"Event Cooldowns: {eventCooldowns.Count}");
            sb.AppendLine($"Pending Dialogue: {pendingDialogueActions.Count}");
            sb.AppendLine();
            sb.AppendLine(worldState.GetDebugInfo());
            sb.AppendLine();
            
            if (activeEvents.Count > 0)
            {
                sb.AppendLine("<b>ACTIVE EVENTS:</b>");
                foreach (var evt in activeEvents)
                {
                    sb.AppendLine($"- {evt.profile.eventName} ({evt.profile.eventType})");
                    sb.AppendLine($"  Time: {evt.ElapsedTime:F1}s / {evt.duration:F1}s");
                    sb.AppendLine($"  Chunk: {evt.chunkId}");
                }
            }
            
            sb.AppendLine($"\nPress {debugToggleKey} to toggle");
            return sb.ToString();
        }
    }
}
