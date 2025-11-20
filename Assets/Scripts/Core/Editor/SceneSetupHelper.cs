using UnityEngine;
using UnityEditor;
using ProtocolEMR.Core;
using ProtocolEMR.Core.Procedural;
using ProtocolEMR.Core.AI;
using ProtocolEMR.Core.Input;
using ProtocolEMR.Core.Settings;
using ProtocolEMR.Core.Performance;
using ProtocolEMR.Core.Dialogue;

namespace ProtocolEMR.Core.Editor
{
    /// <summary>
    /// Editor utility to automatically set up the Main scene with all required systems.
    /// </summary>
    public class SceneSetupHelper : EditorWindow
    {
        [MenuItem("Protocol EMR/Setup/Setup Main Scene")]
        public static void SetupMainScene()
        {
            Debug.Log("[SceneSetup] Starting Main scene setup...");

            // Create core systems
            CreateGameManager();
            CreateMissionSystem();
            CreatePlaytestFlowController();
            CreateRegressionHarness();
            CreateProceduralLevelBuilder();
            CreateNPCSpawner();
            CreateDynamicEventOrchestrator();
            CreateWorldStateBlackboard();

            // Create player spawn point
            CreatePlayerSpawnPoint();

            // Create basic level geometry
            CreateBasicLevelGeometry();

            Debug.Log("[SceneSetup] Main scene setup complete!");
            EditorUtility.DisplayDialog("Scene Setup", "Main scene has been set up successfully!", "OK");
        }

        private static void CreateGameManager()
        {
            GameObject existing = GameObject.Find("GameManager");
            if (existing != null)
            {
                Debug.Log("[SceneSetup] GameManager already exists");
                return;
            }

            GameObject gameManager = new GameObject("GameManager");
            gameManager.AddComponent<GameManager>();
            Undo.RegisterCreatedObjectUndo(gameManager, "Create GameManager");

            Debug.Log("[SceneSetup] Created GameManager");
        }

        private static void CreateMissionSystem()
        {
            GameObject existing = GameObject.Find("MissionSystem");
            if (existing != null)
            {
                Debug.Log("[SceneSetup] MissionSystem already exists");
                return;
            }

            GameObject missionSystem = new GameObject("MissionSystem");
            missionSystem.AddComponent<MissionSystem>();
            Undo.RegisterCreatedObjectUndo(missionSystem, "Create MissionSystem");

            Debug.Log("[SceneSetup] Created MissionSystem");
        }

        private static void CreatePlaytestFlowController()
        {
            GameObject existing = GameObject.Find("PlaytestFlowController");
            if (existing != null)
            {
                Debug.Log("[SceneSetup] PlaytestFlowController already exists");
                return;
            }

            GameObject flowController = new GameObject("PlaytestFlowController");
            PlaytestFlowController controller = flowController.AddComponent<PlaytestFlowController>();
            Undo.RegisterCreatedObjectUndo(flowController, "Create PlaytestFlowController");

            Debug.Log("[SceneSetup] Created PlaytestFlowController");
        }

        private static void CreateRegressionHarness()
        {
            GameObject existing = GameObject.Find("RegressionHarness");
            if (existing != null)
            {
                Debug.Log("[SceneSetup] RegressionHarness already exists");
                return;
            }

            GameObject harness = new GameObject("RegressionHarness");
            harness.AddComponent<RegressionHarness>();
            Undo.RegisterCreatedObjectUndo(harness, "Create RegressionHarness");

            Debug.Log("[SceneSetup] Created RegressionHarness");
        }

        private static void CreateProceduralLevelBuilder()
        {
            GameObject existing = GameObject.Find("ProceduralLevelBuilder");
            if (existing != null)
            {
                Debug.Log("[SceneSetup] ProceduralLevelBuilder already exists");
                return;
            }

            GameObject levelBuilder = new GameObject("ProceduralLevelBuilder");
            levelBuilder.AddComponent<ProceduralLevelBuilder>();
            Undo.RegisterCreatedObjectUndo(levelBuilder, "Create ProceduralLevelBuilder");

            Debug.Log("[SceneSetup] Created ProceduralLevelBuilder (Note: Configure chunk definitions in Inspector)");
        }

        private static void CreateNPCSpawner()
        {
            GameObject existing = GameObject.Find("NPCSpawner");
            if (existing != null)
            {
                Debug.Log("[SceneSetup] NPCSpawner already exists");
                return;
            }

            GameObject npcSpawner = new GameObject("NPCSpawner");
            npcSpawner.AddComponent<NPCSpawner>();
            Undo.RegisterCreatedObjectUndo(npcSpawner, "Create NPCSpawner");

            Debug.Log("[SceneSetup] Created NPCSpawner");
        }

        private static void CreateDynamicEventOrchestrator()
        {
            GameObject existing = GameObject.Find("DynamicEventOrchestrator");
            if (existing != null)
            {
                Debug.Log("[SceneSetup] DynamicEventOrchestrator already exists");
                return;
            }

            GameObject orchestrator = new GameObject("DynamicEventOrchestrator");
            orchestrator.AddComponent<DynamicEventOrchestrator>();
            Undo.RegisterCreatedObjectUndo(orchestrator, "Create DynamicEventOrchestrator");

            Debug.Log("[SceneSetup] Created DynamicEventOrchestrator");
        }

        private static void CreateWorldStateBlackboard()
        {
            GameObject existing = GameObject.Find("WorldStateBlackboard");
            if (existing != null)
            {
                Debug.Log("[SceneSetup] WorldStateBlackboard already exists");
                return;
            }

            GameObject blackboard = new GameObject("WorldStateBlackboard");
            blackboard.AddComponent<WorldStateBlackboard>();
            Undo.RegisterCreatedObjectUndo(blackboard, "Create WorldStateBlackboard");

            Debug.Log("[SceneSetup] Created WorldStateBlackboard");
        }

