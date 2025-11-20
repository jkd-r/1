using UnityEngine;
using ProtocolEMR.Core.Player;

namespace ProtocolEMR.Systems
{
    /// <summary>
    /// Example interactable object that implements the IInteractable interface.
    /// Demonstrates how to create objects the player can interact with using the E key.
    /// Can be attached to any GameObject with a Collider to make it interactable.
    /// </summary>
    public class InteractableObject : MonoBehaviour, IInteractable
    {
        [Header("Interaction Settings")]
        [SerializeField] private string interactionMessage = "Press E to interact";
        [SerializeField] private bool canInteractMultipleTimes = true;
        [SerializeField] private float interactionCooldown = 1.0f;

        [Header("Visual Feedback")]
        [SerializeField] private Color highlightColor = Color.yellow;
        [SerializeField] private bool showHighlight = true;

        private bool hasInteracted = false;
        private float lastInteractionTime = -999f;
        private Renderer objectRenderer;
        private Color originalColor;

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

        private void OnMouseEnter()
        {
            if (showHighlight && objectRenderer != null)
            {
                objectRenderer.material.color = highlightColor;
            }
        }

        private void OnMouseExit()
        {
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
    }
}
