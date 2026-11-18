using LittleDriver.AI.Tasks;
using LittleDriver.Traffic;
using UnityEngine;

namespace LittleDriver.AI.Traffic
{
    /// <summary>
    /// Body of a traffic car. Its decisions are taken by the Behavior Designer tree "AI Car":
    /// stop for the light, let pedestrians cross, keep distance, give way at a busy junction, or follow the lane.
    /// This component only senses (via <see cref="TrafficSensor"/>) and offers the conditions and actions the
    /// tree's tasks call. <see cref="LaneFollower"/> steers the NavMeshAgent; <see cref="AICarMotor"/> applies the speed.
    /// </summary>
    [RequireComponent(typeof(Rigidbody), typeof(LaneFollower), typeof(AICarMotor))]
    [RequireComponent(typeof(TrafficSensor))]
    public sealed class AITrafficCar : MonoBehaviour, IVehicle, INpcBody
    {
        [Tooltip("Gap (m) kept between the front bumper and a stop line.")]
        [SerializeField, Min(0f)] private float stopLineGap = 1f;
        [Tooltip("Gap (m) kept to the car or obstacle ahead.")]
        [SerializeField, Min(0f)] private float followGap = 3f;
        [Tooltip("Closer than this to a stop line (m), the car checks that the junction is free before entering.")]
        [SerializeField, Min(0f)] private float giveWayDistance = 10f;
        [Tooltip("Read-only: the action the behaviour tree is running.")]
        [SerializeField] private string currentAction;

        private Rigidbody body;
        private LaneFollower lane;
        private AICarMotor motor;
        private TrafficSensor sensor;
        private Intersection reservedJunction;

        private void Awake()
        {
            body = GetComponent<Rigidbody>();
            body.isKinematic = true;
            lane = GetComponent<LaneFollower>();
            motor = GetComponent<AICarMotor>();
            sensor = GetComponent<TrafficSensor>();
        }

        // Senses every frame; the behaviour tree reads the results.
        private void Update() => sensor.Sense(motor.Speed);

        public Vector3 Position => transform.position;
        public Vector3 Forward => transform.forward;
        public float Speed => motor.Speed;

        string INpcBody.CurrentAction { set => currentAction = value; }

        private void OnEnable() => Vehicles.Register(this);

        private void OnDisable()
        {
            Vehicles.Unregister(this);
            ReleaseJunction();
        }

        // Conditions (asked by the tree)

        public bool LightSaysStop()
        {
            TrafficLight light = sensor.NextStopLine != null ? sensor.NextStopLine.Light : null;
            if (light == null) return false;
            switch (light.CurrentState)
            {
                case TrafficLightState.Red: return true;
                // On yellow, stop only if it can be done comfortably; otherwise clear the junction.
                case TrafficLightState.Yellow: return sensor.StopLineDistance - stopLineGap >= motor.StoppingDistance * 0.8f;
                default: return false;
            }
        }

        public bool CrossingBusy()
        {
            Crosswalk crosswalk = sensor.NextStopLine != null ? sensor.NextStopLine.Crosswalk : null;
            return crosswalk != null && crosswalk.IsOccupied;
        }

        public bool ObstacleAhead() => sensor.HasObstacle;

        public bool JunctionBusy()
        {
            Intersection junction = UpcomingJunction();
            return junction != null && junction.IsBusyFor(body);
        }

        // Actions (one frame each, run by the tree)

        public void StopAtLine()
        {
            ReleaseJunction();
            BrakeWithin(sensor.StopLineDistance - stopLineGap);
        }

        public void BrakeForObstacle() => BrakeWithin(sensor.ObstacleDistance - followGap);

        public void FollowLane()
        {
            Intersection junction = UpcomingJunction();
            if (junction != null && junction.TryReserve(body)) reservedJunction = junction;
            motor.Cruise(lane.AllowedSpeed(motor.Braking));
        }

        // Helpers

        private void BrakeWithin(float gap)
        {
            if (sensor.HasObstacle) gap = Mathf.Min(gap, sensor.ObstacleDistance - followGap);
            motor.StopWithin(gap, lane.AllowedSpeed(motor.Braking));
        }

        private Intersection UpcomingJunction()
        {
            StopLine line = sensor.NextStopLine;
            return line != null && sensor.StopLineDistance <= giveWayDistance ? line.Intersection : null;
        }

        private void ReleaseJunction()
        {
            if (reservedJunction != null) reservedJunction.CancelReservation(body);
            reservedJunction = null;
        }
    }
}
