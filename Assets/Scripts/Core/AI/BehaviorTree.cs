using UnityEngine;
using System.Collections.Generic;

namespace ProtocolEMR.Core.AI
{
    /// <summary>
    /// Base class for all behavior tree nodes.
    /// </summary>
    public abstract class BehaviorNode
    {
        public enum NodeState
        {
            Ready,
            Running,
            Success,
            Failure
        }

        protected NodeState currentState = NodeState.Ready;
        protected NPCController npc;

        public NodeState CurrentState => currentState;
        public NPCController NPC => npc;

        public BehaviorNode(NPCController controller)
        {
            npc = controller;
        }

        public virtual NodeState Execute()
        {
            if (currentState == NodeState.Ready || currentState == NodeState.Success || currentState == NodeState.Failure)
            {
                OnStart();
            }

            currentState = OnExecute();

            if (currentState == NodeState.Success || currentState == NodeState.Failure)
            {
                OnStop();
            }

            return currentState;
        }

        public virtual void Reset()
        {
            currentState = NodeState.Ready;
            OnReset();
        }

        protected virtual void OnStart() { }
        protected virtual void OnStop() { }
        protected virtual void OnReset() { }
        protected abstract NodeState OnExecute();
    }

    /// <summary>
    /// Selector node - executes children sequentially until one succeeds.
    /// </summary>
    public class SelectorNode : BehaviorNode
    {
        private List<BehaviorNode> children;
        private int currentChildIndex = 0;

        public SelectorNode(NPCController controller, params BehaviorNode[] childNodes) : base(controller)
        {
            children = new List<BehaviorNode>(childNodes);
        }

        protected override void OnStart()
        {
            currentChildIndex = 0;
            foreach (var child in children)
            {
                child.Reset();
            }
        }

        protected override NodeState OnExecute()
        {
            if (children.Count == 0)
                return NodeState.Failure;

            while (currentChildIndex < children.Count)
            {
                var childState = children[currentChildIndex].Execute();

                if (childState == NodeState.Running)
                    return NodeState.Running;

                if (childState == NodeState.Success)
                    return NodeState.Success;

                currentChildIndex++;
            }

            return NodeState.Failure;
        }

        protected override void OnReset()
        {
            currentChildIndex = 0;
            foreach (var child in children)
            {
                child.Reset();
            }
        }
    }

    /// <summary>
    /// Sequence node - executes children sequentially until one fails.
    /// </summary>
    public class SequenceNode : BehaviorNode
    {
        private List<BehaviorNode> children;
        private int currentChildIndex = 0;

        public SequenceNode(NPCController controller, params BehaviorNode[] childNodes) : base(controller)
        {
            children = new List<BehaviorNode>(childNodes);
        }

        protected override void OnStart()
        {
            currentChildIndex = 0;
            foreach (var child in children)
            {
                child.Reset();
            }
        }

        protected override NodeState OnExecute()
        {
            if (children.Count == 0)
                return NodeState.Success;

            while (currentChildIndex < children.Count)
            {
                var childState = children[currentChildIndex].Execute();

                if (childState == NodeState.Running)
                    return NodeState.Running;

                if (childState == NodeState.Failure)
                    return NodeState.Failure;

                currentChildIndex++;
            }

            return NodeState.Success;
        }

        protected override void OnReset()
        {
            currentChildIndex = 0;
            foreach (var child in children)
            {
                child.Reset();
            }
        }
    }

    /// <summary>
    /// Decorator node that inverts the result of its child.
    /// </summary>
    public class InverterNode : BehaviorNode
    {
        private BehaviorNode child;

        public InverterNode(NPCController controller, BehaviorNode childNode) : base(controller)
        {
            child = childNode;
        }

        protected override NodeState OnExecute()
        {
            var childState = child.Execute();

            switch (childState)
            {
                case NodeState.Success:
                    return NodeState.Failure;
                case NodeState.Failure:
                    return NodeState.Success;
                default:
                    return childState;
            }
        }

        protected override void OnReset()
        {
            child.Reset();
        }
    }

    /// <summary>
    /// Decorator node that repeats its child a specified number of times.
    /// </summary>
    public class RepeaterNode : BehaviorNode
    {
        private BehaviorNode child;
        private int repeatCount;
        private int currentIteration = 0;

        public RepeaterNode(NPCController controller, BehaviorNode childNode, int repetitions = -1) : base(controller)
        {
            child = childNode;
            repeatCount = repetitions;
        }

        protected override void OnStart()
        {
            currentIteration = 0;
            child.Reset();
        }

        protected override NodeState OnExecute()
        {
            while (repeatCount == -1 || currentIteration < repeatCount)
            {
                var childState = child.Execute();

                if (childState == NodeState.Running)
                    return NodeState.Running;

                currentIteration++;

                if (repeatCount != -1 && currentIteration >= repeatCount)
                    break;

                child.Reset();
            }

            return NodeState.Success;
        }

        protected override void OnReset()
        {
            currentIteration = 0;
            child.Reset();
        }
    }
}