using System.Collections.Generic;
using System;

namespace TheoryTeam.PolymorphicGrid
{
    /// <summary>
    /// Traversing throw each node in a graph using BFS algorithm
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class BFSTraversal<T> : Traversal<T>
    {
        /// <summary>
        /// Determines number of layers that would be traversed.
        /// Values less than 1 traverse the whole graph.
        /// Value of 1 traverses only start node, 2 traverses start node in addition to its' neighbors.
        /// </summary>
        public int maxDepth = -1;

        private readonly Queue<T> queuedNodes;
        private readonly HashSet<T> visited;

        public BFSTraversal(GetNeighborsTemplate getNeighbors) : base(getNeighbors)
        {
            queuedNodes = new Queue<T>();
            visited = new HashSet<T>();
        }

        public override void Traverse(T start)
        {
            if (!acceptCondition(start))
                throw new ArgumentException("Your start node is not accepted!");

            Clear();
            queuedNodes.Enqueue(start);
            visited.Add(start);
            T current;

            int currentDepth = maxDepth - 1;
            int currentLayerSize = 1;
            int nextLayerSize = 0;

            while (queuedNodes.Count > 0)
            {
                current = queuedNodes.Dequeue();
                onNodeAccepted?.Invoke(current);

                foreach (T n in GetNeighbors(current))
                {
                    if (currentDepth == 0 || !acceptCondition(n) || n.Equals(start))
                        continue;
                    
                    if (ParentsEnabled)
                        parents[n] = parents.ContainsKey(n) ? parentPriority(parents[n], current, start) : current;
                    OnNeighborsFoundCallback?.Invoke(n);

                    if (visited.Add(n))
                    {
                        queuedNodes.Enqueue(n);
                        nextLayerSize++;
                    }
                }

                if (currentDepth > 0)
                {
                    currentLayerSize--;
                    if (currentLayerSize == 0)
                    {
                        currentDepth--;
                        currentLayerSize = nextLayerSize;
                        nextLayerSize = 0;
                    }
                }
            }
        }

        public override void Clear()
        {
            base.Clear();
            queuedNodes.Clear();
            visited.Clear();
        }
    }
}