using UnityEngine;
using ProtocolEMR.Core.Input;
using ProtocolEMR.Systems;

namespace ProtocolEMR.Core.Player
{
    /// <summary>
    /// Core player movement controller with WASD input, sprint stamina system, and interaction raycasting.
    /// Foundation for Sprint 2 animation integration and Sprint 3 combat mechanics.
    /// Uses Unity's CharacterController for physics-based movement with collision detection.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private float walkSpeed = 5.0f;
        [SerializeField] private float sprintSpeed = 8.0f;
        [SerializeField] private float crouchSpeed = 2.5f;
        [SerializeField] private float jumpHeight = 2.0f;
        [SerializeField] private float gravity = -9.81f;

        [Header("Stamina System")]
        [SerializeField] private float maxStamina = 100f;
        [SerializeField] private float staminaDrainRate = 10f;
        [SerializeField] private float staminaRegenRate = 15f;
        [SerializeField] private float staminaRegenDelay = 1.0f;

        [Header("Crouch Settings")]
        [SerializeField] private float standingHeight = 2.0f;
        [SerializeField] private float crouchHeight = 1.0f;
        [SerializeField] private float crouchTransitionSpeed = 10f;

        [Header("Interaction System")]
        [SerializeField] private float interactionRange = 3.0f;
        [SerializeField] private LayerMask interactableLayer;

        private CharacterController controller;
        private Vector3 velocity;
        private bool isGrounded;
        private float currentStamina;
        private float staminaRegenTimer;
        private bool isSprinting;
        private bool isCrouching;
        private bool isPaused;
        private float currentHeight;

        public float CurrentStamina => currentStamina;
        public float MaxStamina => maxStamina;
        public bool IsSprinting => isSprinting;
        public bool IsCrouching => isCrouching;
        public bool IsGrounded => isGrounded;

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
            currentStamina = maxStamina;
            currentHeight = standingHeight;
        }

        private void Start()
        {
            if (InputManager.Instance != null)
            {
                InputManager.Instance.OnMove += HandleMovement;
                InputManager.Instance.OnSprintPressed += OnSprintPressed;
                InputManager.Instance.OnSprintReleased += OnSprintReleased;
                InputManager.Instance.OnCrouchPressed += OnCrouchPressed;
                InputManager.Instance.OnCrouchReleased += OnCrouchReleased;
                InputManager.Instance.OnJump += OnJump;
                InputManager.Instance.OnInteract += OnInteract;
                InputManager.Instance.OnPause += TogglePause;
            }
        }

        private void OnDestroy()
        {
            if (InputManager.Instance != null)
            {
                InputManager.Instance.OnMove -= HandleMovement;
                InputManager.Instance.OnSprintPressed -= OnSprintPressed;
                InputManager.Instance.OnSprintReleased -= OnSprintReleased;
                InputManager.Instance.OnCrouchPressed -= OnCrouchPressed;
                InputManager.Instance.OnCrouchReleased -= OnCrouchReleased;
                InputManager.Instance.OnJump -= OnJump;
                InputManager.Instance.OnInteract -= OnInteract;
                InputManager.Instance.OnPause -= TogglePause;
            }
        }

        private void Update()
        {
            if (isPaused) return;

            HandleGroundCheck();
            HandleGravity();
            HandleStamina();
            HandleCrouchTransition();
        }

        private void HandleMovement(Vector2 moveInput)
        {
            if (isPaused) return;

            isGrounded = controller.isGrounded;

            Vector3 move = transform.right * moveInput.x + transform.forward * moveInput.y;

            float currentSpeed = walkSpeed;
            if (isCrouching)
            {
                currentSpeed = crouchSpeed;
            }
            else if (isSprinting && currentStamina > 0f)
            {
                currentSpeed = sprintSpeed;
            }

            controller.Move(move * currentSpeed * Time.deltaTime);
        }

        private void HandleGroundCheck()
        {
            isGrounded = controller.isGrounded;
        }

        private void HandleGravity()
        {
            if (isGrounded && velocity.y < 0)
            {
                velocity.y = -2f;
            }

            velocity.y += gravity * Time.deltaTime;
            controller.Move(velocity * Time.deltaTime);
        }

        private void HandleStamina()
        {
            if (isSprinting && !isCrouching && InputManager.Instance.GetMovementInput().magnitude > 0.1f)
            {
                currentStamina -= staminaDrainRate * Time.deltaTime;
                currentStamina = Mathf.Max(currentStamina, 0f);
                staminaRegenTimer = staminaRegenDelay;

                if (currentStamina <= 0f)
                {
                    isSprinting = false;
                }
            }
            else
            {
                if (staminaRegenTimer > 0f)
                {
                    staminaRegenTimer -= Time.deltaTime;
                }
                else
                {
                    currentStamina += staminaRegenRate * Time.deltaTime;
                    currentStamina = Mathf.Min(currentStamina, maxStamina);
                }
            }
        }

        private void HandleCrouchTransition()
        {
            float targetHeight = isCrouching ? crouchHeight : standingHeight;
            currentHeight = Mathf.Lerp(currentHeight, targetHeight, Time.deltaTime * crouchTransitionSpeed);
            controller.height = currentHeight;

            Vector3 center = controller.center;
            center.y = currentHeight / 2f;
            controller.center = center;
        }

        private void OnSprintPressed()
        {
            if (InputManager.Instance.EnableHoldToSprint)
            {
                isSprinting = true;
            }
            else
            {
                isSprinting = !isSprinting;
            }
        }

        private void OnSprintReleased()
        {
            if (InputManager.Instance.EnableHoldToSprint)
            {
                isSprinting = false;
            }
        }

        private void OnCrouchPressed()
        {
            if (InputManager.Instance.EnableHoldToCrouch)
            {
                isCrouching = true;
            }
            else
            {
                isCrouching = !isCrouching;
            }
        }

        private void OnCrouchReleased()
        {
            if (InputManager.Instance.EnableHoldToCrouch)
            {
                if (CanStandUp())
                {
                    isCrouching = false;
                }
            }
        }

        private bool CanStandUp()
        {
            Vector3 checkPosition = transform.position + Vector3.up * (standingHeight - crouchHeight);
            return !Physics.CheckSphere(checkPosition, controller.radius);
        }

        private void OnJump()
        {
            if (isGrounded && !isCrouching)
            {
                velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
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
            isPaused = !isPaused;
        }

        public void ResetStamina()
        {
            currentStamina = maxStamina;
        }

        /// <summary>
        /// Attempts to consume stamina for actions like combat attacks.
        /// </summary>
        /// <param name="amount">Amount of stamina to consume</param>
        /// <returns>True if stamina was available and consumed</returns>
        public bool ConsumesStamina(float amount)
        {
            if (currentStamina >= amount)
            {
                currentStamina -= amount;
                staminaRegenTimer = staminaRegenDelay;
                return true;
            }
            return false;
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
