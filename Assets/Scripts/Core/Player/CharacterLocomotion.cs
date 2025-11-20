using UnityEngine;

namespace ProtocolEMR.Core.Player
{
    /// <summary>
    /// Handles smooth character locomotion with acceleration and deceleration.
    /// Provides responsive movement that feels natural with smooth transitions.
    /// Integrates with surface physics for terrain-specific movement characteristics.
    /// </summary>
    public class CharacterLocomotion : MonoBehaviour
    {
        [Header("Acceleration Settings")]
        [SerializeField] private float accelerationTime = 0.15f;
        [SerializeField] private float decelerationTime = 0.1f;
        [SerializeField] private AnimationCurve accelerationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        [Header("Movement Settings")]
        [SerializeField] private float walkSpeed = 5.0f;
        [SerializeField] private float sprintSpeed = 8.0f;
        [SerializeField] private float crouchSpeed = 2.5f;

        [Header("Strafe Settings")]
        [SerializeField] private float strafeSpeedMultiplier = 0.85f;
        [SerializeField] private float backwardSpeedMultiplier = 0.7f;

        private CharacterController characterController;
        private SurfacePhysics surfacePhysics;
        private GroundDetection groundDetection;

        private Vector3 currentVelocity = Vector3.zero;
        private float currentSpeed = 0f;
        private float targetSpeed = 0f;
        private float accelerationTimer = 0f;
        private bool isAccelerating = false;

        public float CurrentSpeed => currentSpeed;
        public Vector3 CurrentVelocity => currentVelocity;
        public bool IsMoving => currentSpeed > 0.01f;

        private void Awake()
        {
            characterController = GetComponent<CharacterController>();
            surfacePhysics = GetComponent<SurfacePhysics>();
            groundDetection = GetComponent<GroundDetection>();

            if (characterController == null)
                Debug.LogError("CharacterLocomotion requires a CharacterController component");
        }

        /// <summary>
        /// Updates character locomotion with input and applies movement.
        /// </summary>
        public void UpdateLocomotion(Vector2 moveInput, bool isSprinting, bool isCrouching, Vector3 verticalVelocity, float deltaTime)
        {
            if (characterController == null)
                return;

            UpdateGroundDetection();
            CalculateMovementVelocity(moveInput, isSprinting, isCrouching, deltaTime);
            ApplyMovement(verticalVelocity, deltaTime);
        }

        /// <summary>
        /// Calculates smooth movement velocity based on input and movement state.
        /// Applies acceleration/deceleration curves for responsive feel.
        /// </summary>
        private void CalculateMovementVelocity(Vector2 moveInput, bool isSprinting, bool isCrouching, float deltaTime)
        {
            float inputMagnitude = moveInput.magnitude;
            
            // Determine target speed based on movement state
            float maxSpeed = GetMaxSpeed(isSprinting, isCrouching);
            targetSpeed = maxSpeed * inputMagnitude;

            // Apply surface speed multiplier
            if (surfacePhysics != null)
            {
                targetSpeed *= surfacePhysics.GetSpeedMultiplier();
            }

            // Apply directional multipliers (strafing, backpedaling)
            if (inputMagnitude > 0.01f)
            {
                float forwardComponent = moveInput.y;
                
                if (forwardComponent < -0.1f)
                {
                    // Moving backward
                    targetSpeed *= backwardSpeedMultiplier;
                }
                else if (Mathf.Abs(moveInput.x) > Mathf.Abs(forwardComponent))
                {
                    // Mostly strafing
                    targetSpeed *= strafeSpeedMultiplier;
                }
            }

            // Smoothly transition to target speed
            UpdateSpeed(targetSpeed, deltaTime);

            // Build movement vector from input
            if (inputMagnitude > 0.01f)
            {
                Vector3 moveDirection = (transform.right * moveInput.x + transform.forward * moveInput.y).normalized;
                currentVelocity = moveDirection * currentSpeed;
            }
            else
            {
                // Decelerate when no input
                currentVelocity = Vector3.Lerp(currentVelocity, Vector3.zero, decelerationTime * deltaTime * 5f);
            }

            // Apply surface damping
            if (surfacePhysics != null)
            {
                currentVelocity = surfacePhysics.ApplySurfaceDamping(currentVelocity, deltaTime);
            }
        }

        /// <summary>
        /// Applies movement velocity to the CharacterController.
        /// Handles vertical velocity (gravity, jumping) separately.
        /// </summary>
        private void ApplyMovement(Vector3 verticalVelocity, float deltaTime)
        {
            Vector3 movement = currentVelocity + verticalVelocity;
            characterController.Move(movement * deltaTime);
        }

        /// <summary>
        /// Updates current speed towards target speed with acceleration/deceleration curves.
        /// </summary>
        private void UpdateSpeed(float newTargetSpeed, float deltaTime)
        {
            if (Mathf.Abs(newTargetSpeed - currentSpeed) < 0.01f)
            {
                currentSpeed = newTargetSpeed;
                accelerationTimer = 0f;
                isAccelerating = false;
                return;
            }

            float timeScale = (newTargetSpeed > currentSpeed) ? accelerationTime : decelerationTime;
            accelerationTimer += deltaTime / timeScale;
            accelerationTimer = Mathf.Clamp01(accelerationTimer);

            float speedDifference = newTargetSpeed - currentSpeed;
            float curveValue = accelerationCurve.Evaluate(accelerationTimer);
            currentSpeed += speedDifference * curveValue * (deltaTime / timeScale);

            if (accelerationTimer >= 1f)
            {
                currentSpeed = newTargetSpeed;
                accelerationTimer = 0f;
            }
        }

        /// <summary>
        /// Gets the maximum speed for current movement state.
        /// </summary>
        private float GetMaxSpeed(bool isSprinting, bool isCrouching)
        {
            if (isCrouching)
                return crouchSpeed;
            if (isSprinting)
                return sprintSpeed;
            return walkSpeed;
        }

        /// <summary>
        /// Updates ground detection for surface physics.
        /// </summary>
        private void UpdateGroundDetection()
        {
            if (groundDetection != null)
            {
                groundDetection.UpdateGroundDetection();
            }
        }

        /// <summary>
        /// Gets the current movement direction normalized.
        /// </summary>
        public Vector3 GetMovementDirection()
        {
            if (currentSpeed < 0.01f)
                return Vector3.zero;

            return currentVelocity.normalized;
        }

        /// <summary>
        /// Gets directional movement magnitude (0-1).
        /// </summary>
        public float GetMovementMagnitude()
        {
            if (currentSpeed < 0.01f)
                return 0f;

            return Mathf.Clamp01(currentSpeed / sprintSpeed);
        }

        /// <summary>
        /// Resets locomotion state (used when spawning or resetting).
        /// </summary>
        public void ResetLocomotion()
        {
            currentVelocity = Vector3.zero;
            currentSpeed = 0f;
            targetSpeed = 0f;
            accelerationTimer = 0f;
            isAccelerating = false;
        }
    }
}
