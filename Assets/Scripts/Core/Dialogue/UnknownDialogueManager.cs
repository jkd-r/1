using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ProtocolEMR.Core.Dialogue
{
    /// <summary>
    /// Central manager for the Unknown entity communication system.
    /// Handles message selection, triggering, history tracking, and personalization.
    /// </summary>
    public class UnknownDialogueManager : MonoBehaviour
    {
        public static UnknownDialogueManager Instance { get; private set; }

        [Header("Database")]
        [SerializeField] private UnknownMessageDatabase messageDatabase;

        [Header("Settings")]
        [SerializeField] [Range(0f, 1f)] private float hintFrequency = 0.5f;
        [SerializeField] [Range(0, 2)] private int gameStage = 0;
        [SerializeField] private int difficultyLevel = 1;

        [Header("Personalization")]
        [SerializeField] private UnknownPersonality personality = UnknownPersonality.Balanced;
        [SerializeField] private bool adaptToPlayerStyle = true;

        [Header("Performance")]
        [SerializeField] private bool enablePerformanceLogging = false;

        private List<MessageHistory> messageHistory = new List<MessageHistory>();
        private Dictionary<string, float> messageCooldowns = new Dictionary<string, float>();
        private float lastMessageTime = 0f;
        private PlayerStyleProfile playerStyleProfile = new PlayerStyleProfile();

        public event Action<UnknownMessage> OnMessageSelected;
        public event Action<UnknownMessage, MessageDisplayMode> OnMessageDisplay;

        public float HintFrequency 
        { 
            get => hintFrequency; 
            set => hintFrequency = Mathf.Clamp01(value); 
        }

        public int GameStage 
        { 
            get => gameStage; 
            set => gameStage = Mathf.Clamp(value, 0, 2); 
        }

        public int DifficultyLevel 
        { 
            get => difficultyLevel; 
            set => difficultyLevel = Mathf.Clamp(value, 0, 3); 
        }

        public UnknownPersonality Personality 
        { 
            get => personality; 
            set => personality = value; 
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            LoadMessageDatabase();
            InitializeSystem();
        }

        private void LoadMessageDatabase()
        {
            if (messageDatabase == null)
            {
                messageDatabase = Resources.Load<UnknownMessageDatabase>("Dialogue/UnknownMessageDatabase");
                
                if (messageDatabase == null)
                {
                    Debug.LogError("UnknownDialogueManager: Message database not found in Resources/Dialogue/");
                }
            }
        }

        private void InitializeSystem()
        {
            Debug.Log("Unknown Dialogue Manager Initialized");
            Debug.Log($"Messages loaded: {(messageDatabase != null ? messageDatabase.messages.Count : 0)}");
        }

        public void TriggerMessage(MessageTrigger trigger, GameObject source = null)
        {
            UnknownMessageEvent evt = new UnknownMessageEvent(trigger, source);
            TriggerMessage(evt);
        }

        public void TriggerMessage(UnknownMessageEvent messageEvent)
        {
            if (messageDatabase == null)
            {
                Debug.LogWarning("UnknownDialogueManager: No message database loaded");
                return;
            }

            float startTime = enablePerformanceLogging ? Time.realtimeSinceStartup : 0f;

            if (!ShouldTriggerMessage(messageEvent))
            {
                return;
            }

            UnknownMessage selectedMessage = SelectMessage(messageEvent);
            
            if (selectedMessage != null)
            {
                DisplayMessage(selectedMessage);
                RecordMessageHistory(selectedMessage);
                UpdatePlayerStyleProfile(messageEvent);
            }

            if (enablePerformanceLogging)
            {
                float elapsed = (Time.realtimeSinceStartup - startTime) * 1000f;
                Debug.Log($"UnknownDialogueManager: Message selection took {elapsed:F3}ms");
            }
        }

        private bool ShouldTriggerMessage(UnknownMessageEvent messageEvent)
        {
            if (Time.time - lastMessageTime < messageDatabase.globalMessageCooldown)
            {
                return false;
            }

            float roll = UnityEngine.Random.value;
            if (roll > hintFrequency)
            {
                return false;
            }

            return true;
        }

        private UnknownMessage SelectMessage(UnknownMessageEvent messageEvent)
        {
            List<UnknownMessage> candidates = GetEligibleMessages(messageEvent);
            
            if (candidates.Count == 0)
            {
                return null;
            }

            candidates = ApplyPersonalityFilter(candidates, messageEvent);
            candidates = RemoveRecentMessages(candidates);
            candidates = ApplyPlayerStyleFilter(candidates);

            if (candidates.Count == 0)
            {
                return null;
            }

            UnknownMessage selected = WeightedRandomSelection(candidates);
            OnMessageSelected?.Invoke(selected);
            
            return selected;
        }

        private List<UnknownMessage> GetEligibleMessages(UnknownMessageEvent messageEvent)
        {
            List<UnknownMessage> eligible = new List<UnknownMessage>();

            foreach (UnknownMessage msg in messageDatabase.messages)
            {
                if (msg.trigger != messageEvent.trigger)
                    continue;

                if (difficultyLevel < msg.minDifficultyLevel || difficultyLevel > msg.maxDifficultyLevel)
                    continue;

                if (msg.gameStage != gameStage)
                    continue;

                if (!msg.canRepeat && HasMessageBeenShown(msg))
                    continue;

                if (IsMessageOnCooldown(msg))
                    continue;

                eligible.Add(msg);
            }

            return eligible;
        }

        private List<UnknownMessage> ApplyPersonalityFilter(List<UnknownMessage> messages, UnknownMessageEvent messageEvent)
        {
            switch (personality)
            {
                case UnknownPersonality.Verbose:
                    return messages;
                
                case UnknownPersonality.Balanced:
                    return messages;
                
                case UnknownPersonality.Cryptic:
                    return messages.Where(m => m.text.Length < 50).ToList();
                
                default:
                    return messages;
            }
        }

        private List<UnknownMessage> RemoveRecentMessages(List<UnknownMessage> messages)
        {
            if (messageHistory.Count < 3)
                return messages;

            HashSet<string> recentTexts = new HashSet<string>();
            for (int i = Mathf.Max(0, messageHistory.Count - 3); i < messageHistory.Count; i++)
            {
                recentTexts.Add(messageHistory[i].messageText);
            }

            return messages.Where(m => !recentTexts.Contains(m.text)).ToList();
        }

        private List<UnknownMessage> ApplyPlayerStyleFilter(List<UnknownMessage> messages)
        {
            if (!adaptToPlayerStyle)
                return messages;

            if (playerStyleProfile.stealthRatio > 0.6f)
            {
                return messages.OrderByDescending(m => m.trigger == MessageTrigger.StealthApproach).ToList();
            }
            else if (playerStyleProfile.aggressionRatio > 0.6f)
            {
                return messages.OrderByDescending(m => m.trigger == MessageTrigger.AggressiveApproach).ToList();
            }

            return messages;
        }

        private UnknownMessage WeightedRandomSelection(List<UnknownMessage> messages)
        {
            float totalWeight = messages.Sum(m => m.selectionWeight);
            float randomValue = UnityEngine.Random.Range(0f, totalWeight);
            float cumulative = 0f;

            foreach (UnknownMessage msg in messages)
            {
                cumulative += msg.selectionWeight;
                if (randomValue <= cumulative)
                {
                    return msg;
                }
            }

            return messages[messages.Count - 1];
        }

        private void DisplayMessage(UnknownMessage message)
        {
            lastMessageTime = Time.time;
            
            StartCoroutine(DisplayMessageAfterDelay(message));
        }

        private System.Collections.IEnumerator DisplayMessageAfterDelay(UnknownMessage message)
        {
            yield return new WaitForSeconds(message.displayDelay);
            
            OnMessageDisplay?.Invoke(message, message.displayMode);
        }

        private void RecordMessageHistory(UnknownMessage message)
        {
            MessageHistory history = new MessageHistory(message.text, message.trigger, Time.time);
            messageHistory.Add(history);

            if (messageHistory.Count > messageDatabase.maxHistorySize)
            {
                messageHistory.RemoveAt(0);
            }

            messageCooldowns[message.text] = Time.time + message.cooldown;
        }

        private bool HasMessageBeenShown(UnknownMessage message)
        {
            return messageHistory.Any(h => h.messageText == message.text);
        }

        private bool IsMessageOnCooldown(UnknownMessage message)
        {
            if (!messageCooldowns.ContainsKey(message.text))
                return false;

            return Time.time < messageCooldowns[message.text];
        }

        private void UpdatePlayerStyleProfile(UnknownMessageEvent messageEvent)
        {
            playerStyleProfile.totalActions++;

            switch (messageEvent.trigger)
            {
                case MessageTrigger.StealthApproach:
                    playerStyleProfile.stealthActions++;
                    break;
                case MessageTrigger.AggressiveApproach:
                    playerStyleProfile.aggressiveActions++;
                    break;
                case MessageTrigger.PuzzleSolved:
                    playerStyleProfile.puzzlesSolved++;
                    break;
                case MessageTrigger.SecretFound:
                    playerStyleProfile.secretsFound++;
                    break;
            }

            playerStyleProfile.UpdateRatios();
        }

        public List<MessageHistory> GetMessageHistory()
        {
            return new List<MessageHistory>(messageHistory);
        }

        public void ClearMessageHistory()
        {
            messageHistory.Clear();
            messageCooldowns.Clear();
            Debug.Log("Unknown message history cleared");
        }

        public void SetHintFrequency(float frequency)
        {
            HintFrequency = frequency;
            Debug.Log($"Unknown hint frequency set to: {frequency:P0}");
        }

        public PlayerStyleProfile GetPlayerStyleProfile()
        {
            return playerStyleProfile;
        }
    }

    /// <summary>
    /// Unknown entity personality modes.
    /// </summary>
    public enum UnknownPersonality
    {
        Verbose,
        Balanced,
        Cryptic
    }

    /// <summary>
    /// Tracks player behavior patterns for adaptive messaging.
    /// </summary>
    [Serializable]
    public class PlayerStyleProfile
    {
        public int totalActions = 0;
        public int stealthActions = 0;
        public int aggressiveActions = 0;
        public int puzzlesSolved = 0;
        public int secretsFound = 0;

        public float stealthRatio = 0f;
        public float aggressionRatio = 0f;
        public float explorationRatio = 0f;

        public void UpdateRatios()
        {
            if (totalActions == 0)
                return;

            stealthRatio = (float)stealthActions / totalActions;
            aggressionRatio = (float)aggressiveActions / totalActions;
            explorationRatio = (float)(secretsFound + puzzlesSolved) / totalActions;
        }
    }
}
