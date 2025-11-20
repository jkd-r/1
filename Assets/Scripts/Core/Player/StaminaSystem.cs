using UnityEngine;
using System;

namespace ProtocolEMR.Core.Player
{
    /// <summary>
    /// Manages player stamina for sprinting and other stamina-dependent actions.
    /// Provides drain and regeneration with configurable delays and rates.
    /// Supports stamina events for UI and other systems.
    /// </summary>
    public class StaminaSystem : MonoBehaviour
    {
        [Header("Stamina Settings")]
        [SerializeField] private float maxStamina = 100f;
        [SerializeField] private float staminaDrainRate = 25f;
        [SerializeField] private float staminaRegenRate = 15f;
        [SerializeField] private float staminaRegenDelay = 1.0f;

        [Header("Stamina Actions")]
        [SerializeField] private float sprintStaminaCost = 1.0f;
        [SerializeField] private float jumpStaminaCost = 5f;
        [SerializeField] private bool enableJumpStaminaCost = false;

        private float currentStamina;
        private float staminaRegenTimer = 0f;
        private bool isSprinting = false;
        private bool isExhausted = false;

        public float CurrentStamina => currentStamina;
        public float MaxStamina => maxStamina;
        public float StaminaPercentage => maxStamina > 0 ? currentStamina / maxStamina : 1f;
        public bool IsSprinting => isSprinting;
        public bool IsExhausted => isExhausted;

        public event Action<float> OnStaminaChanged;
        public event Action OnStaminaDepleted;
        public event Action OnStaminaRestored;

        private void Awake()
        {
            currentStamina = maxStamina;
        }

        /// <summary>
        /// Update stamina each frame based on current state.
        /// </summary>
        public void UpdateStamina(float deltaTime)
        {
            if (isSprinting)
            {
                DrainStamina(deltaTime);
            }
            else
            {
                RegenerateStamina(deltaTime);
            }
        }

        /// <summary>
        /// Set sprint state and begin draining stamina.
        /// </summary>
        public void SetSprinting(bool sprinting)
        {
            if (sprinting && currentStamina <= 0f)
            {
                isSprinting = false;
                return;
            }

            isSprinting = sprinting;

            if (!sprinting)
            {
                staminaRegenTimer = staminaRegenDelay;
            }
        }

        /// <summary>
        /// Drain stamina for sprint or other actions.
        /// </summary>
        private void DrainStamina(float deltaTime)
        {
            float drainAmount = staminaDrainRate * sprintStaminaCost * deltaTime;
            float previousStamina = currentStamina;

            currentStamina -= drainAmount;
            currentStamina = Mathf.Max(currentStamina, 0f);

            if (currentStamina <= 0f && previousStamina > 0f)
            {
                OnStaminaDepleted?.Invoke();
                isSprinting = false;
                isExhausted = true;
                staminaRegenTimer = staminaRegenDelay;
            }

            OnStaminaChanged?.Invoke(currentStamina);
        }

        /// <summary>
        /// Regenerate stamina when not sprinting.
        /// </summary>
        private void RegenerateStamina(float deltaTime)
        {
            // Wait for regeneration delay
            if (staminaRegenTimer > 0f)
            {
                staminaRegenTimer -= deltaTime;
                return;
            }

            float previousStamina = currentStamina;
            currentStamina += staminaRegenRate * deltaTime;
            currentStamina = Mathf.Min(currentStamina, maxStamina);

            // Restore from exhausted state when stamina recovers enough
            if (isExhausted && currentStamina >= maxStamina * 0.5f)
            {
                isExhausted = false;
                OnStaminaRestored?.Invoke();
            }

            if (currentStamina > previousStamina)
            {
                OnStaminaChanged?.Invoke(currentStamina);
            }
        }

        /// <summary>
        /// Attempt to use stamina for an action (like jumping).
        /// Returns true if successful, false if not enough stamina.
        /// </summary>
        public bool TryConsumeStamina(float amount)
        {
            if (currentStamina >= amount)
            {
                currentStamina -= amount;
                OnStaminaChanged?.Invoke(currentStamina);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Attempt to jump with stamina cost.
        /// </summary>
        public bool TryConsumeJumpStamina()
        {
            if (!enableJumpStaminaCost)
                return true;

            return TryConsumeStamina(jumpStaminaCost);
        }

        /// <summary>
        /// Instantly restores stamina to full.
        /// </summary>
        public void RestoreStamina()
        {
            currentStamina = maxStamina;
            isExhausted = false;
            staminaRegenTimer = 0f;
            OnStaminaRestored?.Invoke();
            OnStaminaChanged?.Invoke(currentStamina);
        }

        /// <summary>
        /// Partially restore stamina.
        /// </summary>
        public void RestoreStamina(float amount)
        {
            float previousStamina = currentStamina;
            currentStamina = Mathf.Min(currentStamina + amount, maxStamina);

            if (isExhausted && currentStamina >= maxStamina * 0.5f)
            {
                isExhausted = false;
                OnStaminaRestored?.Invoke();
            }

            if (currentStamina > previousStamina)
            {
                OnStaminaChanged?.Invoke(currentStamina);
            }
        }

        /// <summary>
        /// Reset stamina system (for respawn).
        /// </summary>
        public void ResetStamina()
        {
            currentStamina = maxStamina;
            isExhausted = false;
            isSprinting = false;
            staminaRegenTimer = 0f;
            OnStaminaChanged?.Invoke(currentStamina);
        }

        /// <summary>
        /// Check if player can currently sprint.
        /// </summary>
        public bool CanSprint()
        {
            return currentStamina > 0f && !isExhausted;
        }

        /// <summary>
        /// Get stamina as a percentage (0-1).
        /// </summary>
        public float GetStaminaPercentage()
        {
            return StaminaPercentage;
        }
    }
}
