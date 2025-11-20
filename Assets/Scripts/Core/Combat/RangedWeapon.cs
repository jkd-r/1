using UnityEngine;

namespace ProtocolEMR.Core.Combat
{
    /// <summary>
    /// Ranged weapon implementation with projectile support.
    /// Handles firing, projectile spawning, and hit detection for ranged attacks.
    /// </summary>
    public class RangedWeapon : Weapon
    {
        [Header("Ranged Settings")]
        [SerializeField] private GameObject projectilePrefab;
        [SerializeField] private Transform muzzlePoint;
        [SerializeField] private float projectileSpeed = 50f;
        [SerializeField] private float projectileSpread = 0.05f;
        [SerializeField] private int ammoCount = -1;
        [SerializeField] private int ammoCapacity = 30;

        [Header("Attack Settings")]
        [SerializeField] private LayerMask targetLayers = -1;
        [SerializeField] private bool isAutomatic = false;
        [SerializeField] private float fireRate = 0.1f;

        [Header("Recoil Settings")]
        [SerializeField] private float recoilForce = 1f;
        [SerializeField] private float recoilDuration = 0.2f;

        [Header("References")]
        [SerializeField] private HitDetection hitDetection;
        [SerializeField] private AudioSource fireSound;

        private float lastFireTime = -999f;
        private float ammoPercentage = 1f;
        private bool isFiring = false;

        public int AmmoCount => ammoCount;
        public int AmmoCapacity => ammoCapacity;
        public float AmmoPercentage => ammoCapacity > 0 ? (float)ammoCount / ammoCapacity : 1f;
        public bool HasAmmo => ammoCount != 0;

        public event CombatSystem.IntEvent OnAmmoChanged;

        protected override void Awake()
        {
            base.Awake();
            if (hitDetection == null)
                hitDetection = GetComponent<HitDetection>();
            if (hitDetection == null)
                hitDetection = GetComponentInParent<HitDetection>();
            if (muzzlePoint == null)
                muzzlePoint = transform;
            if (fireSound == null)
                fireSound = GetComponent<AudioSource>();
        }

        private void Update()
        {
            if (isFiring && isAutomatic && CanAttack && HasAmmo)
            {
                PerformFire();
            }
        }

        /// <summary>
        /// Performs a ranged attack.
        /// </summary>
        public override void Attack()
        {
            if (!CanAttack || !HasAmmo)
                return;

            isFiring = true;
            OnAttackStart();
            PerformFire();
        }

        /// <summary>
        /// Stops the current attack.
        /// </summary>
        public override void StopAttack()
        {
            isFiring = false;
            base.StopAttack();
        }

        /// <summary>
        /// Performs the actual firing logic.
        /// </summary>
        private void PerformFire()
        {
            if (Time.time - lastFireTime < fireRate)
                return;

            lastFireTime = Time.time;
            lastAttackTime = Time.time;

            if (animator != null)
            {
                animator.SetTrigger("Fire");
            }

            if (projectilePrefab != null)
            {
                FireProjectile();
            }
            else
            {
                FireRaycast();
            }

            ammoCount--;
            if (ammoCount < 0)
                ammoCount = -1;

            OnAmmoChanged?.Invoke(ammoCount);
            PlayFireSound();

            if (!isAutomatic)
            {
                OnAttackEnd();
            }
        }

        /// <summary>
        /// Fires a projectile.
        /// </summary>
        private void FireProjectile()
        {
            Vector3 spreadDirection = muzzlePoint.forward + Random.insideUnitSphere * projectileSpread;
            spreadDirection.Normalize();

            GameObject projectileInstance = Instantiate(projectilePrefab, muzzlePoint.position, muzzlePoint.rotation);
            Rigidbody projectileRb = projectileInstance.GetComponent<Rigidbody>();

            if (projectileRb != null)
            {
                projectileRb.velocity = spreadDirection * projectileSpeed;
            }

            Projectile projectile = projectileInstance.GetComponent<Projectile>();
            if (projectile != null)
            {
                projectile.SetDamage(baseDamage);
                projectile.SetKnockbackForce(knockbackForce);
                projectile.SetOwner(gameObject);
            }
        }

        /// <summary>
        /// Fires using raycast hit detection instead of projectiles.
        /// </summary>
        private void FireRaycast()
        {
            Vector3 rayDirection = muzzlePoint.forward + Random.insideUnitSphere * projectileSpread;
            rayDirection.Normalize();

            if (hitDetection != null && hitDetection.DetectRaycastHit(muzzlePoint.position, rayDirection, 1000f, targetLayers, out RaycastHit hitInfo))
            {
                Damageable damageable = hitInfo.collider.GetComponent<Damageable>();
                if (damageable != null)
                {
                    Vector3 knockbackDirection = rayDirection;
                    damageable.TakeDamage(baseDamage, knockbackForce, knockbackDirection, gameObject);
                    OnHit(damageable);
                }
            }
        }

        /// <summary>
        /// Reloads the weapon.
        /// </summary>
        public void Reload()
        {
            ammoCount = ammoCapacity;
            OnAmmoChanged?.Invoke(ammoCount);

            if (animator != null)
            {
                animator.SetTrigger("Reload");
            }
        }

        /// <summary>
        /// Sets ammo count.
        /// </summary>
        /// <param name="count">New ammo count (-1 for infinite)</param>
        public void SetAmmo(int count)
        {
            ammoCount = count;
            OnAmmoChanged?.Invoke(ammoCount);
        }

        /// <summary>
        /// Adds ammo.
        /// </summary>
        /// <param name="amount">Amount of ammo to add</param>
        public void AddAmmo(int amount)
        {
            if (ammoCount >= 0)
            {
                ammoCount = Mathf.Min(ammoCount + amount, ammoCapacity);
            }
            OnAmmoChanged?.Invoke(ammoCount);
        }

        /// <summary>
        /// Plays the fire sound.
        /// </summary>
        private void PlayFireSound()
        {
            if (fireSound != null)
            {
                fireSound.PlayOneShot(fireSound.clip);
            }
        }

        /// <summary>
        /// Draws debug gizmos for aiming.
        /// </summary>
        private void OnDrawGizmosSelected()
        {
            Transform origin = muzzlePoint != null ? muzzlePoint : transform;
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(origin.position, origin.position + origin.forward * 10f);
            Gizmos.DrawWireSphere(origin.position, 0.2f);
        }
    }
}
