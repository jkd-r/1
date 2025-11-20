using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ProtocolEMR.Core.Dialogue;
using ProtocolEMR.Core.Settings;

namespace ProtocolEMR.Core.UI
{
    /// <summary>
    /// HUD overlay system for displaying Unknown messages with glitch effects.
    /// Messages appear as screen distortions at the edges of the screen.
    /// </summary>
    public class UnknownHUDOverlay : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject overlayPanel;
        [SerializeField] private TextMeshProUGUI messageText;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private Image glitchOverlay;

        [Header("Display Settings")]
        [SerializeField] private float fadeInDuration = 0.5f;
        [SerializeField] private float fadeOutDuration = 0.5f;
        [SerializeField] [Range(0f, 1f)] private float maxOpacity = 0.9f;

        [Header("Glitch Effects")]
        [SerializeField] private bool enableGlitchEffects = true;
        [SerializeField] private float glitchIntensity = 0.5f;
        [SerializeField] private float chromaticAberrationAmount = 5f;
        [SerializeField] private Color glitchColor = new Color(0.1f, 0.8f, 1f, 0.3f);

        [Header("Audio")]
        [SerializeField] private AudioClip glitchSound;
        [SerializeField] private AudioClip staticSound;
        [SerializeField] private AudioSource audioSource;

        [Header("Positioning")]
        [SerializeField] private MessagePosition messagePosition = MessagePosition.BottomRight;

        private bool isDisplaying = false;
        private Coroutine currentDisplayCoroutine;
        private float effectIntensityMultiplier = 1f;

        public enum MessagePosition
        {
            TopLeft,
            TopRight,
            BottomLeft,
            BottomRight,
            Center
        }

        private void Awake()
        {
            if (overlayPanel != null)
            {
                overlayPanel.SetActive(false);
            }

            if (canvasGroup == null)
            {
                canvasGroup = overlayPanel?.GetComponent<CanvasGroup>();
                if (canvasGroup == null && overlayPanel != null)
                {
                    canvasGroup = overlayPanel.AddComponent<CanvasGroup>();
                }
            }

            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.playOnAwake = false;
            }

