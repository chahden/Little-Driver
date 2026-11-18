using System.Collections.Generic;
using LittleDriver.AI.Tasks;
using LittleDriver.Traffic;
using UnityEngine;

namespace LittleDriver.AI.Pedestrians
{
    /// <summary>
    /// Body of a pedestrian. Its decisions are taken by the Behavior Designer tree "Pedestrian":
    /// finish crossing once on the road; wait at the kerb while a car is too close; step out; otherwise walk.
    /// This component walks the sidewalk network and offers the conditions and actions the tree's tasks call.
    /// </summary>
    [RequireComponent(typeof(Animator))]
    public sealed class Pedestrian : MonoBehaviour, INpcBody
    {
        private static readonly int WalkHash = Animator.StringToHash("Walk 1");

        [SerializeField, Min(0.1f)] private float walkSpeed = 1.3f;
        [Tooltip("Chance to cross when reaching a zebra crossing.")]
        [SerializeField, Range(0f, 1f)] private float crossChance = 0.8f;
        [Tooltip("Chance, at each sidewalk node, to take the way that leads toward the nearest zebra crossing.")]
        [SerializeField, Range(0f, 1f)] private float crossingAttraction = 0.35f;
        [SerializeField, Min(30f)] private float turnSpeed = 360f;
        [Tooltip("Read-only: the action the behaviour tree is running.")]
        [SerializeField] private string currentAction;

        private readonly Queue<Vector3> route = new Queue<Vector3>();
        private Animator animator;
        private SidewalkNode previous;
        private SidewalkNode target;
        private Crosswalk waitingAt;
        private Crosswalk crossing;

        string INpcBody.CurrentAction { set => currentAction = value; }

        private void Awake()
        {
            animator = GetComponent<Animator>();
            animator.applyRootMotion = false; // the clips walk in place; the pedestrian is moved along the route
        }

        /// <summary>Starts walking from <paramref name="from"/> toward <paramref name="to"/>.</summary>
        public void Begin(SidewalkNode from, SidewalkNode to)
        {
            previous = from;
            target = to;
            route.Clear();
            route.Enqueue(to.Position);
        }

        private void OnDisable()
        {
            if (crossing != null) crossing.Leave(this);
            crossing = null;
        }

        // Conditions (asked by the tree)

        public bool HasRoute => target != null;
        public bool IsOnRoad => crossing != null;
        public bool WantsToCross => waitingAt != null;
        public bool CarTooClose => waitingAt != null && !waitingAt.IsClearToCross();

        // Actions (one frame each, run by the tree)

        public void Walk()
        {
            if (route.Count == 0) return;
            SetWalking(true);
            Vector3 goal = route.Peek();
            TurnTowards(goal);
            transform.position = Vector3.MoveTowards(transform.position, goal, walkSpeed * Time.deltaTime);
            if ((transform.position - goal).sqrMagnitude > 0.0001f) return;
            route.Dequeue();
            if (route.Count == 0) Arrive();
        }

        public void Wait()
        {
            SetWalking(false);
            TurnTowards(target.Position);
        }

        public void StepOut()
        {
            crossing = waitingAt;
            waitingAt = null;
            crossing.Enter(this);
            foreach (Vector3 point in crossing.Route(previous.IsSideA)) route.Enqueue(point);
            route.Enqueue(target.Position);
        }

        private void Arrive()
        {
            if (crossing != null) crossing.Leave(this);
            crossing = null;
            SidewalkNode node = target;
            SidewalkNode cameFrom = previous;
            previous = node;

            // At a zebra crossing (and not just off it): maybe cross.
            if (node.Crosswalk != null && node.OtherSide != null && cameFrom != node.OtherSide && Random.value < crossChance)
            {
                waitingAt = node.Crosswalk;
                target = node.OtherSide;
                return;
            }

            target = PickNext(node, cameFrom);
            route.Enqueue(target.Position);
        }

        // Mostly a random way on (never straight back unless it is a dead end); sometimes the way toward a crossing.
        private SidewalkNode PickNext(SidewalkNode node, SidewalkNode cameFrom)
        {
            var options = new List<SidewalkNode>();
            foreach (SidewalkNode n in node.Neighbours)
                if (n != null && n != cameFrom) options.Add(n);
            if (options.Count == 0) return cameFrom != null ? cameFrom : node;
            if (Random.value < crossingAttraction)
            {
                SidewalkNode closest = options[0];
                foreach (SidewalkNode n in options)
                    if (n.DistanceToCrossing < closest.DistanceToCrossing) closest = n;
                return closest;
            }
            return options[Random.Range(0, options.Count)];
        }

        private void SetWalking(bool walking) => animator.SetBool(WalkHash, walking);

        private void TurnTowards(Vector3 point)
        {
            Vector3 direction = point - transform.position;
            direction.y = 0f;
            if (direction.sqrMagnitude < 0.0001f) return;
            transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(direction), turnSpeed * Time.deltaTime);
        }
    }
}
