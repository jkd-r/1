using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;
using ProtocolEMR.Systems;
using ProtocolEMR.Core.Procedural;

namespace ProtocolEMR.Systems.SaveLoad
{
    /// <summary>
    /// Coordinates capture and restoration of global world state data.
    /// </summary>
    public static class WorldStateManager
    {
        public static WorldStateSnapshot CaptureWorldState()
        {
            var snapshot = new WorldStateSnapshot
            {
                sceneName = SceneManager.GetActiveScene().name,
                timestampUtc = DateTime.UtcNow.ToString("o")
            };

            var objectManager = ObjectStateManager.Instance;
            if (objectManager != null)
            {
                objectManager.RecordProceduralState();
                snapshot.objectStates = objectManager.GetAllObjectStates();
                snapshot.proceduralState = Clone(objectManager.GetProceduralState()) ?? new ProceduralState();
            }

            if (ProceduralStateStore.Instance != null)
            {
                ProceduralStateStore.Instance.SaveProceduralState();
                snapshot.proceduralSeedState = Clone(ProceduralStateStore.Instance.GetCurrentState()) ?? new ProceduralStateData();
            }

            var orchestrator = DynamicEventOrchestrator.Instance;
            if (orchestrator != null)
            {
                snapshot.blackboard = WorldBlackboardSnapshot.FromBlackboard(orchestrator.WorldState);
            }

            return snapshot;
        }

        public static void ApplyWorldState(WorldStateSnapshot snapshot)
        {
            if (snapshot == null)
                return;

            var objectManager = ObjectStateManager.Instance;
            if (objectManager != null)
            {
                objectManager.RestoreWorldState(snapshot.objectStates, snapshot.proceduralState);
            }

            ApplyProceduralSeed(snapshot.proceduralSeedState);
            ApplyWorldBlackboard(snapshot.blackboard);
        }

        private static void ApplyProceduralSeed(ProceduralStateData state)
        {
            if (state == null)
                return;

            if (SeedManager.Instance != null)
            {
                SeedManager.Instance.SetSeed(state.seed);

                if (state.scopeOffsets != null)
                {
                    foreach (var scope in state.scopeOffsets)
                    {
                        if (!string.IsNullOrEmpty(scope.scope))
                        {
                            SeedManager.Instance.SetScopeOffset(scope.scope, scope.offset);
                        }
                    }
                }
            }

            if (ProceduralStateStore.Instance != null)
            {
                var current = ProceduralStateStore.Instance.GetCurrentState();
                if (current != null)
                {
                    current.seed = state.seed;
                    current.timestamp = state.timestamp;
                    current.scopeOffsets = new List<ProceduralScopeOffset>(state.scopeOffsets ?? new List<ProceduralScopeOffset>());
                }
            }
        }

        private static void ApplyWorldBlackboard(WorldBlackboardSnapshot snapshot)
        {
            if (snapshot == null)
                return;

            var orchestrator = DynamicEventOrchestrator.Instance;
            if (orchestrator == null)
                return;

            snapshot.ApplyTo(orchestrator.WorldState);
        }

        private static T Clone<T>(T source)
        {
            if (source == null)
                return default;

            return JsonUtility.FromJson<T>(JsonUtility.ToJson(source));
        }
    }

    /// <summary>
    /// Complete snapshot of world-level data.
    /// </summary>
    [Serializable]
    public class WorldStateSnapshot
    {
        public string sceneName = string.Empty;
        public string timestampUtc = DateTime.UtcNow.ToString("o");
        public List<ObjectStateManager.ObjectState> objectStates = new List<ObjectStateManager.ObjectState>();
        public ProceduralState proceduralState = new ProceduralState();
        public ProceduralStateData proceduralSeedState = new ProceduralStateData();
        public WorldBlackboardSnapshot blackboard = new WorldBlackboardSnapshot();
    }

    /// <summary>
    /// Serializable copy of the dynamic world blackboard.
    /// </summary>
    [Serializable]
    public class WorldBlackboardSnapshot
    {
        public int currentChunkId;
        public SerializableVector3 currentChunkPosition;
        public float globalThreatLevel;
        public float gameProgress;
        public int missionPhase;
        public PlayerStyleStatsSnapshot playerStyle = new PlayerStyleStatsSnapshot();
        public List<ChunkStateSnapshot> chunks = new List<ChunkStateSnapshot>();
        public List<NamedFloatValue> chunkThreatLevels = new List<NamedFloatValue>();
        public List<MissionFlagSnapshot> missionFlags = new List<MissionFlagSnapshot>();

