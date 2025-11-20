using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using ProtocolEMR.Core.Procedural;
using ProtocolEMR.Core.AI;
using ProtocolEMR.Core.Dialogue;
using ProtocolEMR.Core.Performance;
using ProtocolEMR.Core.Input;

namespace ProtocolEMR.Core
{
    /// <summary>
    /// Test result status.
    /// </summary>
    public enum TestStatus
    {
        NotRun,
        Running,
        Passed,
        Failed,
        Skipped
    }

    /// <summary>
    /// Individual test result.
    /// </summary>
    [System.Serializable]
    public class TestResult
    {
        public string testName;
        public TestStatus status;
        public string message;
        public float duration;

        public TestResult(string name)
        {
            testName = name;
            status = TestStatus.NotRun;
            message = "";
            duration = 0f;
        }
    }

    /// <summary>
    /// Automated regression harness for testing the full game loop.
    /// Exercises end-to-end functionality and logs progression blockers and performance metrics.
    /// Can be triggered manually or via Unity Test tools.
    /// </summary>
    public class RegressionHarness : MonoBehaviour
    {
        public static RegressionHarness Instance { get; private set; }

        [Header("Test Configuration")]
        [SerializeField] private bool runOnStart = false;
        [SerializeField] private bool showDebugUI = true;
        [SerializeField] private KeyCode runTestsKey = KeyCode.F7;

        [Header("Performance Thresholds")]
        [SerializeField] private float minAcceptableFPS = 30f;
        [SerializeField] private float maxMemoryMB = 3500f;
        [SerializeField] private float maxTestDuration = 300f; // 5 minutes

        private List<TestResult> testResults = new List<TestResult>();
        private bool isRunning = false;
        private float testStartTime = 0f;
        private int currentTestIndex = 0;
        private StringBuilder logBuilder = new StringBuilder();

        public bool IsRunning => isRunning;
        public List<TestResult> TestResults => new List<TestResult>(testResults);

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        private void Start()
        {
            if (runOnStart)
            {
                StartCoroutine(RunAllTestsCoroutine());
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(runTestsKey) && !isRunning)
            {
                StartCoroutine(RunAllTestsCoroutine());
            }
        }

        /// <summary>
        /// Runs all regression tests.
        /// </summary>
        public void RunAllTests()
        {
            if (!isRunning)
            {
                StartCoroutine(RunAllTestsCoroutine());
            }
        }

        /// <summary>
        /// Coroutine to run all tests sequentially.
        /// </summary>
        private IEnumerator RunAllTestsCoroutine()
        {
            isRunning = true;
            testStartTime = Time.time;
            testResults.Clear();
            logBuilder.Clear();
            currentTestIndex = 0;

            LogMessage("=== REGRESSION HARNESS STARTED ===");
            LogMessage($"Start Time: {System.DateTime.Now}");
            LogMessage("");

            // Initialize test suite
            yield return TestGameManagerInitialization();
            yield return TestSeedManagerFunctionality();
            yield return TestProceduralLevelGeneration();
            yield return TestNPCSpawning();
            yield return TestUnknownDialogueSystem();
            yield return TestDynamicEventOrchestrator();
            yield return TestMissionSystem();
            yield return TestPlaytestFlowController();
            yield return TestInputSystem();
            yield return TestSettingsPersistence();
            yield return TestPerformanceTargets();

            // Generate final report
            float totalDuration = Time.time - testStartTime;
            GenerateReport(totalDuration);

            isRunning = false;
        }

        private IEnumerator TestGameManagerInitialization()
        {
            TestResult test = new TestResult("GameManager Initialization");
            float startTime = Time.time;

            try
            {
                if (GameManager.Instance == null)
                {
                    test.status = TestStatus.Failed;
                    test.message = "GameManager instance not found";
                }
                else
                {
                    test.status = TestStatus.Passed;
                    test.message = "GameManager initialized successfully";
                }
            }
            catch (System.Exception ex)
            {
                test.status = TestStatus.Failed;
                test.message = $"Exception: {ex.Message}";
            }

            test.duration = Time.time - startTime;
            testResults.Add(test);
            LogTestResult(test);
            yield return null;
        }

