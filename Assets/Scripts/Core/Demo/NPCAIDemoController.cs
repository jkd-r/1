using UnityEngine;
using UnityEngine.AI;
using ProtocolEMR.Core.AI;

namespace ProtocolEMR.Core.Demo
{
    /// <summary>
    /// Demo controller for NPC AI system testing and demonstration.
    /// Provides UI and controls for testing various NPC behaviors and features.
    /// </summary>
    public class NPCAIDemoController : MonoBehaviour
    {
        [Header("Demo Configuration")]
        [SerializeField] private bool enableDemoUI = true;
        [SerializeField] private bool autoSpawnNPCs = true;
        [SerializeField] private KeyCode toggleUIKey = KeyCode.F2;
        [SerializeField] private KeyCode spawnNPCKey = KeyCode.F3;
        [SerializeField] private KeyCode clearNPCsKey = KeyCode.F4;
        [SerializeField] private KeyCode triggerAlertKey = KeyCode.F5;
        
        [Header("Spawn Settings")]
        [SerializeField] private NPCType defaultNPCType = NPCType.Guard;
        [SerializeField] private int spawnRadius = 20;
        [SerializeField] private int maxDemoNPCs = 10;
        [SerializeField] private Transform spawnCenter;
        
        [Header("Test Scenarios")]
        [SerializeField] private bool enableTestScenarios = true;
        [SerializeField] private KeyCode scenario1Key = KeyCode.Alpha1;
        [SerializeField] private KeyCode scenario2Key = KeyCode.Alpha2;
        [SerializeField] private KeyCode scenario3Key = KeyCode.Alpha3;
        [SerializeField] private KeyCode scenario4Key = KeyCode.Alpha4;

        // Demo state
        private bool showUI = true;
        private int currentNPCCount = 0;
        private Vector3 lastSpawnPosition;
        private float lastAlertTime;

        // References
        private NPCManager npcManager;
        private NPCSpawner npcSpawner;
        private DifficultyManager difficultyManager;
        private GameObject player;

        private void Start()
        {
            InitializeDemo();
            
            if (autoSpawnNPCs)
            {
                SpawnInitialNPCs();
            }
        }

        private void Update()
        {
            HandleInput();
            UpdateDemoState();
        }

        /// <summary>
        /// Initializes the demo system.
        /// </summary>
        private void InitializeDemo()
        {
            // Find or create required managers
            npcManager = FindObjectOfType<NPCManager>();
            if (npcManager == null)
            {
                GameObject managerObj = new GameObject("NPC Manager");
                npcManager = managerObj.AddComponent<NPCManager>();
            }

            npcSpawner = FindObjectOfType<NPCSpawner>();
            if (npcSpawner == null)
            {
                GameObject spawnerObj = new GameObject("NPC Spawner");
                npcSpawner = spawnerObj.AddComponent<NPCSpawner>();
            }

            difficultyManager = DifficultyManager.Instance;
            if (difficultyManager == null)
            {
                GameObject difficultyObj = new GameObject("Difficulty Manager");
                difficultyManager = difficultyObj.AddComponent<DifficultyManager>();
            }

            // Find player
            player = GameObject.FindGameObjectWithTag("Player");
            if (player == null)
            {
                CreateDemoPlayer();
            }

            // Set spawn center
            if (spawnCenter == null)
            {
                spawnCenter = transform;
            }

            Debug.Log("NPC AI Demo Controller initialized");
        }

        /// <summary>
        /// Creates demo player if none exists.
        /// </summary>
        private void CreateDemoPlayer()
        {
            GameObject playerObj = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            playerObj.name = "Demo Player";
            playerObj.tag = "Player";
            playerObj.transform.position = spawnCenter.position;
            
            // Add player controller components
            playerObj.AddComponent<CharacterController>();
            playerObj.AddComponent<Rigidbody>().useGravity = true;
            
            // Add damageable component
            playerObj.AddComponent<DemoDamageable>();
            
            player = playerObj;
        }

