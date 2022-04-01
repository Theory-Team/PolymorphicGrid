using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;

namespace TheoryTeam.PolymorphicGrid
{
    /// <summary>
    /// Base class to create any grid shape and do operations on it.
    /// </summary>
    public abstract class GridMaster : MonoBehaviour, ISerializationCallbackReceiver
    {
        /// <summary>
        /// Float comparer implementation with accuracy attribut to avoid binary calculation mistakes.
        /// </summary>
        protected class SimpleFloatComparer : IComparer<float>
        {
            /// <summary>
            /// Minimum difference to decide that two floats are equal.
            /// </summary>
            public float accuracy;

            public SimpleFloatComparer(float accuracy = .0001f)
            {
                this.accuracy = accuracy;
            }

            public int Compare(float a, float b) => Mathf.Abs(a - b) <= accuracy ? 0 : a.CompareTo(b);
        }

        /// <summary>
        /// This value is used to calculate node corners and node spacing while generating your grid.
        /// </summary>
        public float nodeRadius = .5f;
        /// <summary>
        /// Prefab to be instantiated at each node position.
        /// </summary>
        public GameObject nodePrefab;
        /// <summary>
        /// Create prefab on each node position.
        /// </summary>
        public bool createNodeObject = true;

        /// <summary>
        /// All nodes in the grid.
        /// </summary>
        [SerializeField]
        protected List<Node> nodes = new List<Node>();
        [SerializeField]
        protected bool edgeNieghborsOnly = false;
        [SerializeField]
        private float generatedNodeRadius;

        private Traversal<Node> distanceTraversal = null;
        private Dictionary<(float, float), Node> positionMap;
        private BinarySearchNode2D<float> positionTree;

        /// <summary>
        /// Generated nodes count in this grid.
        /// </summary>
        public int MaxSize => nodes.Count;
        /// <summary>
        /// Node Radius x2.
        /// </summary>
        public float NodeDiameter => generatedNodeRadius * 2f;
        /// <summary>
        /// Get all nodes in the grid.
        /// </summary>
        public IEnumerable<Node> Nodes => nodes;
        /// <summary>
        /// Node radius value used to generated the grid.
        /// </summary>
        public float GeneratedNodeRadius => generatedNodeRadius;
        /// <summary>
        /// Consider only nodes with common edge as neighbors.
        /// </summary>
        public virtual bool EdgeNeighborsOnly
        {
            get => edgeNieghborsOnly;
            set => edgeNieghborsOnly = value;
        }

        /// <summary>
        /// Traversal used by GetDistance method.
        /// </summary>
        protected Traversal<Node> DistanceTraversal => distanceTraversal ?? (distanceTraversal = new BFSTraversal<Node>(GetNeighbors));

        private void UpdatePositionTreeMap()
        {
            if (nodes == null || nodes.Count == 0)
                return;

            positionMap = new Dictionary<(float, float), Node>(nodes.Count);
            positionTree = new BinarySearchNode2D<float>(nodes[0].localPosition.x, nodes[0].localPosition.z, new SimpleFloatComparer());
            positionMap.Add((nodes[0].localPosition.x, nodes[0].localPosition.z), nodes[0]);
            foreach (Node n in nodes)
            {
                if (!positionTree.Add(n.localPosition.x, n.localPosition.z))
                    continue;

                BinarySearchNode2D<float> first = positionTree.Search(n.localPosition.x) as BinarySearchNode2D<float>;
                positionMap.Add((first.value, first.childHead.Search(n.localPosition.z).value), n);
            }
        }

        /// <summary>
        /// Get default node vertices.
        /// </summary>
        /// <returns></returns>
        public abstract Vector3[] GetNodeCorners();

        /// <summary>
        /// Define where to place each node in the grid.
        /// </summary>
        /// <returns></returns>
        public abstract IEnumerable<Vector3> CalculateNodesPositions();

        /// <summary>
        /// Get specific node vertices.
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        public virtual IEnumerable<Vector3> GetNodeCorners(Node n) => from corner in GetNodeCorners() select corner + n.localPosition;

        /// <summary>
        /// Get surrounded nodes for a given node.
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public virtual IEnumerable<Node> GetNeighbors(Node node) => from index in node.neighbors select nodes[index];

