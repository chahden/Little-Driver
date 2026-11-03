using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

namespace LittleDriver.BusKid
{
    [TaskCategory("Little Driver/Bus Kid")]
    [TaskDescription("Waves at the player when the car comes by slowly while the kid is safe on the far side.")]
    public sealed class GreetSlowCar : Action
    {
        public const float StartingPriority = 0.6f;
        public const float MaximumBonus = 0.4f;

        // urgency means car closeness (0 = edge of greet distance, 1 = car next to the kid).
        public static float CalculateUtility(float importance, float urgency)
        {
            float bonus = MaximumBonus * Mathf.Clamp01(urgency);
            float priority = StartingPriority + bonus;
            float utility = importance * priority;
            return utility;
        }

        BusKidNPC kid;
        bool running;
        float startedAt;

        public override void OnAwake() => kid = GetComponent<BusKidNPC>();

        // Allowed once per pass, on the far side, when the car is near and driving slowly (or stopped).
        bool IsAllowed() =>
            kid != null && kid.HasCar && kid.Location == KidLocation.AtFarSide && !kid.GreetedThisPass &&
            kid.CarDistance <= kid.greetDistance && kid.CarSpeedKmh <= kid.greetMaxSpeedKmh;

        public override float GetUtility()
        {
            float utility = 0;
            if (running) utility = BusKidNPC.CommittedUtility;
            else if (IsAllowed()) utility = CalculateUtility(kid.greetImportance, kid.CarCloseness(kid.greetDistance));
            if (kid != null) kid.scores.greet = utility;
            return utility;
        }

        public override void OnStart()
        {
            running = kid != null;
            if (!running) return;
            startedAt = Time.time;
            kid.GreetedThisPass = true;
            kid.scores.runningAction = "Greet Slow Car";
            kid.SetPose(KidPose.Greet);
        }

        public override TaskStatus OnUpdate()
        {
            if (!running) return TaskStatus.Failure;
            kid.FaceCar();
            return Time.time - startedAt >= kid.greetDuration ? TaskStatus.Success : TaskStatus.Running;
        }

        public override void OnEnd()
        {
            running = false;
            if (kid != null) kid.SetPose(KidPose.Idle);
        }
    }
}
