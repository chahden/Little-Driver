using BehaviorDesigner.Runtime.Tasks;

namespace LittleDriver.AI.Pedestrians
{
    [TaskCategory("Little Driver/Road Crosser")]
    [TaskDescription("Fallback: stand still and watch the road. Any allowed action beats its tiny utility.")]
    public sealed class StandIdle : RoadCrosserTask
    {
        public float idleUtility = 0.001f;

        protected override CrosserAction Kind => CrosserAction.Idle;
        protected override CrosserPose Pose => CrosserPose.Idle;
        protected override bool Interruptible => true;

        protected override float Evaluate() => idleUtility;

        protected override void Begin() { }

        protected override bool Step()
        {
            Crosser.LookAtRoad();
            return false;
        }
    }
}