        public static WorldBlackboardSnapshot FromBlackboard(WorldStateBlackboard blackboard)
        {
            var snapshot = new WorldBlackboardSnapshot();
            if (blackboard == null)
                return snapshot;

            snapshot.currentChunkId = blackboard.CurrentChunkId;
            snapshot.currentChunkPosition = SerializableVector3.FromVector3(blackboard.CurrentChunkPosition);
            snapshot.globalThreatLevel = blackboard.GlobalThreatLevel;
            snapshot.gameProgress = blackboard.GameProgress;
            snapshot.missionPhase = blackboard.CurrentMissionPhase;
            snapshot.playerStyle = PlayerStyleStatsSnapshot.FromStats(blackboard.PlayerStyle);

            var chunkStates = ChunkStatesField?.GetValue(blackboard) as Dictionary<int, ChunkState>;
            if (chunkStates != null)
            {
                foreach (var entry in chunkStates)
                {
                    if (entry.Value != null)
                    {
                        snapshot.chunks.Add(ChunkStateSnapshot.FromChunkState(entry.Value));
                    }
                }
            }

            var chunkThreat = ChunkThreatField?.GetValue(blackboard) as Dictionary<int, float>;
            if (chunkThreat != null)
            {
                foreach (var entry in chunkThreat)
                {
                    snapshot.chunkThreatLevels.Add(new NamedFloatValue
                    {
                        key = entry.Key.ToString(),
                        value = entry.Value
                    });
                }
            }

            var missionDict = MissionFlagsField?.GetValue(blackboard) as Dictionary<string, bool>;
            if (missionDict != null)
            {
                foreach (var entry in missionDict)
                {
                    snapshot.missionFlags.Add(new MissionFlagSnapshot
                    {
                        flagName = entry.Key,
                        value = entry.Value
                    });
                }
            }

            return snapshot;
        }

        public void ApplyTo(WorldStateBlackboard blackboard)
        {
            if (blackboard == null)
                return;

            CurrentChunkIdField?.SetValue(blackboard, currentChunkId);
            CurrentChunkPositionField?.SetValue(blackboard, currentChunkPosition.ToVector3());
            GlobalThreatField?.SetValue(blackboard, globalThreatLevel);
            GameProgressField?.SetValue(blackboard, gameProgress);
            MissionPhaseField?.SetValue(blackboard, missionPhase);

            playerStyle?.ApplyTo(blackboard.PlayerStyle);

            var chunkStates = ChunkStatesField?.GetValue(blackboard) as Dictionary<int, ChunkState>;
            if (chunkStates != null)
            {
                chunkStates.Clear();
                foreach (var chunk in chunks)
                {
                    var rebuilt = chunk.ToChunkState();
                    chunkStates[rebuilt.chunkId] = rebuilt;
                }
            }

            var chunkThreat = ChunkThreatField?.GetValue(blackboard) as Dictionary<int, float>;
            if (chunkThreat != null)
            {
                chunkThreat.Clear();
                foreach (var entry in chunkThreatLevels)
                {
                    if (entry == null)
                        continue;

                    if (int.TryParse(entry.key, out int chunkId))
                    {
                        chunkThreat[chunkId] = entry.value;
                    }
                }
            }

            var missionDict = MissionFlagsField?.GetValue(blackboard) as Dictionary<string, bool>;
            if (missionDict != null)
            {
                missionDict.Clear();
                foreach (var flag in missionFlags)
                {
                    if (flag != null && !string.IsNullOrEmpty(flag.flagName))
                    {
                        missionDict[flag.flagName] = flag.value;
                    }
                }
            }
        }

        #region Reflection Cache

        private static readonly FieldInfo ChunkStatesField = typeof(WorldStateBlackboard)
            .GetField("chunkStates", BindingFlags.NonPublic | BindingFlags.Instance);

        private static readonly FieldInfo ChunkThreatField = typeof(WorldStateBlackboard)
            .GetField("chunkThreatLevels", BindingFlags.NonPublic | BindingFlags.Instance);

        private static readonly FieldInfo MissionFlagsField = typeof(WorldStateBlackboard)
            .GetField("missionFlags", BindingFlags.NonPublic | BindingFlags.Instance);

        private static readonly FieldInfo CurrentChunkIdField = typeof(WorldStateBlackboard)
            .GetField("currentChunkId", BindingFlags.NonPublic | BindingFlags.Instance);

