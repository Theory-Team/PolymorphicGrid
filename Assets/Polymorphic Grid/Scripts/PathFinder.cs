using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using System;

namespace TheoryTeam.PolymorphicGrid
{
    /// <summary>
    /// Main class for finding a path accross grids using the optimal ways.
    /// </summary>
    public class PathFinder : MonoBehaviour
    {
        private ThreadStart thread = null;

        private readonly Queue<PathResponseBase> responses = new Queue<PathResponseBase>();
        private readonly Queue<PathRequestBase> requests = new Queue<PathRequestBase>();

        private static PathFinder instance;

        /// <summary>
        /// Get active instance of path finder.
        /// </summary>
        public static PathFinder Instance => instance ?? (instance = new GameObject("Path Finder").AddComponent<PathFinder>());

        private void Awake()
        {
            instance = this;
        }

        private void Update()
        {
            ProcessNextRequest();
            while (responses.Count > 0)
            {
                PathResponseBase response = responses.Dequeue();
                response.request.onResponseCallback(response);
            }
        }

        private void ProcessNextRequest()
        {
            if (requests.Count == 0 || thread != null)
                return;

            PathRequestBase request = requests.Dequeue();
            thread = new ThreadStart(() =>
            {
                try
                {
                    PathResponseBase result;
                    if (request is PathRequest r)
                    {
                        PathResponse response = new PathResponse(r, request.grid.GetNode(r.start), request.grid.GetNode(request.target));
                        response.nodesPath = FindPath(response.target, response.start, request.grid);
                        response.path = RetracePath(response.nodesPath);
                        result = response;
                    }
                    else
                    {
                        AllPathesResponse response = new AllPathesResponse(request, request.grid.GetNode(request.target));
                        response.traversal = FindAllPathes(response.target, request.grid);
                        result = response;
                    }

                    lock (responses)
                        responses.Enqueue(result);
                }
                catch(Exception e)
                {
                    lock (responses)
                        responses.Enqueue(new PathResponseBase(request, null));
                    Debug.LogError(e.StackTrace);
                }

                thread = null;
            });

            thread.Invoke();
        }

        /// <summary>
        /// Send path request and process it on another thread to receive the response with a callback.
        /// </summary>
        /// <param name="request">Find path request</param>
        public void RequestPath(PathRequestBase request) => requests.Enqueue(request);

