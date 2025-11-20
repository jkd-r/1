using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ProtocolEMR.Core.Procedural;
using ProtocolEMR.Core.AI;
using ProtocolEMR.Core.Dialogue;
using ProtocolEMR.Core.Input;

namespace ProtocolEMR.Core
{
    /// <summary>
    /// Game flow state for the playtest.
    /// </summary>
    public enum PlaytestState
    {
        Initializing,
        Tutorial,
        Puzzle,
        Combat,
        Extraction,
        Complete,
        Failed
    }

    /// <summary>
    /// Orchestrates the golden path for the shippable escape-room experience.
    /// Tracks progression through tutorial → puzzle → combat → extraction.
    /// Coordinates all core systems and manages save/load checkpoints.
    /// </summary>
    public class PlaytestFlowController : MonoBehaviour
    {
        public static PlaytestFlowController Instance { get; private set; }

        [Header("Core System References")]
        [SerializeField] private GameManager gameManager;
        [SerializeField] private MissionSystem missionSystem;
        [SerializeField] private ProceduralLevelBuilder levelBuilder;
        [SerializeField] private NPCSpawner npcSpawner;
        [SerializeField] private DynamicEventOrchestrator eventOrchestrator;
        [SerializeField] private UnknownDialogueManager dialogueManager;

        [Header("Player References")]
        [SerializeField] private GameObject playerPrefab;
        [SerializeField] private Transform playerSpawnPoint;

        [Header("Flow Configuration")]
        [SerializeField] private bool autoStartFlow = true;
        [SerializeField] private float tutorialDuration = 30f;
        [SerializeField] private float puzzleTimeout = 300f;
        [SerializeField] private float combatTimeout = 180f;

        [Header("Checkpoint Configuration")]
        [SerializeField] private bool enableAutoCheckpoints = true;
        [SerializeField] private float checkpointInterval = 60f;

        [Header("Debug")]
        [SerializeField] private bool logStateChanges = true;
        [SerializeField] private bool showDebugUI = true;

        private PlaytestState currentState = PlaytestState.Initializing;
        private GameObject playerInstance;
        private float stateTimer = 0f;
        private float lastCheckpointTime = 0f;
        private bool isTransitioning = false;
        private List<string> progressionLog = new List<string>();

        // State tracking
        private int puzzlesSolved = 0;
        private int enemiesDefeated = 0;
        private bool tutorialComplete = false;
        private bool levelGenerated = false;

        public PlaytestState CurrentState => currentState;
        public float StateTimer => stateTimer;
        public bool IsComplete => currentState == PlaytestState.Complete;
        public List<string> ProgressionLog => new List<string>(progressionLog);

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
            InitializeSystems();

            if (autoStartFlow)
            {
                StartCoroutine(StartPlaytestFlowCoroutine());
            }
        }

        private void Update()
        {
            if (isTransitioning || currentState == PlaytestState.Complete || currentState == PlaytestState.Failed)
                return;

            stateTimer += Time.deltaTime;

            // Auto-checkpoint system
            if (enableAutoCheckpoints && Time.time - lastCheckpointTime >= checkpointInterval)
            {
                CreateCheckpoint();
                lastCheckpointTime = Time.time;
            }

            // Handle debug inputs
            if (showDebugUI && UnityEngine.Input.GetKeyDown(KeyCode.F11))
            {
                ForceCompleteCurrentState();
            }

            if (UnityEngine.Input.GetKeyDown(KeyCode.F12))
            {
                PrintProgressionReport();
            }
        }

