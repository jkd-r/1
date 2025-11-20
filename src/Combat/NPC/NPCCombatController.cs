using UnityEngine;
using UnityEngine.AI;
using System.Collections;

namespace ProtocolEMR.Combat.Core
{
    public enum NPCState
    {
        Idle,
        Alert,
        Attack,
        Damaged,
        Stagger,
        Knockback,
        Stunned,
        Death
    }

    public class NPCCombatController : MonoBehaviour
    {
        [Header("NPC Stats")]
        [SerializeField] private float maxHealth = 30f;
        [SerializeField] private float currentHealth = 30f;
        [SerializeField] private float attackDamage = 5f;
        [SerializeField] private float attackRange = 2f;
        [SerializeField] private float attackCooldown = 2f;

        [Header("Animation Settings")]
        [SerializeField] private Animator npcAnimator;

        [Header("State Settings")]
        [SerializeField] private NPCState currentState = NPCState.Idle;
        [SerializeField] private float staggerDuration = 0.3f;
        [SerializeField] private float knockbackRecoveryTime = 0.8f;

        [Header("Aggression Settings")]
        [SerializeField] private float aggressionLevel = 1.0f;
        [SerializeField] private float detectionRange = 10f;

        [Header("Health Bar")]
        [SerializeField] private GameObject healthBarPrefab;
        private GameObject healthBarInstance;
        private NPCHealthBar healthBarController;

        [Header("References")]
        [SerializeField] private Transform targetPlayer;
        [SerializeField] private NavMeshAgent navAgent;
        [SerializeField] private Rigidbody npcRigidbody;

        [Header("Audio")]
        [SerializeField] private AudioSource npcAudioSource;
        [SerializeField] private AudioClip[] gruntSounds;
        [SerializeField] private AudioClip[] painSounds;
        [SerializeField] private AudioClip[] deathSounds;

        [Header("Ragdoll")]
        [SerializeField] private bool useRagdoll = true;
        [SerializeField] private Rigidbody[] ragdollRigidbodies;

        private float lastAttackTime = 0f;
        private float stateTimer = 0f;
        private bool isDead = false;
        private Vector3 knockbackVelocity = Vector3.zero;

        void Awake()
        {
            if (npcAnimator == null)
                npcAnimator = GetComponent<Animator>();

            if (navAgent == null)
                navAgent = GetComponent<NavMeshAgent>();

            if (npcRigidbody == null)
                npcRigidbody = GetComponent<Rigidbody>();

            if (npcAudioSource == null)
                npcAudioSource = gameObject.AddComponent<AudioSource>();

            currentHealth = maxHealth;

            InitializeHealthBar();
            InitializeRagdoll();
        }

        void Start()
        {
            if (targetPlayer == null)
            {
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                    targetPlayer = player.transform;
            }
        }

        void Update()
        {
            if (isDead)
                return;

            UpdateState();
            UpdateHealthBarPosition();
        }

        private void UpdateState()
        {
            stateTimer += Time.deltaTime;

            switch (currentState)
            {
                case NPCState.Idle:
                    HandleIdleState();
                    break;
                case NPCState.Alert:
                    HandleAlertState();
                    break;
                case NPCState.Attack:
                    HandleAttackState();
                    break;
                case NPCState.Damaged:
                    HandleDamagedState();
                    break;
                case NPCState.Stagger:
                    HandleStaggerState();
                    break;
                case NPCState.Knockback:
                    HandleKnockbackState();
                    break;
                case NPCState.Stunned:
                    HandleStunnedState();
                    break;
            }
        }

        private void HandleIdleState()
        {
            if (targetPlayer != null)
            {
                float distance = Vector3.Distance(transform.position, targetPlayer.position);
                if (distance <= detectionRange)
                {
                    ChangeState(NPCState.Alert);
                }
            }
        }

