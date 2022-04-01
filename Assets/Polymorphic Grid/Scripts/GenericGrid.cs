using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

namespace TheoryTeam.PolymorphicGrid
{
    /// <summary>
    /// Base class for generating and handling functionality any type of grid.
    /// </summary>
    public abstract class GenericGrid : GridMaster
    {
        /// <summary>
        /// Node layers count to generate.
        /// </summary>
        public int layersCount = 5;

        public override IEnumerable<Vector3> CalculateNodesPositions()
        {
            if (layersCount < 1)
                throw new StackOverflowException("Layers lass than one generates infinte number of nodes!");

            Vector3[] corners = GetNodeCorners();
            Vector3[] edges = new Vector3[corners.Length];

            for (int i = 0; i < edges.Length; i++)
                edges[i] = (corners[i] + corners[(i + 1) % edges.Length]) * .5f;

            List<Vector3> result = new List<Vector3>();
            BinarySearchNode2D<float> searchTree = new BinarySearchNode2D<float>(0f, 0f, new SimpleFloatComparer(nodeRadius * .1f));
            BFSTraversal<Vector3> traversal = new BFSTraversal<Vector3>(center => from e in edges select center + e * 2f)
            {
                maxDepth = layersCount,
                ParentsEnabled = false,
                onNodeAccepted = pos =>
                {
                    if (searchTree.Add(pos.x, pos.z))
                        result.Add(pos);
                }
            };

            result.Add(Vector3.zero);
            traversal.Traverse(Vector3.zero);
            return result;
        }
    }
}