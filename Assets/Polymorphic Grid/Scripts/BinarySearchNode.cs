using System.Collections.Generic;

namespace TheoryTeam.PolymorphicGrid
{
    /// <summary>
    /// Tree data stucture to make faster search through a set of number.
    /// </summary>
    /// <typeparam name="T">Data type to be handled by the data structure</typeparam>
    public class BinarySearchNode<T>
    {
        /// <summary>
        /// Value stored in this node.
        /// </summary>
        public T value;
        /// <summary>
        /// Right child which should be greater than current node.
        /// </summary>
        public BinarySearchNode<T> right;
        /// <summary>
        /// Left child which should be less than current node.
        /// </summary>
        public BinarySearchNode<T> left;

        /// <summary>
        /// Comparer used to compare and sort values in the tree.
        /// </summary>
        public readonly IComparer<T> comparer;

        /// <summary>
        /// Create new node to add it to the tree.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="comparer"></param>
        public BinarySearchNode(T value, IComparer<T> comparer)
        {
            this.value = value;
            this.comparer = comparer;
            right = null;
            left = null;
        }

        /// <summary>
        /// Search through the tree starting from this node.
        /// </summary>
        /// <param name="value">Value to search for</param>
        /// <returns></returns>
        public virtual BinarySearchNode<T> Search(T value)
        {
            int compare = comparer.Compare(value, this.value);
            if (compare > 0 && right != null)
                return right.Search(value);
            else if (compare < 0 && left != null)
                return left.Search(value);
            return this;
        }

        /// <summary>
        /// Create new node and add it to the tree in its' right place.
        /// </summary>
        /// <param name="value">Value to store in the created node</param>
        /// <returns></returns>
        public virtual bool Add(T value)
        {
            BinarySearchNode<T> found = Search(value);
            int c = comparer.Compare(value, found.value);
            if (c == 0)
                return false;

            if (c > 0)
                found.right = new BinarySearchNode<T>(value, comparer);
            else
                found.left = new BinarySearchNode<T>(value, comparer);
            return true;
        }

        /// <summary>
        /// Check for a value to see if it is contained in the tree.
        /// </summary>
        /// <param name="value">Value to search for</param>
        /// <returns></returns>
        public virtual bool Contains(T value) => comparer.Compare(Search(value).value, value) == 0;
    }

    /// <summary>
    /// This class is used to create two dimensional binary search tree.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class BinarySearchNode2D<T> : BinarySearchNode<T>
    {
        /// <summary>
        /// Child tree head.
        /// </summary>
        public BinarySearchNode<T> childHead;

        public BinarySearchNode2D(T value, T otherValue, IComparer<T> comparer): base(value, comparer)
        {
            childHead = new BinarySearchNode<T>(otherValue, comparer);
        }

        public bool Add(T value, T otherValue)
        {
            BinarySearchNode2D<T> found = Search(value) as BinarySearchNode2D<T>;
            int c = comparer.Compare(value, found.value);
            if (c == 0)
                return found.childHead.Add(otherValue);
            
            if (c > 0)
                found.right = new BinarySearchNode2D<T>(value, otherValue, comparer);
            else
                found.left = new BinarySearchNode2D<T>(value, otherValue, comparer);
            return true;
        }

        public virtual bool Contains(T value, T otherValue)
        {
            BinarySearchNode2D<T> n = Search(value) as BinarySearchNode2D<T>;
            if (comparer.Compare(value, n.value) == 0)
                return n.childHead.Contains(otherValue);
            return false;
        }
    }
}