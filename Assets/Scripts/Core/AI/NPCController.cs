using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

namespace ProtocolEMR.Core.AI
{
    /// <summary>
    /// Main NPC controller that manages all AI systems including behavior trees, perception, navigation, combat, and animations.
    /// </summary>
    [RequireComponent(typeof(NavMeshAgent))]
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(Rigidbody))]
    public class NPCController : MonoBehaviour, IDamageable
    {
        [Header("NPC Configuration")]
        [SerializeField] private NPCType npcType = NPCType.Guard;
        [SerializeField] private NPCParameters parameters = new NPCParameters();
        [SerializeField] private bool proceduralGeneration = true;
        [SerializeField] private int proceduralSeed = 0;
        
        [Header("Behavior Tree")]
        [SerializeField] private bool enableBehaviorTree = true;
        [SerializeField] private float behaviorUpdateInterval = 0.1f;
        
        [Header("Patrol Settings")]
        [SerializeField] private Vector3[] patrolPoints = new Vector3[0];
        [SerializeField] private bool autoGeneratePatrol = false;
        [SerializeField] private float patrolRadius = 10.0f;
        
        [Header("Debug")]
        [SerializeField] private bool drawDebugGizmos = true;
        [SerializeField] private bool showPerception = true;
        [SerializeField] private bool showNavigation = true;
        [SerializeField] private bool showCombat = true;

        // Core Systems
        private NPCPerception perception;
        private NPCNavigation navigation;
        private NPCCombat combat;
        private NPCAnimationController animationController;
        private BehaviorNode behaviorTreeRoot;
        
        // State Management
        private NPCState currentState = NPCState.Idle;
        private float currentHealth;
        private float currentStamina;
        
        // Timers
        private float lastBehaviorUpdate;
        private float lastStateChange;
        private NPCState previousState;

        // Properties
        public NPCType Type => npcType;
        public NPCParameters Parameters => parameters;
        public NPCState CurrentState => currentState;
        public NPCState PreviousState => previousState;
        public float CurrentHealth => currentHealth;
        public float CurrentStamina => currentStamina;
        public NPCPerception Perception => perception;
        public NPCNavigation Navigation => navigation;
        public NPCCombat Combat => combat;
        public NPCAnimationController Animator => animationController;
        public BehaviorNode BehaviorTreeRoot => behaviorTreeRoot;

        // Events
        public System.Action<NPCState, NPCState> OnStateChanged;
        public System.Action<float, float> OnHealthChanged;
        public System.Action<float> OnDied;

        private void Awake()
        {
            InitializeComponents();
            GenerateProceduralParameters();
            InitializeSystems();
            CreateBehaviorTree();
        }

        private void Start()
        {
            ResetNPC();
            
            if (autoGeneratePatrol && patrolPoints.Length == 0)
            {
                GeneratePatrolPoints();
            }
            
            navigation.SetPatrolPoints(patrolPoints);
        }

        private void Update()
        {
            if (currentState == NPCState.Dead)
                return;

            UpdateSystems();
            UpdateBehaviorTree();
            UpdateStamina();
        }

        private void FixedUpdate()
        {
            if (currentState == NPCState.Dead)
                return;

            UpdatePhysics();
        }

        /// <summary>
        /// Initializes required components.
        /// </summary>
        private void InitializeComponents()
        {
            // Ensure NavMeshAgent is properly configured
            NavMeshAgent agent = GetComponent<NavMeshAgent>();
            if (agent == null)
            {
                agent = gameObject.AddComponent<NavMeshAgent>();
            }

            // Ensure Animator is present
            Animator animator = GetComponent<Animator>();
            if (animator == null)
            {
                animator = gameObject.AddComponent<Animator>();
            }

            // Ensure Rigidbody is present and configured
            Rigidbody rigidbody = GetComponent<Rigidbody>();
            if (rigidbody == null)
            {
                rigidbody = gameObject.AddComponent<Rigidbody>();
            }
            
            rigidbody.useGravity = true;
            rigidbody.isKinematic = false;
            rigidbody.constraints = RigidbodyConstraints.FreezeRotation;
        }

        /// <summary>
        /// Generates procedural parameters based on NPC type and seed.
        /// </summary>
        private void GenerateProceduralParameters()
        {
            if (!proceduralGeneration)
                return;

            Random.InitState(proceduralSeed + GetInstanceID());

            // Base parameters by NPC type
            switch (npcType)
            {
                case NPCType.Guard:
                    parameters.maxHealth = Random.Range(60f, 80f);
                    parameters.walkSpeed = Random.Range(3.5f, 4.5f);
                    parameters.runSpeed = Random.Range(7f, 9f);
                    parameters.sprintSpeed = Random.Range(11f, 13f);
                    parameters.damagePerHit = Random.Range(15f, 20f);
                    parameters.perceptionRange = Random.Range(15f, 20f);
                    parameters.aggression = Random.Range(70f, 90f);
                    parameters.intelligence = Random.Range(50f, 70f);
                    parameters.morale = Random.Range(70f, 85f);
                    break;

                case NPCType.Scientist:
                    parameters.maxHealth = Random.Range(30f, 50f);
                    parameters.walkSpeed = Random.Range(3f, 4f);
                    parameters.runSpeed = Random.Range(6f, 8f);
                    parameters.sprintSpeed = Random.Range(9f, 11f);
                    parameters.damagePerHit = Random.Range(5f, 10f);
                    parameters.perceptionRange = Random.Range(10f, 15f);
                    parameters.aggression = Random.Range(10f, 30f);
                    parameters.intelligence = Random.Range(80f, 100f);
                    parameters.morale = Random.Range(30f, 50f);
                    break;

                case NPCType.Civilian:
                    parameters.maxHealth = Random.Range(40f, 60f);
                    parameters.walkSpeed = Random.Range(3f, 4f);
                    parameters.runSpeed = Random.Range(6f, 8f);
                    parameters.sprintSpeed = Random.Range(10f, 12f);
                    parameters.damagePerHit = Random.Range(8f, 12f);
                    parameters.perceptionRange = Random.Range(12f, 18f);
                    parameters.aggression = Random.Range(20f, 40f);
                    parameters.intelligence = Random.Range(40f, 60f);
                    parameters.morale = Random.Range(40f, 60f);
                    break;

                case NPCType.Beast:
                    parameters.maxHealth = Random.Range(70f, 100f);
                    parameters.walkSpeed = Random.Range(4f, 5f);
                    parameters.runSpeed = Random.Range(9f, 11f);
                    parameters.sprintSpeed = Random.Range(13f, 15f);
                    parameters.damagePerHit = Random.Range(20f, 30f);
                    parameters.perceptionRange = Random.Range(18f, 25f);
                    parameters.aggression = Random.Range(80f, 100f);
                    parameters.intelligence = Random.Range(20f, 40f);
                    parameters.morale = Random.Range(85f, 100f);
                    break;
            }

            // Apply procedural variations
            parameters.maxHealth *= Random.Range(0.8f, 1.2f);
            parameters.walkSpeed *= Random.Range(0.85f, 1.15f);
            parameters.runSpeed *= Random.Range(0.85f, 1.15f);
            parameters.sprintSpeed *= Random.Range(0.85f, 1.15f);
            parameters.perceptionRange *= Random.Range(0.9f, 1.1f);
            parameters.reactionTime = Random.Range(0.3f, 0.8f);
        }

        /// <summary>
        /// Initializes all AI systems.
        /// </summary>
        private void InitializeSystems()
        {
            NavMeshAgent agent = GetComponent<NavMeshAgent>();
            Animator animator = GetComponent<Animator>();

            perception = new NPCPerception(this);
            navigation = new NPCNavigation(this, agent);
            combat = new NPCCombat(this);
            animationController = new NPCAnimationController(this, animator);

            currentHealth = parameters.maxHealth;
            currentStamina = parameters.maxStamina;
        }

        /// <summary>
        /// Creates the behavior tree based on NPC type.
        /// </summary>
        private void CreateBehaviorTree()
        {
            if (!enableBehaviorTree)
                return;

            switch (npcType)
            {
                case NPCType.Guard:
                    behaviorTreeRoot = CreateGuardBehaviorTree();
                    break;
                case NPCType.Scientist:
                    behaviorTreeRoot = CreateScientistBehaviorTree();
                    break;
                case NPCType.Civilian:
                    behaviorTreeRoot = CreateCivilianBehaviorTree();
                    break;
                case NPCType.Beast:
                    behaviorTreeRoot = CreateBeastBehaviorTree();
                    break;
                default:
                    behaviorTreeRoot = CreateDefaultBehaviorTree();
                    break;
            }
        }

        /// <summary>
        /// Creates behavior tree for Guard NPCs.
        /// </summary>
        private BehaviorNode CreateGuardBehaviorTree()
        {
            return new SelectorNode(this,
                // High priority: Stun/Death states
                new SequenceNode(this,
                    new IsStunnedCondition(this),
                    new PlayStunAction(this)
                ),
                new SequenceNode(this,
                    new IsDeadCondition(this),
                    new PlayDeathAction(this)
                ),
                
                // Medium priority: Combat behaviors
                new SelectorNode(this,
                    new SequenceNode(this,
                        new CanSeePlayerCondition(this),
                        new IsPlayerInRangeCondition(this, 2.0f),
                        new ChaseAndAttackAction(this)
                    ),
                    new SequenceNode(this,
                        new HeardPlayerCondition(this),
                        new InvestigateSoundAction(this)
                    )
                ),
                
                // Low priority: Movement behaviors
                new SelectorNode(this,
                    new SequenceNode(this,
                        new HealthLowCondition(this, 0.25f),
                        new FleeToSafezoneAction(this)
                    ),
                    new SequenceNode(this,
                        new AtPatrolPointCondition(this),
                        new WaitAtPointAction(this, 3.0f)
                    ),
                    new PatrolAction(this)
                )
            );
        }

        /// <summary>
        /// Creates behavior tree for Scientist NPCs.
        /// </summary>
        private BehaviorNode CreateScientistBehaviorTree()
        {
            return new SelectorNode(this,
                new SequenceNode(this,
                    new IsStunnedCondition(this),
                    new PlayStunAction(this)
                ),
                new SequenceNode(this,
                    new IsDeadCondition(this),
                    new PlayDeathAction(this)
                ),
                new SelectorNode(this,
                    new SequenceNode(this,
                        new HealthLowCondition(this, 0.5f),
                        new FleeToSafezoneAction(this)
                    ),
                    new SequenceNode(this,
                        new HasCoverNearbyCondition(this),
                        new HideBehindCoverAction(this)
                    ),
                    new SequenceNode(this,
                        new AtPatrolPointCondition(this),
                        new WaitAtPointAction(this, 5.0f)
                    ),
                    new PatrolAction(this)
                )
            );
        }

        /// <summary>
        /// Creates behavior tree for Civilian NPCs.
        /// </summary>
        private BehaviorNode CreateCivilianBehaviorTree()
        {
            return new SelectorNode(this,
                new SequenceNode(this,
                    new IsStunnedCondition(this),
                    new PlayStunAction(this)
                ),
                new SequenceNode(this,
                    new IsDeadCondition(this),
                    new PlayDeathAction(this)
                ),
                new SelectorNode(this,
                    new SequenceNode(this,
                        new HealthLowCondition(this, 0.75f),
                        new FleeToSafezoneAction(this)
                    ),
                    new SequenceNode(this,
                        new AtPatrolPointCondition(this),
                        new WaitAtPointAction(this, 4.0f)
                    ),
                    new PatrolAction(this)
                )
            );
        }

        /// <summary>
        /// Creates behavior tree for Beast NPCs.
        /// </summary>
        private BehaviorNode CreateBeastBehaviorTree()
        {
            return new SelectorNode(this,
                new SequenceNode(this,
                    new IsStunnedCondition(this),
                    new PlayStunAction(this)
                ),
                new SequenceNode(this,
                    new IsDeadCondition(this),
                    new PlayDeathAction(this)
                ),
                new SelectorNode(this,
                    new SequenceNode(this,
                        new CanSeePlayerCondition(this),
                        new ChaseAndAttackAction(this)
                    ),
                    new SequenceNode(this,
                        new HealthLowCondition(this, 0.2f),
                        new FleeToSafezoneAction(this)
                    ),
                    new PatrolAction(this)
                )
            );
        }

        /// <summary>
        /// Creates default behavior tree.
        /// </summary>
        private BehaviorNode CreateDefaultBehaviorTree()
        {
            return CreateGuardBehaviorTree();
        }

        /// <summary>
        /// Updates all AI systems.
        /// </summary>
        private void UpdateSystems()
        {
            perception?.UpdatePerception();
            navigation?.UpdateNavigation();
            combat?.UpdateCombat();
            animationController?.UpdateAnimations();
        }

        /// <summary>
        /// Updates behavior tree execution.
        /// </summary>
        private void UpdateBehaviorTree()
        {
            if (behaviorTreeRoot == null || Time.time - lastBehaviorUpdate < behaviorUpdateInterval)
                return;

            lastBehaviorUpdate = Time.time;
            behaviorTreeRoot.Execute();
        }

        /// <summary>
        /// Updates stamina system.
        /// </summary>
        private void UpdateStamina()
        {
            // Drain stamina when sprinting
            if (navigation != null && navigation.Data.hasPath && animationController != null && animationController.Data.isRunning)
            {
                currentStamina -= parameters.staminaDrainRate * Time.deltaTime;
                currentStamina = Mathf.Max(0f, currentStamina);
            }
            // Recover stamina when not sprinting
            else
            {
                currentStamina += parameters.staminaRecoveryRate * Time.deltaTime;
                currentStamina = Mathf.Min(parameters.maxStamina, currentStamina);
            }
        }

        /// <summary>
        /// Updates physics-based movement.
        /// </summary>
        private void UpdatePhysics()
        {
            // Apply movement forces if needed
            Rigidbody rigidbody = GetComponent<Rigidbody>();
            if (rigidbody != null && navigation != null && !navigation.Data.hasPath)
            {
                // Apply friction when not pathfinding
                Vector3 horizontalVelocity = new Vector3(rigidbody.velocity.x, 0f, rigidbody.velocity.z);
                rigidbody.AddForce(-horizontalVelocity * 5f, ForceMode.Acceleration);
            }
        }

        /// <summary>
        /// Sets NPC state and triggers state change events.
        /// </summary>
        public void SetState(NPCState newState)
        {
            if (currentState == newState)
                return;

            previousState = currentState;
            currentState = newState;
            lastStateChange = Time.time;

            OnStateChanged?.Invoke(previousState, currentState);
            Debug.Log($"{name} state changed from {previousState} to {newState}");
        }

        /// <summary>
        /// Moves NPC to target position.
        /// </summary>
        public void MoveToTarget(float speed)
        {
            if (navigation != null)
            {
                navigation.MoveToTarget(speed);
            }
        }

        /// <summary>
        /// Makes NPC attack.
        /// </summary>
        public void Attack()
        {
            if (combat != null)
            {
                combat.Attack();
            }
        }

        /// <summary>
        /// Makes NPC die.
        /// </summary>
        public void Die()
        {
            SetState(NPCState.Dead);
            
            if (navigation != null)
            {
                navigation.StopMovement();
            }
            
            if (animationController != null)
            {
                animationController.PlayDeathAnimation();
            }

            // Schedule removal after death animation
            Invoke(nameof(RemoveNPC), 5.0f);

            OnDied?.Invoke(currentHealth);
            Debug.Log($"{name} died");
        }

        /// <summary>
        /// Removes NPC from scene.
        /// </summary>
        private void RemoveNPC()
        {
            Destroy(gameObject);
        }

        /// <summary>
        /// Resets NPC to initial state.
        /// </summary>
        public void ResetNPC()
        {
            currentHealth = parameters.maxHealth;
            currentStamina = parameters.maxStamina;
            SetState(NPCState.Idle);
            
            if (behaviorTreeRoot != null)
            {
                behaviorTreeRoot.Reset();
            }
        }

        /// <summary>
        /// Generates patrol points automatically.
        /// </summary>
        private void GeneratePatrolPoints()
        {
            List<Vector3> points = new List<Vector3>();
            int pointCount = Random.Range(3, 6);

            for (int i = 0; i < pointCount; i++)
            {
                Vector3 randomPoint = transform.position + Random.insideUnitSphere * patrolRadius;
                randomPoint.y = transform.position.y;
                
                NavMeshHit hit;
                if (NavMesh.SamplePosition(randomPoint, out hit, patrolRadius, NavMesh.AllAreas))
                {
                    points.Add(hit.position);
                }
            }

            patrolPoints = points.ToArray();
        }

        /// <summary>
        /// Takes damage from external source.
        /// </summary>
        public void TakeDamage(float damage)
        {
            Vector3 damageDirection = Vector3.zero;
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                damageDirection = (transform.position - player.transform.position).normalized;
            }

            TakeDamage(damage, damageDirection);
        }

        /// <summary>
        /// Takes damage with direction.
        /// </summary>
        public void TakeDamage(float damage, Vector3 damageDirection)
        {
            if (currentState == NPCState.Dead)
                return;

            if (combat != null)
            {
                combat.TakeDamage(damage, damageDirection);
            }

            OnHealthChanged?.Invoke(currentHealth, parameters.maxHealth);
        }

        /// <summary>
        /// Registers a sound for NPC to hear.
        /// </summary>
        public void RegisterSound(Vector3 soundPosition, float soundIntensity)
        {
            perception?.RegisterSound(soundPosition, soundIntensity);
        }

        /// <summary>
        /// Gets difficulty multiplier for stats.
        /// </summary>
        public float GetDifficultyMultiplier()
        {
            // This would be tied to global difficulty settings
            return 1.0f;
        }

        private void OnDrawGizmos()
        {
            if (!drawDebugGizmos)
                return;

            if (showPerception && perception != null)
            {
                perception.DrawDebugGizmos();
            }

            if (showNavigation && navigation != null)
            {
                navigation.DrawDebugGizmos();
            }

            if (showCombat && combat != null)
            {
                combat.DrawDebugGizmos();
            }

            if (animationController != null)
            {
                animationController.DrawDebugGizmos();
            }
        }

        private void OnValidate()
        {
            // Ensure parameters are valid
            parameters.maxHealth = Mathf.Max(1f, parameters.maxHealth);
            parameters.walkSpeed = Mathf.Max(0.1f, parameters.walkSpeed);
            parameters.runSpeed = Mathf.Max(parameters.walkSpeed, parameters.runSpeed);
            parameters.sprintSpeed = Mathf.Max(parameters.runSpeed, parameters.sprintSpeed);
            parameters.perceptionRange = Mathf.Max(1f, parameters.perceptionRange);
            parameters.fieldOfView = Mathf.Clamp(parameters.fieldOfView, 1f, 360f);
        }
    }
}