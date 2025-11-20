using UnityEngine;
using System;

namespace ProtocolEMR.Core.AI
{
    /// <summary>
    /// Difficulty levels for NPC AI scaling.
    /// </summary>
    public enum DifficultyLevel
    {
        Easy,
        Normal,
        Hard,
        Nightmare
    }

    /// <summary>
    /// Difficulty scaling configuration for NPC parameters.
    /// </summary>
    [System.Serializable]
    public class DifficultySettings
    {
        [Header("Health Scaling")]
        [Range(0.1f, 5f)] public float healthMultiplier = 1.0f;
        [Range(0.1f, 5f)] public float damageMultiplier = 1.0f;
        
        [Header("Perception Scaling")]
        [Range(0.1f, 2f)] public float perceptionRangeMultiplier = 1.0f;
        [Range(0.1f, 2f)] public float hearingRangeMultiplier = 1.0f;
        [Range(0.1f, 2f)] public float fieldOfViewMultiplier = 1.0f;
        
        [Header("Combat Scaling")]
        [Range(0.1f, 2f)] public float attackSpeedMultiplier = 1.0f;
        [Range(0f, 1f)] public float dodgeChanceMultiplier = 1.0f;
        [Range(0.1f, 2f)] public float accuracyMultiplier = 1.0f;
        
        [Header("Behavior Scaling")]
        [Range(0.1f, 2f)] public float reactionTimeMultiplier = 1.0f;
        [Range(0f, 200f)] public float aggressionBonus = 0f;
        [Range(0f, 200f)] public float intelligenceBonus = 0f;
        [Range(0f, 200f)] public float moraleBonus = 0f;
        
        [Header("Speed Scaling")]
        [Range(0.5f, 2f)] public float movementSpeedMultiplier = 1.0f;
        [Range(0.5f, 2f)] public float sprintSpeedMultiplier = 1.0f;
        
        [Header("Group Behavior")]
        [Range(0f, 1f)] public float groupCoordinationLevel = 0.5f;
        [Range(1f, 10f)] public float maxGroupSize = 3f;
        [Range(0f, 1f)] public float alertPropagationSpeed = 0.5f;
        
        [Header("Special Features")]
        public bool enableAdvancedTactics = false;
        public bool enableFlanking = false;
        public bool enableCoverUsage = false;
        public bool enableRetreatCoordination = false;
        public bool enableAdaptiveBehavior = false;
    }

    /// <summary>
    /// Global difficulty manager for NPC AI scaling.
    /// </summary>
    public class DifficultyManager : MonoBehaviour
    {
        [Header("Current Difficulty")]
        [SerializeField] private DifficultyLevel currentDifficulty = DifficultyLevel.Normal;
        [SerializeField] private bool useDynamicDifficulty = false;
        [SerializeField] private float playerPerformanceScore = 0f;
        
        [Header("Difficulty Settings")]
        [SerializeField] private DifficultySettings easySettings = new DifficultySettings();
        [SerializeField] private DifficultySettings normalSettings = new DifficultySettings();
        [SerializeField] private DifficultySettings hardSettings = new DifficultySettings();
        [SerializeField] private DifficultySettings nightmareSettings = new DifficultySettings();
        
        [Header("Dynamic Difficulty")]
        [SerializeField] private float performanceUpdateInterval = 30f;
        [SerializeField] private float difficultyChangeThreshold = 20f;
        [SerializeField] private AnimationCurve difficultyScalingCurve = AnimationCurve.Linear(0f, 0.5f, 100f, 2.0f);
        
        [Header("Debug")]
        [SerializeField] private bool showDifficultyInfo = true;

        // Singleton pattern
        public static DifficultyManager Instance { get; private set; }

        // Events
        public System.Action<DifficultyLevel> OnDifficultyChanged;

        // Runtime data
        private float lastPerformanceUpdate;
        private int playerDeaths = 0;
        private int npcKills = 0;
        private float sessionTime = 0f;
        private float lastDifficultyChange;

