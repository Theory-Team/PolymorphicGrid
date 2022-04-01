using System.Collections.Generic;
using UnityEngine;
using System;

namespace TheoryTeam.PolymorphicGrid
{
    /// <summary>
    /// Representation for node in each grid.
    /// </summary>
    [Serializable]
    public class Node : IHeap<Node>
    {
        /// <summary>
        /// This field is used by path finding system to check whether this node is walkable or not.
        /// </summary>
        public bool walkable;
        /// <summary>
        /// Position of this node local to its' grid master.
        /// </summary>
        public Vector3 localPosition;
        /// <summary>
        /// Indexes of all neighbors of this node.
        /// </summary>
        public List<int> neighbors;
        /// <summary>
        /// Grid master of this node.
        /// </summary>
        public GridMaster grid;
        /// <summary>
        /// Associated object in the scene with this node (if no object found this will return null).
        /// </summary>
        public Transform worldObject;
        /// <summary>
        /// Corners which shape this node.
        /// </summary>
        public Vector3[] vertices;

        /// <summary>
        /// Distance between current node and start node.
        /// </summary>
        [HideInInspector]
        internal int gCost;
        /// <summary>
        /// Distance between current node and target node.
        /// </summary>
        [HideInInspector]
        internal int hCost;

        /// <summary>
        /// Parent node used by path finding system to retrace found path.
        /// </summary>
        [HideInInspector]
        internal Node parent;

        [SerializeField]
        private int index;

        /// <summary>
        /// Total distance cost for this node.
        /// </summary>
        internal int fCost => gCost + hCost;
        /// <summary>
        /// Position of this node in world space.
        /// </summary>
        public Vector3 WorldPosition => grid.transform.localToWorldMatrix.MultiplyPoint3x4(localPosition);
        /// <summary>
        /// Index of this node in the heap (Used by path finding system to calculate the path as fast as possible).
        /// </summary>
        public int HeapIndex { get; set; }
        /// <summary>
        /// Index of this node in the grid.
        /// </summary>
        public int Index => index;

        /// <summary>
        /// Create new node using main parameters.
        /// </summary>
        /// <param name="localPosition"></param>
        /// <param name="index"></param>
        /// <param name="grid"></param>
        /// <param name="worldObject"></param>
        /// <param name="vertices"></param>
        public Node(Vector3 localPosition, int index, GridMaster grid, Transform worldObject, Vector3[] vertices = null)
        {
            this.walkable = true;
            this.localPosition = localPosition;
            this.worldObject = worldObject;
            this.index = index;
            this.grid = grid;
            this.vertices = vertices;

            neighbors = new List<int>();
        }

        /// <summary>
        /// Create new node using serialized node.
        /// </summary>
        /// <param name="snode"></param>
        /// <param name="index"></param>
        /// <param name="grid"></param>
        /// <param name="worldObject"></param>
        public Node(in SerializableNode snode, int index, GridMaster grid, Transform worldObject)
        {
            walkable = snode.walkable;
            localPosition = snode.localPosition;
            vertices = snode.vertices == null ? null : (Vector3[])snode.vertices.Clone();
            neighbors = new List<int>(snode.neighbors);
            this.worldObject = worldObject;
            this.index = index;
            this.grid = grid;
        }

        /// <summary>
        /// Compare two nodes to decide which one have higher priority.
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        public int CompareTo(Node n)
        {
            int f = -fCost.CompareTo(n.fCost);
            if (f == 0)
                return -hCost.CompareTo(n.hCost);
            return f;
        }

        /// <summary>
        /// Check whether a given point is inside node borders or not.
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public bool ContainsPoint(Vector3 point) => TheoryMath.PointInsideConvexPolygon(point - localPosition, grid.GetNodeCorners());
    }

}