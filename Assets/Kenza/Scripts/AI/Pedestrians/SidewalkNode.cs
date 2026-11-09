using System.Collections.Generic;
using LittleDriver.Traffic;
using UnityEngine;

namespace LittleDriver.AI.Pedestrians
{
    /// <summary>
    /// Point of the sidewalk network. Links are two-way. A node at the end of a zebra crossing
    /// also knows the crossing and the node on the other side of the road.
    /// </summary>
    public sealed class SidewalkNode : MonoBehaviour
    {
        [SerializeField] private List<SidewalkNode> neighbours = new List<SidewalkNode>();
        [SerializeField] private Crosswalk crosswalk;
        [SerializeField] private SidewalkNode otherSide;
        [Tooltip("True on the node at the crossing's side A (the walk starts at -forward).")]
        [SerializeField] private bool isSideA;

        public IReadOnlyList<SidewalkNode> Neighbours => neighbours;
        public Crosswalk Crosswalk => crosswalk;
        public SidewalkNode OtherSide => otherSide;
        public bool IsSideA => isSideA;
        public Vector3 Position => transform.position;

        private void OnDrawGizmos()
        {
            Gizmos.color = crosswalk != null ? Color.white : new Color(1f, 0.6f, 0.2f);
            Gizmos.DrawSphere(Position, 0.25f);
            foreach (SidewalkNode n in neighbours)
                if (n != null && n.GetInstanceID() > GetInstanceID()) Gizmos.DrawLine(Position, n.Position);
            if (otherSide != null) Gizmos.DrawLine(Position, otherSide.Position);
        }
    }
}
