using UnityEngine;

namespace ProtocolEMR.Core.SaveSystem
{
    /// <summary>
    /// Manages event-based save triggers such as mission checkpoints, puzzle completion, and combat encounters.
    /// Automatically creates saves at important game moments to prevent progress loss.
    /// </summary>
    public class SaveTriggerSystem : MonoBehaviour
    {
        public static SaveTriggerSystem Instance { get; private set; }

        [Header("Auto-Save Settings")]
        [SerializeField] private bool saveOnMissionStart = true;
        [SerializeField] private bool saveOnMissionComplete = true;
        [SerializeField] private bool saveOnPuzzleSolved = true;
        [SerializeField] private bool saveOnCombatStart = true;
        [SerializeField] private bool saveOnDialogueComplete = true;
        [SerializeField] private float minTimeBetweenSaves = 30f; // Minimum 30 seconds between saves

        private float lastSaveTime;

        public event System.Action<string> OnAutoSaveTriggered;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            lastSaveTime = -minTimeBetweenSaves;
        }

        /// <summary>
        /// Triggers an auto-save if conditions are met.
        /// </summary>
        private void TriggerAutoSave(string reason)
        {
            if (Time.realtimeSinceStartup - lastSaveTime < minTimeBetweenSaves)
            {
                Debug.Log($"Auto-save skipped (too soon): {reason}");
                return;
            }

            if (SaveGameManager.Instance != null && !SaveGameManager.Instance.IsSaving)
            {
                Debug.Log($"Auto-save triggered: {reason}");
                SaveGameManager.Instance.AutoSave();
                lastSaveTime = Time.realtimeSinceStartup;
                OnAutoSaveTriggered?.Invoke(reason);
            }
        }

        /// <summary>
        /// Called when a mission starts.
        /// </summary>
        public void OnMissionStarted(string missionId)
        {
            if (saveOnMissionStart)
            {
                TriggerAutoSave($"Mission Started: {missionId}");
            }
        }

        /// <summary>
        /// Called when a mission is completed.
        /// </summary>
        public void OnMissionCompleted(string missionId)
        {
            if (saveOnMissionComplete)
            {
                TriggerAutoSave($"Mission Completed: {missionId}");
            }

            if (StatisticsTracker.Instance != null)
            {
                StatisticsTracker.Instance.IncrementMissionsCompleted();
            }
        }

        /// <summary>
        /// Called when a puzzle is solved.
        /// </summary>
        public void OnPuzzleSolved(string puzzleId)
        {
            if (saveOnPuzzleSolved)
            {
                TriggerAutoSave($"Puzzle Solved: {puzzleId}");
            }

            if (StatisticsTracker.Instance != null)
            {
                StatisticsTracker.Instance.IncrementPuzzlesSolved();
            }
        }

        /// <summary>
        /// Called when combat encounter starts.
        /// </summary>
        public void OnCombatStarted()
        {
            if (saveOnCombatStart)
            {
                TriggerAutoSave("Combat Started");
            }

            if (StatisticsTracker.Instance != null)
            {
                StatisticsTracker.Instance.IncrementCombatEncounters();
            }
        }

        /// <summary>
        /// Called when dialogue sequence completes.
        /// </summary>
        public void OnDialogueCompleted(string npcId)
        {
            if (saveOnDialogueComplete)
            {
                TriggerAutoSave($"Dialogue Completed: {npcId}");
            }
        }

        /// <summary>
        /// Called when player encounters an NPC.
        /// </summary>
        public void OnNPCEncountered(string npcId)
        {
            if (StatisticsTracker.Instance != null)
            {
                StatisticsTracker.Instance.IncrementNPCsEncountered();
            }
        }

        /// <summary>
        /// Called when player finds a collectible.
        /// </summary>
        public void OnCollectibleFound(string collectibleId)
        {
            if (StatisticsTracker.Instance != null)
            {
                StatisticsTracker.Instance.IncrementCollectiblesFound();
            }
        }

        /// <summary>
        /// Called when player discovers a secret area.
        /// </summary>
        public void OnSecretDiscovered(string secretId)
        {
            TriggerAutoSave($"Secret Discovered: {secretId}");

            if (StatisticsTracker.Instance != null)
            {
                StatisticsTracker.Instance.IncrementSecretsFound();
            }
        }

        /// <summary>
        /// Called when player triggers an alert.
        /// </summary>
        public void OnAlertTriggered()
        {
            if (StatisticsTracker.Instance != null)
            {
                StatisticsTracker.Instance.IncrementAlertsTriggered();
            }
        }

        /// <summary>
        /// Called when player dies.
        /// </summary>
        public void OnPlayerDeath()
        {
            if (StatisticsTracker.Instance != null)
            {
                StatisticsTracker.Instance.IncrementDeaths();
            }
        }

        /// <summary>
        /// Creates a checkpoint save with a custom name.
        /// </summary>
        public void CreateCheckpoint(string checkpointName)
        {
            if (SaveGameManager.Instance != null)
            {
                SaveGameManager.Instance.CreateCheckpoint(checkpointName);
                Debug.Log($"Checkpoint created: {checkpointName}");
            }
        }

        /// <summary>
        /// Forces an immediate auto-save regardless of timing restrictions.
        /// </summary>
        public void ForceAutoSave(string reason)
        {
            if (SaveGameManager.Instance != null && !SaveGameManager.Instance.IsSaving)
            {
                Debug.Log($"Forced auto-save: {reason}");
                SaveGameManager.Instance.AutoSave();
                lastSaveTime = Time.realtimeSinceStartup;
                OnAutoSaveTriggered?.Invoke(reason);
            }
        }
    }
}