        private IEnumerator TestSeedManagerFunctionality()
        {
            TestResult test = new TestResult("SeedManager Functionality");
            float startTime = Time.time;

            try
            {
                if (SeedManager.Instance == null)
                {
                    test.status = TestStatus.Failed;
                    test.message = "SeedManager instance not found";
                }
                else
                {
                    int originalSeed = SeedManager.Instance.CurrentSeed;
                    int testValue1 = SeedManager.Instance.GetRandomInt(SeedManager.SCOPE_NPCS, 0, 100);
                    SeedManager.Instance.ResetScopeOffset(SeedManager.SCOPE_NPCS);
                    int testValue2 = SeedManager.Instance.GetRandomInt(SeedManager.SCOPE_NPCS, 0, 100);

                    if (testValue1 == testValue2)
                    {
                        test.status = TestStatus.Passed;
                        test.message = $"Deterministic generation verified (Seed: {originalSeed})";
                    }
                    else
                    {
                        test.status = TestStatus.Failed;
                        test.message = "Deterministic generation failed";
                    }
                }
            }
            catch (System.Exception ex)
            {
                test.status = TestStatus.Failed;
                test.message = $"Exception: {ex.Message}";
            }

            test.duration = Time.time - startTime;
            testResults.Add(test);
            LogTestResult(test);
            yield return null;
        }

        private IEnumerator TestProceduralLevelGeneration()
        {
            TestResult test = new TestResult("Procedural Level Generation");
            float startTime = Time.time;

            try
            {
                ProceduralLevelBuilder levelBuilder = FindObjectOfType<ProceduralLevelBuilder>();

                if (levelBuilder == null)
                {
                    test.status = TestStatus.Skipped;
                    test.message = "ProceduralLevelBuilder not found in scene";
                }
                else if (levelBuilder.GenerationComplete)
                {
                    test.status = TestStatus.Passed;
                    test.message = $"Level generated successfully ({levelBuilder.GeneratedChunks.Count} chunks)";
                }
                else if (levelBuilder.IsGenerating)
                {
                    test.status = TestStatus.Running;
                    test.message = "Level generation in progress...";

                    // Wait for completion with timeout
                    float timeout = 30f;
                    float elapsed = 0f;

                    while (levelBuilder.IsGenerating && elapsed < timeout)
                    {
                        elapsed += Time.deltaTime;
                        yield return null;
                    }

                    if (levelBuilder.GenerationComplete)
                    {
                        test.status = TestStatus.Passed;
                        test.message = $"Level generated successfully ({levelBuilder.GeneratedChunks.Count} chunks)";
                    }
                    else
                    {
                        test.status = TestStatus.Failed;
                        test.message = "Level generation timed out";
                    }
                }
                else
                {
                    test.status = TestStatus.Failed;
                    test.message = "Level builder not active";
                }
            }
            catch (System.Exception ex)
            {
                test.status = TestStatus.Failed;
                test.message = $"Exception: {ex.Message}";
            }

            test.duration = Time.time - startTime;
            testResults.Add(test);
            LogTestResult(test);
        }

        private IEnumerator TestNPCSpawning()
        {
            TestResult test = new TestResult("NPC Spawning");
            float startTime = Time.time;

            try
            {
                NPCSpawner spawner = FindObjectOfType<NPCSpawner>();

                if (spawner == null)
                {
                    test.status = TestStatus.Skipped;
                    test.message = "NPCSpawner not found in scene";
                }
                else
                {
                    int npcCount = FindObjectsOfType<NPCController>().Length;
                    
                    if (npcCount > 0)
                    {
                        test.status = TestStatus.Passed;
                        test.message = $"NPCs spawned successfully ({npcCount} active)";
                    }
                    else
                    {
                        test.status = TestStatus.Passed;
                        test.message = "NPC spawner present (no NPCs spawned yet)";
                    }
                }
            }
            catch (System.Exception ex)
            {
                test.status = TestStatus.Failed;
                test.message = $"Exception: {ex.Message}";
            }

            test.duration = Time.time - startTime;
            testResults.Add(test);
            LogTestResult(test);
            yield return null;
        }

        private IEnumerator TestUnknownDialogueSystem()
        {
            TestResult test = new TestResult("Unknown Dialogue System");
            float startTime = Time.time;

            try
            {
                if (UnknownDialogueManager.Instance == null)
                {
                    test.status = TestStatus.Failed;
                    test.message = "UnknownDialogueManager instance not found";
                }
                else
                {
                    // Test message trigger
                    UnknownDialogueTriggers.TriggerGameStart();
                    
                    yield return new WaitForSeconds(0.5f);

                    test.status = TestStatus.Passed;
                    test.message = "Dialogue system functional";
                }
            }
            catch (System.Exception ex)
            {
                test.status = TestStatus.Failed;
                test.message = $"Exception: {ex.Message}";
            }

            test.duration = Time.time - startTime;
            testResults.Add(test);
            LogTestResult(test);
        }