        /// <summary>
        /// Spawns initial NPCs for demo.
        /// </summary>
        private void SpawnInitialNPCs()
        {
            // Spawn a few different NPC types
            SpawnNPCOfType(NPCType.Guard, 2);
            SpawnNPCOfType(NPCType.Scientist, 1);
            SpawnNPCOfType(NPCType.Civilian, 1);
        }

        /// <summary>
        /// Handles user input for demo controls.
        /// </summary>
        private void HandleInput()
        {
            // Toggle UI
            if (Input.GetKeyDown(toggleUIKey))
            {
                showUI = !showUI;
            }

            // Spawn NPC
            if (Input.GetKeyDown(spawnNPCKey))
            {
                SpawnRandomNPC();
            }

            // Clear NPCs
            if (Input.GetKeyDown(clearNPCsKey))
            {
                ClearAllNPCs();
            }

            // Trigger alert
            if (Input.GetKeyDown(triggerAlertKey))
            {
                TriggerGlobalAlert();
            }

            // Test scenarios
            if (enableTestScenarios)
            {
                if (Input.GetKeyDown(scenario1Key))
                {
                    RunScenario1_PatrolTest();
                }
                if (Input.GetKeyDown(scenario2Key))
                {
                    RunScenario2_CombatTest();
                }
                if (Input.GetKeyDown(scenario3Key))
                {
                    RunScenario3_FleeTest();
                }
                if (Input.GetKeyDown(scenario4Key))
                {
                    RunScenario4_GroupTest();
                }
            }
        }

        /// <summary>
        /// Updates demo state and statistics.
        /// </summary>
        private void UpdateDemoState()
        {
            currentNPCCount = npcManager != null ? npcManager.AllNPCs.Count : 0;
        }

        /// <summary>
        /// Spawns a random NPC.
        /// </summary>
        private void SpawnRandomNPC()
        {
            if (currentNPCCount >= maxDemoNPCs)
            {
                Debug.LogWarning("Maximum demo NPC limit reached");
                return;
            }

            Vector3 spawnPos = GetRandomSpawnPosition();
            CreateNPCAtPosition(defaultNPCType, spawnPos);
        }

        /// <summary>
        /// Spawns NPCs of specific type.
        /// </summary>
        private void SpawnNPCOfType(NPCType type, int count)
        {
            for (int i = 0; i < count; i++)
            {
                if (currentNPCCount >= maxDemoNPCs)
                    break;

                Vector3 spawnPos = GetRandomSpawnPosition();
                CreateNPCAtPosition(type, spawnPos);
            }
        }

        /// <summary>
        /// Creates NPC at specific position.
        /// </summary>
        private void CreateNPCAtPosition(NPCType type, Vector3 position)
        {
            GameObject npcObj = NPCPrefabConfiguration.CreateNPCPrefab(type, $"{type}_Demo_{currentNPCCount}");
            npcObj.transform.position = position;
            
            // Ensure NPC is on NavMesh
            NavMeshHit hit;
            if (NavMesh.SamplePosition(position, out hit, 2f, NavMesh.AllAreas))
            {
                npcObj.transform.position = hit.position;
            }
            else
            {
                Debug.LogWarning($"Could not place NPC on NavMesh at {position}");
                Destroy(npcObj);
                return;
            }

            // Register with NPC manager
            NPCController controller = npcObj.GetComponent<NPCController>();
            if (controller != null)
            {
                npcManager.RegisterNPC(controller);
            }

            lastSpawnPosition = position;
            Debug.Log($"Spawned {type} NPC at {position}");
        }

        /// <summary>
        /// Gets random spawn position within radius.
        /// </summary>
        private Vector3 GetRandomSpawnPosition()
        {
            Vector3 randomOffset = Random.insideUnitSphere * spawnRadius;
            randomOffset.y = 0; // Keep on ground level
            return spawnCenter.position + randomOffset;
        }

        /// <summary>
        /// Clears all spawned NPCs.
        /// </summary>
        private void ClearAllNPCs()
        {
            if (npcManager != null)
            {
                foreach (var npc in npcManager.AllNPCs)
                {
                    if (npc != null)
                    {
                        DestroyImmediate(npc.gameObject);
                    }
                }
            }

            currentNPCCount = 0;
            Debug.Log("Cleared all demo NPCs");
        }

