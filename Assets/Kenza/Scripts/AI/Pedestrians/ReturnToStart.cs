using BehaviorDesigner.Runtime.Tasks;
using Mathf = UnityEngine.Mathf;

namespace LittleDriver.AI.Pedestrians
{
    [TaskCategory("Little Driver/Road Crosser")]
    [TaskDescription("Walks the route back to the start spot, so the scene can replay on the next pass.\n" +
                     "Utility = importance × (0.5 + 0.5 × extra waiting), only when the car is far away.")]
    public sealed class ReturnToStart : RoadCrosserTask
    {
        [Tooltip("Seconds to wait at the end before going back.")]
        public float returnDelay = 6f;
        [Tooltip("Only goes back when the player's car is at least this far away (m).")]
        public float safeReturnDistance = 30f;
        public float speed = 1.3f;
        public float importance = 0.6f;

        protected override CrosserAction Kind => CrosserAction.Return;
        protected override CrosserPose Pose => CrosserPose.Walk;
        protected override bool InProgress => Crosser.Location == CrosserLocation.OnRoad && !Crosser.HeadingToEnd;

        protected override float Evaluate()
        {
            if (Crosser.Location != CrosserLocation.AtEnd || Crosser.SecondsAtEnd < returnDelay || Crosser.CarDistance < safeReturnDistance) return 0f;
            float extraWaiting = (Crosser.SecondsAtEnd - returnDelay) / Mathf.Max(1f, returnDelay);
            return Utility(importance, 0.5f, 0.5f, extraWaiting);
        }

        protected override void Begin() => Crosser.StartPath(false);

        protected override bool Step() => Crosser.FollowPath(false, speed);
    }
}
