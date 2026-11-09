using System.Collections.Generic;
using UnityEngine;

namespace LittleDriver.AI.Pedestrians
{
    /// <summary>Fills the sidewalks with random copies of the template characters at random places.</summary>
    public sealed class PedestrianSpawner : MonoBehaviour
    {
        [Tooltip("Characters to copy (keep them under an inactive parent).")]
        [SerializeField] private Pedestrian[] templates = new Pedestrian[0];
        [SerializeField] private Transform sidewalkNetwork;
        [SerializeField, Min(1)] private int count = 24;
        [Tooltip("Share of pedestrians placed on sidewalks leading to a zebra crossing, so crossings are busy.")]
        [SerializeField, Range(0f, 1f)] private float nearCrossingShare = 0.35f;
        [SerializeField] private Transform player;
        [SerializeField, Min(0f)] private float minDistanceFromPlayer = 15f;

        private void Start()
        {
            var links = new List<(SidewalkNode a, SidewalkNode b)>();
            var crossingLinks = new List<(SidewalkNode a, SidewalkNode b)>();
            foreach (SidewalkNode node in sidewalkNetwork.GetComponentsInChildren<SidewalkNode>())
                foreach (SidewalkNode other in node.Neighbours)
                {
                    if (other == null || node.GetInstanceID() > other.GetInstanceID()) continue;
                    links.Add((node, other));
                    // Oriented toward the crossing, so these pedestrians walk up to it.
                    if (other.Crosswalk != null) crossingLinks.Add((node, other));
                    else if (node.Crosswalk != null) crossingLinks.Add((other, node));
                }
            if (links.Count == 0 || templates.Length == 0) return;

            for (int spawned = 0, attempts = 0; spawned < count && attempts < count * 20; attempts++)
            {
                bool nearCrossing = crossingLinks.Count > 0 && Random.value < nearCrossingShare;
                var (from, to) = nearCrossing ? crossingLinks[Random.Range(0, crossingLinks.Count)] : links[Random.Range(0, links.Count)];
                if (!nearCrossing && Random.value < 0.5f) (from, to) = (to, from);
                Vector3 position = Vector3.Lerp(from.Position, to.Position, nearCrossing ? Random.Range(0.6f, 0.95f) : Random.value);
                if (player != null && Vector3.Distance(position, player.position) < minDistanceFromPlayer) continue;

                Vector3 heading = to.Position - from.Position;
                heading.y = 0f;
                Pedestrian pedestrian = Instantiate(templates[Random.Range(0, templates.Length)], position,
                    heading.sqrMagnitude > 0.001f ? Quaternion.LookRotation(heading) : Quaternion.identity, transform);
                pedestrian.Begin(from, to);
                spawned++;
            }
        }
    }
}