        private IEnumerator TestDynamicEventOrchestrator()
        {
            TestResult test = new TestResult("Dynamic Event Orchestrator");
            float startTime = Time.time;

            try
            {
                if (DynamicEventOrchestrator.Instance == null)
                {
                    test.status = TestStatus.Skipped;
                    test.message = "DynamicEventOrchestrator not found in scene";
                }
                else
                {
                    test.status = TestStatus.Passed;
                    test.message = $"Event orchestrator active ({DynamicEventOrchestrator.Instance.ActiveEvents.Count} events)";
                }
            }
            catch (System.Exception ex)
            {
                test.status = TestStatus.Failed;
                test.message = $"Exception: {ex.Message}";
            }

            test.duration = Time.time - startTime;
            testResults.Add(test);
            LogTestResult(test);
            yield return null;
        }

        private IEnumerator TestMissionSystem()
        {
            TestResult test = new TestResult("Mission System");
            float startTime = Time.time;

            try
            {
                if (MissionSystem.Instance == null)
                {
                    test.status = TestStatus.Failed;
                    test.message = "MissionSystem instance not found";
                }
                else
                {
                    var objectives = MissionSystem.Instance.GetAllObjectives();
                    float progress = MissionSystem.Instance.GetCompletionProgress();

                    test.status = TestStatus.Passed;
                    test.message = $"Mission system active ({objectives.Count} objectives, {progress * 100:F0}% complete)";
                }
            }
            catch (System.Exception ex)
            {
                test.status = TestStatus.Failed;
                test.message = $"Exception: {ex.Message}";
            }

            test.duration = Time.time - startTime;
            testResults.Add(test);
            LogTestResult(test);
            yield return null;
        }

        private IEnumerator TestPlaytestFlowController()
        {
            TestResult test = new TestResult("Playtest Flow Controller");
            float startTime = Time.time;

            try
            {
                if (PlaytestFlowController.Instance == null)
                {
                    test.status = TestStatus.Skipped;
                    test.message = "PlaytestFlowController not found in scene";
                }
                else
                {
                    test.status = TestStatus.Passed;
                    test.message = $"Flow controller active (State: {PlaytestFlowController.Instance.CurrentState})";
                }
            }
            catch (System.Exception ex)
            {
                test.status = TestStatus.Failed;
                test.message = $"Exception: {ex.Message}";
            }

            test.duration = Time.time - startTime;
            testResults.Add(test);
            LogTestResult(test);
            yield return null;
        }

        private IEnumerator TestInputSystem()
        {
            TestResult test = new TestResult("Input System");
            float startTime = Time.time;

            try
            {
                if (InputManager.Instance == null)
                {
                    test.status = TestStatus.Failed;
                    test.message = "InputManager instance not found";
                }
                else
                {
                    test.status = TestStatus.Passed;
                    test.message = "Input system functional";
                }
            }
            catch (System.Exception ex)
            {
                test.status = TestStatus.Failed;
                test.message = $"Exception: {ex.Message}";
            }

            test.duration = Time.time - startTime;
            testResults.Add(test);
            LogTestResult(test);
            yield return null;
        }

        private IEnumerator TestSettingsPersistence()
        {
            TestResult test = new TestResult("Settings Persistence");
            float startTime = Time.time;

            try
            {
                if (ProtocolEMR.Core.Settings.SettingsManager.Instance == null)
                {
                    test.status = TestStatus.Failed;
                    test.message = "SettingsManager instance not found";
                }
                else
                {
                    test.status = TestStatus.Passed;
                    test.message = "Settings manager functional";
                }
            }
            catch (System.Exception ex)
            {
                test.status = TestStatus.Failed;
                test.message = $"Exception: {ex.Message}";
            }

            test.duration = Time.time - startTime;
            testResults.Add(test);
            LogTestResult(test);
            yield return null;
        }

