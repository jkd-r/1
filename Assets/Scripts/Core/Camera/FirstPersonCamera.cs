using UnityEngine;
using ProtocolEMR.Core.Input;
using ProtocolEMR.Core.Settings;

namespace ProtocolEMR.Core.Camera
{
    /// <summary>
    /// First-person camera controller with smooth mouse look, camera bobbing, and FOV adjustment.
    /// Handles camera rotation, head bobbing during movement, and screen shake effects.
    /// Sensitivity and accessibility options are configurable through the SettingsManager.
    /// </summary>
    [RequireComponent(typeof(UnityEngine.Camera))]
    public class FirstPersonCamera : MonoBehaviour
    {
        [Header("Camera Settings")]
        [SerializeField] private Transform playerBody;
        [SerializeField] private float mouseSensitivity = 1.0f;
        [SerializeField] private float defaultFOV = 90f;
        [SerializeField] private float minFOV = 60f;
        [SerializeField] private float maxFOV = 120f;

        [Header("Camera Bobbing")]
        [SerializeField] private bool enableCameraBob = true;
        [SerializeField] private float bobFrequency = 2.0f;
        [SerializeField] private float bobAmplitude = 0.05f;
        [SerializeField] private float bobSmoothing = 10f;

        [Header("Camera Shake")]
        [SerializeField] private float shakeIntensity = 1.0f;
        [SerializeField] private float shakeDuration = 0.0f;

        private UnityEngine.Camera cam;
        private float xRotation = 0f;
        private float bobTimer = 0f;
        private Vector3 originalPosition;
        private Vector3 targetBobPosition;
        private float currentShakeDuration = 0f;
        private bool isPaused = false;

        private void Awake()
        {
            cam = GetComponent<UnityEngine.Camera>();
            originalPosition = transform.localPosition;
            cam.fieldOfView = defaultFOV;
        }

        private void Start()
        {
            if (InputManager.Instance != null)
            {
                InputManager.Instance.OnLook += HandleLookInput;
                InputManager.Instance.OnPause += TogglePause;
            }

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            LoadSettings();
        }

        private void OnDestroy()
        {
            if (InputManager.Instance != null)
            {
                InputManager.Instance.OnLook -= HandleLookInput;
                InputManager.Instance.OnPause -= TogglePause;
            }
        }

        private void HandleLookInput(Vector2 lookDelta)
        {
            if (isPaused) return;

            float mouseX = lookDelta.x * mouseSensitivity * Time.deltaTime;
            float mouseY = lookDelta.y * mouseSensitivity * Time.deltaTime;

            xRotation -= mouseY;
            xRotation = Mathf.Clamp(xRotation, -90f, 90f);

            transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
            if (playerBody != null)
            {
                playerBody.Rotate(Vector3.up * mouseX);
            }
        }

        private void Update()
        {
            if (isPaused) return;

            HandleCameraBob();
            HandleCameraShake();
        }

        private void HandleCameraBob()
        {
            if (!enableCameraBob || InputManager.Instance == null)
            {
                targetBobPosition = Vector3.zero;
                transform.localPosition = Vector3.Lerp(transform.localPosition, originalPosition, Time.deltaTime * bobSmoothing);
                return;
            }

            Vector2 moveInput = InputManager.Instance.GetMovementInput();
            float movementMagnitude = moveInput.magnitude;

            if (movementMagnitude > 0.1f)
            {
                bobTimer += Time.deltaTime * bobFrequency;
                float bobOffsetY = Mathf.Sin(bobTimer) * bobAmplitude;
                float bobOffsetX = Mathf.Cos(bobTimer * 0.5f) * bobAmplitude * 0.5f;
                targetBobPosition = new Vector3(bobOffsetX, bobOffsetY, 0f);
            }
            else
            {
                bobTimer = 0f;
                targetBobPosition = Vector3.zero;
            }

            Vector3 targetPosition = originalPosition + targetBobPosition;
            transform.localPosition = Vector3.Lerp(transform.localPosition, targetPosition, Time.deltaTime * bobSmoothing);
        }

        private void HandleCameraShake()
        {
            if (currentShakeDuration > 0f)
            {
                currentShakeDuration -= Time.deltaTime;
                Vector3 shakeOffset = Random.insideUnitSphere * shakeIntensity * 0.1f;
                transform.localPosition += shakeOffset;
            }
        }

        public void TriggerCameraShake(float duration, float intensity)
        {
            currentShakeDuration = duration;
            shakeIntensity = intensity;
        }

        public void SetFOV(float fov)
        {
            cam.fieldOfView = Mathf.Clamp(fov, minFOV, maxFOV);
        }

        public void SetMouseSensitivity(float sensitivity)
        {
            mouseSensitivity = sensitivity;
        }

        public void SetCameraBobEnabled(bool enabled)
        {
            enableCameraBob = enabled;
        }

        private void TogglePause()
        {
            isPaused = !isPaused;
            Cursor.lockState = isPaused ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = isPaused;
        }

        private void LoadSettings()
        {
            if (SettingsManager.Instance != null)
            {
                mouseSensitivity = SettingsManager.Instance.GetMouseSensitivity();
                cam.fieldOfView = SettingsManager.Instance.GetFieldOfView();
                enableCameraBob = SettingsManager.Instance.IsCameraBobEnabled();
            }
        }

        public void ApplySettings()
        {
            LoadSettings();
        }
    }
}
