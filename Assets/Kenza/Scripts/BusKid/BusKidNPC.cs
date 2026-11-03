using BehaviorDesigner.Runtime;
using LittleDriver.Prototype;
using UnityEngine;

namespace LittleDriver.BusKid
{
    public enum KidLocation { AtHideSpot, OnRoad, AtFarSide }
    public enum KidPose { Idle, Walk, Run, Greet }

    // Live numbers written by the Bus Kid tasks so you can watch the vote in the Inspector.
    [System.Serializable]
    public sealed class BusKidScores
    {
        public string runningAction = "None";
        public float dartOut;
        public float greet;
        public float walkBack;
        public float idle;
    }

    // The kid's body and senses. The Behavior Designer tasks decide WHAT to do (highest
    // GetUtility wins); this component measures the car and moves/animates the kid.
    [DefaultExecutionOrder(-50)]
    [RequireComponent(typeof(Animator), typeof(Rigidbody), typeof(CapsuleCollider))]
    public sealed class BusKidNPC : MonoBehaviour
    {
        // A running action reports this so nothing (max 1.0) can interrupt it before it finishes.
        public const float CommittedUtility = 1.1f;

        [Header("Scene references")]
        [Tooltip("The player's car. Leave empty to use the object tagged Player.")]
        public Transform car;
        public BehaviorTree behaviorTree;
        [Tooltip("Route across the road: [0] = Hide Spot behind the bus ... last = Far Side.")]
        public Transform[] crossingPath = new Transform[0];

        [Header("Movement")]
        [Min(0.1f)] public float runSpeed = 3.5f;
        [Min(0.1f)] public float walkSpeed = 1.4f;
        [Tooltip("Degrees per second.")]
        [Min(30)] public float turnSpeed = 540f;

        [Header("Dart out from behind the bus")]
        [Tooltip("The kid runs out when an approaching car is this close (m).")]
        [Min(1)] public float dartDistance = 25f;
        [Tooltip("A car slower than this (km/h) counts as stopped, not approaching.")]
        [Min(0)] public float minimumCarSpeedKmh = 5f;
        [Tooltip("Multiplies the dart score. Higher = darting wins more often.")]
        [Range(0.01f, 1)] public float dartImportance = 1f;

        [Header("Wave at a slow car")]
        [Min(1)] public float greetDistance = 12f;
        [Min(0)] public float greetMaxSpeedKmh = 10f;
        [Min(0.1f)] public float greetDuration = 2.5f;
        [Tooltip("Multiplies the greet score. Higher = waving wins more often.")]
        [Range(0.01f, 1)] public float greetImportance = 0.8f;

        [Header("Walk back behind the bus")]
        [Tooltip("Seconds the kid waits on the far side before going back.")]
        [Min(0)] public float returnDelay = 6f;
        [Tooltip("The kid only walks back when the car is at least this far away (m).")]
        [Min(0)] public float safeReturnDistance = 30f;
        [Tooltip("Multiplies the walk-back score. Higher = walking back wins more often.")]
        [Range(0.01f, 1)] public float returnImportance = 0.6f;

        [Header("Live values (Play mode, read-only)")]
        public BusKidScores scores = new BusKidScores();
        [SerializeField] KidLocation location;
        [SerializeField] float carDistance;
        [SerializeField] float carSpeedKmh;
        [SerializeField] bool carApproaching;

        public KidLocation Location => location;
        public bool HasCar => car != null;
        public float CarDistance => carDistance;
        public float CarSpeedKmh => carSpeedKmh;
        // Moving toward the kid faster than minimumCarSpeedKmh.
        public bool CarApproaching => carApproaching;
        public float SecondsAtFarSide => location == KidLocation.AtFarSide ? Time.time - arrivedAtFarSideTime : 0;
        public bool GreetedThisPass { get; set; }

        public bool HasPath
        {
            get
            {
                if (crossingPath == null || crossingPath.Length < 2) return false;
                foreach (var point in crossingPath) if (point == null) return false;
                return true;
            }
        }

        static readonly int WalkHash = Animator.StringToHash("Walk 1");
        static readonly int RunHash = Animator.StringToHash("Run 1");
        static readonly int GreetHash = Animator.StringToHash("Greet");
        static readonly int SmileHash = Animator.StringToHash("Smile 01");

        Animator animator, faceAnimator;
        Rigidbody body, carBody;
        SprintPrototypeCar playerCar;
        Vector3 startPosition, lastCarPosition;
        Quaternion startRotation;
        float arrivedAtFarSideTime;
        int nextPoint;
        bool hasWalk, hasRun, hasGreet, hasSmile;

        void Awake()
        {
            animator = GetComponent<Animator>();
            animator.applyRootMotion = false; // the clips run in place; we move the kid ourselves
            body = GetComponent<Rigidbody>();
            body.isKinematic = true;
            body.useGravity = false;
            hasWalk = HasBool(animator, WalkHash);
            hasRun = HasBool(animator, RunHash);
            hasGreet = HasBool(animator, GreetHash);
            foreach (var child in GetComponentsInChildren<Animator>(true))
                if (child != animator) { faceAnimator = child; break; }
            hasSmile = faceAnimator != null && HasBool(faceAnimator, SmileHash);

            if (car == null)
            {
                var tagged = GameObject.FindGameObjectWithTag("Player");
                if (tagged != null) car = tagged.transform;
            }
            if (car != null)
            {
                carBody = car.GetComponent<Rigidbody>();
                lastCarPosition = car.position;
                playerCar = car.GetComponent<SprintPrototypeCar>();
                if (playerCar != null) playerCar.Restarted += ResetKid;
            }
            if (behaviorTree == null) behaviorTree = GetComponent<BehaviorTree>();

            startPosition = transform.position;
            startRotation = transform.rotation;
            location = KidLocation.AtHideSpot;
        }

