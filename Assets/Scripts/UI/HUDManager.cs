using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using ProtocolEMR.Core.Input;

namespace ProtocolEMR.UI
{
    public class HUDManager : MonoBehaviour
    {
        public static HUDManager Instance { get; private set; }

        [Header("HUD Canvas")]
        [SerializeField] private Canvas hudCanvas;
        [SerializeField] private CanvasScaler canvasScaler;

        [Header("Health & Stamina")]
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

        private float currentHealth = 100f;
        private float maxHealth = 100f;
        private float currentStamina = 100f;
        private float maxStamina = 100f;
        private int currentAmmo = 30;
        private int maxAmmo = 30;

        public event Action<float, float> OnHealthChanged;
        public event Action<float, float> OnStaminaChanged;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            InitializeHUD();
        }

        private void Start()
        {
            if (InputManager.Instance != null)
            {
                InputManager.Instance.OnInventory += OnInventoryToggle;
            }
        }

        private void OnDestroy()
        {
            if (InputManager.Instance != null)
            {
                InputManager.Instance.OnInventory -= OnInventoryToggle;
            }
        }

        private void InitializeHUD()
        {
            if (healthBar == null || staminaBar == null)
            {
                Debug.LogWarning("HUD Manager: Health or Stamina bar not assigned!");
            }

            UpdateHealthDisplay();
            UpdateStaminaDisplay();
        }

        public void SetHealth(float health, float maxHealth)
        {
            currentHealth = Mathf.Clamp(health, 0, maxHealth);
            this.maxHealth = maxHealth;
            UpdateHealthDisplay();
            OnHealthChanged?.Invoke(currentHealth, maxHealth);

            if (currentHealth <= maxHealth * 0.2f)
            {
                TriggerLowHealthWarning();
            }
        }

        public void SetStamina(float stamina, float maxStamina)
        {
            currentStamina = Mathf.Clamp(stamina, 0, maxStamina);
            this.maxStamina = maxStamina;
            UpdateStaminaDisplay();
            OnStaminaChanged?.Invoke(currentStamina, maxStamina);
        }

        public void SetAmmo(int current, int max)
        {
            currentAmmo = current;
            maxAmmo = max;
            UpdateAmmoDisplay();
        }

        public void SetObjective(string objectiveText, float fadeDuration = 5f)
        {
            if (this.objectiveText != null)
            {
                this.objectiveText.text = objectiveText;
                StartCoroutine(FadeOutText(objectiveCanvasGroup, fadeDuration));
            }
        }

        public void ShowInteractionPrompt(string promptText)
        {
            if (interactionPrompt != null)
            {
                interactionPrompt.text = promptText;
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
                meleeWeaponIcon.sprite = meleeSprite;
            if (rangedWeaponIcon != null)
                rangedWeaponIcon.sprite = rangedSprite;
        }

        public void ShowDamageIndicator(Vector3 damageDirection, float intensity = 1f)
        {
            StartCoroutine(DamageFlashCoroutine(damageDirection, intensity));
        }

        public void AddNotification(string message, float duration = 3f)
        {
            if (notificationPrefab == null || notificationContainer == null)
            {
                Debug.LogWarning("Notification prefab or container not assigned!");
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
            if (hudCanvas != null)
            {
                CanvasGroup canvasGroup = hudCanvas.GetComponent<CanvasGroup>();
                if (canvasGroup == null)
                {
                    canvasGroup = hudCanvas.gameObject.AddComponent<CanvasGroup>();
                }
                canvasGroup.alpha = Mathf.Clamp01(opacity);
            }
        }

        private void UpdateHealthDisplay()
        {
            if (healthBar != null)
            {
                healthBar.fillAmount = maxHealth > 0 ? currentHealth / maxHealth : 0;
            }

            if (healthText != null)
            {
                healthText.text = $"{currentHealth:F0}/{maxHealth:F0}";
            }
        }

        private void UpdateStaminaDisplay()
        {
            if (staminaBar != null)
            {
                staminaBar.fillAmount = maxStamina > 0 ? currentStamina / maxStamina : 0;
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

        private void TriggerLowHealthWarning()
        {
            if (vignettePulse != null)
            {
                StartCoroutine(PulseVignette());
            }
        }

        private IEnumerator DamageFlashCoroutine(Vector3 damageDirection, float intensity)
        {
            float elapsed = 0f;
            float duration = damageFlashDuration;

            Vector3 screenCenter = new Vector3(Screen.width / 2f, Screen.height / 2f, 0);
            Vector3 damageScreenPos = damageDirection;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float alpha = Mathf.Lerp(intensity, 0, elapsed / duration);

                if (damageIndicatorLeft != null)
                    damageIndicatorLeft.color = new Color(1, 0, 0, alpha);
                if (damageIndicatorRight != null)
                    damageIndicatorRight.color = new Color(1, 0, 0, alpha);
                if (damageIndicatorTop != null)
                    damageIndicatorTop.color = new Color(1, 0, 0, alpha);
                if (damageIndicatorBottom != null)
                    damageIndicatorBottom.color = new Color(1, 0, 0, alpha);

                yield return null;
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
                    elapsed = 0f;

                float pulse = Mathf.Sin((elapsed / pulseDuration) * Mathf.PI);
                if (vignettePulse != null)
                {
                    vignettePulse.alpha = pulse * 0.5f;
                }

                yield return null;
            }

            if (vignettePulse != null)
                vignettePulse.alpha = 0f;
        }

        private IEnumerator FadeOutText(CanvasGroup canvasGroup, float fadeDuration)
        {
            if (canvasGroup == null) yield break;

            canvasGroup.alpha = 1f;
            float elapsed = 0f;

            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                canvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
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

        private void OnInventoryToggle()
        {
            Debug.Log("Inventory toggled from HUD");
        }
    }
}
