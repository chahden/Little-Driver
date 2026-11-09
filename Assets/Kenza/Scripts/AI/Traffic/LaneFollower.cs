using UnityEngine;
using UnityEngine.AI;

namespace LittleDriver.AI.Traffic
{
    /// <summary>Steers the NavMeshAgent from waypoint to waypoint along the lane paths, picking a random branch at junctions.</summary>
    [RequireComponent(typeof(NavMeshAgent))]
    public sealed class LaneFollower : MonoBehaviour
    {
        [SerializeField] private LaneWaypoint startWaypoint;
        [Tooltip("Head for the next waypoint when this close (m). Larger values give rounder turns.")]
        [SerializeField, Min(0.5f)] private float switchDistance = 3.5f;

        private NavMeshAgent agent;

        public LaneWaypoint Target { get; private set; }

        private void Awake() => agent = GetComponent<NavMeshAgent>();

        /// <summary>Used by the spawner before the car's first frame.</summary>
        public void SetStart(LaneWaypoint waypoint) => startWaypoint = waypoint;

        private void Start()
        {
            if (startWaypoint == null)
            {
                Debug.LogError($"{name}: assign a start waypoint.", this);
                enabled = false;
                return;
            }
            agent.Warp(transform.position); // placed on the lane, at or just after the start waypoint
            SetTarget(startWaypoint.PickNext());
            if (Target != null) transform.rotation = Quaternion.LookRotation(Flat(Target.Position - transform.position));
        }

        private void Update()
        {
            if (Target != null && Flat(Target.Position - transform.position).magnitude <= switchDistance)
                SetTarget(Target.PickNext());
        }

        /// <summary>Highest speed that still lets the car slow to the target waypoint's limit in time.</summary>
        public float AllowedSpeed(float deceleration)
        {
            if (Target == null || Target.SpeedLimit <= 0f) return float.PositiveInfinity;
            float distance = Flat(Target.Position - transform.position).magnitude;
            return Mathf.Sqrt(Target.SpeedLimit * Target.SpeedLimit + 2f * deceleration * distance);
        }

        private void SetTarget(LaneWaypoint waypoint)
        {
            if (waypoint == null) return;
            Target = waypoint;
            agent.SetDestination(waypoint.Position);
        }

        private static Vector3 Flat(Vector3 v) => new Vector3(v.x, 0f, v.z);
    }
}
