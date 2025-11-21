using UnityEngine;
using System.Collections.Generic;

namespace ProtocolEMR.Core.Animation
{
    /// <summary>
    /// Main animation orchestrator for characters (player and NPCs).
    /// Provides high-level animation control, blending, and synchronization with movement systems.
    /// Supports smooth transitions between movement states and action animations.
    /// </summary>
    public class CharacterAnimationController : MonoBehaviour
    {
        [System.Serializable]
        public class AnimationBlendSettings
        {
            [Header("Blend Times")]
            public float locomotionBlendTime = 0.15f;
            public float actionAnimationBlendTime = 0.2f;
            public float transitionBlendTime = 0.3f;

            [Header("Speed Thresholds")]
            public float stationaryThreshold = 0.01f;
            public float walkThreshold = 2.0f;
            public float runThreshold = 5.0f;
            public float sprintThreshold = 8.0f;
        }

        [System.Serializable]
        public class AnimationSetup
        {
            [Header("Locomotion Animations")]
            public string idleAnimationName = "Locomotion.Idle";
            public string walkAnimationName = "Locomotion.Walk";
            public string runAnimationName = "Locomotion.Run";
            public string sprintAnimationName = "Locomotion.Sprint";
            public string jumpAnimationName = "Locomotion.Jump";
            public string fallAnimationName = "Locomotion.Fall";
            public string landAnimationName = "Locomotion.Land";
            public string crouchIdleAnimationName = "Locomotion.CrouchIdle";
            public string crouchWalkAnimationName = "Locomotion.CrouchWalk";

            [Header("Action Animations")]
            public string attackAnimationName = "Actions.Attack";
            public string damageAnimationName = "Actions.Damage";
            public string deathAnimationName = "Actions.Death";
            public string interactAnimationName = "Actions.Interact";

            [Header("Animator Parameters")]
            public string speedParameterName = "Speed";
            public string verticalSpeedParameterName = "VerticalSpeed";
            public string directionParameterName = "Direction";
            public string isGroundedParameterName = "IsGrounded";
            public string isMovingParameterName = "IsMoving";
            public string isCrouchingParameterName = "IsCrouching";
        }

        [SerializeField] private Animator animator;
        [SerializeField] private AnimationBlendSettings blendSettings = new AnimationBlendSettings();
        [SerializeField] private AnimationSetup animationSetup = new AnimationSetup();

        private AnimationStateManager stateManager;
        private float currentSpeed;
        private float targetSpeed;
        private float currentVerticalSpeed;
        private float targetVerticalSpeed;
        private bool isGrounded = true;
        private bool isMoving = false;
        private bool isCrouching = false;
        private bool isJumping = false;
        private bool isFalling = false;
        private AnimationState currentLocomotionState = AnimationState.Idle;

        private Dictionary<string, float> animationLengths = new Dictionary<string, float>();

        public enum AnimationState
        {
            Idle,
            Walk,
            Run,
            Sprint,
            Jump,
            Fall,
            Land,
            CrouchIdle,
            CrouchWalk,
            Attack,
            Damage,
            Death,
            Interact
        }

        public AnimationStateManager StateManager => stateManager;
        public float CurrentSpeed => currentSpeed;
        public bool IsGrounded => isGrounded;
        public bool IsMoving => isMoving;
        public bool IsCrouching => isCrouching;
        public AnimationState CurrentLocomotionState => currentLocomotionState;

        private void Awake()
        {
            if (animator == null)
            {
                animator = GetComponent<Animator>();
            }

            stateManager = new AnimationStateManager(animator);
            CacheAnimationLengths();
        }

        private void Update()
        {
            if (animator == null) return;

            stateManager.Update(Time.deltaTime);
            UpdateAnimatorParameters();
        }

