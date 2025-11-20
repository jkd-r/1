using UnityEngine;

#if UNITY_EDITOR
using UnityEditor.Animations;
using UnityEditor;
using System.Collections.Generic;

namespace ProtocolEMR.Core.Animation
{
    /// <summary>
    /// Editor helper script to programmatically create the animator controller for locomotion.
    /// This is optional - the controller can also be created manually in the Animator window.
    /// Use menu item: Tools → Protocol EMR → Create Locomotion Animator
    /// </summary>
    public class AnimatorSetupHelper : MonoBehaviour
    {
        [MenuItem("Tools/Protocol EMR/Create Locomotion Animator")]
        public static void CreateLocomotionAnimator()
        {
            string path = "Assets/Animations/PlayerLocomotion.controller";

            AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(path);

            // Add parameters
            controller.AddParameter("Locomotion_Speed", AnimatorControllerParameterType.Float);
            controller.AddParameter("Direction_X", AnimatorControllerParameterType.Float);
            controller.AddParameter("Direction_Y", AnimatorControllerParameterType.Float);
            controller.AddParameter("IsCrouching", AnimatorControllerParameterType.Bool);
            controller.AddParameter("IsJumping", AnimatorControllerParameterType.Bool);
            controller.AddParameter("IsLanding", AnimatorControllerParameterType.Bool);

            // Get root state machine
            AnimatorStateMachine rootStateMachine = controller.layers[0].stateMachine;

            // Clear default states
            foreach (ChildAnimatorState state in rootStateMachine.states)
            {
                rootStateMachine.RemoveState(state.state);
            }

            // Create states (without animations - those come from imported assets)
            var idleState = rootStateMachine.AddState("Idle");
            var walkState = rootStateMachine.AddState("Walk");
            var runState = rootStateMachine.AddState("Run");
            var sprintState = rootStateMachine.AddState("Sprint");
            var crouchIdleState = rootStateMachine.AddState("CrouchIdle");
            var crouchWalkState = rootStateMachine.AddState("CrouchWalk");
            var jumpState = rootStateMachine.AddState("Jump");
            var landState = rootStateMachine.AddState("Land");

            // Set default state
            rootStateMachine.defaultState = idleState;

            // Create transitions
            // Idle → Walk
            var transition = idleState.AddTransition(walkState);
            transition.AddCondition(AnimatorConditionMode.Greater, 0.1f, "Locomotion_Speed");
            transition.transitionDuration = 0.2f;

            // Walk → Run
            transition = walkState.AddTransition(runState);
            transition.AddCondition(AnimatorConditionMode.Greater, 0.4f, "Locomotion_Speed");
            transition.transitionDuration = 0.2f;

            // Run → Sprint
            transition = runState.AddTransition(sprintState);
            transition.AddCondition(AnimatorConditionMode.Greater, 0.8f, "Locomotion_Speed");
            transition.transitionDuration = 0.15f;

            // Sprint → Run
            transition = sprintState.AddTransition(runState);
            transition.AddCondition(AnimatorConditionMode.Less, 0.75f, "Locomotion_Speed");
            transition.transitionDuration = 0.2f;

            // Run → Walk
            transition = runState.AddTransition(walkState);
            transition.AddCondition(AnimatorConditionMode.Less, 0.3f, "Locomotion_Speed");
            transition.transitionDuration = 0.2f;

            // Walk → Idle
            transition = walkState.AddTransition(idleState);
            transition.AddCondition(AnimatorConditionMode.Less, 0.05f, "Locomotion_Speed");
            transition.transitionDuration = 0.2f;

            // Any → Jump
            transition = idleState.AddTransition(jumpState);
            transition.AddCondition(AnimatorConditionMode.If, 0f, "IsJumping");
            transition.transitionDuration = 0.05f;

            transition = walkState.AddTransition(jumpState);
            transition.AddCondition(AnimatorConditionMode.If, 0f, "IsJumping");
            transition.transitionDuration = 0.05f;

            transition = runState.AddTransition(jumpState);
            transition.AddCondition(AnimatorConditionMode.If, 0f, "IsJumping");
            transition.transitionDuration = 0.05f;

            // Jump → Land
            transition = jumpState.AddTransition(landState);
            transition.exitTime = 0.9f;
            transition.hasExitTime = true;
            transition.transitionDuration = 0.05f;

            // Land → Idle/Walk/Run (based on speed)
            transition = landState.AddTransition(idleState);
            transition.AddCondition(AnimatorConditionMode.Less, 0.1f, "Locomotion_Speed");
            transition.hasExitTime = false;
            transition.exitTime = 1.0f;
            transition.transitionDuration = 0.1f;

            transition = landState.AddTransition(walkState);
            transition.AddCondition(AnimatorConditionMode.Greater, 0.1f, "Locomotion_Speed");
            transition.AddCondition(AnimatorConditionMode.Less, 0.4f, "Locomotion_Speed");
            transition.hasExitTime = false;
            transition.exitTime = 1.0f;
            transition.transitionDuration = 0.1f;

            transition = landState.AddTransition(runState);
            transition.AddCondition(AnimatorConditionMode.Greater, 0.4f, "Locomotion_Speed");
            transition.hasExitTime = false;
            transition.exitTime = 1.0f;
            transition.transitionDuration = 0.1f;

            // Crouch transitions
            transition = idleState.AddTransition(crouchIdleState);
            transition.AddCondition(AnimatorConditionMode.If, 0f, "IsCrouching");
            transition.transitionDuration = 0.3f;

            transition = walkState.AddTransition(crouchIdleState);
            transition.AddCondition(AnimatorConditionMode.If, 0f, "IsCrouching");
            transition.transitionDuration = 0.3f;

            transition = crouchIdleState.AddTransition(crouchWalkState);
            transition.AddCondition(AnimatorConditionMode.Greater, 0.1f, "Locomotion_Speed");
            transition.transitionDuration = 0.2f;

            transition = crouchWalkState.AddTransition(crouchIdleState);
            transition.AddCondition(AnimatorConditionMode.Less, 0.05f, "Locomotion_Speed");
            transition.transitionDuration = 0.2f;

            transition = crouchIdleState.AddTransition(idleState);
            transition.AddCondition(AnimatorConditionMode.IfNot, 0f, "IsCrouching");
            transition.transitionDuration = 0.3f;

            transition = crouchWalkState.AddTransition(walkState);
            transition.AddCondition(AnimatorConditionMode.IfNot, 0f, "IsCrouching");
            transition.transitionDuration = 0.3f;

            AssetDatabase.SaveAssets();
            EditorUtility.DisplayDialog("Success", "Locomotion Animator Controller created at Assets/Animations/PlayerLocomotion.controller", "OK");
        }
    }
}
#endif
