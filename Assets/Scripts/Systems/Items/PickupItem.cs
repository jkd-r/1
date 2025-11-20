using UnityEngine;
using ProtocolEMR.Core.Player;

namespace ProtocolEMR.Systems
{
    /// <summary>
    /// Represents a pickup item in the world that can be collected into the inventory.
    /// Handles item pickup, feedback, and world state management.
    /// </summary>
    public class PickupItem : InteractableObject
    {
        [Header("Item Properties")]
        [SerializeField] private Item itemData;
        [SerializeField] private int pickupCount = 1;
        [SerializeField] private bool removeAfterPickup = true;
        [SerializeField] private bool singleUseOnly = true;

        [Header("Pickup Feedback")]
        [SerializeField] private AudioClip pickupSound;
        [SerializeField] private float pickupSoundVolume = 0.8f;
        [SerializeField] private float pickupAnimationDuration = 0.5f;
        [SerializeField] private bool usePickupAnimation = true;

        [Header("Contextual Messages")]
        [SerializeField] private string weaponPickupMessage = "A tool. Use it wisely.";
        [SerializeField] private string documentPickupMessage = "Interesting. Add this to your knowledge.";
        [SerializeField] private string consumablePickupMessage = "This may prove useful.";
        [SerializeField] private string toolPickupMessage = "A useful device.";
        [SerializeField] private string miscPickupMessage = "What is this?";

        private bool hasBeenPickedUp = false;
        private Vector3 originalPosition;
        private Collider pickupCollider;

        private void Awake()
        {
            if (itemData == null)
            {
                Debug.LogError($"PickupItem '{gameObject.name}' has no itemData assigned!");
                return;
            }

            originalPosition = transform.position;
            pickupCollider = GetComponent<Collider>();

            // Set up interaction message
            string action = "Pick up";
            switch (itemData.Type)
            {
                case Item.ItemType.Weapon:
                    interactionMessage = $"Press E to {action} {itemData.Name}";
                    break;
                case Item.ItemType.Tool:
                    interactionMessage = $"Press E to {action} {itemData.Name}";
                    break;
                case Item.ItemType.Consumable:
                    interactionMessage = $"Press E to {action} {itemData.Name}";
                    break;
                case Item.ItemType.Document:
                    interactionMessage = $"Press E to {action} {itemData.Name}";
                    break;
                default:
                    interactionMessage = $"Press E to {action}";
                    break;
            }

            base.Awake();
        }

        private void Start()
        {
            if (singleUseOnly && canInteractMultipleTimes)
            {
                canInteractMultipleTimes = false;
            }
        }

        protected override void PerformInteraction(GameObject interactor)
        {
            if (hasBeenPickedUp) return;

            hasBeenPickedUp = true;

            PickupItem_Internal();
        }

        /// <summary>
        /// Internal pickup logic.
        /// </summary>
        private void PickupItem_Internal()
        {
            InventoryManager inventory = InventoryManager.Instance;
            if (inventory == null)
            {
                Debug.LogError("InventoryManager not found!");
                return;
            }

            bool successfullyAdded = inventory.AddItem(itemData, pickupCount);

            if (successfullyAdded)
            {
                PlayPickupFeedback();
                ShowPickupNotification();

                if (usePickupAnimation)
                {
                    StartCoroutine(PlayPickupAnimation());
                }
                else if (removeAfterPickup)
                {
                    Destroy(gameObject);
                }
            }
            else
            {
                Debug.LogWarning($"Could not pick up {itemData.Name}: inventory full or weight exceeded");
                hasBeenPickedUp = false;
            }
        }

        /// <summary>
        /// Plays pickup audio and haptic feedback.
        /// </summary>
        private void PlayPickupFeedback()
        {
            if (pickupSound != null)
            {
                AudioSource.PlayClipAtPoint(pickupSound, transform.position, pickupSoundVolume);
            }

            // Optional: Add haptic feedback here
            // GamepadVibration.Instance?.PlayBrief();
        }

        /// <summary>
        /// Shows a pickup notification in the UI.
        /// </summary>
        private void ShowPickupNotification()
        {
            string message = $"Item picked up: {itemData.Name}";
            if (ItemPickupNotification.Instance != null)
            {
                ItemPickupNotification.Instance.ShowNotification(message, 2.5f);
            }
        }

        /// <summary>
        /// Plays pickup animation (shrink and fade out).
        /// </summary>
        private System.Collections.IEnumerator PlayPickupAnimation()
        {
            float elapsedTime = 0f;
            Vector3 startScale = transform.localScale;
            Color startColor = Color.white;

            if (GetComponent<Renderer>() != null)
            {
                startColor = GetComponent<Renderer>().material.color;
            }

            if (pickupCollider != null)
            {
                pickupCollider.enabled = false;
            }

            while (elapsedTime < pickupAnimationDuration)
            {
                elapsedTime += Time.deltaTime;
                float progress = elapsedTime / pickupAnimationDuration;

                // Scale down
                transform.localScale = Vector3.Lerp(startScale, Vector3.zero, progress);

                // Fade out
                if (GetComponent<Renderer>() != null)
                {
                    Renderer renderer = GetComponent<Renderer>();
                    Color newColor = startColor;
                    newColor.a = Mathf.Lerp(1f, 0f, progress);
                    renderer.material.color = newColor;
                }

                // Move up slightly
                transform.Translate(Vector3.up * 2f * Time.deltaTime, Space.World);

                yield return null;
            }

            if (removeAfterPickup)
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// Gets the item data for this pickup.
        /// </summary>
        public Item GetItemData()
        {
            return itemData;
        }

        /// <summary>
        /// Resets this item as if not picked up.
        /// </summary>
        public void Reset()
        {
            hasBeenPickedUp = false;
            transform.position = originalPosition;
            transform.localScale = Vector3.one;

            if (GetComponent<Renderer>() != null)
            {
                Renderer renderer = GetComponent<Renderer>();
                Color color = renderer.material.color;
                color.a = 1f;
                renderer.material.color = color;
            }

            if (pickupCollider != null)
            {
                pickupCollider.enabled = true;
            }

            ResetInteraction();
        }

        public bool HasBeenPickedUp => hasBeenPickedUp;

        private void OnValidate()
        {
            if (singleUseOnly && canInteractMultipleTimes)
            {
                canInteractMultipleTimes = false;
            }
        }
    }
}
