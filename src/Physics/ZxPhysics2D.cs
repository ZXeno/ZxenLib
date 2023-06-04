namespace ZxenLib.Physics;

using System;
using System.Collections.Generic;
using System.Linq;
using Components;
using Collisions;
using Events;
using Forces;
using Interfaces;
using Microsoft.Xna.Framework;

public class ZxPhysics2D
{
    private IEventDispatcher eventDispatcher;
    private ForceRegistry forceRegistry;
    private List<Rigidbody2D> rigidBodies;

    private List<Rigidbody2D> bodies1;
    private List<Rigidbody2D> bodies2;
    private List<CollisionManifold> collisions;

    private Gravity2D gravity;
    private float updateRate = 0f;
    private int impulseIterations = 6;

    public ZxPhysics2D(IEventDispatcher eventDispatcher)
    {
        this.eventDispatcher = eventDispatcher;
        this.forceRegistry = new();
        this.rigidBodies = new();
        this.bodies1 = new();
        this.bodies2 = new();
        this.collisions = new();
    }

    public void Initialize(float updateRate, Vector2 gravity)
    {
        this.updateRate = updateRate;
        this.gravity = new Gravity2D(gravity);
    }

    public void SetUpdateRate(float updateRate)
    {
        this.updateRate = updateRate;
    }

    public void SetGravity(Vector2 gravity)
    {
        if (this.gravity == null!)
        {
            this.gravity = new Gravity2D(gravity);
            return;
        }

        this.gravity.SetForce(gravity);
    }

    public void Update(float deltaTime)
    {
        this.FixedUpdate();
    }

    public void FixedUpdate()
    {
        this.bodies1.Clear();
        this.bodies2.Clear();
        this.collisions.Clear();

        // Find Collisions
        // FIXME: THIS IS SO FUCKING STUPID
        for (int x = 0; x < this.rigidBodies.Count; x++)
        {
            for (int y = x; y < this.rigidBodies.Count; y++)
            {
                if (y == x)
                {
                    continue;
                }

                Rigidbody2D r1 = this.rigidBodies[x];
                Rigidbody2D r2 = this.rigidBodies[y];
                ICollider2D? c1 = r1.Collider;
                ICollider2D? c2 = r2.Collider;

                if (c1 == null || c2 == null || r1.IsInfiniteMass || r2.IsInfiniteMass)
                {
                    continue;
                }

                CollisionManifold? result = CollisionDetector.FindCollisionFeatures(c1, c2);
                if (result != null && result.IsColliding)
                {
                    this.bodies1.Add(r1);
                    this.bodies2.Add(r2);
                    this.collisions.Add(result);
                }
            }
        }

        // update forces
        this.forceRegistry.UpdateForces(this.updateRate);

        // Resolve Collisions
        for (int x = 0; x < this.impulseIterations; x++)
        {
            for (int i = 0; i < this.collisions.Count; i++)
            {
                int points = this.collisions[i].ContactPoints.Count();
                for (int j = 0; j < points; j++)
                {
                    Rigidbody2D r1 = this.bodies1[i];
                    Rigidbody2D r2 = this.bodies2[i];
                    ApplyImpulse(r1, r2, this.collisions[i]);
                }
            }
        }

        // update rigidbodies
        foreach (Rigidbody2D body in this.rigidBodies)
        {
            body.PhysicsUpdate(this.updateRate);
        }
    }

    private void ApplyImpulse(Rigidbody2D a, Rigidbody2D b, CollisionManifold collision)
    {
        float inverseMass1 = a.InverseMass;
        float inverseMass2 = b.InverseMass;
        float invMassSum = inverseMass1 + inverseMass2;

        if (invMassSum == 0)
        {
            return;
        }

        // relative velocity
        Vector2 relativeVelocity = b.Velocity - a.Velocity;
        Vector2 relativeNormal = collision.Normal;
        relativeNormal.Normalize();
        if (Vector2.Dot(relativeVelocity, relativeNormal) > 0)
        {
            return;
        }

        float e = Math.Min(a.CoefficientOfRestitution, b.CoefficientOfRestitution);
        float numerator = -(1f + e) * Vector2.Dot(relativeVelocity, relativeNormal);
        float j = numerator / invMassSum;
        if (collision.ContactPoints.Count() > 0 && j != 0)
        {
            j /= collision.ContactPoints.Count();
        }

        Vector2 impulse = relativeNormal * j;
        a.Velocity = a.Velocity + impulse * inverseMass1 * -1;
        b.Velocity = b.Velocity + impulse * inverseMass2;
    }

    /// <summary>
    /// Registers a <see cref="Rigidbody2D"/> object with the physics system.
    /// </summary>
    /// <param name="body"></param>
    public void RegisterRigidbody(Rigidbody2D body)
    {
        this.rigidBodies.Add(body);
        if (body.IsAffectedByGravity)
        {
            this.forceRegistry.Add(body, this.gravity);
        }
    }

    public void UnregisterRigidbody(uint id)
    {
        // TODO: this can be optimized significantly
        Rigidbody2D resolvedItem = null;
        for (int x = 0; x < this.rigidBodies.Count; x++)
        {
            if (this.rigidBodies[x].Id != id)
            {
                continue;
            }

            resolvedItem = this.rigidBodies[x];
            break;
        }

        if (resolvedItem == null)
        {
            return;
        }

        this.rigidBodies.Remove(resolvedItem);
    }
}