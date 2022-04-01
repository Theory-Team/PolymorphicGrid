using System.Collections.Generic;
using System;

namespace TheoryTeam.PolymorphicGrid
{
    /// <summary>
    /// Traversing throw each node in a graph using recursive DFS algorithm
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class RecursiveDFSTraveral<T> : Traversal<T>
    {
        /// <summary>
        /// Determines number of layers that would be traversed.
        /// </summary>
        public int depth;

        private readonly HashSet<T> visited;

        public RecursiveDFSTraveral(GetNeighborsTemplate getneighbors) : base(getneighbors)
        {
            visited = new HashSet<T>();
            depth = int.MaxValue;
        }

        public override void Traverse(T start)
        {
            if (!acceptCondition(start))
                throw new ArgumentException("Your start node is note accepted!");

            Clear();
            Traverseneighbors(start, depth);

            void Traverseneighbors(T current, int depth)
            {
                if (!visited.Contains(current))
                {
                    visited.Add(current);
                    onNodeAccepted?.Invoke(current);
                }

                if (depth < 2)
                    return;

                foreach (T n in GetNeighbors(current))
                {
                    if (!acceptCondition(n))
                        continue;

                    if (ParentsEnabled)
                        parents[n] = parents.ContainsKey(n) ? parentPriority(parents[n], current, start) : current;
                    OnNeighborsFoundCallback?.Invoke(n);
                    Traverseneighbors(n, depth - 1);
                }
            }
        }

        public override void Clear()
        {
            base.Clear();
            visited.Clear();
        }
    }
}