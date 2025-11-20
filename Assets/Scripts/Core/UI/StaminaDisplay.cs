using UnityEngine;
using UnityEngine.UI;
using ProtocolEMR.Core.Player;
using ProtocolEMR.Core.Settings;

namespace ProtocolEMR.Core.UI
{
    /// <summary>
    /// Displays stamina bar and manages stamina UI feedback.
    /// Shows stamina only when sprinting or recovering, fades after recovery.
    /// Provides visual feedback for stamina depletion (red tint) and recovery.
    /// </summary>
    public class StaminaDisplay : MonoBehaviour
    {
        [Header("UI Components")]
        [SerializeField] private Image staminaBar;
        [SerializeField] private Image staminaBackgroundBar;
        [SerializeField] private Text staminaText;
        [SerializeField] private CanvasGroup canvasGroup;

        [Header("Display Settings")]
        [SerializeField] private float fadeInDuration = 0.2f;
        [SerializeField] private float fadeOutDuration = 0.5f;
        [SerializeField] private float fadeOutDelay = 1.0f;
        [SerializeField] private Color normalColor = Color.white;
        [SerializeField] private Color depletedColor = Color.red;

        [Header("Accessibility")]
        [SerializeField] private bool useTextDisplay = true;
        [SerializeField] private bool enableAnimations = true;

        private PlayerController playerController;
        private float fadeTimer;
        private bool shouldFadeOut;
        private CanvasGroup staminaBarCanvasGroup;

        private void Start()
        {
            playerController = FindObjectOfType<PlayerController>();

            if (canvasGroup == null)
                canvasGroup = GetComponent<CanvasGroup>();

            if (canvasGroup != null)
                canvasGroup.alpha = 0f;

            if (staminaBar != null && staminaBar.GetComponent<CanvasGroup>() == null)
            {
                staminaBarCanvasGroup = staminaBar.gameObject.AddComponent<CanvasGroup>();
            }
            else if (staminaBar != null)
            {
                staminaBarCanvasGroup = staminaBar.GetComponent<CanvasGroup>();
            }

            ApplyAccessibilitySettings();

            if (SettingsManager.Instance != null)
            {
                SettingsManager.Instance.OnSettingsChanged += ApplyAccessibilitySettings;
            }
        }

        private void OnDestroy()
        {
            if (SettingsManager.Instance != null)
            {
                SettingsManager.Instance.OnSettingsChanged -= ApplyAccessibilitySettings;
            }
        }

        private void Update()
        {
            if (playerController == null)
                return;

            UpdateStaminaDisplay();
            UpdateStaminaFadeout();
        }

        private void UpdateStaminaDisplay()
        {
            float staminaPercent = playerController.CurrentStamina / playerController.MaxStamina;

            if (staminaBar != null)
            {
                staminaBar.fillAmount = staminaPercent;

                Color barColor = Color.Lerp(depletedColor, normalColor, staminaPercent);
                staminaBar.color = barColor;
            }

            if (useTextDisplay && staminaText != null)
            {
                staminaText.text = Mathf.RoundToInt(playerController.CurrentStamina).ToString();
            }

            if (playerController.IsSprinting || playerController.CurrentStamina < playerController.MaxStamina)
            {
                shouldFadeOut = false;
                fadeTimer = fadeOutDelay;

                if (canvasGroup != null && enableAnimations)
                {
                    canvasGroup.alpha = Mathf.Lerp(canvasGroup.alpha, 1f, fadeInDuration);
                }
                else if (canvasGroup != null)
                {
                    canvasGroup.alpha = 1f;
                }
            }
        }

        private void UpdateStaminaFadeout()
        {
            if (playerController.CurrentStamina >= playerController.MaxStamina * 0.95f && !playerController.IsSprinting)
            {
                fadeTimer -= Time.deltaTime;

                if (fadeTimer <= 0)
                    shouldFadeOut = true;
            }

            if (shouldFadeOut && canvasGroup != null && enableAnimations)
            {
                canvasGroup.alpha = Mathf.Lerp(canvasGroup.alpha, 0f, Time.deltaTime / fadeOutDuration);

                if (canvasGroup.alpha < 0.01f)
                {
                    canvasGroup.alpha = 0f;
                }
            }
        }

        private void ApplyAccessibilitySettings()
        {
            if (SettingsManager.Instance == null)
                return;

            var accessibilitySettings = SettingsManager.Instance.GetAccessibilitySettings();
            enableAnimations = !accessibilitySettings.enableMotionBlur;
        }

        public void SetStaminaColor(Color color)
        {
            normalColor = color;
        }

        public void SetVisibility(bool visible)
        {
            if (canvasGroup != null)
                canvasGroup.alpha = visible ? 1f : 0f;
        }
    }
}
