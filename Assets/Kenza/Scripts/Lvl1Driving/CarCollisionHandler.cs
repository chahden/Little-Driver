using System;
using UnityEngine;

namespace LittleDriver.Driving
{
    /// <summary>
    /// Gentle reaction to bumping into something: the car stops, rolls back a little and drives on by itself
    /// after a few seconds. Curb barriers only block the car and are ignored here.
    /// </summary>
    [RequireComponent(typeof(Lvl1Car))]
    public sealed class CarCollisionHandler : MonoBehaviour
    {
        [Tooltip("Layers that only block the car silently, e.g. the curb barriers.")]
        [SerializeField] private LayerMask silentLayers;
        [SerializeField, Min(0f)] private float backOffDistance = 1.5f;
        [SerializeField, Min(0.01f)] private float backOffSeconds = 0.6f;
        [Tooltip("Seconds until the player can drive again.")]
        [SerializeField, Min(0.1f)] private float resumeAfterSeconds = 3f;
        [Tooltip("Touching the same object again this soon after driving on is not a new bump.")]
        [SerializeField, Min(0f)] private float graceSeconds = 2f;

        private Lvl1Car car;
        private Collider lastBumped;
        private float graceUntil;

        /// <summary>Raised with the contact point when a bump counts.</summary>
        public event Action<Vector3> Bumped;

        private void Awake() => car = GetComponent<Lvl1Car>();

        private void OnCollisionEnter(Collision collision)
        {
            if (!car.CanDrive || (silentLayers.value & (1 << collision.gameObject.layer)) != 0) return;
            if (collision.collider == lastBumped && Time.time < graceUntil) return;
            if (!TryGetSideContact(collision, out Vector3 point)) return;

            lastBumped = collision.collider;
            graceUntil = Time.time + resumeAfterSeconds + graceSeconds;
            Vector3 back = -transform.forward;
            back.y = 0f;
            car.PauseAfterBump(back.normalized * backOffDistance, backOffSeconds, resumeAfterSeconds);
            Bumped?.Invoke(point);
        }

        // Contacts from below (the ground) are not bumps.
        private static bool TryGetSideContact(Collision collision, out Vector3 point)
        {
            for (int i = 0; i < collision.contactCount; i++)
            {
                ContactPoint contact = collision.GetContact(i);
                if (contact.normal.y < 0.65f)
                {
                    point = contact.point;
                    return true;
                }
            }
            point = default;
            return false;
        }
    }
}
