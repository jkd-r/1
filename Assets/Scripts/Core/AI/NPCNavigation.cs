using UnityEngine;
using UnityEngine.AI;

namespace ProtocolEMR.Core.AI
{
    /// <summary>
    /// Navigation and pathfinding system for NPCs.
    /// </summary>
    [System.Serializable]
    public class NPCNavigation
    {
        private NPCController npc;
        private NavMeshAgent navMeshAgent;
        private NavigationData navigationData;
        
        [Header("Navigation Settings")]
        [SerializeField] private float pathRecalculationInterval = 0.5f;
        [SerializeField] private float stuckThreshold = 0.1f;
        [SerializeField] private float stuckTimeThreshold = 3.0f;
        [SerializeField] private float teleportDistance = 2.0f;
        
        private float lastPathRecalculationTime;
        private Vector3 lastPosition;
        private float timeWithoutMovement;

        public NavigationData Data => navigationData;
        public bool HasPath => navigationData.hasPath;
        public bool IsStuck => navigationData.isStuck;
        public Vector3 TargetPosition
        {
            get => navigationData.targetPosition;
            set => SetTargetPosition(value);
        }

        public NPCNavigation(NPCController controller, NavMeshAgent agent)
        {
            npc = controller;
            navMeshAgent = agent;
            navigationData = new NavigationData();
            InitializeNavMeshAgent();
        }

        /// <summary>
        /// Initializes NavMeshAgent with NPC parameters.
        /// </summary>
        private void InitializeNavMeshAgent()
        {
            if (navMeshAgent == null) return;

            navMeshAgent.speed = npc.Parameters.walkSpeed;
            navMeshAgent.acceleration = 8.0f;
            navMeshAgent.angularSpeed = 120.0f;
            navMeshAgent.stoppingDistance = 0.5f;
            navMeshAgent.autoBraking = true;
            navMeshAgent.obstacleAvoidanceType = ObstacleAvoidanceType.LowQualityObstacleAvoidance;
            navMeshAgent.avoidancePriority = Random.Range(1, 99);
        }

        /// <summary>
        /// Updates navigation system including pathfinding and stuck detection.
        /// </summary>
        public void UpdateNavigation()
        {
            if (navMeshAgent == null || !navMeshAgent.enabled) return;

            UpdatePathRecalculation();
            UpdateStuckDetection();
            UpdateNavigationData();
        }

        /// <summary>
        /// Recalculates path at specified intervals.
        /// </summary>
        private void UpdatePathRecalculation()
        {
            if (navigationData.hasPath && Time.time - lastPathRecalculationTime > pathRecalculationInterval)
            {
                RecalculatePath();
                lastPathRecalculationTime = Time.time;
            }
        }

        /// <summary>
        /// Detects if NPC is stuck and handles recovery.
        /// </summary>
        private void UpdateStuckDetection()
        {
            Vector3 currentPosition = npc.transform.position;
            float movementDistance = Vector3.Distance(currentPosition, lastPosition);
            
            if (movementDistance < stuckThreshold)
            {
                timeWithoutMovement += Time.deltaTime;
                
                if (timeWithoutMovement > stuckTimeThreshold)
                {
                    navigationData.isStuck = true;
                    HandleStuckSituation();
                }
            }
            else
            {
                timeWithoutMovement = 0f;
                navigationData.isStuck = false;
            }
            
            lastPosition = currentPosition;
        }

        /// <summary>
        /// Updates navigation data for external access.
        /// </summary>
        private void UpdateNavigationData()
        {
            navigationData.hasPath = navMeshAgent.hasPath && navMeshAgent.remainingDistance > navMeshAgent.stoppingDistance;
        }

        /// <summary>
        /// Sets target position for navigation.
        /// </summary>
        public void SetTargetPosition(Vector3 target)
        {
            navigationData.targetPosition = target;
            RecalculatePath();
        }

        /// <summary>
        /// Recalculates path to current target.
        /// </summary>
        private void RecalculatePath()
        {
            if (navMeshAgent == null || !navMeshAgent.isOnNavMesh) return;

            NavMeshPath path = new NavMeshPath();
            if (NavMesh.CalculatePath(npc.transform.position, navigationData.targetPosition, NavMesh.AllAreas, path))
            {
                navMeshAgent.SetPath(path);
                navigationData.hasPath = true;
            }
            else
            {
                navigationData.hasPath = false;
            }
        }

        /// <summary>
        /// Handles stuck situations with recovery strategies.
        /// </summary>
        private void HandleStuckSituation()
        {
            // Strategy 1: Try to teleport slightly forward
            Vector3 forwardDirection = npc.transform.forward;
            Vector3 teleportPosition = npc.transform.position + forwardDirection * teleportDistance;
            
            NavMeshHit hit;
            if (NavMesh.SamplePosition(teleportPosition, out hit, teleportDistance, NavMesh.AllAreas))
            {
                npc.transform.position = hit.position;
                timeWithoutMovement = 0f;
                return;
            }

            // Strategy 2: Try random direction
            for (int i = 0; i < 8; i++)
            {
                float randomAngle = Random.Range(0f, 360f);
                Vector3 randomDirection = Quaternion.Euler(0, randomAngle, 0) * Vector3.forward;
                Vector3 randomPosition = npc.transform.position + randomDirection * teleportDistance;
                
                if (NavMesh.SamplePosition(randomPosition, out hit, teleportDistance, NavMesh.AllAreas))
                {
                    npc.transform.position = hit.position;
                    timeWithoutMovement = 0f;
                    return;
                }
            }

            // Strategy 3: Reset stuck state and try again later
            timeWithoutMovement = 0f;
        }

