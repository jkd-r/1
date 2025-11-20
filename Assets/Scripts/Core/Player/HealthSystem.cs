using UnityEngine;
using System;
using ProtocolEMR.UI;

namespace ProtocolEMR.Core.Player
{
    public class HealthSystem : MonoBehaviour
    {
        [SerializeField] private float maxHealth = 100f;
        [SerializeField] private float maxStamina = 100f;
        [SerializeField] private float staminaRegenRate = 15f;
        [SerializeField] private float staminaRegenDelay = 1f;
        [SerializeField] private float damageFlashDuration = 0.5f;

        private float currentHealth;
        private float currentStamina;
        private float timeSinceLastStaminaUse = 0f;
        private bool isDead = false;

        public event Action<float, float> OnHealthChanged;
        public event Action<float, float> OnStaminaChanged;
        public event Action<float> OnDamageTaken;
        public event Action OnDeath;

        public float CurrentHealth => currentHealth;
        public float MaxHealth => maxHealth;
        public float CurrentStamina => currentStamina;
        public float MaxStamina => maxStamina;
        public bool IsDead => isDead;

        private void Start()
        {
            currentHealth = maxHealth;
            currentStamina = maxStamina;
        }

        private void Update()
        {
            UpdateStaminaRegen();
            UpdateHUD();
        }

        public void TakeDamage(float damage, Vector3 damageDirection = default)
        {
            if (isDead) return;

            currentHealth -= damage;
            OnDamageTaken?.Invoke(damage);

            if (HUDManager.Instance != null)
            {
                HUDManager.Instance.ShowDamageIndicator(damageDirection, damage / maxHealth);
            }

            if (NotificationManager.Instance != null && damage > 0)
            {
                NotificationManager.Instance.ShowWarning($"Took {damage:F0} damage!");
            }

            if (currentHealth <= 0)
            {
                Die();
            }
        }

        public void Heal(float amount)
        {
            if (isDead) return;

            currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
        }

        public bool ConsumesStamina(float amount)
        {
            if (currentStamina >= amount)
            {
                currentStamina -= amount;
                timeSinceLastStaminaUse = 0f;
                OnStaminaChanged?.Invoke(currentStamina, maxStamina);
                return true;
            }
            return false;
        }

        public void RestoreStamina(float amount)
        {
            currentStamina = Mathf.Min(currentStamina + amount, maxStamina);
            OnStaminaChanged?.Invoke(currentStamina, maxStamina);
        }

        private void UpdateStaminaRegen()
        {
            timeSinceLastStaminaUse += Time.deltaTime;

            if (timeSinceLastStaminaUse >= staminaRegenDelay && currentStamina < maxStamina)
            {
                RestoreStamina(staminaRegenRate * Time.deltaTime);
            }
        }

        private void UpdateHUD()
        {
            if (HUDManager.Instance != null)
            {
                HUDManager.Instance.SetHealth(currentHealth, maxHealth);
                HUDManager.Instance.SetStamina(currentStamina, maxStamina);
            }
        }

        private void Die()
        {
            isDead = true;
            OnDeath?.Invoke();

            if (HUDManager.Instance != null)
            {
                HUDManager.Instance.AddNotification("You died...", 5f);
            }

            Debug.Log("Player died!");
            
            // TODO: Show death screen, offer respawn
        }

        public void Revive(float healthRestored = 0.5f)
        {
            isDead = false;
            currentHealth = maxHealth * healthRestored;
            currentStamina = maxStamina * 0.5f;
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
            OnStaminaChanged?.Invoke(currentStamina, maxStamina);
        }

        public void SetHealth(float health)
        {
            currentHealth = Mathf.Clamp(health, 0, maxHealth);
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
        }

        public void SetMaxHealth(float newMaxHealth)
        {
            maxHealth = newMaxHealth;
            currentHealth = Mathf.Min(currentHealth, maxHealth);
        }

        public float GetHealthPercentage() => currentHealth / maxHealth;
        public float GetStaminaPercentage() => currentStamina / maxStamina;
    }
}
