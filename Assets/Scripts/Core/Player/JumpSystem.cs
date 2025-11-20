using UnityEngine;

namespace ProtocolEMR.Core.Player
{
    /// <summary>
    /// Handles jump mechanics with apex control and variable jump height.
    /// Supports short hops and full jumps with responsive control at the peak.
    /// Integrates with surface physics for realistic jump heights on different terrain.
    /// </summary>
    public class JumpSystem : MonoBehaviour
    {
        [Header("Jump Settings")]
        [SerializeField] private float jumpHeight = 2.0f;
        [SerializeField] private float gravity = -9.81f;
        [SerializeField] private float groundDrag = 0.95f;

        [Header("Apex Control")]
        [SerializeField] private float apexControlFactor = 0.5f;
        [SerializeField] private float apexThreshold = 10f;

        [Header("Coyote Time")]
        [SerializeField] private float coyoteTime = 0.1f;
        [SerializeField] private bool enableCoyoteTime = true;

        [Header("Jump Buffer")]
        [SerializeField] private float jumpBufferTime = 0.05f;
        [SerializeField] private bool enableJumpBuffer = true;

        private CharacterController characterController;
        private GroundDetection groundDetection;
        private SurfacePhysics surfacePhysics;

        private Vector3 verticalVelocity = Vector3.zero;
        private float coyoteTimer = 0f;
        private float jumpBufferTimer = 0f;
        private bool isJumping = false;
        private float jumpApexVelocity = 0f;

        public float VerticalSpeed => verticalVelocity.y;
        public bool IsJumping => isJumping;
        public bool CanJump => (groundDetection?.IsGrounded ?? false) || coyoteTimer > 0f;
        public Vector3 VerticalVelocity => verticalVelocity;

        private void Awake()
        {
            characterController = GetComponent<CharacterController>();
            groundDetection = GetComponent<GroundDetection>();
            surfacePhysics = GetComponent<SurfacePhysics>();

            if (characterController == null)
                Debug.LogError("JumpSystem requires a CharacterController component");
        }

        /// <summary>
        /// Update jump system each frame.
        /// Handles gravity, apex control, and state management.
        /// </summary>
        public void UpdateJumpSystem(float deltaTime)
        {
            UpdateGroundState(deltaTime);
            ApplyGravity(deltaTime);
            ApplyApexControl(deltaTime);
        }

        /// <summary>
        /// Attempts to perform a jump.
        /// Returns true if jump was successful, false if on cooldown or grounded check failed.
        /// </summary>
        public bool TryJump()
        {
            if (!CanJump)
            {
                // Queue jump in buffer if enabled
                if (enableJumpBuffer)
                {
                    jumpBufferTimer = jumpBufferTime;
                }
                return false;
            }

            PerformJump();
            return true;
        }

        /// <summary>
        /// Process buffered jump if available.
        /// Should be called when player lands.
        /// </summary>
        public void ProcessJumpBuffer()
        {
            if (jumpBufferTimer > 0f)
            {
                jumpBufferTimer -= Time.deltaTime;
                if (jumpBufferTimer > 0f)
                {
                    PerformJump();
                    jumpBufferTimer = 0f;
                }
            }
        }

        /// <summary>
        /// Performs the actual jump action.
        /// Calculates jump velocity based on surface and jump height.
        /// </summary>
        private void PerformJump()
        {
            float jumpMultiplier = 1.0f;
            if (surfacePhysics != null)
            {
                jumpMultiplier = surfacePhysics.GetJumpMultiplier();
            }

            float effectiveJumpHeight = jumpHeight * jumpMultiplier;
            verticalVelocity.y = Mathf.Sqrt(effectiveJumpHeight * -2f * gravity);
            jumpApexVelocity = verticalVelocity.y;

            isJumping = true;
            coyoteTimer = 0f;
        }

        /// <summary>
        /// Updates ground state tracking and coyote time.
        /// </summary>
        private void UpdateGroundState(float deltaTime)
        {
            if (groundDetection == null)
                return;

            bool wasGrounded = coyoteTimer > 0f || (verticalVelocity.y <= 0f && groundDetection.IsGrounded);
            bool isGrounded = groundDetection.IsGrounded;

            if (isGrounded)
            {
                if (verticalVelocity.y < 0f)
                {
                    verticalVelocity.y = -2f; // Small downward velocity to keep grounded
                }
                isJumping = false;
                coyoteTimer = coyoteTime;
            }
            else
            {
                coyoteTimer -= deltaTime;
            }

            // Process buffered jump when landing
            if (isGrounded && !wasGrounded)
            {
                ProcessJumpBuffer();
            }
        }

        /// <summary>
        /// Apply gravity and vertical movement.
        /// </summary>
        private void ApplyGravity(float deltaTime)
        {
            bool isGrounded = groundDetection?.IsGrounded ?? false;

            if (!isGrounded)
            {
                verticalVelocity.y += gravity * deltaTime;
            }
        }

        /// <summary>
        /// Apply apex control - reduced gravity at peak of jump for responsive control.
        /// </summary>
        private void ApplyApexControl(float deltaTime)
        {
            if (!isJumping)
                return;

            // Check if near apex (upward velocity is very small)
            if (Mathf.Abs(verticalVelocity.y) < apexThreshold)
            {
                // Reduce gravity effect at apex for better control
                verticalVelocity.y += gravity * apexControlFactor * deltaTime;
            }
        }

        /// <summary>
        /// Gets the current jump state as a normalized value (0 = grounded, 1 = apex, < 0 = falling).
        /// Useful for animations.
        /// </summary>
        public float GetJumpPhase()
        {
            if (groundDetection?.IsGrounded ?? false)
                return 0f;

            if (verticalVelocity.y > 0f)
            {
                // Ascending
                return Mathf.Clamp01(verticalVelocity.y / jumpApexVelocity);
            }
            else
            {
                // Descending
                return -1f;
            }
        }

        /// <summary>
        /// Instantly stops jump and applies falling gravity.
        /// </summary>
        public void StopJump()
        {
            if (verticalVelocity.y > 0f)
            {
                verticalVelocity.y = 0f;
            }
        }

        /// <summary>
        /// Resets jump system to grounded state.
        /// </summary>
        public void ResetJump()
        {
            verticalVelocity = Vector3.zero;
            isJumping = false;
            coyoteTimer = 0f;
            jumpBufferTimer = 0f;
            jumpApexVelocity = 0f;
        }

        /// <summary>
        /// Sets external vertical velocity (for knockback, platforms, etc).
        /// </summary>
        public void SetVerticalVelocity(float newVelocity)
        {
            verticalVelocity.y = newVelocity;
            if (newVelocity > 0f)
            {
                isJumping = true;
                jumpApexVelocity = newVelocity;
            }
        }

        /// <summary>
        /// Adds to current vertical velocity (for cumulative effects).
        /// </summary>
        public void AddVerticalVelocity(float amount)
        {
            verticalVelocity.y += amount;
        }
    }
}
