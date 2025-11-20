using UnityEngine;
using ProtocolEMR.Core.Player;

namespace ProtocolEMR.Systems
{
    /// <summary>
    /// Manages interaction detection via raycasting and updates the interaction prompt UI.
    /// Detects nearby interactable objects and displays appropriate prompts.
    /// </summary>
    public class InteractionManager : MonoBehaviour
    {
        [Header("Interaction Settings")]
        [SerializeField] private float interactionRange = 3.0f;
        [SerializeField] private LayerMask interactableLayer;
        [SerializeField] private float updateInterval = 0.1f;

        private IInteractable currentInteractable;
        private float nextUpdateTime;
        private Camera mainCamera;

        private static InteractionManager instance;
        public static InteractionManager Instance
        {
            get
            {
                if (instance == null)
                {
                    GameObject managerObject = new GameObject("InteractionManager");
                    instance = managerObject.AddComponent<InteractionManager>();
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

            mainCamera = Camera.main;
            if (mainCamera == null)
            {
                Debug.LogError("InteractionManager: No main camera found!");
            }

            nextUpdateTime = Time.time;
        }

        private void Update()
        {
            if (mainCamera == null) return;

            // Update interaction detection at intervals for performance
            if (Time.time >= nextUpdateTime)
            {
                UpdateInteractionDetection();
                nextUpdateTime = Time.time + updateInterval;
            }
        }

        /// <summary>
        /// Updates interaction detection via raycast.
        /// </summary>
        private void UpdateInteractionDetection()
        {
            Ray ray = new Ray(mainCamera.transform.position, mainCamera.transform.forward);
            RaycastHit hit;
            IInteractable newInteractable = null;

            if (Physics.Raycast(ray, out hit, interactionRange, interactableLayer))
            {
                newInteractable = hit.collider.GetComponent<IInteractable>();
            }

            // If we switched interactables, update feedback
            if (newInteractable != currentInteractable)
            {
                if (currentInteractable != null)
                {
                    OnInteractableLeft();
                }

                currentInteractable = newInteractable;

                if (currentInteractable != null)
                {
                    OnInteractableEntered();
                }
            }
        }

        /// <summary>
        /// Called when entering range of an interactable object.
        /// </summary>
        private void OnInteractableEntered()
        {
            if (currentInteractable != null && currentInteractable.CanInteract())
            {
                string prompt = currentInteractable.GetInteractionMessage();
                if (InteractionPromptUI.Instance != null)
                {
                    InteractionPromptUI.Instance.ShowPrompt(prompt);
                }

                var interactable = currentInteractable as MonoBehaviour;
                if (interactable != null)
                {
                    // Notify the object it's being highlighted
                    var highlightable = interactable.GetComponent<IHighlightable>();
                    if (highlightable != null)
                    {
                        highlightable.OnHighlight();
                    }
                }
            }
        }

        /// <summary>
        /// Called when leaving range of an interactable object.
        /// </summary>
        private void OnInteractableLeft()
        {
            if (InteractionPromptUI.Instance != null)
            {
                InteractionPromptUI.Instance.HidePrompt();
            }

            var interactable = currentInteractable as MonoBehaviour;
            if (interactable != null)
            {
                // Notify the object it's no longer highlighted
                var highlightable = interactable.GetComponent<IHighlightable>();
                if (highlightable != null)
                {
                    highlightable.OnUnhighlight();
                }
            }

            currentInteractable = null;
        }

        /// <summary>
        /// Gets the currently highlighted interactable.
        /// </summary>
        public IInteractable GetCurrentInteractable()
        {
            return currentInteractable;
        }

        /// <summary>
        /// Sets the interaction range.
        /// </summary>
        public void SetInteractionRange(float range)
        {
            interactionRange = range;
        }

        private void OnDrawGizmosSelected()
        {
            if (mainCamera != null)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawRay(mainCamera.transform.position, mainCamera.transform.forward * interactionRange);
            }
        }
    }
}
