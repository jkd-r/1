using UnityEngine;
using System;

namespace ProtocolEMR.Core.Combat
{
    /// <summary>
    /// Handles visual and audio feedback for combat actions including damage numbers, hit effects, and knockback visualization.
    /// </summary>
    public class CombatFeedback : MonoBehaviour
    {
        [Header("Damage Number Settings")]
        [SerializeField] private GameObject damageNumberPrefab;
        [SerializeField] private float damageNumberLifetime = 2f;
        [SerializeField] private Vector3 damageNumberOffset = Vector3.up * 1.5f;
        [SerializeField] private float damageNumberSpeed = 3f;

        [Header("Hit Effect Settings")]
        [SerializeField] private GameObject hitEffectPrefab;
        [SerializeField] private float hitEffectLifetime = 1f;
        [SerializeField] private AudioClip hitSoundClip;
        [SerializeField] private AudioClip heavyHitSoundClip;

        [Header("Screen Shake Settings")]
        [SerializeField] private bool enableScreenShake = true;
        [SerializeField] private float shakeAmount = 0.1f;
        [SerializeField] private float shakeDuration = 0.2f;

        [Header("Blood Spatter Settings")]
        [SerializeField] private GameObject bloodSplatterPrefab;
        [SerializeField] private int splatterCount = 10;
        [SerializeField] private float splatterSpeed = 5f;
        [SerializeField] private float splatterLifetime = 2f;

        [Header("Audio Source")]
        [SerializeField] private AudioSource audioSource;

        private Camera mainCamera;
        private Vector3 originalCameraPosition;

        public event Action<float, Vector3> OnDamageDisplayed;
        public event Action<Vector3> OnHitEffectCreated;

        private void Awake()
        {
            mainCamera = Camera.main;
            if (mainCamera != null)
            {
                originalCameraPosition = mainCamera.transform.position;
            }

            if (audioSource == null)
            {
                audioSource = GetComponent<AudioSource>();
            }
        }

        /// <summary>
        /// Displays damage feedback for a hit.
        /// </summary>
        /// <param name="damage">Amount of damage</param>
        /// <param name="position">Position where damage occurred</param>
        /// <param name="isCritical">Whether this is a critical hit</param>
        public void DisplayDamage(float damage, Vector3 position, bool isCritical = false)
        {
            CreateDamageNumber(damage, position, isCritical);
            CreateHitEffect(position);
            PlayHitSound(damage);

            OnDamageDisplayed?.Invoke(damage, position);
        }

        /// <summary>
        /// Creates a floating damage number at the specified position.
        /// </summary>
        /// <param name="damage">Amount of damage to display</param>
        /// <param name="position">Position to spawn damage number</param>
        /// <param name="isCritical">Whether this is a critical hit</param>
        private void CreateDamageNumber(float damage, Vector3 position, bool isCritical)
        {
            if (damageNumberPrefab == null)
                return;

            Vector3 spawnPosition = position + damageNumberOffset;
            GameObject damageNumberInstance = Instantiate(damageNumberPrefab, spawnPosition, Quaternion.identity);

            TextMesh textMesh = damageNumberInstance.GetComponent<TextMesh>();
            if (textMesh != null)
            {
                textMesh.text = isCritical ? damage.ToString("F0") + "!" : damage.ToString("F0");
                textMesh.color = isCritical ? Color.red : Color.white;
            }

            DamageNumberFloater floater = damageNumberInstance.AddComponent<DamageNumberFloater>();
            floater.SetupFloater(damageNumberSpeed, damageNumberLifetime);
        }

        /// <summary>
        /// Creates a hit effect at the specified position.
        /// </summary>
        /// <param name="position">Position to create effect</param>
        private void CreateHitEffect(Vector3 position)
        {
            if (hitEffectPrefab == null)
                return;

            GameObject effectInstance = Instantiate(hitEffectPrefab, position, Quaternion.identity);
            ParticleSystem particleSystem = effectInstance.GetComponent<ParticleSystem>();
            if (particleSystem != null)
            {
                particleSystem.Play();
            }

            Destroy(effectInstance, hitEffectLifetime);
            OnHitEffectCreated?.Invoke(position);
        }

        /// <summary>
        /// Creates blood spatter effect.
        /// </summary>
        /// <param name="position">Position of spatter</param>
        /// <param name="direction">Direction of spatter</param>
        public void CreateBloodSpatter(Vector3 position, Vector3 direction)
        {
            if (bloodSplatterPrefab == null)
                return;

            for (int i = 0; i < splatterCount; i++)
            {
                Vector3 randomDirection = direction + Random.insideUnitSphere * 0.5f;
                randomDirection.Normalize();

                GameObject splatterInstance = Instantiate(bloodSplatterPrefab, position, Quaternion.identity);
                Rigidbody splatterRb = splatterInstance.GetComponent<Rigidbody>();

                if (splatterRb != null)
                {
                    splatterRb.velocity = randomDirection * splatterSpeed;
                }

                Destroy(splatterInstance, splatterLifetime);
            }
        }

        /// <summary>
        /// Plays a hit sound.
        /// </summary>
        /// <param name="damage">Damage amount (affects which sound plays)</param>
        private void PlayHitSound(float damage)
        {
            if (audioSource == null)
                return;

            AudioClip clipToPlay = damage > 20f ? heavyHitSoundClip : hitSoundClip;
            if (clipToPlay != null)
            {
                audioSource.PlayOneShot(clipToPlay);
            }
        }

        /// <summary>
        /// Applies screen shake effect.
        /// </summary>
        /// <param name="intensity">Intensity of screen shake (0-1)</param>
        public void ApplyScreenShake(float intensity = 1f)
        {
            if (!enableScreenShake || mainCamera == null)
                return;

            StartCoroutine(ScreenShakeCoroutine(intensity));
        }

        /// <summary>
        /// Coroutine for screen shake effect.
        /// </summary>
        private System.Collections.IEnumerator ScreenShakeCoroutine(float intensity)
        {
            float elapsed = 0f;

            while (elapsed < shakeDuration)
            {
                elapsed += Time.deltaTime;
                float progress = 1f - (elapsed / shakeDuration);

                Vector3 randomOffset = Random.insideUnitSphere * shakeAmount * intensity * progress;
                mainCamera.transform.position = originalCameraPosition + randomOffset;

                yield return null;
            }

            mainCamera.transform.position = originalCameraPosition;
        }

        /// <summary>
        /// Displays critical hit feedback.
        /// </summary>
        /// <param name="position">Position of critical hit</param>
        public void DisplayCriticalHit(Vector3 position)
        {
            ApplyScreenShake(2f);
            CreateBloodSpatter(position, Vector3.up);
        }
    }

    /// <summary>
    /// Component that makes damage numbers float upward and fade out.
    /// </summary>
    public class DamageNumberFloater : MonoBehaviour
    {
        private float floatSpeed = 3f;
        private float lifetime = 2f;
        private float elapsedTime = 0f;
        private CanvasGroup canvasGroup;

        public void SetupFloater(float speed, float duration)
        {
            floatSpeed = speed;
            lifetime = duration;
            canvasGroup = GetComponent<CanvasGroup>();
        }

        private void Update()
        {
            elapsedTime += Time.deltaTime;

            transform.position += Vector3.up * floatSpeed * Time.deltaTime;

            if (canvasGroup != null)
            {
                canvasGroup.alpha = Mathf.Clamp01(1f - (elapsedTime / lifetime));
            }

            if (elapsedTime >= lifetime)
            {
                Destroy(gameObject);
            }
        }
    }
}
