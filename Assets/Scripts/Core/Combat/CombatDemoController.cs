using UnityEngine;

namespace ProtocolEMR.Core.Combat
{
    /// <summary>
    /// Demo controller for testing and validating the combat system.
    /// Provides debug UI and testing capabilities for combat mechanics.
    /// </summary>
    public class CombatDemoController : MonoBehaviour
    {
        [Header("Combat System")]
        [SerializeField] private CombatSystem combatSystem;
        [SerializeField] private Damageable playerDamageable;

        [Header("Test Targets")]
        [SerializeField] private Damageable[] testTargets;

        [Header("Debug Settings")]
        [SerializeField] private bool showDebugUI = true;
        [SerializeField] private bool debugHitDetection = false;

        private GUILayout guiLayout;

        private void Awake()
        {
            if (combatSystem == null)
                combatSystem = GetComponent<CombatSystem>();
            if (playerDamageable == null)
                playerDamageable = GetComponent<Damageable>();
        }

        private void Update()
        {
            HandleDebugInput();
        }

        /// <summary>
        /// Handles keyboard input for debugging.
        /// </summary>
        private void HandleDebugInput()
        {
            if (Input.GetKeyDown(KeyCode.H))
            {
                if (playerDamageable != null)
                {
                    playerDamageable.Heal(20f);
                    Debug.Log($"Player healed. Health: {playerDamageable.CurrentHealth}/{playerDamageable.MaxHealth}");
                }
            }

            if (Input.GetKeyDown(KeyCode.K))
            {
                if (playerDamageable != null)
                {
                    playerDamageable.TakeDamage(10f);
                    Debug.Log($"Player took damage. Health: {playerDamageable.CurrentHealth}/{playerDamageable.MaxHealth}");
                }
            }

            if (Input.GetKeyDown(KeyCode.R))
            {
                if (playerDamageable != null)
                {
                    playerDamageable.FullHeal();
                    playerDamageable.Revive(1f);
                    Debug.Log("Player revived and healed");
                }
            }

            if (Input.GetKeyDown(KeyCode.M))
            {
                if (combatSystem != null && combatSystem.MeleeWeapon != null)
                {
                    combatSystem.SwitchWeapon(combatSystem.MeleeWeapon);
                    Debug.Log("Switched to melee weapon");
                }
            }

            if (Input.GetKeyDown(KeyCode.G))
            {
                if (combatSystem != null && combatSystem.RangedWeapon != null)
                {
                    combatSystem.SwitchWeapon(combatSystem.RangedWeapon);
                    Debug.Log("Switched to ranged weapon");
                }
            }

            if (Input.GetKeyDown(KeyCode.T))
            {
                DamageTestTargets(25f);
            }

            if (Input.GetKeyDown(KeyCode.D))
            {
                debugHitDetection = !debugHitDetection;
                Debug.Log($"Hit detection debug draw: {(debugHitDetection ? "ON" : "OFF")}");
            }
        }

        /// <summary>
        /// Damages all test targets.
        /// </summary>
        /// <param name="damage">Amount of damage to apply</param>
        private void DamageTestTargets(float damage)
        {
            if (testTargets == null || testTargets.Length == 0)
            {
                Debug.LogWarning("No test targets assigned");
                return;
            }

            foreach (Damageable target in testTargets)
            {
                if (target != null)
                {
                    target.TakeDamage(damage, 50f, Vector3.forward);
                    Debug.Log($"Target {target.name} took {damage} damage. Health: {target.CurrentHealth}/{target.MaxHealth}");
                }
            }
        }

        /// <summary>
        /// Draws debug UI.
        /// </summary>
        private void OnGUI()
        {
            if (!showDebugUI)
                return;

            GUILayout.BeginArea(new Rect(10, 10, 400, 400));
            GUILayout.Box("Combat System Debug Info");

            if (combatSystem != null)
            {
                GUILayout.Label($"Current Weapon: {(combatSystem.CurrentWeapon != null ? combatSystem.CurrentWeapon.WeaponName : "None")}");
                GUILayout.Label($"Can Attack: {combatSystem.CanAttack()}");
                GUILayout.Label($"Weapon Cooldown: {combatSystem.GetWeaponCooldown() * 100:F0}%");

                if (combatSystem.CurrentWeapon is RangedWeapon rangedWeapon)
                {
                    GUILayout.Label($"Ammo: {rangedWeapon.AmmoCount}");
                    if (GUILayout.Button("Reload"))
                    {
                        combatSystem.ReloadCurrentWeapon();
                    }
                }
            }

            GUILayout.Space(10);

            if (playerDamageable != null)
            {
                GUILayout.Label($"Player Health: {playerDamageable.CurrentHealth:F0}/{playerDamageable.MaxHealth:F0}");
                GUILayout.Label($"Health %: {playerDamageable.HealthPercentage * 100:F0}%");
                GUILayout.Label($"Is Dead: {playerDamageable.IsDead}");
                GUILayout.Label($"Is Knocked Back: {playerDamageable.IsKnockedBack}");

                GUILayout.Space(5);

                if (GUILayout.Button("Test Damage (10)"))
                {
                    playerDamageable.TakeDamage(10f);
                }
                if (GUILayout.Button("Test Knockback"))
                {
                    playerDamageable.ApplyKnockback(100f, Vector3.forward);
                }
                if (GUILayout.Button("Heal (20)"))
                {
                    playerDamageable.Heal(20f);
                }
                if (GUILayout.Button("Full Heal"))
                {
                    playerDamageable.FullHeal();
                }
            }

            GUILayout.Space(10);
            GUILayout.Label("Debug Keys:");
            GUILayout.Label("H - Heal 20");
            GUILayout.Label("K - Take 10 damage");
            GUILayout.Label("R - Revive/Full Heal");
            GUILayout.Label("M - Switch to Melee");
            GUILayout.Label("G - Switch to Ranged");
            GUILayout.Label("T - Damage All Targets");
            GUILayout.Label("D - Toggle Hit Detection Debug");

            GUILayout.EndArea();
        }
    }
}