            LoadAccessibilitySettings();
        }

        private void Start()
        {
            if (UnknownDialogueManager.Instance != null)
            {
                UnknownDialogueManager.Instance.OnMessageDisplay += HandleMessageDisplay;
            }

            SetMessagePosition(messagePosition);
        }

        private void OnDestroy()
        {
            if (UnknownDialogueManager.Instance != null)
            {
                UnknownDialogueManager.Instance.OnMessageDisplay -= HandleMessageDisplay;
            }
        }

        private void LoadAccessibilitySettings()
        {
            if (SettingsManager.Instance != null)
            {
                AccessibilitySettings settings = SettingsManager.Instance.GetAccessibilitySettings();
                effectIntensityMultiplier = settings.enableMotionBlur ? 1f : 0.3f;
            }
        }

        private void HandleMessageDisplay(UnknownMessage message, MessageDisplayMode displayMode)
        {
            if (displayMode == MessageDisplayMode.HUDOverlay || displayMode == MessageDisplayMode.Both)
            {
                DisplayMessage(message);
            }
        }

        public void DisplayMessage(UnknownMessage message)
        {
            if (isDisplaying && currentDisplayCoroutine != null)
            {
                StopCoroutine(currentDisplayCoroutine);
            }

            currentDisplayCoroutine = StartCoroutine(DisplayMessageCoroutine(message));
        }

        private System.Collections.IEnumerator DisplayMessageCoroutine(UnknownMessage message)
        {
            isDisplaying = true;

            if (messageText != null)
            {
                messageText.text = message.text;
            }

            if (overlayPanel != null)
            {
                overlayPanel.SetActive(true);
            }

            if (enableGlitchEffects)
            {
                PlayGlitchSound();
                StartCoroutine(GlitchEffect());
            }

            yield return StartCoroutine(FadeIn());

            float displayDuration = message.displayDuration > 0 ? message.displayDuration : 3f;
            yield return new WaitForSeconds(displayDuration);

            yield return StartCoroutine(FadeOut());

            if (overlayPanel != null)
            {
                overlayPanel.SetActive(false);
            }

            isDisplaying = false;
        }

        private System.Collections.IEnumerator FadeIn()
        {
            if (canvasGroup == null)
                yield break;

            float elapsed = 0f;
            canvasGroup.alpha = 0f;

            while (elapsed < fadeInDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / fadeInDuration;
                canvasGroup.alpha = Mathf.Lerp(0f, maxOpacity, t);
                yield return null;
            }

            canvasGroup.alpha = maxOpacity;
        }

        private System.Collections.IEnumerator FadeOut()
        {
            if (canvasGroup == null)
                yield break;

            float elapsed = 0f;
            float startAlpha = canvasGroup.alpha;

            while (elapsed < fadeOutDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / fadeOutDuration;
                canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, t);
                yield return null;
            }

            canvasGroup.alpha = 0f;
        }

        private System.Collections.IEnumerator GlitchEffect()
        {
            if (glitchOverlay == null)
                yield break;

            float duration = 0.3f;
            float elapsed = 0f;

            Color originalColor = glitchColor;
            glitchOverlay.color = originalColor;
            glitchOverlay.enabled = true;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                
                float intensity = Mathf.Sin(elapsed * 30f) * glitchIntensity * effectIntensityMultiplier;
                Color flickerColor = originalColor;
                flickerColor.a = Mathf.Abs(intensity) * 0.5f;
                glitchOverlay.color = flickerColor;

                if (messageText != null)
                {
                    Vector3 offset = new Vector3(
                        Random.Range(-2f, 2f) * intensity,
                        Random.Range(-2f, 2f) * intensity,
                        0f
                    );
                    messageText.transform.localPosition += offset;
                }

                yield return null;
            }

            if (glitchOverlay != null)
            {
                glitchOverlay.enabled = false;
            }

            if (messageText != null)
            {
                messageText.transform.localPosition = Vector3.zero;
            }
        }

        private void PlayGlitchSound()
        {
            if (glitchSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(glitchSound, 0.5f);
            }

            if (staticSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(staticSound, 0.3f * effectIntensityMultiplier);
            }
        }

        private void SetMessagePosition(MessagePosition position)
        {
            if (overlayPanel == null)
                return;

            RectTransform rectTransform = overlayPanel.GetComponent<RectTransform>();
            if (rectTransform == null)
                return;

            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.pivot = new Vector2(0.5f, 0.5f);

            switch (position)
            {
                case MessagePosition.TopLeft:
                    rectTransform.anchorMin = new Vector2(0f, 1f);
                    rectTransform.anchorMax = new Vector2(0.3f, 1f);
                    rectTransform.pivot = new Vector2(0f, 1f);
                    break;

                case MessagePosition.TopRight:
                    rectTransform.anchorMin = new Vector2(0.7f, 1f);
                    rectTransform.anchorMax = new Vector2(1f, 1f);
                    rectTransform.pivot = new Vector2(1f, 1f);
                    break;

                case MessagePosition.BottomLeft:
                    rectTransform.anchorMin = new Vector2(0f, 0f);
                    rectTransform.anchorMax = new Vector2(0.3f, 0.2f);
                    rectTransform.pivot = new Vector2(0f, 0f);
                    break;

                case MessagePosition.BottomRight:
                    rectTransform.anchorMin = new Vector2(0.7f, 0f);
                    rectTransform.anchorMax = new Vector2(1f, 0.2f);
                    rectTransform.pivot = new Vector2(1f, 0f);
                    break;

                case MessagePosition.Center:
                    rectTransform.anchorMin = new Vector2(0.2f, 0.4f);
                    rectTransform.anchorMax = new Vector2(0.8f, 0.6f);
                    rectTransform.pivot = new Vector2(0.5f, 0.5f);
                    break;
            }

            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
        }

        public void SetGlitchIntensity(float intensity)
        {
            glitchIntensity = Mathf.Clamp01(intensity);
        }

        public void SetMessagePosition(int positionIndex)
        {
            if (positionIndex >= 0 && positionIndex <= 4)
            {
                SetMessagePosition((MessagePosition)positionIndex);
            }
        }
    }
}
