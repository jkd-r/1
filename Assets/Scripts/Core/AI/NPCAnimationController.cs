using UnityEngine;

namespace ProtocolEMR.Core.AI
{
    /// <summary>
    /// Animation controller for NPCs - handles state transitions and animation blending.
    /// </summary>
    [System.Serializable]
    public class NPCAnimationController
    {
        private NPCController npc;
        private Animator animator;
        private AnimationData animationData;
        
        [Header("Animation Settings")]
        [SerializeField] private float animationBlendTime = 0.2f;
        [SerializeField] private float locomotionBlendTime = 0.1f;
        [SerializeField] private string[] attackAnimations = { "Attack1", "Attack2", "Attack3" };
        [SerializeField] private string[] hurtAnimations = { "Hurt1", "Hurt2" };
        [SerializeField] private string[] idleAnimations = { "Idle", "Idle2", "Idle3" };

        public AnimationData Data => animationData;
        public Animator Animator => animator;
        public string CurrentAnimationState => animationData.currentAnimationState;

        public NPCAnimationController(NPCController controller, Animator anim)
        {
            npc = controller;
            animator = anim;
            animationData = new AnimationData();
            InitializeAnimator();
        }

        /// <summary>
        /// Initializes animator controller.
        /// </summary>
        private void InitializeAnimator()
        {
            if (animator == null) return;

            // Set default animation parameters
            animator.SetFloat("Speed", 0f);
            animator.SetBool("IsGrounded", true);
            animator.SetBool("IsAttacking", false);
            animator.SetBool("IsDead", false);
            animator.SetBool("IsStunned", false);
            animator.SetBool("InCombat", false);
            animator.SetBool("IsCrouching", false);
        }

        /// <summary>
        /// Updates animations based on current state and movement.
        /// </summary>
        public void UpdateAnimations()
        {
            if (animator == null) return;

            UpdateLocomotionAnimation();
            UpdateStateAnimation();
            UpdateAnimationData();
        }

        /// <summary>
        /// Updates locomotion animations based on movement speed.
        /// </summary>
        private void UpdateLocomotionAnimation()
        {
            Vector3 velocity = npc.GetComponent<Rigidbody>().velocity;
            float horizontalSpeed = new Vector3(velocity.x, 0f, velocity.z).magnitude;
            
            // Calculate normalized speed (0 = idle, 1 = walk, 2 = run, 3 = sprint)
            float normalizedSpeed = 0f;
            
            if (horizontalSpeed > 0.1f)
            {
                if (horizontalSpeed <= npc.Parameters.walkSpeed + 0.5f)
                {
                    normalizedSpeed = 1f; // Walk
                }
                else if (horizontalSpeed <= npc.Parameters.runSpeed + 1.0f)
                {
                    normalizedSpeed = 2f; // Run
                }
                else
                {
                    normalizedSpeed = 3f; // Sprint
                }
            }

            // Update animator speed parameter with smooth blending
            float currentSpeed = animator.GetFloat("Speed");
            float targetSpeed = normalizedSpeed;
            float newSpeed = Mathf.Lerp(currentSpeed, targetSpeed, Time.deltaTime / locomotionBlendTime);
            animator.SetFloat("Speed", newSpeed);

            // Update movement flags
            animationData.isMoving = horizontalSpeed > 0.1f;
            animationData.isRunning = normalizedSpeed >= 2f;

            animator.SetBool("IsMoving", animationData.isMoving);
            animator.SetBool("IsRunning", animationData.isRunning);
        }

        /// <summary>
        /// Updates state-based animations.
        /// </summary>
        private void UpdateStateAnimation()
        {
            switch (npc.CurrentState)
            {
                case NPCState.Idle:
                    if (!animationData.isMoving)
                    {
                        PlayIdleAnimation();
                    }
                    break;

                case NPCState.Alert:
                    animator.SetTrigger("Alert");
                    break;

                case NPCState.Chase:
                    // Locomotion handled by UpdateLocomotionAnimation
                    break;

                case NPCState.Flee:
                    animator.SetTrigger("Flee");
                    break;

                case NPCState.Hide:
                    animator.SetBool("IsCrouching", true);
                    break;

                case NPCState.Attack:
                    // Attack animations triggered by combat system
                    break;

                case NPCState.Patrol:
                    // Locomotion handled by UpdateLocomotionAnimation
                    break;

                case NPCState.Investigate:
                    animator.SetTrigger("Investigate");
                    break;

                case NPCState.Stun:
                    if (!animationData.isStunned)
                    {
                        PlayStunAnimation();
                    }
                    break;

                case NPCState.Dead:
                    if (!animationData.isDead)
                    {
                        PlayDeathAnimation();
                    }
                    break;
            }
        }

        /// <summary>
        /// Updates animation data for external access.
        /// </summary>
        private void UpdateAnimationData()
        {
            animationData.currentAnimationState = GetCurrentAnimationName();
            animationData.animationBlendTime = animationBlendTime;
            animationData.isAttacking = animator.GetBool("IsAttacking");
            animationData.isDead = animator.GetBool("IsDead");
            animationData.isStunned = animator.GetBool("IsStunned");
        }

