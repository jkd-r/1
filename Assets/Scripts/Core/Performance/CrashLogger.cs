using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;
using System.Text;

namespace ProtocolEMR.Core.Performance
{
    /// <summary>
    /// Crash logging and telemetry system for build validation.
    /// Captures crash information, performance metrics, and system state for debugging.
    /// </summary>
    public class CrashLogger : MonoBehaviour
    {
        public static CrashLogger Instance { get; private set; }
        
        [Header("Crash Logging")]
        [SerializeField] private bool enableCrashLogging = true;
        [SerializeField] private bool enableTelemetryCapture = true;
        [SerializeField] private float telemetryInterval = 60f;
        [SerializeField] private int maxLogEntries = 100;
        
        private string crashLogPath;
        private string telemetryPath;
        private List<CrashLogEntry> crashLogs = new List<CrashLogEntry>();
        private List<TelemetrySnapshot> telemetryData = new List<TelemetrySnapshot>();
        private float lastTelemetryTime;
        private string sessionID;
        
        // System state tracking
        private Vector3 lastPlayerPosition;
        private int lastPlayerHealth;
        private string lastSceneName;
        private float sessionStartTime;
        
        public string SessionID => sessionID;
        
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            InitializeCrashLogger();
        }
        
        private void Start()
        {
            sessionStartTime = Time.time;
            lastSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            
            // Register for application quit
            Application.quitting += OnApplicationQuit;
            
            // Register for scene changes
            UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
            
            // Register for unhandled exceptions
            Application.logMessageReceived += HandleLog;
            
            LogSystemInfo();
        }
        
        private void Update()
        {
            if (enableTelemetryCapture && Time.time - lastTelemetryTime >= telemetryInterval)
            {
                CaptureTelemetrySnapshot();
                lastTelemetryTime = Time.time;
            }
            
            // Track player state
            TrackPlayerState();
        }
        
        /// <summary>
        /// Initializes the crash logger.
        /// </summary>
        private void InitializeCrashLogger()
        {
            sessionID = Guid.NewGuid().ToString("N")[..8];
            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            
            crashLogPath = Path.Combine(Application.persistentDataPath, $"crash_log_{timestamp}_{sessionID}.json");
            telemetryPath = Path.Combine(Application.persistentDataPath, $"telemetry_{timestamp}_{sessionID}.json");
            
            Debug.Log($"CrashLogger initialized - Session: {sessionID}");
        }
        
        /// <summary>
        /// Logs system information.
        /// </summary>
        private void LogSystemInfo()
        {
            var systemInfo = new SystemInfoEntry
            {
                sessionID = sessionID,
                timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                operatingSystem = SystemInfo.operatingSystem,
                deviceModel = SystemInfo.deviceModel,
                processorType = SystemInfo.processorType,
                processorCount = SystemInfo.processorCount,
                graphicsDeviceName = SystemInfo.graphicsDeviceName,
                graphicsDeviceType = SystemInfo.graphicsDeviceType.ToString(),
                graphicsMemorySize = SystemInfo.graphicsMemorySize,
                systemMemorySize = SystemInfo.systemMemorySize,
                unityVersion = Application.version,
                applicationVersion = Application.version,
                targetFrameRate = Application.targetFrameRate,
                qualityLevel = QualitySettings.names[QualitySettings.GetQualityLevel()],
                screenResolution = $"{Screen.width}x{Screen.height}@{Screen.currentResolution.refreshRate}Hz"
            };
            
            LogCrashEvent("SYSTEM_INFO", "System information captured", systemInfo);
        }
        
        /// <summary>
        /// Handles Unity log messages.
        /// </summary>
        private void HandleLog(string logString, string stackTrace, LogType type)
        {
            if (!enableCrashLogging) return;
            
            // Only log errors and exceptions
            if (type == LogType.Error || type == LogType.Exception)
            {
                var errorEntry = new ErrorInfo
                {
                    message = logString,
                    stackTrace = stackTrace,
                    logType = type.ToString(),
                    timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                };
                
                LogCrashEvent("UNITY_ERROR", $"Unity {type.ToString()}", errorEntry);
                
                // Save immediately for critical errors
                if (type == LogType.Exception)
                {
                    SaveCrashLog();
                }
            }
        }
        
