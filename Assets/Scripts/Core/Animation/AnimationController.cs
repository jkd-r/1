using UnityEngine;
using ProtocolEMR.Core.Settings;

namespace ProtocolEMR.Core.Animation
{
    /// <summary>
    /// Manages animation state and transitions for the player character.
    /// Drives the animator with movement speed and direction parameters.
    /// Syncs with PlayerController movement state and stamina system.
    /// </summary>
    [RequireComponent(typeof(Animator))]
    public class AnimationController : MonoBehaviour
    {
        [Header("Animation Parameters")]
        [SerializeField] private float locomotionSpeedMultiplier = 1.0f;
        [SerializeField] private float transitionSmoothTime = 0.15f;
        [SerializeField] private float directionSmoothTime = 0.1f;

        [Header("Animation Layers")]
        [SerializeField] private string baseLayerName = "Base Layer";

        private Animator animator;
        private Vector2 currentDirection;
        private float currentSpeed;
        private float speedVelocity;
        private float directionVelocity;

        private int speedParameterId;
        private int directionXParameterId;
        private int directionYParameterId;
        private int isCrouchingParameterId;
        private int isJumpingParameterId;
        private int isLandingParameterId;

        private bool isJumping;
        private bool isLanding;

        public bool IsJumping => isJumping;
        public bool IsLanding => isLanding;

        private void Awake()
        {
            animator = GetComponent<Animator>();
            CacheAnimatorParameters();
        }

        private void CacheAnimatorParameters()
        {
            speedParameterId = Animator.StringToHash("Locomotion_Speed");
            directionXParameterId = Animator.StringToHash("Direction_X");
            directionYParameterId = Animator.StringToHash("Direction_Y");
            isCrouchingParameterId = Animator.StringToHash("IsCrouching");
            isJumpingParameterId = Animator.StringToHash("IsJumping");
            isLandingParameterId = Animator.StringToHash("IsLanding");
        }

        private void Update()
        {
            UpdateAnimationParameters();
        }

        private void UpdateAnimationParameters()
        {
            animator.SetFloat(speedParameterId, currentSpeed, transitionSmoothTime, Time.deltaTime);
            animator.SetFloat(directionXParameterId, currentDirection.x, directionSmoothTime, Time.deltaTime);
            animator.SetFloat(directionYParameterId, currentDirection.y, directionSmoothTime, Time.deltaTime);
        }

        /// <summary>
        /// Updates the locomotion speed parameter based on movement direction and desired speed.
        /// </summary>
        public void SetLocomotionSpeed(Vector3 moveDirection, float desiredSpeed, float maxSpeed)
        {
            if (moveDirection.sqrMagnitude > 0.01f)
            {
                currentSpeed = Mathf.Lerp(currentSpeed, desiredSpeed / maxSpeed, transitionSmoothTime);
            }
            else
            {
                currentSpeed = Mathf.Lerp(currentSpeed, 0f, transitionSmoothTime);
            }

            Vector3 relativeDirection = transform.parent != null
                ? transform.parent.TransformDirection(moveDirection)
                : moveDirection;

            currentDirection.x = relativeDirection.x;
            currentDirection.y = relativeDirection.z;
        }

        /// <summary>
        /// Sets whether the character is crouching.
        /// </summary>
        public void SetCrouching(bool isCrouching)
        {
            animator.SetBool(isCrouchingParameterId, isCrouching);
        }

        /// <summary>
        /// Triggers jump animation and state.
        /// </summary>
        public void PlayJump()
        {
            isJumping = true;
            animator.SetBool(isJumpingParameterId, true);
        }

        /// <summary>
        /// Called when player lands after jump.
        /// </summary>
        public void PlayLanding()
        {
            isJumping = false;
            isLanding = true;
            animator.SetBool(isJumpingParameterId, false);
            animator.SetBool(isLandingParameterId, true);

            Invoke(nameof(ClearLandingState), 0.3f);
        }

        private void ClearLandingState()
        {
            isLanding = false;
            animator.SetBool(isLandingParameterId, false);
        }

        /// <summary>
        /// Gets the current animation state info.
        /// </summary>
        public AnimatorStateInfo GetCurrentStateInfo()
        {
            return animator.GetCurrentAnimatorStateInfo(0);
        }

        /// <summary>
        /// Gets animation length in seconds for a given state hash.
        /// </summary>
        public float GetAnimationLength(int stateHash)
        {
            AnimatorClipInfo[] clipInfo = animator.GetCurrentAnimatorClipInfo(0);
            foreach (AnimatorClipInfo info in clipInfo)
            {
                if (info.clip != null)
                {
                    return info.clip.length;
                }
            }
            return 0f;
        }

        public Animator GetAnimator() => animator;
    }
}
