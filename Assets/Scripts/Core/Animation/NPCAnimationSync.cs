using UnityEngine;
using ProtocolEMR.Core.AI;

namespace ProtocolEMR.Core.Animation
{
    /// <summary>
    /// NPC animation synchronization component that bridges AI system with the animation system.
    /// Keeps NPC animations in sync with movement speed, AI state, and combat actions.
    /// Works alongside NPCAnimationController for state-based animation control.
    /// </summary>
    public class NPCAnimationSync : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private NPCController npcController;
        [SerializeField] private CharacterAnimationController characterAnimationController;
        [SerializeField] private Animator animator;

        [Header("Animation Configuration")]
        [SerializeField] private float locomotionUpdateFrequency = 0.05f;
        [SerializeField] private float combatAnimationBlendTime = 0.2f;

        [Header("Combat Animation Settings")]
        [SerializeField] private string attackAnimationPrefix = "Attack";
        [SerializeField] private string damageAnimationPrefix = "Damage";
        [SerializeField] private string deathAnimationName = "Death";

        private Rigidbody rigidbody;
        private float locomotionUpdateTimer;
        private float previousSpeed;
        private bool wasInCombat;
        private bool wasAttacking;
        private bool wasDead;

        private void Awake()
        {
            if (animator == null)
            {
                animator = GetComponent<Animator>();
            }

            if (npcController == null)
            {
                npcController = GetComponent<NPCController>();
            }

            if (characterAnimationController == null)
            {
                characterAnimationController = GetComponent<CharacterAnimationController>();
                if (characterAnimationController == null)
                {
                    characterAnimationController = gameObject.AddComponent<CharacterAnimationController>();
                }
            }

            rigidbody = GetComponent<Rigidbody>();
        }

        private void Update()
        {
            if (npcController == null || animator == null)
                return;

            locomotionUpdateTimer -= Time.deltaTime;
            if (locomotionUpdateTimer <= 0)
            {
                UpdateLocomotionAnimations();
                locomotionUpdateTimer = locomotionUpdateFrequency;
            }

            UpdateStateAnimations();
            UpdateCombatAnimations();
        }

        /// <summary>
        /// Updates locomotion animations based on NPC movement speed.
        /// </summary>
        private void UpdateLocomotionAnimations()
        {
            if (rigidbody == null) return;

            // Calculate horizontal speed
            Vector3 velocity = rigidbody.velocity;
            float horizontalSpeed = new Vector3(velocity.x, 0f, velocity.z).magnitude;

            // Update animation controller
            if (characterAnimationController != null)
            {
                characterAnimationController.SetMovementSpeed(horizontalSpeed, horizontalSpeed > 0.1f);
            }

            previousSpeed = horizontalSpeed;
        }

        /// <summary>
        /// Updates animations based on NPC AI state.
        /// </summary>
        private void UpdateStateAnimations()
        {
            if (npcController == null) return;

            NPCState currentState = npcController.CurrentState;

            switch (currentState)
            {
                case NPCState.Idle:
                    if (characterAnimationController != null && characterAnimationController.CurrentLocomotionState != CharacterAnimationController.AnimationState.Idle)
                    {
                        characterAnimationController.SetMovementSpeed(0, false);
                    }
                    break;

                case NPCState.Patrol:
                    // Locomotion handled by movement system
                    break;

                case NPCState.Alert:
                    if (animator != null)
                    {
                        animator.SetTrigger("Alert");
                    }
                    break;

                case NPCState.Chase:
                    // Locomotion handled by movement system
                    break;

                case NPCState.Flee:
                    if (animator != null)
                    {
                        animator.SetTrigger("Flee");
                    }
                    break;

                case NPCState.Investigate:
                    if (animator != null)
                    {
                        animator.SetTrigger("Investigate");
                    }
                    break;

                case NPCState.Hide:
                    if (characterAnimationController != null)
                    {
                        characterAnimationController.SetCrouching(true);
                    }
                    break;

                case NPCState.Stun:
                    PlayStunAnimation();
                    break;

                case NPCState.Dead:
                    if (!wasDead)
                    {
                        PlayDeathAnimation();
                        wasDead = true;
                    }
                    break;
            }
        }

