using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

namespace LittleDriver.BusKid
{
    [TaskCategory("Little Driver/Bus Kid")]
    [TaskDescription("Runs out from behind the bus across the road when a car is coming.")]
    public sealed class DartOutFromBus : Action
    {
        public const float StartingPriority = 0.6f;
        public const float MaximumBonus = 0.4f;

        // urgency means car closeness (0 = edge of dart distance, 1 = car next to the kid).
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

        // Allowed only while hiding behind the bus and a car is driving toward the kid, inside dart distance.
        bool IsAllowed() =>
            kid != null && kid.HasPath && kid.Location == KidLocation.AtHideSpot &&
            kid.CarApproaching && kid.CarDistance <= kid.dartDistance;

        public override float GetUtility()
        {
            float utility = 0;
            if (running) utility = BusKidNPC.CommittedUtility;
            else if (IsAllowed()) utility = CalculateUtility(kid.dartImportance, kid.CarCloseness(kid.dartDistance));
            if (kid != null) kid.scores.dartOut = utility;
            return utility;
        }

        public override void OnStart()
        {
            running = kid != null && kid.HasPath;
            if (!running) return;
            kid.scores.runningAction = "Dart Out From Bus";
            kid.StartPath(true);
            kid.SetPose(KidPose.Run);
        }

        public override TaskStatus OnUpdate()
        {
            if (!running) return TaskStatus.Failure;
            return kid.FollowPath(true, kid.runSpeed) ? TaskStatus.Success : TaskStatus.Running;
        }

        public override void OnEnd()
        {
            running = false;
            if (kid != null) kid.SetPose(KidPose.Idle);
        }
    }
}
