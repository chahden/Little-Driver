using System.Collections.Generic;
using UnityEngine;

namespace LittleDriver.Traffic
{
    /// <summary>
    /// Junction box between the stop lines. Tracks the vehicles inside it and short reservations
    /// made just before entering, so only one AI car crosses at a time ("yield to the car in the junction").
    /// </summary>
    [RequireComponent(typeof(BoxCollider))]
    public sealed class Intersection : MonoBehaviour
    {
        private readonly HashSet<Rigidbody> occupants = new HashSet<Rigidbody>();
        private readonly HashSet<Rigidbody> reservations = new HashSet<Rigidbody>();

        private void Awake() => GetComponent<BoxCollider>().isTrigger = true;

        /// <summary>True when another vehicle is inside or about to enter.</summary>
        public bool IsBusyFor(Rigidbody vehicle)
        {
            occupants.RemoveWhere(IsGone);
            reservations.RemoveWhere(IsGone);
            return ContainsOther(occupants, vehicle) || ContainsOther(reservations, vehicle);
        }

        public bool TryReserve(Rigidbody vehicle)
        {
            if (IsBusyFor(vehicle)) return false;
            reservations.Add(vehicle);
            return true;
        }

        public void CancelReservation(Rigidbody vehicle) => reservations.Remove(vehicle);

        private void OnTriggerEnter(Collider other)
        {
            if (other.attachedRigidbody != null) occupants.Add(other.attachedRigidbody);
        }

        private void OnTriggerExit(Collider other)
        {
            Rigidbody vehicle = other.attachedRigidbody;
            if (vehicle == null) return;
            occupants.Remove(vehicle);
            reservations.Remove(vehicle);
        }

        private static bool IsGone(Rigidbody vehicle) => vehicle == null || !vehicle.gameObject.activeInHierarchy;

        private static bool ContainsOther(HashSet<Rigidbody> set, Rigidbody vehicle)
        {
            foreach (Rigidbody other in set)
                if (other != vehicle) return true;
            return false;
        }
    }
}
