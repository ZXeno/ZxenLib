namespace ZxenLib.Physics;

using System;
using System.Collections.Generic;
using System.Linq;
using Components;
using Collisions;
using Dynamics;
using Events;
using Forces;
using Interfaces;
using Microsoft.Xna.Framework;

public class ZxPhysics2D
{
    private IEventDispatcher eventDispatcher;



    private ForceRegistry forceRegistry;
    private List<Rigidbody2D> rigidBodies;

    private World world;

    private JumpForce gravity;
    private float timeStep = 0f;
    private int velocityIterations = 6;
    private int positionIterations = 2;

    private float iterationTimer = 0;

    public ZxPhysics2D(IEventDispatcher eventDispatcher)
    {
        this.eventDispatcher = eventDispatcher;
        this.forceRegistry = new();
        this.rigidBodies = new();
        // this.bodies1 = new();
        // this.bodies2 = new();
        // this.collisions = new();
    }

    public void Initialize(Vector2 gravity, float fixedTimestep = 1f / 60f)
    {
        this.timeStep = fixedTimestep;
        this.gravity = new JumpForce(gravity);

        this.world = new World(gravity);
    }

    public void SetUpdateRate(float updateRate)
    {
        this.timeStep = updateRate;
    }

    public void SetGravity(Vector2 gravity)
    {
        if (this.gravity == null!)
        {
            this.gravity = new JumpForce(gravity);
            return;
        }

        this.gravity.SetForce(gravity);
    }

    /// <summary>
    /// Updates the physics simulation. This will only run as frequently as the configured fixed time step.
    /// </summary>
    /// <param name="deltaTime"></param>
    public void Update(float deltaTime)
    {
        // To make sure we resolve an appropriate number of physics steps,
        // we run this on a timer. Whenever we hit an iteration, we run
        // a physics step. As long as we are running faster than the configured
        // time step, we will run the physics checks (on avg) less than once per
        // frame. If we are running below the configured time step, our physics
        // simulation could potentially run more frequently than once per frame.
        this.iterationTimer += deltaTime;
        while (this.iterationTimer >= deltaTime)
        {
            this.FixedUpdate();
            this.iterationTimer -= this.timeStep;
        }

        this.iterationTimer = 0;
    }

    public void FixedUpdate()
    {
        // update forces
        this.forceRegistry.UpdateForces(this.timeStep);

        // Progress Simulation Step
        this.world.Step(this.timeStep, this.velocityIterations, this.positionIterations);

        // Sync
        foreach (Rigidbody2D body in this.rigidBodies)
        {
            body.PhysicsUpdate(this.timeStep);
        }
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