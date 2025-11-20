using UnityEngine;

namespace ProtocolEMR.Core.AI
{
    /// <summary>
    /// Condition nodes for behavior trees - check various game states.
    /// </summary>
    public abstract class ConditionNode : BehaviorNode
    {
        protected ConditionNode(NPCController controller) : base(controller) { }

        protected override NodeState OnExecute()
        {
            return CheckCondition() ? NodeState.Success : NodeState.Failure;
        }

        protected abstract bool CheckCondition();
    }

    /// <summary>
    /// Action nodes for behavior trees - perform specific behaviors.
    /// </summary>
    public abstract class ActionNode : BehaviorNode
    {
        protected ActionNode(NPCController controller) : base(controller) { }

        protected override NodeState OnExecute()
        {
            return PerformAction() ? NodeState.Success : NodeState.Failure;
        }

        protected abstract bool PerformAction();
    }

    // CONDITION NODES

    /// <summary>
    /// Checks if NPC is stunned.
    /// </summary>
    public class IsStunnedCondition : ConditionNode
    {
        public IsStunnedCondition(NPCController controller) : base(controller) { }

        protected override bool CheckCondition()
        {
            return npc.CurrentState == NPCState.Stun;
        }
    }

    /// <summary>
    /// Checks if NPC is dead.
    /// </summary>
    public class IsDeadCondition : ConditionNode
    {
        public IsDeadCondition(NPCController controller) : base(controller) { }

        protected override bool CheckCondition()
        {
            return npc.CurrentState == NPCState.Dead || npc.CurrentHealth <= 0;
        }
    }

    /// <summary>
    /// Checks if NPC can see the player.
    /// </summary>
    public class CanSeePlayerCondition : ConditionNode
    {
        public CanSeePlayerCondition(NPCController controller) : base(controller) { }

        protected override bool CheckCondition()
        {
            return npc.Perception.canSeePlayer;
        }
    }

    /// <summary>
    /// Checks if player is within attack range.
    /// </summary>
    public class IsPlayerInRangeCondition : ConditionNode
    {
        private float range;

        public IsPlayerInRangeCondition(NPCController controller, float attackRange) : base(controller)
        {
            range = attackRange;
        }

        protected override bool CheckCondition()
        {
            if (npc.Perception.lastKnownPlayerPosition == Vector3.zero)
                return false;

            float distance = Vector3.Distance(npc.transform.position, npc.Perception.lastKnownPlayerPosition);
            return distance <= range;
        }
    }

    /// <summary>
    /// Checks if NPC heard the player.
    /// </summary>
    public class HeardPlayerCondition : ConditionNode
    {
        public HeardPlayerCondition(NPCController controller) : base(controller) { }

        protected override bool CheckCondition()
        {
            return npc.Perception.canHearPlayer || npc.Perception.timeSinceSound < 2.0f;
        }
    }

    /// <summary>
    /// Checks if NPC health is below threshold.
    /// </summary>
    public class HealthLowCondition : ConditionNode
    {
        private float threshold;

        public HealthLowCondition(NPCController controller, float healthThreshold) : base(controller)
        {
            threshold = healthThreshold;
        }

        protected override bool CheckCondition()
        {
            return npc.CurrentHealth <= (npc.Parameters.maxHealth * threshold);
        }
    }

    /// <summary>
    /// Checks if NPC is at patrol point.
    /// </summary>
    public class AtPatrolPointCondition : ConditionNode
    {
        public AtPatrolPointCondition(NPCController controller) : base(controller) { }

        protected override bool CheckCondition()
        {
            if (npc.Navigation.patrolPoints.Length == 0)
                return false;

            Vector3 targetPoint = npc.Navigation.patrolPoints[npc.Navigation.currentPatrolIndex];
            float distance = Vector3.Distance(npc.transform.position, targetPoint);
            return distance < 1.0f;
        }
    }

    /// <summary>
    /// Checks if NPC has cover nearby.
    /// </summary>
    public class HasCoverNearbyCondition : ConditionNode
    {
        public HasCoverNearbyCondition(NPCController controller) : base(controller) { }

        protected override bool CheckCondition()
        {
            // Simple implementation - check for nearby walls/obstacles
            Collider[] colliders = Physics.OverlapSphere(npc.transform.position, 5.0f);
            foreach (var collider in colliders)
            {
                if (collider.gameObject != npc.gameObject && collider.gameObject.layer == LayerMask.NameToLayer("Environment"))
                {
                    return true;
                }
            }
            return false;
        }
    }

    // ACTION NODES

    /// <summary>
    /// Plays stun animation and reduces health.
    /// </summary>
    public class PlayStunAction : ActionNode
    {
        public PlayStunAction(NPCController controller) : base(controller) { }

        protected override bool PerformAction()
        {
            npc.SetState(NPCState.Stun);
            npc.Animator.SetTrigger("Stun");
            return true;
        }
    }

    /// <summary>
    /// Plays death animation and makes NPC inactive.
    /// </summary>
    public class PlayDeathAction : ActionNode
    {
        public PlayDeathAction(NPCController controller) : base(controller) { }

        protected override bool PerformAction()
        {
            npc.SetState(NPCState.Dead);
            npc.Animator.SetTrigger("Die");
            npc.enabled = false;
            return true;
        }
    }

