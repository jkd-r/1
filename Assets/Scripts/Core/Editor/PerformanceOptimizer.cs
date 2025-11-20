using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using ProtocolEMR.Core.AI;
using ProtocolEMR.Core.Performance;
using ProtocolEMR.Core.Procedural;
using ProtocolEMR.Core.Dialogue;

namespace ProtocolEMR.Core.Editor
{
    /// <summary>
    /// Performance optimization utilities for Protocol EMR build pipeline.
    /// Identifies and addresses performance hotspots in procedural generation, NPC ticks, and dialogue UI.
    /// </summary>
    public class PerformanceOptimizer
    {
        [MenuItem("Protocol EMR/Performance/Optimize Build Settings")]
        public static void OptimizeBuildSettings()
        {
            Debug.Log("Optimizing build settings for performance...");
            
            // Graphics optimization
            OptimizeGraphicsSettings();
            
            // Physics optimization
            OptimizePhysicsSettings();
            
            // Audio optimization
            OptimizeAudioSettings();
            
            // Script compilation optimization
            OptimizeScriptSettings();
            
            // Memory optimization
            OptimizeMemorySettings();
            
            Debug.Log("Build settings optimization complete!");
        }
        
        [MenuItem("Protocol EMR/Performance/Analyze Performance Hotspots")]
        public static void AnalyzePerformanceHotspots()
        {
            Debug.Log("Analyzing performance hotspots...");
            
            var report = new PerformanceReport();
            
            // Analyze NPC system
            AnalyzeNPCSystem(report);
            
            // Analyze Dialogue system
            AnalyzeDialogueSystem(report);
            
            // Analyze Procedural generation
            AnalyzeProceduralSystem(report);
            
            // Analyze UI systems
            AnalyzeUISystem(report);
            
            // Generate report
            GeneratePerformanceReport(report);
            
            Debug.Log("Performance analysis complete!");
        }
        
        [MenuItem("Protocol EMR/Performance/Configure Quality Settings")]
        public static void ConfigureQualitySettings()
        {
            Debug.Log("Configuring quality settings for 60 FPS target...");
            
            // Configure quality levels
            QualitySettings.SetQualityLevel(2); // Medium as default
            
            // Optimize for 60 FPS at 1080p
            QualitySettings.vSyncCount = 1; // Enable VSync
            QualitySettings.antiAliasing = 2; // 2x MSAA
            QualitySettings.anisotropicFiltering = AnisotropicFiltering.Enable;
            QualitySettings.shadowDistance = 50f;
            QualitySettings.shadowCascades = 2;
            QualitySettings.lodBias = 1.0f;
            QualitySettings.maximumLODLevel = 1;
            
            // URP specific optimizations
            var urpAsset = UnityEngine.Rendering.GraphicsSettings.renderPipelineAsset as UnityEngine.Rendering.Universal.UniversalRenderPipelineAsset;
            if (urpAsset != null)
            {
                urpAsset.renderScale = 1.0f;
                urpAsset.shadowDistance = 50f;
                urpAsset.shadowCascadeCount = 2;
                urpAsset.msaaSampleCount = 2;
                urpAsset.renderScale = 1.0f;
                urpAsset.supportsHDR = false; // Disable HDR for performance
                urpAsset.supportsDynamicBatching = true;
                urpAsset.supportsInstancing = true;
            }
            
            Debug.Log("Quality settings configured for 60 FPS target");
        }
        
        [MenuItem("Protocol EMR/Performance/Generate Performance Test Scene")]
        public static void GeneratePerformanceTestScene()
        {
            Debug.Log("Generating performance test scene...");
            
            // Create test scene
            string scenePath = "Assets/Scenes/PerformanceTest.unity";
            
            #if UNITY_EDITOR
            var scene = UnityEditor.SceneManagement.EditorSceneManager.NewScene(UnityEditor.SceneManagement.NewSceneSetup.EmptyScene, UnityEditor.SceneManagement.NewSceneMode.Single);
            
            // Add performance test components
            var performanceTestGO = new GameObject("PerformanceTestController");
            performanceTestGO.AddComponent<PerformanceTestController>();
            
            // Add test objects for stress testing
            CreateStressTestObjects();
            
            // Save scene
            UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene, scenePath);
            #endif
            
            Debug.Log($"Performance test scene created: {scenePath}");
        }
        