        // Properties
        public DifficultyLevel CurrentDifficulty => currentDifficulty;
        public DifficultySettings CurrentSettings => GetDifficultySettings(currentDifficulty);
        public float PlayerPerformanceScore => playerPerformanceScore;

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
            InitializeDifficultySettings();
            ApplyDifficultySettings();
        }

        private void Update()
        {
            UpdateSessionTime();
            
            if (useDynamicDifficulty)
            {
                UpdateDynamicDifficulty();
            }
        }

        /// <summary>
        /// Initializes difficulty settings with default values.
        /// </summary>
        private void InitializeDifficultySettings()
        {
            // Easy difficulty
            easySettings.healthMultiplier = 0.6f;
            easySettings.damageMultiplier = 0.5f;
            easySettings.perceptionRangeMultiplier = 0.7f;
            easySettings.reactionTimeMultiplier = 1.5f;
            easySettings.dodgeChanceMultiplier = 0.5f;
            easySettings.groupCoordinationLevel = 0.2f;
            easySettings.maxGroupSize = 2f;
            
            // Normal difficulty (baseline)
            normalSettings.healthMultiplier = 1.0f;
            normalSettings.damageMultiplier = 1.0f;
            normalSettings.perceptionRangeMultiplier = 1.0f;
            normalSettings.reactionTimeMultiplier = 1.0f;
            normalSettings.dodgeChanceMultiplier = 1.0f;
            normalSettings.groupCoordinationLevel = 0.5f;
            normalSettings.maxGroupSize = 3f;
            
            // Hard difficulty
            hardSettings.healthMultiplier = 1.5f;
            hardSettings.damageMultiplier = 1.25f;
            hardSettings.perceptionRangeMultiplier = 1.2f;
            hardSettings.reactionTimeMultiplier = 0.8f;
            hardSettings.dodgeChanceMultiplier = 1.3f;
            hardSettings.groupCoordinationLevel = 0.7f;
            hardSettings.maxGroupSize = 4f;
            hardSettings.enableAdvancedTactics = true;
            hardSettings.enableFlanking = true;
            
            // Nightmare difficulty
            nightmareSettings.healthMultiplier = 2.0f;
            nightmareSettings.damageMultiplier = 1.5f;
            nightmareSettings.perceptionRangeMultiplier = 1.3f;
            nightmareSettings.reactionTimeMultiplier = 0.6f;
            nightmareSettings.dodgeChanceMultiplier = 1.5f;
            nightmareSettings.groupCoordinationLevel = 1.0f;
            nightmareSettings.maxGroupSize = 6f;
            nightmareSettings.enableAdvancedTactics = true;
            nightmareSettings.enableFlanking = true;
            nightmareSettings.enableCoverUsage = true;
            nightmareSettings.enableRetreatCoordination = true;
            nightmareSettings.enableAdaptiveBehavior = true;
        }

        /// <summary>
        /// Applies current difficulty settings to game systems.
        /// </summary>
        private void ApplyDifficultySettings()
        {
            DifficultySettings settings = CurrentSettings;
            
            // Apply to NPC manager if available
            if (NPCManager.Instance != null)
            {
                ApplyDifficultyToNPCManager(settings);
            }
            
            Debug.Log($"Applied {currentDifficulty} difficulty settings");
        }

        /// <summary>
        /// Applies difficulty settings to NPC manager.
        /// </summary>
        private void ApplyDifficultyToNPCManager(DifficultySettings settings)
        {
            // Update global alert range and propagation
            // This would require extending NPCManager to accept difficulty settings
        }

        /// <summary>
        /// Updates session time tracking.
        /// </summary>
        private void UpdateSessionTime()
        {
            sessionTime += Time.deltaTime;
        }

        /// <summary>
        /// Updates dynamic difficulty based on player performance.
        /// </summary>
        private void UpdateDynamicDifficulty()
        {
            if (Time.time - lastPerformanceUpdate < performanceUpdateInterval)
                return;

            lastPerformanceUpdate = Time.time;
            CalculatePlayerPerformance();
            
            // Check if difficulty should change
            DifficultyLevel suggestedDifficulty = GetSuggestedDifficulty();
            
            if (suggestedDifficulty != currentDifficulty && Time.time - lastDifficultyChange > 60f)
            {
                SetDifficulty(suggestedDifficulty);
                lastDifficultyChange = Time.time;
            }
        }

