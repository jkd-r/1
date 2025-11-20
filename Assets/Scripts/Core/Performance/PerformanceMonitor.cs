using UnityEngine;
using System.Text;
using System.Collections.Generic;
using System.IO;
using ProtocolEMR.Core.Procedural;

namespace ProtocolEMR.Core.Performance
{
    /// <summary>
    /// Enhanced performance monitoring tool for tracking FPS, frame time, memory usage, and system health.
    /// Displays real-time performance metrics and provides telemetry data for build validation.
    /// Can be toggled on/off with F1 key during gameplay.
    /// </summary>
    public class PerformanceMonitor : MonoBehaviour
    {
        [Header("Display Settings")]
        [SerializeField] private bool showOnStart = true;
        [SerializeField] private KeyCode toggleKey = KeyCode.F1;
        [SerializeField] private int fontSize = 14;
        [SerializeField] private Color backgroundColor = new Color(0, 0, 0, 0.7f);
        [SerializeField] private Color textColor = Color.white;

        [Header("Update Settings")]
        [SerializeField] private float updateInterval = 0.5f;
        
        [Header("Performance Tracking")]
        [SerializeField] private bool enableDetailedLogging = false;
        [SerializeField] private bool enableTelemetryCapture = false;
        [SerializeField] private float telemetryInterval = 30.0f;
        [SerializeField] private int maxTelemetryEntries = 120; // 1 hour at 30s intervals

        private bool isVisible;
        private float deltaTime = 0.0f;
        private float updateTimer = 0.0f;
        private int frameCount = 0;
        private float fps = 0.0f;
        private float ms = 0.0f;
        private long memoryUsage = 0;
        private StringBuilder stringBuilder;
        private GUIStyle backgroundStyle;
        private GUIStyle textStyle;
        
        // Enhanced performance tracking
        private float minFPS = float.MaxValue;
        private float maxFPS = 0f;
        private float averageFPS = 0f;
        private int frameDropCount = 0;
        private int hitchCount = 0;
        private float totalFrameTime = 0f;
        private int totalFrames = 0;
        
        // Telemetry data
        private List<TelemetryEntry> telemetryEntries = new List<TelemetryEntry>();
        private float lastTelemetryTime = 0f;
        private string telemetryPath;

        private void Awake()
        {
            isVisible = showOnStart;
            stringBuilder = new StringBuilder(512);
            telemetryPath = Path.Combine(Application.persistentDataPath, "performance_telemetry.json");
            
            // Load existing telemetry if available
            LoadTelemetry();
        }

        private void Update()
        {
            if (Input.GetKeyDown(toggleKey))
            {
                isVisible = !isVisible;
            }

            // Track frame time regardless of visibility
            deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
            frameCount++;
            updateTimer += Time.unscaledDeltaTime;
            totalFrames++;
            totalFrameTime += Time.unscaledDeltaTime;

            if (updateTimer >= updateInterval)
            {
                fps = frameCount / updateTimer;
                ms = (updateTimer / frameCount) * 1000.0f;
                memoryUsage = System.GC.GetTotalMemory(false);

                // Update performance statistics
                UpdatePerformanceStats();
                
                // Check for telemetry capture
                if (enableTelemetryCapture && Time.time - lastTelemetryTime >= telemetryInterval)
                {
                    CaptureTelemetry();
                    lastTelemetryTime = Time.time;
                }

                frameCount = 0;
                updateTimer = 0.0f;
            }
        }

