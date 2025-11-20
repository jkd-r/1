using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace ProtocolEMR.UI
{
    /// <summary>
    /// Centralized heads-up-display that surfaces player vitals, ammo, minimap data,
    /// interaction prompts, and contextual notifications. Also routes damage feedback
    /// and compass heading updates for other systems.
    /// </summary>
    public class HUDDisplay : MonoBehaviour
    {
        public static HUDDisplay Instance { get; private set; }

        [Header("HUD Canvas")]
        [SerializeField] private Canvas hudCanvas;
        [SerializeField] private CanvasScaler canvasScaler;
        [SerializeField] private CanvasGroup hudCanvasGroup;
        [SerializeField] private float hudFadeSpeed = 10f;

        [Header("Health & Stamina")]
        [SerializeField] private HealthBar healthWidget;
        [SerializeField] private HealthBar staminaWidget;
        [SerializeField] private Image healthBar;
        [SerializeField] private Image staminaBar;
        [SerializeField] private Text healthText;
        [SerializeField] private Text staminaText;

        [Header("Ammo Counter")]
        [SerializeField] private Text ammoText;
        [SerializeField] private Transform ammoContainer;

        [Header("Crosshair")]
        [SerializeField] private Image crosshairImage;
        [SerializeField] private float crosshairSize = 1f;

        [Header("Objective Marker")]
        [SerializeField] private Text objectiveText;
        [SerializeField] private CanvasGroup objectiveCanvasGroup;

        [Header("Interaction Prompt")]
        [SerializeField] private Text interactionPrompt;
        [SerializeField] private CanvasGroup interactionCanvasGroup;

        [Header("Weapon Indicator")]
        [SerializeField] private Image meleeWeaponIcon;
        [SerializeField] private Image rangedWeaponIcon;

        [Header("Notifications")]
        [SerializeField] private Transform notificationContainer;
        [SerializeField] private GameObject notificationPrefab;

        [Header("Damage Feedback")]
        [SerializeField] private Image damageIndicatorLeft;
        [SerializeField] private Image damageIndicatorRight;
        [SerializeField] private Image damageIndicatorTop;
        [SerializeField] private Image damageIndicatorBottom;
        [SerializeField] private CanvasGroup vignettePulse;
        [SerializeField] private float damageFlashDuration = 0.5f;

        [Header("Minimap")]
        [SerializeField] private RawImage minimapImage;
        [SerializeField] private RectTransform minimapContainer;
        [SerializeField] private RectTransform playerIndicator;
        [SerializeField] private Camera minimapCamera;
        [SerializeField] private float minimapHeight = 50f;
        [SerializeField] private float minimapZoom = 60f;
        [SerializeField] private float minimapZoomLerpSpeed = 5f;
        [SerializeField] private bool rotateMinimapWithPlayer = true;
        [SerializeField] private bool hideMinimapWhenNoTarget = true;

        [Header("Compass")]
        [SerializeField] private RectTransform compassNeedle;
        [SerializeField] private Text compassHeadingText;

        private float currentHealth = 100f;
        private float maxHealth = 100f;
        private float previousHealth = 100f;
        private float currentStamina = 100f;
        private float maxStamina = 100f;
        private float previousStamina = 100f;
        private int currentAmmo = 30;
        private int maxAmmo = 30;
        private bool minimapVisible = true;
        private Transform minimapTarget;
        private float minimapZoomCurrent;

        public event Action<float, float> OnHealthChanged;
        public event Action<float, float> OnStaminaChanged;

        public bool IsVisible => hudCanvasGroup == null || hudCanvasGroup.alpha > 0.01f;
        public Canvas HUDCanvas => hudCanvas;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            if (hudCanvas != null && hudCanvasGroup == null)
            {
                hudCanvasGroup = hudCanvas.GetComponent<CanvasGroup>();
                if (hudCanvasGroup == null)
                {
                    hudCanvasGroup = hudCanvas.gameObject.AddComponent<CanvasGroup>();
                }
            }

            minimapZoomCurrent = minimapZoom;
            InitializeHUD();
        }

        private void LateUpdate()
        {
            UpdateMinimap();
        }

        public void SetHealth(float health, float maximum)
        {
            previousHealth = currentHealth;
            currentHealth = Mathf.Clamp(health, 0f, maximum);
            maxHealth = Mathf.Max(1f, maximum);

            UpdateHealthDisplay();
            OnHealthChanged?.Invoke(currentHealth, maxHealth);

            if (currentHealth <= maxHealth * 0.2f)
            {
                TriggerLowHealthWarning();
            }
        }

        public void SetStamina(float stamina, float maximum)
        {
            previousStamina = currentStamina;
            currentStamina = Mathf.Clamp(stamina, 0f, maximum);
            maxStamina = Mathf.Max(1f, maximum);

            UpdateStaminaDisplay();
            OnStaminaChanged?.Invoke(currentStamina, maxStamina);
        }

        public void SetAmmo(int current, int max)
        {
            currentAmmo = Mathf.Max(0, current);
            maxAmmo = Mathf.Max(0, max);
            UpdateAmmoDisplay();
        }

        public void SetObjective(string text, float fadeDuration = 5f)
        {
            if (objectiveText == null)
            {
                return;
            }

            objectiveText.text = text;
            if (objectiveCanvasGroup != null)
            {
                StopCoroutineSafe(ref objectiveFadeRoutine);
                objectiveFadeRoutine = StartCoroutine(FadeOutText(objectiveCanvasGroup, fadeDuration));
            }
        }

        public void ShowInteractionPrompt(string promptText)
        {
            if (interactionPrompt != null)
            {
                interactionPrompt.text = promptText;
            }

            if (interactionCanvasGroup != null)
            {
                interactionCanvasGroup.alpha = 1f;
            }
        }

        public void HideInteractionPrompt()
        {
            if (interactionCanvasGroup != null)
            {
                interactionCanvasGroup.alpha = 0f;
            }
        }

        public void SetWeaponIcons(Sprite meleeSprite, Sprite rangedSprite)
        {
            if (meleeWeaponIcon != null)
            {
                meleeWeaponIcon.sprite = meleeSprite;
            }

            if (rangedWeaponIcon != null)
            {
                rangedWeaponIcon.sprite = rangedSprite;
            }
        }

        public void SetCrosshairVisible(bool visible)
        {
            if (crosshairImage != null)
            {
                crosshairImage.enabled = visible;
            }
        }

        public void SetCrosshairScale(float scale)
        {
            crosshairSize = Mathf.Max(0.25f, scale);
            if (crosshairImage != null)
            {
                crosshairImage.rectTransform.localScale = Vector3.one * crosshairSize;
            }
        }

        public void ShowDamageIndicator(Vector3 damageDirection, float intensity = 1f)
        {
            StartCoroutine(DamageFlashCoroutine(damageDirection, intensity));
            if (healthWidget != null && previousHealth > currentHealth)
            {
                healthWidget.FlashDamage(intensity);
            }
        }

        public void AddNotification(string message, float duration = 3f)
        {
            if (notificationPrefab == null || notificationContainer == null)
            {
                Debug.LogWarning("HUDDisplay: Notification prefab or container missing.");
                return;
            }

            GameObject notificationObj = Instantiate(notificationPrefab, notificationContainer);
            Text notificationText = notificationObj.GetComponent<Text>();
            if (notificationText != null)
            {
                notificationText.text = message;
            }

            CanvasGroup canvasGroup = notificationObj.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = notificationObj.AddComponent<CanvasGroup>();
            }

            StartCoroutine(NotificationFadeCoroutine(canvasGroup, notificationObj, duration));
        }

        public void SetHUDOpacity(float opacity)
        {
            if (hudCanvasGroup == null)
            {
                return;
            }

            hudCanvasGroup.alpha = Mathf.Clamp01(opacity);
        }

        public void SetHUDVisible(bool visible, bool instant = false)
        {
            if (hudCanvas == null || hudCanvasGroup == null)
            {
                return;
            }

            hudCanvas.enabled = true;
            StopCoroutineSafe(ref hudVisibilityRoutine);
            if (instant)
            {
                hudCanvasGroup.alpha = visible ? 1f : 0f;
                hudCanvasGroup.interactable = visible;
                hudCanvasGroup.blocksRaycasts = visible;
                hudCanvas.enabled = visible || hudCanvasGroup.alpha > 0f;
            }
            else
            {
                hudVisibilityRoutine = StartCoroutine(AnimateHUDVisibility(visible));
            }
        }

        public void UpdateCompassHeading(float headingDegrees)
        {
            if (compassNeedle != null)
            {
                compassNeedle.localRotation = Quaternion.Euler(0f, 0f, -headingDegrees);
            }

            if (compassHeadingText != null)
            {
                compassHeadingText.text = $"{Mathf.Repeat(headingDegrees, 360f):000}°";
            }
        }

        public void SetMinimapTarget(Transform target)
        {
            minimapTarget = target;
            UpdateMinimapVisibility();
        }

        public void SetMinimapCamera(Camera camera)
        {
            minimapCamera = camera;
        }

        public void SetMinimapRenderTexture(RenderTexture texture)
        {
            if (minimapImage != null)
            {
                minimapImage.texture = texture;
            }
        }

        public void SetMinimapEnabled(bool enabled)
        {
            minimapVisible = enabled;
            UpdateMinimapVisibility();
        }

        public void SetMinimapZoom(float zoom)
        {
            minimapZoom = Mathf.Clamp(zoom, 15f, 200f);
        }

        public void SetPlayerIndicatorRotation(float heading)
        {
            if (playerIndicator != null)
            {
                playerIndicator.localRotation = Quaternion.Euler(0f, 0f, -heading);
            }
        }

        private void InitializeHUD()
        {
            UpdateHealthDisplay();
            UpdateStaminaDisplay();
            UpdateAmmoDisplay();
            UpdateMinimapVisibility();
        }

        private void UpdateHealthDisplay()
        {
            float normalized = maxHealth > 0f ? currentHealth / maxHealth : 0f;

            if (healthWidget != null)
            {
                healthWidget.SetValue(currentHealth, maxHealth);
            }

            if (healthBar != null)
            {
                healthBar.fillAmount = normalized;
            }

            if (healthText != null)
            {
                healthText.text = $"{currentHealth:F0}/{maxHealth:F0}";
            }
        }

        private void UpdateStaminaDisplay()
        {
            float normalized = maxStamina > 0f ? currentStamina / maxStamina : 0f;

            if (staminaWidget != null)
            {
                staminaWidget.SetValue(currentStamina, maxStamina);
            }

            if (staminaBar != null)
            {
                staminaBar.fillAmount = normalized;
            }

            if (staminaText != null)
            {
                staminaText.text = $"{currentStamina:F0}";
            }
        }

        private void UpdateAmmoDisplay()
        {
            if (ammoText != null)
            {
                ammoText.text = $"{currentAmmo}/{maxAmmo}";
            }
        }

        private void UpdateMinimap()
        {
            if (!minimapVisible || minimapCamera == null || minimapTarget == null)
            {
                return;
            }

            Vector3 targetPosition = minimapTarget.position;
            Vector3 desiredPosition = new Vector3(targetPosition.x, targetPosition.y + minimapHeight, targetPosition.z);
            minimapCamera.transform.position = Vector3.Lerp(minimapCamera.transform.position, desiredPosition, Time.deltaTime * 15f);

            Quaternion desiredRotation = rotateMinimapWithPlayer
                ? Quaternion.Euler(90f, minimapTarget.eulerAngles.y, 0f)
                : Quaternion.Euler(90f, 0f, 0f);
            minimapCamera.transform.rotation = Quaternion.Slerp(minimapCamera.transform.rotation, desiredRotation, Time.deltaTime * 10f);

            minimapZoomCurrent = Mathf.Lerp(minimapZoomCurrent, minimapZoom, Time.deltaTime * minimapZoomLerpSpeed);
            minimapCamera.orthographicSize = minimapZoomCurrent;

            if (rotateMinimapWithPlayer)
            {
                SetPlayerIndicatorRotation(minimapTarget.eulerAngles.y);
            }
        }

        private void UpdateMinimapVisibility()
        {
            bool shouldShow = minimapVisible && minimapTarget != null;
            if (minimapContainer != null)
            {
                minimapContainer.gameObject.SetActive(shouldShow || !hideMinimapWhenNoTarget);
            }

            if (minimapImage != null)
            {
                minimapImage.enabled = shouldShow;
            }
        }

        private IEnumerator DamageFlashCoroutine(Vector3 damageDirection, float intensity)
        {
            float elapsed = 0f;
            float duration = damageFlashDuration;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float alpha = Mathf.Lerp(intensity, 0f, elapsed / duration);

                Color color = new Color(1f, 0f, 0f, alpha);
                if (damageIndicatorLeft != null) damageIndicatorLeft.color = color;
                if (damageIndicatorRight != null) damageIndicatorRight.color = color;
                if (damageIndicatorTop != null) damageIndicatorTop.color = color;
                if (damageIndicatorBottom != null) damageIndicatorBottom.color = color;

                yield return null;
            }
        }

        private void TriggerLowHealthWarning()
        {
            if (vignettePulse != null)
            {
                StartCoroutine(PulseVignette());
            }
        }

        private IEnumerator PulseVignette()
        {
            float elapsed = 0f;
            float pulseDuration = 0.5f;

            while (currentHealth <= maxHealth * 0.2f)
            {
                elapsed += Time.deltaTime;
                if (elapsed > pulseDuration)
                {
                    elapsed = 0f;
                }

                float pulse = Mathf.Sin((elapsed / pulseDuration) * Mathf.PI);
                if (vignettePulse != null)
                {
                    vignettePulse.alpha = pulse * 0.5f;
                }

                yield return null;
            }

            if (vignettePulse != null)
            {
                vignettePulse.alpha = 0f;
            }
        }

        private IEnumerator FadeOutText(CanvasGroup canvasGroup, float duration)
        {
            if (canvasGroup == null)
            {
                yield break;
            }

            canvasGroup.alpha = 1f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                canvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / duration);
                yield return null;
            }

            canvasGroup.alpha = 0f;
        }

        private IEnumerator NotificationFadeCoroutine(CanvasGroup canvasGroup, GameObject notificationObj, float duration)
        {
            float elapsed = 0f;
            float fadeInTime = 0.2f;
            float fadeOutTime = 0.3f;

            canvasGroup.alpha = 0f;

            while (elapsed < fadeInTime)
            {
                elapsed += Time.deltaTime;
                canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / fadeInTime);
                yield return null;
            }

            canvasGroup.alpha = 1f;
            yield return new WaitForSeconds(duration);

            elapsed = 0f;
            while (elapsed < fadeOutTime)
            {
                elapsed += Time.deltaTime;
                canvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / fadeOutTime);
                yield return null;
            }

            Destroy(notificationObj);
        }

        private IEnumerator AnimateHUDVisibility(bool visible)
        {
            float target = visible ? 1f : 0f;
            float start = hudCanvasGroup.alpha;
            float elapsed = 0f;

            while (!Mathf.Approximately(hudCanvasGroup.alpha, target))
            {
                elapsed += Time.unscaledDeltaTime * hudFadeSpeed;
                hudCanvasGroup.alpha = Mathf.Lerp(start, target, elapsed);
                hudCanvasGroup.interactable = visible;
                hudCanvasGroup.blocksRaycasts = visible;
                if (elapsed >= 1f)
                {
                    break;
                }
                yield return null;
            }

            hudCanvasGroup.alpha = target;
            hudCanvas.enabled = visible || target > 0f;
        }

        private void StopCoroutineSafe(ref Coroutine routine)
        {
            if (routine != null)
            {
                StopCoroutine(routine);
                routine = null;
            }
        }

        private Coroutine hudVisibilityRoutine;
        private Coroutine objectiveFadeRoutine;
    }
}
