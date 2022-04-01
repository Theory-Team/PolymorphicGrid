using System.Collections.Generic;
using System;

namespace TheoryTeam.PolymorphicGrid
{
    /// <summary>
    /// Base class for traversing throw each node in a graph.
    /// </summary>
    /// <typeparam name="T">Node type</typeparam>
    public abstract class Traversal<T>
    {
        /// <summary>
        /// The tamplate of the function that called for each reached node.
        /// </summary>
        /// <param name="neighbor">Reached node</param>
        public delegate void OnNeighborsFound(T neighbor);
        /// <summary>
        /// Template for getting neighbors of the given node.
        /// </summary>
        /// <param name="current"></param>
        /// <returns></returns>
        public delegate IEnumerable<T> GetNeighborsTemplate(T current);

        /// <summary>
        /// Method which will be used to get the neighbors.
        /// </summary>
        public GetNeighborsTemplate GetNeighbors;
        /// <summary>
        /// This callback will automatically be called on reached neighbor.
        /// </summary>
        public OnNeighborsFound OnNeighborsFoundCallback;
        /// <summary>
        /// The function pointer (Func) that decides which parent to choose depending on both old and new parents in addition to start node.
        /// The point existance of this variable is that the node could be traversed more than once.
        /// </summary>
        public Func<T, T, T, T> parentPriority;
        /// <summary>
        /// The Func that returns the distance between two adjacent nodes.
        /// </summary>
        public Func<T, T, int> straightDistance;
        /// <summary>
        /// The Func that determine the acceptance of a node for traversing (for example this fucntion is determining that unwalkable nodes are not accepted in path finding algorithms).
        /// </summary>
        public Func<T, bool> acceptCondition;
        /// <summary>
        /// This callback will automatically be called on accepted node.
        /// </summary>
        public Action<T> onNodeAccepted;

        /// <summary>
        /// This variable must be used to store parents.
        /// </summary>
        protected Dictionary<T, T> parents;

        /// <summary>
        /// Should this traversal store parents or not.
        /// </summary>
        public virtual bool ParentsEnabled { get; set; }

        public virtual int RejectedDistance => int.MaxValue;

        public Traversal(GetNeighborsTemplate getneighbors)
        {
            GetNeighbors = getneighbors;
            acceptCondition = n => true;
            parentPriority = GetOldFirst;
            parents = new Dictionary<T, T>();
            ParentsEnabled = true;
        }

        /// <summary>
        /// Traverse throw the graph starting from start node.
        /// </summary>
        /// <param name="start"></param>
        public abstract void Traverse(T start);

        /// <summary>
        /// Clear stored data in the object.
        /// </summary>
        public virtual void Clear()
        {
            parents.Clear();
        }

        /// <summary>
        /// Fucntion that returns if the node has parent or not.
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        public bool HasParent(T n) => parents.ContainsKey(n);

        /// <summary>
        /// Fuction that returns the parent node.
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        public T GetParent(T n) => parents[n];

        /// <summary>
        /// Retrace parents to calculate the distance between two nodes (Infinity loop case returns 'RejectedDistance').
        /// </summary>
        /// <param name="start"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public int GetDistance(T start, T target)
        {
            int dis = 0;
            for (T current = start; !current.Equals(target); current = parents[current])
            {
                if ((current.Equals(start) && dis > 0) || !parents.ContainsKey(current))
                    return RejectedDistance;
                dis += straightDistance(current, parents[current]);
            }
            return dis;
        }

        /// <summary>
        /// This function is used in parentPraiority func.
        /// </summary>
        /// <param name="oldParent"></param>
        /// <param name="newParent"></param>
        /// <param name="startNode"></param>
        /// <returns></returns>
        public T GetOldFirst(T oldParent, T newParent, T startNode) => oldParent;

        /// <summary>
        /// Get the closer parent to start.
        /// This function is used in parentPraiority func.
        /// </summary>
        /// <param name="oldParent"></param>
        /// <param name="newParent"></param>
        /// <param name="startNode"></param>
        /// <returns></returns>
        public T GetCloserToStart(T oldParent, T newParent, T startNode) =>
            oldParent == null || GetDistance(oldParent, startNode) > GetDistance(newParent, startNode) ? newParent : oldParent;
    }
}