    /// <summary>
    /// Chases and attacks the player.
    /// </summary>
    public class ChaseAndAttackAction : ActionNode
    {
        public ChaseAndAttackAction(NPCController controller) : base(controller) { }

        protected override bool PerformAction()
        {
            npc.SetState(NPCState.Chase);
            
            if (npc.Perception.canSeePlayer)
            {
                npc.Navigation.targetPosition = npc.Perception.lastKnownPlayerPosition;
                npc.MoveToTarget(npc.Parameters.sprintSpeed);
                
                // Attack if in range
                float distance = Vector3.Distance(npc.transform.position, npc.Perception.lastKnownPlayerPosition);
                if (distance <= 2.0f && npc.Combat.canAttack)
                {
                    npc.Attack();
                }
            }
            
            return true;
        }
    }

    /// <summary>
    /// Investigates sound location.
    /// </summary>
    public class InvestigateSoundAction : ActionNode
    {
        public InvestigateSoundAction(NPCController controller) : base(controller) { }

        protected override bool PerformAction()
        {
            npc.SetState(NPCState.Investigate);
            npc.Navigation.targetPosition = npc.Perception.soundSource;
            npc.MoveToTarget(npc.Parameters.runSpeed);
            return true;
        }
    }

    /// <summary>
    /// Flees toward safezone.
    /// </summary>
    public class FleeToSafezoneAction : ActionNode
    {
        public FleeToSafezoneAction(NPCController controller) : base(controller) { }

        protected override bool PerformAction()
        {
            npc.SetState(NPCState.Flee);
            
            // Find nearest safezone or patrol area
            Vector3 safePosition = FindSafePosition();
            npc.Navigation.targetPosition = safePosition;
            npc.MoveToTarget(npc.Parameters.sprintSpeed * 1.2f); // Flee faster than normal sprint
            
            return true;
        }

        private Vector3 FindSafePosition()
        {
            // Simple implementation - return original patrol position
            if (npc.Navigation.patrolPoints.Length > 0)
            {
                return npc.Navigation.patrolPoints[0];
            }
            
            // Or move away from player
            Vector3 direction = (npc.transform.position - npc.Perception.lastKnownPlayerPosition).normalized;
            return npc.transform.position + direction * 20.0f;
        }
    }

    /// <summary>
    /// Patrols between points.
    /// </summary>
    public class PatrolAction : ActionNode
    {
        public PatrolAction(NPCController controller) : base(controller) { }

        protected override bool PerformAction()
        {
            npc.SetState(NPCState.Patrol);
            
            if (npc.Navigation.patrolPoints.Length == 0)
            {
                return false;
            }

            Vector3 targetPoint = npc.Navigation.patrolPoints[npc.Navigation.currentPatrolIndex];
            npc.Navigation.targetPosition = targetPoint;
            npc.MoveToTarget(npc.Parameters.walkSpeed);
            
            return true;
        }
    }

    /// <summary>
    /// Waits at patrol point.
    /// </summary>
    public class WaitAtPointAction : ActionNode
    {
        private float waitDuration;

        public WaitAtPointAction(NPCController controller, float duration = 3.0f) : base(controller)
        {
            waitDuration = duration;
        }

        protected override bool PerformAction()
        {
            npc.Navigation.waitTimeAtPoint += Time.deltaTime;
            
            if (npc.Navigation.waitTimeAtPoint >= waitDuration)
            {
                npc.Navigation.waitTimeAtPoint = 0f;
                npc.Navigation.currentPatrolIndex = (npc.Navigation.currentPatrolIndex + 1) % npc.Navigation.patrolPoints.Length;
                return true;
            }
            
            return false; // Still waiting
        }
    }

    /// <summary>
    /// Hides behind cover.
    /// </summary>
    public class HideBehindCoverAction : ActionNode
    {
        public HideBehindCoverAction(NPCController controller) : base(controller) { }

        protected override bool PerformAction()
        {
            npc.SetState(NPCState.Hide);
            
            // Find cover position
            Vector3 coverPosition = FindCoverPosition();
            npc.Navigation.targetPosition = coverPosition;
            npc.MoveToTarget(npc.Parameters.runSpeed);
            
            // Crouch when at cover
            float distance = Vector3.Distance(npc.transform.position, coverPosition);
            if (distance < 1.0f)
            {
                npc.Animator.SetBool("IsCrouching", true);
            }
            
            return true;
        }

        private Vector3 FindCoverPosition()
        {
            // Simple implementation - find nearest wall
            Collider[] colliders = Physics.OverlapSphere(npc.transform.position, 5.0f);
            Vector3 bestCover = npc.transform.position;
            float bestDistance = float.MaxValue;
            
            foreach (var collider in colliders)
            {
                if (collider.gameObject != npc.gameObject && collider.gameObject.layer == LayerMask.NameToLayer("Environment"))
                {
                    Vector3 coverPos = collider.ClosestPoint(npc.transform.position);
                    float distance = Vector3.Distance(npc.transform.position, coverPos);
                    
                    if (distance < bestDistance)
                    {
                        bestDistance = distance;
                        bestCover = coverPos;
                    }
                }
            }
            
            return bestCover;
        }
    }
}