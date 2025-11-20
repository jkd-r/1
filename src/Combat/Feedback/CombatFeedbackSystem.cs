using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace ProtocolEMR.Combat.Core
{
    public class CombatFeedbackSystem : MonoBehaviour
    {
        [Header("Screen Shake Settings")]
        [SerializeField] private float punchShakeIntensity = 0.1f;
        [SerializeField] private float weaponShakeIntensity = 0.3f;
        [SerializeField] private float rangedShakeIntensity = 0.5f;
        [SerializeField] private float shakeDuration = 0.2f;

        [Header("Damage Flash Settings")]
        [SerializeField] private Image damageFlashImage;
        [SerializeField] private Color damageFlashColor = new Color(1f, 0f, 0f, 0.3f);
        [SerializeField] private float flashDuration = 0.3f;

        [Header("Particle Systems")]
        [SerializeField] private GameObject bloodSpatterPrefab;
        [SerializeField] private GameObject impactSparksPrefab;
        [SerializeField] private GameObject dustImpactPrefab;

        [Header("Damage Numbers")]
        [SerializeField] private bool showDamageNumbers = true;
        [SerializeField] private GameObject damageNumberPrefab;
        [SerializeField] private Color damageNumberColor = Color.red;

        [Header("Health Bar Settings")]
        [SerializeField] private GameObject healthBarPrefab;

        [Header("References")]
        [SerializeField] private Camera playerCamera;
        [SerializeField] private Transform cameraTransform;

        private Vector3 originalCameraPosition;
        private bool isShaking = false;

        void Awake()
        {
            if (playerCamera == null)
                playerCamera = Camera.main;

            if (cameraTransform == null && playerCamera != null)
                cameraTransform = playerCamera.transform;

            if (cameraTransform != null)
                originalCameraPosition = cameraTransform.localPosition;
        }

        public void OnHitSuccess(Vector3 hitPosition, float damage, AttackType attackType)
        {
            float shakeIntensity = GetShakeIntensityForAttack(attackType);
            TriggerScreenShake(shakeIntensity);

            SpawnBloodEffect(hitPosition);

            if (showDamageNumbers)
                ShowDamageNumber(hitPosition, damage);
        }

        public void OnPlayerDamaged(float damage)
        {
            TriggerDamageFlash();
            TriggerScreenShake(0.3f);
        }

        public void TriggerScreenShake(float intensity)
        {
            if (!isShaking)
            {
                StartCoroutine(ScreenShakeCoroutine(intensity));
            }
        }

        private IEnumerator ScreenShakeCoroutine(float intensity)
        {
            if (cameraTransform == null)
                yield break;

            isShaking = true;
            float elapsed = 0f;

            while (elapsed < shakeDuration)
            {
                float x = Random.Range(-1f, 1f) * intensity;
                float y = Random.Range(-1f, 1f) * intensity;

                cameraTransform.localPosition = originalCameraPosition + new Vector3(x, y, 0);

                elapsed += Time.deltaTime;
                yield return null;
            }

            cameraTransform.localPosition = originalCameraPosition;
            isShaking = false;
        }

        private void TriggerDamageFlash()
        {
            if (damageFlashImage != null)
            {
                StartCoroutine(DamageFlashCoroutine());
            }
        }

        private IEnumerator DamageFlashCoroutine()
        {
            damageFlashImage.color = damageFlashColor;
            float elapsed = 0f;

            while (elapsed < flashDuration)
            {
                float alpha = Mathf.Lerp(damageFlashColor.a, 0f, elapsed / flashDuration);
                damageFlashImage.color = new Color(damageFlashColor.r, damageFlashColor.g, damageFlashColor.b, alpha);

                elapsed += Time.deltaTime;
                yield return null;
            }

            damageFlashImage.color = new Color(damageFlashColor.r, damageFlashColor.g, damageFlashColor.b, 0f);
        }

        private void SpawnBloodEffect(Vector3 position)
        {
            if (bloodSpatterPrefab != null)
            {
                GameObject blood = Instantiate(bloodSpatterPrefab, position, Quaternion.identity);
                ParticleSystem ps = blood.GetComponent<ParticleSystem>();
                if (ps != null)
                {
                    var main = ps.main;
                    main.startSpeed = Random.Range(2f, 8f);
                }
                Destroy(blood, 5f);
            }
        }

        private void ShowDamageNumber(Vector3 worldPosition, float damage)
        {
            if (damageNumberPrefab == null)
                return;

            GameObject damageNumber = Instantiate(damageNumberPrefab, worldPosition + Vector3.up * 2f, Quaternion.identity);
            
            TextMesh textMesh = damageNumber.GetComponent<TextMesh>();
            if (textMesh != null)
            {
                textMesh.text = "+" + damage.ToString("F0");
                textMesh.color = damageNumberColor;
            }

            StartCoroutine(AnimateDamageNumber(damageNumber.transform));
        }

        private IEnumerator AnimateDamageNumber(Transform numberTransform)
        {
            float duration = 1f;
            float elapsed = 0f;
            Vector3 startPos = numberTransform.position;

            while (elapsed < duration)
            {
                numberTransform.position = startPos + Vector3.up * elapsed * 2f;
                
                TextMesh textMesh = numberTransform.GetComponent<TextMesh>();
                if (textMesh != null)
                {
                    Color color = textMesh.color;
                    color.a = 1f - (elapsed / duration);
                    textMesh.color = color;
                }

                if (playerCamera != null)
                    numberTransform.LookAt(playerCamera.transform);

                elapsed += Time.deltaTime;
                yield return null;
            }

            Destroy(numberTransform.gameObject);
        }

        private float GetShakeIntensityForAttack(AttackType attackType)
        {
            switch (attackType)
            {
                case AttackType.Punch:
                case AttackType.Kick:
                    return punchShakeIntensity;
                case AttackType.Wrench:
                case AttackType.Crowbar:
                case AttackType.Pipe:
                    return weaponShakeIntensity;
                default:
                    return punchShakeIntensity;
            }
        }

        public void SpawnImpactEffect(Vector3 position, Vector3 normal)
        {
            if (impactSparksPrefab != null)
            {
                GameObject sparks = Instantiate(impactSparksPrefab, position, Quaternion.LookRotation(normal));
                Destroy(sparks, 2f);
            }
        }

        public void EnableDamageNumbers(bool enabled)
        {
            showDamageNumbers = enabled;
        }

        public bool AreDamageNumbersEnabled()
        {
            return showDamageNumbers;
        }
    }
}
