using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ProtocolEMR.Systems
{
    /// <summary>
    /// Manages the display of interaction prompts near the player's crosshair.
    /// Shows contextual "Press E to [action]" messages for nearby interactable objects.
    /// </summary>
    public class InteractionPromptUI : MonoBehaviour
    {
        [Header("UI Settings")]
        [SerializeField] private TextMeshProUGUI promptText;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private float fadeSpeed = 5f;
        [SerializeField] private float targetAlpha = 1f;

        private string currentPrompt = "";
        private float desiredAlpha = 0f;

        private static InteractionPromptUI instance;
        public static InteractionPromptUI Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<InteractionPromptUI>();
                }
                return instance;
            }
        }

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }
            instance = this;

            if (promptText == null)
                promptText = GetComponentInChildren<TextMeshProUGUI>();

            if (canvasGroup == null)
                canvasGroup = GetComponent<CanvasGroup>();

            if (canvasGroup == null)
                canvasGroup = gameObject.AddComponent<CanvasGroup>();

            canvasGroup.alpha = 0f;
        }

        private void Update()
        {
            canvasGroup.alpha = Mathf.Lerp(canvasGroup.alpha, desiredAlpha, Time.deltaTime * fadeSpeed);
        }

        /// <summary>
        /// Shows an interaction prompt.
        /// </summary>
        public void ShowPrompt(string message)
        {
            if (currentPrompt != message)
            {
                currentPrompt = message;
                if (promptText != null)
                {
                    promptText.text = message;
                }
            }
            desiredAlpha = targetAlpha;
        }

        /// <summary>
        /// Hides the interaction prompt.
        /// </summary>
        public void HidePrompt()
        {
            desiredAlpha = 0f;
        }

        /// <summary>
        /// Gets the current prompt text.
        /// </summary>
        public string GetCurrentPrompt()
        {
            return currentPrompt;
        }
    }
}
