using System;

namespace LittleDriver.AI.BehaviorTrees
{
    public enum NodeStatus { Success, Failure, Running }

    public abstract class Node
    {
        protected Node(string name) => Name = name;

        public string Name { get; }

        public abstract NodeStatus Tick();
    }

    public abstract class Composite : Node
    {
        protected readonly Node[] Children;

        protected Composite(string name, Node[] children) : base(name) => Children = children;
    }

    /// <summary>
    /// Tries children in order and returns the first result that is not Failure.
    /// Re-evaluated from the first child every tick, so a higher-priority branch can interrupt a running one.
    /// </summary>
    public sealed class Selector : Composite
    {
        public Selector(string name, params Node[] children) : base(name, children) { }

        public override NodeStatus Tick()
        {
            foreach (Node child in Children)
            {
                NodeStatus status = child.Tick();
                if (status != NodeStatus.Failure) return status;
            }
            return NodeStatus.Failure;
        }
    }

    /// <summary>Runs children in order while they succeed; returns the first result that is not Success.</summary>
    public sealed class Sequence : Composite
    {
        public Sequence(string name, params Node[] children) : base(name, children) { }

        public override NodeStatus Tick()
        {
            foreach (Node child in Children)
            {
                NodeStatus status = child.Tick();
                if (status != NodeStatus.Success) return status;
            }
            return NodeStatus.Success;
        }
    }

    /// <summary>Leaf that checks a question: Success when true, Failure when false.</summary>
    public sealed class ConditionNode : Node
    {
        private readonly Func<bool> predicate;

        public ConditionNode(string name, Func<bool> predicate) : base(name) => this.predicate = predicate;

        public override NodeStatus Tick() => predicate() ? NodeStatus.Success : NodeStatus.Failure;
    }

    /// <summary>Leaf that does something and reports how it went.</summary>
    public sealed class ActionNode : Node
    {
        private readonly Func<NodeStatus> action;

        public ActionNode(string name, Func<NodeStatus> action) : base(name) => this.action = action;

        public override NodeStatus Tick() => action();
    }

    public sealed class BehaviorTree
    {
        private readonly Node root;

        public BehaviorTree(Node root) => this.root = root;

        public NodeStatus Tick() => root.Tick();
    }
}