        private void OnGUI()
        {
            if (!isVisible) return;

            if (backgroundStyle == null)
            {
                backgroundStyle = new GUIStyle();
                Texture2D backgroundTexture = new Texture2D(1, 1);
                backgroundTexture.SetPixel(0, 0, backgroundColor);
                backgroundTexture.Apply();
                backgroundStyle.normal.background = backgroundTexture;
                backgroundStyle.padding = new RectOffset(10, 10, 10, 10);
            }

            if (textStyle == null)
            {
                textStyle = new GUIStyle(GUI.skin.label);
                textStyle.fontSize = fontSize;
                textStyle.normal.textColor = textColor;
                textStyle.alignment = TextAnchor.UpperLeft;
            }

            stringBuilder.Clear();
            stringBuilder.AppendLine($"<b>PERFORMANCE MONITOR</b>");
            stringBuilder.AppendLine($"FPS: {fps:F1} (Min: {minFPS:F0}, Max: {maxFPS:F0})");
            stringBuilder.AppendLine($"Frame Time: {ms:F1} ms");
            stringBuilder.AppendLine($"Memory: {(memoryUsage / 1024f / 1024f):F2} MB");
            stringBuilder.AppendLine($"Average FPS: {averageFPS:F1}");
            stringBuilder.AppendLine($"Frame Drops: {frameDropCount}");
            stringBuilder.AppendLine($"Hitches (>16ms): {hitchCount}");
            stringBuilder.AppendLine($"Resolution: {Screen.width}x{Screen.height}");
            stringBuilder.AppendLine($"Quality: {QualitySettings.names[QualitySettings.GetQualityLevel()]}");
            
            if (enableTelemetryCapture)
            {
                stringBuilder.AppendLine($"Telemetry: {telemetryEntries.Count}/{maxTelemetryEntries}");
            }
            
            stringBuilder.AppendLine();
            
            // Display seed information if SeedManager is available
            if (SeedManager.Instance != null)
            {
                stringBuilder.AppendLine($"<b>PROCEDURAL SEED</b>");
                stringBuilder.AppendLine($"Seed: {SeedManager.Instance.CurrentSeed}");
                stringBuilder.AppendLine($"Press F8 to copy seed");
                stringBuilder.AppendLine();
            }
            
            // Display event orchestrator info if available
            if (DynamicEventOrchestrator.Instance != null)
            {
                stringBuilder.AppendLine($"<b>DYNAMIC EVENTS</b>");
                stringBuilder.AppendLine($"Active Events: {DynamicEventOrchestrator.Instance.ActiveEvents.Count}");
                stringBuilder.AppendLine($"Press F9 for details");
                stringBuilder.AppendLine();
            }
            
            stringBuilder.AppendLine($"Press {toggleKey} to toggle");
            
            // Performance warning indicators
            if (fps < 60f)
            {
                stringBuilder.AppendLine($"<color=yellow>⚠ Below 60 FPS target</color>");
            }
            if (memoryUsage > 3.5f * 1024f * 1024f * 1024f) // 3.5GB
            {
                stringBuilder.AppendLine($"<color=orange>⚠ High memory usage</color>");
            }
            if (ms > 16.67f) // 60 FPS threshold
            {
                stringBuilder.AppendLine($"<color=red>⚠ Frame time spike detected</color>");
            }

            string displayText = stringBuilder.ToString();
            Vector2 size = textStyle.CalcSize(new GUIContent(displayText));
            Rect backgroundRect = new Rect(10, 10, size.x + 20, size.y + 20);
            Rect textRect = new Rect(20, 20, size.x, size.y);

            GUI.Box(backgroundRect, "", backgroundStyle);
            GUI.Label(textRect, displayText, textStyle);

            DrawPerformanceBar(backgroundRect.y + backgroundRect.height + 10);
        }

        private void DrawPerformanceBar(float yPosition)
        {
            float barWidth = 200f;
            float barHeight = 20f;
            float xPosition = 10f;

            Color barColor = Color.green;
            if (fps < 60f) barColor = Color.yellow;
            if (fps < 30f) barColor = Color.red;

            float fpsPercentage = Mathf.Clamp01(fps / 120f);

            Rect backgroundBarRect = new Rect(xPosition, yPosition, barWidth, barHeight);
            Rect fillBarRect = new Rect(xPosition, yPosition, barWidth * fpsPercentage, barHeight);

            DrawColoredRect(backgroundBarRect, new Color(0.2f, 0.2f, 0.2f, 0.7f));
            DrawColoredRect(fillBarRect, barColor);
        }