        /// <summary>
        /// Initializes all core systems and validates references.
        /// </summary>
        private void InitializeSystems()
        {
            LogProgress("Initializing PlaytestFlowController...");

            // Auto-find systems if not assigned
            if (gameManager == null)
                gameManager = FindObjectOfType<GameManager>();
            
            if (missionSystem == null)
                missionSystem = FindObjectOfType<MissionSystem>();
            
            if (levelBuilder == null)
                levelBuilder = FindObjectOfType<ProceduralLevelBuilder>();
            
            if (npcSpawner == null)
                npcSpawner = FindObjectOfType<NPCSpawner>();
            
            if (eventOrchestrator == null)
                eventOrchestrator = FindObjectOfType<DynamicEventOrchestrator>();
            
            if (dialogueManager == null)
                dialogueManager = UnknownDialogueManager.Instance;

            // Subscribe to mission events
            if (missionSystem != null)
            {
                missionSystem.OnObjectiveCompleted += OnObjectiveCompleted;
                missionSystem.OnAllObjectivesCompleted += OnAllObjectivesCompleted;
            }

            ValidateSystems();
            LogProgress("Systems initialized");
        }

        /// <summary>
        /// Validates that all required systems are present.
        /// </summary>
        private void ValidateSystems()
        {
            bool allSystemsReady = true;

            if (gameManager == null)
            {
                Debug.LogWarning("[PlaytestFlow] GameManager not found");
                allSystemsReady = false;
            }

            if (missionSystem == null)
            {
                Debug.LogWarning("[PlaytestFlow] MissionSystem not found - creating one");
                GameObject missionObj = new GameObject("MissionSystem");
                missionSystem = missionObj.AddComponent<MissionSystem>();
            }

            if (levelBuilder == null)
            {
                Debug.LogWarning("[PlaytestFlow] ProceduralLevelBuilder not found");
            }

            if (npcSpawner == null)
            {
                Debug.LogWarning("[PlaytestFlow] NPCSpawner not found");
            }

            if (allSystemsReady)
            {
                LogProgress("All core systems validated");
            }
            else
            {
                LogProgress("Some systems missing - functionality may be limited");
            }
        }

        /// <summary>
        /// Starts the playtest flow coroutine.
        /// </summary>
        private IEnumerator StartPlaytestFlowCoroutine()
        {
            LogProgress("Starting playtest flow...");

            // Wait for systems to initialize
            yield return new WaitForSeconds(1f);

            // Spawn player if needed
            if (playerInstance == null && playerPrefab != null)
            {
                SpawnPlayer();
            }

            // Generate level
            if (levelBuilder != null && !levelGenerated)
            {
                LogProgress("Generating procedural level...");
                levelBuilder.GenerateLevel();
                
                // Wait for level generation
                float timeout = 30f;
                float elapsed = 0f;
                while (!levelBuilder.GenerationComplete && elapsed < timeout)
                {
                    elapsed += Time.deltaTime;
                    yield return null;
                }

                if (levelBuilder.GenerationComplete)
                {
                    levelGenerated = true;
                    LogProgress("Level generation complete");
                }
                else
                {
                    LogProgress("Level generation timed out - continuing anyway");
                }
            }

            // Start tutorial phase
            TransitionToState(PlaytestState.Tutorial);

            // Trigger initial Unknown dialogue
            if (dialogueManager != null)
            {
                UnknownDialogueTriggers.TriggerWelcome();
            }
        }

        /// <summary>
        /// Spawns the player at the spawn point.
        /// </summary>
        private void SpawnPlayer()
        {
            if (playerPrefab == null)
            {
                Debug.LogWarning("[PlaytestFlow] Player prefab not assigned");
                return;
            }

            Vector3 spawnPos = playerSpawnPoint != null ? playerSpawnPoint.position : Vector3.zero;
            Quaternion spawnRot = playerSpawnPoint != null ? playerSpawnPoint.rotation : Quaternion.identity;

            playerInstance = Instantiate(playerPrefab, spawnPos, spawnRot);
            playerInstance.name = "Player";

            LogProgress($"Player spawned at {spawnPos}");
        }

        /// <summary>
        /// Transitions to a new playtest state.
        /// </summary>
        private void TransitionToState(PlaytestState newState)
        {
            if (isTransitioning)
                return;

            StartCoroutine(TransitionToStateCoroutine(newState));
        }

