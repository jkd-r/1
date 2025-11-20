using UnityEngine;
using TMPro;
using System.Collections;

namespace ProtocolEMR.Systems
{
    /// <summary>
    /// Displays pickup notifications when items are collected.
    /// Shows a temporary message that fades out after a duration.
    /// </summary>
    public class ItemPickupNotification : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI notificationText;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private float fadeDuration = 0.5f;

        private Coroutine currentCoroutine;

        private static ItemPickupNotification instance;
        public static ItemPickupNotification Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<ItemPickupNotification>();
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

            if (notificationText == null)
                notificationText = GetComponentInChildren<TextMeshProUGUI>();

            if (canvasGroup == null)
                canvasGroup = GetComponent<CanvasGroup>();

            if (canvasGroup == null)
                canvasGroup = gameObject.AddComponent<CanvasGroup>();

            canvasGroup.alpha = 0f;
        }

        /// <summary>
        /// Shows a notification with the given message for a duration.
        /// </summary>
        public void ShowNotification(string message, float displayDuration)
        {
            if (currentCoroutine != null)
            {
                StopCoroutine(currentCoroutine);
            }

            if (notificationText != null)
            {
                notificationText.text = message;
            }

            currentCoroutine = StartCoroutine(ShowNotification_Internal(displayDuration));
        }

        private IEnumerator ShowNotification_Internal(float displayDuration)
        {
            // Fade in
            float elapsedTime = 0f;
            while (elapsedTime < fadeDuration)
            {
                elapsedTime += Time.deltaTime;
                canvasGroup.alpha = Mathf.Clamp01(elapsedTime / fadeDuration);
                yield return null;
            }

            canvasGroup.alpha = 1f;

            // Display
            yield return new WaitForSeconds(displayDuration - fadeDuration);

            // Fade out
            elapsedTime = 0f;
            while (elapsedTime < fadeDuration)
            {
                elapsedTime += Time.deltaTime;
                canvasGroup.alpha = Mathf.Clamp01(1f - (elapsedTime / fadeDuration));
                yield return null;
            }

            canvasGroup.alpha = 0f;
        }
    }
}
