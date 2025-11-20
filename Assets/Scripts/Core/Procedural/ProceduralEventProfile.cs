using UnityEngine;
using System;
using System.Collections.Generic;

namespace ProtocolEMR.Core.Procedural
{
    /// <summary>
    /// Types of dynamic procedural events.
    /// </summary>
    public enum DynamicEventType
    {
        Ambient,
        Combat,
        Puzzle,
        Story,
        Discovery,
        Ambush
    }

    /// <summary>
    /// ScriptableObject configuration for procedural dynamic events.
    /// Defines spawn rules, cooldowns, requirements, and Unknown dialogue integration.
    /// </summary>
    [CreateAssetMenu(fileName = "NewEventProfile", menuName = "Protocol EMR/Procedural/Event Profile")]
    public class ProceduralEventProfile : ScriptableObject
    {
        [Header("Event Identity")]
        [Tooltip("Unique identifier for this event type")]
        public string eventId = "event_001";
        
        [Tooltip("Display name for this event")]
        public string eventName = "New Event";
        
        [Tooltip("Type of event")]
        public DynamicEventType eventType = DynamicEventType.Ambient;
        
        [Header("Spawn Rules")]
        [Tooltip("Minimum difficulty level required (0-3)")]
        [Range(0, 3)]
        public int minDifficultyLevel = 0;
        
        [Tooltip("Maximum difficulty level allowed (0-3)")]
        [Range(0, 3)]
        public int maxDifficultyLevel = 3;
        
        [Tooltip("Minimum game progress required (0-1)")]
        [Range(0f, 1f)]
        public float minGameProgress = 0f;
        
        [Tooltip("Maximum game progress allowed (0-1)")]
        [Range(0f, 1f)]
        public float maxGameProgress = 1f;
        
        [Tooltip("Minimum threat level in chunk required (0-1)")]
        [Range(0f, 1f)]
        public float minThreatLevel = 0f;
        
        [Tooltip("Weight for spawn selection (higher = more likely)")]
        [Range(0.1f, 10f)]
        public float spawnWeight = 1f;
        
        [Header("Cooldowns & Timing")]
        [Tooltip("Cooldown before this event can spawn again (seconds)")]
        public float eventCooldown = 120f;
        
        [Tooltip("Minimum time between any events in same chunk (seconds)")]
        public float chunkEventCooldown = 30f;
        
        [Tooltip("Duration of the event (0 = indefinite)")]
        public float eventDuration = 60f;
        
        [Header("Mission Requirements")]
        [Tooltip("Required mission flags to be set")]
        public List<string> requiredMissionFlags = new List<string>();
        
        [Tooltip("Mission flags that must NOT be set")]
        public List<string> forbiddenMissionFlags = new List<string>();
        
        [Tooltip("Required mission phase (-1 = any)")]
        public int requiredMissionPhase = -1;
        
        [Header("Unknown Dialogue")]
        [Tooltip("Trigger dialogue when event starts")]
        public bool triggerDialogueOnStart = true;
        
        [Tooltip("Trigger dialogue when event ends")]
        public bool triggerDialogueOnEnd = true;
        
        [Tooltip("Custom dialogue tags for context")]
        public List<string> dialogueTags = new List<string>();
        
        [Header("Event Parameters")]
        [Tooltip("Number of NPCs to spawn (for combat events)")]
        public int npcSpawnCount = 3;
        
        [Tooltip("Puzzle complexity (for puzzle events)")]
        [Range(1, 5)]
        public int puzzleComplexity = 2;
        
        [Tooltip("Ambient intensity (for ambient events)")]
        [Range(0f, 1f)]
        public float ambientIntensity = 0.5f;
        
        [Header("Repeatability")]
        [Tooltip("Can this event repeat?")]
        public bool canRepeat = true;
        
        [Tooltip("Maximum times this event can occur (-1 = unlimited)")]
        public int maxOccurrences = -1;

        /// <summary>
        /// Checks if this event can spawn given the current world state.
        /// </summary>
        public bool CanSpawn(WorldStateBlackboard worldState, int difficultyLevel)
        {
            // Check difficulty level
            if (difficultyLevel < minDifficultyLevel || difficultyLevel > maxDifficultyLevel)
                return false;
            
            // Check game progress
            if (worldState.GameProgress < minGameProgress || worldState.GameProgress > maxGameProgress)
                return false;
            
            // Check threat level
            float currentThreat = worldState.GetChunkThreatLevel(worldState.CurrentChunkId);
            if (currentThreat < minThreatLevel)
                return false;
            
            // Check required mission flags
            foreach (string flag in requiredMissionFlags)
            {
                if (!worldState.GetMissionFlag(flag))
                    return false;
            }
            
            // Check forbidden mission flags
            foreach (string flag in forbiddenMissionFlags)
            {
                if (worldState.GetMissionFlag(flag))
                    return false;
            }
            
            // Check mission phase
            if (requiredMissionPhase >= 0 && worldState.CurrentMissionPhase != requiredMissionPhase)
                return false;
            
            return true;
        }

        /// <summary>
        /// Gets context data for Unknown dialogue system.
        /// </summary>
        public Dictionary<string, object> GetDialogueContext(WorldStateBlackboard worldState)
        {
            var context = new Dictionary<string, object>();
            context["eventId"] = eventId;
            context["eventType"] = eventType.ToString();
            context["chunkId"] = worldState.CurrentChunkId;
            context["threatLevel"] = worldState.GetChunkThreatLevel(worldState.CurrentChunkId);
            context["gameProgress"] = worldState.GameProgress;
            context["stealthRatio"] = worldState.PlayerStyle.StealthRatio;
            context["combatRatio"] = worldState.PlayerStyle.CombatRatio;
            context["explorationRatio"] = worldState.PlayerStyle.ExplorationRatio;
            
            foreach (string tag in dialogueTags)
            {
                context[$"tag_{tag}"] = true;
            }
            
            return context;
        }
    }
}
