using System.Collections.Generic;
using LittleDriver.AI.Pedestrians;
using LittleDriver.Driving;
using LittleDriver.Traffic;
using UnityEngine;

namespace LittleDriver.Game
{
    /// <summary>
    /// Earns a star for patience: while someone is crossing (people on a zebra crossing, or the kid, cat or dog),
    /// the player's car must come to a stop nearby with them ahead, and lose no star until they are safely across.
    /// </summary>
    public sealed class CourtesyRewards : MonoBehaviour
    {
        private sealed class Episode
        {
            public bool Waited;
            public bool Spoiled;
        }

        [SerializeField] private Lvl1Car car;
        [SerializeField] private StarScore score;
        [SerializeField] private Crosswalk[] crosswalks = new Crosswalk[0];
        [SerializeField] private RoadCrosser[] crossers = new RoadCrosser[0];
        [Tooltip("Below this speed (m/s) the car counts as stopped.")]
        [SerializeField, Min(0f)] private float stoppedSpeed = 0.5f;
        [Tooltip("The car must stop this close (m) to whoever is crossing.")]
        [SerializeField, Min(1f)] private float waitingDistance = 30f;

        private readonly Dictionary<Component, Episode> episodes = new Dictionary<Component, Episode>();

        private void OnEnable()
        {
            foreach (Crosswalk crosswalk in crosswalks)
            {
                crosswalk.Occupied += Begin;
                crosswalk.Cleared += OnCrossingCleared;
            }
            foreach (RoadCrosser crosser in crossers)
            {
                crosser.CrossingStarted += Begin;
                crosser.ReachedEnd += OnCrosserArrived;
            }
            score.Penalized += SpoilAll;
            car.Restarted += episodes.Clear;
        }

        private void OnDisable()
        {
            foreach (Crosswalk crosswalk in crosswalks)
            {
                crosswalk.Occupied -= Begin;
                crosswalk.Cleared -= OnCrossingCleared;
            }
            foreach (RoadCrosser crosser in crossers)
            {
                crosser.CrossingStarted -= Begin;
                crosser.ReachedEnd -= OnCrosserArrived;
            }
            score.Penalized -= SpoilAll;
            car.Restarted -= episodes.Clear;
        }

        private void Update()
        {
            foreach (var pair in episodes)
                if (!pair.Value.Waited && IsWaitingFor(pair.Key.transform.position)) pair.Value.Waited = true;
        }

        private void Begin(Component walker) => episodes[walker] = new Episode();

        private void OnCrossingCleared(Crosswalk crosswalk) => End(crosswalk, StarReason.WaitedAtCrossing);
        private void OnCrosserArrived(RoadCrosser crosser) => End(crosser, StarReason.WaitedForCrosser);

        private void End(Component walker, StarReason reason)
        {
            if (!episodes.TryGetValue(walker, out Episode episode)) return;
            episodes.Remove(walker);
            if (episode.Waited && !episode.Spoiled) score.Reward(reason);
        }

        private void SpoilAll(StarReason reason)
        {
            foreach (Episode episode in episodes.Values) episode.Spoiled = true;
        }

        // Stopped, close by, with the crossing (or crosser) in front of the car.
        private bool IsWaitingFor(Vector3 target)
        {
            if (!car.CanDrive || car.Speed > stoppedSpeed) return false;
            Vector3 toTarget = target - car.Position;
            toTarget.y = 0f;
            return toTarget.magnitude <= waitingDistance && Vector3.Dot(car.Forward, toTarget.normalized) > 0.3f;
        }
    }
}
