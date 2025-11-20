using UnityEngine;
using ProtocolEMR.Core.Settings;

namespace ProtocolEMR.Core.Camera
{
    /// <summary>
    /// Handles camera shake effects for landing, impact, and other events.
    /// Integrates with settings manager for accessibility (shake intensity control).
    /// </summary>
    public class CameraShake : MonoBehaviour
    {
        [SerializeField] private float shakeDecayRate = 5f;

        private float shakeAmount = 0f;
        private float shakeIntensity = 1.0f;
        private Vector3 originalPosition;

        private void Start()
        {
            originalPosition = transform.localPosition;

            if (SettingsManager.Instance != null)
            {
                var accessibility = SettingsManager.Instance.GetAccessibilitySettings();
                shakeIntensity = accessibility.cameraShakeIntensity;

                SettingsManager.Instance.OnSettingsChanged += UpdateShakeIntensity;
            }
        }

        private void OnDestroy()
        {
            if (SettingsManager.Instance != null)
            {
                SettingsManager.Instance.OnSettingsChanged -= UpdateShakeIntensity;
            }
        }

        private void Update()
        {
            if (shakeAmount > 0)
            {
                shakeAmount -= Time.deltaTime * shakeDecayRate;
                shakeAmount = Mathf.Max(shakeAmount, 0f);

                ApplyShake();
            }
        }

        private void ApplyShake()
        {
            float offset = Random.Range(-shakeAmount, shakeAmount) * shakeIntensity;
            transform.localPosition = originalPosition + new Vector3(offset * 0.1f, offset * 0.05f, 0);
        }

        public void Shake(float amount = 0.1f)
        {
            shakeAmount = Mathf.Clamp01(amount);
        }

        private void UpdateShakeIntensity()
        {
            if (SettingsManager.Instance != null)
            {
                var accessibility = SettingsManager.Instance.GetAccessibilitySettings();
                shakeIntensity = accessibility.cameraShakeIntensity;
            }
        }
    }
}
