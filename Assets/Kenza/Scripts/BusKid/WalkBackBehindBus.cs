using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

namespace LittleDriver.BusKid
{
    [TaskCategory("Little Driver/Bus Kid")]
    [TaskDescription("Walks back across the road to hide behind the bus again, so the kid can dart out on the next pass.")]
    public sealed class WalkBackBehindBus : Action
    {
        public const float StartingPriority = 0.5f;
        public const float MaximumBonus = 0.5f;

        // urgency means extra waiting (0 = return delay just passed, 1 = waited twice the delay).
        public static float CalculateUtility(float importance, float urgency)
        {
            float bonus = MaximumBonus * Mathf.Clamp01(urgency);
            float priority = StartingPriority + bonus;
            float utility = importance * priority;
            return utility;
        }

        BusKidNPC kid;
        bool running;

        public override void OnAwake() => kid = GetComponent<BusKidNPC>();

        // Allowed on the far side after the return delay, and only when the car is far away.
        bool IsAllowed() =>
            kid != null && kid.HasPath && kid.Location == KidLocation.AtFarSide &&
            kid.SecondsAtFarSide >= kid.returnDelay && kid.CarDistance >= kid.safeReturnDistance;

        float ExtraWaiting() => (kid.SecondsAtFarSide - kid.returnDelay) / Mathf.Max(1, kid.returnDelay);

        public override float GetUtility()
        {
            float utility = 0;
            if (running) utility = BusKidNPC.CommittedUtility;
            else if (IsAllowed()) utility = CalculateUtility(kid.returnImportance, ExtraWaiting());
            if (kid != null) kid.scores.walkBack = utility;
            return utility;
        }

        public override void OnStart()
        {
            running = kid != null && kid.HasPath;
            if (!running) return;
            kid.scores.runningAction = "Walk Back Behind Bus";
            kid.StartPath(false);
            kid.SetPose(KidPose.Walk);
        }

        public override TaskStatus OnUpdate()
        {
            if (!running) return TaskStatus.Failure;
            return kid.FollowPath(false, kid.walkSpeed) ? TaskStatus.Success : TaskStatus.Running;
        }

        public override void OnEnd()
        {
            running = false;
            if (kid != null) kid.SetPose(KidPose.Idle);
        }
    }
}
