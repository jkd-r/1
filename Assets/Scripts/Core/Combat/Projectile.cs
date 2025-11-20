using UnityEngine;
using System;

namespace ProtocolEMR.Core.Combat
{
    /// <summary>
    /// Projectile component for ranged weapons. Handles collision detection, damage application, and lifecycle.
    /// </summary>
    public class Projectile : MonoBehaviour
    {
        [Header("Projectile Settings")]
        [SerializeField] private float lifetime = 30f;
        [SerializeField] private float damage = 20f;
        [SerializeField] private float knockbackForce = 50f;
        [SerializeField] private LayerMask targetLayers = -1;

        [Header("Physics")]
        [SerializeField] private bool useGravity = true;
        [SerializeField] private float gravityScale = 1f;

        [Header("Impact")]
        [SerializeField] private GameObject impactEffectPrefab;
        [SerializeField] private AudioClip impactSound;

        private Rigidbody rigidBody;
        private GameObject owner;
        private float spawnTime;
        private bool hasHit = false;

        public event Action<Collider, Vector3> OnProjectileHit;

        private void Awake()
        {
            rigidBody = GetComponent<Rigidbody>();
            if (rigidBody == null)
            {
                rigidBody = gameObject.AddComponent<Rigidbody>();
            }

            rigidBody.useGravity = useGravity;
            spawnTime = Time.time;
        }

        private void Update()
        {
            if (useGravity && rigidBody != null)
            {
                rigidBody.AddForce(Physics.gravity * (gravityScale - 1f), ForceMode.Acceleration);
            }

            if (Time.time - spawnTime > lifetime)
            {
                Destroy(gameObject);
            }
        }

        private void OnTriggerEnter(Collider collision)
        {
            OnCollide(collision);
        }

        private void OnCollisionEnter(Collision collision)
        {
            OnCollide(collision.gameObject.GetComponent<Collider>());
        }

        /// <summary>
        /// Handles collision with objects.
        /// </summary>
        /// <param name="collision">Collider that was hit</param>
        private void OnCollide(Collider collision)
        {
            if (hasHit)
                return;

            if (owner != null && collision.gameObject == owner)
                return;

            if ((targetLayers.value & (1 << collision.gameObject.layer)) == 0)
                return;

            hasHit = true;

            Damageable damageable = collision.GetComponent<Damageable>();
            if (damageable != null)
            {
                Vector3 knockbackDirection = rigidBody != null ? rigidBody.velocity.normalized : transform.forward;
                damageable.TakeDamage(damage, knockbackForce, knockbackDirection, owner);
                OnProjectileHit?.Invoke(collision, transform.position);
            }

            CreateImpactEffect(collision);
            PlayImpactSound();

            Destroy(gameObject);
        }

        /// <summary>
        /// Creates impact effect at collision point.
        /// </summary>
        /// <param name="collision">Collider that was hit</param>
        private void CreateImpactEffect(Collider collision)
        {
            if (impactEffectPrefab == null)
                return;

            GameObject effectInstance = Instantiate(impactEffectPrefab, transform.position, Quaternion.identity);
            ParticleSystem particleSystem = effectInstance.GetComponent<ParticleSystem>();
            if (particleSystem != null)
            {
                particleSystem.Play();
            }

            Destroy(effectInstance, 3f);
        }

        /// <summary>
        /// Plays impact sound.
        /// </summary>
        private void PlayImpactSound()
        {
            if (impactSound == null)
                return;

            AudioSource.PlayClipAtPoint(impactSound, transform.position, 1f);
        }

        /// <summary>
        /// Sets projectile damage.
        /// </summary>
        /// <param name="newDamage">New damage value</param>
        public void SetDamage(float newDamage)
        {
            damage = newDamage;
        }

        /// <summary>
        /// Sets projectile knockback force.
        /// </summary>
        /// <param name="force">New knockback force</param>
        public void SetKnockbackForce(float force)
        {
            knockbackForce = force;
        }

        /// <summary>
        /// Sets the owner of this projectile.
        /// </summary>
        /// <param name="newOwner">Owner game object</param>
        public void SetOwner(GameObject newOwner)
        {
            owner = newOwner;
        }

        /// <summary>
        /// Gets projectile velocity.
        /// </summary>
        /// <returns>Current velocity</returns>
        public Vector3 GetVelocity()
        {
            return rigidBody != null ? rigidBody.velocity : Vector3.zero;
        }

        /// <summary>
        /// Draws debug gizmos for trajectory.
        /// </summary>
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, 0.1f);

            if (rigidBody != null && rigidBody.velocity.magnitude > 0.1f)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawLine(transform.position, transform.position + rigidBody.velocity.normalized * 2f);
            }
        }
    }
}
