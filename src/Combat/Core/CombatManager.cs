using UnityEngine;
using System.Collections.Generic;

namespace ProtocolEMR.Combat.Core
{
    public enum AttackType
    {
        Punch,
        Kick,
        Wrench,
        Crowbar,
        Pipe
    }

    public enum CombatState
    {
        Idle,
        Attacking,
        Dodging,
        Stunned,
        Parrying
    }

    public class CombatManager : MonoBehaviour
    {
        [Header("Combat Settings")]
        [SerializeField] private float punchDamage = 10f;
        [SerializeField] private float kickDamage = 15f;
        [SerializeField] private float attackSpeed = 0.6f;
        [SerializeField] private float meleeRange = 2f;
        [SerializeField] private float meleeWidth = 0.5f;

        [Header("Stamina Settings")]
        [SerializeField] private float punchStaminaCost = 5f;
        [SerializeField] private float kickStaminaCost = 8f;
        [SerializeField] private float dodgeStaminaCost = 10f;
        [SerializeField] private float currentStamina = 100f;
        [SerializeField] private float maxStamina = 100f;
        [SerializeField] private float staminaRegenRate = 10f;

        [Header("Knockback Settings")]
        [SerializeField] private float punchKnockback = 5f;
        [SerializeField] private float kickKnockback = 10f;

        [Header("Dodge Settings")]
        [SerializeField] private float dodgeDuration = 0.4f;
        [SerializeField] private float dodgeCooldown = 1.5f;
        [SerializeField] private float parryWindow = 0.3f;

        [Header("References")]
        [SerializeField] private Camera playerCamera;
        [SerializeField] private Animator playerAnimator;
        [SerializeField] private Transform attackOrigin;
        [SerializeField] private LayerMask enemyLayer;

        private CombatState currentState = CombatState.Idle;
        private float lastAttackTime = 0f;
        private float lastDodgeTime = -999f;
        private bool isDodging = false;
        private bool isInvulnerable = false;
        private WeaponManager weaponManager;
        private CombatFeedbackSystem feedbackSystem;
        private CombatAudioManager audioManager;

        private Dictionary<AttackType, float> attackDamages;
        private Dictionary<AttackType, float> knockbackForces;

        void Awake()
        {
            weaponManager = GetComponent<WeaponManager>();
            feedbackSystem = GetComponent<CombatFeedbackSystem>();
            audioManager = GetComponent<CombatAudioManager>();

            if (playerCamera == null)
                playerCamera = Camera.main;

            InitializeAttackData();
        }

        void Update()
        {
            RegenerateStamina();
            HandleCombatInput();
        }

        private void InitializeAttackData()
        {
            attackDamages = new Dictionary<AttackType, float>
            {
                { AttackType.Punch, punchDamage },
                { AttackType.Kick, kickDamage },
                { AttackType.Wrench, 15f },
                { AttackType.Crowbar, 20f },
                { AttackType.Pipe, 18f }
            };

            knockbackForces = new Dictionary<AttackType, float>
            {
                { AttackType.Punch, punchKnockback },
                { AttackType.Kick, kickKnockback },
                { AttackType.Wrench, 7f },
                { AttackType.Crowbar, 9f },
                { AttackType.Pipe, 8f }
            };
        }

        private void HandleCombatInput()
        {
            if (currentState == CombatState.Attacking || currentState == CombatState.Stunned)
                return;

            if (Input.GetMouseButtonDown(0))
            {
                PerformAttack(AttackType.Punch);
            }
            else if (Input.GetMouseButtonDown(1))
            {
                PerformAttack(AttackType.Kick);
            }
            else if (Input.GetKeyDown(KeyCode.Q))
            {
                PerformDodge();
            }
        }

        public void PerformAttack(AttackType attackType)
        {
            if (Time.time - lastAttackTime < attackSpeed)
                return;

            float staminaCost = attackType == AttackType.Kick ? kickStaminaCost : punchStaminaCost;
            
            if (currentStamina < staminaCost)
                return;

            currentStamina -= staminaCost;
            lastAttackTime = Time.time;
            currentState = CombatState.Attacking;

            string animationTrigger = GetAnimationTrigger(attackType);
            if (playerAnimator != null)
                playerAnimator.SetTrigger(animationTrigger);

            Invoke(nameof(ResetAttackState), attackSpeed);
            
            PerformHitDetection(attackType);
            
            if (audioManager != null)
                audioManager.PlayAttackSound(attackType);
        }