        /// <summary>
        /// Updates combat-related animations.
        /// </summary>
        private void UpdateCombatAnimations()
        {
            if (npcController == null || animator == null)
                return;

            bool isCurrentlyInCombat = npcController.CurrentState == NPCState.Attack || npcController.CurrentState == NPCState.Chase;
            bool isCurrentlyAttacking = npcController.CurrentState == NPCState.Attack;

            // Detect combat state change
            if (isCurrentlyInCombat != wasInCombat)
            {
                animator.SetBool("InCombat", isCurrentlyInCombat);
                wasInCombat = isCurrentlyInCombat;
            }

            // Detect attack state change
            if (isCurrentlyAttacking != wasAttacking)
            {
                wasAttacking = isCurrentlyAttacking;
                if (isCurrentlyAttacking)
                {
                    TriggerAttackAnimation();
                }
            }
        }

        /// <summary>
        /// Triggers attack animation based on NPC type and combat data.
        /// </summary>
        public void TriggerAttackAnimation(int attackIndex = -1)
        {
            if (animator == null) return;

            string attackAnimName = attackAnimationPrefix;
            if (attackIndex >= 0)
            {
                attackAnimName += (attackIndex + 1);
            }

            animator.SetTrigger(attackAnimName);
        }

        /// <summary>
        /// Plays damage animation with optional knockback direction.
        /// </summary>
        public void PlayDamageAnimation(Vector3 knockbackDirection = default)
        {
            if (characterAnimationController != null)
            {
                characterAnimationController.PlayDamageAnimation();
            }
            else if (animator != null)
            {
                animator.SetTrigger(damageAnimationPrefix);
            }
        }

        /// <summary>
        /// Plays stun animation.
        /// </summary>
        private void PlayStunAnimation()
        {
            if (animator != null)
            {
                animator.SetBool("IsStunned", true);
                animator.SetTrigger("Stun");
            }
        }

        /// <summary>
        /// Plays death animation.
        /// </summary>
        private void PlayDeathAnimation()
        {
            if (characterAnimationController != null)
            {
                characterAnimationController.PlayDeathAnimation();
            }
            else if (animator != null)
            {
                animator.SetBool("IsDead", true);
                animator.SetTrigger(deathAnimationName);
            }
        }

        /// <summary>
        /// Sets grounded state for animation feedback.
        /// </summary>
        public void SetGrounded(bool grounded)
        {
            if (characterAnimationController != null)
            {
                characterAnimationController.SetGrounded(grounded);
            }
            else if (animator != null)
            {
                animator.SetBool("IsGrounded", grounded);
            }
        }

        /// <summary>
        /// Sets jumping state for animation feedback.
        /// </summary>
        public void SetJumping(bool jumping)
        {
            if (jumping)
            {
                if (characterAnimationController != null)
                {
                    characterAnimationController.PlayJumpAnimation();
                }
                else if (animator != null)
                {
                    animator.SetTrigger("Jump");
                }
            }
        }

        /// <summary>
        /// Gets the current animation state name.
        /// </summary>
        public string GetCurrentAnimationState()
        {
            if (animator == null) return "None";

            AnimatorClipInfo[] clipInfo = animator.GetCurrentAnimatorClipInfo(0);
            if (clipInfo.Length > 0)
            {
                return clipInfo[0].clip.name;
            }
            return "None";
        }

        /// <summary>
        /// Gets the length of a specific animation.
        /// </summary>
        public float GetAnimationLength(string animationName)
        {
            if (animator == null || animator.runtimeAnimatorController == null)
                return 0f;

            AnimationClip[] clips = animator.runtimeAnimatorController.animationClips;
            foreach (AnimationClip clip in clips)
            {
                if (clip.name == animationName)
                {
                    return clip.length;
                }
            }
            return 0f;
        }

        /// <summary>
        /// Returns the character animation controller component.
        /// </summary>
        public CharacterAnimationController GetCharacterAnimationController()
        {
            return characterAnimationController;
        }

        /// <summary>
        /// Enables or disables animation synchronization.
        /// </summary>
        public void SetAnimationSyncEnabled(bool enabled)
        {
            enabled = enabled;
        }
    }
}
