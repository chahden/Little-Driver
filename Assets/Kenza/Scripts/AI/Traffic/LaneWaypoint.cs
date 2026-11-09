using System.Collections.Generic;
using UnityEngine;

namespace LittleDriver.AI.Traffic
{
    /// <summary>Point on a designated lane path. Several next points mean a junction where the car picks one at random.</summary>
    public sealed class LaneWaypoint : MonoBehaviour
    {
        [SerializeField] private List<LaneWaypoint> next = new List<LaneWaypoint>();
        [Tooltip("Speed (m/s) a car may have when it reaches this point, e.g. inside a turn. 0 = no limit.")]
        [SerializeField, Min(0f)] private float speedLimit;

        public Vector3 Position => transform.position;
        public float SpeedLimit => speedLimit;
        public IReadOnlyList<LaneWaypoint> Next => next;

        public LaneWaypoint PickNext() => next.Count == 0 ? null : next[Random.Range(0, next.Count)];

        private void OnDrawGizmos()
        {
            Gizmos.color = next.Count > 1 ? Color.magenta : speedLimit > 0f ? Color.yellow : Color.cyan;
            Gizmos.DrawSphere(Position, 0.35f);
            foreach (LaneWaypoint target in next)
                if (target != null) Gizmos.DrawLine(Position, target.Position);
        }
    }
}
