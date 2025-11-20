using UnityEngine;
using ProtocolEMR.Core.Input;

namespace ProtocolEMR.Core.Player
{
    /// <summary>
    /// Main movement orchestrator that coordinates all movement subsystems.
    /// Integrates CharacterLocomotion, JumpSystem, StaminaSystem, and SurfacePhysics.
    /// Provides a single interface for player movement control and feedback.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(CharacterLocomotion))]
    [RequireComponent(typeof(GroundDetection))]
    [RequireComponent(typeof(SurfacePhysics))]
    [RequireComponent(typeof(JumpSystem))]
    [RequireComponent(typeof(StaminaSystem))]
    [RequireComponent(typeof(MovementFeedback))]
    public class MovementController : MonoBehaviour
    {
        [Header("Control Settings")]
        [SerializeField] private bool enableMovement = true;
        [SerializeField] private bool enableJump = true;
        [SerializeField] private bool enableSprint = true;
        [SerializeField] private bool enableCrouch = true;

        [Header("Crouch Settings")]
        [SerializeField] private float standingHeight = 2.0f;
        [SerializeField] private float crouchHeight = 1.0f;
        [SerializeField] private float crouchTransitionSpeed = 10f;

        private CharacterController characterController;
        private CharacterLocomotion locomotion;
        private GroundDetection groundDetection;
        private SurfacePhysics surfacePhysics;
        private JumpSystem jumpSystem;
        private StaminaSystem staminaSystem;
        private MovementFeedback movementFeedback;

        private Vector2 moveInput = Vector2.zero;
        private bool isSprinting = false;
        private bool isCrouching = false;
        private bool isPaused = false;
        private float currentHeight;

        public bool IsSprinting => isSprinting;
        public bool IsCrouching => isCrouching;
        public bool IsMoving => locomotion.IsMoving;
        public bool IsGrounded => groundDetection.IsGrounded;
        public float CurrentStamina => staminaSystem.CurrentStamina;
        public float MaxStamina => staminaSystem.MaxStamina;

        private void Awake()
        {
            characterController = GetComponent<CharacterController>();
            locomotion = GetComponent<CharacterLocomotion>();
            groundDetection = GetComponent<GroundDetection>();
            surfacePhysics = GetComponent<SurfacePhysics>();
            jumpSystem = GetComponent<JumpSystem>();
            staminaSystem = GetComponent<StaminaSystem>();
            movementFeedback = GetComponent<MovementFeedback>();

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
                InputManager.Instance.OnJump += OnJumpPressed;
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
                InputManager.Instance.OnJump -= OnJumpPressed;
                InputManager.Instance.OnPause -= TogglePause;
            }
        }

        private void Update()
        {
            if (isPaused || !enableMovement)
                return;

            // Update all movement systems
            groundDetection.UpdateGroundDetection();
            jumpSystem.UpdateJumpSystem(Time.deltaTime);
            staminaSystem.UpdateStamina(Time.deltaTime);
            HandleCrouchTransition();

            // Main movement update
            locomotion.UpdateLocomotion(moveInput, isSprinting, isCrouching, jumpSystem.VerticalVelocity, Time.deltaTime);

            // Movement feedback
            movementFeedback.UpdateMovementFeedback(Time.deltaTime);
        }

        private void HandleMovement(Vector2 input)
        {
            if (isPaused || !enableMovement)
                return;

            moveInput = input;
        }

        private void OnSprintPressed()
        {
            if (!enableSprint || isPaused || isCrouching)
                return;

            if (InputManager.Instance.EnableHoldToSprint)
            {
                isSprinting = true;
                staminaSystem.SetSprinting(true);
            }
            else
            {
                // Toggle sprint
                if (staminaSystem.CanSprint())
                {
                    isSprinting = !isSprinting;
                    staminaSystem.SetSprinting(isSprinting);
                }
            }
        }

