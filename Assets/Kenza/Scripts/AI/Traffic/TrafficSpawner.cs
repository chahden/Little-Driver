using System.Collections.Generic;
using UnityEngine;

namespace LittleDriver.AI.Traffic
{
    /// <summary>Adds copies of the traffic cars on straight lane stretches until the road holds <see cref="count"/> cars.</summary>
    public sealed class TrafficSpawner : MonoBehaviour
    {
        [SerializeField] private AITrafficCar[] templates = new AITrafficCar[0];
        [SerializeField] private Transform lanes;
        [SerializeField, Min(1)] private int count = 12;
        [SerializeField] private Transform player;
        [SerializeField, Min(0f)] private float minDistanceFromPlayer = 40f;
        [Tooltip("Minimum distance (m) between two cars at spawn.")]
        [SerializeField, Min(1f)] private float minGap = 18f;
        [Tooltip("Only straight lane stretches at least this long (m) are used.")]
        [SerializeField, Min(1f)] private float minStretch = 20f;

        private void Start()
        {
            var taken = new List<Vector3>();
            foreach (AITrafficCar car in FindObjectsByType<AITrafficCar>(FindObjectsSortMode.None)) taken.Add(car.transform.position);
            if (player != null) taken.Add(player.position);

            var stretches = new List<(LaneWaypoint from, LaneWaypoint to)>();
            foreach (LaneWaypoint waypoint in lanes.GetComponentsInChildren<LaneWaypoint>())
            {
                LaneWaypoint next = waypoint.Next.Count == 1 ? waypoint.Next[0] : null;
                if (next != null && waypoint.SpeedLimit <= 0f && next.SpeedLimit <= 0f && Vector3.Distance(waypoint.Position, next.Position) >= minStretch)
                    stretches.Add((waypoint, next));
            }
            if (stretches.Count == 0 || templates.Length == 0) return;

            for (int attempts = 0; taken.Count - (player != null ? 1 : 0) < count && attempts < count * 30; attempts++)
            {
                var (from, to) = stretches[Random.Range(0, stretches.Count)];
                Vector3 position = Vector3.Lerp(from.Position, to.Position, Random.Range(0.1f, 0.9f));
                if (player != null && Vector3.Distance(position, player.position) < minDistanceFromPlayer) continue;
                if (taken.Exists(p => Vector3.Distance(p, position) < minGap)) continue;

                AITrafficCar car = Instantiate(templates[Random.Range(0, templates.Length)], position,
                    Quaternion.LookRotation(to.Position - from.Position), transform);
                car.GetComponent<LaneFollower>().SetStart(from);
                taken.Add(position);
            }
        }
    }
}