        /// <summary>
        /// Triggers a global alert at player position.
        /// </summary>
        private void TriggerGlobalAlert()
        {
            if (npcManager == null || player == null)
                return;

            Vector3 alertPos = player.transform.position;
            npcManager.CreateGlobalAlert(alertPos, player, 100f, AlertType.AlarmTriggered);
            
            lastAlertTime = Time.time;
            Debug.Log($"Triggered global alert at {alertPos}");
        }

        // TEST SCENARIOS

        /// <summary>
        /// Scenario 1: Patrol behavior test.
        /// </summary>
        private void RunScenario1_PatrolTest()
        {
            Debug.Log("=== Running Scenario 1: Patrol Test ===");
            
            // Clear existing NPCs
            ClearAllNPCs();
            
            // Spawn guard NPCs with patrol behavior
            for (int i = 0; i < 3; i++)
            {
                Vector3 spawnPos = spawnCenter.position + new Vector3(i * 5f - 5f, 0, 10f);
                CreateNPCAtPosition(NPCType.Guard, spawnPos);
            }
            
            Debug.Log("Spawned 3 guard NPCs for patrol testing");
        }

        /// <summary>
        /// Scenario 2: Combat behavior test.
        /// </summary>
        private void RunScenario2_CombatTest()
        {
            Debug.Log("=== Running Scenario 2: Combat Test ===");
            
            // Clear existing NPCs
            ClearAllNPCs();
            
            // Spawn aggressive NPCs around player
            Vector3 playerPos = player != null ? player.transform.position : spawnCenter.position;
            
            for (int i = 0; i < 4; i++)
            {
                float angle = (360f / 4f) * i;
                Vector3 offset = Quaternion.Euler(0, angle, 0) * Vector3.forward * 8f;
                Vector3 spawnPos = playerPos + offset;
                CreateNPCAtPosition(NPCType.Guard, spawnPos);
            }
            
            // Trigger combat
            if (npcManager != null)
            {
                npcManager.CreateGlobalAlert(playerPos, player, 100f, AlertType.AttackDetected);
            }
            
            Debug.Log("Spawned 4 guard NPCs for combat testing");
        }

        /// <summary>
        /// Scenario 3: Flee behavior test.
        /// </summary>
        private void RunScenario3_FleeTest()
        {
            Debug.Log("=== Running Scenario 3: Flee Test ===");
            
            // Clear existing NPCs
            ClearAllNPCs();
            
            // Spawn civilian NPCs near player
            Vector3 playerPos = player != null ? player.transform.position : spawnCenter.position;
            
            for (int i = 0; i < 5; i++)
            {
                Vector3 spawnPos = playerPos + Random.insideUnitSphere * 5f;
                spawnPos.y = playerPos.y;
                CreateNPCAtPosition(NPCType.Civilian, spawnPos);
            }
            
            // NPCs should flee when player gets close
            Debug.Log("Spawned 5 civilian NPCs for flee testing");
        }

        /// <summary>
        /// Scenario 4: Group coordination test.
        /// </summary>
        private void RunScenario4_GroupTest()
        {
            Debug.Log("=== Running Scenario 4: Group Test ===");
            
            // Clear existing NPCs
            ClearAllNPCs();
            
            // Spawn group of guards
            Vector3 groupPos = spawnCenter.position + Vector3.forward * 15f;
            
            for (int i = 0; i < 6; i++)
            {
                Vector3 spawnPos = groupPos + Random.insideUnitSphere * 3f;
                spawnPos.y = groupPos.y;
                CreateNPCAtPosition(NPCType.Guard, spawnPos);
            }
            
            // Test group coordination
            if (npcManager != null && player != null)
            {
                npcManager.CreateGlobalAlert(player.transform.position, player, 100f, AlertType.VisualSighting);
            }
            
            Debug.Log("Spawned 6 guard NPCs for group coordination testing");
        }

