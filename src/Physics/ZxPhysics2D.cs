namespace ZxenLib.Physics;

using System;
using System.Collections.Generic;
using Events;
using Forces;
using Microsoft.Xna.Framework;

public class ZxPhysics2D
{
    private IEventDispatcher eventDispatcher;
    private ForceRegistry forceRegistry;
    private Dictionary<uint, Rigidbody2D> rigidBodies;
    private Gravity2D gravity;
    private float updateRate = 0f;

    public ZxPhysics2D(IEventDispatcher eventDispatcher)
    {
        this.eventDispatcher = eventDispatcher;
        this.forceRegistry = new();
        this.rigidBodies = new();
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
        foreach (Rigidbody2D body in this.rigidBodies.Values)
        {
            body.PhysicsUpdate(this.updateRate);
        }
    }

    public void RegisterRigidbody(Rigidbody2D body)
    {
        this.rigidBodies.Add(body.Id, body);
    }

    public void UnregisterRigidbody(uint id)
    {
        this.rigidBodies.Remove(id);
    }
}