        private static readonly FieldInfo CurrentChunkPositionField = typeof(WorldStateBlackboard)
            .GetField("currentChunkPosition", BindingFlags.NonPublic | BindingFlags.Instance);

        private static readonly FieldInfo GlobalThreatField = typeof(WorldStateBlackboard)
            .GetField("globalThreatLevel", BindingFlags.NonPublic | BindingFlags.Instance);

        private static readonly FieldInfo GameProgressField = typeof(WorldStateBlackboard)
            .GetField("gameProgress", BindingFlags.NonPublic | BindingFlags.Instance);

        private static readonly FieldInfo MissionPhaseField = typeof(WorldStateBlackboard)
            .GetField("currentMissionPhase", BindingFlags.NonPublic | BindingFlags.Instance);

        #endregion
    }

    /// <summary>
    /// Serializable chunk entry.
    /// </summary>
    [Serializable]
    public class ChunkStateSnapshot
    {
        public int chunkId;
        public SerializableVector3 position;
        public float lastVisitTime;
        public int visitCount;
        public bool hasActiveEvent;
        public string activeEventId;
        public List<string> completedEvents = new List<string>();

        public static ChunkStateSnapshot FromChunkState(ChunkState chunk)
        {
            return new ChunkStateSnapshot
            {
                chunkId = chunk.chunkId,
                position = SerializableVector3.FromVector3(chunk.position),
                lastVisitTime = chunk.lastVisitTime,
                visitCount = chunk.visitCount,
                hasActiveEvent = chunk.hasActiveEvent,
                activeEventId = chunk.activeEventId,
                completedEvents = new List<string>(chunk.completedEventIds ?? new List<string>())
            };
        }

        public ChunkState ToChunkState()
        {
            var state = new ChunkState(chunkId, position.ToVector3())
            {
                lastVisitTime = lastVisitTime,
                visitCount = visitCount,
                hasActiveEvent = hasActiveEvent,
                activeEventId = activeEventId ?? string.Empty,
                completedEventIds = new List<string>(completedEvents ?? new List<string>())
            };
            return state;
        }
    }

    /// <summary>
    /// Serializable mission flag entry.
    /// </summary>
    [Serializable]
    public class MissionFlagSnapshot
    {
        public string flagName;
        public bool value;
    }

    /// <summary>
    /// Serializable snapshot of player style stats with reflection helpers.
    /// </summary>
    [Serializable]
    public class PlayerStyleStatsSnapshot
    {
        public int totalActions;
        public int stealthActions;
        public int combatActions;
        public int explorationActions;
        public int puzzleActions;

        public static PlayerStyleStatsSnapshot FromStats(PlayerStyleStats stats)
        {
            if (stats == null)
                return new PlayerStyleStatsSnapshot();

            return new PlayerStyleStatsSnapshot
            {
                totalActions = stats.TotalActions,
                stealthActions = stats.StealthActions,
                combatActions = stats.CombatActions,
                explorationActions = stats.ExplorationActions,
                puzzleActions = stats.PuzzleActions
            };
        }

        public void ApplyTo(PlayerStyleStats stats)
        {
            if (stats == null)
                return;

            TotalActionsField?.SetValue(stats, Math.Max(0, totalActions));
            StealthActionsField?.SetValue(stats, Math.Max(0, stealthActions));
            CombatActionsField?.SetValue(stats, Math.Max(0, combatActions));
            ExplorationActionsField?.SetValue(stats, Math.Max(0, explorationActions));
            PuzzleActionsField?.SetValue(stats, Math.Max(0, puzzleActions));
        }

        private static readonly FieldInfo TotalActionsField = typeof(PlayerStyleStats)
            .GetField("totalActions", BindingFlags.NonPublic | BindingFlags.Instance);

        private static readonly FieldInfo StealthActionsField = typeof(PlayerStyleStats)
            .GetField("stealthActions", BindingFlags.NonPublic | BindingFlags.Instance);

        private static readonly FieldInfo CombatActionsField = typeof(PlayerStyleStats)
            .GetField("combatActions", BindingFlags.NonPublic | BindingFlags.Instance);

        private static readonly FieldInfo ExplorationActionsField = typeof(PlayerStyleStats)
            .GetField("explorationActions", BindingFlags.NonPublic | BindingFlags.Instance);

        private static readonly FieldInfo PuzzleActionsField = typeof(PlayerStyleStats)
            .GetField("puzzleActions", BindingFlags.NonPublic | BindingFlags.Instance);
    }
}
