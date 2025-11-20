using UnityEngine;

namespace ProtocolEMR.Core.AI
{
    /// <summary>
    /// Enumeration of possible NPC states for behavior management.
    /// </summary>
    public enum NPCState
    {
        Idle,
        Alert,
        Chase,
        Flee,
        Hide,
        Attack,
        Patrol,
        Investigate,
        Dialogue,
        Stun,
        Dead
    }

    /// <summary>
    /// Enumeration of NPC types with different behavior patterns.
    /// </summary>
    public enum NPCType
    {
        Guard,
        Scientist,
        Civilian,
        Beast
    }

    /// <summary>
    /// Data structure for procedurally generated NPC parameters.
    /// </summary>
    [System.Serializable]
    public class NPCParameters
    {
        [Header("Movement Speeds (m/s)")]
        public float walkSpeed = 4.0f;
        public float runSpeed = 8.0f;
        public float sprintSpeed = 12.0f;
        
        [Header("Health & Combat")]
        public float maxHealth = 50.0f;
        public float damagePerHit = 15.0f;
        public float attackFrequency = 1.0f;
        
        [Header("Perception")]
        public float perceptionRange = 15.0f;
        public float fieldOfView = 120.0f;
        public float hearingRange = 10.0f;
        public float reactionTime = 0.5f;
        
        [Header("Behavior")]
        [Range(0f, 100f)] public float aggression = 70.0f;
        [Range(0f, 100f)] public float intelligence = 60.0f;
        [Range(0f, 100f)] public float morale = 80.0f;
        
        [Header("Stamina")]
        public float maxStamina = 100.0f;
        public float staminaDrainRate = 10.0f;
        public float staminaRecoveryRate = 20.0f;
    }

    /// <summary>
    /// Perception data for NPC awareness of surroundings.
    /// </summary>
    [System.Serializable]
    public struct PerceptionData
    {
        public bool canSeePlayer;
        public bool canHearPlayer;
        public Vector3 lastKnownPlayerPosition;
        public float timeSinceLastSighting;
        public float alertness;
        public Vector3 soundSource;
        public float timeSinceSound;
    }

    /// <summary>
    /// Navigation and pathfinding data for NPC movement.
    /// </summary>
    [System.Serializable]
    public struct NavigationData
    {
        public Vector3 targetPosition;
        public bool hasPath;
        public bool isStuck;
        public float timeStuck;
        public Vector3[] patrolPoints;
        public int currentPatrolIndex;
        public float waitTimeAtPoint;
    }

    /// <summary>
    /// Combat-related data for NPC behavior.
    /// </summary>
    [System.Serializable]
    public struct CombatData
    {
        public bool isInCombat;
        public float timeSinceLastAttack;
        public bool canAttack;
        public bool isDodging;
        public float dodgeCooldown;
        public GameObject currentTarget;
        public Vector3 lastAttackDirection;
    }

    /// <summary>
    /// Animation and audio state tracking.
    /// </summary>
    [System.Serializable]
    public struct AnimationData
    {
        public string currentAnimationState;
        public float animationBlendTime;
        public bool isMoving;
        public bool isRunning;
        public bool isAttacking;
        public bool isDead;
        public bool isStunned;
    }
}