using UnityEngine;
using System.Collections.Generic;

namespace ProtocolEMR.Combat.Core
{
    public enum WeaponType
    {
        Fist,
        Wrench,
        Crowbar,
        Pipe,
        Taser,
        PlasmaRifle
    }

    [System.Serializable]
    public class WeaponData
    {
        public WeaponType type;
        public string weaponName;
        public float damage;
        public float attackSpeed;
        public int maxDurability;
        public int currentDurability;
        public int maxAmmo;
        public int currentAmmo;
        public float cooldown;
        public GameObject weaponModel;
        public bool isRanged;
    }

    public class WeaponManager : MonoBehaviour
    {
        [Header("Weapon Definitions")]
        [SerializeField] private List<WeaponData> availableWeapons = new List<WeaponData>();

        [Header("Current Weapon")]
        [SerializeField] private WeaponType currentWeaponType = WeaponType.Fist;
        private WeaponData currentWeapon;

        [Header("Weapon Slots")]
        private Dictionary<int, WeaponType> weaponSlots = new Dictionary<int, WeaponType>();

        [Header("Ranged Weapon Settings")]
        [SerializeField] private float taserStunDuration = 5f;
        [SerializeField] private float taserCooldown = 1.5f;
        [SerializeField] private float plasmaCooldown = 1f;
        [SerializeField] private float reloadDuration = 2f;
        [SerializeField] private LayerMask targetLayer;

        [Header("References")]
        [SerializeField] private Camera playerCamera;
        [SerializeField] private Transform weaponHolder;
        [SerializeField] private GameObject muzzleFlashPrefab;
        [SerializeField] private GameObject shellEjectionPrefab;

        private CombatAudioManager audioManager;
        private CombatFeedbackSystem feedbackSystem;
        private float lastShotTime = 0f;
        private bool isReloading = false;

        void Awake()
        {
            audioManager = GetComponent<CombatAudioManager>();
            feedbackSystem = GetComponent<CombatFeedbackSystem>();

            if (playerCamera == null)
                playerCamera = Camera.main;

            InitializeWeapons();
            InitializeWeaponSlots();
        }

        void Update()
        {
            HandleWeaponSwitching();
            HandleRangedWeaponInput();
        }

        private void InitializeWeapons()
        {
            availableWeapons = new List<WeaponData>
            {
                new WeaponData
                {
                    type = WeaponType.Fist,
                    weaponName = "Fist",
                    damage = 10f,
                    attackSpeed = 0.6f,
                    maxDurability = -1,
                    currentDurability = -1,
                    isRanged = false
                },
                new WeaponData
                {
                    type = WeaponType.Wrench,
                    weaponName = "Wrench",
                    damage = 15f,
                    attackSpeed = 0.5f,
                    maxDurability = 100,
                    currentDurability = 100,
                    isRanged = false
                },
                new WeaponData
                {
                    type = WeaponType.Crowbar,
                    weaponName = "Crowbar",
                    damage = 20f,
                    attackSpeed = 0.7f,
                    maxDurability = 100,
                    currentDurability = 100,
                    isRanged = false
                },
                new WeaponData
                {
                    type = WeaponType.Pipe,
                    weaponName = "Pipe",
                    damage = 18f,
                    attackSpeed = 0.6f,
                    maxDurability = 100,
                    currentDurability = 100,
                    isRanged = false
                },
                new WeaponData
                {
                    type = WeaponType.Taser,
                    weaponName = "Taser",
                    damage = 0f,
                    attackSpeed = 0f,
                    maxAmmo = 20,
                    currentAmmo = 20,
                    cooldown = taserCooldown,
                    isRanged = true
                },
                new WeaponData
                {
                    type = WeaponType.PlasmaRifle,
                    weaponName = "Plasma Rifle",
                    damage = 25f,
                    attackSpeed = 0f,
                    maxAmmo = 15,
                    currentAmmo = 15,
                    cooldown = plasmaCooldown,
                    isRanged = true
                }
            };

            currentWeapon = availableWeapons[0];
        }

        private void InitializeWeaponSlots()
        {
            weaponSlots[1] = WeaponType.Fist;
        }

        private void HandleWeaponSwitching()
        {
            if (isReloading)
                return;

            for (int i = 1; i <= 7; i++)
            {
                if (Input.GetKeyDown(KeyCode.Alpha0 + i))
                {
                    if (weaponSlots.ContainsKey(i))
                    {
                        SwitchToWeapon(weaponSlots[i]);
                    }
                }
            }
        }

        public void SwitchToWeapon(WeaponType weaponType)
        {
            WeaponData weapon = availableWeapons.Find(w => w.type == weaponType);
            if (weapon != null)
            {
                currentWeapon = weapon;
                currentWeaponType = weaponType;

                UpdateWeaponModel();

                if (audioManager != null)
                    audioManager.PlayWeaponSwitchSound();
            }
        }

        private void UpdateWeaponModel()
        {
            if (weaponHolder == null)
                return;

            foreach (Transform child in weaponHolder)
            {
                child.gameObject.SetActive(false);
            }

            if (currentWeapon.weaponModel != null)
            {
                currentWeapon.weaponModel.SetActive(true);
            }
        }

