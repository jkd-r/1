using UnityEngine;

namespace ProtocolEMR.Core.AI
{
    /// <summary>
    /// Combat system for NPCs - handles attacks, damage, and combat behaviors.
    /// </summary>
    [System.Serializable]
    public class NPCCombat
    {
        private NPCController npc;
        private CombatData combatData;
        
        [Header("Combat Settings")]
        [SerializeField] private float attackRange = 2.0f;
        [SerializeField] private float attackCooldown = 1.0f;
        [SerializeField] private float dodgeChance = 0.3f;
        [SerializeField] private float dodgeCooldown = 2.0f;
        [SerializeField] private float knockbackForce = 5.0f;
        [SerializeField] private LayerMask attackLayerMask = -1;
        
        private float lastAttackTime;
        private float lastDodgeTime;

        public CombatData Data => combatData;
        public bool IsInCombat => combatData.isInCombat;
        public bool CanAttack => combatData.canAttack;
        public GameObject CurrentTarget => combatData.currentTarget;

        public NPCCombat(NPCController controller)
        {
            npc = controller;
            combatData = new CombatData();
        }

        /// <summary>
        /// Updates combat system including attack cooldowns and target tracking.
        /// </summary>
        public void UpdateCombat()
        {
            UpdateAttackCooldown();
            UpdateDodgeCooldown();
            UpdateTargetTracking();
            UpdateCombatState();
        }

        /// <summary>
        /// Updates attack cooldown timers.
        /// </summary>
        private void UpdateAttackCooldown()
        {
            if (Time.time - lastAttackTime >= npc.Parameters.attackFrequency)
            {
                combatData.canAttack = true;
            }
            else
            {
                combatData.canAttack = false;
            }
        }

        /// <summary>
        /// Updates dodge cooldown timers.
        /// </summary>
        private void UpdateDodgeCooldown()
        {
            combatData.dodgeCooldown = Mathf.Max(0f, dodgeCooldown - (Time.time - lastDodgeTime));
        }

        /// <summary>
        /// Updates target tracking and validation.
        /// </summary>
        private void UpdateTargetTracking()
        {
            if (combatData.currentTarget == null)
            {
                // Try to find player as target
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player != null && npc.Perception.CanSeePlayer)
                {
                    combatData.currentTarget = player;
                }
            }
            else
            {
                // Validate target is still valid
                float distance = Vector3.Distance(npc.transform.position, combatData.currentTarget.transform.position);
                if (distance > npc.Parameters.perceptionRange * 1.5f)
                {
                    combatData.currentTarget = null;
                }
            }
        }

        /// <summary>
        /// Updates combat state based on conditions.
        /// </summary>
        private void UpdateCombatState()
        {
            bool wasInCombat = combatData.isInCombat;
            combatData.isInCombat = combatData.currentTarget != null && npc.Perception.ShouldBeAlert();

            if (!wasInCombat && combatData.isInCombat)
            {
                OnEnterCombat();
            }
            else if (wasInCombat && !combatData.isInCombat)
            {
                OnExitCombat();
            }
        }

        /// <summary>
        /// Called when NPC enters combat.
        /// </summary>
        private void OnEnterCombat()
        {
            npc.Animator.SetBool("InCombat", true);
            Debug.Log($"{npc.name} entered combat");
        }

        /// <summary>
        /// Called when NPC exits combat.
        /// </summary>
        private void OnExitCombat()
        {
            npc.Animator.SetBool("InCombat", false);
            combatData.currentTarget = null;
            Debug.Log($"{npc.name} exited combat");
        }

        /// <summary>
        /// Performs attack against current target.
        /// </summary>
        public void Attack()
        {
            if (!combatData.canAttack || combatData.currentTarget == null)
                return;

            lastAttackTime = Time.time;
            combatData.canAttack = false;
            combatData.timeSinceLastAttack = 0f;

            // Play attack animation
            npc.Animator.SetTrigger("Attack");
            combatData.isAttacking = true;

            // Store attack direction for knockback
            combatData.lastAttackDirection = (combatData.currentTarget.transform.position - npc.transform.position).normalized;

            // Deal damage after animation delay
            npc.Invoke(nameof(DealDamage), 0.5f);

            Debug.Log($"{npc.name} attacked {combatData.currentTarget.name}");
        }

        /// <summary>
        /// Deals damage to target.
        /// </summary>
        private void DealDamage()
        {
            if (combatData.currentTarget == null) return;

            float distance = Vector3.Distance(npc.transform.position, combatData.currentTarget.transform.position);
            if (distance <= attackRange)
            {
                // Deal damage to target
                IDamageable damageable = combatData.currentTarget.GetComponent<IDamageable>();
                if (damageable != null)
                {
                    damageable.TakeDamage(npc.Parameters.damagePerHit);
                    
                    // Apply knockback
                    ApplyKnockback(combatData.currentTarget);
                }
            }

            combatData.isAttacking = false;
        }

