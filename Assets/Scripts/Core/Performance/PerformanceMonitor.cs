using UnityEngine;
using System.Text;
using ProtocolEMR.Core.Procedural;

namespace ProtocolEMR.Core.Performance
{
    /// <summary>
    /// Debug performance monitoring tool for tracking FPS, frame time, and memory usage.
    /// Displays real-time performance metrics in the game view for development and optimization.
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

        private void Awake()
        {
            isVisible = showOnStart;
            stringBuilder = new StringBuilder(256);
        }

        private void Update()
        {
            if (Input.GetKeyDown(toggleKey))
            {
                isVisible = !isVisible;
            }

            if (!isVisible) return;

            deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
            frameCount++;
            updateTimer += Time.unscaledDeltaTime;

            if (updateTimer >= updateInterval)
            {
                fps = frameCount / updateTimer;
                ms = (updateTimer / frameCount) * 1000.0f;
                memoryUsage = System.GC.GetTotalMemory(false);

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
            stringBuilder.AppendLine($"FPS: {fps:F1}");
            stringBuilder.AppendLine($"Frame Time: {ms:F1} ms");
            stringBuilder.AppendLine($"Memory: {(memoryUsage / 1024f / 1024f):F2} MB");
            stringBuilder.AppendLine($"Resolution: {Screen.width}x{Screen.height}");
            stringBuilder.AppendLine($"Quality: {QualitySettings.names[QualitySettings.GetQualityLevel()]}");
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
    }
}
