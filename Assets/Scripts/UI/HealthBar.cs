using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace ProtocolEMR.UI
{
    /// <summary>
    /// Generic bar component that visualizes health, stamina, shields, or any other
    /// normalized value. Supports delayed fill visuals, flash feedback, and smooth
    /// fade in/out transitions for world-space or screen-space UI.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CanvasGroup))]
    public class HealthBar : MonoBehaviour
    {
        [Header("Bar Elements")]
        [SerializeField] private Image fillImage;
        [SerializeField] private Image delayedFillImage;
        [SerializeField] private Image flashOverlayImage;
        [SerializeField] private Text valueLabel;
        [SerializeField] private Text nameLabel;

        [Header("Appearance")]
        [SerializeField] private Gradient fillGradient;
        [SerializeField] private float fillLerpSpeed = 12f;
        [SerializeField] private float delayedFillLerpSpeed = 4f;
        [SerializeField] private float flashDuration = 0.25f;
        [SerializeField] private Color flashColor = Color.white;
        [SerializeField] private bool showNumericValue = true;
        [SerializeField] private string valueFormat = "{0:F0}/{1:F0}";

        [Header("Visibility")]
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private float fadeSpeed = 10f;
        [SerializeField] private bool startHidden = false;

        private float targetFill = 1f;
        private float currentFill = 1f;
        private float delayedFill = 1f;
        private Coroutine visibilityRoutine;
        private Coroutine flashRoutine;

        public float CurrentValue { get; private set; } = 1f;
        public float MaxValue { get; private set; } = 1f;
        public bool IsVisible => canvasGroup == null || canvasGroup.alpha > 0.01f;

        private void Awake()
        {
            if (canvasGroup == null)
            {
                canvasGroup = GetComponent<CanvasGroup>();
            }

            canvasGroup.alpha = startHidden ? 0f : 1f;
            ApplyFillInstant();
        }

        private void Update()
        {
            currentFill = Mathf.Lerp(currentFill, targetFill, Time.deltaTime * fillLerpSpeed);
            delayedFill = Mathf.Lerp(delayedFill, targetFill, Time.deltaTime * delayedFillLerpSpeed);
            ApplyFill();
        }

        /// <summary>
        /// Updates the underlying values for the bar and refreshes the visual state.
        /// </summary>
        public void SetValue(float current, float max)
        {
            MaxValue = Mathf.Max(0.001f, max);
            CurrentValue = Mathf.Clamp(current, 0f, MaxValue);
            targetFill = CurrentValue / MaxValue;
            UpdateValueLabel();
        }

        /// <summary>
        /// Updates the owner label for world-space health bars (e.g., enemy name).
        /// </summary>
        public void SetOwnerLabel(string label)
        {
            if (nameLabel != null)
            {
                nameLabel.text = label;
            }
        }

        /// <summary>
        /// Smoothly fades the widget in or out.
        /// </summary>
        public void SetVisible(bool visible, bool instant = false)
        {
            if (canvasGroup == null)
            {
                return;
            }

            if (instant)
            {
                canvasGroup.alpha = visible ? 1f : 0f;
                return;
            }

            if (visibilityRoutine != null)
            {
                StopCoroutine(visibilityRoutine);
            }
            visibilityRoutine = StartCoroutine(FadeRoutine(visible));
        }

        /// <summary>
        /// Triggers a short flash animation, typically called when damage is applied.
        /// </summary>
        public void FlashDamage(float intensity = 1f)
        {
            if (flashOverlayImage == null)
            {
                return;
            }

            if (flashRoutine != null)
            {
                StopCoroutine(flashRoutine);
            }
            flashRoutine = StartCoroutine(FlashRoutine(Mathf.Clamp01(intensity)));
        }

        private void UpdateValueLabel()
        {
            if (valueLabel != null)
            {
                valueLabel.enabled = showNumericValue;
                if (showNumericValue)
                {
                    valueLabel.text = string.Format(valueFormat, CurrentValue, MaxValue);
                }
            }
        }

        private void ApplyFill()
        {
            if (fillImage != null)
            {
                fillImage.fillAmount = currentFill;
                if (fillGradient != null)
                {
                    fillImage.color = fillGradient.Evaluate(currentFill);
                }
            }

            if (delayedFillImage != null)
            {
                delayedFillImage.fillAmount = delayedFill;
            }
        }

        private void ApplyFillInstant()
        {
            currentFill = targetFill;
            delayedFill = targetFill;
            ApplyFill();
        }

        private IEnumerator FadeRoutine(bool visible)
        {
            float target = visible ? 1f : 0f;
            while (!Mathf.Approximately(canvasGroup.alpha, target))
            {
                canvasGroup.alpha = Mathf.MoveTowards(canvasGroup.alpha, target, Time.unscaledDeltaTime * fadeSpeed);
                yield return null;
            }
            canvasGroup.alpha = target;
            visibilityRoutine = null;
        }

        private IEnumerator FlashRoutine(float intensity)
        {
            flashOverlayImage.color = new Color(flashColor.r, flashColor.g, flashColor.b, flashColor.a * intensity);
            float elapsed = 0f;

            while (elapsed < flashDuration)
            {
                elapsed += Time.deltaTime;
                float alpha = Mathf.Lerp(flashColor.a * intensity, 0f, elapsed / flashDuration);
                flashOverlayImage.color = new Color(flashColor.r, flashColor.g, flashColor.b, alpha);
                yield return null;
            }

            flashOverlayImage.color = new Color(flashColor.r, flashColor.g, flashColor.b, 0f);
            flashRoutine = null;
        }
    }
}
