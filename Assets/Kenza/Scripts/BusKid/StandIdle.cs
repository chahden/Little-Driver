using BehaviorDesigner.Runtime.Tasks;

namespace LittleDriver.BusKid
{
    [TaskCategory("Little Driver/Bus Kid")]
    [TaskDescription("Fallback: stand still and look at the road. Any allowed action beats its tiny score.")]
    public sealed class StandIdle : Action
    {
        public const float IdleUtility = 0.001f;

        BusKidNPC kid;

        public override void OnAwake() => kid = GetComponent<BusKidNPC>();

        public override float GetUtility()
        {
            if (kid != null) kid.scores.idle = IdleUtility;
            return IdleUtility;
        }

        public override void OnStart()
        {
            if (kid == null) return;
            kid.scores.runningAction = "Stand Idle";
            kid.SetPose(KidPose.Idle);
        }

        public override TaskStatus OnUpdate()
        {
            if (kid != null) kid.LookAtRoad();
            return TaskStatus.Running;
        }
    }
}