        /// <summary>
        /// Moves NPC to target with specified speed.
        /// </summary>
        public void MoveToTarget(float speed)
        {
            if (navMeshAgent == null || !navMeshAgent.enabled) return;

            navMeshAgent.speed = speed;
            navMeshAgent.isStopped = false;
            
            if (navigationData.targetPosition != Vector3.zero)
            {
                navMeshAgent.SetDestination(navigationData.targetPosition);
            }
        }

        /// <summary>
        /// Stops NPC movement.
        /// </summary>
        public void StopMovement()
        {
            if (navMeshAgent == null) return;

            navMeshAgent.isStopped = true;
            navMeshAgent.velocity = Vector3.zero;
        }

        /// <summary>
        /// Sets up patrol points for NPC.
        /// </summary>
        public void SetPatrolPoints(Vector3[] points)
        {
            navigationData.patrolPoints = points;
            navigationData.currentPatrolIndex = 0;
            navigationData.waitTimeAtPoint = 0f;
        }

        /// <summary>
        /// Gets distance to target position.
        /// </summary>
        public float GetDistanceToTarget()
        {
            if (navigationData.targetPosition == Vector3.zero)
                return float.MaxValue;

            return Vector3.Distance(npc.transform.position, navigationData.targetPosition);
        }

        /// <summary>
        /// Checks if NPC has reached target position.
        /// </summary>
        public bool HasReachedTarget()
        {
            if (navMeshAgent == null || !navMeshAgent.hasPath)
                return false;

            return navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance;
        }

        /// <summary>
        /// Gets current patrol point.
        /// </summary>
        public Vector3 GetCurrentPatrolPoint()
        {
            if (navigationData.patrolPoints.Length == 0)
                return npc.transform.position;

            return navigationData.patrolPoints[navigationData.currentPatrolIndex];
        }

        /// <summary>
        /// Advances to next patrol point.
        /// </summary>
        public void AdvancePatrolPoint()
        {
            if (navigationData.patrolPoints.Length == 0) return;

            navigationData.currentPatrolIndex = (navigationData.currentPatrolIndex + 1) % navigationData.patrolPoints.Length;
            navigationData.waitTimeAtPoint = 0f;
        }

        /// <summary>
        /// Finds cover position near NPC.
        /// </summary>
        public Vector3 FindCoverPosition()
        {
            Collider[] colliders = Physics.OverlapSphere(npc.transform.position, 10.0f);
            Vector3 bestCover = npc.transform.position;
            float bestScore = float.MaxValue;

            foreach (var collider in colliders)
            {
                if (collider.gameObject == npc.gameObject) continue;
                if (collider.gameObject.layer != LayerMask.NameToLayer("Environment")) continue;

                Vector3 coverPos = collider.ClosestPoint(npc.transform.position);
                
                // Check if cover position is on NavMesh
                NavMeshHit hit;
                if (NavMesh.SamplePosition(coverPos, out hit, 2.0f, NavMesh.AllAreas))
                {
                    // Score based on distance from player and line of sight
                    float score = 0f;
                    
                    // Prefer cover closer to NPC
                    float distanceToCover = Vector3.Distance(npc.transform.position, hit.position);
                    score += distanceToCover * 0.5f;
                    
                    // Prefer cover that blocks line of sight to player
                    GameObject player = GameObject.FindGameObjectWithTag("Player");
                    if (player != null)
                    {
                        Vector3 playerPos = player.transform.position;
                        if (Physics.Linecast(hit.position, playerPos, LayerMask.GetMask("Environment")))
                        {
                            score -= 10f; // Bonus for blocking line of sight
                        }
                    }
                    
                    if (score < bestScore)
                    {
                        bestScore = score;
                        bestCover = hit.position;
                    }
                }
            }

            return bestCover;
        }

        /// <summary>
        /// Draws debug visualization for navigation.
        /// </summary>
        public void DrawDebugGizmos()
        {
            if (npc == null || navMeshAgent == null) return;

            // Draw target position
            if (navigationData.targetPosition != Vector3.zero)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireSphere(navigationData.targetPosition, 0.5f);
                Gizmos.DrawLine(npc.transform.position, navigationData.targetPosition);
            }

            // Draw patrol points
            if (navigationData.patrolPoints.Length > 0)
            {
                Gizmos.color = Color.green;
                for (int i = 0; i < navigationData.patrolPoints.Length; i++)
                {
                    Gizmos.DrawWireSphere(navigationData.patrolPoints[i], 0.3f);
                    
                    if (i < navigationData.patrolPoints.Length - 1)
                    {
                        Gizmos.DrawLine(navigationData.patrolPoints[i], navigationData.patrolPoints[i + 1]);
                    }
                    else
                    {
                        Gizmos.DrawLine(navigationData.patrolPoints[i], navigationData.patrolPoints[0]);
                    }
                }

                // Highlight current patrol point
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(navigationData.patrolPoints[navigationData.currentPatrolIndex], 0.4f);
            }

            // Draw stuck indicator
            if (navigationData.isStuck)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireCube(npc.transform.position + Vector3.up * 2f, Vector3.one * 0.5f);
            }
        }
    }
}