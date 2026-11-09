using System.Collections.Generic;
using UnityEngine;

namespace LittleDriver.Traffic
{
    /// <summary>
    /// Stop line of one approach lane, before a traffic light or a zebra crossing. The transform sits on the line,
    /// facing the driving direction; the trigger box starts at the line and covers the lane, so a car entering it has crossed the line.
    /// </summary>
    [RequireComponent(typeof(BoxCollider))]
    public sealed class StopLine : MonoBehaviour
    {
        private static readonly List<StopLine> active = new List<StopLine>();

        [SerializeField] private TrafficLight trafficLight;
        [SerializeField] private Intersection intersection;
        [SerializeField] private Crosswalk crosswalk;

        private BoxCollider area;

        public static IReadOnlyList<StopLine> Active => active;
        public TrafficLight Light => trafficLight;
        public Intersection Intersection => intersection;
        public Crosswalk Crosswalk => crosswalk;
        public Vector3 Forward => transform.forward;

        private void Awake()
        {
            area = GetComponent<BoxCollider>();
            area.isTrigger = true;
        }

        private void OnEnable() => active.Add(this);
        private void OnDisable() => active.Remove(this);

        /// <summary>
        /// Distance along the lane from <paramref name="point"/> to the line (negative once past it).
        /// False when the point is outside this lane or not heading the same way.
        /// </summary>
        public bool TryGetDistanceAhead(Vector3 point, Vector3 heading, out float distance)
        {
            distance = 0f;
            if (Vector3.Dot(heading, transform.forward) < 0.7f) return false;
            Vector3 local = transform.InverseTransformPoint(point);
            if (Mathf.Abs(local.x - area.center.x) > area.size.x * 0.5f) return false;
            distance = -local.z;
            return true;
        }
    }
}