        void Update()
        {
            if (car == null)
            {
                carDistance = float.PositiveInfinity;
                carSpeedKmh = 0;
                carApproaching = false;
                return;
            }
            // Works with any car: use its physics velocity, or measure how far it moved this frame.
            Vector3 velocity;
            if (carBody != null && !carBody.isKinematic) velocity = carBody.linearVelocity;
            else velocity = Time.deltaTime > 0 ? (car.position - lastCarPosition) / Time.deltaTime : Vector3.zero;
            lastCarPosition = car.position;
            velocity.y = 0;

            Vector3 carToKid = transform.position - car.position;
            carToKid.y = 0;
            carDistance = carToKid.magnitude;
            carSpeedKmh = velocity.magnitude * 3.6f;
            carApproaching = carSpeedKmh >= minimumCarSpeedKmh && Vector3.Dot(velocity, carToKid) > 0;
            if (carDistance > greetDistance * 1.5f) GreetedThisPass = false;
        }

        // 0 = car at the edge of the range (or farther), 1 = car right next to the kid.
        public float CarCloseness(float range) => Mathf.Clamp01(1 - carDistance / Mathf.Max(0.01f, range));

        public void StartPath(bool towardFarSide)
        {
            location = KidLocation.OnRoad;
            nextPoint = towardFarSide ? 1 : crossingPath.Length - 2;
        }

        // Moves one frame along crossingPath. Returns true when the last marker is reached.
        public bool FollowPath(bool towardFarSide, float speed)
        {
            Vector3 goal = crossingPath[nextPoint].position;
            TurnTowards(goal);
            transform.position = Vector3.MoveTowards(transform.position, goal, speed * Time.deltaTime);
            if ((transform.position - goal).sqrMagnitude > 0.0001f) return false;

            int lastPoint = towardFarSide ? crossingPath.Length - 1 : 0;
            if (nextPoint != lastPoint)
            {
                nextPoint += towardFarSide ? 1 : -1;
                return false;
            }
            location = towardFarSide ? KidLocation.AtFarSide : KidLocation.AtHideSpot;
            if (towardFarSide) arrivedAtFarSideTime = Time.time;
            return true;
        }

        public void FaceCar()
        {
            if (car != null) TurnTowards(car.position);
        }

        // Idle kids face the road: behind the bus toward the first road marker, on the far side back across.
        public void LookAtRoad()
        {
            if (!HasPath) return;
            if (location == KidLocation.AtHideSpot) TurnTowards(crossingPath[1].position);
            else if (location == KidLocation.AtFarSide) TurnTowards(crossingPath[crossingPath.Length - 2].position);
        }

        // The Hodaart controller goes Idle -> action when a bool is true and back when it is false.
        public void SetPose(KidPose pose)
        {
            if (hasWalk) animator.SetBool(WalkHash, pose == KidPose.Walk);
            if (hasRun) animator.SetBool(RunHash, pose == KidPose.Run);
            if (hasGreet) animator.SetBool(GreetHash, pose == KidPose.Greet);
            if (hasSmile) faceAnimator.SetBool(SmileHash, pose == KidPose.Greet);
        }

        [ContextMenu("Reset Kid")]
        public void ResetKid()
        {
            if (behaviorTree != null) behaviorTree.DisableBehavior();
            transform.SetPositionAndRotation(startPosition, startRotation);
            body.position = startPosition;
            body.rotation = startRotation;
            location = KidLocation.AtHideSpot;
            GreetedThisPass = false;
            SetPose(KidPose.Idle);
            scores.runningAction = "None";
            if (behaviorTree != null && isActiveAndEnabled) behaviorTree.EnableBehavior();
        }

        void TurnTowards(Vector3 point)
        {
            Vector3 direction = point - transform.position;
            direction.y = 0;
            if (direction.sqrMagnitude < 0.0001f) return;
            transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(direction), turnSpeed * Time.deltaTime);
        }

        static bool HasBool(Animator target, int hash)
        {
            if (target.runtimeAnimatorController == null) return false;
            foreach (var parameter in target.parameters)
                if (parameter.nameHash == hash && parameter.type == AnimatorControllerParameterType.Bool) return true;
            return false;
        }

        void OnDestroy()
        {
            if (playerCar != null) playerCar.Restarted -= ResetKid;
        }

        void OnDrawGizmos()
        {
            if (!HasPath) return;
            Gizmos.color = Color.yellow;
            for (int i = 0; i < crossingPath.Length; i++)
            {
                Gizmos.DrawWireSphere(crossingPath[i].position, 0.25f);
                if (i > 0) Gizmos.DrawLine(crossingPath[i - 1].position, crossingPath[i].position);
            }
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, dartDistance);
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, greetDistance);
        }
    }
}
