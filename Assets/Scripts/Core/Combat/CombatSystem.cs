using UnityEngine;
using System;
using ProtocolEMR.Core.Input;
using ProtocolEMR.Core.Player;

namespace ProtocolEMR.Core.Combat
{
    /// <summary>
    /// Main combat system orchestrator that coordinates weapon usage, attacks, and combat feedback.
    /// Integrates with player input, health systems, and stamina management.
    /// </summary>
    public class CombatSystem : MonoBehaviour
    {
        public delegate void IntEvent(int value);

        [Header("Combat Setup")]
        [SerializeField] private Weapon meleeWeapon;
        [SerializeField] private Weapon rangedWeapon;
        [SerializeField] private Weapon currentWeapon;

        [Header("Player References")]
        [SerializeField] private Damageable playerDamageable;
        [SerializeField] private HealthSystem playerHealth;
        [SerializeField] private PlayerController playerController;

        [Header("Combat Feedback")]
        [SerializeField] private CombatFeedback combatFeedback;

        [Header("Settings")]
        [SerializeField] private float weaponSwitchCooldown = 0.3f;
        [SerializeField] private bool enableAutomaticFire = false;

        private float lastWeaponSwitchTime = -999f;
        private bool isCombatActive = false;

        public Weapon CurrentWeapon => currentWeapon;
        public Weapon MeleeWeapon => meleeWeapon;
        public Weapon RangedWeapon => rangedWeapon;
        public bool IsCombatActive => isCombatActive;

        public event Action OnCombatStarted;
        public event Action OnCombatEnded;
        public event Action<Weapon> OnWeaponSwitched;
        public event Action<float> OnDamageDealt;

        private void Awake()
        {
            if (playerDamageable == null)
                playerDamageable = GetComponent<Damageable>();
            if (playerHealth == null)
                playerHealth = GetComponent<HealthSystem>();
            if (playerController == null)
                playerController = GetComponent<PlayerController>();
            if (combatFeedback == null)
                combatFeedback = GetComponent<CombatFeedback>();

            if (currentWeapon == null && meleeWeapon != null)
                currentWeapon = meleeWeapon;
        }

        private void Start()
        {
            if (InputManager.Instance != null)
            {
                InputManager.Instance.OnFire += OnFireInput;
                InputManager.Instance.OnFireReleased += OnFireReleasedInput;
                InputManager.Instance.OnAimPressed += OnAimPressedInput;
                InputManager.Instance.OnAimReleased += OnAimReleasedInput;
            }

            if (meleeWeapon != null)
            {
                meleeWeapon.OnHitTarget += OnWeaponHit;
            }
            if (rangedWeapon != null)
            {
                rangedWeapon.OnHitTarget += OnWeaponHit;
            }

            if (playerDamageable != null)
            {
                playerDamageable.OnDamageTaken += OnPlayerDamageTaken;
                playerDamageable.OnKnockbackApplied += OnPlayerKnockback;
            }
        }

        private void OnDestroy()
        {
            if (InputManager.Instance != null)
            {
                InputManager.Instance.OnFire -= OnFireInput;
                InputManager.Instance.OnFireReleased -= OnFireReleasedInput;
                InputManager.Instance.OnAimPressed -= OnAimPressedInput;
                InputManager.Instance.OnAimReleased -= OnAimReleasedInput;
            }

            if (meleeWeapon != null)
            {
                meleeWeapon.OnHitTarget -= OnWeaponHit;
            }
            if (rangedWeapon != null)
            {
                rangedWeapon.OnHitTarget -= OnWeaponHit;
            }

            if (playerDamageable != null)
            {
                playerDamageable.OnDamageTaken -= OnPlayerDamageTaken;
                playerDamageable.OnKnockbackApplied -= OnPlayerKnockback;
            }
        }

        /// <summary>
        /// Handles fire input from player.
        /// </summary>
        private void OnFireInput()
        {
            if (currentWeapon == null)
                return;

            if (playerController != null && !playerController.ConsumesStamina(currentWeapon.StaminaCost))
                return;

            currentWeapon.Attack();
            isCombatActive = true;
            OnCombatStarted?.Invoke();
        }

