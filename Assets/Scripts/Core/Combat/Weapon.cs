using UnityEngine;
using System;

namespace ProtocolEMR.Core.Combat
{
    /// <summary>
    /// Base class for all weapons. Defines common weapon properties and attack behavior.
    /// Subclasses implement specific attack types (melee, ranged, etc.).
    /// </summary>
    public abstract class Weapon : MonoBehaviour
    {
        [Header("Weapon Settings")]
        [SerializeField] protected string weaponName = "Weapon";
        [SerializeField] protected float baseDamage = 20f;
        [SerializeField] protected float attackCooldown = 1f;
        [SerializeField] protected float knockbackForce = 50f;
        [SerializeField] protected float staminaCost = 10f;

        [Header("References")]
        [SerializeField] protected Animator animator;

        protected float lastAttackTime = -999f;
        protected bool isAttacking = false;

        public string WeaponName => weaponName;
        public float BaseDamage => baseDamage;
        public float AttackCooldown => attackCooldown;
        public float KnockbackForce => knockbackForce;
        public float StaminaCost => staminaCost;
        public bool CanAttack => Time.time - lastAttackTime >= attackCooldown && !isAttacking;
        public bool IsAttacking => isAttacking;

        public event Action OnAttackStarted;
        public event Action OnAttackEnded;
        public event Action<Damageable> OnHitTarget;

        protected virtual void Awake()
        {
            if (animator == null)
                animator = GetComponentInParent<Animator>();
        }

        /// <summary>
        /// Performs an attack.
        /// </summary>
        public abstract void Attack();

        /// <summary>
        /// Stops the current attack.
        /// </summary>
        public virtual void StopAttack()
        {
            isAttacking = false;
        }

        /// <summary>
        /// Called when attack starts.
        /// </summary>
        protected virtual void OnAttackStart()
        {
            isAttacking = true;
            OnAttackStarted?.Invoke();
        }

        /// <summary>
        /// Called when attack ends.
        /// </summary>
        protected virtual void OnAttackEnd()
        {
            isAttacking = false;
            lastAttackTime = Time.time;
            OnAttackEnded?.Invoke();
        }

        /// <summary>
        /// Called when a target is hit.
        /// </summary>
        /// <param name="target">The target that was hit</param>
        protected virtual void OnHit(Damageable target)
        {
            OnHitTarget?.Invoke(target);
        }

        /// <summary>
        /// Sets the weapon's damage value.
        /// </summary>
        /// <param name="damage">New damage value</param>
        public virtual void SetDamage(float damage)
        {
            baseDamage = damage;
        }

        /// <summary>
        /// Sets the weapon's knockback force.
        /// </summary>
        /// <param name="force">New knockback force</param>
        public virtual void SetKnockbackForce(float force)
        {
            knockbackForce = force;
        }

        /// <summary>
        /// Gets attack cooldown percentage (0-1).
        /// </summary>
        /// <returns>Cooldown progress percentage</returns>
        public virtual float GetCooldownPercentage()
        {
            if (attackCooldown <= 0f)
                return 1f;

            float elapsed = Time.time - lastAttackTime;
            return Mathf.Clamp01(elapsed / attackCooldown);
        }
    }
}
