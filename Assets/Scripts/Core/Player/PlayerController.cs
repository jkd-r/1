using UnityEngine;
using ProtocolEMR.Core.Input;
using ProtocolEMR.Systems;

namespace ProtocolEMR.Core.Player
{
    /// <summary>
    /// Core player movement controller with WASD input, sprint stamina system, and interaction raycasting.
    /// Now acts as a bridge to the modular movement system (MovementController).
    /// Maintains backward compatibility while delegating to specialized subsystems.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Interaction System")]
        [SerializeField] private float interactionRange = 3.0f;
        [SerializeField] private LayerMask interactableLayer;

        private MovementController movementController;

        public float CurrentStamina => movementController?.CurrentStamina ?? 0f;
        public float MaxStamina => movementController?.MaxStamina ?? 100f;
        public bool IsSprinting => movementController?.IsSprinting ?? false;
        public bool IsCrouching => movementController?.IsCrouching ?? false;
        public bool IsGrounded => movementController?.IsGrounded ?? false;

        private void Awake()
        {
            movementController = GetComponent<MovementController>();
            if (movementController == null)
            {
                Debug.LogError("PlayerController requires MovementController component");
            }
        }

        private void Start()
        {
            if (InputManager.Instance != null)
            {
                InputManager.Instance.OnInteract += OnInteract;
                InputManager.Instance.OnPause += TogglePause;
            }
        }

        private void OnDestroy()
        {
            if (InputManager.Instance != null)
            {
                InputManager.Instance.OnInteract -= OnInteract;
                InputManager.Instance.OnPause -= TogglePause;
            }
        }

        private void OnInteract()
        {
            IInteractable interactable = InteractionManager.Instance?.GetCurrentInteractable();
            if (interactable != null && interactable.CanInteract())
            {
                interactable.OnInteract(gameObject);
                Debug.Log($"Interacted with interactable object");
            }
        }

        private void TogglePause()
        {
            // Pause logic handled by MovementController
        }

        public void ResetStamina()
        {
            movementController?.RestoreStamina();
        }

        private void OnDrawGizmosSelected()
        {
            if (UnityEngine.Camera.main != null)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawRay(UnityEngine.Camera.main.transform.position, UnityEngine.Camera.main.transform.forward * interactionRange);
            }
        }
    }

    /// <summary>
    /// Interface for interactable objects in the game world.
    /// Implement this interface on any object the player should be able to interact with.
    /// </summary>
    public interface IInteractable
    {
        void OnInteract(GameObject interactor);
        string GetInteractionMessage();
        bool CanInteract();
    }
}