        private void PerformHitDetection(AttackType attackType)
        {
            Vector3 rayOrigin = attackOrigin != null ? attackOrigin.position : playerCamera.transform.position;
            Vector3 rayDirection = playerCamera.transform.forward;

            RaycastHit[] hits = Physics.SphereCastAll(rayOrigin, meleeWidth, rayDirection, meleeRange, enemyLayer);

            foreach (RaycastHit hit in hits)
            {
                NPCCombatController npc = hit.collider.GetComponent<NPCCombatController>();
                if (npc != null)
                {
                    float damage = attackDamages[attackType];
                    float knockback = knockbackForces[attackType];

                    npc.TakeDamage(damage, rayDirection * knockback, hit.point);

                    if (feedbackSystem != null)
                        feedbackSystem.OnHitSuccess(hit.point, damage, attackType);

                    if (audioManager != null)
                        audioManager.PlayHitSound(hit.point);

                    break;
                }
            }
        }

        public void PerformDodge()
        {
            if (Time.time - lastDodgeTime < dodgeCooldown)
                return;

            if (currentStamina < dodgeStaminaCost)
                return;

            currentStamina -= dodgeStaminaCost;
            lastDodgeTime = Time.time;
            isDodging = true;
            isInvulnerable = true;
            currentState = CombatState.Dodging;

            if (playerAnimator != null)
                playerAnimator.SetTrigger("Dodge");

            Vector3 dodgeDirection = GetDodgeDirection();
            ApplyDodgeMovement(dodgeDirection);

            Invoke(nameof(EndDodge), dodgeDuration);

            if (audioManager != null)
                audioManager.PlayDodgeSound();
        }

        private Vector3 GetDodgeDirection()
        {
            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");

            if (horizontal == 0 && vertical == 0)
                horizontal = 1f;

            Vector3 direction = (playerCamera.transform.right * horizontal + playerCamera.transform.forward * vertical).normalized;
            direction.y = 0;

            return direction;
        }

        private void ApplyDodgeMovement(Vector3 direction)
        {
            Rigidbody rb = GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.AddForce(direction * 10f, ForceMode.Impulse);
            }
        }

        private void EndDodge()
        {
            isDodging = false;
            isInvulnerable = false;
            currentState = CombatState.Idle;
        }

        private void ResetAttackState()
        {
            currentState = CombatState.Idle;
        }

        private void RegenerateStamina()
        {
            if (currentState != CombatState.Attacking && currentState != CombatState.Dodging)
            {
                currentStamina = Mathf.Min(currentStamina + staminaRegenRate * Time.deltaTime, maxStamina);
            }
        }

        private string GetAnimationTrigger(AttackType attackType)
        {
            switch (attackType)
            {
                case AttackType.Punch:
                    return Random.value > 0.5f ? "PunchJab" : "PunchCross";
                case AttackType.Kick:
                    return Random.value > 0.5f ? "KickRoundhouse" : "KickSide";
                case AttackType.Wrench:
                    return "WrenchSwing";
                case AttackType.Crowbar:
                    return "CrowbarJab";
                case AttackType.Pipe:
                    return "PipeSmash";
                default:
                    return "PunchJab";
            }
        }

        public void TakeDamage(float damage)
        {
            if (isInvulnerable)
                return;

            if (feedbackSystem != null)
                feedbackSystem.OnPlayerDamaged(damage);

            if (audioManager != null)
                audioManager.PlayPlayerPainSound();
        }

        public bool IsInvulnerable()
        {
            return isInvulnerable;
        }

        public CombatState GetCurrentState()
        {
            return currentState;
        }

        public float GetCurrentStamina()
        {
            return currentStamina;
        }

        public float GetMaxStamina()
        {
            return maxStamina;
        }

        public float GetDodgeCooldownRemaining()
        {
            return Mathf.Max(0, dodgeCooldown - (Time.time - lastDodgeTime));
        }
    }
}
