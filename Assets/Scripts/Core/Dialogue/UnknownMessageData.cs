using UnityEngine;
using System;
using System.Collections.Generic;

namespace ProtocolEMR.Core.Dialogue
{
    /// <summary>
    /// Categories for Unknown messages based on context.
    /// </summary>
    public enum MessageCategory
    {
        Combat,
        Puzzle,
        Exploration,
        Mission,
        Narrative,
        Warning,
        Encouragement,
        Commentary
    }

    /// <summary>
    /// Specific trigger conditions for messages.
    /// </summary>
    public enum MessageTrigger
    {
        // Combat triggers
        PlayerHitNPC,
        PlayerTookDamage,
        NPCDefeated,
        PlayerLowHealth,
        DodgeSuccessful,
        CombatStarted,
        
        // Puzzle triggers
        PuzzleEncountered,
        PuzzleAttemptFailed,
        PuzzleSolved,
        PuzzleSolvedPerfectly,
        PlayerStuck,
        
        // Exploration triggers
        NewAreaDiscovered,
        SecretFound,
        NPCEncountered,
        ItemFound,
        DangerDetected,
        
        // Mission triggers
        MissionStart,
        MissionMilestone,
        MissionComplete,
        ObjectiveFailed,
        NewMissionAvailable,
        
        // Story triggers
        PlotPointReached,
        ProceduralStoryMilestone,
        MajorEventOccurred,
        SecretDiscovered,
        
        // Player behavior
        StealthApproach,
        AggressiveApproach,
        DocumentRead,
        
        // General
        GameStart,
        PlayerDeath,
        Custom
    }

    /// <summary>
    /// Display mode for Unknown messages.
    /// </summary>
    public enum MessageDisplayMode
    {
        Phone,
        HUDOverlay,
        Both
    }

    /// <summary>
    /// Represents a single Unknown message.
    /// </summary>
    [Serializable]
    public class UnknownMessage
    {
        [Header("Message Content")]
        [TextArea(2, 4)]
        public string text;
        
        [Header("Categorization")]
        public MessageCategory category;
        public MessageTrigger trigger;
        
        [Header("Context")]
        [Tooltip("Minimum difficulty level required for this message")]
        public int minDifficultyLevel = 0;
        
        [Tooltip("Maximum difficulty level for this message")]
        public int maxDifficultyLevel = 3;
        
        [Tooltip("Weight for selection algorithm (higher = more likely)")]
        [Range(0.1f, 10f)]
        public float selectionWeight = 1f;
        
        [Header("Display Settings")]
        public MessageDisplayMode displayMode = MessageDisplayMode.Both;
        
        [Tooltip("Delay before message appears (seconds)")]
        [Range(0f, 3f)]
        public float displayDelay = 0.5f;
        
        [Tooltip("Duration message stays visible (0 = until dismissed)")]
        [Range(0f, 10f)]
        public float displayDuration = 3f;
        
        [Header("Conditions")]
        [Tooltip("Cooldown before this message can appear again (seconds)")]
        [Range(0f, 300f)]
        public float cooldown = 60f;
        
        [Tooltip("Can this message repeat or is it one-time only?")]
        public bool canRepeat = true;
        
        [Tooltip("Game progress stage (0 = early, 1 = mid, 2 = late)")]
        [Range(0, 2)]
        public int gameStage = 0;
        
        public UnknownMessage(string text, MessageCategory category, MessageTrigger trigger)
        {
            this.text = text;
            this.category = category;
            this.trigger = trigger;
        }
    }

    /// <summary>
    /// ScriptableObject database containing all Unknown messages.
    /// </summary>
    [CreateAssetMenu(fileName = "UnknownMessageDatabase", menuName = "Protocol EMR/Dialogue/Message Database")]
    public class UnknownMessageDatabase : ScriptableObject
    {
        [Header("Message Collection")]
        public List<UnknownMessage> messages = new List<UnknownMessage>();
        
        [Header("Settings")]
        [Tooltip("Global cooldown between any messages (seconds)")]
        [Range(0f, 30f)]
        public float globalMessageCooldown = 5f;
        
        [Tooltip("Maximum messages stored in history")]
        public int maxHistorySize = 100;
        
        public List<UnknownMessage> GetMessagesByCategory(MessageCategory category)
        {
            return messages.FindAll(m => m.category == category);
        }
        
        public List<UnknownMessage> GetMessagesByTrigger(MessageTrigger trigger)
        {
            return messages.FindAll(m => m.trigger == trigger);
        }
        
        public List<UnknownMessage> GetMessagesForContext(MessageTrigger trigger, int difficultyLevel, int gameStage)
        {
            return messages.FindAll(m => 
                m.trigger == trigger &&
                difficultyLevel >= m.minDifficultyLevel &&
                difficultyLevel <= m.maxDifficultyLevel &&
                m.gameStage == gameStage
            );
        }
    }

    /// <summary>
    /// Tracks message display history.
    /// </summary>
    [Serializable]
    public class MessageHistory
    {
        public string messageText;
        public MessageTrigger trigger;
        public float timestamp;
        public bool wasDisplayed;
        
        public MessageHistory(string text, MessageTrigger trigger, float timestamp)
        {
            this.messageText = text;
            this.trigger = trigger;
            this.timestamp = timestamp;
            this.wasDisplayed = true;
        }
    }

    /// <summary>
    /// Event data for triggering Unknown messages.
    /// </summary>
    public class UnknownMessageEvent
    {
        public MessageTrigger trigger;
        public GameObject sourceObject;
        public Vector3 position;
        public Dictionary<string, object> contextData;
        
        public UnknownMessageEvent(MessageTrigger trigger, GameObject source = null)
        {
            this.trigger = trigger;
            this.sourceObject = source;
            this.position = source != null ? source.transform.position : Vector3.zero;
            this.contextData = new Dictionary<string, object>();
        }
        
        public void AddContextData(string key, object value)
        {
            contextData[key] = value;
        }
        
        public T GetContextData<T>(string key)
        {
            if (contextData.ContainsKey(key))
            {
                return (T)contextData[key];
            }
            return default(T);
        }
    }
}
