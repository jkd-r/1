using UnityEngine;
using ProtocolEMR.Core.Player;

namespace ProtocolEMR.Systems
{
    /// <summary>
    /// Base interactable object that implements the IInteractable and IHighlightable interfaces.
    /// Demonstrates how to create objects the player can interact with using the E key.
    /// Can be attached to any GameObject with a Collider to make it interactable.
    /// </summary>
    public class InteractableObject : MonoBehaviour, IInteractable, IHighlightable
    {
        [Header("Interaction Settings")]
        [SerializeField] protected string interactionMessage = "Press E to interact";
        [SerializeField] protected bool canInteractMultipleTimes = true;
        [SerializeField] protected float interactionCooldown = 1.0f;

        [Header("Visual Feedback")]
        [SerializeField] private Color highlightColor = Color.yellow;
        [SerializeField] private bool showHighlight = true;
        [SerializeField] private float glowIntensity = 1.5f;

        [Header("Audio Feedback")]
        [SerializeField] private AudioClip highlightSound;
        [SerializeField] private float highlightSoundVolume = 0.5f;

        private bool hasInteracted = false;
        private float lastInteractionTime = -999f;
        private Renderer objectRenderer;
        private Color originalColor;
        private bool isHighlighted = false;

        private void Awake()
        {
            objectRenderer = GetComponent<Renderer>();
            if (objectRenderer != null)
            {
                originalColor = objectRenderer.material.color;
            }

            if (GetComponent<Collider>() == null)
            {
                Debug.LogWarning($"InteractableObject '{gameObject.name}' has no Collider. Add a Collider component for raycast detection.");
            }
        }

        public void OnInteract(GameObject interactor)
        {
            if (!canInteractMultipleTimes && hasInteracted)
            {
                Debug.Log($"{gameObject.name}: Already interacted, cannot interact again");
                return;
            }

            if (Time.time - lastInteractionTime < interactionCooldown)
            {
                Debug.Log($"{gameObject.name}: Interaction on cooldown");
                return;
            }

            lastInteractionTime = Time.time;
            hasInteracted = true;

            Debug.Log($"{gameObject.name}: Interacted by {interactor.name}");

            PerformInteraction(interactor);
        }

        protected virtual void PerformInteraction(GameObject interactor)
        {
            Debug.Log($"{gameObject.name}: Default interaction performed");
        }

        public void OnHighlight()
        {
            if (isHighlighted) return;

            isHighlighted = true;

            if (showHighlight && objectRenderer != null)
            {
                objectRenderer.material.color = highlightColor * glowIntensity;
            }

            if (highlightSound != null && AudioListener.volume > 0)
            {
                AudioSource.PlayClipAtPoint(highlightSound, transform.position, highlightSoundVolume);
            }
        }

        public void OnUnhighlight()
        {
            if (!isHighlighted) return;

            isHighlighted = false;

            if (showHighlight && objectRenderer != null)
            {
                objectRenderer.material.color = originalColor;
            }
        }

        public string GetInteractionMessage()
        {
            return interactionMessage;
        }

        public bool CanInteract()
        {
            if (!canInteractMultipleTimes && hasInteracted)
                return false;

            if (Time.time - lastInteractionTime < interactionCooldown)
                return false;

            return true;
        }

        public void ResetInteraction()
        {
            hasInteracted = false;
            lastInteractionTime = -999f;
        }

        public bool IsHighlighted => isHighlighted;
    }
}
