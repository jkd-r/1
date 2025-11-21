using System;
using UnityEngine;
using System.Collections.Generic;

namespace ProtocolEMR.Core.Animation
{
    /// <summary>
    /// State machine for managing animation transitions with smooth blending and state queuing.
    /// Handles seamless transitions between animation states and provides query methods for current state.
    /// </summary>
    [System.Serializable]
    public class AnimationStateManager
    {
        [System.Serializable]
        public class AnimationState
        {
            public string stateName;
            public float transitionDuration = 0.2f;
            public bool canInterrupt = true;
            public int priority = 0;
        }

        private Animator animator;
        private AnimationState currentState;
        private AnimationState previousState;
        private Queue<AnimationState> stateQueue;
        private float stateTransitionTimer;
        private float stateTransitionDuration;
        private bool isTransitioning;

        public AnimationState CurrentState => currentState;
        public AnimationState PreviousState => previousState;
        public bool IsTransitioning => isTransitioning;
        public string CurrentStateName => currentState?.stateName ?? "None";
        public float TransitionProgress => stateTransitionDuration > 0 ? stateTransitionTimer / stateTransitionDuration : 1.0f;

        public AnimationStateManager(Animator anim)
        {
            animator = anim;
            stateQueue = new Queue<AnimationState>();
            currentState = null;
            previousState = null;
            isTransitioning = false;
        }

        /// <summary>
        /// Updates the state machine, handling transitions and state queuing.
        /// </summary>
        public void Update(float deltaTime)
        {
            if (animator == null) return;

            if (isTransitioning)
            {
                stateTransitionTimer -= deltaTime;
                if (stateTransitionTimer <= 0)
                {
                    isTransitioning = false;
                    stateTransitionTimer = 0;
                }
            }

            // Process state queue if available
            if (stateQueue.Count > 0 && !isTransitioning)
            {
                AnimationState nextState = stateQueue.Dequeue();
                TransitionToState(nextState);
            }
        }

        /// <summary>
        /// Transitions to a new animation state with optional queueing.
        /// </summary>
        public void TransitionToState(string stateName, float transitionDuration = 0.2f, bool queue = false, int priority = 0)
        {
            var state = new AnimationState
            {
                stateName = stateName,
                transitionDuration = transitionDuration,
                canInterrupt = true,
                priority = priority
            };

            if (queue && isTransitioning)
            {
                stateQueue.Enqueue(state);
            }
            else
            {
                TransitionToState(state);
            }
        }

        /// <summary>
        /// Internal method to perform the actual state transition.
        /// </summary>
        private void TransitionToState(AnimationState newState)
        {
            if (animator == null || newState == null) return;

            // Don't transition to the same state
            if (currentState != null && currentState.stateName == newState.stateName)
            {
                return;
            }

            // Store previous state
            previousState = currentState;
            currentState = newState;

            // Start transition
            animator.CrossFade(newState.stateName, newState.transitionDuration);
            stateTransitionDuration = newState.transitionDuration;
            stateTransitionTimer = newState.transitionDuration;
            isTransitioning = true;
        }

        /// <summary>
        /// Sets a boolean animator parameter.
        /// </summary>
        public void SetBool(string parameterName, bool value)
        {
            if (animator != null)
            {
                animator.SetBool(parameterName, value);
            }
        }

        /// <summary>
        /// Sets a float animator parameter.
        /// </summary>
        public void SetFloat(string parameterName, float value)
        {
            if (animator != null)
            {
                animator.SetFloat(parameterName, value);
            }
        }

        /// <summary>
        /// Sets an integer animator parameter.
        /// </summary>
        public void SetInteger(string parameterName, int value)
        {
            if (animator != null)
            {
                animator.SetInteger(parameterName, value);
            }
        }

        /// <summary>
        /// Triggers an animator parameter.
        /// </summary>
        public void SetTrigger(string parameterName)
        {
            if (animator != null)
            {
                animator.SetTrigger(parameterName);
            }
        }

        /// <summary>
        /// Gets a boolean animator parameter.
        /// </summary>
        public bool GetBool(string parameterName)
        {
            if (animator != null)
            {
                return animator.GetBool(parameterName);
            }
            return false;
        }

        /// <summary>
        /// Gets a float animator parameter.
        /// </summary>
        public float GetFloat(string parameterName)
        {
            if (animator != null)
            {
                return animator.GetFloat(parameterName);
            }
            return 0f;
        }

        /// <summary>
        /// Gets an integer animator parameter.
        /// </summary>
        public int GetInteger(string parameterName)
        {
            if (animator != null)
            {
                return animator.GetInteger(parameterName);
            }
            return 0;
        }

        /// <summary>
        /// Gets the current animation clip info.
        /// </summary>
        public AnimatorClipInfo[] GetCurrentClipInfo(int layerIndex = 0)
        {
            if (animator != null)
            {
                return animator.GetCurrentAnimatorClipInfo(layerIndex);
            }
            return new AnimatorClipInfo[0];
        }

        /// <summary>
        /// Gets the length of a specific animation clip.
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
        /// Gets the name of the currently playing animation.
        /// </summary>
        public string GetCurrentAnimationName()
        {
            AnimatorClipInfo[] clipInfo = GetCurrentClipInfo();
            if (clipInfo.Length > 0)
            {
                return clipInfo[0].clip.name;
            }
            return "None";
        }

        /// <summary>
        /// Gets the normalized time of the current animation (0-1).
        /// </summary>
        public float GetCurrentAnimationNormalizedTime()
        {
            if (animator == null) return 0f;
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            return stateInfo.normalizedTime;
        }

        /// <summary>
        /// Checks if an animation is currently playing.
        /// </summary>
        public bool IsAnimationPlaying(string animationName)
        {
            if (animator == null) return false;
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            return stateInfo.IsName(animationName);
        }

        /// <summary>
        /// Clears all queued states.
        /// </summary>
        public void ClearQueue()
        {
            stateQueue.Clear();
        }

        /// <summary>
        /// Sets the layer weight for animator blending.
        /// </summary>
        public void SetLayerWeight(int layerIndex, float weight)
        {
            if (animator != null && layerIndex < animator.layerCount)
            {
                animator.SetLayerWeight(layerIndex, weight);
            }
        }

        /// <summary>
        /// Gets the animator component.
        /// </summary>
        public Animator GetAnimator()
        {
            return animator;
        }
    }
}
