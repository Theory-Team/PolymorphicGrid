using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace TheoryTeam.PolymorphicGrid
{
    /// <summary>
    /// Base class for handling any non symmetrical grid generation and functionality.
    /// </summary>
    public abstract class NonSymmetricalGenericGrid : GenericGrid
    {
        private IEnumerable<Node> GetNeighborsFromVertices(Node current)
        {
            for (int i = 0; i < current.vertices.Length; i++)
            {
                Node child = FindNode((current.vertices[i] + current.vertices[(i + 1) % current.vertices.Length]) + current.localPosition);
                if (child != null)
                    yield return child;
            }

            Node FindNode(Vector3 pos)
            {
                foreach (Node n in nodes)
                    if ((pos - n.localPosition).sqrMagnitude < .0001f)
                        return n;
                return null;
            }
        }

        private void CalculateNonSymmetricalDependencies()
        {
            Node first = nodes[0];
            foreach (Node n in nodes)
            {
                if (n.localPosition.sqrMagnitude < .0001f)
                {
                    first = n;
                    break;
                }
            }

            first.vertices = GetNodeCorners();
            BFSTraversal<Node> traversal = new BFSTraversal<Node>(GetNeighborsFromVertices);
            traversal.OnNeighborsFoundCallback = node =>
            {
                if (node.vertices == null || node.vertices.Length == 0)
                {
                    node.vertices = (from vert in traversal.GetParent(node).vertices select -vert).ToArray();
                    if (node.worldObject != null)
                    {
                        node.worldObject.rotation = Quaternion.AngleAxis(traversal.GetParent(node).worldObject.eulerAngles.y + 180, Vector3.up);
                        node.worldObject.localScale = new Vector3(-traversal.GetParent(node).worldObject.localScale.x, 1f, 1f);
                    }
                }
            };

            traversal.Traverse(first);
        }

        public override IEnumerable<Vector3> GetGeometricalNeighbors(Vector3 current) => GetGeometricalNeighbors(current, IsMirrored(current));

        public override void Init(bool reassignNeighbors = true)
        {
            base.Init(false);
            CalculateNonSymmetricalDependencies();

            if (reassignNeighbors)
                ReassignNeighbors();
        }

        public override IEnumerable<Vector3> CalculateNodesPositions()
        {
            Vector3[] corners = GetNodeCorners();
            Vector3[] edges = new Vector3[corners.Length];

            for (int i = 0; i < edges.Length; i++)
                edges[i] = (corners[i] + corners[(i + 1) % edges.Length]) * .5f;

            List<Vector3> result = new List<Vector3>();
            BinarySearchNode2D<float> searchTree = new BinarySearchNode2D<float>(0f, 0f, new SimpleFloatComparer());
            BFSTraversal<Vector3> traveral = new BFSTraversal<Vector3>(null)
            {
                maxDepth = layersCount,
                straightDistance = (Vector3 a, Vector3 b) => 1,
                onNodeAccepted = pos =>
                {
                    if (searchTree.Add(pos.x, pos.z))
                        result.Add(pos);
                }
            };

            traveral.GetNeighbors = current =>
            {
                float sign = Mathf.Sign(-(traveral.GetDistance(current, Vector3.zero) % 2) + .5f);
                return from edge in edges select current + edge * sign * 2f;
            };

            result.Add(Vector3.zero);
            traveral.Traverse(Vector3.zero);
            return result;
        }

        public override IEnumerable<Vector3> GetNodeCorners(Node n) => from corner in n.vertices select corner + n.localPosition;

        public IEnumerable<Vector3> GetGeometricalNeighbors(Vector3 current, bool mirrored)
        {
            Vector3[] corners = GetNodeCorners(mirrored).ToArray();
            for (int i = 0; i < corners.Length; i++)
                yield return current + corners[i] + corners[(i + 1) % corners.Length];
        }
        
        /// <summary>
        /// Get corners depending on mirror atribute.
        /// </summary>
        /// <param name="mirrored"></param>
        /// <returns></returns>
        public IEnumerable<Vector3> GetNodeCorners(bool mirrored)
        {
            float sign = mirrored ? -1f : 1f;
            return from corner in GetNodeCorners() select corner * sign;
        }

        /// <summary>
        /// Check wheather the node in the given position is mirrored or not.
        /// </summary>
        /// <param name="pos"></param>
        /// <returns></returns>
        public bool IsMirrored(Vector3 pos)
        {
            bool result = false;
            Vector3 current = Vector3.zero;
            List<Vector3> neighbors = new List<Vector3>(3);
            while (true)
            {
                neighbors.Clear();
                neighbors.AddRange(GetGeometricalNeighbors(current, result));

                int closest = 0;
                for (int i = 1; i < neighbors.Count; i++)
                    if ((neighbors[i] - pos).sqrMagnitude < (neighbors[closest] - pos).sqrMagnitude)
                        closest = i;

                if ((neighbors[closest] - pos).sqrMagnitude > (current - pos).sqrMagnitude)
                    break;

                current = neighbors[closest];
                result = !result;
            }

            return result;
        }
    }
}