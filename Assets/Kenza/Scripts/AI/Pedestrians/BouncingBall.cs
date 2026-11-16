using UnityEngine;

namespace LittleDriver.AI.Pedestrians
{
    /// <summary>
    /// A ball that bounces across the road along its own route when its <see cref="RoadCrosser"/> kicks it,
    /// so the child chasing it comes out a moment later. The child dribbles it back to the start afterwards.
    /// Moved by script (kinematic): the player's car can still bump into it.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public sealed class BouncingBall : MonoBehaviour
    {
        private enum BallState { Resting, Rolling, Stopped, Carried, Settling }

        [Header("Route")]
        [Tooltip("[0] = resting spot by the child ... last = where the ball stops. Heights are the ball's centre.")]
        [SerializeField] private Transform[] rollPath = new Transform[0];

        [Header("Roll")]
        [Tooltip("Speed (m/s) when kicked.")]
        [SerializeField, Min(0.1f)] private float startSpeed = 6f;
        [Tooltip("Speed (m/s) when it reaches the end of the route.")]
        [SerializeField, Min(0.1f)] private float endSpeed = 1.5f;
        [Tooltip("Height (m) of the first hop; later hops get lower.")]
        [SerializeField, Min(0f)] private float bounceHeight = 0.7f;
        [Tooltip("Distance (m) covered by one hop.")]
        [SerializeField, Min(0.2f)] private float hopLength = 2.5f;
        [SerializeField, Min(0.01f)] private float radius = 0.33f;

        [Header("Carried back")]
        [Tooltip("How far (m) in front of the child the ball rolls while the child walks it back.")]
        [SerializeField, Min(0f)] private float carryDistance = 0.6f;
        [Tooltip("Seconds to roll back onto its resting spot once the child is home.")]
        [SerializeField, Min(0.05f)] private float settleSeconds = 0.4f;

        [Header("Sound (optional)")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip bounceSound;

        private Rigidbody body;
        private Vector3 homePosition;
        private Quaternion homeRotation;
        private BallState state = BallState.Resting;
        private float travelled;
        private float routeLength;
        private int lastHop;
        private Transform carrier;
        private float carrierHeightOffset;
        private Vector3 settleFrom;
        private float settleElapsed;

        public Vector3 HomePosition => homePosition;
        public bool IsRolling => state == BallState.Rolling;

        private void Awake()
        {
            body = GetComponent<Rigidbody>();
            body.isKinematic = true;
            body.interpolation = RigidbodyInterpolation.Interpolate;
            homePosition = transform.position;
            homeRotation = transform.rotation;
            routeLength = 0f;
            for (int i = 1; i < rollPath.Length; i++) routeLength += Vector3.Distance(rollPath[i - 1].position, rollPath[i].position);
        }

        /// <summary>Sends the ball bouncing along its route.</summary>
        public void Kick()
        {
            if (rollPath.Length < 2)
            {
                Debug.LogWarning($"{name}: the roll path needs at least 2 points.", this);
                return;
            }
            travelled = 0f;
            lastHop = 0;
            state = BallState.Rolling;
        }

        /// <summary>The child walks the ball back: it rolls in front of <paramref name="child"/>.</summary>
        public void CarryBack(Transform child, float heightAboveChild)
        {
            carrier = child;
            carrierHeightOffset = heightAboveChild;
            state = BallState.Carried;
        }

        /// <summary>The child is home: the ball rolls back onto its resting spot.</summary>
        public void Settle()
        {
            settleFrom = transform.position;
            settleElapsed = 0f;
            state = BallState.Settling;
        }

        [ContextMenu("Reset Ball")]
        public void ResetBall()
        {
            state = BallState.Resting;
            carrier = null;
            transform.SetPositionAndRotation(homePosition, homeRotation);
            if (body != null) body.position = homePosition;
        }

        private void Update()
        {
            switch (state)
            {
                case BallState.Rolling: Roll(); break;
                case BallState.Carried: FollowCarrier(); break;
                case BallState.Settling: SettleStep(); break;
            }
        }

        private void Roll()
        {
            float progress = routeLength > 0f ? travelled / routeLength : 1f;
            float speed = Mathf.Lerp(startSpeed, endSpeed, progress);
            travelled = Mathf.Min(routeLength, travelled + speed * Time.deltaTime);
            progress = routeLength > 0f ? travelled / routeLength : 1f;

            Vector3 ground = PointAlongRoute(travelled, out Vector3 direction);
            // Hops shrink as the ball slows down: |sin| arcs whose height fades to zero at the end of the route.
            float hop = Mathf.Abs(Mathf.Sin(Mathf.PI * travelled / hopLength)) * bounceHeight * (1f - progress);
            MoveTo(ground + Vector3.up * hop, direction);

            int hopIndex = Mathf.FloorToInt(travelled / hopLength);
            if (hopIndex != lastHop)
            {
                lastHop = hopIndex;
                if (audioSource != null && bounceSound != null) audioSource.PlayOneShot(bounceSound, 1f - progress * 0.7f);
            }

            if (travelled >= routeLength) state = BallState.Stopped;
        }

        private void FollowCarrier()
        {
            if (carrier == null)
            {
                state = BallState.Stopped;
                return;
            }
            Vector3 forward = carrier.forward;
            forward.y = 0f;
            Vector3 target = carrier.position + forward.normalized * carryDistance;
            target.y = carrier.position.y + carrierHeightOffset;
            MoveTo(target, target - transform.position);
        }

        private void SettleStep()
        {
            settleElapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(settleElapsed / settleSeconds));
            Vector3 target = Vector3.Lerp(settleFrom, homePosition, t);
            MoveTo(target, target - transform.position);
            if (t >= 1f) state = BallState.Resting;
        }

        // Moves the ball and spins it as if it rolled the horizontal distance covered.
        private void MoveTo(Vector3 position, Vector3 direction)
        {
            Vector3 step = position - transform.position;
            step.y = 0f;
            direction.y = 0f;
            if (step.sqrMagnitude > 0.000001f && direction.sqrMagnitude > 0.000001f)
            {
                Vector3 axis = Vector3.Cross(Vector3.up, direction.normalized);
                float degrees = step.magnitude / radius * Mathf.Rad2Deg;
                transform.rotation = Quaternion.AngleAxis(degrees, axis) * transform.rotation;
            }
            transform.position = position;
        }

        private Vector3 PointAlongRoute(float distance, out Vector3 direction)
        {
            direction = Vector3.forward;
            for (int i = 1; i < rollPath.Length; i++)
            {
                Vector3 a = rollPath[i - 1].position, b = rollPath[i].position;
                float segment = Vector3.Distance(a, b);
                direction = b - a;
                if (distance <= segment || i == rollPath.Length - 1)
                    return Vector3.Lerp(a, b, segment > 0f ? Mathf.Clamp01(distance / segment) : 1f);
                distance -= segment;
            }
            return rollPath.Length > 0 ? rollPath[rollPath.Length - 1].position : transform.position;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = new Color(1f, 0.5f, 0f);
            for (int i = 1; i < rollPath.Length; i++)
                if (rollPath[i - 1] != null && rollPath[i] != null)
                    Gizmos.DrawLine(rollPath[i - 1].position, rollPath[i].position);
        }
    }
}