        private static void OptimizeGraphicsSettings()
        {
            // Set target frame rate
            Application.targetFrameRate = 60;
            
            // Optimize quality settings
            QualitySettings.skinWeights = SkinWeights.TwoBones;
            QualitySettings.particleRaycastBudget = 64;
            QualitySettings.asyncUploadTimeSlice = 2;
            QualitySettings.asyncUploadBufferSize = 16;
            QualitySettings.lodBias = 1.0f;
            
            // Disable unnecessary features
            QualitySettings.softParticles = false;
            QualitySettings.streamingMipmapsActive = false;
            
            Debug.Log("Graphics settings optimized");
        }
        
        private static void OptimizePhysicsSettings()
        {
            // Optimize physics for performance
            Physics.defaultSolverIterations = 4;
            Physics.defaultSolverVelocityIterations = 1;
            Physics.bounceThreshold = 2f;
            Physics.sleepThreshold = 0.005f;
            Physics.defaultContactOffset = 0.01f;
            
            // Reduce physics overhead
            Physics.autoSimulation = true;
            Physics.autoSyncTransforms = false;
            
            Debug.Log("Physics settings optimized");
        }
        
        private static void OptimizeAudioSettings()
        {
            // Optimize audio settings
            AudioConfiguration config = AudioSettings.GetConfiguration();
            config.sampleRate = 44100;
            config.dspBufferSize = 1024;
            AudioSettings.Reset(config);
            
            Debug.Log("Audio settings optimized");
        }
        
        private static void OptimizeScriptSettings()
        {
            // Optimize script compilation
            PlayerSettings.stripEngineCode = true;
            PlayerSettings.stripUnusedMeshComponents = true;
            PlayerSettings.optimizeMeshData = true;
            
            // Set IL2CPP for better performance
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.Standalone, ScriptingImplementation.IL2CPP);
            
