using UnityEngine;
using System;

namespace ProtocolEMR.Core.Combat
{
    /// <summary>
    /// Generic damage and health system component that can be used by both player and NPCs.
    /// Handles health tracking, damage application, knockback, and death state.
    /// </summary>
    public class Damageable : MonoBehaviour
    {
        [Header("Health Settings")]
        [SerializeField] private float maxHealth = 100f;
        [SerializeField] private float currentHealth;
        [SerializeField] private bool invulnerable = false;

        [Header("Knockback Settings")]
        [SerializeField] private float knockbackMultiplier = 1f;
        [SerializeField] private float knockbackDuration = 0.3f;

        [Header("References")]
        [SerializeField] private Rigidbody rigidBody;
        [SerializeField] private CharacterController characterController;

        private bool isDead = false;
        private bool isKnockedBack = false;
        private float knockbackTimer = 0f;
        private Vector3 knockbackVelocity = Vector3.zero;

        public float CurrentHealth => currentHealth;
        public float MaxHealth => maxHealth;
        public float HealthPercentage => maxHealth > 0 ? currentHealth / maxHealth : 1f;
        public bool IsDead => isDead;
        public bool IsKnockedBack => isKnockedBack;

        public event Action<float> OnDamageTaken;
        public event Action<float> OnHealthChanged;
        public event Action OnDeath;
        public event Action<Vector3, float> OnKnockbackApplied;

        private void Awake()
        {
            if (rigidBody == null)
                rigidBody = GetComponent<Rigidbody>();
            if (characterController == null)
                characterController = GetComponent<CharacterController>();

            currentHealth = maxHealth;
        }

        private void Update()
        {
            UpdateKnockback();
        }

        /// <summary>
        /// Applies damage to this object.
        /// </summary>
        /// <param name="damage">Amount of damage to apply</param>
        /// <param name="knockbackForce">Force of knockback in Newtons</param>
        /// <param name="knockbackDirection">Direction of knockback force</param>
        /// <param name="damageSource">Source of the damage (for tracking)</param>
        public void TakeDamage(float damage, float knockbackForce = 0f, Vector3 knockbackDirection = default, GameObject damageSource = null)
        {
            if (isDead || invulnerable)
                return;

            currentHealth -= damage;
            OnDamageTaken?.Invoke(damage);
            OnHealthChanged?.Invoke(currentHealth);

            if (knockbackForce > 0f)
            {
                ApplyKnockback(knockbackForce, knockbackDirection);
            }

            if (currentHealth <= 0f)
            {
                Die(damageSource);
            }
        }

        /// <summary>
        /// Applies knockback to this object.
        /// </summary>
        /// <param name="force">Force magnitude of knockback</param>
        /// <param name="direction">Direction of knockback</param>
        public void ApplyKnockback(float force, Vector3 direction)
        {
            if (direction.magnitude > 0.01f)
            {
                direction.Normalize();
            }
            else
            {
                direction = Vector3.forward;
            }

            if (rigidBody != null && !rigidBody.isKinematic)
            {
                rigidBody.velocity += direction * force * knockbackMultiplier;
                isKnockedBack = true;
                knockbackTimer = knockbackDuration;
                OnKnockbackApplied?.Invoke(direction, force);
            }
            else if (characterController != null && characterController.enabled)
            {
                knockbackVelocity = direction * force * knockbackMultiplier;
                isKnockedBack = true;
                knockbackTimer = knockbackDuration;
                OnKnockbackApplied?.Invoke(direction, force);
            }
        }

        /// <summary>
        /// Updates knockback state over time.
        /// </summary>
        private void UpdateKnockback()
        {
            if (!isKnockedBack)
                return;

            knockbackTimer -= Time.deltaTime;

            if (characterController != null && characterController.enabled)
            {
                characterController.Move(knockbackVelocity * Time.deltaTime);
                knockbackVelocity *= 0.9f;
            }

            if (knockbackTimer <= 0f)
            {
                isKnockedBack = false;
                knockbackVelocity = Vector3.zero;
            }
        }

        /// <summary>
        /// Heals this object.
        /// </summary>
        /// <param name="amount">Amount of health to restore</param>
        public void Heal(float amount)
        {
            if (isDead)
                return;

            currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
            OnHealthChanged?.Invoke(currentHealth);
        }

        /// <summary>
        /// Fully restores health.
        /// </summary>
        public void FullHeal()
        {
            if (isDead)
                return;

            currentHealth = maxHealth;
            OnHealthChanged?.Invoke(currentHealth);
        }

        /// <summary>
        /// Sets health to a specific value.
        /// </summary>
        /// <param name="health">New health value</param>
        public void SetHealth(float health)
        {
            currentHealth = Mathf.Clamp(health, 0f, maxHealth);
            OnHealthChanged?.Invoke(currentHealth);

            if (currentHealth <= 0f && !isDead)
            {
                Die(null);
            }
        }

        /// <summary>
        /// Handles death state.
        /// </summary>
        /// <param name="killer">The object that caused death</param>
        private void Die(GameObject killer)
        {
            if (isDead)
                return;

            isDead = true;
            isKnockedBack = false;
            OnDeath?.Invoke();

            gameObject.layer = LayerMask.NameToLayer("Dead");

            if (rigidBody != null)
            {
                rigidBody.isKinematic = true;
            }
            if (characterController != null)
            {
                characterController.enabled = false;
            }
        }

        /// <summary>
        /// Revives this object.
        /// </summary>
        /// <param name="healthRestored">Percentage of max health to restore (0-1)</param>
        public void Revive(float healthRestored = 1f)
        {
            isDead = false;
            isKnockedBack = false;
            knockbackTimer = 0f;
            knockbackVelocity = Vector3.zero;

            currentHealth = maxHealth * Mathf.Clamp01(healthRestored);
            OnHealthChanged?.Invoke(currentHealth);

            if (rigidBody != null)
            {
                rigidBody.isKinematic = false;
                rigidBody.velocity = Vector3.zero;
            }
            if (characterController != null)
            {
                characterController.enabled = true;
            }
        }

        /// <summary>
        /// Sets invulnerability state.
        /// </summary>
        /// <param name="invulnerable">Whether this object should be invulnerable</param>
        public void SetInvulnerable(bool invulnerable)
        {
            this.invulnerable = invulnerable;
        }

        /// <summary>
        /// Sets the max health value.
        /// </summary>
        /// <param name="newMaxHealth">New maximum health</param>
        public void SetMaxHealth(float newMaxHealth)
        {
            maxHealth = newMaxHealth;
            currentHealth = Mathf.Min(currentHealth, maxHealth);
            OnHealthChanged?.Invoke(currentHealth);
        }

        /// <summary>
        /// Draws debug gizmos for health system.
        /// </summary>
        private void OnDrawGizmosSelected()
        {
            if (isDead)
            {
                Gizmos.color = Color.red;
            }
            else if (HealthPercentage < 0.33f)
            {
                Gizmos.color = Color.yellow;
            }
            else
            {
                Gizmos.color = Color.green;
            }

            Gizmos.DrawWireSphere(transform.position, 0.5f);
        }
    }
}