        /// <summary>
        /// Calculate expected positions of the neighbors of a given nodes' position.
        /// </summary>
        /// <param name="current"></param>
        /// <returns></returns>
        public virtual IEnumerable<Vector3> GetGeometricalNeighbors(Vector3 current)
        {
            Vector3[] corners = GetNodeCorners();
            for (int i = 0; i < corners.Length; i++)
                yield return current + corners[i] + corners[(i + 1) % corners.Length];
        }

        /// <summary>
        /// Create nodes and initialize the grid.
        /// </summary>
        /// <param name="reassignNeighbors"></param>
        public virtual void Init(bool reassignNeighbors = true)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();

            Clear();
            generatedNodeRadius = nodeRadius;

            int i = 0;
            Transform currentNode = null;
            foreach (Vector3 pos in CalculateNodesPositions())
            {
                if (createNodeObject)
                {
                    currentNode = Instantiate(nodePrefab).transform;
                    currentNode.parent = transform;
                    currentNode.localPosition = pos;
                    currentNode.localRotation = Quaternion.identity;
                    currentNode.gameObject.name = $"Node {i}";
                }

                nodes.Add(new Node(pos, i, this, currentNode));
                i++;
            }
            
            UpdatePositionTreeMap();
            if (reassignNeighbors)
                ReassignNeighbors();
            sw.Stop();
            UnityEngine.Debug.Log($"Grid created in {sw.ElapsedTicks / 10000f}ms");
        }

        /// <summary>
        /// Reassign neighbors for each node in the grid.
        /// </summary>
        public virtual void ReassignNeighbors()
        {
            List<Vector3> neighbors = new List<Vector3>();
            BFSTraversal<Vector3> traversal = new BFSTraversal<Vector3>(GetGeometricalNeighbors)
            {
                maxDepth = EdgeNeighborsOnly ? 2 : 5,
                ParentsEnabled = false,
                acceptCondition = current => positionTree.Contains(current.x, current.z),
                onNodeAccepted = pos => neighbors.Add(pos)
            };

            for (int i = 0; i < nodes.Count; i++)
            {
                neighbors.Clear();
                traversal.Traverse(nodes[i].localPosition);
                foreach (Vector3 neighbor in neighbors)
                {
                    if (neighbor == nodes[i].localPosition)
                        continue;

                    BinarySearchNode2D<float> first = positionTree.Search(neighbor.x) as BinarySearchNode2D<float>;
                    BinarySearchNode<float> second = first.childHead.Search(neighbor.z);
                    if (IsNeighbors(nodes[i], positionMap[(first.value, second.value)]))
                        nodes[i].neighbors.Add(positionMap[(first.value, second.value)].Index);
                }
            }
        }

        /// <summary>
        /// Return true if the two given nodes are neighbors.
        /// </summary>
        /// <param name="a">First node to check</param>
        /// <param name="b">Second node to check</param>
        /// <returns></returns>
        public virtual bool IsNeighbors(in Node a, in Node b)
            {
                int matchingCounter = 0;
                foreach (Vector3 ca in GetNodeCorners(a))
                    foreach (Vector3 cb in GetNodeCorners(b))
                        if ((ca - cb).sqrMagnitude < .0001f)
                            matchingCounter++;

                return (EdgeNeighborsOnly && matchingCounter > 1) || (!EdgeNeighborsOnly && matchingCounter > 0);
            }

        /// <summary>
        /// Save generated grid into scriptable object so you can store it and load it when ever you want.
        /// </summary>
        /// <returns></returns>
        public virtual SerializableGrid Serialize()
        {
            if (nodes == null || nodes.Count == 0)
                throw new System.NullReferenceException("Nothing to be serialized!");

            SerializableGrid result = ScriptableObject.CreateInstance<SerializableGrid>();
            result.serializedNodes = new SerializableNode[nodes.Count];
            for (int i = 0; i < result.serializedNodes.Length; i++)
                result.serializedNodes[i] = new SerializableNode(nodes[i]);
            result.gridTypeName = GetType().Name;
            result.createNodeObject = createNodeObject;
            result.nodeRadius = generatedNodeRadius;
            return result;
        }