        /// <summary>
        /// Caches animation clip lengths for faster lookups.
        /// </summary>
        private void CacheAnimationLengths()
        {
            if (animator == null || animator.runtimeAnimatorController == null)
                return;

            AnimationClip[] clips = animator.runtimeAnimatorController.animationClips;
            foreach (AnimationClip clip in clips)
            {
                if (!animationLengths.ContainsKey(clip.name))
                {
                    animationLengths[clip.name] = clip.length;
                }
            }
        }

        /// <summary>
        /// Updates animator parameters based on current state.
        /// </summary>
        private void UpdateAnimatorParameters()
        {
            // Smooth speed blending
            currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed, Time.deltaTime / blendSettings.locomotionBlendTime);
            currentVerticalSpeed = Mathf.Lerp(currentVerticalSpeed, targetVerticalSpeed, Time.deltaTime / blendSettings.locomotionBlendTime);

            // Set animator parameters
            stateManager.SetFloat(animationSetup.speedParameterName, currentSpeed);
            stateManager.SetFloat(animationSetup.verticalSpeedParameterName, currentVerticalSpeed);
            stateManager.SetBool(animationSetup.isGroundedParameterName, isGrounded);
            stateManager.SetBool(animationSetup.isMovingParameterName, isMoving);
            stateManager.SetBool(animationSetup.isCrouchingParameterName, isCrouching);
        }

        /// <summary>
        /// Sets the movement speed and updates animation state accordingly.
        /// </summary>
        public void SetMovementSpeed(float speed, bool moving = true)
        {
            targetSpeed = Mathf.Max(0, speed);
            isMoving = moving || speed > blendSettings.stationaryThreshold;

            // Determine locomotion state based on speed
            AnimationState desiredState = DetermineLocomotionState(speed);
            if (desiredState != currentLocomotionState && isGrounded && !isJumping)
            {
                TransitionToLocomotionState(desiredState);
            }
        }

        /// <summary>
        /// Determines the appropriate locomotion state based on speed.
        /// </summary>
        private AnimationState DetermineLocomotionState(float speed)
        {
            if (!isGrounded || isJumping || isFalling) return currentLocomotionState;

            if (isCrouching)
            {
                return speed > blendSettings.stationaryThreshold ? AnimationState.CrouchWalk : AnimationState.CrouchIdle;
            }

            if (speed <= blendSettings.stationaryThreshold)
                return AnimationState.Idle;
            if (speed <= blendSettings.walkThreshold)
                return AnimationState.Walk;
            if (speed <= blendSettings.runThreshold)
                return AnimationState.Run;
            
            return AnimationState.Sprint;
        }

        /// <summary>
        /// Transitions to a specific locomotion state.
        /// </summary>
        private void TransitionToLocomotionState(AnimationState state)
        {
            currentLocomotionState = state;
            string animationName = GetAnimationNameForState(state);

            if (!string.IsNullOrEmpty(animationName))
            {
                stateManager.TransitionToState(animationName, blendSettings.locomotionBlendTime);
            }
        }

        /// <summary>
        /// Gets the animation name for a specific locomotion state.
        /// </summary>
        private string GetAnimationNameForState(AnimationState state)
        {
            return state switch
            {
                AnimationState.Idle => animationSetup.idleAnimationName,
                AnimationState.Walk => animationSetup.walkAnimationName,
                AnimationState.Run => animationSetup.runAnimationName,
                AnimationState.Sprint => animationSetup.sprintAnimationName,
                AnimationState.Jump => animationSetup.jumpAnimationName,
                AnimationState.Fall => animationSetup.fallAnimationName,
                AnimationState.Land => animationSetup.landAnimationName,
                AnimationState.CrouchIdle => animationSetup.crouchIdleAnimationName,
                AnimationState.CrouchWalk => animationSetup.crouchWalkAnimationName,
                _ => null
            };
        }

        /// <summary>
        /// Sets vertical speed (for jumping/falling).
        /// </summary>
        public void SetVerticalSpeed(float speed)
        {
            targetVerticalSpeed = speed;
        }