        private static void CreatePlayerSpawnPoint()
        {
            GameObject existing = GameObject.Find("PlayerSpawnPoint");
            if (existing != null)
            {
                Debug.Log("[SceneSetup] PlayerSpawnPoint already exists");
                return;
            }

            GameObject spawnPoint = new GameObject("PlayerSpawnPoint");
            spawnPoint.transform.position = new Vector3(0, 1, 0);
            Undo.RegisterCreatedObjectUndo(spawnPoint, "Create PlayerSpawnPoint");

            Debug.Log("[SceneSetup] Created PlayerSpawnPoint at (0, 1, 0)");
        }

        private static void CreateBasicLevelGeometry()
        {
            // Check if floor exists
            GameObject existingFloor = GameObject.Find("Floor");
            if (existingFloor == null)
            {
                GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
                floor.name = "Floor";
                floor.transform.position = Vector3.zero;
                floor.transform.localScale = new Vector3(2, 1, 2); // 20x20 units
                Undo.RegisterCreatedObjectUndo(floor, "Create Floor");

                Debug.Log("[SceneSetup] Created Floor");
            }

            // Check if directional light exists
            Light existingLight = FindObjectOfType<Light>();
            if (existingLight == null)
            {
                GameObject light = new GameObject("Directional Light");
                Light lightComponent = light.AddComponent<Light>();
                lightComponent.type = LightType.Directional;
                light.transform.rotation = Quaternion.Euler(50, -30, 0);
                Undo.RegisterCreatedObjectUndo(light, "Create Directional Light");

                Debug.Log("[SceneSetup] Created Directional Light");
            }
        }

        [MenuItem("Protocol EMR/Setup/Clear Scene")]
        public static void ClearScene()
        {
            if (!EditorUtility.DisplayDialog("Clear Scene",
                "This will remove all GameObjects from the scene (except camera). Continue?",
                "Yes", "Cancel"))
            {
                return;
            }

            // Find and destroy all root objects except the main camera
            GameObject[] allObjects = FindObjectsOfType<GameObject>();
            foreach (GameObject obj in allObjects)
            {
                if (obj.transform.parent == null && obj.name != "Main Camera")
                {
                    Undo.DestroyObjectImmediate(obj);
                }
            }

            Debug.Log("[SceneSetup] Scene cleared");
        }

        [MenuItem("Protocol EMR/Setup/Validate Scene")]
        public static void ValidateScene()
        {
            Debug.Log("=== SCENE VALIDATION ===");

            bool allGood = true;

            // Check GameManager
            if (FindObjectOfType<GameManager>() == null)
            {
                Debug.LogWarning("✗ GameManager not found");
                allGood = false;
            }
            else
            {
                Debug.Log("✓ GameManager found");
            }

            // Check MissionSystem
            if (FindObjectOfType<MissionSystem>() == null)
            {
                Debug.LogWarning("✗ MissionSystem not found");
                allGood = false;
            }
            else
            {
                Debug.Log("✓ MissionSystem found");
            }

            // Check PlaytestFlowController
            if (FindObjectOfType<PlaytestFlowController>() == null)
            {
                Debug.LogWarning("✗ PlaytestFlowController not found");
                allGood = false;
            }
            else
            {
                Debug.Log("✓ PlaytestFlowController found");
            }

            // Check ProceduralLevelBuilder
            if (FindObjectOfType<ProceduralLevelBuilder>() == null)
            {
                Debug.LogWarning("⚠ ProceduralLevelBuilder not found (optional)");
            }
            else
            {
                Debug.Log("✓ ProceduralLevelBuilder found");
            }

            // Check NPCSpawner
            if (FindObjectOfType<NPCSpawner>() == null)
            {
                Debug.LogWarning("⚠ NPCSpawner not found (optional)");
            }
            else
            {
                Debug.Log("✓ NPCSpawner found");
            }

            // Check DynamicEventOrchestrator
            if (FindObjectOfType<DynamicEventOrchestrator>() == null)
            {
                Debug.LogWarning("⚠ DynamicEventOrchestrator not found (optional)");
            }
            else
            {
                Debug.Log("✓ DynamicEventOrchestrator found");
            }

            // Check RegressionHarness
            if (FindObjectOfType<RegressionHarness>() == null)
            {
                Debug.LogWarning("⚠ RegressionHarness not found (optional)");
            }
            else
            {
                Debug.Log("✓ RegressionHarness found");
            }

            // Check for Main Camera
            Camera mainCamera = Camera.main;
            if (mainCamera == null)
            {
                Debug.LogWarning("✗ Main Camera not found");
                allGood = false;
            }
            else
            {
                Debug.Log("✓ Main Camera found");
            }

            // Check for PlayerSpawnPoint
            GameObject spawnPoint = GameObject.Find("PlayerSpawnPoint");
            if (spawnPoint == null)
            {
                Debug.LogWarning("⚠ PlayerSpawnPoint not found (recommended)");
            }
            else
            {
                Debug.Log("✓ PlayerSpawnPoint found");
            }

            if (allGood)
            {
                Debug.Log("=== SCENE VALIDATION PASSED ===");
                EditorUtility.DisplayDialog("Scene Validation", "All required systems are present!", "OK");
            }
            else
            {
                Debug.LogWarning("=== SCENE VALIDATION FAILED ===");
                EditorUtility.DisplayDialog("Scene Validation", "Some required systems are missing. Check console for details.", "OK");
            }
        }
    }
}
