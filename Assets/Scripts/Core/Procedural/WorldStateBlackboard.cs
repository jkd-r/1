using UnityEngine;
using System;
using System.Collections.Generic;

namespace ProtocolEMR.Core.Procedural
{
    /// <summary>
    /// Stores current world state data for dynamic event orchestration.
    /// Tracks chunk information, threat levels, player statistics, and mission states.
    /// </summary>
    [Serializable]
    public class WorldStateBlackboard
    {
        [Header("Chunk State")]
        [SerializeField] private int currentChunkId = 0;
        [SerializeField] private Vector3 currentChunkPosition = Vector3.zero;
        [SerializeField] private Dictionary<int, ChunkState> chunkStates = new Dictionary<int, ChunkState>();
        
        [Header("Threat Levels")]
        [SerializeField] private float globalThreatLevel = 0f;
        [SerializeField] private Dictionary<int, float> chunkThreatLevels = new Dictionary<int, float>();
        
        [Header("Player Statistics")]
        [SerializeField] private PlayerStyleStats playerStyle = new PlayerStyleStats();
        
        [Header("Mission State")]
        [SerializeField] private Dictionary<string, bool> missionFlags = new Dictionary<string, bool>();
        [SerializeField] private int currentMissionPhase = 0;
        [SerializeField] private float gameProgress = 0f; // 0 to 1
        
        public int CurrentChunkId => currentChunkId;
        public Vector3 CurrentChunkPosition => currentChunkPosition;
        public float GlobalThreatLevel => globalThreatLevel;
        public PlayerStyleStats PlayerStyle => playerStyle;
        public float GameProgress => gameProgress;
        public int CurrentMissionPhase => currentMissionPhase;

        public WorldStateBlackboard()
        {
            playerStyle = new PlayerStyleStats();
        }

        /// <summary>
        /// Updates the current chunk.
        /// </summary>
        public void SetCurrentChunk(int chunkId, Vector3 position)
        {
            currentChunkId = chunkId;
            currentChunkPosition = position;
            
            if (!chunkStates.ContainsKey(chunkId))
            {
                chunkStates[chunkId] = new ChunkState(chunkId, position);
            }
            
            chunkStates[chunkId].lastVisitTime = Time.time;
            chunkStates[chunkId].visitCount++;
        }

        /// <summary>
        /// Gets state for a specific chunk, creating it if it doesn't exist.
        /// </summary>
        public ChunkState GetChunkState(int chunkId)
        {
            if (!chunkStates.ContainsKey(chunkId))
            {
                chunkStates[chunkId] = new ChunkState(chunkId, Vector3.zero);
            }
            return chunkStates[chunkId];
        }

        /// <summary>
        /// Updates threat level for a specific chunk.
        /// </summary>
        public void SetChunkThreatLevel(int chunkId, float threatLevel)
        {
            chunkThreatLevels[chunkId] = Mathf.Clamp01(threatLevel);
            RecalculateGlobalThreat();
        }

        /// <summary>
        /// Gets threat level for a specific chunk.
        /// </summary>
        public float GetChunkThreatLevel(int chunkId)
        {
            return chunkThreatLevels.ContainsKey(chunkId) ? chunkThreatLevels[chunkId] : 0f;
        }

        /// <summary>
        /// Recalculates global threat level based on all chunks.
        /// </summary>
        private void RecalculateGlobalThreat()
        {
            if (chunkThreatLevels.Count == 0)
            {
                globalThreatLevel = 0f;
                return;
            }

            float sum = 0f;
            foreach (var kvp in chunkThreatLevels)
            {
                sum += kvp.Value;
            }
            globalThreatLevel = sum / chunkThreatLevels.Count;
        }

        /// <summary>
        /// Updates player style statistics.
        /// </summary>
        public void RecordPlayerAction(PlayerActionType actionType)
        {
            playerStyle.RecordAction(actionType);
        }

        /// <summary>
        /// Sets or updates a mission flag.
        /// </summary>
        public void SetMissionFlag(string flagName, bool value)
        {
            missionFlags[flagName] = value;
        }

        /// <summary>
        /// Gets a mission flag value.
        /// </summary>
        public bool GetMissionFlag(string flagName)
        {
            return missionFlags.ContainsKey(flagName) && missionFlags[flagName];
        }

        /// <summary>
        /// Sets the current mission phase.
        /// </summary>
        public void SetMissionPhase(int phase)
        {
            currentMissionPhase = phase;
        }

        /// <summary>
        /// Sets game progress (0 to 1).
        /// </summary>
        public void SetGameProgress(float progress)
        {
            gameProgress = Mathf.Clamp01(progress);
        }

        /// <summary>
        /// Gets a debug string representation of the world state.
        /// </summary>
        public string GetDebugInfo()
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"Current Chunk: {currentChunkId} at {currentChunkPosition}");
            sb.AppendLine($"Global Threat: {globalThreatLevel:F2}");
            sb.AppendLine($"Game Progress: {gameProgress:P0}");
            sb.AppendLine($"Mission Phase: {currentMissionPhase}");
            sb.AppendLine($"Player Style - Stealth: {playerStyle.StealthRatio:P0}, Combat: {playerStyle.CombatRatio:P0}, Exploration: {playerStyle.ExplorationRatio:P0}");
            sb.AppendLine($"Active Mission Flags: {missionFlags.Count}");
            return sb.ToString();
        }
    }

    /// <summary>
    /// State data for a single chunk.
    /// </summary>
    [Serializable]
    public class ChunkState
    {
        public int chunkId;
        public Vector3 position;
        public float lastVisitTime;
        public int visitCount;
        public bool hasActiveEvent;
        public string activeEventId;
        public List<string> completedEventIds = new List<string>();
        
        public ChunkState(int id, Vector3 pos)
        {
            chunkId = id;
            position = pos;
            lastVisitTime = Time.time;
            visitCount = 0;
            hasActiveEvent = false;
            activeEventId = string.Empty;
        }
    }

    /// <summary>
    /// Tracks player playstyle statistics.
    /// </summary>
    [Serializable]
    public class PlayerStyleStats
    {
        private int totalActions = 0;
        private int stealthActions = 0;
        private int combatActions = 0;
        private int explorationActions = 0;
        private int puzzleActions = 0;

        public int TotalActions => totalActions;
        public int StealthActions => stealthActions;
        public int CombatActions => combatActions;
        public int ExplorationActions => explorationActions;
        public int PuzzleActions => puzzleActions;

        public float StealthRatio => totalActions > 0 ? (float)stealthActions / totalActions : 0f;
        public float CombatRatio => totalActions > 0 ? (float)combatActions / totalActions : 0f;
        public float ExplorationRatio => totalActions > 0 ? (float)explorationActions / totalActions : 0f;
        public float PuzzleRatio => totalActions > 0 ? (float)puzzleActions / totalActions : 0f;

        public void RecordAction(PlayerActionType actionType)
        {
            totalActions++;
            
            switch (actionType)
            {
                case PlayerActionType.Stealth:
                    stealthActions++;
                    break;
                case PlayerActionType.Combat:
                    combatActions++;
                    break;
                case PlayerActionType.Exploration:
                    explorationActions++;
                    break;
                case PlayerActionType.Puzzle:
                    puzzleActions++;
                    break;
            }
        }
    }

    /// <summary>
    /// Types of player actions for style tracking.
    /// </summary>
    public enum PlayerActionType
    {
        Stealth,
        Combat,
        Exploration,
        Puzzle
    }
}
