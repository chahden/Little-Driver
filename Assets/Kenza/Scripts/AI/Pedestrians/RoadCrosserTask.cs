using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

namespace LittleDriver.AI.Pedestrians
{
    /// <summary>
    /// Base of the road crossers' Behavior Designer actions. The Utility Selector runs the child whose
    /// GetUtility() is highest; each action scores itself with importance × (base + bonus × urgency).
    /// </summary>
    public abstract class RoadCrosserTask : Action
    {
        /// <summary>An action in progress reports this so nothing (max 1.0) interrupts it halfway across the road.</summary>
        public const float CommittedUtility = 1.1f;

        private bool running;

        protected RoadCrosser Crosser { get; private set; }
        protected abstract CrosserAction Kind { get; }
        protected abstract CrosserPose Pose { get; }
        /// <summary>True for actions any allowed action may replace at once (e.g. idling).</summary>
        protected virtual bool Interruptible => false;
        /// <summary>True while the crosser is in the middle of this action, even if the tree restarted the task.</summary>
        protected virtual bool InProgress => false;

        public override void OnAwake() => Crosser = GetComponent<RoadCrosser>();

        public override float GetUtility()
        {
            float utility = !Interruptible && (running || InProgress) ? CommittedUtility : Evaluate();
            Crosser.ShowUtility(Kind, utility);
            return utility;
        }

        // Behavior Designer may (re)start a child that is not the best choice, e.g. the first child when the
        // selector restarts. An action that is neither allowed nor in progress fails at once, and the selector
        // picks the next best; one already in progress just carries on.
        public override void OnStart()
        {
            bool resuming = InProgress;
            running = resuming || Evaluate() > 0f;
            if (!running) return;
            Crosser.ShowRunning(Kind);
            if (!resuming) Begin();
            Crosser.SetPose(Pose);
        }

        public override TaskStatus OnUpdate()
        {
            if (!running) return TaskStatus.Failure;
            return Step() ? TaskStatus.Success : TaskStatus.Running;
        }

        public override void OnEnd()
        {
            if (running) Crosser.SetPose(CrosserPose.Idle);
            running = false;
        }

        /// <summary>0 when the action is not allowed right now.</summary>
        protected abstract float Evaluate();
        protected abstract void Begin();
        /// <summary>One frame of the action; true when finished.</summary>
        protected abstract bool Step();

        protected static float Utility(float importance, float basePriority, float bonus, float urgency) =>
            importance * (basePriority + bonus * Mathf.Clamp01(urgency));
    }
}
