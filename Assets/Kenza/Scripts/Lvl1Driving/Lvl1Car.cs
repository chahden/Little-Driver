using System;
using LittleDriver.Traffic;
using UnityEngine;

namespace LittleDriver.Driving
{
    /// <summary>
    /// Moves the player's car in the direction it faces: GO/STOP pedals, steering wheel,
    /// seat-belt and finish state, the short pause after a bump, and restart.
    /// </summary>
    [RequireComponent(typeof(Rigidbody), typeof(Lvl1CarInput))]
    public sealed class Lvl1Car : MonoBehaviour, IVehicle
    {
        [Header("Driving")]
        [SerializeField] private float maximumSpeed = 10f;
        [SerializeField] private float acceleration = 4f;
        [SerializeField] private float coastingDeceleration = 2f;
        [SerializeField] private float brakingDeceleration = 18f;

        [Header("Steering")]
        [Tooltip("Degrees per second the car turns with the wheel fully turned.")]
        [SerializeField] private float maxTurnSpeed = 60f;
        [Tooltip("Below this speed (m/s) the car turns less, so it can't spin on the spot.")]
        [SerializeField, Min(0.1f)] private float speedForFullTurn = 3f;

        [Header("Finish")]
        [Tooltip("A trigger at the end of the course. Driving into it completes the level.")]
        [SerializeField] private Collider finishLine;

        private Rigidbody body;
        private Lvl1CarInput input;
        private Vector3 startPosition;
        private Quaternion startRotation;
        private float requestedSpeed;
        private Vector3 nudgeStart;
        private Vector3 nudgeOffset;
        private float nudgeSeconds;
        private float pauseSeconds;
        private float pauseElapsed;

        public bool IsBuckled { get; private set; }
        public bool IsFinished { get; private set; }
        /// <summary>Set by the game rules, e.g. when no stars are left.</summary>
        public bool IsLocked { get; private set; }
        /// <summary>True during the short stop after a bump.</summary>
        public bool IsPaused { get; private set; }
        public bool CanDrive => IsBuckled && !IsFinished && !IsLocked && !IsPaused;
        /// <summary>Forward speed in m/s.</summary>
        public float Speed => body.isKinematic ? 0f : Mathf.Max(0f, Vector3.Dot(body.linearVelocity, transform.forward));
        public Vector3 Position => transform.position;
        public Vector3 Forward => transform.forward;

        public event Action Restarted;

        private void Awake()
        {
            body = GetComponent<Rigidbody>();
            input = GetComponent<Lvl1CarInput>();
            startPosition = body.position;
            startRotation = body.rotation;
        }

        private void OnEnable() => Vehicles.Register(this);
        private void OnDisable() => Vehicles.Unregister(this);

        private void FixedUpdate()
        {
            if (IsPaused) TickPause();
            else if (!CanDrive) Halt();
            else Drive();
        }

        private void Drive()
        {
            bool braking = input.Brake;
            bool accelerating = input.Accelerate && !braking;
            float target = accelerating ? maximumSpeed : 0f;
            float rate = braking ? brakingDeceleration : accelerating ? acceleration : coastingDeceleration;
            requestedSpeed = Mathf.MoveTowards(requestedSpeed, target, rate * Time.fixedDeltaTime);

            // Turn only while moving: slow car = small turn, stopped car = no turn.
            float turnAmount = Mathf.Clamp01(requestedSpeed / speedForFullTurn);
            body.angularVelocity = Vector3.up * (input.Steer * maxTurnSpeed * turnAmount * Mathf.Deg2Rad);

            Vector3 forward = body.rotation * Vector3.forward;
            forward.y = 0f;
            body.linearVelocity = forward.normalized * requestedSpeed;
        }

        public void Buckle() => IsBuckled = true;

        public void SetLocked(bool locked)
        {
            IsLocked = locked;
            if (locked) Halt();
        }

        /// <summary>Stops the car, eases it by <paramref name="nudge"/> and gives control back after <paramref name="seconds"/>.</summary>
        public void PauseAfterBump(Vector3 nudge, float nudgeDuration, float seconds)
        {
            Halt();
            IsPaused = true;
            body.isKinematic = true;
            nudgeStart = body.position;
            nudgeOffset = nudge;
            nudgeSeconds = Mathf.Max(0.01f, nudgeDuration);
            pauseSeconds = seconds;
            pauseElapsed = 0f;
        }

        private void TickPause()
        {
            pauseElapsed += Time.fixedDeltaTime;
            float progress = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(pauseElapsed / nudgeSeconds));
            body.MovePosition(nudgeStart + nudgeOffset * progress);
            if (pauseElapsed < pauseSeconds) return;
            IsPaused = false;
            body.isKinematic = false;
            Halt();
        }

        private void Halt()
        {
            requestedSpeed = 0f;
            if (body.isKinematic) return;
            body.linearVelocity = Vector3.zero;
            body.angularVelocity = Vector3.zero;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other != finishLine || !CanDrive) return;
            IsFinished = true;
            Halt();
        }

        public void Restart()
        {
            IsBuckled = IsFinished = IsLocked = IsPaused = false;
            body.isKinematic = false;
            Halt();
            body.position = startPosition;
            body.rotation = startRotation;
            transform.SetPositionAndRotation(startPosition, startRotation);
            input.ReleaseAll();
            Restarted?.Invoke();
        }
    }
}
