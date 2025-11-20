using UnityEngine;
using UnityEditor;
using ProtocolEMR.Core.AI;

namespace ProtocolEMR.Core.Editor
{
    /// <summary>
    /// Editor utility for creating NPC prefabs and test scenes.
    /// </summary>
    public class NPCAIEditorTools
    {
        [MenuItem("Protocol EMR/AI/Create NPC Prefabs")]
        public static void CreateNPCPrefabs()
        {
            CreateNPCPrefab(NPCType.Guard, "Guard_NPC");
            CreateNPCPrefab(NPCType.Scientist, "Scientist_NPC");
            CreateNPCPrefab(NPCType.Civilian, "Civilian_NPC");
            CreateNPCPrefab(NPCType.Beast, "Beast_NPC");
            
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            Debug.Log("NPC prefabs created successfully");
        }

        [MenuItem("Protocol EMR/AI/Create Test Scene")]
        public static void CreateTestScene()
        {
            // Create new scene
            SceneManagement.SceneManager.NewScene(SceneManagement.LoadSceneMode.Single);
            
            // Create basic environment
            CreateTestEnvironment();
            
            // Create demo controller
            CreateDemoController();
            
            // Create player
            CreateTestPlayer();
            
            // Bake NavMesh
            BakeTestNavMesh();
            
            Debug.Log("NPC AI test scene created successfully");
        }

        private static void CreateNPCPrefab(NPCType npcType, string prefabName)
        {
            GameObject npcObject = NPCPrefabConfiguration.CreateNPCPrefab(npcType, prefabName);
            
            // Add additional components for better demo
            AddDemoComponents(npcObject, npcType);
            
            // Create prefab
            string prefabPath = $"Assets/Prefabs/NPCs/{prefabName}.prefab";
            System.IO.Directory.CreateDirectory("Assets/Prefabs/NPCs/");
            PrefabUtility.SaveAsPrefabAsset(npcObject, prefabPath);
            
            // Destroy temporary object
            Object.DestroyImmediate(npcObject);
        }

        private static void AddDemoComponents(GameObject npcObject, NPCType npcType)
        {
            // Add capsule collider for physics
            CapsuleCollider collider = npcObject.GetComponent<CapsuleCollider>();
            if (collider == null)
            {
                collider = npcObject.AddComponent<CapsuleCollider>();
                collider.center = Vector3.up * 0.5f;
                collider.height = 2f;
                collider.radius = 0.5f;
            }

            // Configure NavMeshAgent
            NavMeshAgent agent = npcObject.GetComponent<NavMeshAgent>();
            if (agent != null)
            {
                agent.radius = 0.3f;
                agent.height = 1.8f;
                agent.baseOffset = 0.9f;
            }

            // Add Animator if missing
            Animator animator = npcObject.GetComponent<Animator>();
            if (animator == null)
            {
                animator = npcObject.AddComponent<Animator>();
                
                // Create simple animator controller
                AnimatorController controller = CreateSimpleAnimatorController(npcType);
                animator.runtimeAnimatorController = controller;
            }
        }

        private static AnimatorController CreateSimpleAnimatorController(NPCType npcType)
        {
            // Create basic animator controller with states
            AnimatorController controller = AnimatorController.CreateAnimatorController($"Assets/Animators/{npcType}_Controller.controller");
            
            // Add parameters
            controller.AddParameter("Speed", AnimatorControllerParameterType.Float);
            controller.AddParameter("IsAttacking", AnimatorControllerParameterType.Bool);
            controller.AddParameter("IsDead", AnimatorControllerParameterType.Bool);
            controller.AddParameter("Trigger", AnimatorControllerParameterType.Trigger);
            
            return controller;
        }

        private static void CreateTestEnvironment()
        {
            // Create ground plane
            GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Ground";
            ground.transform.localScale = new Vector3(10f, 1f, 10f);
            
            // Create some obstacles
            CreateObstacle(new Vector3(5f, 0f, 5f), new Vector3(2f, 3f, 2f));
            CreateObstacle(new Vector3(-5f, 0f, 5f), new Vector3(2f, 3f, 2f));
            CreateObstacle(new Vector3(0f, 0f, 10f), new Vector3(3f, 3f, 2f));
            CreateObstacle(new Vector3(8f, 0f, -3f), new Vector3(2f, 3f, 2f));
            CreateObstacle(new Vector3(-8f, 0f, -3f), new Vector3(2f, 3f, 2f));
            
            // Create walls for enclosure
            CreateWall(new Vector3(0f, 1.5f, 15f), new Vector3(20f, 3f, 0.5f));
            CreateWall(new Vector3(0f, 1.5f, -15f), new Vector3(20f, 3f, 0.5f));
            CreateWall(new Vector3(15f, 1.5f, 0f), new Vector3(0.5f, 3f, 30f));
            CreateWall(new Vector3(-15f, 1.5f, 0f), new Vector3(0.5f, 3f, 30f));
        }