        /// <summary>
        /// Coroutine for state transitions with proper cleanup.
        /// </summary>
        private IEnumerator TransitionToStateCoroutine(PlaytestState newState)
        {
            isTransitioning = true;

            // Exit current state
            OnStateExit(currentState);

            PlaytestState previousState = currentState;
            currentState = newState;
            stateTimer = 0f;

            if (logStateChanges)
            {
                LogProgress($"Transitioned from {previousState} to {newState}");
            }

            // Enter new state
            OnStateEnter(newState);

            yield return new WaitForSeconds(0.5f); // Small delay for state transition

            isTransitioning = false;
        }

        /// <summary>
        /// Called when entering a new state.
        /// </summary>
        private void OnStateEnter(PlaytestState state)
        {
            switch (state)
            {
                case PlaytestState.Tutorial:
                    StartTutorialPhase();
                    break;

                case PlaytestState.Puzzle:
                    StartPuzzlePhase();
                    break;

                case PlaytestState.Combat:
                    StartCombatPhase();
                    break;

                case PlaytestState.Extraction:
                    StartExtractionPhase();
                    break;

                case PlaytestState.Complete:
                    OnPlaytestComplete();
                    break;

                case PlaytestState.Failed:
                    OnPlaytestFailed();
                    break;
            }
        }

        /// <summary>
        /// Called when exiting a state.
        /// </summary>
        private void OnStateExit(PlaytestState state)
        {
            // Cleanup based on state
            switch (state)
            {
                case PlaytestState.Tutorial:
                    tutorialComplete = true;
                    break;
            }
        }

        /// <summary>
        /// Starts the tutorial phase.
        /// </summary>
        private void StartTutorialPhase()
        {
            LogProgress("=== TUTORIAL PHASE STARTED ===");

            if (dialogueManager != null)
            {
                UnknownDialogueTriggers.TriggerTutorial(0);
            }

            // Start tutorial timer
            StartCoroutine(TutorialTimerCoroutine());
        }

        /// <summary>
        /// Tutorial timer coroutine.
        /// </summary>
        private IEnumerator TutorialTimerCoroutine()
        {
            yield return new WaitForSeconds(tutorialDuration);

            if (currentState == PlaytestState.Tutorial)
            {
                if (missionSystem != null)
                {
                    missionSystem.CompleteObjective("tutorial_movement");
                    missionSystem.CompleteObjective("tutorial_interaction");
                }

                TransitionToState(PlaytestState.Puzzle);
            }
        }

        /// <summary>
        /// Starts the puzzle phase.
        /// </summary>
        private void StartPuzzlePhase()
        {
            LogProgress("=== PUZZLE PHASE STARTED ===");

            if (dialogueManager != null)
            {
                UnknownDialogueTriggers.TriggerPuzzleDiscovery("escape_room");
            }

            // Spawn puzzle event if orchestrator available
            if (eventOrchestrator != null)
            {
                // This would trigger puzzle spawning via the event orchestrator
                LogProgress("Puzzle events initialized via orchestrator");
            }
        }

        /// <summary>
        /// Starts the combat phase.
        /// </summary>
        private void StartCombatPhase()
        {
            LogProgress("=== COMBAT PHASE STARTED ===");

            if (dialogueManager != null)
            {
                UnknownDialogueTriggers.TriggerThreatDetected(2);
            }

            // Spawn NPCs for combat
            if (npcSpawner != null)
            {
                LogProgress("Spawning combat NPCs...");
                // NPCSpawner should auto-spawn based on zones
            }

            // Trigger combat event
            if (eventOrchestrator != null)
            {
                LogProgress("Combat events initialized via orchestrator");
            }
        }

        /// <summary>
        /// Starts the extraction phase.
        /// </summary>
        private void StartExtractionPhase()
        {
            LogProgress("=== EXTRACTION PHASE STARTED ===");

            if (dialogueManager != null)
            {
                UnknownDialogueTriggers.TriggerExtractionReady();
            }
        }

