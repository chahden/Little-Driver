using System;
using BehaviorDesigner.Runtime;
using LittleDriver.Driving;
using UnityEngine;

namespace LittleDriver.AI.Pedestrians
{
    public enum CrosserLocation { AtStart, OnRoad, AtEnd }
    public enum CrosserPose { Idle, Walk, Run }
    public enum CrosserAction { EnterRoad, Return, Idle }

    /// <summary>How the Animator shows one pose: a bool parameter, an animator state, or both.</summary>
    [Serializable]
    public sealed class PoseAnimation
    {
        [Tooltip("Bool parameter set while in this pose (leave empty if the controller has none).")]
        public string boolParameter;
        [Tooltip("Animator state to cross-fade to (leave empty to rely on the bool parameter).")]
        public string state;
        [Min(0.05f)] public float speed = 1f;
    }

    /// <summary>
    /// Body and senses of someone (child, cat, dog) waiting at the roadside: watches the player's car,
    /// moves along a route onto/across the road and animates. A Behavior Designer tree with a
    /// Utility Selector decides WHAT to do; this component only does it.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public sealed class RoadCrosser : MonoBehaviour
    {
        [Header("Scene references")]
        [SerializeField] private Lvl1Car car;
        [SerializeField] private BehaviorTree behaviorTree;
        [Tooltip("Route: [0] = start spot at the roadside ... last = end spot (far or same sidewalk).")]
        [SerializeField] private Transform[] crossingPath = new Transform[0];
        [Tooltip("Shown to the driver when this crosser goes onto the road.")]
        [SerializeField] private string warningMessage = "Watch out! Someone is crossing!";

        [Header("Movement")]
        [SerializeField, Min(0.1f)] private float turnSpeed = 540f;
        [Tooltip("A car slower than this (km/h) is not 'approaching'.")]
        [SerializeField, Min(0f)] private float minimumCarSpeedKmh = 5f;

        [Header("Animation")]
        [SerializeField] private PoseAnimation idle = new PoseAnimation();
        [SerializeField] private PoseAnimation walk = new PoseAnimation();
        [SerializeField] private PoseAnimation run = new PoseAnimation();

        [Header("Live values (Play mode, read-only)")]
        [SerializeField] private string runningAction = "None";
        [SerializeField] private float enterRoadUtility;
        [SerializeField] private float returnUtility;
        [SerializeField] private float idleUtility;
        [SerializeField] private CrosserLocation location;
        [SerializeField] private float carDistance;
        [SerializeField] private bool carApproaching;

        private Animator animator;
        private Vector3 startPosition;
        private Quaternion startRotation;
        private float arrivedAtEndTime;
        private int nextPoint;
        private int lastStepFrame = -1;
        private CrosserPose pose = (CrosserPose)(-1);

        /// <summary>Raised when the crosser starts going onto the road.</summary>
        public event Action<RoadCrosser> CrossingStarted;

        public string WarningMessage => warningMessage;
        public CrosserLocation Location => location;
        /// <summary>Direction of the current or last trip along the route.</summary>
        public bool HeadingToEnd { get; private set; }
        public float CarDistance => carDistance;
        public bool CarApproaching => carApproaching;
        public float SecondsAtEnd => location == CrosserLocation.AtEnd ? Time.time - arrivedAtEndTime : 0f;

        private void Awake()
        {
            // On this object or a child "Model" (clips that animate their root must not move the crosser itself).
            animator = GetComponentInChildren<Animator>();
            animator.applyRootMotion = false; // the clips run in place; the crosser is moved along the route
            GetComponent<Rigidbody>().isKinematic = true;
            startPosition = transform.position;
            startRotation = transform.rotation;
        }

        private void Start() => SetPose(CrosserPose.Idle);

        private void OnEnable() => car.Restarted += ResetCrosser;
        private void OnDisable() => car.Restarted -= ResetCrosser;

        private void Update() => RefreshSenses();

        private void RefreshSenses()
        {
            Vector3 toCrosser = transform.position - car.transform.position;
            toCrosser.y = 0f;
            carDistance = toCrosser.magnitude;
            carApproaching = car.Speed * 3.6f >= minimumCarSpeedKmh && Vector3.Dot(car.transform.forward, toCrosser) > 0f;
        }

        /// <summary>0 = car at the edge of <paramref name="range"/> or farther, 1 = car right next to the crosser.</summary>
        public float CarCloseness(float range) => Mathf.Clamp01(1f - carDistance / Mathf.Max(0.01f, range));

        public void StartPath(bool towardEnd)
        {
            location = CrosserLocation.OnRoad;
            HeadingToEnd = towardEnd;
            nextPoint = towardEnd ? 1 : crossingPath.Length - 2;
            if (towardEnd) CrossingStarted?.Invoke(this);
        }

        /// <summary>Moves one frame along the route. Returns true when its last point is reached.</summary>
        public bool FollowPath(bool towardEnd, float speed)
        {
            // Behavior Designer's Utility Selector can update the running action twice in one frame; move only once.
            if (lastStepFrame == Time.frameCount) return false;
            lastStepFrame = Time.frameCount;
            Vector3 goal = crossingPath[nextPoint].position;
            TurnTowards(goal);
            transform.position = Vector3.MoveTowards(transform.position, goal, speed * Time.deltaTime);
            if ((transform.position - goal).sqrMagnitude > 0.0001f) return false;

            int lastPoint = towardEnd ? crossingPath.Length - 1 : 0;
            if (nextPoint != lastPoint)
            {
                nextPoint += towardEnd ? 1 : -1;
                return false;
            }
            location = towardEnd ? CrosserLocation.AtEnd : CrosserLocation.AtStart;
            if (towardEnd) arrivedAtEndTime = Time.time;
            return true;
        }

        /// <summary>Waiting crossers face the road.</summary>
        public void LookAtRoad()
        {
            if (location == CrosserLocation.AtStart) TurnTowards(crossingPath[1].position);
            else if (location == CrosserLocation.AtEnd) TurnTowards(crossingPath[crossingPath.Length - 2].position);
        }

        public void SetPose(CrosserPose newPose)
        {
            if (newPose == pose) return;
            pose = newPose;
            SetBool(idle, newPose == CrosserPose.Idle);
            SetBool(walk, newPose == CrosserPose.Walk);
            SetBool(run, newPose == CrosserPose.Run);
            PoseAnimation active = newPose == CrosserPose.Run ? run : newPose == CrosserPose.Walk ? walk : idle;
            if (!string.IsNullOrEmpty(active.state)) animator.CrossFadeInFixedTime(active.state, 0.15f);
            animator.speed = active.speed;
        }

        // Inspector read-outs, so the utility vote can be watched in Play mode.
        public void ShowUtility(CrosserAction action, float utility)
        {
            if (action == CrosserAction.EnterRoad) enterRoadUtility = utility;
            else if (action == CrosserAction.Return) returnUtility = utility;
            else idleUtility = utility;
        }

        public void ShowRunning(CrosserAction action) => runningAction = action.ToString();

        [ContextMenu("Reset Crosser")]
        public void ResetCrosser()
        {
            behaviorTree.DisableBehavior();
            transform.SetPositionAndRotation(startPosition, startRotation);
            location = CrosserLocation.AtStart;
            SetPose(CrosserPose.Idle);
            runningAction = "None";
            RefreshSenses(); // the car was just moved back to the start; don't let the tree act on old readings
            if (isActiveAndEnabled) behaviorTree.EnableBehavior();
        }

        private void SetBool(PoseAnimation animation, bool on)
        {
            if (!string.IsNullOrEmpty(animation.boolParameter)) animator.SetBool(animation.boolParameter, on);
        }

        private void TurnTowards(Vector3 point)
        {
            Vector3 direction = point - transform.position;
            direction.y = 0f;
            if (direction.sqrMagnitude < 0.0001f) return;
            transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(direction), turnSpeed * Time.deltaTime);
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;
            for (int i = 1; i < crossingPath.Length; i++)
                if (crossingPath[i - 1] != null && crossingPath[i] != null)
                    Gizmos.DrawLine(crossingPath[i - 1].position, crossingPath[i].position);
        }
    }
}