        /// <summary>
        /// Applies knockback force to target.
        /// </summary>
        private void ApplyKnockback(GameObject target)
        {
            Rigidbody targetRigidbody = target.GetComponent<Rigidbody>();
            if (targetRigidbody != null)
            {
                Vector3 knockbackDirection = (target.transform.position - npc.transform.position).normalized;
                knockbackDirection.y = 0.5f; // Add upward component
                targetRigidbody.AddForce(knockbackDirection * knockbackForce, ForceMode.Impulse);
            }
        }

        /// <summary>
        /// Attempts to dodge incoming attack.
        /// </summary>
        public bool TryDodge()
        {
            if (combatData.dodgeCooldown > 0f || combatData.isDodging)
                return false;

            float dodgeRoll = Random.Range(0f, 1f);
            float effectiveDodgeChance = dodgeChance * (npc.Parameters.intelligence / 100f);

            if (dodgeRoll <= effectiveDodgeChance)
            {
                PerformDodge();
                return true;
            }

            return false;
        }

        /// <summary>
        /// Performs dodge action.
        /// </summary>
        private void PerformDodge()
        {
            lastDodgeTime = Time.time;
            combatData.isDodging = true;

            // Play dodge animation
            npc.Animator.SetTrigger("Dodge");

            // Apply dodge movement
            Vector3 dodgeDirection = Random.insideUnitSphere;
            dodgeDirection.y = 0;
            dodgeDirection.Normalize();

            Vector3 dodgePosition = npc.transform.position + dodgeDirection * 2.0f;
            npc.Navigation.SetTargetPosition(dodgePosition);

            npc.Invoke(nameof(CompleteDodge), 0.8f);

            Debug.Log($"{npc.name} dodged");
        }

        /// <summary>
        /// Completes dodge action.
        /// </summary>
        private void CompleteDodge()
        {
            combatData.isDodging = false;
        }

        /// <summary>
        /// Takes damage from external source.
        /// </summary>
        public void TakeDamage(float damage, Vector3 damageDirection)
        {
            npc.CurrentHealth -= damage;
            combatData.lastAttackDirection = damageDirection;

            // Play hurt animation
            npc.Animator.SetTrigger("Hurt");

            // Apply knockback
            Rigidbody npcRigidbody = npc.GetComponent<Rigidbody>();
            if (npcRigidbody != null)
            {
                Vector3 knockbackDirection = -damageDirection.normalized;
                knockbackDirection.y = 0.3f;
                npcRigidbody.AddForce(knockbackDirection * knockbackForce * 0.5f, ForceMode.Impulse);
            }

            // Check if should flee based on health
            if (npc.CurrentHealth <= npc.Parameters.maxHealth * 0.25f)
            {
                float fleeChance = Random.Range(0f, 100f);
                if (fleeChance > npc.Parameters.morale)
                {
                    npc.SetState(NPCState.Flee);
                }
            }

            // Check if should dodge
            if (Random.Range(0f, 100f) < npc.Parameters.intelligence)
            {
                TryDodge();
            }

            Debug.Log($"{npc.name} took {damage} damage, health: {npc.CurrentHealth}/{npc.Parameters.maxHealth}");

            // Check for death
            if (npc.CurrentHealth <= 0)
            {
                npc.Die();
            }
        }

        /// <summary>
        /// Stuns NPC for specified duration.
        /// </summary>
        public void Stun(float duration)
        {
            npc.SetState(NPCState.Stun);
            npc.Animator.SetTrigger("Stun");
            npc.Invoke(nameof(RecoverFromStun), duration);

            Debug.Log($"{npc.name} stunned for {duration} seconds");
        }

        /// <summary>
        /// Recovers from stun state.
        /// </summary>
        private void RecoverFromStun()
        {
            if (npc.CurrentState == NPCState.Stun)
            {
                npc.SetState(NPCState.Idle);
                npc.Animator.SetTrigger("Recover");
            }
        }

        /// <summary>
        /// Gets effective damage based on difficulty scaling.
        /// </summary>
        public float GetEffectiveDamage()
        {
            float difficultyMultiplier = GetDifficultyMultiplier();
            return npc.Parameters.damagePerHit * difficultyMultiplier;
        }

        /// <summary>
        /// Gets difficulty multiplier for combat stats.
        /// </summary>
        private float GetDifficultyMultiplier()
        {
            // This would be tied to a global difficulty setting
            // For now, return 1.0 (Normal difficulty)
            return 1.0f;
        }

        /// <summary>
        /// Draws debug visualization for combat.
        /// </summary>
        public void DrawDebugGizmos()
        {
            if (npc == null) return;

            // Draw attack range
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(npc.transform.position, attackRange);

            // Draw line to current target
            if (combatData.currentTarget != null)
            {
                Gizmos.color = Color.magenta;
                Gizmos.DrawLine(npc.transform.position, combatData.currentTarget.transform.position);
            }

            // Draw dodge cooldown indicator
            if (combatData.dodgeCooldown > 0f)
            {
                Gizmos.color = Color.blue;
                Vector3 indicatorPos = npc.transform.position + Vector3.up * 3f;
                Gizmos.DrawWireSphere(indicatorPos, 0.2f);
            }
        }
    }

    /// <summary>
    /// Interface for damageable objects.
    /// </summary>
    public interface IDamageable
    {
        void TakeDamage(float damage);
    }
}