        private void HandleRangedWeaponInput()
        {
            if (!currentWeapon.isRanged)
                return;

            if (isReloading)
                return;

            if (Input.GetMouseButtonDown(0))
            {
                FireRangedWeapon();
            }

            if (Input.GetKeyDown(KeyCode.R))
            {
                ReloadWeapon();
            }
        }

        private void FireRangedWeapon()
        {
            if (Time.time - lastShotTime < currentWeapon.cooldown)
                return;

            if (currentWeapon.currentAmmo <= 0)
            {
                if (audioManager != null)
                    audioManager.PlayEmptyClickSound();
                return;
            }

            lastShotTime = Time.time;
            currentWeapon.currentAmmo--;

            Vector3 rayOrigin = playerCamera.transform.position;
            Vector3 rayDirection = playerCamera.transform.forward;

            if (Physics.Raycast(rayOrigin, rayDirection, out RaycastHit hit, 100f, targetLayer))
            {
                ProcessRangedHit(hit);
            }

            SpawnMuzzleFlash();
            SpawnShellEjection();

            if (audioManager != null)
            {
                if (currentWeaponType == WeaponType.Taser)
                    audioManager.PlayTaserSound();
                else if (currentWeaponType == WeaponType.PlasmaRifle)
                    audioManager.PlayPlasmaSound();
            }

            if (feedbackSystem != null)
            {
                float intensity = currentWeaponType == WeaponType.Taser ? 0.3f : 0.5f;
                feedbackSystem.TriggerScreenShake(intensity);
            }
        }

        private void ProcessRangedHit(RaycastHit hit)
        {
            NPCCombatController npc = hit.collider.GetComponent<NPCCombatController>();
            if (npc != null)
            {
                if (currentWeaponType == WeaponType.Taser)
                {
                    npc.ApplyStun(taserStunDuration);
                }
                else if (currentWeaponType == WeaponType.PlasmaRifle)
                {
                    npc.TakeDamage(currentWeapon.damage, Vector3.zero, hit.point);
                }

                if (feedbackSystem != null)
                    feedbackSystem.OnHitSuccess(hit.point, currentWeapon.damage, AttackType.Punch);
            }
        }

        private void SpawnMuzzleFlash()
        {
            if (muzzleFlashPrefab != null && weaponHolder != null)
            {
                GameObject flash = Instantiate(muzzleFlashPrefab, weaponHolder.position, weaponHolder.rotation);
                Destroy(flash, 0.1f);
            }
        }

        private void SpawnShellEjection()
        {
            if (shellEjectionPrefab != null && weaponHolder != null)
            {
                GameObject shell = Instantiate(shellEjectionPrefab, weaponHolder.position, weaponHolder.rotation);
                Rigidbody rb = shell.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.AddForce(weaponHolder.right * 2f + weaponHolder.up * 1f, ForceMode.Impulse);
                }
                Destroy(shell, 3f);
            }
        }

        private void ReloadWeapon()
        {
            if (currentWeapon.currentAmmo >= currentWeapon.maxAmmo)
                return;

            if (!currentWeapon.isRanged)
                return;

            isReloading = true;

            if (audioManager != null)
                audioManager.PlayReloadSound();

            Invoke(nameof(CompleteReload), reloadDuration);
        }

        private void CompleteReload()
        {
            currentWeapon.currentAmmo = currentWeapon.maxAmmo;
            isReloading = false;
        }

        public bool PickupWeapon(WeaponType weaponType)
        {
            WeaponData weapon = availableWeapons.Find(w => w.type == weaponType);
            if (weapon == null)
                return false;

            int slot = GetNextAvailableSlot();
            if (slot == -1)
                return false;

            weaponSlots[slot] = weaponType;

            if (audioManager != null)
                audioManager.PlayPickupSound();

            return true;
        }

        private int GetNextAvailableSlot()
        {
            for (int i = 2; i <= 7; i++)
            {
                if (!weaponSlots.ContainsKey(i))
                    return i;
            }
            return -1;
        }

        public void ReduceWeaponDurability(int amount = 1)
        {
            if (currentWeapon.maxDurability == -1)
                return;

            currentWeapon.currentDurability -= amount;

            if (currentWeapon.currentDurability <= 0)
            {
                BreakWeapon();
            }
        }

        private void BreakWeapon()
        {
            int slotToRemove = -1;
            foreach (var slot in weaponSlots)
            {
                if (slot.Value == currentWeaponType)
                {
                    slotToRemove = slot.Key;
                    break;
                }
            }

            if (slotToRemove != -1)
                weaponSlots.Remove(slotToRemove);

            if (audioManager != null)
                audioManager.PlayWeaponBreakSound();

            SwitchToWeapon(WeaponType.Fist);
        }

        public void DropWeapon()
        {
            if (currentWeaponType == WeaponType.Fist)
                return;

            int slotToRemove = -1;
            foreach (var slot in weaponSlots)
            {
                if (slot.Value == currentWeaponType)
                {
                    slotToRemove = slot.Key;
                    break;
                }
            }

            if (slotToRemove != -1)
                weaponSlots.Remove(slotToRemove);

            SwitchToWeapon(WeaponType.Fist);
        }

        public WeaponData GetCurrentWeapon()
        {
            return currentWeapon;
        }

        public bool IsReloading()
        {
            return isReloading;
        }

        public float GetReloadProgress()
        {
            return isReloading ? 1f - (reloadDuration / 2f) : 0f;
        }
    }
}