        /// <summary>
        /// Updates grounded state and plays appropriate animations.
        /// </summary>
        public void SetGrounded(bool grounded)
        {
            if (isGrounded == grounded) return;

            isGrounded = grounded;

            if (isGrounded)
            {
                isJumping = false;
                if (!isFalling)
                {
                    PlayLandAnimation();
                }
                isFalling = false;
            }
        }

        /// <summary>
        /// Plays jump animation and sets flags.
        /// </summary>
        public void PlayJumpAnimation()
        {
            if (!isGrounded) return;

            isJumping = true;
            isGrounded = false;
            stateManager.TransitionToState(animationSetup.jumpAnimationName, blendSettings.actionAnimationBlendTime);
        }

        /// <summary>
        /// Sets falling state and plays fall animation.
        /// </summary>
        public void SetFalling(bool falling)
        {
            isFalling = falling;
            if (falling && isGrounded)
            {
                isGrounded = false;
                stateManager.TransitionToState(animationSetup.fallAnimationName, blendSettings.actionAnimationBlendTime);
            }
        }

        /// <summary>
        /// Plays landing animation.
        /// </summary>
        private void PlayLandAnimation()
        {
            if (animationLengths.ContainsKey(animationSetup.landAnimationName))
            {
                stateManager.TransitionToState(animationSetup.landAnimationName, 0.1f);
            }
        }

        /// <summary>
        /// Sets crouch state.
        /// </summary>
        public void SetCrouching(bool crouching)
        {
            if (isCrouching == crouching) return;

            isCrouching = crouching;
            
            if (crouching)
            {
                TransitionToLocomotionState(AnimationState.CrouchIdle);
            }
            else
            {
                TransitionToLocomotionState(AnimationState.Idle);
            }
        }

        /// <summary>
        /// Plays attack animation.
        /// </summary>
        public void PlayAttackAnimation(string attackType = "")
        {
            string animName = string.IsNullOrEmpty(attackType) ? animationSetup.attackAnimationName : attackType;
            stateManager.TransitionToState(animName, blendSettings.actionAnimationBlendTime, queue: true);
        }

        /// <summary>
        /// Plays damage/hurt animation.
        /// </summary>
        public void PlayDamageAnimation()
        {
            stateManager.TransitionToState(animationSetup.damageAnimationName, blendSettings.actionAnimationBlendTime, queue: true);
        }

        /// <summary>
        /// Plays death animation.
        /// </summary>
        public void PlayDeathAnimation()
        {
            stateManager.TransitionToState(animationSetup.deathAnimationName, 0.5f);
        }

        /// <summary>
        /// Plays interact animation.
        /// </summary>
        public void PlayInteractAnimation()
        {
            stateManager.TransitionToState(animationSetup.interactAnimationName, blendSettings.actionAnimationBlendTime, queue: true);
        }

        /// <summary>
        /// Gets the length of a specific animation.
        /// </summary>
        public float GetAnimationLength(string animationName)
        {
            if (animationLengths.TryGetValue(animationName, out float length))
            {
                return length;
            }
            return stateManager.GetAnimationLength(animationName);
        }

        /// <summary>
        /// Checks if a specific animation is currently playing.
        /// </summary>
        public bool IsAnimationPlaying(string animationName)
        {
            return stateManager.IsAnimationPlaying(animationName);
        }

        /// <summary>
        /// Gets the normalized time of the current animation.
        /// </summary>
        public float GetCurrentAnimationNormalizedTime()
        {
            return stateManager.GetCurrentAnimationNormalizedTime();
        }

        /// <summary>
        /// Gets the current animation name.
        /// </summary>
        public string GetCurrentAnimationName()
        {
            return stateManager.GetCurrentAnimationName();
        }

        /// <summary>
        /// Returns the animator component.
        /// </summary>
        public Animator GetAnimator()
        {
            return animator;
        }
    }
}
