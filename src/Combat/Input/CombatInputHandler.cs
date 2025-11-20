using UnityEngine;

namespace ProtocolEMR.Combat.Core
{
    public class CombatInputHandler : MonoBehaviour
    {
        [Header("Combat Keys")]
        [SerializeField] private KeyCode primaryAttackKey = KeyCode.Mouse0;
        [SerializeField] private KeyCode secondaryAttackKey = KeyCode.Mouse1;
        [SerializeField] private KeyCode dodgeKey = KeyCode.Q;
        [SerializeField] private KeyCode parryKey = KeyCode.Q;
        [SerializeField] private KeyCode reloadKey = KeyCode.R;
        [SerializeField] private KeyCode dropWeaponKey = KeyCode.G;

        [Header("Weapon Slot Keys")]
        [SerializeField] private KeyCode[] weaponSlotKeys = new KeyCode[]
        {
            KeyCode.Alpha1,
            KeyCode.Alpha2,
            KeyCode.Alpha3,
            KeyCode.Alpha4,
            KeyCode.Alpha5,
            KeyCode.Alpha6,
            KeyCode.Alpha7
        };

        [Header("Integration Settings")]
        [SerializeField] private bool allowCombatWhileSprinting = false;
        [SerializeField] private bool allowCombatWhileJumping = false;

        [Header("References")]
        [SerializeField] private CombatManager combatManager;
        [SerializeField] private WeaponManager weaponManager;

        private bool isSprinting = false;
        private bool isJumping = false;

        void Awake()
        {
            if (combatManager == null)
                combatManager = GetComponent<CombatManager>();

            if (weaponManager == null)
                weaponManager = GetComponent<WeaponManager>();
        }

        void Update()
        {
            UpdateMovementState();
            HandleCombatInput();
        }

        private void UpdateMovementState()
        {
            isSprinting = Input.GetKey(KeyCode.LeftShift);
        }

        private void HandleCombatInput()
        {
            if (!CanPerformCombat())
                return;

            if (Input.GetKeyDown(dodgeKey))
            {
                if (combatManager != null)
                    combatManager.PerformDodge();
            }

            if (Input.GetMouseButtonDown(0))
            {
                if (combatManager != null)
                {
                    WeaponData currentWeapon = weaponManager?.GetCurrentWeapon();
                    if (currentWeapon != null && currentWeapon.isRanged)
                    {
                    }
                    else
                    {
                        combatManager.PerformAttack(AttackType.Punch);
                    }
                }
            }

            if (Input.GetMouseButtonDown(1))
            {
                if (combatManager != null)
                    combatManager.PerformAttack(AttackType.Kick);
            }

            if (Input.GetKeyDown(reloadKey))
            {
            }

            if (Input.GetKeyDown(dropWeaponKey))
            {
                if (weaponManager != null)
                    weaponManager.DropWeapon();
            }
        }

        private bool CanPerformCombat()
        {
            if (isSprinting && !allowCombatWhileSprinting)
                return false;

            if (isJumping && !allowCombatWhileJumping)
                return false;

            return true;
        }

        public void SetAllowCombatWhileSprinting(bool allow)
        {
            allowCombatWhileSprinting = allow;
        }

        public void SetAllowCombatWhileJumping(bool allow)
        {
            allowCombatWhileJumping = allow;
        }

        public void RebindKey(string actionName, KeyCode newKey)
        {
            switch (actionName)
            {
                case "PrimaryAttack":
                    primaryAttackKey = newKey;
                    break;
                case "SecondaryAttack":
                    secondaryAttackKey = newKey;
                    break;
                case "Dodge":
                    dodgeKey = newKey;
                    break;
                case "Reload":
                    reloadKey = newKey;
                    break;
                case "DropWeapon":
                    dropWeaponKey = newKey;
                    break;
            }
        }

        public KeyCode GetKeyBinding(string actionName)
        {
            switch (actionName)
            {
                case "PrimaryAttack":
                    return primaryAttackKey;
                case "SecondaryAttack":
                    return secondaryAttackKey;
                case "Dodge":
                    return dodgeKey;
                case "Reload":
                    return reloadKey;
                case "DropWeapon":
                    return dropWeaponKey;
                default:
                    return KeyCode.None;
            }
        }
    }
}