        /// <summary>
        /// Logs a crash event.
        /// </summary>
        public void LogCrashEvent(string eventType, string description, object data = null)
        {
            try
            {
                var entry = new CrashLogEntry
                {
                    sessionID = sessionID,
                    timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    eventType = eventType,
                    description = description,
                    dataJson = data != null ? JsonUtility.ToJson(data) : null,
                    sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name,
                    gameTime = Time.time.ToString("F2"),
                    memoryUsageMB = System.GC.GetTotalMemory(false) / 1024f / 1024f,
                    fps = GetCurrentFPS()
                };
                
                crashLogs.Add(entry);
                
                // Limit entries
                if (crashLogs.Count > maxLogEntries)
                {
                    crashLogs.RemoveAt(0);
                }
                
                // Save critical events immediately
                if (eventType == "CRASH" || eventType == "UNITY_EXCEPTION")
                {
                    SaveCrashLog();
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to log crash event: {e.Message}");
            }
        }
        
        /// <summary>
        /// Captures telemetry snapshot.
        /// </summary>
        private void CaptureTelemetrySnapshot()
        {
            try
            {
                var performanceMonitor = FindObjectOfType<PerformanceMonitor>();
                var seedManager = FindObjectOfType<SeedManager>();
                
                var snapshot = new TelemetrySnapshot
                {
                    sessionID = sessionID,
                    timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    gameTime = Time.time,
                    memoryUsageMB = System.GC.GetTotalMemory(false) / 1024f / 1024f,
                    fps = performanceMonitor?.GetCurrentFPS() ?? 0f,
                    frameTime = performanceMonitor?.GetCurrentFrameTime() ?? 0f,
                    sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name,
                    playerPosition = GetPlayerPosition(),
                    seed = seedManager?.CurrentSeed ?? 0,
                    activeGameObjects = FindObjectsOfType<GameObject>().Length,
                    activeNPCs = FindObjectsOfType<NPCController>().Length,
                    systemLoad = GetSystemLoad()
                };
                
                telemetryData.Add(snapshot);
                
                // Limit telemetry entries
                if (telemetryData.Count > 1440) // 24 hours at 1-minute intervals
                {
                    telemetryData.RemoveAt(0);
                }
                
                // Save telemetry periodically
                if (telemetryData.Count % 10 == 0)
                {
                    SaveTelemetry();
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to capture telemetry: {e.Message}");
            }
        }
        
        /// <summary>
        /// Tracks player state changes.
        /// </summary>
        private void TrackPlayerState()
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                Vector3 currentPosition = player.transform.position;
                
                // Check for significant position change
                if (Vector3.Distance(currentPosition, lastPlayerPosition) > 10f)
                {
                    lastPlayerPosition = currentPosition;
                    LogCrashEvent("PLAYER_POSITION", "Player moved significantly", new { position = currentPosition });
                }
            }
        }
        
        /// <summary>
        /// Handles scene loaded event.
        /// </summary>
        private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
        {
            if (scene.name != lastSceneName)
            {
                LogCrashEvent("SCENE_CHANGE", $"Scene changed from {lastSceneName} to {scene.name}", 
                    new { fromScene = lastSceneName, toScene = scene.name });
                lastSceneName = scene.name;
            }
        }
        
        /// <summary>
        /// Handles application quit.
        /// </summary>
        private void OnApplicationQuit()
        {
            LogCrashEvent("SESSION_END", "Application quit normally", 
                new { sessionDuration = Time.time - sessionStartTime });
            
            SaveCrashLog();
            SaveTelemetry();
        }
        
