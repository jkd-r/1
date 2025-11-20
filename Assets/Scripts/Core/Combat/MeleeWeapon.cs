using UnityEngine;

namespace ProtocolEMR.Core.Combat
{
    /// <summary>
    /// Melee weapon implementation with hit box detection and damage application.
    /// Supports combo chains and variable damage based on attack strength.
    /// </summary>
    public class MeleeWeapon : Weapon
    {
        [Header("Melee Settings")]
        [SerializeField] private float hitBoxRadius = 1f;
        [SerializeField] private Vector3 hitBoxOffset = Vector3.forward;
        [SerializeField] private float attackReachDistance = 3f;
        [SerializeField] private LayerMask targetLayers = -1;

        [Header("Combo Settings")]
        [SerializeField] private int maxComboLength = 3;
        [SerializeField] private float comboWindow = 1.2f;
        [SerializeField] private float comboDamageMultiplier = 1.2f;

        [Header("Attack Animation")]
        [SerializeField] private string attackTrigger = "Attack";
        [SerializeField] private string comboCountParam = "ComboCount";
        [SerializeField] private float animationDuration = 0.6f;

        [Header("References")]
        [SerializeField] private HitDetection hitDetection;
        [SerializeField] private Transform hitBoxOrigin;

        private int currentCombo = 0;
        private float comboTimer = 0f;

        protected override void Awake()
        {
            base.Awake();
            if (hitDetection == null)
                hitDetection = GetComponent<HitDetection>();
            if (hitDetection == null)
                hitDetection = GetComponentInParent<HitDetection>();
            if (hitBoxOrigin == null)
                hitBoxOrigin = transform;
        }

        private void Update()
        {
            UpdateComboTimer();
        }

        /// <summary>
        /// Performs a melee attack.
        /// </summary>
        public override void Attack()
        {
            if (!CanAttack)
                return;

            OnAttackStart();
            currentCombo = (currentCombo + 1) % (maxComboLength + 1);
            if (currentCombo == 0) currentCombo = 1;
            comboTimer = comboWindow;

            if (animator != null)
            {
                animator.SetTrigger(attackTrigger);
                animator.SetInteger(comboCountParam, currentCombo);
            }

            PerformMeleeHit();
            Invoke(nameof(OnAttackEnd), animationDuration);
        }

        /// <summary>
        /// Performs the actual melee hit detection and damage application.
        /// </summary>
        private void PerformMeleeHit()
        {
            if (hitDetection == null)
                return;

            Vector3 hitBoxPosition = hitBoxOrigin.position + hitBoxOrigin.TransformDirection(hitBoxOffset);
            hitDetection.ResetHitTargets();

            Collider[] hits = hitDetection.DetectMeleeHits(hitBoxPosition, hitBoxRadius, targetLayers);

            float damageMultiplier = 1f + ((currentCombo - 1) * (comboDamageMultiplier - 1f));
            float finalDamage = baseDamage * damageMultiplier;

            foreach (Collider hit in hits)
            {
                if (hit.CompareTag("Player"))
                    continue;

                if (hitDetection.HasAlreadyHit(hit))
                    continue;

                Damageable damageable = hit.GetComponent<Damageable>();
                if (damageable != null)
                {
                    Vector3 knockbackDirection = (hit.transform.position - hitBoxPosition).normalized;
                    damageable.TakeDamage(finalDamage, knockbackForce, knockbackDirection, gameObject);
                    hitDetection.RegisterHit(hit);
                    OnHit(damageable);
                }
            }
        }

        /// <summary>
        /// Updates the combo timer.
        /// </summary>
        private void UpdateComboTimer()
        {
            if (comboTimer > 0)
            {
                comboTimer -= Time.deltaTime;
            }
            else if (currentCombo > 0)
            {
                currentCombo = 0;
                if (animator != null)
                {
                    animator.SetInteger(comboCountParam, 0);
                }
            }
        }

        /// <summary>
        /// Resets the combo chain.
        /// </summary>
        public void ResetCombo()
        {
            currentCombo = 0;
            comboTimer = 0f;
            if (animator != null)
            {
                animator.SetInteger(comboCountParam, 0);
            }
        }

        /// <summary>
        /// Gets the current combo count.
        /// </summary>
        /// <returns>Current combo number</returns>
        public int GetComboCount()
        {
            return currentCombo;
        }

        /// <summary>
        /// Draws debug gizmos for hit detection.
        /// </summary>
        private void OnDrawGizmosSelected()
        {
            Vector3 origin = hitBoxOrigin != null ? hitBoxOrigin.position : transform.position;
            Vector3 offset = hitBoxOrigin != null ? hitBoxOrigin.TransformDirection(hitBoxOffset) : hitBoxOffset;
            Vector3 hitBoxPosition = origin + offset;

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(hitBoxPosition, hitBoxRadius);
            Gizmos.DrawLine(origin, hitBoxPosition);
        }
    }
}
