namespace ZxenLib.Physics.Forces;

using System.Collections.Generic;
using Components;

public class ForceRegistry
{
    private Dictionary<long, ForceRegistration> registry;

    public ForceRegistry()
    {
        this.registry = new();
    }

    public long Add(Rigidbody2D rigidbody, IForceGenerator generator)
    {
        long hashkey = rigidbody.GetHashCode() + generator.GetHashCode();
        this.registry.Add(hashkey, new ForceRegistration(generator, rigidbody));
        return hashkey;
    }

    public void RemoveByKey(long key)
    {
        this.registry.Remove(key);
    }

    public void Remove(Rigidbody2D rigidbody, IForceGenerator generator)
    {
        long hashkey = rigidbody.GetHashCode() + generator.GetHashCode();
        this.registry.Remove(hashkey);
    }

    public void Clear()
    {
        this.registry.Clear();
    }

    public void UpdateForces(float deltaTime)
    {
        foreach (var registration in this.registry)
        {
            Rigidbody2D? rb = registration.Value.Rigidbody;
            IForceGenerator? gen = registration.Value.ForceGenerator;

            if (gen != null && rb != null && rb.IsEnabled && rb.IsAwake)
            {
                gen.UpdateForce(rb, deltaTime);
            }
        }
    }
}