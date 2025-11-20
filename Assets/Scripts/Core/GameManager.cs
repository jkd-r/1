using UnityEngine;
using ProtocolEMR.Core.Input;
using ProtocolEMR.Core.Settings;
using ProtocolEMR.Core.Performance;
using ProtocolEMR.Core.SaveSystem;

namespace ProtocolEMR.Core
{
    /// <summary>
    /// Central game manager for Protocol EMR.
    /// Initializes and manages core systems including input, settings, performance monitoring, and save/load.
    /// Handles game state transitions and system lifecycle management.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Core Systems")]
        [SerializeField] private InputManager inputManagerPrefab;
        [SerializeField] private SettingsManager settingsManagerPrefab;
        [SerializeField] private PerformanceMonitor performanceMonitorPrefab;

        [Header("Save System")]
        [SerializeField] private bool initializeSaveSystem = true;

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

            if (initializeSaveSystem)
            {
                InitializeSaveSystem();
            }
        }

        private void InitializeSaveSystem()
        {
            if (ProfileManager.Instance == null)
            {
                GameObject profileManager = new GameObject("ProfileManager");
                profileManager.AddComponent<ProfileManager>();
                DontDestroyOnLoad(profileManager);
                Debug.Log("ProfileManager instantiated by GameManager");
            }

            if (SaveGameManager.Instance == null)
            {
                GameObject saveManager = new GameObject("SaveGameManager");
                saveManager.AddComponent<SaveGameManager>();
                DontDestroyOnLoad(saveManager);
                Debug.Log("SaveGameManager instantiated by GameManager");
            }

            if (StatisticsTracker.Instance == null)
            {
                GameObject statsTracker = new GameObject("StatisticsTracker");
                statsTracker.AddComponent<StatisticsTracker>();
                DontDestroyOnLoad(statsTracker);
                Debug.Log("StatisticsTracker instantiated by GameManager");
            }

            if (SaveTriggerSystem.Instance == null)
            {
                GameObject saveTriggers = new GameObject("SaveTriggerSystem");
                saveTriggers.AddComponent<SaveTriggerSystem>();
                DontDestroyOnLoad(saveTriggers);
                Debug.Log("SaveTriggerSystem instantiated by GameManager");
            }

            if (SaveLoadHotkeyHandler.Instance == null)
            {
                GameObject hotkeyHandler = new GameObject("SaveLoadHotkeyHandler");
                hotkeyHandler.AddComponent<SaveLoadHotkeyHandler>();
                DontDestroyOnLoad(hotkeyHandler);
                Debug.Log("SaveLoadHotkeyHandler instantiated by GameManager");
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
    }
}
