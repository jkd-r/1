using UnityEngine;
using ProtocolEMR.Core.Player;
using ProtocolEMR.Core.Input;
using ProtocolEMR.Core.Camera;

namespace ProtocolEMR.Core.Animation
{
    /// <summary>
    /// Player-specific animation handling that integrates with movement and input systems.
    /// Synchronizes player animations with movement input, stamina state, and combat actions.
    /// </summary>
    public class PlayerAnimations : MonoBehaviour
    {
        [Header("Component References")]
        [SerializeField] private CharacterAnimationController animationController;
        [SerializeField] private MovementController movementController;
        [SerializeField] private FirstPersonCamera firstPersonCamera;

        [Header("Head Bob Settings")]
        [SerializeField] private float headBobAmount = 0.02f;
        [SerializeField] private float headBobFrequency = 5.0f;
        [SerializeField] private Transform cameraTransform;

        [Header("Landing Feedback")]
        [SerializeField] private float landingCameraShakeDuration = 0.1f;
        [SerializeField] private float landingCameraShakeAmount = 0.15f;

        [Header("Upper Body Layer")]
        [SerializeField] private int upperBodyLayerIndex = 1;
        [SerializeField] private bool useUpperBodyLayer = true;

        private Vector3 initialCameraPosition;
        private float headBobTimer = 0f;
        private bool wasGroundedLastFrame = true;
        private float upperBodyLayerWeight = 0f;

        private void Awake()
        {
            if (animationController == null)
            {
                animationController = GetComponent<CharacterAnimationController>();
            }

            if (movementController == null)
            {
                movementController = GetComponent<MovementController>();
            }

            if (cameraTransform == null && firstPersonCamera != null)
            {
                cameraTransform = firstPersonCamera.transform;
            }

            if (cameraTransform != null)
            {
                initialCameraPosition = cameraTransform.localPosition;
            }
        }

        private void Start()
        {
            if (InputManager.Instance != null)
            {
                InputManager.Instance.OnFire += OnAttackInput;
            }
        }

        private void OnDestroy()
        {
            if (InputManager.Instance != null)
            {
                InputManager.Instance.OnFire -= OnAttackInput;
            }
        }

        private void Update()
        {
            if (animationController == null || movementController == null)
                return;

            UpdateLocomotionAnimations();
            UpdateHeadBob();
            UpdateJumpAndLanding();
            UpdateCrouchAnimations();
            UpdateUpperBodyLayer();
        }

        /// <summary>
        /// Updates locomotion animations based on movement state.
        /// </summary>
        private void UpdateLocomotionAnimations()
        {
            Vector3 velocity = movementController.GetComponent<CharacterController>().velocity;
            float speed = new Vector3(velocity.x, 0, velocity.z).magnitude;

            bool isSprinting = movementController.IsSprinting;
            bool isMoving = movementController.IsMoving;

            // Update animation controller with current speed
            animationController.SetMovementSpeed(speed, isMoving);

            // Update animator parameters for foot IK and other systems
            animationController.StateManager.SetBool("IsSprinting", isSprinting);
        }

        /// <summary>
        /// Updates head bob animation based on movement.
        /// </summary>
        private void UpdateHeadBob()
        {
            if (cameraTransform == null || !movementController.IsMoving || !movementController.IsGrounded)
            {
                headBobTimer = 0f;
                if (cameraTransform != null)
                {
                    cameraTransform.localPosition = initialCameraPosition;
                }
                return;
            }

            // Calculate bob amount based on movement type
            float currentHeadBobAmount = headBobAmount;
            if (movementController.IsSprinting)
            {
                currentHeadBobAmount *= 1.5f;
            }
            if (movementController.IsCrouching)
            {
                currentHeadBobAmount *= 0.5f;
            }

            headBobTimer += Time.deltaTime * headBobFrequency;
            float bobOffset = Mathf.Sin(headBobTimer) * currentHeadBobAmount;

            if (cameraTransform != null)
            {
                Vector3 newPosition = initialCameraPosition;
                newPosition.y += bobOffset;
                cameraTransform.localPosition = newPosition;
            }
        }

        /// <summary>
        /// Updates jump and landing animations.
        /// </summary>
        private void UpdateJumpAndLanding()
        {
            bool isGroundedNow = movementController.IsGrounded;

            // Detect landing
            if (!wasGroundedLastFrame && isGroundedNow)
            {
                animationController.SetGrounded(true);
                ApplyLandingFeedback();
            }
            else if (wasGroundedLastFrame && !isGroundedNow)
            {
                animationController.SetGrounded(false);
            }

            wasGroundedLastFrame = isGroundedNow;
        }

        /// <summary>
        /// Applies camera and visual feedback for landing.
        /// </summary>
        private void ApplyLandingFeedback()
        {
            if (firstPersonCamera != null && landingCameraShakeDuration > 0)
            {
                firstPersonCamera.TriggerCameraShake(landingCameraShakeDuration, landingCameraShakeAmount);
            }
        }

        /// <summary>
        /// Updates crouch animations.
        /// </summary>
        private void UpdateCrouchAnimations()
        {
            animationController.SetCrouching(movementController.IsCrouching);
        }

        /// <summary>
        /// Updates upper body layer weight for smooth blending.
        /// </summary>
        private void UpdateUpperBodyLayer()
        {
            if (!useUpperBodyLayer || animationController.StateManager.GetAnimator() == null)
                return;

            // In future, this can be used for upper body animations while lower body continues locomotion
            float targetWeight = 0f;
            upperBodyLayerWeight = Mathf.Lerp(upperBodyLayerWeight, targetWeight, Time.deltaTime * 5.0f);

            animationController.StateManager.SetLayerWeight(upperBodyLayerIndex, upperBodyLayerWeight);
        }

        /// <summary>
        /// Called when attack input is registered.
        /// </summary>
        private void OnAttackInput()
        {
            animationController.PlayAttackAnimation();
        }

        /// <summary>
        /// Plays damage animation with optional knockback direction.
        /// </summary>
        public void PlayDamageAnimation(Vector3 knockbackDirection = default)
        {
            animationController.PlayDamageAnimation();
        }

        /// <summary>
        /// Plays death animation.
        /// </summary>
        public void PlayDeathAnimation()
        {
            animationController.PlayDeathAnimation();
        }

        /// <summary>
        /// Plays interaction animation.
        /// </summary>
        public void PlayInteractionAnimation()
        {
            animationController.PlayInteractAnimation();
        }

        /// <summary>
        /// Sets the upper body layer weight for animation blending.
        /// </summary>
        public void SetUpperBodyLayerWeight(float weight)
        {
            upperBodyLayerWeight = Mathf.Clamp01(weight);
        }

        /// <summary>
        /// Gets the animation controller component.
        /// </summary>
        public CharacterAnimationController GetAnimationController()
        {
            return animationController;
        }

        /// <summary>
        /// Gets the current animation state.
        /// </summary>
        public CharacterAnimationController.AnimationState GetCurrentAnimationState()
        {
            return animationController.CurrentLocomotionState;
        }
    }
}
