namespace ZxenLib;

using System.Collections.Generic;

/// <summary>
/// Implementation of a generic Priority Queue, using <see cref="IComparer{T}"/> to compare two items and sort a list accordingly.
/// </summary>
/// <typeparam name="T">The containing type.</typeparam>
public class PriorityQueue<T>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PriorityQueue{T}"/> class.
    /// </summary>
    public PriorityQueue()
    {
        this.Comparer = Comparer<T>.Default;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PriorityQueue{T}"/> class.
    /// </summary>
    /// <param name="comparer">A custom comparer for the type of <see cref="{T}"/>.</param>
    public PriorityQueue(IComparer<T> comparer)
    {
        this.Comparer = comparer;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PriorityQueue{T}"/> class.
    /// </summary>
    /// <param name="comparer">A custom comparer for the type of <see cref="{T}"/>.</param>
    /// <param name="capacity">The specified capacity of the queue.</param>
    public PriorityQueue(IComparer<T> comparer, int capacity)
    {
        this.Comparer = comparer;
        this.InnerList.Capacity = capacity;
    }

    /// <summary>
    /// Returns the count of the items in the <see cref="PriorityQueue{T}"/>.
    /// </summary>
    public int Count => this.InnerList.Count;

    /// <summary>
    /// The inner collection used for the priority queue.
    /// </summary>
    protected List<T> InnerList { get; set; } = new List<T>();

    /// <summary>
    /// Gets or sets the Comparer used for this <see cref="PriorityQueue{T}"/>.
    /// </summary>
    protected IComparer<T> Comparer { get; set; }

    /// <summary>
    /// Indexer for accessing items in the <see cref="PriorityQueue{T}"/>.
    /// </summary>
    /// <param name="index">The index of the object to retrieve.</param>
    /// <returns><see cref="{T}"/>.</returns>
    public T this[int index]
    {
        get => this.InnerList[index];

        set
        {
            this.InnerList[index] = value;
            this.Update(index);
        }
    }

    /// <summary>
    /// Push an object onto the priority queue.
    /// </summary>
    /// <param name="item">The new object.</param>
    /// <returns>The index in the list where the object is now. Changes when objects are taken from or put into the <see cref="PriorityQueue{T}"/>.</returns>
    public int Push(T item)
    {
        int location1 = this.InnerList.Count;
        this.InnerList.Add(item);
        do
        {
            if (location1 == 0)
            {
                break;
            }

            int location2 = (location1 - 1) / 2;
            if (this.DoCompare(location1, location2) < 0)
            {
                this.SwitchElements(location1, location2);
                location1 = location2;
            }
            else
            {
                break;
            }
        }
        while (true);
        return location1;
    }

    /// <summary>
    /// Get the smallest object and remove it.
    /// </summary>
    /// <returns>The smallest object.</returns>
    public T Pop()
    {
        T result = this.InnerList[0];
        int index = 0;
        int location1;
        int location2;
        int nLocation;
        this.InnerList[0] = this.InnerList[this.InnerList.Count - 1];
        this.InnerList.RemoveAt(this.InnerList.Count - 1);
        do
        {
            nLocation = index;
            location1 = (2 * index) + 1;
            location2 = (2 * index) + 2;
            if (this.InnerList.Count > location1 && this.DoCompare(index, location1) > 0)
            {
                index = location1;
            }

            if (this.InnerList.Count > location2 && this.DoCompare(index, location2) > 0)
            {
                index = location2;
            }

            if (index == nLocation)
            {
                break;
            }

            this.SwitchElements(index, nLocation);
        }
        while (true);

        return result;
    }

    /// <summary>
    /// Notify the <see cref="PriorityQueue{T}"/> that the object at index x has changed
    /// and the <see cref="PriorityQueue{T}"/> needs to restore order.
    ///
    /// Since you dont have access to any indexes (except by using the explicit IList.this)
    /// you should not call this function without knowing exactly what you do.
    /// </summary>
    /// <param name="changedIndex">The index of the changed object.</param>
    public void Update(int changedIndex)
    {
        int index = changedIndex;
        int location2;
        do
        {
            if (index == 0)
            {
                break;
            }

            location2 = (index - 1) / 2;
            if (this.DoCompare(index, location2) < 0)
            {
                this.SwitchElements(index, location2);
                index = location2;
                continue;
            }

            break;
        }
        while (true);

        if (index < changedIndex)
        {
            return;
        }

        do
        {
            int nLocation = index;
            int location1 = (2 * index) + 1;
            location2 = (2 * index) + 2;
            if (this.InnerList.Count > location1 && this.DoCompare(index, location1) > 0)
            {
                index = location1;
            }

            if (this.InnerList.Count > location2 && this.DoCompare(index, location2) > 0)
            {
                index = location2;
            }

            if (index == nLocation)
            {
                break;
            }

            this.SwitchElements(index, nLocation);
        }
        while (true);
    }

    /// <summary>
    /// Get the smallest object without removing it.
    /// </summary>
    /// <returns>The smallest object.</returns>
    public T Peek()
    {
        if (this.InnerList.Count > 0)
        {
            return this.InnerList[0];
        }

        return default;
    }

    /// <summary>
    /// Clears the collection.
    /// </summary>
    public void Clear()
    {
        this.InnerList.Clear();
    }

    /// <summary>
    /// Removes an item from the <see cref="PriorityQueue{T}"/>.
    /// </summary>
    /// <param name="item">The item to remove.</param>
    public void Remove(T item)
    {
        int index = -1;
        for (int x = 0; x < this.InnerList.Count; x++)
        {
            if (this.Comparer.Compare(this.InnerList[x], item) == 0)
            {
                index = x;
                break;
            }
        }

        if (index != -1)
        {
            this.InnerList.RemoveAt(index);
        }
    }

    /// <summary>
    /// Switches to elements of the <see cref="PriorityQueue{T}"/>.
    /// </summary>
    /// <param name="x">The index of the first object.</param>
    /// <param name="y">The index of the second object.</param>
    protected void SwitchElements(int x, int y)
    {
        T element = this.InnerList[x];
        this.InnerList[x] = this.InnerList[y];
        this.InnerList[y] = element;
    }

    /// <summary>
    /// Runs when a comparison is made.
    /// </summary>
    /// <param name="x">The index of the first object.</param>
    /// <param name="y">The index of the second object.</param>
    /// <returns>A signed integer that indicates the relative values of x and y. A value less than zero indicates that x is less than y, a value of zero means that x equals y, and a value greater than zero indicates that x is greater than y.</returns>
    protected virtual int DoCompare(int x, int y)
    {
        return this.Comparer.Compare(this.InnerList[x], this.InnerList[y]);
    }
}