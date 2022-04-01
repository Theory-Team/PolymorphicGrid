using System;

namespace TheoryTeam.PolymorphicGrid
{
    public class Heap<T> where T : IHeap<T>
    {
        private readonly T[] items;

        public int Count { get; private set; }

        public Heap(int maxSize)
        {
            items = new T[maxSize];
            Count = 0;
        }

        public void Add(T item)
        {
            items[Count] = item;
            items[Count].HeapIndex = Count;
            SwapUp(item);
            Count++;
        }

        public void Clear() => Count = 0;

        public void UpdateItem(T item)
        {
            SwapUp(item);
            Heapfy(item);
        }

        public T RemoveFirst()
        {
            if (Count == 0)
                throw new NullReferenceException("No items found in the heap!");

            T first = items[0];
            Count--;
            items[0] = items[Count];
            items[0].HeapIndex = 0;
            Heapfy(items[0]);
            return first;
        }

        public bool Contains(T item)
        {
            return item.Equals(items[item.HeapIndex]);
        }

        private void Heapfy(T item)
        {
            int leftChild = item.HeapIndex * 2 + 1;
            int rightChild = item.HeapIndex * 2 + 2;
            int swapIndex = item.HeapIndex;

            if (leftChild < Count)
            {
                if (items[swapIndex].CompareTo(items[leftChild]) == -1)
                    swapIndex = leftChild;

                if (rightChild < Count && items[swapIndex].CompareTo(items[rightChild]) == -1)
                    swapIndex = rightChild;

                if (swapIndex != item.HeapIndex)
                {
                    Swap(item, items[swapIndex]);
                    Heapfy(item);
                }
            }
        }

        private void SwapUp(T item)
        {
            int parentIndex = (item.HeapIndex - 1) / 2;
            T parentItem = items[parentIndex];
            if (item.HeapIndex != 0 && item.CompareTo(parentItem) == 1)
            {
                Swap(item, parentItem);
                SwapUp(item);
            }
        }

        private void Swap(T a, T b)
        {
            items[a.HeapIndex] = b;
            items[b.HeapIndex] = a;
            int temp = a.HeapIndex;
            a.HeapIndex = b.HeapIndex;
            b.HeapIndex = temp;
        }
    }

    public interface IHeap<T> : IComparable<T>
    {
        int HeapIndex { get; set; }
    }
}