        private static void CreateObstacle(Vector3 position, Vector3 scale)
        {
            GameObject obstacle = GameObject.CreatePrimitive(PrimitiveType.Cube);
            obstacle.name = "Obstacle";
            obstacle.transform.position = position;
            obstacle.transform.localScale = scale;
            obstacle.layer = LayerMask.NameToLayer("Environment");
        }

        private static void CreateWall(Vector3 position, Vector3 scale)
        {
            GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.name = "Wall";
            wall.transform.position = position;
            wall.transform.localScale = scale;
            wall.layer = LayerMask.NameToLayer("Environment");
        }

        private static void CreateDemoController()
        {
            GameObject demoController = new GameObject("NPC AI Demo Controller");
            demoController.AddComponent<Demo.NPCAIDemoController>();
        }

        private static void CreateTestPlayer()
        {
            GameObject player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            player.name = "Player";
            player.tag = "Player";
            player.transform.position = Vector3.up * 0.5f;
            player.transform.localScale = new Vector3(0.5f, 1f, 0.5f);
            
            // Add player components
            CharacterController characterController = player.AddComponent<CharacterController>();
            characterController.radius = 0.3f;
            characterController.height = 1.8f;
            characterController.center = Vector3.up * 0.9f;
            
            Rigidbody rigidbody = player.AddComponent<Rigidbody>();
            rigidbody.useGravity = true;
            rigidbody.isKinematic = false;
            rigidbody.constraints = RigidbodyConstraints.FreezeRotation;
            
            // Add demo damageable
            player.AddComponent<Demo.DemoDamageable>();
            
            // Add basic movement for testing
            player.AddComponent<TestPlayerMovement>();
        }

        private static void BakeTestNavMesh()
        {
            // This would normally open the NavMesh baking window
            // For now, we'll just create a NavMesh surface component
            GameObject navMeshSurface = new GameObject("NavMesh Surface");
            navMeshSurface.AddComponent<UnityEngine.AI.NavMeshSurface>();
            
            Debug.Log("NavMesh surface created. Please bake manually in Window -> AI -> Navigation.");
        }

        [MenuItem("Protocol EMR/AI/Bake Test NavMesh")]
        public static void BakeTestNavMeshManually()
        {
            // Find NavMeshSurface component
            UnityEngine.AI.NavMeshSurface surface = Object.FindObjectOfType<UnityEngine.AI.NavMeshSurface>();
            if (surface != null)
            {
                surface.BuildNavMesh();
                Debug.Log("NavMesh baked successfully");
            }
            else
            {
                Debug.LogWarning("No NavMeshSurface found. Create test scene first.");
            }
        }
    }

    /// <summary>
    /// Simple test player movement for demo purposes.
    /// </summary>
    public class TestPlayerMovement : MonoBehaviour
    {
        public float moveSpeed = 5f;
        public float mouseSensitivity = 2f;
        public float jumpSpeed = 5f;
        
        private CharacterController characterController;
        private Camera playerCamera;
        private float verticalRotation = 0f;
        private Vector3 velocity;

        private void Start()
        {
            characterController = GetComponent<CharacterController>();
            
            // Create camera
            GameObject cameraObj = new GameObject("Player Camera");
            cameraObj.transform.SetParent(transform);
            cameraObj.transform.localPosition = Vector3.up * 0.8f;
            playerCamera = cameraObj.AddComponent<Camera>();
            cameraObj.AddComponent<AudioListener>();
            
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void Update()
        {
            // Mouse look
            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;
            
            transform.Rotate(Vector3.up * mouseX);
            verticalRotation -= mouseY;
            verticalRotation = Mathf.Clamp(verticalRotation, -90f, 90f);
            playerCamera.transform.localRotation = Quaternion.Euler(verticalRotation, 0f, 0f);
            
            // Movement
            float moveX = Input.GetAxis("Horizontal");
            float moveZ = Input.GetAxis("Vertical");
            
            Vector3 moveDirection = transform.right * moveX + transform.forward * moveZ;
            characterController.Move(moveDirection * moveSpeed * Time.deltaTime);
            
            // Jumping
            if (characterController.isGrounded && Input.GetButtonDown("Jump"))
            {
                velocity.y = jumpSpeed;
            }
            
            // Gravity
            velocity.y += Physics.gravity.y * Time.deltaTime;
            characterController.Move(velocity * Time.deltaTime);
        }
    }
}