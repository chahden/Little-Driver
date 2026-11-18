using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

namespace LittleDriver.AI.Tasks
{
    /// <summary>Anything a Behavior Designer tree drives: AI cars, pedestrians and traffic lights.</summary>
    public interface INpcBody
    {
        /// <summary>Shown read-only in the Inspector ("Current Action"), so the running branch can be watched.</summary>
        string CurrentAction { set; }
    }

    /// <summary>
    /// The one central function every NPC action goes through (US 5.3): it records which action runs,
    /// then performs it. Add logging, pausing or debugging here and it applies to every NPC.
    /// </summary>
    public static class NpcActions
    {
        public static TaskStatus Run(INpcBody body, string action, System.Func<TaskStatus> perform)
        {
            if (body != null) body.CurrentAction = action;
            return perform();
        }
    }

    /// <summary>Base of the Little Driver action nodes: finds its body and runs through <see cref="NpcActions.Run"/>.</summary>
    public abstract class NpcAction<TBody> : Action where TBody : Component, INpcBody
    {
        protected TBody Body { get; private set; }

        public override void OnAwake() => Body = GetComponent<TBody>();

        public override TaskStatus OnUpdate() =>
            Body == null ? TaskStatus.Failure : NpcActions.Run(Body, FriendlyName, Perform);

        /// <summary>One frame of the action.</summary>
        protected abstract TaskStatus Perform();
    }

    /// <summary>Base of the Little Driver condition nodes: a yes/no question about its body.</summary>
    public abstract class NpcCondition<TBody> : Conditional where TBody : Component
    {
        protected TBody Body { get; private set; }

        public override void OnAwake() => Body = GetComponent<TBody>();

        public override TaskStatus OnUpdate() =>
            Body != null && Check() ? TaskStatus.Success : TaskStatus.Failure;

        protected abstract bool Check();
    }
}