        private void HandleAlertState()
        {
            if (targetPlayer == null)
            {
                ChangeState(NPCState.Idle);
                return;
            }

            float distance = Vector3.Distance(transform.position, targetPlayer.position);

            if (distance <= attackRange)
            {
                ChangeState(NPCState.Attack);
            }
            else if (navAgent != null && navAgent.isOnNavMesh)
            {
                navAgent.SetDestination(targetPlayer.position);
            }

            if (npcAnimator != null)
                npcAnimator.SetFloat("Speed", navAgent.velocity.magnitude);
        }

        private void HandleAttackState()
        {
            if (targetPlayer == null)
            {
                ChangeState(NPCState.Idle);
                return;
            }

            Vector3 directionToPlayer = (targetPlayer.position - transform.position).normalized;
            Quaternion lookRotation = Quaternion.LookRotation(new Vector3(directionToPlayer.x, 0, directionToPlayer.z));
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);

            if (Time.time - lastAttackTime >= attackCooldown)
            {
                PerformAttack();
                lastAttackTime = Time.time;
            }

            float distance = Vector3.Distance(transform.position, targetPlayer.position);
            if (distance > attackRange)
            {
                ChangeState(NPCState.Alert);
            }
        }

        private void HandleDamagedState()
        {
            if (stateTimer >= 0.2f)
            {
                ChangeState(NPCState.Alert);
            }
        }

        private void HandleStaggerState()
        {
            if (stateTimer >= staggerDuration)
            {
                ChangeState(NPCState.Alert);
            }
        }

        private void HandleKnockbackState()
        {
            if (stateTimer >= knockbackRecoveryTime)
            {
                ChangeState(NPCState.Alert);
            }
        }

        private void HandleStunnedState()
        {
        }

        private void PerformAttack()
        {
            if (npcAnimator != null)
                npcAnimator.SetTrigger("Attack");

            if (targetPlayer != null)
            {
                CombatManager playerCombat = targetPlayer.GetComponent<CombatManager>();
                if (playerCombat != null)
                {
                    float scaledDamage = attackDamage * aggressionLevel;
                    playerCombat.TakeDamage(scaledDamage);
                }
            }

            PlayRandomSound(gruntSounds);
        }

        public void TakeDamage(float damage, Vector3 knockbackForce, Vector3 hitPoint)
        {
            if (isDead)
                return;

            currentHealth -= damage;

            if (healthBarController != null)
                healthBarController.UpdateHealth(currentHealth, maxHealth);

            if (currentHealth <= 0)
            {
                Die();
                return;
            }

            if (knockbackForce.magnitude > 5f)
            {
                ApplyKnockback(knockbackForce);
                ChangeState(NPCState.Knockback);
            }
            else
            {
                ChangeState(NPCState.Stagger);
            }

            PlayRandomSound(painSounds);

            if (npcAnimator != null)
            {
                if (knockbackForce.magnitude > 5f)
                    npcAnimator.SetTrigger("Knockback");
                else
                    npcAnimator.SetTrigger("Hit");
            }
        }

        private void ApplyKnockback(Vector3 force)
        {
            if (npcRigidbody != null)
            {
                npcRigidbody.AddForce(force, ForceMode.Impulse);
            }

            if (navAgent != null && navAgent.isOnNavMesh)
            {
                navAgent.isStopped = true;
                StartCoroutine(ReenableNavAgent(knockbackRecoveryTime));
            }
        }

        private IEnumerator ReenableNavAgent(float delay)
        {
            yield return new WaitForSeconds(delay);
            if (navAgent != null && navAgent.isOnNavMesh)
            {
                navAgent.isStopped = false;
            }
        }

        public void ApplyStun(float duration)
        {
            if (isDead)
                return;

            ChangeState(NPCState.Stunned);

            if (npcAnimator != null)
                npcAnimator.SetTrigger("Stunned");

            if (navAgent != null && navAgent.isOnNavMesh)
                navAgent.isStopped = true;

            StartCoroutine(RecoverFromStun(duration));
        }

