using UnityEngine;
using ProtocolEMR.Core.Input;
using ProtocolEMR.Core.Animation;
using ProtocolEMR.Core.Audio;

namespace ProtocolEMR.Core.Player
{
    /// <summary>
    /// Core player movement controller with WASD input, sprint stamina system, and interaction raycasting.
    /// Foundation for Sprint 2 animation integration and Sprint 3 combat mechanics.
    /// Uses Unity's CharacterController for physics-based movement with collision detection.
    /// Integrates animation controller for locomotion and audio manager for footsteps/breathing.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private float walkSpeed = 5.5f;
        [SerializeField] private float runSpeed = 8.0f;
        [SerializeField] private float sprintSpeed = 12.0f;
        [SerializeField] private float crouchSpeed = 3.0f;
        [SerializeField] private float jumpHeight = 5.5f;
        [SerializeField] private float gravity = -9.81f;
        [SerializeField] private float acceleration = 20f;
        [SerializeField] private float deceleration = 15f;

        [Header("Stamina System")]
        [SerializeField] private float maxStamina = 100f;
        [SerializeField] private float staminaDrainRate = 20f;
        [SerializeField] private float staminaRegenRate = 20f;
        [SerializeField] private float staminaRegenDelay = 0.5f;
        [SerializeField] private float sprintDuration = 5f;
        [SerializeField] private float sprintRecoveryTime = 3f;

        [Header("Crouch Settings")]
        [SerializeField] private float standingHeight = 1.8f;
        [SerializeField] private float crouchHeight = 1.2f;
        [SerializeField] private float crouchTransitionSpeed = 10f;

        [Header("Animation Settings")]
        [SerializeField] private AnimationController animationController;

        [Header("Audio Settings")]
        [SerializeField] private float footstepInterval = 0.5f;
        [SerializeField] private float sprintFootstepInterval = 0.35f;

        [Header("Interaction System")]
        [SerializeField] private float interactionRange = 3.0f;
        [SerializeField] private LayerMask interactableLayer;

        private CharacterController controller;
        private Vector3 velocity;
        private Vector3 lastPosition;
        private bool isGrounded;
        private float currentStamina;
        private float staminaRegenTimer;
        private float footstepTimer;
        private bool isSprinting;
        private bool isCrouching;
        private bool isPaused;
        private float currentHeight;
        private Vector2 currentMoveInput;
        private bool wasSprintingLastFrame;
        private int lastFootstepSoundTime;

        public float CurrentStamina => currentStamina;
        public float MaxStamina => maxStamina;
        public bool IsSprinting => isSprinting;
        public bool IsCrouching => isCrouching;
        public bool IsGrounded => isGrounded;
        public float CurrentSpeed => (transform.position - lastPosition).magnitude / Time.deltaTime;

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
            currentStamina = maxStamina;
            currentHeight = standingHeight;
            lastPosition = transform.position;

            if (animationController == null)
                animationController = GetComponent<AnimationController>();
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
            UpdateAnimations();
            HandleFootsteps();
            HandleBreathing();

            lastPosition = transform.position;
        }

        private void HandleMovement(Vector2 moveInput)
        {
            if (isPaused) return;

            currentMoveInput = moveInput;
            isGrounded = controller.isGrounded;

            Vector3 move = transform.right * moveInput.x + transform.forward * moveInput.y;

            float targetSpeed = walkSpeed;
            if (isCrouching)
            {
                targetSpeed = crouchSpeed;
            }
            else if (isSprinting && currentStamina > 0f && moveInput.magnitude > 0.1f)
            {
                targetSpeed = sprintSpeed;
            }
            else if (!isSprinting && moveInput.magnitude > 0.1f)
            {
                targetSpeed = runSpeed;
            }
            else if (moveInput.magnitude < 0.1f)
            {
                targetSpeed = 0f;
            }

            controller.Move(move * targetSpeed * Time.deltaTime);
        }

        private void UpdateAnimations()
        {
            if (animationController == null)
                return;

            float maxSpeed = isSprinting && currentStamina > 0f ? sprintSpeed : (currentMoveInput.magnitude > 0f ? runSpeed : walkSpeed);
            Vector3 moveDir = transform.right * currentMoveInput.x + transform.forward * currentMoveInput.y;

            animationController.SetLocomotionSpeed(moveDir, CurrentSpeed, maxSpeed);
            animationController.SetCrouching(isCrouching);
        }

        private void HandleFootsteps()
        {
            if (AudioManager.Instance == null || !isGrounded)
                return;

            float currentFootstepInterval = isSprinting ? sprintFootstepInterval : footstepInterval;

            if (currentMoveInput.magnitude > 0.1f)
            {
                footstepTimer -= Time.deltaTime;

                if (footstepTimer <= 0)
                {
                    AudioManager.Instance.PlayFootstep(transform.position, isSprinting);
                    footstepTimer = currentFootstepInterval;
                }
            }
            else
            {
                footstepTimer = currentFootstepInterval;
            }
        }

        private void HandleBreathing()
        {
            if (AudioManager.Instance == null)
                return;

            if (isSprinting && currentStamina > 0f && currentMoveInput.magnitude > 0.1f)
            {
                if (!wasSprintingLastFrame)
                {
                    AudioManager.Instance.PlayBreathing();
                }
                wasSprintingLastFrame = true;
            }
            else
            {
                if (wasSprintingLastFrame)
                {
                    AudioManager.Instance.StopBreathing();
                }
                wasSprintingLastFrame = false;
            }
        }

        private void HandleGroundCheck()
        {
            isGrounded = controller.isGrounded;
        }

        private void HandleGravity()
        {
            if (isGrounded && velocity.y < 0)
            {
                HandleJumpLanding();
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

                if (animationController != null)
                {
                    animationController.PlayJump();
                }

                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlayFootstep(transform.position);
                }
            }
        }

        private void HandleJumpLanding()
        {
            if (isGrounded && velocity.y < -2f && animationController != null)
            {
                animationController.PlayLanding();

                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlayLandingSound(transform.position);
                }

                velocity.y = -2f;
            }
        }

        private void OnInteract()
        {
            Ray ray = new Ray(UnityEngine.Camera.main.transform.position, UnityEngine.Camera.main.transform.forward);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, interactionRange, interactableLayer))
            {
                IInteractable interactable = hit.collider.GetComponent<IInteractable>();
                if (interactable != null)
                {
                    interactable.OnInteract(gameObject);
                    Debug.Log($"Interacted with: {hit.collider.name}");
                }
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
    }
}
