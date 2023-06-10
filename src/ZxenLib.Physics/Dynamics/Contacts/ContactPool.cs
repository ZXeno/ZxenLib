namespace ZxenLib.Physics.Dynamics.Contacts;

using System;
using System.Buffers;
using System.Runtime.CompilerServices;

public class ContactPool<T>
    where T : Contact, new()
{
    private T[] _objects;

    private int _total;

    private int _capacity;

    public ContactPool(int capacity = 256)
    {
        this._capacity = capacity;
        this._objects = ArrayPool<T>.Shared.Rent(this._capacity);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T Get()
    {
        if (this._total > 0)
        {
            --this._total;
            T? item = this._objects[this._total];
            this._objects[this._total] = null;
            return item;
        }

        return new T();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Return(T item)
    {
        item.Reset();
        if (this._total < this._capacity)
        {
            this._objects[this._total] = item;
            ++this._total;
        }
        else
        {
            T[]? old = this._objects;
            this._capacity *= 2;
            this._objects = ArrayPool<T>.Shared.Rent(this._capacity);
            Array.Copy(old, this._objects, this._total);
            ArrayPool<T>.Shared.Return(old, true);
            this._objects[this._total] = item;
            ++this._total;
        }
    }
}