        private void OnGUI()
        {
            if (!showUI || !enableDemoUI)
                return;

            GUILayout.BeginArea(new Rect(10, 10, 350, 500));
            GUILayout.BeginVertical("box");
            
            GUILayout.Label("NPC AI Demo Controller", GUI.skin.box);
            GUILayout.Space(10);
            
            // Statistics
            GUILayout.Label($"Active NPCs: {currentNPCCount}/{maxDemoNPCs}");
            GUILayout.Label($"Current Difficulty: {difficultyManager?.CurrentDifficulty ?? DifficultyLevel.Normal}");
            GUILayout.Label($"Player Performance: {difficultyManager?.PlayerPerformanceScore:F1}");
            GUILayout.Space(10);
            
            // Controls
            GUILayout.Label("Controls:");
            GUILayout.Label($"{toggleUIKey} - Toggle UI");
            GUILayout.Label($"{spawnNPCKey} - Spawn Random NPC");
            GUILayout.Label($"{clearNPCsKey} - Clear All NPCs");
            GUILayout.Label($"{triggerAlertKey} - Trigger Global Alert");
            GUILayout.Space(10);
            
            // NPC Type Selection
            GUILayout.Label("NPC Type to Spawn:");
            if (GUILayout.Button("Guard"))
            {
                defaultNPCType = NPCType.Guard;
                SpawnRandomNPC();
            }
            if (GUILayout.Button("Scientist"))
            {
                defaultNPCType = NPCType.Scientist;
                SpawnRandomNPC();
            }
            if (GUILayout.Button("Civilian"))
            {
                defaultNPCType = NPCType.Civilian;
                SpawnRandomNPC();
            }
            if (GUILayout.Button("Beast"))
            {
                defaultNPCType = NPCType.Beast;
                SpawnRandomNPC();
            }
            GUILayout.Space(10);
            
            // Test Scenarios
            if (enableTestScenarios)
            {
                GUILayout.Label("Test Scenarios:");
                if (GUILayout.Button($"{scenario1Key} - Patrol Test"))
                {
                    RunScenario1_PatrolTest();
                }
                if (GUILayout.Button($"{scenario2Key} - Combat Test"))
                {
                    RunScenario2_CombatTest();
                }
                if (GUILayout.Button($"{scenario3Key} - Flee Test"))
                {
                    RunScenario3_FleeTest();
                }
                if (GUILayout.Button($"{scenario4Key} - Group Test"))
                {
                    RunScenario4_GroupTest();
                }
                GUILayout.Space(10);
            }
            
            // Difficulty Settings
            GUILayout.Label("Difficulty:");
            if (GUILayout.Button("Easy"))
            {
                difficultyManager?.SetDifficulty(DifficultyLevel.Easy);
            }
            if (GUILayout.Button("Normal"))
            {
                difficultyManager?.SetDifficulty(DifficultyLevel.Normal);
            }
            if (GUILayout.Button("Hard"))
            {
                difficultyManager?.SetDifficulty(DifficultyLevel.Hard);
            }
            if (GUILayout.Button("Nightmare"))
            {
                difficultyManager?.SetDifficulty(DifficultyLevel.Nightmare);
            }
            
            GUILayout.EndVertical();
            GUILayout.EndArea();
        }

        private void OnDrawGizmos()
        {
            // Draw spawn radius
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(spawnCenter != null ? spawnCenter.position : transform.position, spawnRadius);
            
            // Draw last spawn position
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(lastSpawnPosition, 1f);
        }
    }

    /// <summary>
    /// Demo damageable component for testing combat.
    /// </summary>
    public class DemoDamageable : MonoBehaviour, IDamageable
    {
        [SerializeField] private float health = 100f;
        [SerializeField] private float maxHealth = 100f;

        public float Health => health;
        public float MaxHealth => maxHealth;

        public void TakeDamage(float damage)
        {
            health -= damage;
            health = Mathf.Max(0f, health);
            
            Debug.Log($"Demo player took {damage} damage, health: {health}/{maxHealth}");
            
            if (health <= 0)
            {
                Die();
            }
        }

        private void Die()
        {
            Debug.Log("Demo player died");
            // Respawn after delay
            Invoke(nameof(Respawn), 3f);
        }

        private void Respawn()
        {
            health = maxHealth;
            transform.position = Vector3.zero;
            Debug.Log("Demo player respawned");
        }
    }
}