            Debug.Log("Script settings optimized");
        }
        
        private static void OptimizeMemorySettings()
        {
            // Configure garbage collection
            System.GC.Collect();
            
            // Optimize texture import settings
            OptimizeTextureSettings();
            
            Debug.Log("Memory settings optimized");
        }
        
        private static void OptimizeTextureSettings()
        {
            // Find all texture assets and optimize them
            string[] textureGuids = AssetDatabase.FindAssets("t:texture");
            
            foreach (string guid in textureGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var importer = AssetImporter.GetAtPath(path) as TextureImporter;
                
                if (importer != null)
                {
                    // Optimize for performance
                    importer.mipmapEnabled = true;
                    importer.wrapMode = TextureWrapMode.Repeat;
                    importer.isReadable = false;
                    
                    // Compress textures
                    if (importer.textureCompression == TextureImporterCompression.Uncompressed)
                    {
                        importer.textureCompression = TextureImporterCompression.Compressed;
                    }
                    
                    importer.SaveAndReimport();
                }
            }
            
            Debug.Log($"Optimized {textureGuids.Length} textures");
        }
        
        private static void AnalyzeNPCSystem(PerformanceReport report)
        {
            report.npcAnalysis = new NPCAnalysis();
            
            // Check for potential NPC performance issues
            var npcControllers = Object.FindObjectsOfType<NPCController>();
            
            if (npcControllers.Length > 20)
            {
                report.npcAnalysis.recommendations.Add($"High NPC count ({npcControllers.Length}). Consider implementing NPC pooling or culling.");
            }
            
            // Check behavior tree complexity
            foreach (var npc in npcControllers)
            {
                if (npc.BehaviorTreeRoot != null)
                {
                    int nodeCount = CountBehaviorNodes(npc.BehaviorTreeRoot);
                    if (nodeCount > 50)
                    {
                        report.npcAnalysis.recommendations.Add($"Complex behavior tree detected ({nodeCount} nodes). Consider simplifying or optimizing.");
                    }
                }
            }
            
            report.npcAnalysis.totalNPCs = npcControllers.Length;
            report.npcAnalysis.status = "Analysis complete";
        }
        
        private static void AnalyzeDialogueSystem(PerformanceReport report)
        {
            report.dialogueAnalysis = new DialogueAnalysis();
            
            // Check dialogue system performance
            var dialogueManager = Object.FindObjectOfType<UnknownDialogueManager>();
            
            if (dialogueManager != null)
            {
                // Check message database size
                var messageDatabase = dialogueManager.GetComponent<UnknownDialogueManager>();
                if (messageDatabase != null)
                {
                    // This would need to be implemented based on the actual message database structure
                    report.dialogueAnalysis.recommendations.Add("Dialogue system detected. Ensure message database is optimized for fast lookups.");
                }
            }
            
            report.dialogueAnalysis.status = "Analysis complete";
        }
        
        private static void AnalyzeProceduralSystem(PerformanceReport report)
        {
            report.proceduralAnalysis = new ProceduralAnalysis();
            
            // Check procedural generation performance
            var seedManager = Object.FindObjectOfType<SeedManager>();
            
            if (seedManager != null)
            {
                report.proceduralAnalysis.seedSystemActive = true;
                report.proceduralAnalysis.recommendations.Add("Seed system is active. Monitor random number generation performance.");
            }
            
            report.proceduralAnalysis.status = "Analysis complete";
        }
        
        private static void AnalyzeUISystem(PerformanceReport report)
        {
            report.uiAnalysis = new UIAnalysis();
            
            // Find all UI canvases
            var canvases = Object.FindObjectsOfType<Canvas>();
            
            foreach (var canvas in canvases)
            {
                if (canvas.renderMode != RenderMode.ScreenSpaceOverlay)
                {
                    report.uiAnalysis.recommendations.Add($"Non-overlay canvas detected: {canvas.name}. Consider using ScreenSpaceOverlay for better performance.");
                }
                
                if (canvas.pixelPerfect)
                {
                    report.uiAnalysis.recommendations.Add($"PixelPerfect canvas detected: {canvas.name}. Disable for better performance.");
                }
            }
            
            report.uiAnalysis.totalCanvases = canvases.Length;
            report.uiAnalysis.status = "Analysis complete";
        }
        
        private static int CountBehaviorNodes(BehaviorNode node)
        {
            if (node == null) return 0;
            
            int count = 1;
            
            // Simplified node counting - would need access to actual BehaviorNode implementation
            // This is a placeholder for the optimization analysis
            try
            {
                // Use reflection or public API to count children if available
                var childrenProperty = node.GetType().GetProperty("Children");
                if (childrenProperty != null)
                {
                    var children = childrenProperty.GetValue(node) as IEnumerable<BehaviorNode>;
                    if (children != null)
                    {
                        foreach (var child in children)
                        {
                            count += CountBehaviorNodes(child);
                        }
                    }
                }
            }
            catch
            {
                // If we can't access children, just count the node itself
                count = 1;
            }
            
            return count;
        }
        
        private static void GeneratePerformanceReport(PerformanceReport report)
        {
            string reportPath = Path.Combine(Application.dataPath, "../PerformanceReport.txt");
            
            using (StreamWriter writer = new StreamWriter(reportPath))
            {
                writer.WriteLine("=== Protocol EMR Performance Analysis Report ===");
                writer.WriteLine($"Generated: {System.DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                writer.WriteLine();
                
                writer.WriteLine("--- NPC System Analysis ---");
                writer.WriteLine($"Total NPCs: {report.npcAnalysis.totalNPCs}");
                writer.WriteLine($"Status: {report.npcAnalysis.status}");
                foreach (var recommendation in report.npcAnalysis.recommendations)
                {
                    writer.WriteLine($"  • {recommendation}");
                }
                writer.WriteLine();
                
                writer.WriteLine("--- Dialogue System Analysis ---");
                writer.WriteLine($"Status: {report.dialogueAnalysis.status}");
                foreach (var recommendation in report.dialogueAnalysis.recommendations)
                {
                    writer.WriteLine($"  • {recommendation}");
                }
                writer.WriteLine();
                
                writer.WriteLine("--- Procedural System Analysis ---");
                writer.WriteLine($"Seed System Active: {report.proceduralAnalysis.seedSystemActive}");
                writer.WriteLine($"Status: {report.proceduralAnalysis.status}");
                foreach (var recommendation in report.proceduralAnalysis.recommendations)
                {
                    writer.WriteLine($"  • {recommendation}");
                }
                writer.WriteLine();
                
                writer.WriteLine("--- UI System Analysis ---");
                writer.WriteLine($"Total Canvases: {report.uiAnalysis.totalCanvases}");
                writer.WriteLine($"Status: {report.uiAnalysis.status}");
                foreach (var recommendation in report.uiAnalysis.recommendations)
                {
                    writer.WriteLine($"  • {recommendation}");
                }
                writer.WriteLine();
                
                writer.WriteLine("--- Performance Targets ---");
                writer.WriteLine("Target: 60 FPS @ 1080p Medium settings");
                writer.WriteLine("Memory Target: <3.5 GB");
                writer.WriteLine("Frame Time Target: <16.67ms");
                writer.WriteLine();
                
                writer.WriteLine("--- Recommendations ---");
                writer.WriteLine("1. Run Unity Profiler to identify specific hotspots");
                writer.WriteLine("2. Test with 20+ NPCs to verify performance");
                writer.WriteLine("3. Monitor memory usage during extended play sessions");
                writer.WriteLine("4. Validate procedural generation determinism");
                writer.WriteLine("5. Test dialogue system performance with large message databases");
            }
            
            Debug.Log($"Performance report generated: {reportPath}");
        }
        
        private static void CreateStressTestObjects()
        {
            // Create stress test objects for performance testing
            int npcCount = 20;
            
            for (int i = 0; i < npcCount; i++)
            {
                var npc = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                npc.name = $"StressTestNPC_{i}";
                npc.transform.position = new Vector3(i * 2f, 0f, 0f);
                
                // Add NPC controller for stress testing
                if (npc.GetComponent<NPCController>() == null)
                {
                    npc.AddComponent<NPCController>();
                }
            }
            
            Debug.Log($"Created {npcCount} stress test NPCs");
        }
    }
    
    /// <summary>
    /// Performance analysis report structure.
    /// </summary>
    [System.Serializable]
    public class PerformanceReport
    {
        public NPCAnalysis npcAnalysis;
        public DialogueAnalysis dialogueAnalysis;
        public ProceduralAnalysis proceduralAnalysis;
        public UIAnalysis uiAnalysis;
    }
    
    [System.Serializable]
    public class NPCAnalysis
    {
        public int totalNPCs;
        public string status;
        public List<string> recommendations = new List<string>();
    }
    
    [System.Serializable]
    public class DialogueAnalysis
    {
        public string status;
        public List<string> recommendations = new List<string>();
    }
    
    [System.Serializable]
    public class ProceduralAnalysis
    {
        public bool seedSystemActive;
        public string status;
        public List<string> recommendations = new List<string>();
    }
    
    [System.Serializable]
    public class UIAnalysis
    {
        public int totalCanvases;
        public string status;
        public List<string> recommendations = new List<string>();
    }
    
    /// <summary>
    /// Performance test controller for automated testing.
    /// </summary>
    public class PerformanceTestController : MonoBehaviour
    {
        [Header("Test Configuration")]
        [SerializeField] private bool enableAutomatedTesting = true;
        [SerializeField] private float testDuration = 3600f; // 1 hour
        [SerializeField] private int npcSpawnCount = 20;
        
        private float testStartTime;
        private bool testRunning;
        
        private void Start()
        {
            if (enableAutomatedTesting)
            {
                StartPerformanceTest();
            }
        }
        
        public void StartPerformanceTest()
        {
            Debug.Log("Starting automated performance test...");
            testStartTime = Time.time;
            testRunning = true;
            
            // Spawn test NPCs
            SpawnTestNPCs();
            
            // Start telemetry capture
            var performanceMonitor = FindObjectOfType<PerformanceMonitor>();
            if (performanceMonitor != null)
            {
                performanceMonitor.ResetStatistics();
            }
        }
        
        private void Update()
        {
            if (!testRunning) return;
            
            // Check if test duration has elapsed
            if (Time.time - testStartTime >= testDuration)
            {
                CompletePerformanceTest();
            }
        }
        
        private void SpawnTestNPCs()
        {
            // Implementation would depend on the actual NPC spawning system
            Debug.Log($"Spawning {npcSpawnCount} test NPCs for performance testing");
        }
        
        private void CompletePerformanceTest()
        {
            testRunning = false;
            
            // Generate performance report
            var performanceMonitor = FindObjectOfType<PerformanceMonitor>();
            if (performanceMonitor != null)
            {
                var summary = performanceMonitor.GetPerformanceSummary();
                Debug.Log($"Performance test completed: {summary.performanceGrade} grade, {summary.averageFPS:F1} avg FPS");
                
                // Export telemetry
                performanceMonitor.ExportTelemetryToCSV();
            }
            
            Debug.Log("Automated performance test completed!");
        }
    }
}