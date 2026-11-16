using BehaviorDesigner.Runtime.Tasks;

namespace LittleDriver.AI.Pedestrians
{
    [TaskCategory("Little Driver/Road Crosser")]
    [TaskDescription("Goes along the route onto the road when the player's car is coming (darts across, or ambles in the lane).\n" +
                     "Utility = importance × (0.6 + 0.4 × closeness), closeness = 1 - distance / triggerDistance.")]
    public sealed class EnterRoad : RoadCrosserTask
    {
        [Tooltip("Starts when the approaching car is closer than this (m).")]
        public float triggerDistance = 25f;
        [Tooltip("Speed along the route (m/s).")]
        public float speed = 3.2f;
        [Tooltip("Run (dart) or walk (amble) along the route.")]
        public bool run = true;
        public float importance = 1f;

        protected override CrosserAction Kind => CrosserAction.EnterRoad;
        protected override CrosserPose Pose => run ? CrosserPose.Run : CrosserPose.Walk;
        protected override bool InProgress => Crosser.Location == CrosserLocation.OnRoad && Crosser.HeadingToEnd;

        // Allowed only while waiting at the start and the car is driving toward the crosser, inside the trigger distance.
        protected override float Evaluate() =>
            Crosser.Location == CrosserLocation.AtStart && Crosser.CarApproaching && Crosser.CarDistance <= triggerDistance
                ? Utility(importance, 0.6f, 0.4f, Crosser.CarCloseness(triggerDistance))
                : 0f;

        protected override void Begin() => Crosser.StartPath(true);

        protected override bool Step()
        {
            // A ball chaser stands and watches the ball for a moment before running after it.
            Crosser.SetPose(Crosser.WaitingForBall ? CrosserPose.Idle : Pose);
            return Crosser.FollowPath(true, speed);
        }
    }
}
