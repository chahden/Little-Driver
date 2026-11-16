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
        [Tooltip("A hit only counts when it is this close (m) to the lane path ahead, so things beside a bend are ignored.")]
        [SerializeField, Min(0.5f)] private float laneHalfWidth = 1.55f;
        [Tooltip("Extra width (m) for moving things, so a car drifting into the lane is still seen.")]
        [SerializeField, Min(0f)] private float vehicleMargin = 1.1f;

        private readonly RaycastHit[] hits = new RaycastHit[16];
        private LaneFollower lane;

        public StopLine NextStopLine { get; private set; }
        public float StopLineDistance { get; private set; }
        public bool HasObstacle { get; private set; }
        public float ObstacleDistance { get; private set; }

        private Vector3 FrontPoint => transform.position + transform.forward * frontOffset;

        private void Awake() => lane = GetComponent<LaneFollower>();

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
            int count = Physics.BoxCastNonAlloc(origin, probeHalfExtents, transform.forward, hits,
                transform.rotation, length, obstacleLayers, QueryTriggerInteraction.Ignore);
            ObstacleDistance = float.PositiveInfinity;
            for (int i = 0; i < count; i++)
            {
                RaycastHit hit = hits[i];
                if (hit.distance >= ObstacleDistance || hit.transform.IsChildOf(transform)) continue;
                // A hit at distance 0 overlapped the probe from the start: right in front, so it always counts.
                // Otherwise it must be on the lane ahead; moving things (cars, crossers) get a wider margin for their body.
                float reach = hit.collider.attachedRigidbody != null ? laneHalfWidth + vehicleMargin : laneHalfWidth;
                if (hit.distance > 0f && !IsOnLaneAhead(hit.point, reach)) continue;
                ObstacleDistance = hit.distance;
            }
            HasObstacle = !float.IsPositiveInfinity(ObstacleDistance);
        }

        // The probe looks straight ahead; in a bend that sweeps over the roadside, so check the hit against the lane itself.
        private bool IsOnLaneAhead(Vector3 point, float reach)
        {
            if (lane == null || lane.Target == null) return true;
            LaneWaypoint target = lane.Target;
            if (IsNearSegment(point, transform.position, target.Position, reach)) return true;
            foreach (LaneWaypoint next in target.Next)
            {
                if (next == null) continue;
                if (IsNearSegment(point, target.Position, next.Position, reach)) return true;
                foreach (LaneWaypoint after in next.Next)
                    if (after != null && IsNearSegment(point, next.Position, after.Position, reach)) return true;
            }
            return false;
        }

        private static bool IsNearSegment(Vector3 point, Vector3 a, Vector3 b, float reach)
        {
            Vector2 p = new Vector2(point.x, point.z), start = new Vector2(a.x, a.z), end = new Vector2(b.x, b.z);
            Vector2 segment = end - start;
            float t = segment.sqrMagnitude > 0.0001f ? Mathf.Clamp01(Vector2.Dot(p - start, segment) / segment.sqrMagnitude) : 0f;
            return (p - (start + segment * t)).sqrMagnitude <= reach * reach;
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