        /// <summary>
        /// Handles fire release input from player.
        /// </summary>
        private void OnFireReleasedInput()
        {
            if (currentWeapon == null)
                return;

            currentWeapon.StopAttack();
        }

        /// <summary>
        /// Handles aim pressed input.
        /// </summary>
        private void OnAimPressedInput()
        {
            if (rangedWeapon != null)
            {
                SwitchWeapon(rangedWeapon);
            }
        }

        /// <summary>
        /// Handles aim released input.
        /// </summary>
        private void OnAimReleasedInput()
        {
            if (meleeWeapon != null)
            {
                SwitchWeapon(meleeWeapon);
            }
        }

        /// <summary>
        /// Switches to a different weapon.
        /// </summary>
        /// <param name="newWeapon">Weapon to switch to</param>
        public void SwitchWeapon(Weapon newWeapon)
        {
            if (Time.time - lastWeaponSwitchTime < weaponSwitchCooldown)
                return;

            if (newWeapon == currentWeapon)
                return;

            currentWeapon = newWeapon;
            lastWeaponSwitchTime = Time.time;
            OnWeaponSwitched?.Invoke(currentWeapon);
        }

        /// <summary>
        /// Called when the equipped weapon hits a target.
        /// </summary>
        /// <param name="target">Target that was hit</param>
        private void OnWeaponHit(Damageable target)
        {
            if (target == null)
                return;

            OnDamageDealt?.Invoke(currentWeapon.BaseDamage);

            if (combatFeedback != null)
            {
                combatFeedback.DisplayDamage(currentWeapon.BaseDamage, target.transform.position);
            }
        }

        /// <summary>
        /// Called when player takes damage.
        /// </summary>
        /// <param name="damage">Amount of damage taken</param>
        private void OnPlayerDamageTaken(float damage)
        {
            if (combatFeedback != null)
            {
                combatFeedback.ApplyScreenShake(Mathf.Clamp01(damage / 50f));
            }
        }

        /// <summary>
        /// Called when player is knocked back.
        /// </summary>
        /// <param name="direction">Direction of knockback</param>
        /// <param name="force">Force of knockback</param>
        private void OnPlayerKnockback(Vector3 direction, float force)
        {
            if (combatFeedback != null)
            {
                combatFeedback.ApplyScreenShake(Mathf.Clamp01(force / 200f));
            }
        }

        /// <summary>
        /// Gets the current weapon's ammo count (if applicable).
        /// </summary>
        /// <returns>Ammo count or -1 if melee weapon</returns>
        public int GetCurrentAmmo()
        {
            if (currentWeapon is RangedWeapon rangedWeapon)
            {
                return rangedWeapon.AmmoCount;
            }
            return -1;
        }

        /// <summary>
        /// Gets the current weapon's cooldown percentage.
        /// </summary>
        /// <returns>Cooldown progress (0-1)</returns>
        public float GetWeaponCooldown()
        {
            return currentWeapon != null ? currentWeapon.GetCooldownPercentage() : 1f;
        }

        /// <summary>
        /// Reloads the current weapon if applicable.
        /// </summary>
        public void ReloadCurrentWeapon()
        {
            if (currentWeapon is RangedWeapon rangedWeapon)
            {
                rangedWeapon.Reload();
            }
        }

        /// <summary>
        /// Sets the combat active state.
        /// </summary>
        /// <param name="active">Whether combat is active</param>
        public void SetCombatActive(bool active)
        {
            bool wasActive = isCombatActive;
            isCombatActive = active;

            if (!wasActive && active)
            {
                OnCombatStarted?.Invoke();
            }
            else if (wasActive && !active)
            {
                OnCombatEnded?.Invoke();
            }
        }

        /// <summary>
        /// Gets whether the current weapon can attack.
        /// </summary>
        /// <returns>True if weapon can attack</returns>
        public bool CanAttack()
        {
            return currentWeapon != null && currentWeapon.CanAttack;
        }
    }
}
