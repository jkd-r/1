using UnityEngine;
using TMPro;

namespace ProtocolEMR.Systems
{
    /// <summary>
    /// Interactive terminal that displays information and can unlock doors or trigger events.
    /// </summary>
    public class Terminal : InteractableObject
    {
        [Header("Terminal Content")]
        [SerializeField] private string terminalTitle = "Terminal";
        [TextArea(5, 10)]
        [SerializeField] private string terminalText = "System online. Enter commands.";
        [SerializeField] private bool requiresHacking = false;
        [SerializeField] private float hackingDuration = 3.0f;

        [Header("Terminal Audio")]
        [SerializeField] private AudioClip bootSound;
        [SerializeField] private AudioClip hackingLoopSound;
        [SerializeField] private AudioClip hackingCompleteSound;
        [SerializeField] private float audioVolume = 0.7f;

        [Header("Terminal Display")]
        [SerializeField] private TextMeshProUGUI displayText;
        [SerializeField] private CanvasGroup canvasGroup;

        private bool isHacking = false;
        private float hackingTimer = 0f;
        private bool hasBeenUsed = false;
        private AudioSource audioSource;

        private void Awake()
        {
            base.Awake();

            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.spatialBlend = 0.5f;
                audioSource.maxDistance = 30f;
            }

            interactionMessage = "Press E to Access Terminal";
        }

        private void Update()
        {
            if (isHacking)
            {
                hackingTimer += Time.deltaTime;
                if (hackingTimer >= hackingDuration)
                {
                    CompleteHacking();
                }
            }
        }

        protected override void PerformInteraction(GameObject interactor)
        {
            if (requiresHacking && !hasBeenUsed)
            {
                InventoryManager inventory = InventoryManager.Instance;
                if (inventory == null || !inventory.HasTool(Item.ToolType.HackingDevice))
                {
                    ShowAccessDenied();
                    return;
                }

                BeginHacking();
            }
            else
            {
                AccessTerminal();
            }
        }

        /// <summary>
        /// Begins the hacking process.
        /// </summary>
        private void BeginHacking()
        {
            isHacking = true;
            hackingTimer = 0f;

            if (bootSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(bootSound, audioVolume);
            }

            if (InteractionPromptUI.Instance != null)
            {
                InteractionPromptUI.Instance.ShowPrompt("Hacking...");
            }

            Debug.Log($"Hacking terminal {gameObject.name}...");
        }

        /// <summary>
        /// Completes the hacking process.
        /// </summary>
        private void CompleteHacking()
        {
            isHacking = false;
            hasBeenUsed = true;

            if (hackingCompleteSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(hackingCompleteSound, audioVolume);
            }

            AccessTerminal();
        }

        /// <summary>
        /// Accesses the terminal and displays content.
        /// </summary>
        private void AccessTerminal()
        {
            Debug.Log($"Terminal {gameObject.name} accessed!");
            Debug.Log($"[{terminalTitle}]");
            Debug.Log(terminalText);

            if (displayText != null)
            {
                displayText.text = $"[{terminalTitle}]\n\n{terminalText}";
            }

            if (canvasGroup != null)
            {
                canvasGroup.alpha = 1f;
            }

            PerformTerminalAction();
        }

        /// <summary>
        /// Shows access denied message.
        /// </summary>
        private void ShowAccessDenied()
        {
            if (InteractionPromptUI.Instance != null)
            {
                InteractionPromptUI.Instance.ShowPrompt("Access Denied (Requires Hacking Device)");
            }

            Debug.Log($"Access denied to terminal {gameObject.name}");
        }

        /// <summary>
        /// Override this to perform custom actions when terminal is accessed.
        /// </summary>
        protected virtual void PerformTerminalAction()
        {
            // Override in subclasses
        }

        public string GetTerminalContent()
        {
            return $"[{terminalTitle}]\n\n{terminalText}";
        }

        public void SetTerminalContent(string title, string content)
        {
            terminalTitle = title;
            terminalText = content;
        }

        public bool HasBeenUsed => hasBeenUsed;
        public bool IsHacking => isHacking;
    }
}