        /// <summary>
        /// Load generated grid form disk.
        /// </summary>
        /// <param name="grid">Grid to load</param>
        public virtual void Deserialize(SerializableGrid grid)
        {
            if (grid.gridTypeName != GetType().Name)
                throw new System.ArgumentException($"You are tring to load {grid.gridTypeName} into {GetType().Name}!");

            Clear();
            nodes = new List<Node>(grid.serializedNodes.Length);
            createNodeObject = grid.createNodeObject;
            Transform obj = null;

            for (int i = 0; i < nodes.Capacity; i++)
            {
                if (createNodeObject)
                {
                    obj = Instantiate(nodePrefab, transform).transform;
                    obj.localPosition = grid.serializedNodes[i].localPosition;
                    obj.localRotation = grid.serializedNodes[i].worldObjectRotation;
                    obj.localScale = grid.serializedNodes[i].worldObjectScale;
                }

                nodes.Add(new Node(grid.serializedNodes[i], i, this, obj));
            }

            generatedNodeRadius = nodeRadius = grid.nodeRadius;
        }

        /// <summary>
        /// Get minimum distance between any two nodes on the grid.
        /// </summary>
        /// <param name="a">First node</param>
        /// <param name="b">Second node</param>
        /// <returns></returns>
        public virtual int GetDistance(Node a, Node b)
        {
            DistanceTraversal.straightDistance = GetApproxStraightDistance;
            DistanceTraversal.Traverse(b);

            DistanceTraversal.straightDistance = GetStraightDistance;
            return DistanceTraversal.GetDistance(a, b);
        }

        /// <summary>
        /// Project position on the grid and find to which node belongs.
        /// </summary>
        /// <param name="position">Position to find its' node</param>
        /// <returns></returns>
        public virtual int GetNodeIndex(in Vector3 position)
        {
            if (positionMap.Count == 0)
                UpdatePositionTreeMap();

            float old = (positionTree.comparer as SimpleFloatComparer).accuracy;
            (positionTree.comparer as SimpleFloatComparer).accuracy = generatedNodeRadius * generatedNodeRadius;
            int result = -1;

            Vector3 local = transform.worldToLocalMatrix.MultiplyPoint3x4(position);
            BinarySearchNode2D<float> node = positionTree.Search(local.x) as BinarySearchNode2D<float>;
            BinarySearchNode<float> otherNode = node.childHead.Search(local.z);
            Node found = positionMap[(node.value, otherNode.value)];

            if (found.ContainsPoint(local))
                result = found.Index;
            else
            {
                foreach (Node n in GetNeighbors(found))
                    if (n.ContainsPoint(local))
                        result = n.Index;
            }

            (positionTree.comparer as SimpleFloatComparer).accuracy = old;
            return result;
        }

        /// <summary>
        /// Get the straight distance between two nodes.
        /// </summary>
        /// <param name="a">First node</param>
        /// <param name="b">Second node</param>
        /// <returns></returns>
        public int GetStraightDistance(Node a, Node b)
        {
            float original = (a.localPosition - b.localPosition).sqrMagnitude;
            float approx = .5f * (original + 1f);
            return (int)(5f * (approx + original / approx));
        }

        /// <summary>
        /// Get approximetely the straight distance between two nodes.
        /// </summary>
        /// <param name="a">First node</param>
        /// <param name="b">Second node</param>
        /// <returns></returns>
        public int GetApproxStraightDistance(Node a, Node b) => (int)(5f * ((a.localPosition - b.localPosition).sqrMagnitude + 1f));

        /// <summary>
        /// Get node for nodes list using an index.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public Node GetNode(in int index) => nodes[index];

        /// <summary>
        /// Project position on the grid and find to which node belongs.
        /// </summary>
        /// <param name="position">Position to find its' node</param>
        /// <returns></returns>
        public Node GetNode(in Vector3 position)
        {
            int index = GetNodeIndex(position);
            return index < 0 || index >= MaxSize ? null : nodes[index];
        }

        /// <summary>
        /// Clear and destroy all nodes and connections for this grid.
        /// </summary>
        public void Clear()
        {
            foreach (Node n in nodes)
                if (n.worldObject != null)
                    DestroyImmediate(n.worldObject.gameObject);
            DistanceTraversal.Clear();
            nodes.Clear();

            if (positionMap != null)
            {
                positionMap.Clear();
                positionTree = null;
            }
        }

        public void OnBeforeSerialize() { }

        public void OnAfterDeserialize()
        {
            if (positionMap == null || positionMap.Count == 0)
                UpdatePositionTreeMap();
        }
    }
}