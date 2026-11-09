using UnityEngine;
using UnityEngine.AI;

namespace LittleDriver.AI.Traffic
{
    /// <summary>Eases the NavMeshAgent's speed toward the speed the Behavior Tree asked for.</summary>
    [DefaultExecutionOrder(10)]
    [RequireComponent(typeof(NavMeshAgent))]
    public sealed class AICarMotor : MonoBehaviour
    {
        [Tooltip("Normal driving speed (m/s). 8 m/s is about 30 km/h.")]
        [SerializeField, Min(0.1f)] private float cruiseSpeed = 8f;
        [SerializeField, Min(0.1f)] private float acceleration = 2.5f;
        [Tooltip("Comfortable deceleration (m/s²) used to plan stops.")]
        [SerializeField, Min(0.1f)] private float braking = 5f;

        private NavMeshAgent agent;
        private float targetSpeed;

        public float Speed => agent.speed;
        public float Braking => braking;
        public float StoppingDistance => Speed * Speed / (2f * braking);

        private void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
            agent.speed = 0f;
            agent.acceleration = 50f; // velocity follows agent.speed closely; this motor does the smoothing
            agent.autoBraking = false;
            agent.stoppingDistance = 0f;
            agent.obstacleAvoidanceType = ObstacleAvoidanceType.NoObstacleAvoidance; // the tree handles obstacles
        }

        public void Cruise(float speedLimit) => targetSpeed = Mathf.Min(cruiseSpeed, speedLimit);

        /// <summary>Slows down so the car comes to rest after <paramref name="distance"/> metres, never above <paramref name="speedLimit"/>.</summary>
        public void StopWithin(float distance, float speedLimit) =>
            targetSpeed = Mathf.Min(Mathf.Min(cruiseSpeed, speedLimit), Mathf.Sqrt(2f * braking * Mathf.Max(0f, distance)));

        private void Update()
        {
            float rate = targetSpeed > agent.speed ? acceleration : braking * 2f;
            agent.speed = Mathf.MoveTowards(agent.speed, targetSpeed, rate * Time.deltaTime);
        }
    }
}
