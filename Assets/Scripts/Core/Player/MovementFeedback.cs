using UnityEngine;
using System;

namespace ProtocolEMR.Core.Player
{
    /// <summary>
    /// Provides audio and visual feedback for player movement.
    /// Handles footstep sounds, landing impacts, and animation synchronization.
    /// Integrates with surface physics for realistic sound feedback per terrain type.
    /// </summary>
    public class MovementFeedback : MonoBehaviour
    {
        [Header("Footstep Settings")]
        [SerializeField] private float footstepVolumeWalk = 0.4f;
        [SerializeField] private float footstepVolumeSprint = 0.6f;
        [SerializeField] private float footstepRateWalk = 0.5f;
        [SerializeField] private float footstepRateSprint = 0.35f;
        [SerializeField] private AudioClip[] footstepClips;

        [Header("Landing Settings")]
        [SerializeField] private float landingVolumeMin = 0.3f;
        [SerializeField] private float landingVolumeMax = 0.7f;
        [SerializeField] private AudioClip[] landingClips;
        [SerializeField] private float minFallHeightForSound = 3.0f;

        [Header("Audio Source")]
        [SerializeField] private float audioSourceMaxDistance = 20f;

        private CharacterLocomotion locomotion;
        private SurfacePhysics surfacePhysics;
        private JumpSystem jumpSystem;
        private GroundDetection groundDetection;

        private AudioSource audioSource;
        private float footstepTimer = 0f;
        private bool wasGrounded = false;
        private float maxAirHeight = 0f;

        public event Action OnFootstepPlayed;
        public event Action<float> OnLandingImpact;

        private void Awake()
        {
            locomotion = GetComponent<CharacterLocomotion>();
            surfacePhysics = GetComponent<SurfacePhysics>();
            jumpSystem = GetComponent<JumpSystem>();
            groundDetection = GetComponent<GroundDetection>();

            // Create audio source if not present
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }

            audioSource.maxDistance = audioSourceMaxDistance;
            audioSource.spatialBlend = 1.0f; // 3D audio
        }

        /// <summary>
        /// Update movement feedback each frame.
        /// </summary>
        public void UpdateMovementFeedback(float deltaTime)
        {
            UpdateFootsteps(deltaTime);
            UpdateLandingFeedback(deltaTime);
        }

        /// <summary>
        /// Update and trigger footstep sounds based on movement speed and terrain.
        /// </summary>
        private void UpdateFootsteps(float deltaTime)
        {
            if (groundDetection == null || !groundDetection.IsGrounded || locomotion == null)
            {
                footstepTimer = 0f;
                return;
            }

            // Calculate footstep rate based on movement speed
            float speed = locomotion.CurrentSpeed;
            if (speed < 0.1f)
            {
                footstepTimer = 0f;
                return;
            }

            // Determine if sprinting from speed relative to walk speed
            bool isSprinting = speed > 6.0f; // Approximate sprint threshold
            float baseRate = isSprinting ? footstepRateSprint : footstepRateWalk;
            float footstepInterval = 1.0f / baseRate;

            footstepTimer -= deltaTime;
            if (footstepTimer <= 0f)
            {
                PlayFootstep(isSprinting);
                footstepTimer = footstepInterval;
            }
        }

        /// <summary>
        /// Play a footstep sound appropriate for current surface and speed.
        /// </summary>
        private void PlayFootstep(bool isSprinting)
        {
            if (footstepClips == null || footstepClips.Length == 0)
                return;

            float volume = isSprinting ? footstepVolumeSprint : footstepVolumeWalk;

            // Apply surface-specific volume modifications
            if (surfacePhysics != null)
            {
                volume *= GetSurfaceVolumeMultiplier(surfacePhysics.GetSurfaceSoundCategory());
            }

            AudioClip clip = footstepClips[UnityEngine.Random.Range(0, footstepClips.Length)];
            audioSource.PlayOneShot(clip, volume);

            OnFootstepPlayed?.Invoke();
        }

        /// <summary>
        /// Update landing feedback when falling.
        /// </summary>
        private void UpdateLandingFeedback(float deltaTime)
        {
            if (groundDetection == null || jumpSystem == null)
                return;

            bool isGrounded = groundDetection.IsGrounded;
            float verticalVelocity = jumpSystem.VerticalSpeed;

            // Track maximum height during fall
            if (!isGrounded && verticalVelocity < 0)
            {
                if (transform.position.y > maxAirHeight)
                {
                    maxAirHeight = transform.position.y;
                }
            }

            // Detect landing
            if (isGrounded && !wasGrounded)
            {
                float fallHeight = maxAirHeight - transform.position.y;
                if (fallHeight > minFallHeightForSound)
                {
                    PlayLandingSound(fallHeight);
                    OnLandingImpact?.Invoke(fallHeight);
                }
                maxAirHeight = transform.position.y;
            }

            wasGrounded = isGrounded;
        }

        /// <summary>
        /// Play landing sound based on fall height and surface.
        /// </summary>
        private void PlayLandingSound(float fallHeight)
        {
            if (landingClips == null || landingClips.Length == 0)
                return;

            // Scale volume based on fall height
            float normalizedHeight = Mathf.Clamp01(fallHeight / 10.0f);
            float volume = Mathf.Lerp(landingVolumeMin, landingVolumeMax, normalizedHeight);

            // Apply surface-specific volume modifications
            if (surfacePhysics != null)
            {
                volume *= GetSurfaceVolumeMultiplier(surfacePhysics.GetSurfaceSoundCategory());
            }

            AudioClip clip = landingClips[UnityEngine.Random.Range(0, landingClips.Length)];
            audioSource.PlayOneShot(clip, volume);
        }

        /// <summary>
        /// Get volume multiplier based on surface type.
        /// </summary>
        private float GetSurfaceVolumeMultiplier(string surfaceType)
        {
            return surfaceType switch
            {
                "Mud" => 0.6f,
                "Sand" => 0.7f,
                "Water" => 0.5f,
                "Metal" => 1.2f,
                "Wood" => 0.9f,
                "Concrete" => 1.0f,
                "Ice" => 1.1f,
                _ => 1.0f
            };
        }

        /// <summary>
        /// Get animation parameter for movement speed.
        /// Returns normalized speed (0-1) for blending animations.
        /// </summary>
        public float GetAnimationSpeed()
        {
            if (locomotion == null)
                return 0f;

            return locomotion.GetMovementMagnitude();
        }

        /// <summary>
        /// Get animation parameters for direction.
        /// </summary>
        public Vector3 GetAnimationDirection()
        {
            if (locomotion == null)
                return Vector3.zero;

            return locomotion.GetMovementDirection();
        }

        /// <summary>
        /// Get jump phase for jump animation blending.
        /// </summary>
        public float GetJumpPhase()
        {
            if (jumpSystem == null)
                return 0f;

            return jumpSystem.GetJumpPhase();
        }

        /// <summary>
        /// Check if currently in air (for jump animations).
        /// </summary>
        public bool IsInAir()
        {
            if (groundDetection == null)
                return false;

            return !groundDetection.IsGrounded;
        }

        /// <summary>
        /// Set custom footstep clips for surface-specific audio.
        /// </summary>
        public void SetFootstepClips(AudioClip[] clips)
        {
            footstepClips = clips;
        }

        /// <summary>
        /// Set custom landing clips.
        /// </summary>
        public void SetLandingClips(AudioClip[] clips)
        {
            landingClips = clips;
        }
    }
}
