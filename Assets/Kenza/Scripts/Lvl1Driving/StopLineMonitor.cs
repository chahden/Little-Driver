using System;
using LittleDriver.Traffic;
using UnityEngine;

namespace LittleDriver.Driving
{
    /// <summary>
    /// Reports when the player's car drives over a stop line while its light is red,
    /// or onto a zebra crossing while someone is on it.
    /// Also reports good behaviour: the car stopped before a red light and only drove on once it turned green.
    /// </summary>
    [RequireComponent(typeof(Lvl1Car))]
    public sealed class StopLineMonitor : MonoBehaviour
    {
        [Header("Stopping at a red light")]
        [Tooltip("Below this speed (m/s) the car counts as stopped.")]
        [SerializeField, Min(0f)] private float stoppedSpeed = 0.5f;
        [Tooltip("The car must stop at most this far (m) before the stop line for the stop to count.")]
        [SerializeField, Min(1f)] private float waitingRange = 12f;
        [Tooltip("Distance (m) from the car's pivot to its front bumper.")]
        [SerializeField, Min(0f)] private float frontOffset = 1.7f;

        private Lvl1Car car;
        /// <summary>The red-light stop line the car is waiting at, if any.</summary>
        private StopLine waitedAtRed;

        public event Action RanRedLight;
        public event Action DroveOntoBusyCrossing;
        /// <summary>The car waited at a red light and drove off once it was green.</summary>
        public event Action StoppedAtRedThenWentOnGreen;

        private Vector3 FrontPoint => transform.position + transform.forward * frontOffset;

        private void Awake() => car = GetComponent<Lvl1Car>();
        private void OnEnable() => car.Restarted += ForgetWait;
        private void OnDisable() => car.Restarted -= ForgetWait;

        private void Update()
        {
            if (!car.CanDrive || car.Speed > stoppedSpeed) return;
            StopLine line = NextLightLineAhead();
            if (line != null && line.Light.CurrentState == TrafficLightState.Red) waitedAtRed = line;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!car.CanDrive || !other.TryGetComponent(out StopLine line)) return;
            if (Vector3.Dot(transform.forward, line.Forward) < 0.5f) return; // crossing it sideways or the wrong way

            if (line.Light != null)
            {
                TrafficLightState state = line.Light.CurrentState;
                if (state == TrafficLightState.Red) RanRedLight?.Invoke();
                else if (state == TrafficLightState.Green && line == waitedAtRed) StoppedAtRedThenWentOnGreen?.Invoke();
                if (line == waitedAtRed) waitedAtRed = null;
            }

            if (line.Crosswalk != null && line.Crosswalk.IsOccupied) DroveOntoBusyCrossing?.Invoke();
        }

        // Closest stop line with a traffic light in the car's lane, in front of the bumper and within waiting range.
        private StopLine NextLightLineAhead()
        {
            StopLine closest = null;
            float closestDistance = waitingRange;
            Vector3 front = FrontPoint;
            foreach (StopLine line in StopLine.Active)
            {
                if (line.Light == null || !line.TryGetDistanceAhead(front, transform.forward, out float distance)) continue;
                if (distance < 0f || distance > closestDistance) continue;
                closest = line;
                closestDistance = distance;
            }
            return closest;
        }

        private void ForgetWait() => waitedAtRed = null;
    }
}
