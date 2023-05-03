namespace ZxenLib.Entities;

using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Generic pool for entities that allows cacheing of existing entity objects, to reduce instantiation of new <see cref="Entity"/> objects.
/// </summary>
/// <typeparam name="T">The type of the pooled object where <see cref="{T}"/> is of type <see cref="IEntity"/>.</typeparam>
public class EntityPool<T> where T : IEntity
{
    private readonly Queue<T> entityPool;

    /// <summary>
    /// Initializes a new instance of the <see cref="EntityPool{T}"/> class.
    /// </summary>
    /// <param name="size">The size of this pool.</param>
    public EntityPool(int size)
    {
        this.entityPool = new Queue<T>(size);
        this.Size = size;
    }

    /// <summary>
    /// Gets the size of the pool.
    /// </summary>
    public int Size { get; }

    /// <summary>
    /// The count of objects in the pool.
    /// </summary>
    /// <returns>The number of objects in the pool of type <see cref="int""/>.</returns>
    public int Count
    {
        get => this.entityPool.Count();
    }

    /// <summary>
    /// Flushes the pool.
    /// </summary>
    public void Flush()
    {
        this.entityPool.Clear();
    }

    /// <summary>
    /// Adds an object of type <see cref="{T}"/> to the pool.
    /// </summary>
    /// <param name="obj">The <see cref="{T}"/> to add.</param>
    public void AddToPool(T obj)
    {
        if (obj == null)
        {
            return;
        }

        if (this.entityPool.Count < this.Size)
        {
            obj.IsEnabled = false;
            this.entityPool.Enqueue(obj);
        }
    }

    /// <summary>
    /// Retrieves an object of type <see cref="{T}"/> from the pool.
    /// </summary>
    /// <returns>Object of type <see cref="{T}"/>.</returns>
    public T GetFromPool()
    {
        T entity = default;

        while (entity == null && this.entityPool.Count > 0)
        {
            entity = this.entityPool.Dequeue();
        }

        return entity;
    }
}