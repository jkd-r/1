using UnityEngine;
using System.Collections;
using ProtocolEMR.Core.Input;
using ProtocolEMR.Core.Settings;
using ProtocolEMR.Core.Performance;
using ProtocolEMR.Core.Dialogue;
using ProtocolEMR.Core.Procedural;

namespace ProtocolEMR.Core
{
    /// <summary>
    /// Central game manager for Protocol EMR.
    /// Initializes and manages core systems including input, settings, and performance monitoring.
    /// Handles game state transitions and system lifecycle management.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Core Systems")]
        [SerializeField] private InputManager inputManagerPrefab;
        [SerializeField] private SettingsManager settingsManagerPrefab;
        [SerializeField] private PerformanceMonitor performanceMonitorPrefab;
        [SerializeField] private UnknownDialogueManager unknownDialogueManagerPrefab;
        [SerializeField] private SeedManager seedManagerPrefab;
        [SerializeField] private GameObject proceduralStateStorePrefab;
        [SerializeField] private CrashLogger crashLoggerPrefab;

        private bool isPaused = false;
        private float timeScaleBeforePause = 1.0f;

        public bool IsPaused => isPaused;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            InitializeCoreSystemsIfNeeded();
        }

        private void Start()
        {
            Application.targetFrameRate = 60;
            QualitySettings.vSyncCount = 0;

            if (InputManager.Instance != null)
            {
                InputManager.Instance.OnPause += TogglePause;
            }

            // Initialize seed system after settings are loaded
            InitializeSeedSystem();

            // Start performance validation for builds
            #if !UNITY_EDITOR
            StartCoroutine(PerformanceValidationCoroutine());
            #endif

            Debug.Log("Protocol EMR - Game Manager Initialized");
            Debug.Log($"Unity Version: {Application.unityVersion}");
            Debug.Log($"Platform: {Application.platform}");
            Debug.Log($"Data Path: {Application.persistentDataPath}");
        }

        private void OnDestroy()
        {
            if (InputManager.Instance != null)
            {
                InputManager.Instance.OnPause -= TogglePause;
            }
        }

        private void InitializeCoreSystemsIfNeeded()
        {
            if (InputManager.Instance == null && inputManagerPrefab != null)
            {
                Instantiate(inputManagerPrefab);
                Debug.Log("InputManager instantiated by GameManager");
            }

            if (SettingsManager.Instance == null && settingsManagerPrefab != null)
            {
                Instantiate(settingsManagerPrefab);
                Debug.Log("SettingsManager instantiated by GameManager");
            }

            if (FindObjectOfType<PerformanceMonitor>() == null && performanceMonitorPrefab != null)
            {
                Instantiate(performanceMonitorPrefab);
                Debug.Log("PerformanceMonitor instantiated by GameManager");
            }

            if (UnknownDialogueManager.Instance == null && unknownDialogueManagerPrefab != null)
            {
                Instantiate(unknownDialogueManagerPrefab);
                Debug.Log("UnknownDialogueManager instantiated by GameManager");
            }

            if (SeedManager.Instance == null && seedManagerPrefab != null)
            {
                Instantiate(seedManagerPrefab);
                Debug.Log("SeedManager instantiated by GameManager");
            }

            if (ProceduralStateStore.Instance == null && proceduralStateStorePrefab != null)
            {
                Instantiate(proceduralStateStorePrefab);
                Debug.Log("ProceduralStateStore instantiated by GameManager");
            }

            if (CrashLogger.Instance == null && crashLoggerPrefab != null)
            {
                Instantiate(crashLoggerPrefab);
                Debug.Log("CrashLogger instantiated by GameManager");
            }
        }

        /// <summary>
        /// Initializes the seed system based on settings.
        /// </summary>
        private void InitializeSeedSystem()
        {
            if (SeedManager.Instance == null || SettingsManager.Instance == null)
                return;

            // Apply settings-based seed configuration
            if (SettingsManager.Instance.UseProceduralSeed())
            {
                int seed = SettingsManager.Instance.GetProceduralSeed();
                SeedManager.Instance.SetSeed(seed);
                Debug.Log($"Applied settings-based seed: {seed}");
            }
        }

        public void TogglePause()
        {
            isPaused = !isPaused;

            if (isPaused)
            {
                timeScaleBeforePause = Time.timeScale;
                Time.timeScale = 0f;
                Debug.Log("Game Paused");
            }
            else
            {
                Time.timeScale = timeScaleBeforePause;
                Debug.Log("Game Resumed");
            }
        }

        public void SetPaused(bool paused)
        {
            if (isPaused != paused)
            {
                TogglePause();
            }
        }

        public void QuitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        public void LoadScene(string sceneName)
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
        }

        public void LoadSceneAsync(string sceneName)
        {
            StartCoroutine(LoadSceneCoroutine(sceneName));
        }

        private System.Collections.IEnumerator LoadSceneCoroutine(string sceneName)
        {
            AsyncOperation asyncLoad = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneName);

            while (!asyncLoad.isDone)
            {
                float progress = Mathf.Clamp01(asyncLoad.progress / 0.9f);
                Debug.Log($"Loading scene: {progress * 100}%");
                yield return null;
            }
        }
        
        /// <summary>
        /// Performance validation coroutine for build validation.
        /// </summary>
        private IEnumerator PerformanceValidationCoroutine()
        {
            Debug.Log("Starting performance validation for build...");
            
            // Wait for systems to initialize
            yield return new WaitForSeconds(5f);
            
            float validationDuration = 60f; // 1 minute validation
            float validationTimer = 0f;
            
            var performanceMonitor = FindObjectOfType<PerformanceMonitor>();
            var crashLogger = FindObjectOfType<CrashLogger>();
            
            if (performanceMonitor == null)
            {
                Debug.LogWarning("PerformanceMonitor not found for validation");
                yield break;
            }
            
            // Reset performance statistics
            performanceMonitor.ResetStatistics();
            
            while (validationTimer < validationDuration)
            {
                validationTimer += Time.deltaTime;
                
                // Check for performance issues
                float currentFPS = performanceMonitor.GetCurrentFPS();
                float currentMemory = performanceMonitor.GetMemoryUsage() / 1024f / 1024f;
                
                if (currentFPS < 30f)
                {
                    Debug.LogWarning($"Performance validation: Low FPS detected ({currentFPS:F1})");
                    if (crashLogger != null)
                    {
                        crashLogger.LogCrashEvent("PERFORMANCE_WARNING", $"Low FPS: {currentFPS:F1}");
                    }
                }
                
                if (currentMemory > 3.5f * 1024f) // 3.5GB
                {
                    Debug.LogWarning($"Performance validation: High memory usage ({currentMemory:F2} MB)");
                    if (crashLogger != null)
                    {
                        crashLogger.LogCrashEvent("PERFORMANCE_WARNING", $"High memory: {currentMemory:F2} MB");
                    }
                }
                
                yield return new WaitForSeconds(1f);
            }
            
            // Generate performance summary
            var summary = performanceMonitor.GetPerformanceSummary();
            
            Debug.Log($"Performance validation complete:");
            Debug.Log($"  Average FPS: {summary.averageFPS:F1}");
            Debug.Log($"  Performance Grade: {summary.performanceGrade}");
            Debug.Log($"  Target 60 FPS Met: {summary.target60FPSMet}");
            Debug.Log($"  Memory Target Met: {summary.memoryTargetMet}");
            
            // Log performance results
            if (crashLogger != null)
            {
                crashLogger.LogCrashEvent("PERFORMANCE_VALIDATION", "Performance validation completed", summary);
            }
            
            // Export telemetry if needed
            if (!summary.target60FPSMet || !summary.memoryTargetMet)
            {
                performanceMonitor.ExportTelemetryToCSV();
                Debug.LogWarning("Performance targets not met - telemetry exported for analysis");
            }
        }
    }
}
