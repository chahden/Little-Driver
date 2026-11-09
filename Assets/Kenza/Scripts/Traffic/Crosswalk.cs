using System.Collections.Generic;
using UnityEngine;

namespace LittleDriver.Traffic
{
    /// <summary>
    /// Zebra crossing without lights: pedestrians have priority. The transform sits on the road centre,
    /// forward = walking direction from side A to side B. Tracks who is on it and whether it is safe to step out.
    /// </summary>
    public sealed class Crosswalk : MonoBehaviour
    {
        [Tooltip("Half the road width (m): where pedestrians step off the kerb.")]
        [SerializeField, Min(0.5f)] private float kerbDistance = 7f;
        [Tooltip("Half the crossing's width along the road (m).")]
        [SerializeField, Min(0.5f)] private float halfDepth = 2.85f;
        [Tooltip("Deceleration (m/s²) assumed when judging whether a car can still stop.")]
        [SerializeField, Min(0.5f)] private float assumedBraking = 4f;
        [Tooltip("Extra distance (m) a car must have left before a pedestrian steps out.")]
        [SerializeField, Min(0f)] private float safetyMargin = 4f;

        private readonly HashSet<Object> walkers = new HashSet<Object>();

        public bool IsOccupied
        {
            get
            {
                walkers.RemoveWhere(w => w == null);
                return walkers.Count > 0;
            }
        }

        public void Enter(Object walker) => walkers.Add(walker);
        public void Leave(Object walker) => walkers.Remove(walker);

        /// <summary>Kerb-to-kerb points for a walker starting on side A (or B).</summary>
        public Vector3[] Route(bool fromSideA)
        {
            Vector3 a = transform.position - transform.forward * kerbDistance;
            Vector3 b = transform.position + transform.forward * kerbDistance;
            return fromSideA ? new[] { a, b } : new[] { b, a };
        }

        /// <summary>True when no car on this road is on the crossing or too close to stop before it.</summary>
        public bool IsClearToCross()
        {
            foreach (IVehicle vehicle in Vehicles.All)
            {
                Vector3 toCrossing = transform.position - vehicle.Position;
                toCrossing.y = 0f;
                if (Mathf.Abs(Vector3.Dot(toCrossing, transform.forward)) > kerbDistance + 1f) continue; // another road
                float ahead = Vector3.Dot(toCrossing, vehicle.Forward);
                if (ahead < -(halfDepth + 3f)) continue; // already past
                if (ahead < halfDepth + 3f) return false; // on or right at the crossing
                float stoppingDistance = vehicle.Speed * vehicle.Speed / (2f * assumedBraking);
                if (vehicle.Speed > 0.5f && ahead - halfDepth - 3f < stoppingDistance + safetyMargin) return false;
            }
            return true;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = IsOccupied ? Color.red : Color.white;
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(Vector3.up * 0.1f, new Vector3(halfDepth * 2f, 0.2f, kerbDistance * 2f));
        }
    }
}
