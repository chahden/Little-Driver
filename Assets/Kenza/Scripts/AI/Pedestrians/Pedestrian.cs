using System.Collections.Generic;
using LittleDriver.AI.BehaviorTrees;
using LittleDriver.Traffic;
using UnityEngine;

namespace LittleDriver.AI.Pedestrians
{
    /// <summary>
    /// Walks the sidewalk network and sometimes crosses at a zebra crossing. Behavior Tree:
    /// finish crossing once on the road; wait at the kerb while a car is too close; step out; otherwise walk.
    /// </summary>
    [RequireComponent(typeof(Animator))]
    public sealed class Pedestrian : MonoBehaviour
    {
        private static readonly int WalkHash = Animator.StringToHash("Walk 1");

        [SerializeField, Min(0.1f)] private float walkSpeed = 1.3f;
        [Tooltip("Chance to cross when reaching a zebra crossing.")]
        [SerializeField, Range(0f, 1f)] private float crossChance = 0.8f;
        [SerializeField, Min(30f)] private float turnSpeed = 360f;
        [Tooltip("Read-only: the action the tree is running.")]
        [SerializeField] private string currentAction;

        private readonly Queue<Vector3> route = new Queue<Vector3>();
        private Animator animator;
        private BehaviorTree tree;
        private SidewalkNode previous;
        private SidewalkNode target;
        private Crosswalk waitingAt;
        private Crosswalk crossing;

        private void Awake()
        {
            animator = GetComponent<Animator>();
            animator.applyRootMotion = false; // the clips walk in place; the pedestrian is moved along the route
            tree = new BehaviorTree(new Selector("Pedestrian",
                new Sequence("Finish crossing",
                    new ConditionNode("On the road?", () => crossing != null),
                    Act("Cross the street", Walk)),
                new Sequence("Wait at the kerb",
                    new ConditionNode("Wants to cross?", () => waitingAt != null),
                    new ConditionNode("Car too close?", () => !waitingAt.IsClearToCross()),
                    Act("Wait for cars", Wait)),
                new Sequence("Step onto the crossing",
                    new ConditionNode("Wants to cross?", () => waitingAt != null),
                    Act("Step out", StepOut)),
                Act("Walk on the sidewalk", Walk)));
        }

        /// <summary>Starts walking from <paramref name="from"/> toward <paramref name="to"/>.</summary>
        public void Begin(SidewalkNode from, SidewalkNode to)
        {
            previous = from;
            target = to;
            route.Clear();
            route.Enqueue(to.Position);
        }

        private void Update()
        {
            if (target != null) tree.Tick();
        }

        private void OnDisable()
        {
            if (crossing != null) crossing.Leave(this);
            crossing = null;
        }

        private ActionNode Act(string name, System.Func<NodeStatus> action) =>
            new ActionNode(name, () => { currentAction = name; return action(); });

        private NodeStatus Walk()
        {
            if (route.Count == 0) return NodeStatus.Running;
            SetWalking(true);
            Vector3 goal = route.Peek();
            TurnTowards(goal);
            transform.position = Vector3.MoveTowards(transform.position, goal, walkSpeed * Time.deltaTime);
            if ((transform.position - goal).sqrMagnitude > 0.0001f) return NodeStatus.Running;
            route.Dequeue();
            if (route.Count == 0) Arrive();
            return NodeStatus.Running;
        }

        private NodeStatus Wait()
        {
            SetWalking(false);
            TurnTowards(target.Position);
            return NodeStatus.Running;
        }

        private NodeStatus StepOut()
        {
            crossing = waitingAt;
            waitingAt = null;
            crossing.Enter(this);
            foreach (Vector3 point in crossing.Route(previous.IsSideA)) route.Enqueue(point);
            route.Enqueue(target.Position);
            return NodeStatus.Running;
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

        // Random way on, avoiding turning straight back unless it is a dead end.
        private static SidewalkNode PickNext(SidewalkNode node, SidewalkNode cameFrom)
        {
            var options = new List<SidewalkNode>();
            foreach (SidewalkNode n in node.Neighbours)
                if (n != null && n != cameFrom) options.Add(n);
            if (options.Count == 0) return cameFrom != null ? cameFrom : node;
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
