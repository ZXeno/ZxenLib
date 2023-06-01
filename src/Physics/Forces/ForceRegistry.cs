namespace ZxenLib.Physics.Forces;

using System.Collections.Generic;

public class ForceRegistry
{
    private Dictionary<long, ForceRegistration> registry;

    public ForceRegistry()
    {
        this.registry = new();
    }

    public void Add(Rigidbody2D rigidbody, IForceGenerator generator)
    {
        long hashkey = rigidbody.GetHashCode() + generator.GetHashCode();
        this.registry.Add(hashkey, new ForceRegistration(generator, rigidbody));
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

            if (gen != null && rb != null)
            {
                gen.UpdateForce(rb, deltaTime);
            }
        }
    }

    public void ZeroForces()
    {
        // TODO: zero all registered forces
    }
}