        /// <summary>
        /// Calculates player performance score.
        /// </summary>
        private void CalculatePlayerPerformance()
        {
            float killRatio = npcKills / Mathf.Max(1f, playerDeaths);
            float timeScore = Mathf.Clamp01(sessionTime / 600f); // 10 minutes = 1.0
            float baseScore = killRatio * 50f + timeScore * 30f + (100f - playerDeaths * 10f);
            
            playerPerformanceScore = Mathf.Clamp(baseScore, 0f, 100f);
        }

        /// <summary>
        /// Gets suggested difficulty based on performance.
        /// </summary>
        private DifficultyLevel GetSuggestedDifficulty()
        {
            if (playerPerformanceScore < 25f)
                return DifficultyLevel.Easy;
            else if (playerPerformanceScore < 50f)
                return DifficultyLevel.Normal;
            else if (playerPerformanceScore < 75f)
                return DifficultyLevel.Hard;
            else
                return DifficultyLevel.Nightmare;
        }

        /// <summary>
        /// Sets difficulty level.
        /// </summary>
        public void SetDifficulty(DifficultyLevel difficulty)
        {
            if (currentDifficulty == difficulty)
                return;

            currentDifficulty = difficulty;
            ApplyDifficultySettings();
            OnDifficultyChanged?.Invoke(difficulty);
            
            Debug.Log($"Difficulty changed to {difficulty}");
        }

        /// <summary>
        /// Gets difficulty settings for specific level.
        /// </summary>
        public DifficultySettings GetDifficultySettings(DifficultyLevel difficulty)
        {
            switch (difficulty)
            {
                case DifficultyLevel.Easy:
                    return easySettings;
                case DifficultyLevel.Normal:
                    return normalSettings;
                case DifficultyLevel.Hard:
                    return hardSettings;
                case DifficultyLevel.Nightmare:
                    return nightmareSettings;
                default:
                    return normalSettings;
            }
        }

        /// <summary>
        /// Applies difficulty scaling to NPC parameters.
        /// </summary>
        public NPCParameters ApplyDifficultyToNPC(NPCParameters baseParameters, NPCType npcType)
        {
            DifficultySettings settings = CurrentSettings;
            NPCParameters scaledParams = baseParameters; // Copy base parameters

            // Apply health scaling
            scaledParams.maxHealth *= settings.healthMultiplier;
            
            // Apply damage scaling
            scaledParams.damagePerHit *= settings.damageMultiplier;
            
            // Apply perception scaling
            scaledParams.perceptionRange *= settings.perceptionRangeMultiplier;
            scaledParams.hearingRange *= settings.hearingRangeMultiplier;
            scaledParams.fieldOfView *= settings.fieldOfViewMultiplier;
            
            // Apply combat scaling
            scaledParams.attackFrequency *= settings.attackSpeedMultiplier;
            
            // Apply behavior scaling
            scaledParams.reactionTime *= settings.reactionTimeMultiplier;
            scaledParams.aggression = Mathf.Clamp(scaledParams.aggression + settings.aggressionBonus, 0f, 100f);
            scaledParams.intelligence = Mathf.Clamp(scaledParams.intelligence + settings.intelligenceBonus, 0f, 100f);
            scaledParams.morale = Mathf.Clamp(scaledParams.morale + settings.moraleBonus, 0f, 100f);
            
            // Apply speed scaling
            scaledParams.walkSpeed *= settings.movementSpeedMultiplier;
            scaledParams.runSpeed *= settings.movementSpeedMultiplier;
            scaledParams.sprintSpeed *= settings.movementSpeedMultiplier;

            return scaledParams;
        }

