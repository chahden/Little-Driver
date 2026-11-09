using LittleDriver.AI.BehaviorTrees;
using LittleDriver.Traffic;
using UnityEngine;

namespace LittleDriver.AI.Traffic
{
    /// <summary>
    /// Traffic car brain. Each frame the sensor looks ahead, then the Behavior Tree picks a target speed:
    /// stop for the light, let pedestrians cross, keep distance, give way at a busy junction, or follow the lane.
    /// <see cref="LaneFollower"/> steers the NavMeshAgent; <see cref="AICarMotor"/> applies the speed.
    /// </summary>
    [RequireComponent(typeof(Rigidbody), typeof(LaneFollower), typeof(AICarMotor))]
    [RequireComponent(typeof(TrafficSensor))]
    public sealed class AITrafficCar : MonoBehaviour, IVehicle
    {
        [Tooltip("Gap (m) kept between the front bumper and a stop line.")]
        [SerializeField, Min(0f)] private float stopLineGap = 1f;
        [Tooltip("Gap (m) kept to the car or obstacle ahead.")]
        [SerializeField, Min(0f)] private float followGap = 3f;
        [Tooltip("Closer than this to a stop line (m), the car checks that the junction is free before entering.")]
        [SerializeField, Min(0f)] private float giveWayDistance = 10f;
        [Tooltip("Read-only: the action the tree is running.")]
        [SerializeField] private string currentAction;

        private Rigidbody body;
        private LaneFollower lane;
        private AICarMotor motor;
        private TrafficSensor sensor;
        private BehaviorTree tree;
        private Intersection reservedJunction;

        private void Awake()
        {
            body = GetComponent<Rigidbody>();
            body.isKinematic = true;
            lane = GetComponent<LaneFollower>();
            motor = GetComponent<AICarMotor>();
            sensor = GetComponent<TrafficSensor>();
            tree = new BehaviorTree(new Selector("Drive",
                new Sequence("Obey traffic light",
                    new ConditionNode("Light says stop?", LightSaysStop),
                    Act("Stop at red light", StopAtLine)),
                new Sequence("Let pedestrians cross",
                    new ConditionNode("Zebra crossing busy?", CrossingBusy),
                    Act("Wait at zebra crossing", StopAtLine)),
                new Sequence("Keep distance",
                    new ConditionNode("Obstacle ahead?", () => sensor.HasObstacle),
                    Act("Brake for obstacle", BrakeForObstacle)),
                new Sequence("Give way",
                    new ConditionNode("Junction busy?", JunctionBusy),
                    Act("Give way at junction", StopAtLine)),
                Act("Follow lane", FollowLane)));
        }

        private void Update()
        {
            sensor.Sense(motor.Speed);
            tree.Tick();
        }

        public Vector3 Position => transform.position;
        public Vector3 Forward => transform.forward;
        public float Speed => motor.Speed;

        private void OnEnable() => Vehicles.Register(this);

        private void OnDisable()
        {
            Vehicles.Unregister(this);
            ReleaseJunction();
        }

        private ActionNode Act(string name, System.Func<NodeStatus> action) =>
            new ActionNode(name, () => { currentAction = name; return action(); });

        // Conditions

        private bool LightSaysStop()
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

        private bool CrossingBusy()
        {
            Crosswalk crosswalk = sensor.NextStopLine != null ? sensor.NextStopLine.Crosswalk : null;
            return crosswalk != null && crosswalk.IsOccupied;
        }

        private bool JunctionBusy()
        {
            Intersection junction = UpcomingJunction();
            return junction != null && junction.IsBusyFor(body);
        }

        // Actions

        private NodeStatus StopAtLine()
        {
            ReleaseJunction();
            BrakeWithin(sensor.StopLineDistance - stopLineGap);
            return NodeStatus.Running;
        }

        private NodeStatus BrakeForObstacle()
        {
            BrakeWithin(sensor.ObstacleDistance - followGap);
            return NodeStatus.Running;
        }

        private NodeStatus FollowLane()
        {
            Intersection junction = UpcomingJunction();
            if (junction != null && junction.TryReserve(body)) reservedJunction = junction;
            motor.Cruise(lane.AllowedSpeed(motor.Braking));
            return NodeStatus.Running;
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
