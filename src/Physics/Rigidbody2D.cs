namespace ZxenLib.Physics;

using System;
using Entities;
using Entities.Components;
using Extensions;
using Microsoft.Xna.Framework;

public class Rigidbody2D : EntityComponent
{
    private bool fixedRotation = false;
    private float angularVelocity = 0f;
    private float linearDampening = 0f;
    private float angularDampening = 0f;
    private float mass = 0f;
    private float inverseMass = 0f;
    private Vector2 accumulatedForce = new Vector2();
    private Vector2 lineaVelocity = new Vector2();
    private Vector2 position = new Vector2();
    private Angle rotation = new Angle();
    private Transform parentTransform;

    /// <summary>
    /// Creates a new instance of the <see cref="Rigidbody2D"/> class.
    /// </summary>
    public Rigidbody2D()
    {
        this.Id = Ids.GetNewId();
        this.IsEnabled = true;
    }

    /// <summary>
    /// Gets the local base rotation of this rigidbody, relative to its parent <see cref="Transform"/>.
    /// </summary>
    public Angle Rotation
    {
        get => this.rotation;
        set => this.rotation = value;
    }

    /// <summary>
    /// Gets or sets the local position of this rigidbody, relative to its parent <see cref="Transform"/>.
    /// </summary>
    public Vector2 Position
    {
        get => this.position;
        set => this.position = value;
    }

    /// <summary>
    /// Gets the world coordinate of this rigidbody.
    /// </summary>
    public Vector2 WorldPosition => this.parentTransform.Position + this.position;

    /// <summary>
    /// Flag indicating if this <see cref="Rigidbody2D"/> should have a fixed rotation.
    /// </summary>
    public bool FixedRotation
    {
        get => this.fixedRotation;
        set => this.fixedRotation = value;
    }

    /// <summary>
    /// Gets or sets the angular velocity.
    /// </summary>
    public float AngularVelocity
    {
        get => this.angularVelocity;
        set => this.angularVelocity = value;
    }

    /// <summary>
    /// Gets or sets the linear dampening value. This acts kind of like friction.
    /// </summary>
    public float LinearDampening
    {
        get => this.linearDampening;
        set => this.linearDampening = value;
    }

    /// <summary>
    /// Gets or sets the angular dampening. This acts kind of like friction.
    /// </summary>
    public float AngularDampening
    {
        get => this.angularDampening;
        set => this.angularDampening = value;
    }

    /// <summary>
    /// Gets the mass. When setting the mass, this will also calculate the inverse mass.
    /// </summary>
    public float Mass
    {
        get => this.mass;
        set
        {
            this.mass = value;
            this.inverseMass = value != 0f ? 1f / this.mass : 0f;
        }
    }

    /// <summary>
    /// Registers the component with a parent entity.
    /// </summary>
    /// <param name="parent">The <see cref="IEntity"/> object parent of this <see cref="IEntityComponent"/>.</param>
    public override void Register(IEntity parent)
    {
        ArgumentNullException.ThrowIfNull(parent);

        if (this.Parent?.Id != parent.Id)
        {
            this.Parent = parent;
        }

        parent.RegisterComponent(this);

        this.parentTransform = Transform.GetOrAddTransform(parent);
    }

    /// <summary>
    /// Performs update on the physics update loop.
    /// </summary>
    /// <param name="deltaTime"></param>
    public void PhysicsUpdate(float deltaTime)
    {
        if (this.mass == 0f)
        {
            return;
        }

        Vector2 acceleration = this.accumulatedForce * this.inverseMass;
        this.position += this.lineaVelocity * deltaTime;

        this.SyncTransform();
        this.ClearAccumulators();
    }

    private void SyncTransform()
    {
        this.parentTransform.Position = this.position;
    }

    /// <summary>
    /// Clears the accumulated force.
    /// </summary>
    public void ClearAccumulators()
    {
        this.accumulatedForce = new();
    }

    /// <summary>
    /// Adds a force to the accumulated force.
    /// </summary>
    /// <param name="force"></param>
    public void AddForce(Vector2 force)
    {
        this.accumulatedForce += force;
    }
}