        /// <summary>
        /// Plays a random idle animation.
        /// </summary>
        private void PlayIdleAnimation()
        {
            if (Random.Range(0f, 1f) < 0.01f) // 1% chance per frame
            {
                string randomIdle = idleAnimations[Random.Range(0, idleAnimations.Length)];
                animator.SetTrigger(randomIdle);
            }
        }

        /// <summary>
        /// Plays stun animation.
        /// </summary>
        private void PlayStunAnimation()
        {
            animationData.isStunned = true;
            animator.SetBool("IsStunned", true);
            animator.SetTrigger("Stun");
        }

        /// <summary>
        /// Plays death animation.
        /// </summary>
        private void PlayDeathAnimation()
        {
            animationData.isDead = true;
            animator.SetBool("IsDead", true);
            animator.SetTrigger("Die");
        }

        /// <summary>
        /// Plays attack animation.
        /// </summary>
        public void PlayAttackAnimation()
        {
            if (attackAnimations.Length > 0)
            {
                string randomAttack = attackAnimations[Random.Range(0, attackAnimations.Length)];
                animator.SetTrigger(randomAttack);
                animator.SetBool("IsAttacking", true);
                
                // Schedule attack completion
                npc.Invoke(nameof(CompleteAttack), GetAnimationLength(randomAttack));
            }
        }

        /// <summary>
        /// Plays hurt animation.
        /// </summary>
        public void PlayHurtAnimation()
        {
            if (hurtAnimations.Length > 0)
            {
                string randomHurt = hurtAnimations[Random.Range(0, hurtAnimations.Length)];
                animator.SetTrigger(randomHurt);
            }
        }

        /// <summary>
        /// Completes attack animation.
        /// </summary>
        private void CompleteAttack()
        {
            animator.SetBool("IsAttacking", false);
        }

        /// <summary>
        /// Gets the length of an animation clip.
        /// </summary>
        private float GetAnimationLength(string animationName)
        {
            AnimationClip[] clips = animator.runtimeAnimatorController.animationClips;
            foreach (AnimationClip clip in clips)
            {
                if (clip.name == animationName)
                {
                    return clip.length;
                }
            }
            return 1.0f; // Default fallback
        }

        /// <summary>
        /// Gets current animation name.
        /// </summary>
        private string GetCurrentAnimationName()
        {
            AnimatorClipInfo[] clipInfo = animator.GetCurrentAnimatorClipInfo(0);
            if (clipInfo.Length > 0)
            {
                return clipInfo[0].clip.name;
            }
            return "None";
        }

        /// <summary>
        /// Sets look direction for head tracking.
        /// </summary>
        public void SetLookDirection(Vector3 direction)
        {
            if (animator != null)
            {
                animator.SetLookAtPosition(npc.transform.position + direction);
                animator.SetLookAtWeight(1.0f, 0.5f, 1.0f, 0.0f, 0.5f);
            }
        }

        /// <summary>
        /// Resets look direction.
        /// </summary>
        public void ResetLookDirection()
        {
            if (animator != null)
            {
                animator.SetLookAtWeight(0.0f);
            }
        }

        /// <summary>
        /// Plays footstep sound.
        /// </summary>
        public void PlayFootstepSound()
        {
            if (animationData.isMoving && !animationData.isDead)
            {
                // This would integrate with audio system
                // For now, just log the footstep
                Debug.Log($"{npc.name} footstep");
            }
        }

        /// <summary>
        /// Gets animation speed multiplier for time scaling.
        /// </summary>
        public float GetAnimationSpeedMultiplier()
        {
            return animator != null ? animator.GetFloat("Speed") : 0f;
        }

        /// <summary>
        /// Sets animation layer weight.
        /// </summary>
        public void SetLayerWeight(int layerIndex, float weight)
        {
            if (animator != null && layerIndex < animator.layerCount)
            {
                animator.SetLayerWeight(layerIndex, weight);
            }
        }

        /// <summary>
        /// Cross-fades to animation state.
        /// </summary>
        public void CrossFade(string stateName, float transitionDuration = 0.2f)
        {
            if (animator != null)
            {
                animator.CrossFade(stateName, transitionDuration);
            }
        }

        /// <summary>
        /// Draws debug visualization for animation state.
        /// </summary>
        public void DrawDebugGizmos()
        {
            if (npc == null) return;

            // Draw animation state indicator above NPC
            Vector3 indicatorPos = npc.transform.position + Vector3.up * 2.5f;
            
            // Color based on animation state
            Color stateColor = Color.white;
            switch (npc.CurrentState)
            {
                case NPCState.Idle:
                    stateColor = Color.green;
                    break;
                case NPCState.Alert:
                    stateColor = Color.yellow;
                    break;
                case NPCState.Chase:
                    stateColor = Color.red;
                    break;
                case NPCState.Flee:
                    stateColor = Color.blue;
                    break;
                case NPCState.Attack:
                    stateColor = Color.magenta;
                    break;
                case NPCState.Stun:
                    stateColor = Color.cyan;
                    break;
                case NPCState.Dead:
                    stateColor = Color.black;
                    break;
            }

            // This would normally draw UI text, but for debug we'll use gizmos
            Gizmos.color = stateColor;
            Gizmos.DrawWireSphere(indicatorPos, 0.1f);
        }
    }
}