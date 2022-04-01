using System.Collections.Generic;
using UnityEngine;

namespace TheoryTeam.PolymorphicGrid
{
    public class SquareGrid : GridMaster
    {
        /// <summary>
        /// Horizontal node count to generate.
        /// </summary>
        public int gridSizeX = 10;
        /// <summary>
        /// Vertical node count to generate.
        /// </summary>
        public int gridSizeY = 10;

        [SerializeField]
        private bool edgeNeighborsOnly;
        [SerializeField]
        private int generatedGridSizeX;
        [SerializeField]
        private int generatedGridSizeY;

        public override bool EdgeNeighborsOnly { get => edgeNeighborsOnly; set => edgeNeighborsOnly = value; }

        /// <summary>
        /// Position of the last node in the top right corner of the grid.
        /// </summary>
        public Vector3 Max => NodeDiameter * generatedGridSizeX * .5f * Vector3.right + NodeDiameter * generatedGridSizeY * .5f * Vector3.forward;
        /// <summary>
        /// Position of the first node in the down left corner of the grid.
        /// </summary>
        public Vector3 Min => -Max;

        /// <summary>
        /// Horizontal node count in the generated grid.
        /// </summary>
        public int GeneratedGridSizeX => generatedGridSizeX;
        /// <summary>
        /// Vertical ndoe count in the generated grid.
        /// </summary>
        public int GeneratedGridSizeY => generatedGridSizeY;

        public override IEnumerable<Vector3> CalculateNodesPositions()
        {
            Vector3 startPos = Min;
            for (int y = 0; y < gridSizeY; y++)
                for (int x = 0; x < gridSizeX; x++)
                    yield return startPos + Vector3.right * (x * NodeDiameter + nodeRadius) + Vector3.forward * (y * NodeDiameter + nodeRadius);
        }

        public override void ReassignNeighbors() { }

        public override IEnumerable<Node> GetNeighbors(Node node)
        {
            ConvertIndex(node.Index, out int x, out int y);
            return GetNeighbors(x, y);
        }

        public override int GetNodeIndex(in Vector3 position)
        {
            Vector3 min = Min;
            Vector3 max = Max;
            Vector3 local = transform.InverseTransformPoint(position);

            int x = Mathf.FloorToInt((local.x - min.x) / (max.x - min.x) * generatedGridSizeX);
            if (x < 0 || x >= generatedGridSizeX)
                return -1;

            int y = Mathf.FloorToInt((local.z - min.z) / (max.z - min.z) * generatedGridSizeY);
            if (y < 0 || y >= generatedGridSizeY)
                return -1;

            return Convert2Index(x, y);
        }
        
        public override int GetDistance(Node a, Node b)
        {
            ConvertIndex(a.Index, out int x1, out int y1);
            ConvertIndex(b.Index, out int x2, out int y2);

            int x = Mathf.Abs(x1 - x2);
            int y = Mathf.Abs(y1 - y2);

            return x > y ? y * 14 + (x - y) * 10 : x * 14 + (y - x) * 10;
        }
        
        public override Vector3[] GetNodeCorners()
        {
            return new Vector3[4] {
                new Vector3(-GeneratedNodeRadius, 0f, -GeneratedNodeRadius),
                new Vector3(GeneratedNodeRadius, 0f, -GeneratedNodeRadius),
                new Vector3(GeneratedNodeRadius, 0f, GeneratedNodeRadius),
                new Vector3(-GeneratedNodeRadius, 0f, GeneratedNodeRadius)
            };
        }

        public override void Deserialize(SerializableGrid grid)
        {
            base.Deserialize(grid);

            int xCount = 0;
            float lastX = float.NegativeInfinity;
            foreach (Node n in nodes)
            {
                if (n.localPosition.x < lastX)
                    break;

                lastX = n.localPosition.x;
                xCount++;
            }

            generatedGridSizeX = gridSizeX = xCount;
            generatedGridSizeY = gridSizeY = nodes.Count / gridSizeX;
        }

        public override void Init(bool reassignNeighbors = true)
        {
            generatedGridSizeX = gridSizeX;
            generatedGridSizeY = gridSizeY;
            base.Init(reassignNeighbors);
        }

        /// <summary>
        /// Get neighbors for given node using its' x, y indexes.
        /// </summary>
        /// <param name="xpos">X index of the node</param>
        /// <param name="ypos">Y index of the node</param>
        /// <returns></returns>
        public IEnumerable<Node> GetNeighbors(int xpos, int ypos)
        {
            if (EdgeNeighborsOnly)
            {
                if (xpos - 1 >= 0)
                    yield return GetNode(xpos - 1, ypos);
                if (xpos + 1 < gridSizeX)
                    yield return GetNode(xpos + 1, ypos);
                if (ypos - 1 >= 0)
                    yield return GetNode(xpos, ypos - 1);
                if (ypos + 1 < gridSizeY)
                    yield return GetNode(xpos, ypos + 1);
            }
            else
            {
                int minY = Mathf.Max(0, ypos - 1);
                int maxY = Mathf.Min(ypos + 2, gridSizeY);
                int minX = Mathf.Max(0, xpos - 1);
                int maxX = Mathf.Min(xpos + 2, gridSizeX);

                for (int y = minY; y < maxY; y++)
                    for (int x = minX; x < maxX; x++)
                        if (x != xpos || y != ypos)
                            yield return GetNode(x, y);
            }
        }

        /// <summary>
        /// Convert into x, y indexes.
        /// </summary>
        /// <param name="index">Index to convert</param>
        /// <param name="x">X index</param>
        /// <param name="y">Y index</param>
        public void ConvertIndex(in int index, out int x, out int y)
        {
            y = index / generatedGridSizeX;
            x = index % generatedGridSizeX;
        }

        /// <summary>
        /// Convert form x, y indexes into array index.
        /// </summary>
        /// <param name="x">X index</param>
        /// <param name="y">Y index</param>
        /// <returns></returns>
        public int Convert2Index(in int x, in int y) => y * generatedGridSizeX + x;

        /// <summary>
        /// Get node using its' x, y indexes.
        /// </summary>
        /// <param name="x">X index</param>
        /// <param name="y">Y index</param>
        /// <returns></returns>
        public Node GetNode(in int x, in int y) => nodes[Convert2Index(x, y)];
    }
}