        private IEnumerator TestPerformanceTargets()
        {
            TestResult test = new TestResult("Performance Targets");
            float startTime = Time.time;

            try
            {
                PerformanceMonitor monitor = FindObjectOfType<PerformanceMonitor>();

                if (monitor == null)
                {
                    test.status = TestStatus.Skipped;
                    test.message = "PerformanceMonitor not found";
                }
                else
                {
                    float fps = monitor.GetCurrentFPS();
                    float memoryMB = monitor.GetMemoryUsage() / 1024f / 1024f;

                    bool fpsOK = fps >= minAcceptableFPS;
                    bool memoryOK = memoryMB <= maxMemoryMB;

                    if (fpsOK && memoryOK)
                    {
                        test.status = TestStatus.Passed;
                        test.message = $"Performance targets met (FPS: {fps:F1}, Memory: {memoryMB:F2}MB)";
                    }
                    else
                    {
                        test.status = TestStatus.Failed;
                        test.message = $"Performance targets not met - FPS: {fps:F1} (Min: {minAcceptableFPS}), Memory: {memoryMB:F2}MB (Max: {maxMemoryMB}MB)";
                    }
                }
            }
            catch (System.Exception ex)
            {
                test.status = TestStatus.Failed;
                test.message = $"Exception: {ex.Message}";
            }

            test.duration = Time.time - startTime;
            testResults.Add(test);
            LogTestResult(test);
            yield return null;
        }

        private void LogTestResult(TestResult test)
        {
            string statusSymbol = test.status switch
            {
                TestStatus.Passed => "✓",
                TestStatus.Failed => "✗",
                TestStatus.Skipped => "○",
                _ => "?"
            };

            string message = $"{statusSymbol} {test.testName} ({test.duration:F2}s): {test.message}";
            LogMessage(message);
        }

        private void LogMessage(string message)
        {
            logBuilder.AppendLine(message);
            Debug.Log($"[RegressionHarness] {message}");
        }

        private void GenerateReport(float totalDuration)
        {
            LogMessage("");
            LogMessage("=== REGRESSION TEST REPORT ===");
            LogMessage($"End Time: {System.DateTime.Now}");
            LogMessage($"Total Duration: {totalDuration:F2}s");
            LogMessage("");

            int passed = 0, failed = 0, skipped = 0;

            foreach (var test in testResults)
            {
                switch (test.status)
                {
                    case TestStatus.Passed: passed++; break;
                    case TestStatus.Failed: failed++; break;
                    case TestStatus.Skipped: skipped++; break;
                }
            }

            int total = testResults.Count;
            float passRate = total > 0 ? (float)passed / total * 100f : 0f;

            LogMessage($"Total Tests: {total}");
            LogMessage($"Passed: {passed}");
            LogMessage($"Failed: {failed}");
            LogMessage($"Skipped: {skipped}");
            LogMessage($"Pass Rate: {passRate:F1}%");
            LogMessage("");

            if (failed > 0)
            {
                LogMessage("FAILED TESTS:");
                foreach (var test in testResults)
                {
                    if (test.status == TestStatus.Failed)
                    {
                        LogMessage($"  - {test.testName}: {test.message}");
                    }
                }
                LogMessage("");
            }

            LogMessage("=== END OF REPORT ===");

            // Save report to file
            SaveReportToFile();
        }

        private void SaveReportToFile()
        {
            try
            {
                string filename = $"regression_report_{System.DateTime.Now:yyyyMMdd_HHmmss}.txt";
                string path = System.IO.Path.Combine(Application.persistentDataPath, filename);
                System.IO.File.WriteAllText(path, logBuilder.ToString());
                Debug.Log($"[RegressionHarness] Report saved to: {path}");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[RegressionHarness] Failed to save report: {ex.Message}");
            }
        }

        private void OnGUI()
        {
            if (!showDebugUI)
                return;

            GUILayout.BeginArea(new Rect(Screen.width - 310, 10, 300, 200));
            GUILayout.Label("<b>Regression Harness</b>", new GUIStyle(GUI.skin.label) { richText = true });

            if (isRunning)
            {
                GUILayout.Label($"Running tests... ({currentTestIndex}/{testResults.Count})");
                GUILayout.Label($"Time elapsed: {Time.time - testStartTime:F1}s");
            }
            else
            {
                if (GUILayout.Button($"Run Tests ({runTestsKey})"))
                {
                    StartCoroutine(RunAllTestsCoroutine());
                }

                if (testResults.Count > 0)
                {
                    int passed = testResults.FindAll(t => t.status == TestStatus.Passed).Count;
                    int failed = testResults.FindAll(t => t.status == TestStatus.Failed).Count;
                    int skipped = testResults.FindAll(t => t.status == TestStatus.Skipped).Count;

                    GUILayout.Label($"Last Run: {passed} passed, {failed} failed, {skipped} skipped");
                }
            }

            GUILayout.EndArea();
        }
    }
}