        /// <summary>
        /// Gets difficulty multiplier for specific stat.
        /// </summary>
        public float GetDifficultyMultiplier(string statName)
        {
            DifficultySettings settings = CurrentSettings;
            
            switch (statName.ToLower())
            {
                case "health":
                    return settings.healthMultiplier;
                case "damage":
                    return settings.damageMultiplier;
                case "perception":
                    return settings.perceptionRangeMultiplier;
                case "speed":
                    return settings.movementSpeedMultiplier;
                case "reaction":
                    return settings.reactionTimeMultiplier;
                case "dodge":
                    return settings.dodgeChanceMultiplier;
                default:
                    return 1.0f;
            }
        }

        /// <summary>
        /// Records player death for performance tracking.
        /// </summary>
        public void RecordPlayerDeath()
        {
            playerDeaths++;
            playerPerformanceScore = Mathf.Max(0f, playerPerformanceScore - 10f);
        }

        /// <summary>
        /// Records NPC kill for performance tracking.
        /// </summary>
        public void RecordNPCKill()
        {
            npcKills++;
            playerPerformanceScore = Mathf.Min(100f, playerPerformanceScore + 5f);
        }

        /// <summary>
        /// Gets difficulty description.
        /// </summary>
        public string GetDifficultyDescription(DifficultyLevel difficulty)
        {
            switch (difficulty)
            {
                case DifficultyLevel.Easy:
                    return "Reduced enemy health and damage, slower reactions";
                case DifficultyLevel.Normal:
                    return "Balanced gameplay experience";
                case DifficultyLevel.Hard:
                    return "Tougher enemies with improved tactics";
                case DifficultyLevel.Nightmare:
                    return "Extreme challenge with coordinated enemy groups";
                default:
                    return "Unknown difficulty";
            }
        }

        /// <summary>
        /// Gets difficulty color for UI.
        /// </summary>
        public Color GetDifficultyColor(DifficultyLevel difficulty)
        {
            switch (difficulty)
            {
                case DifficultyLevel.Easy:
                    return Color.green;
                case DifficultyLevel.Normal:
                    return Color.white;
                case DifficultyLevel.Hard:
                    return Color.yellow;
                case DifficultyLevel.Nightmare:
                    return Color.red;
                default:
                    return Color.gray;
            }
        }

        /// <summary>
        /// Resets performance tracking.
        /// </summary>
        public void ResetPerformanceTracking()
        {
            playerDeaths = 0;
            npcKills = 0;
            sessionTime = 0f;
            playerPerformanceScore = 50f;
            lastPerformanceUpdate = Time.time;
        }

        /// <summary>
        /// Gets performance grade.
        /// </summary>
        public string GetPerformanceGrade()
        {
            if (playerPerformanceScore >= 90f) return "S";
            if (playerPerformanceScore >= 80f) return "A";
            if (playerPerformanceScore >= 70f) return "B";
            if (playerPerformanceScore >= 60f) return "C";
            if (playerPerformanceScore >= 50f) return "D";
            return "F";
        }

        /// <summary>
        /// Toggles dynamic difficulty.
        /// </summary>
        public void ToggleDynamicDifficulty()
        {
            useDynamicDifficulty = !useDynamicDifficulty;
            
            if (useDynamicDifficulty)
            {
                ResetPerformanceTracking();
            }
        }

        private void OnGUI()
        {
            if (!showDifficultyInfo) return;

            GUILayout.BeginArea(new Rect(10, 10, 300, 200));
            GUILayout.BeginVertical("box");
            
            GUILayout.Label($"Difficulty: {currentDifficulty}");
            GUILayout.Label($"Performance Score: {playerPerformanceScore:F1} ({GetPerformanceGrade()})");
            GUILayout.Label($"Kills: {npcKills} | Deaths: {playerDeaths}");
            GUILayout.Label($"Session Time: {sessionTime / 60f:F1} min");
            
            if (useDynamicDifficulty)
            {
                GUILayout.Label("Dynamic Difficulty: ON");
            }
            
            GUILayout.EndVertical();
            GUILayout.EndArea();
        }
    }
}