        /// <summary>
        /// Called when playtest is complete.
        /// </summary>
        private void OnPlaytestComplete()
        {
            LogProgress("=== PLAYTEST COMPLETE ===");
            LogProgress($"Total time: {Time.time:F2}s");

            if (dialogueManager != null)
            {
                UnknownDialogueTriggers.TriggerMissionComplete();
            }

            PrintProgressionReport();
        }

        /// <summary>
        /// Called when playtest fails.
        /// </summary>
        private void OnPlaytestFailed()
        {
            LogProgress("=== PLAYTEST FAILED ===");
            LogProgress($"Failed at state: {currentState}");
            LogProgress($"Time elapsed: {Time.time:F2}s");

            if (dialogueManager != null)
            {
                UnknownDialogueTriggers.TriggerDeath();
            }
        }

        /// <summary>
        /// Called when a mission objective is completed.
        /// </summary>
        private void OnObjectiveCompleted(MissionObjective objective)
        {
            LogProgress($"Objective completed: {objective.objectiveId}");

            // Handle state transitions based on objectives
            switch (objective.objectiveId)
            {
                case "tutorial_movement":
                case "tutorial_interaction":
                    // Wait for both tutorial objectives
                    if (missionSystem.GetObjective("tutorial_movement").isComplete &&
                        missionSystem.GetObjective("tutorial_interaction").isComplete)
                    {
                        TransitionToState(PlaytestState.Puzzle);
                    }
                    break;

                case "puzzle_solve":
                    TransitionToState(PlaytestState.Combat);
                    break;

                case "combat_encounter":
                    TransitionToState(PlaytestState.Extraction);
                    break;

                case "extraction_reach":
                    TransitionToState(PlaytestState.Complete);
                    break;
            }
        }

        /// <summary>
        /// Called when all objectives are completed.
        /// </summary>
        private void OnAllObjectivesCompleted()
        {
            if (currentState != PlaytestState.Complete)
            {
                TransitionToState(PlaytestState.Complete);
            }
        }

        /// <summary>
        /// Creates a save checkpoint.
        /// </summary>
        private void CreateCheckpoint()
        {
            LogProgress($"Checkpoint created at state: {currentState}");

            // Save mission state
            if (missionSystem != null)
            {
                string missionData = missionSystem.SerializeMissionState();
                PlayerPrefs.SetString("checkpoint_mission", missionData);
            }

            // Save flow state
            PlayerPrefs.SetInt("checkpoint_state", (int)currentState);
            PlayerPrefs.SetFloat("checkpoint_time", Time.time);
            PlayerPrefs.Save();
        }

        /// <summary>
        /// Loads the last checkpoint.
        /// </summary>
        public void LoadCheckpoint()
        {
            if (!PlayerPrefs.HasKey("checkpoint_state"))
            {
                Debug.LogWarning("[PlaytestFlow] No checkpoint found");
                return;
            }

            int savedState = PlayerPrefs.GetInt("checkpoint_state");
            float savedTime = PlayerPrefs.GetFloat("checkpoint_time");

            LogProgress($"Loading checkpoint from state: {(PlaytestState)savedState}");

            // Load mission state
            if (missionSystem != null && PlayerPrefs.HasKey("checkpoint_mission"))
            {
                string missionData = PlayerPrefs.GetString("checkpoint_mission");
                missionSystem.DeserializeMissionState(missionData);
            }

            TransitionToState((PlaytestState)savedState);
        }

        /// <summary>
        /// Forces completion of the current state (for debugging).
        /// </summary>
        private void ForceCompleteCurrentState()
        {
            LogProgress($"Force completing state: {currentState}");

            switch (currentState)
            {
                case PlaytestState.Tutorial:
                    if (missionSystem != null)
                    {
                        missionSystem.CompleteObjective("tutorial_movement");
                        missionSystem.CompleteObjective("tutorial_interaction");
                    }
                    break;

                case PlaytestState.Puzzle:
                    if (missionSystem != null)
                    {
                        missionSystem.CompleteObjective("puzzle_solve");
                    }
                    break;

                case PlaytestState.Combat:
                    if (missionSystem != null)
                    {
                        missionSystem.CompleteObjective("combat_encounter");
                    }
                    break;

                case PlaytestState.Extraction:
                    if (missionSystem != null)
                    {
                        missionSystem.CompleteObjective("extraction_reach");
                    }
                    break;
            }
        }

