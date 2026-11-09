using System;
using LittleDriver.Traffic;
using UnityEngine;

namespace LittleDriver.Driving
{
    /// <summary>
    /// Reports when the player's car drives over a stop line while its light is red,
    /// or onto a zebra crossing while someone is on it.
    /// </summary>
    [RequireComponent(typeof(Lvl1Car))]
    public sealed class StopLineMonitor : MonoBehaviour
    {
        private Lvl1Car car;

        public event Action RanRedLight;
        public event Action DroveOntoBusyCrossing;

        private void Awake() => car = GetComponent<Lvl1Car>();

        private void OnTriggerEnter(Collider other)
        {
            if (!car.CanDrive || !other.TryGetComponent(out StopLine line)) return;
            if (Vector3.Dot(transform.forward, line.Forward) < 0.5f) return; // crossing it sideways or the wrong way
            if (line.Light != null && line.Light.CurrentState == TrafficLightState.Red) RanRedLight?.Invoke();
            if (line.Crosswalk != null && line.Crosswalk.IsOccupied) DroveOntoBusyCrossing?.Invoke();
        }
    }
}