        private void DrawColoredRect(Rect rect, Color color)
        {
            Texture2D texture = new Texture2D(1, 1);
            texture.SetPixel(0, 0, color);
            texture.Apply();
            GUI.DrawTexture(rect, texture);
        }

        public void SetVisible(bool visible)
        {
            isVisible = visible;
        }

        public float GetCurrentFPS()
        {
            return fps;
        }

        public float GetCurrentFrameTime()
        {
            return ms;
        }

        public long GetMemoryUsage()
        {
            return memoryUsage;
        }
        
        /// <summary>
        /// Updates performance statistics.
        /// </summary>
        private void UpdatePerformanceStats()
        {
            // Update min/max FPS
            if (fps < minFPS && fps > 0) minFPS = fps;
            if (fps > maxFPS) maxFPS = fps;
            
            // Update average FPS
            if (totalFrames > 0)
            {
                averageFPS = totalFrames / totalFrameTime;
            }
            
            // Count frame drops (below 30 FPS)
            if (fps < 30f && fps > 0)
            {
                frameDropCount++;
            }
            
            // Count hitches (frame time > 16.67ms)
            if (ms > 16.67f)
            {
                hitchCount++;
            }
            
            if (enableDetailedLogging)
            {
                Debug.Log($"Performance Update - FPS: {fps:F1}, Memory: {(memoryUsage / 1024f / 1024f):F2}MB, Frame Time: {ms:F1}ms");
            }
        }
        
        /// <summary>
        /// Captures telemetry data for performance analysis.
        /// </summary>
        private void CaptureTelemetry()
        {
            try
            {
                var entry = new TelemetryEntry
                {
                    timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    fps = fps,
                    frameTime = ms,
                    memoryUsageMB = memoryUsage / 1024f / 1024f,
                    averageFPS = averageFPS,
                    frameDrops = frameDropCount,
                    hitches = hitchCount,
                    resolution = $"{Screen.width}x{Screen.height}",
                    qualityLevel = QualitySettings.names[QualitySettings.GetQualityLevel()],
                    totalPlaytime = Time.time,
                    seed = SeedManager.Instance?.CurrentSeed ?? 0
                };
                
                telemetryEntries.Add(entry);
                
                // Limit entries to maxTelemetryEntries
                if (telemetryEntries.Count > maxTelemetryEntries)
                {
                    telemetryEntries.RemoveAt(0);
                }
                
                // Save telemetry
                SaveTelemetry();
                
                Debug.Log($"Telemetry captured: {entry.fps:F1} FPS, {entry.memoryUsageMB:F2} MB");
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to capture telemetry: {e.Message}");
            }
        }
        
        /// <summary>
        /// Saves telemetry data to file.
        /// </summary>
        private void SaveTelemetry()
        {
            try
            {
                var telemetryData = new TelemetryData
                {
                    entries = telemetryEntries,
                    captureDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    version = Application.version,
                    platform = Application.platform.ToString()
                };
                
                string json = JsonUtility.ToJson(telemetryData, true);
                File.WriteAllText(telemetryPath, json);
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to save telemetry: {e.Message}");
            }
        }
        