        private IEnumerator RecoverFromStun(float duration)
        {
            yield return new WaitForSeconds(duration);

            if (navAgent != null && navAgent.isOnNavMesh)
                navAgent.isStopped = false;

            ChangeState(NPCState.Alert);
        }

        private void Die()
        {
            isDead = true;
            currentState = NPCState.Death;

            if (npcAnimator != null)
                npcAnimator.SetTrigger("Death");

            if (navAgent != null)
                navAgent.enabled = false;

            PlayRandomSound(deathSounds);

            if (useRagdoll)
                EnableRagdoll();

            if (healthBarInstance != null)
                Destroy(healthBarInstance);

            StartCoroutine(RemoveCorpse(3f));
        }

        private IEnumerator RemoveCorpse(float delay)
        {
            yield return new WaitForSeconds(delay);

            float fadeDuration = 2f;
            float elapsed = 0f;

            Renderer[] renderers = GetComponentsInChildren<Renderer>();

            while (elapsed < fadeDuration)
            {
                float alpha = 1f - (elapsed / fadeDuration);
                foreach (Renderer renderer in renderers)
                {
                    foreach (Material mat in renderer.materials)
                    {
                        if (mat.HasProperty("_Color"))
                        {
                            Color color = mat.color;
                            color.a = alpha;
                            mat.color = color;
                        }
                    }
                }
                elapsed += Time.deltaTime;
                yield return null;
            }

            Destroy(gameObject);
        }

        private void ChangeState(NPCState newState)
        {
            currentState = newState;
            stateTimer = 0f;

            if (npcAnimator != null)
            {
                npcAnimator.SetInteger("State", (int)newState);
            }
        }

        private void InitializeHealthBar()
        {
            if (healthBarPrefab != null)
            {
                healthBarInstance = Instantiate(healthBarPrefab, transform.position + Vector3.up * 2.5f, Quaternion.identity);
                healthBarController = healthBarInstance.GetComponent<NPCHealthBar>();
                if (healthBarController != null)
                    healthBarController.Initialize(maxHealth);
            }
        }

        private void UpdateHealthBarPosition()
        {
            if (healthBarInstance != null && Camera.main != null)
            {
                healthBarInstance.transform.position = transform.position + Vector3.up * 2.5f;
                healthBarInstance.transform.LookAt(Camera.main.transform);
                healthBarInstance.transform.Rotate(0, 180, 0);
            }
        }

        private void InitializeRagdoll()
        {
            ragdollRigidbodies = GetComponentsInChildren<Rigidbody>();
            DisableRagdoll();
        }

        private void EnableRagdoll()
        {
            if (npcAnimator != null)
                npcAnimator.enabled = false;

            foreach (Rigidbody rb in ragdollRigidbodies)
            {
                if (rb != npcRigidbody)
                {
                    rb.isKinematic = false;
                    rb.useGravity = true;
                }
            }
        }

        private void DisableRagdoll()
        {
            foreach (Rigidbody rb in ragdollRigidbodies)
            {
                if (rb != npcRigidbody)
                {
                    rb.isKinematic = true;
                    rb.useGravity = false;
                }
            }
        }

        private void PlayRandomSound(AudioClip[] clips)
        {
            if (clips == null || clips.Length == 0 || npcAudioSource == null)
                return;

            AudioClip clip = clips[Random.Range(0, clips.Length)];
            float pitch = Random.Range(0.9f, 1.1f);
            npcAudioSource.pitch = pitch;
            npcAudioSource.PlayOneShot(clip);
        }

        public void SetAggressionLevel(float level)
        {
            aggressionLevel = Mathf.Clamp(level, 0.5f, 2.0f);
        }

        public NPCState GetCurrentState()
        {
            return currentState;
        }

        public float GetHealthPercentage()
        {
            return currentHealth / maxHealth;
        }

        public bool IsDead()
        {
            return isDead;
        }
    }
}