        private void OnSprintReleased()
        {
            if (InputManager.Instance.EnableHoldToSprint)
            {
                isSprinting = false;
                staminaSystem.SetSprinting(false);
            }
        }

        private void OnCrouchPressed()
        {
            if (!enableCrouch || isPaused)
                return;

            if (InputManager.Instance.EnableHoldToCrouch)
            {
                isCrouching = true;
                isSprinting = false;
                staminaSystem.SetSprinting(false);
            }
            else
            {
                isCrouching = !isCrouching;
                if (isCrouching)
                {
                    isSprinting = false;
                    staminaSystem.SetSprinting(false);
                }
            }
        }

        private void OnCrouchReleased()
        {
            if (!InputManager.Instance.EnableHoldToCrouch)
                return;

            if (CanStandUp())
            {
                isCrouching = false;
            }
        }

        private bool CanStandUp()
        {
            Vector3 checkPosition = transform.position + Vector3.up * (standingHeight - crouchHeight);
            return !Physics.CheckSphere(checkPosition, characterController.radius);
        }

        private void OnJumpPressed()
        {
            if (!enableJump || isPaused || isCrouching)
                return;

            jumpSystem.TryJump();
        }

        private void HandleCrouchTransition()
        {
            float targetHeight = isCrouching ? crouchHeight : standingHeight;
            currentHeight = Mathf.Lerp(currentHeight, targetHeight, Time.deltaTime * crouchTransitionSpeed);
            characterController.height = currentHeight;

            Vector3 center = characterController.center;
            center.y = currentHeight / 2f;
            characterController.center = center;
        }

        private void TogglePause()
        {
            isPaused = !isPaused;
        }

        /// <summary>
        /// Get movement speed as a magnitude (0-1 for normalized, >1 for actual speed).
        /// </summary>
        public float GetMovementSpeed()
        {
            return locomotion.CurrentSpeed;
        }

        /// <summary>
        /// Get current movement direction.
        /// </summary>
        public Vector3 GetMovementDirection()
        {
            return locomotion.GetMovementDirection();
        }

        /// <summary>
        /// Get stamina percentage (0-1).
        /// </summary>
        public float GetStaminaPercentage()
        {
            return staminaSystem.GetStaminaPercentage();
        }

        /// <summary>
        /// Check if player can currently sprint.
        /// </summary>
        public bool CanSprint()
        {
            return staminaSystem.CanSprint() && !isCrouching;
        }

        /// <summary>
        /// Restore player stamina (for pickups, etc).
        /// </summary>
        public void RestoreStamina(float amount = -1f)
        {
            if (amount < 0f)
            {
                staminaSystem.RestoreStamina();
            }
            else
            {
                staminaSystem.RestoreStamina(amount);
            }
        }

        /// <summary>
        /// Reset movement system (for respawn).
        /// </summary>
        public void ResetMovement()
        {
            moveInput = Vector2.zero;
            isSprinting = false;
            isCrouching = false;
            locomotion.ResetLocomotion();
            jumpSystem.ResetJump();
            staminaSystem.ResetStamina();
            currentHeight = standingHeight;
        }

        /// <summary>
        /// Enable/disable movement input.
        /// </summary>
        public void SetMovementEnabled(bool enabled)
        {
            enableMovement = enabled;
        }

        /// <summary>
        /// Get animation blend values for the animator.
        /// </summary>
        public float GetAnimationSpeed()
        {
            return movementFeedback.GetAnimationSpeed();
        }

        /// <summary>
        /// Get animation direction for turning.
        /// </summary>
        public Vector3 GetAnimationDirection()
        {
            return movementFeedback.GetAnimationDirection();
        }

        /// <summary>
        /// Get jump phase for jump animation (0 = grounded, 1 = apex, -1 = falling).
        /// </summary>
        public float GetJumpPhase()
        {
            return movementFeedback.GetJumpPhase();
        }

        /// <summary>
        /// Check if in air.
        /// </summary>
        public bool IsInAir()
        {
            return movementFeedback.IsInAir();
        }
    }
}