        /// <summary>
        /// Return all nodes along the path form b to a.
        /// </summary>
        /// <param name="a">Origin node</param>
        /// <param name="b">Target node</param>
        /// <param name="grid">Grid to walk along</param>
        /// <returns></returns>
        public static IEnumerable<Node> FindPath(Node a, Node b, GridMaster grid)
        {
            Heap<Node> openSet = new Heap<Node>(grid.MaxSize);
            HashSet<Node> closeSet = new HashSet<Node>();
            int newMoveCost;
            openSet.Add(a);

            while (openSet.Count > 0)
            {
                Node current = openSet.RemoveFirst();
                closeSet.Add(current);

                if (current == b)
                    return RetracePath(b, a);

                foreach (Node n in grid.GetNeighbors(current))
                {
                    if (!n.walkable || closeSet.Contains(n))
                        continue;

                    newMoveCost = current.gCost + grid.GetStraightDistance(current, n);
                    if (newMoveCost < n.gCost || !openSet.Contains(n))
                    {
                        n.gCost = newMoveCost;
                        n.hCost = grid.GetStraightDistance(n, b);
                        n.parent = current;

                        if (!openSet.Contains(n))
                            openSet.Add(n);
                        else
                            openSet.UpdateItem(n);
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Calculate shortest path from each node in the grid to the target node.
        /// </summary>
        /// <param name="target">Target node</param>
        /// <param name="grid">Master grid</param>
        public static Traversal<Node> FindAllPathes(Node target, GridMaster grid)
        {
            BFSTraversal<Node> traversal = new BFSTraversal<Node>(grid.GetNeighbors)
            {
                acceptCondition = n => n.walkable,
                parentPriority = (Node oldParent, Node newParent, Node startNode) => newParent.hCost < oldParent.hCost ? newParent : oldParent
            };

            traversal.OnNeighborsFoundCallback = n =>
            {
                Node parent = traversal.GetParent(n);
                n.hCost = parent.hCost + grid.GetStraightDistance(n, parent);
            };

            target.hCost = 0;
            traversal.Traverse(target);
            return traversal;
        }

        /// <summary>
        /// Extract corners form nodes array and convert to simple path.
        /// </summary>
        /// <param name="path">Nodes array to convert</param>
        /// <returns></returns>
        public static IEnumerable<Vector3> RetracePath(IEnumerable<Node> path)
        {
            Node last = null;
            Vector3 dir = Vector3.zero;
            Vector3 newDir;
            foreach (Node current in path)
            {
                if (last == null)
                {
                    last = current;
                    continue;
                }

                newDir = (current.WorldPosition - last.WorldPosition).normalized;
                if (newDir != dir)
                {
                    yield return last.WorldPosition;
                    dir = newDir;
                }

                last = current;
            }

            yield return last.WorldPosition;
        }

        /// <summary>
        /// Retrace path to nodes array using parent child node feature.
        /// </summary>
        /// <param name="end">Last node in the path</param>
        /// <param name="start">First node in the path</param>
        /// <returns></returns>
        public static IEnumerable<Node> RetracePath(Node end, Node start) => RetracePath(end, start, current => current.parent);

        /// <summary>
        /// Retrace path to nodes array using parents.
        /// </summary>
        /// <param name="end">Last node in the path</param>
        /// <param name="start">First node in the path</param>
        /// <param name="getParent">Get parent function</param>
        /// <returns></returns>
        public static IEnumerable<Node> RetracePath(Node end, Node start, Func<Node, Node> getParent)
        {
            for (; end != start; end = getParent(end))
                yield return end;
            yield return start;
        }
    }

    /// <summary>
    /// Base class for requesting a path.
    /// </summary>
    public class PathRequestBase
    {
        /// <summary>
        /// Target point in the path.
        /// </summary>
        public Vector3 target;
        /// <summary>
        /// Grid to request path on.
        /// </summary>
        public GridMaster grid;
        /// <summary>
        /// This callback will automatically be called on path found.
        /// </summary>
        public Action<PathResponseBase> onResponseCallback;

        /// <summary>
        /// create new find all paths request.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="grid"></param>
        /// <param name="onResponseCallback"></param>
        public PathRequestBase(Vector3 target, GridMaster grid, Action<PathResponseBase> onResponseCallback)
        {
            this.grid = grid;
            this.target = target;
            this.onResponseCallback = onResponseCallback;
        }
    }

    /// <summary>
    /// Main class for responsing to a path request.
    /// </summary>
    public class PathResponseBase
    {
        /// <summary>
        /// Original request.
        /// </summary>
        public PathRequestBase request;
        /// <summary>
        /// Target node associated with target position in the request.
        /// </summary>
        public Node target;

        /// <summary>
        /// Create new base response object.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="target"></param>
        public PathResponseBase(PathRequestBase request, Node target)
        {
            this.request = request;
            this.target = target;
        }
    }

    /// <summary>
    /// Class for requesting a path between two points.
    /// </summary>
    public class PathRequest : PathRequestBase
    {
        /// <summary>
        /// first point in the path.
        /// </summary>
        public Vector3 start;

        /// <summary>
        /// Create new path request to request a path from start point to target point.
        /// </summary>
        /// <param name="start"></param>
        /// <param name="target"></param>
        /// <param name="grid"></param>
        /// <param name="onResponseCallback"></param>
        public PathRequest(Vector3 start, Vector3 target, GridMaster grid, Action<PathResponseBase> onResponseCallback) : base(target, grid, onResponseCallback)
        {
            this.start = start;
        }
    }

    /// <summary>
    /// Class for responsing to find path request.
    /// </summary>
    public class PathResponse : PathResponseBase
    {
        /// <summary>
        /// All nodes in the resulted path.
        /// </summary>
        public IEnumerable<Node> nodesPath;
        /// <summary>
        /// All corners in the resulted path.
        /// </summary>
        public IEnumerable<Vector3> path;
        /// <summary>
        /// node associated with start position in the request.
        /// </summary>
        public Node start;

        /// <summary>
        /// Create new Path response.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="start"></param>
        /// <param name="target"></param>
        public PathResponse(PathRequest request, Node start, Node target) : base(request, target)
        {
            this.start = start;
        }
    }

    /// <summary>
    /// Class for responsing to all paths request.
    /// </summary>
    public class AllPathesResponse : PathResponseBase
    {
        /// <summary>
        /// Traversal used by path finder to hold all data about found paths.
        /// </summary>
        public Traversal<Node> traversal;

        /// <summary>
        /// Create new all paths response.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="target"></param>
        public AllPathesResponse(PathRequestBase request, Node target) : base(request, target) { }
    }
}