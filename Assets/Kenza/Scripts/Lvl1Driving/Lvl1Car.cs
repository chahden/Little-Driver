using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace LittleDriver.Kenza
{
    // Kenza's copy of SprintPrototypeCar for "Scene lvl1 Setup 1".
    // Instead of two fixed lanes, the car is steered with the on-screen steering wheel
    // and drives in the direction it is facing, so it works on roads that turn.
    [RequireComponent(typeof(Rigidbody), typeof(BoxCollider), typeof(AudioSource))]
    public sealed class Lvl1Car : MonoBehaviour
    {
        [Header("Driving")]
        [SerializeField] private float maximumSpeed = 10f;
        [SerializeField] private float acceleration = 4f;
        [SerializeField] private float coastingDeceleration = 2f;
        [SerializeField] private float brakingDeceleration = 18f;

        [Header("Steering wheel")]
        [SerializeField] private Lvl1SteeringWheel steeringWheel;
        [Tooltip("Degrees per second the car turns with the wheel fully turned.")]
        [SerializeField] private float maxTurnSpeed = 60f;
        [Tooltip("Below this speed (m/s) the car turns less, so it can't spin on the spot.")]
        [SerializeField, Min(0.1f)] private float speedForFullTurn = 3f;
        [Tooltip("Tiny wheel movements smaller than this are ignored.")]
        [SerializeField, Range(0f, 0.3f)] private float steeringDeadZone = 0.05f;

        [Header("Finish")]
        [Tooltip("A trigger box at the end of the road. Driving into it completes the course.")]
        [SerializeField] private Collider finishLine;

        [Header("Camera (follows behind the car)")]
        [SerializeField] private Camera followCamera;
        [Tooltip("Where the camera sits, measured from the car: x = right, y = up, z = forward (negative = behind).")]
        [SerializeField] private Vector3 cameraOffset = new Vector3(0, 6, -10);
        [SerializeField] private float cameraLookAhead = 5f;
        [SerializeField, Min(0.1f)] private float cameraSmoothing = 5f;

        [Header("Horn manners")]
        [SerializeField] private AudioClip hornSound;
        [Tooltip("Tunisian Arabic: بالشويّة على الزمّارة! نستعملوها كان وقت الحاجة.")]
        [SerializeField] private AudioClip hornWarningVoice;
        [SerializeField, Range(0f, 1f)] private float hornWarningVolume = 1f;
        [SerializeField, Min(0.1f)] private float hornPressWindow = 2f;
        [SerializeField, Min(1)] private int hornPressLimit = 3;
        [SerializeField, Min(0.1f)] private float maximumHornHold = 2f;
        [SerializeField, Min(0.1f)] private float hornCooldown = 3f;

        [Header("Gentle collision recovery")]
        [SerializeField] private AudioClip collisionSound;
        [SerializeField] private ParticleSystem collisionEffect;
        [SerializeField, Min(0.1f)] private float recoveryDistance = 2f;
        [SerializeField, Min(0.1f)] private float recoveryDuration = 0.65f;

        public bool IsBuckled { get; private set; }
        public bool IsFinished { get; private set; }
        public int CollisionCount { get; private set; }
        public int HornCount { get; private set; }
        public string LastContact { get; private set; } = "None";
        public bool IsHonking => hornSource != null && hornSource.isPlaying;
        public bool IsHornBlocked => Time.unscaledTime < hornBlockedUntil || (voiceSource != null && voiceSource.isPlaying);
        public bool CanHorn => IsBuckled && !IsFinished && !IsRecovering && !IsHornBlocked && !hornNeedsRelease;
        public bool IsRecovering { get; private set; }
        public float CollisionUntil { get; private set; }
        // Speed in the direction the car is facing (not world Z), so it is right after turning.
        public float Speed => Mathf.Max(0, Vector3.Dot(body.linearVelocity, transform.forward));
        public float MaximumSpeedKmh
        {
            get => maximumSpeed * 3.6f;
            set => maximumSpeed = Mathf.Max(1, value) / 3.6f;
        }
        public event Action Restarted;


        private Rigidbody body;
        private AudioSource hornSource;
        private AudioClip hornClip;
        private AudioClip bumpClip;
        private AudioSource voiceSource;
        private AudioSource effectsSource;
        private readonly Queue<float> hornPresses = new Queue<float>();
        private float hornStartedAt;
        private float hornBlockedUntil;
        private bool hornWasHeld;
        private bool hornNeedsRelease;
        private Vector3 recoveryStart;
        private Vector3 recoveryBackward;
        private float recoveryElapsed;
        private Vector3 startPosition;
        private Quaternion startRotation;
        private float requestedSpeed;
        private bool goHeld;
        private bool brakeHeld;
        private bool hornHeld;

        private void Awake()
        {
            body = GetComponent<Rigidbody>();
            hornSource = GetComponent<AudioSource>();
            startPosition = body.position;
            startRotation = body.rotation;
            // Temporary synthesized sound; replace with a licensed horn recording for final art.
            const int sampleRate = 22050;
            var samples = new float[(int)(sampleRate * 0.22f)];
            for (int i = 0; i < samples.Length; i++)
            {
                float t = (float)i / sampleRate;
                float envelope = Mathf.Min(1, t / 0.015f) * Mathf.Min(1, (0.22f - t) / 0.06f);
                samples[i] = (Mathf.Sin(t * 330 * 2 * Mathf.PI) + Mathf.Sin(t * 440 * 2 * Mathf.PI)) * 0.12f * envelope;
            }
            if (hornSound == null)
            {
                hornClip = AudioClip.Create("Prototype gentle horn", samples.Length, 1, sampleRate, false);
                hornClip.SetData(samples, 0);
            }
            hornSource.clip = hornSound != null ? hornSound : hornClip;
            hornSource.loop = true;
            hornSource.playOnAwake = false;
            voiceSource = gameObject.AddComponent<AudioSource>();
            voiceSource.playOnAwake = false;
            effectsSource = gameObject.AddComponent<AudioSource>();
            effectsSource.playOnAwake = false;
            if (collisionSound == null)
            {
                var bumpSamples = new float[(int)(sampleRate * 0.25f)];
                for (int i = 0; i < bumpSamples.Length; i++)
                {
                    float t = (float)i / sampleRate;
                    bumpSamples[i] = Mathf.Sin(2 * Mathf.PI * (520 * t - 360 * t * t)) *
                        Mathf.Sin(Mathf.PI * t / 0.25f) * Mathf.Exp(-12 * t) * 0.25f;
                }
                bumpClip = AudioClip.Create("Prototype soft bump", bumpSamples.Length, 1, sampleRate, false);
                bumpClip.SetData(bumpSamples, 0);
            }
        }

        private void Update()
        {
            var keyboard = Keyboard.current;
            UpdateHorn();
            if (keyboard == null) return;
            if (keyboard.rKey.wasPressedThisFrame) Restart();
        }

        private void FixedUpdate()
        {
            if (IsRecovering)
            {
                recoveryElapsed += Time.fixedDeltaTime;
                float progress = Mathf.Clamp01(recoveryElapsed / recoveryDuration);
                body.MovePosition(recoveryStart + recoveryBackward * (recoveryDistance * Mathf.SmoothStep(0, 1, progress)));
                if (progress >= 1)
                {
                    body.position = recoveryStart + recoveryBackward * recoveryDistance;
                    IsRecovering = false;
                    body.isKinematic = false;
                    body.linearVelocity = Vector3.zero;
                    body.angularVelocity = Vector3.zero;
                }
                return;
            }
            if (!IsBuckled || IsFinished)
            {
                requestedSpeed = 0;
                body.linearVelocity = Vector3.zero;
                body.angularVelocity = Vector3.zero;
                return;
            }

            var keyboard = Keyboard.current;
            bool braking = brakeHeld || (keyboard != null && (keyboard.spaceKey.isPressed || keyboard.downArrowKey.isPressed));
            bool accelerating = goHeld || (keyboard != null && (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed));
            float target = accelerating && !braking ? maximumSpeed : 0;
            float rate = braking ? brakingDeceleration : accelerating ? acceleration : coastingDeceleration;
            requestedSpeed = Mathf.MoveTowards(requestedSpeed, target, rate * Time.fixedDeltaTime);

            // Steering: -1 = full left, +1 = full right. Keyboard is a backup for testing in the Editor.
            float steer = steeringWheel != null ? steeringWheel.steerValue : 0f;
            if (keyboard != null)
            {
                if (keyboard.leftArrowKey.isPressed || keyboard.aKey.isPressed) steer = -1f;
                if (keyboard.rightArrowKey.isPressed || keyboard.dKey.isPressed) steer = 1f;
            }
            if (Mathf.Abs(steer) < steeringDeadZone) steer = 0f;

            // Turn only while moving: slow car = small turn, stopped car = no turn.
            float turnAmount = Mathf.Clamp01(requestedSpeed / speedForFullTurn);
            body.angularVelocity = Vector3.up * (steer * maxTurnSpeed * turnAmount * Mathf.Deg2Rad);

            // Drive where the car is pointing (flat on the ground).
            Vector3 forward = body.rotation * Vector3.forward;
            forward.y = 0f;
            body.linearVelocity = forward.normalized * requestedSpeed;
        }

        private void LateUpdate()
        {
            if (followCamera == null) return;
            Vector3 forward = transform.forward;
            forward.y = 0f;
            if (forward.sqrMagnitude < 0.0001f) return;
            forward.Normalize();
            Vector3 wanted = transform.position + Quaternion.LookRotation(forward) * cameraOffset;
            Transform cam = followCamera.transform;
            cam.position = Vector3.Lerp(cam.position, wanted, 1f - Mathf.Exp(-cameraSmoothing * Time.deltaTime));
            cam.LookAt(transform.position + forward * cameraLookAhead);
        }

        public void SetGoHeld(bool held) => goHeld = held;
        public void SetBrakeHeld(bool held) => brakeHeld = held;
        public void Buckle() => IsBuckled = true;

        public void SetHornHeld(bool held)
        {
            hornHeld = held;
            UpdateHorn();
        }

        private void UpdateHorn()
        {
            if (hornSource == null) return;
            bool keyboardHeld = Keyboard.current != null && Keyboard.current.hKey.isPressed;
            bool held = hornHeld || keyboardHeld;
            if (!held) hornNeedsRelease = false;
            bool pressed = held && !hornWasHeld;
            hornWasHeld = held;
            if (!held || !CanHorn)
            {
                hornSource.Stop();
                return;
            }
            float now = Time.unscaledTime;
            if (pressed)
            {
                while (hornPresses.Count > 0 && now - hornPresses.Peek() > hornPressWindow) hornPresses.Dequeue();
                hornPresses.Enqueue(now);
                HornCount++;
                if (hornPresses.Count >= hornPressLimit)
                {
                    BlockHorn();
                    return;
                }
                hornStartedAt = now;
                hornSource.Play();
            }
            if (hornSource.isPlaying && now - hornStartedAt >= maximumHornHold) BlockHorn();
        }

        private void BlockHorn()
        {
            hornSource.Stop();
            hornNeedsRelease = true;
            hornBlockedUntil = Time.unscaledTime + hornCooldown;
            hornPresses.Clear();
            if (hornWarningVoice != null) voiceSource.PlayOneShot(hornWarningVoice, hornWarningVolume);
            else Debug.LogWarning("Assign Horn Warning Voice: بالشويّة على الزمّارة! نستعملوها كان وقت الحاجة.", this);
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (!IsBuckled || IsFinished || IsRecovering) return;
            // Supporting road contacts are not obstacle impacts.
            bool obstacleContact = false;
            for (int i = 0; i < collision.contactCount; i++)
                if (collision.GetContact(i).normal.y < 0.65f) obstacleContact = true;
            if (!obstacleContact) return;
            CollisionCount++;
            LastContact = collision.gameObject.name;
            CollisionUntil = Time.unscaledTime + 1.5f;
            requestedSpeed = 0;
            body.linearVelocity = Vector3.zero;
            body.angularVelocity = Vector3.zero;
            recoveryStart = body.position;
            // Move back the way the car came, not along world Z.
            Vector3 back = -transform.forward;
            back.y = 0f;
            recoveryBackward = back.normalized;
            recoveryElapsed = 0;
            IsRecovering = true;
            body.isKinematic = true;
            hornSource.Stop();
            hornNeedsRelease = true;
            effectsSource.PlayOneShot(collisionSound != null ? collisionSound : bumpClip);
            if (collisionEffect != null)
            {
                collisionEffect.transform.position = collision.contactCount > 0 ? collision.GetContact(0).point : transform.position;
                collisionEffect.Play();
            }
            Debug.Log($"Player collided with '{LastContact}' (collision #{CollisionCount}).", this);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other != finishLine || !IsBuckled || IsFinished) return;
            IsFinished = true;
            ReleaseControls();
            body.linearVelocity = Vector3.zero;
            body.angularVelocity = Vector3.zero;
        }

        public void ReleaseControls()
        {
            goHeld = false;
            brakeHeld = false;
            hornHeld = false;
            hornWasHeld = false;
            hornNeedsRelease = true;
            if (hornSource != null) hornSource.Stop();
        }

        private void OnApplicationFocus(bool focused) { if (!focused) ReleaseControls(); }
        private void OnApplicationPause(bool paused) { if (paused) ReleaseControls(); }
        private void OnDisable() => ReleaseControls();

        public void Restart()
        {
            ReleaseControls();
            IsBuckled = false;
            IsFinished = false;
            IsRecovering = false;
            body.isKinematic = false;
            requestedSpeed = 0;
            body.linearVelocity = Vector3.zero;
            body.angularVelocity = Vector3.zero;
            body.position = startPosition;
            body.rotation = startRotation;
            CollisionCount = HornCount = 0;
            LastContact = "None";
            CollisionUntil = 0;
            hornBlockedUntil = 0;
            hornPresses.Clear();
            hornNeedsRelease = false;
            voiceSource.Stop();
            effectsSource.Stop();
            if (collisionEffect != null) collisionEffect.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            hornSource.Stop();
            Restarted?.Invoke();
        }

        private void OnDestroy()
        {
            if (hornClip != null) Destroy(hornClip);
            if (bumpClip != null) Destroy(bumpClip);
        }
    }
}