        /// <summary>
        /// Loads telemetry data from file.
        /// </summary>
        private void LoadTelemetry()
        {
            try
            {
                if (File.Exists(telemetryPath))
                {
                    string json = File.ReadAllText(telemetryPath);
                    var telemetryData = JsonUtility.FromJson<TelemetryData>(json);
                    
                    if (telemetryData != null && telemetryData.entries != null)
                    {
                        telemetryEntries = telemetryData.entries;
                        Debug.Log($"Loaded {telemetryEntries.Count} telemetry entries");
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to load telemetry: {e.Message}");
            }
        }
        
        /// <summary>
        /// Gets performance summary for build validation.
        /// </summary>
        public PerformanceSummary GetPerformanceSummary()
        {
            return new PerformanceSummary
            {
                averageFPS = averageFPS,
                minFPS = minFPS,
                maxFPS = maxFPS,
                currentFPS = fps,
                currentFrameTime = ms,
                currentMemoryMB = memoryUsage / 1024f / 1024f,
                totalFrameDrops = frameDropCount,
                totalHitches = hitchCount,
                totalPlaytimeMinutes = Time.time / 60f,
                target60FPSMet = averageFPS >= 60f,
                memoryTargetMet = memoryUsage <= 3.5f * 1024f * 1024f * 1024f, // 3.5GB
                performanceGrade = CalculatePerformanceGrade()
            };
        }
        
        /// <summary>
        /// Calculates performance grade based on metrics.
        /// </summary>
        private string CalculatePerformanceGrade()
        {
            if (averageFPS >= 60f && memoryUsage <= 2.5f * 1024f * 1024f * 1024f && hitchCount == 0)
                return "A+";
            if (averageFPS >= 55f && memoryUsage <= 3.0f * 1024f * 1024f * 1024f && hitchCount <= 5)
                return "A";
            if (averageFPS >= 50f && memoryUsage <= 3.5f * 1024f * 1024f * 1024f && hitchCount <= 10)
                return "B";
            if (averageFPS >= 45f)
                return "C";
            return "D";
        }
        
        /// <summary>
        /// Resets performance statistics.
        /// </summary>
        public void ResetStatistics()
        {
            minFPS = float.MaxValue;
            maxFPS = 0f;
            averageFPS = 0f;
            frameDropCount = 0;
            hitchCount = 0;
            totalFrameTime = 0f;
            totalFrames = 0;
            telemetryEntries.Clear();
            
            Debug.Log("Performance statistics reset");
        }
        
        /// <summary>
        /// Exports telemetry to CSV for analysis.
        /// </summary>
        public void ExportTelemetryToCSV()
        {
            if (telemetryEntries.Count == 0)
            {
                Debug.LogWarning("No telemetry data to export");
                return;
            }
            
            try
            {
                string csvPath = Path.Combine(Application.persistentDataPath, $"performance_telemetry_{DateTime.Now:yyyyMMdd_HHmmss}.csv");
                
                using (StreamWriter writer = new StreamWriter(csvPath))
                {
                    // Header
                    writer.WriteLine("Timestamp,FPS,FrameTime,MemoryMB,AverageFPS,FrameDrops,Hitches,Resolution,QualityLevel,Playtime,Seed");
                    
                    // Data
                    foreach (var entry in telemetryEntries)
                    {
                        writer.WriteLine($"{entry.timestamp},{entry.fps:F1},{entry.frameTime:F2},{entry.memoryUsageMB:F2},{entry.averageFPS:F1},{entry.frameDrops},{entry.hitches},{entry.resolution},{entry.qualityLevel},{entry.totalPlaytime:F1},{entry.seed}");
                    }
                }
                
                Debug.Log($"Telemetry exported to: {csvPath}");
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to export telemetry: {e.Message}");
            }
        }
    }
    
    /// <summary>
    /// Telemetry entry structure.
    /// </summary>
    [Serializable]
    public class TelemetryEntry
    {
        public string timestamp;
        public float fps;
        public float frameTime;
        public float memoryUsageMB;
        public float averageFPS;
        public int frameDrops;
        public int hitches;
        public string resolution;
        public string qualityLevel;
        public float totalPlaytime;
        public int seed;
    }
    
    /// <summary>
    /// Telemetry data container.
    /// </summary>
    [Serializable]
    public class TelemetryData
    {
        public List<TelemetryEntry> entries;
        public string captureDate;
        public string version;
        public string platform;
    }
    
    /// <summary>
    /// Performance summary for build validation.
    /// </summary>
    public class PerformanceSummary
    {
        public float averageFPS;
        public float minFPS;
        public float maxFPS;
        public float currentFPS;
        public float currentFrameTime;
        public float currentMemoryMB;
        public int totalFrameDrops;
        public int totalHitches;
        public float totalPlaytimeMinutes;
        public bool target60FPSMet;
        public bool memoryTargetMet;
        public string performanceGrade;
    }
}