        /// <summary>
        /// Handles application pause (useful for mobile crashes).
        /// </summary>
        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                LogCrashEvent("APP_PAUSE", "Application paused");
                SaveCrashLog();
                SaveTelemetry();
            }
            else
            {
                LogCrashEvent("APP_RESUME", "Application resumed");
            }
        }
        
        /// <summary>
        /// Gets current FPS estimate.
        /// </summary>
        private float GetCurrentFPS()
        {
            return 1.0f / Time.unscaledDeltaTime;
        }
        
        /// <summary>
        /// Gets player position.
        /// </summary>
        private Vector3 GetPlayerPosition()
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            return player != null ? player.transform.position : Vector3.zero;
        }
        
        /// <summary>
        /// Gets system load estimate.
        /// </summary>
        private float GetSystemLoad()
        {
            // Simple estimate based on frame time
            float frameTime = Time.unscaledDeltaTime * 1000f; // Convert to ms
            return Mathf.Clamp01(frameTime / 16.67f); // Normalize to 60 FPS target
        }
        
        /// <summary>
        /// Saves crash log to file.
        /// </summary>
        private void SaveCrashLog()
        {
            try
            {
                var crashLogData = new CrashLogData
                {
                    sessionID = sessionID,
                    entries = crashLogs,
                    sessionStartTime = sessionStartTime,
                    lastUpdated = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                };
                
                string json = JsonUtility.ToJson(crashLogData, true);
                File.WriteAllText(crashLogPath, json);
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to save crash log: {e.Message}");
            }
        }
        
        /// <summary>
        /// Saves telemetry data to file.
        /// </summary>
        private void SaveTelemetry()
        {
            try
            {
                var telemetryLogData = new TelemetryLogData
                {
                    sessionID = sessionID,
                    snapshots = telemetryData,
                    lastUpdated = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                };
                
                string json = JsonUtility.ToJson(telemetryLogData, true);
                File.WriteAllText(telemetryPath, json);
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to save telemetry: {e.Message}");
            }
        }
        
        /// <summary>
        /// Forces a crash log entry (for testing).
        /// </summary>
        [ContextMenu("Force Test Crash Log")]
        public void ForceTestCrashLog()
        {
            LogCrashEvent("TEST_CRASH", "This is a test crash log entry", 
                new { test = true, timestamp = DateTime.Now });
        }
        
        /// <summary>
        /// Gets crash log summary.
        /// </summary>
        public CrashLogSummary GetCrashLogSummary()
        {
            return new CrashLogSummary
            {
                sessionID = sessionID,
                totalEntries = crashLogs.Count,
                errorCount = crashLogs.Count(e => e.eventType == "UNITY_ERROR"),
                exceptionCount = crashLogs.Count(e => e.eventType == "UNITY_EXCEPTION"),
                sessionDuration = Time.time - sessionStartTime,
                lastUpdated = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
            };
        }
        
        /// <summary>
        /// Exports crash log to readable format.
        /// </summary>
        public void ExportCrashLogToText()
        {
            try
            {
                string textPath = Path.Combine(Application.persistentDataPath, $"crash_log_readable_{sessionID}.txt");
                
                using (StreamWriter writer = new StreamWriter(textPath))
                {
                    writer.WriteLine("=== Protocol EMR Crash Log ===");
                    writer.WriteLine($"Session ID: {sessionID}");
                    writer.WriteLine($"Export Date: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                    writer.WriteLine($"Total Entries: {crashLogs.Count}");
                    writer.WriteLine();
                    
                    foreach (var entry in crashLogs)
                    {
                        writer.WriteLine($"[{entry.timestamp}] {entry.eventType}: {entry.description}");
                        writer.WriteLine($"  Scene: {entry.sceneName}");
                        writer.WriteLine($"  Game Time: {entry.gameTime}s");
                        writer.WriteLine($"  Memory: {entry.memoryUsageMB:F2} MB");
                        writer.WriteLine($"  FPS: {entry.fps:F1}");
                        
                        if (!string.IsNullOrEmpty(entry.dataJson))
                        {
                            writer.WriteLine($"  Data: {entry.dataJson}");
                        }
                        
                        writer.WriteLine();
                    }
                }
                
                Debug.Log($"Crash log exported to: {textPath}");
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to export crash log: {e.Message}");
            }
        }
    }
    
    /// <summary>
    /// Crash log entry structure.
    /// </summary>
    [Serializable]
    public class CrashLogEntry
    {
        public string sessionID;
        public string timestamp;
        public string eventType;
        public string description;
        public string dataJson;
        public string sceneName;
        public string gameTime;
        public float memoryUsageMB;
        public float fps;
    }
    
    /// <summary>
    /// Telemetry snapshot structure.
    /// </summary>
    [Serializable]
    public class TelemetrySnapshot
    {
        public string sessionID;
        public string timestamp;
        public float gameTime;
        public float memoryUsageMB;
        public float fps;
        public float frameTime;
        public string sceneName;
        public Vector3 playerPosition;
        public int seed;
        public int activeGameObjects;
        public int activeNPCs;
        public float systemLoad;
    }
    
    /// <summary>
    /// System info entry structure.
    /// </summary>
    [Serializable]
    public class SystemInfoEntry
    {
        public string sessionID;
        public string timestamp;
        public string operatingSystem;
        public string deviceModel;
        public string processorType;
        public int processorCount;
        public string graphicsDeviceName;
        public string graphicsDeviceType;
        public int graphicsMemorySize;
        public int systemMemorySize;
        public string unityVersion;
        public string applicationVersion;
        public int targetFrameRate;
        public string qualityLevel;
        public string screenResolution;
    }
    
    /// <summary>
    /// Error info structure.
    /// </summary>
    [Serializable]
    public class ErrorInfo
    {
        public string message;
        public string stackTrace;
        public string logType;
        public string timestamp;
    }
    
    /// <summary>
    /// Crash log data container.
    /// </summary>
    [Serializable]
    public class CrashLogData
    {
        public string sessionID;
        public List<CrashLogEntry> entries;
        public float sessionStartTime;
        public string lastUpdated;
    }
    
    /// <summary>
    /// Telemetry log data container.
    /// </summary>
    [Serializable]
    public class TelemetryLogData
    {
        public string sessionID;
        public List<TelemetrySnapshot> snapshots;
        public string lastUpdated;
    }
    
    /// <summary>
    /// Crash log summary.
    /// </summary>
    public class CrashLogSummary
    {
        public string sessionID;
        public int totalEntries;
        public int errorCount;
        public int exceptionCount;
        public float sessionDuration;
        public string lastUpdated;
    }
}