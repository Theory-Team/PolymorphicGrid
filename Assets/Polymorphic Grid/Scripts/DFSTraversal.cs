using System.Collections.Generic;
using System;

namespace TheoryTeam.PolymorphicGrid
{
    /// <summary>
    /// Traversing throw each node in a graph using iterative DFS algorithm
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class DFSTraversal<T> : Traversal<T>
    {
        private readonly Stack<T> stackedNodes;
        private readonly HashSet<T> visited;

        public DFSTraversal(GetNeighborsTemplate getneighbors) : base(getneighbors)
        {
            stackedNodes = new Stack<T>();
            visited = new HashSet<T>();
        }

        public override void Traverse(T start)
        {
            if (!acceptCondition(start))
                throw new ArgumentException("Your start node is not accepted!");

            Clear();
            stackedNodes.Push(start);
            T current;

            while (stackedNodes.Count > 0)
            {
                current = stackedNodes.Pop();
                onNodeAccepted?.Invoke(current);
                visited.Add(current);

                foreach (T n in GetNeighbors(current))
                {
                    if (!acceptCondition(n))
                        continue;

                    if (ParentsEnabled)
                        parents[n] = parents.ContainsKey(n) ? parentPriority(parents[n], current, start) : current;
                    OnNeighborsFoundCallback?.Invoke(n);

                    if (!visited.Contains(n))
                        stackedNodes.Push(n);
                }
            }
        }

        public override void Clear()
        {
            base.Clear();
            stackedNodes.Clear();
            visited.Clear();
        }
    }
}