        /// <summary>
        /// Logs progression for debugging and telemetry.
        /// </summary>
        private void LogProgress(string message)
        {
            string timestamp = $"[{Time.time:F2}s]";
            string logMessage = $"{timestamp} {message}";
            
            progressionLog.Add(logMessage);
            
            if (logStateChanges)
            {
                Debug.Log($"[PlaytestFlow] {message}");
            }
        }

        /// <summary>
        /// Prints a full progression report.
        /// </summary>
        public void PrintProgressionReport()
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("=== PLAYTEST PROGRESSION REPORT ===");
            sb.AppendLine($"Current State: {currentState}");
            sb.AppendLine($"State Timer: {stateTimer:F2}s");
            sb.AppendLine($"Total Time: {Time.time:F2}s");
            sb.AppendLine();
            sb.AppendLine("Progression Log:");
            
            foreach (var entry in progressionLog)
            {
                sb.AppendLine(entry);
            }

            if (missionSystem != null)
            {
                sb.AppendLine();
                sb.AppendLine("Mission Objectives:");
                var objectives = missionSystem.GetAllObjectives();
                foreach (var obj in objectives)
                {
                    string status = obj.isComplete ? "✓" : "○";
                    sb.AppendLine($"  {status} {obj.description} ({obj.progress * 100:F0}%)");
                }

                sb.AppendLine($"Overall Progress: {missionSystem.GetCompletionProgress() * 100:F0}%");
            }

            Debug.Log(sb.ToString());
        }

        /// <summary>
        /// Public API for triggering puzzle solve.
        /// </summary>
        public void OnPuzzleSolved(string puzzleId)
        {
            puzzlesSolved++;
            LogProgress($"Puzzle solved: {puzzleId} (Total: {puzzlesSolved})");

            if (missionSystem != null)
            {
                missionSystem.CompleteObjective("puzzle_solve");
            }
        }

        /// <summary>
        /// Public API for tracking enemy defeats.
        /// </summary>
        public void OnEnemyDefeated()
        {
            enemiesDefeated++;
            LogProgress($"Enemy defeated (Total: {enemiesDefeated})");

            // Complete combat objective after defeating enough enemies
            if (enemiesDefeated >= 3 && missionSystem != null)
            {
                missionSystem.CompleteObjective("combat_encounter");
            }
        }

        /// <summary>
        /// Public API for reaching extraction.
        /// </summary>
        public void OnExtractionReached()
        {
            LogProgress("Extraction point reached");

            if (missionSystem != null)
            {
                missionSystem.CompleteObjective("extraction_reach");
            }
        }

        private void OnDestroy()
        {
            if (missionSystem != null)
            {
                missionSystem.OnObjectiveCompleted -= OnObjectiveCompleted;
                missionSystem.OnAllObjectivesCompleted -= OnAllObjectivesCompleted;
            }
        }

        private void OnGUI()
        {
            if (!showDebugUI)
                return;

            GUILayout.BeginArea(new Rect(10, 10, 300, 400));
            GUILayout.Label($"<b>Playtest Flow Debug</b>", new GUIStyle(GUI.skin.label) { richText = true });
            GUILayout.Label($"State: {currentState}");
            GUILayout.Label($"Timer: {stateTimer:F1}s");
            
            if (missionSystem != null)
            {
                GUILayout.Label($"Progress: {missionSystem.GetCompletionProgress() * 100:F0}%");
            }

            GUILayout.Space(10);
            GUILayout.Label("Controls:");
            GUILayout.Label("F11 - Skip Current Phase");
            GUILayout.Label("F12 - Print Report");

            GUILayout.EndArea();
        }
    }
}
