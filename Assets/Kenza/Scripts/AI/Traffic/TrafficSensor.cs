using LittleDriver.Traffic;
using UnityEngine;

namespace LittleDriver.AI.Traffic
{
    /// <summary>What an AI car sees ahead: the next stop line in its lane and the nearest obstacle.</summary>
    public sealed class TrafficSensor : MonoBehaviour
    {
        [Tooltip("Stop lines closer than this (m) are noticed.")]
        [SerializeField, Min(1f)] private float stopLineRange = 35f;
        [Tooltip("Obstacle probe length (m) when standing still.")]
        [SerializeField, Min(1f)] private float minimumProbeLength = 6f;
        [Tooltip("Extra probe length per m/s of speed.")]
        [SerializeField, Min(0f)] private float probeSecondsAhead = 1.6f;
        [SerializeField] private Vector3 probeHalfExtents = new Vector3(0.9f, 0.5f, 0.1f);
        [Tooltip("Distance (m) from the car's pivot to its front bumper.")]
        [SerializeField, Min(0f)] private float frontOffset = 1.7f;
        [SerializeField, Min(0f)] private float probeHeight = 0.8f;
        [SerializeField] private LayerMask obstacleLayers;

        public StopLine NextStopLine { get; private set; }
        public float StopLineDistance { get; private set; }
        public bool HasObstacle { get; private set; }
        public float ObstacleDistance { get; private set; }

        private Vector3 FrontPoint => transform.position + transform.forward * frontOffset;

        public void Sense(float speed)
        {
            FindNextStopLine();
            ProbeForObstacle(speed);
        }

        private void FindNextStopLine()
        {
            NextStopLine = null;
            StopLineDistance = float.PositiveInfinity;
            Vector3 front = FrontPoint;
            foreach (StopLine line in StopLine.Active)
            {
                if (!line.TryGetDistanceAhead(front, transform.forward, out float distance)) continue;
                if (distance < 0f || distance > stopLineRange || distance >= StopLineDistance) continue;
                NextStopLine = line;
                StopLineDistance = distance;
            }
        }

        private void ProbeForObstacle(float speed)
        {
            Vector3 origin = FrontPoint + Vector3.up * probeHeight;
            float length = minimumProbeLength + speed * probeSecondsAhead;
            HasObstacle = Physics.BoxCast(origin, probeHalfExtents, transform.forward, out RaycastHit hit,
                transform.rotation, length, obstacleLayers, QueryTriggerInteraction.Ignore);
            ObstacleDistance = HasObstacle ? hit.distance : float.PositiveInfinity;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = HasObstacle ? Color.red : Color.green;
            Gizmos.matrix = Matrix4x4.TRS(FrontPoint + Vector3.up * probeHeight, transform.rotation, Vector3.one);
            float length = HasObstacle ? ObstacleDistance : minimumProbeLength;
            Gizmos.DrawWireCube(Vector3.forward * length * 0.5f, new Vector3(probeHalfExtents.x * 2f, probeHalfExtents.y * 2f, length));
        }
    }
}
