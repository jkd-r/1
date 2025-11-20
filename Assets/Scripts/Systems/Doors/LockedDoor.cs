using UnityEngine;

namespace ProtocolEMR.Systems
{
    /// <summary>
    /// A door with lock states: Unlocked, physically locked, or electronically locked.
    /// Requires appropriate tools or actions to unlock.
    /// </summary>
    public class LockedDoor : InteractableDoor
    {
        public enum LockType
        {
            None,
            Physical,
            Electronic
        }

        [Header("Lock Settings")]
        [SerializeField] private LockType lockType = LockType.None;
        [SerializeField] private bool startLocked = false;
        [SerializeField] private float unlockDuration = 2.0f;
        [SerializeField] private bool requiresCorrectTool = true;

        [Header("Lock Audio")]
        [SerializeField] private AudioClip lockSound;
        [SerializeField] private AudioClip unlockSound;
        [SerializeField] private AudioClip hackingSound;
        [SerializeField] private float lockSoundVolume = 0.7f;

        private bool isLocked;
        private bool isUnlocking = false;
        private float unlockTimer = 0f;
        private AudioSource audioSource;

        private void OnEnable()
        {
            if (audioSource == null)
            {
                audioSource = GetComponent<AudioSource>();
                if (audioSource == null)
                {
                    audioSource = gameObject.AddComponent<AudioSource>();
                    audioSource.spatialBlend = 1f;
                    audioSource.maxDistance = 50f;
                }
            }

            isLocked = startLocked;
            UpdateInteractionMessage();
        }

        private void Update()
        {
            if (isUnlocking)
            {
                unlockTimer += Time.deltaTime;
                if (unlockTimer >= unlockDuration)
                {
                    CompleteUnlock();
                }
            }
        }

        protected override void PerformInteraction(GameObject interactor)
        {
            if (isLocked)
            {
                AttemptUnlock();
            }
            else
            {
                base.PerformInteraction(interactor);
            }
        }

        /// <summary>
        /// Attempts to unlock the door with appropriate tools.
        /// </summary>
        private void AttemptUnlock()
        {
            InventoryManager inventory = InventoryManager.Instance;
            if (inventory == null)
            {
                Debug.LogError("InventoryManager not found!");
                return;
            }

            bool canUnlock = false;

            switch (lockType)
            {
                case LockType.Physical:
                    if (inventory.HasTool(Item.ToolType.Lockpick))
                    {
                        canUnlock = true;
                        if (hackingSound != null && audioSource != null)
                        {
                            audioSource.PlayOneShot(lockSound, lockSoundVolume);
                        }
                    }
                    break;

                case LockType.Electronic:
                    if (inventory.HasTool(Item.ToolType.HackingDevice))
                    {
                        canUnlock = true;
                        if (hackingSound != null && audioSource != null)
                        {
                            audioSource.PlayOneShot(hackingSound, lockSoundVolume);
                        }
                    }
                    break;

                case LockType.None:
                    canUnlock = true;
                    break;
            }

            if (canUnlock)
            {
                BeginUnlock();
            }
            else
            {
                ShowCannotUnlockMessage();
            }
        }

        /// <summary>
        /// Begins the unlocking process.
        /// </summary>
        private void BeginUnlock()
        {
            if (isUnlocking) return;

            isUnlocking = true;
            unlockTimer = 0f;

            Debug.Log($"Unlocking {gameObject.name}...");
        }

        /// <summary>
        /// Completes the unlock process.
        /// </summary>
        private void CompleteUnlock()
        {
            isUnlocking = false;
            isLocked = false;

            if (unlockSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(unlockSound, lockSoundVolume);
            }

            UpdateInteractionMessage();

            Debug.Log($"{gameObject.name} is now unlocked!");
        }

        /// <summary>
        /// Shows a message when door cannot be unlocked.
        /// </summary>
        private void ShowCannotUnlockMessage()
        {
            string requirement = lockType == LockType.Physical ? "Requires Lockpick" : "Requires Hacking Device";
            if (InteractionPromptUI.Instance != null)
            {
                InteractionPromptUI.Instance.ShowPrompt($"Press E to Examine ({requirement})");
            }
        }

        /// <summary>
        /// Updates the interaction message based on lock state.
        /// </summary>
        private void UpdateInteractionMessage()
        {
            if (isLocked)
            {
                interactionMessage = "Press E to Examine (Locked)";
            }
            else
            {
                interactionMessage = "Press E to Open";
            }
        }

        /// <summary>
        /// Manually unlocks the door (e.g., via puzzle solution).
        /// </summary>
        public void ForceUnlock()
        {
            if (isLocked)
            {
                isLocked = false;
                isUnlocking = false;
                unlockTimer = 0f;

                if (unlockSound != null && audioSource != null)
                {
                    audioSource.PlayOneShot(unlockSound, lockSoundVolume);
                }

                UpdateInteractionMessage();

                Debug.Log($"{gameObject.name} force unlocked!");
            }
        }

        /// <summary>
        /// Manually locks the door.
        /// </summary>
        public void ForceLock()
        {
            if (!isLocked)
            {
                isLocked = true;
                isUnlocking = false;
                unlockTimer = 0f;

                if (lockSound != null && audioSource != null)
                {
                    audioSource.PlayOneShot(lockSound, lockSoundVolume);
                }

                UpdateInteractionMessage();

                Debug.Log($"{gameObject.name} force locked!");
            }
        }

        public bool IsLocked => isLocked;
        public bool IsUnlocking => isUnlocking;
    }
}
