namespace ZxenLib.Entities
{
    using System.Collections.Generic;
    using System.Linq;

    public class EntityPool<T> where T : Entity
    {
        private readonly Queue<T> entityPool;

        public int Size { get; }

        public EntityPool(int size)
        {
            this.entityPool = new Queue<T>(size);
            this.Size = size;
        }

        public void Flush()
        {
            this.entityPool.Clear();
        }

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

        public T GetFromPool()
        {
            T entity = null;

            while (entity == null && this.entityPool.Count > 0)
            {
                entity = this.entityPool.Dequeue();
            }

            return entity;
        }

        public int Count()
        {
            return this.entityPool.Count();
        }
    }
}
