namespace ZxenLib.Physics.Components;

using System;
using Dynamics;
using Microsoft.Xna.Framework;
using ZxenLib.Entities;
using ZxenLib.Entities.Components;
using ZxenLib.Entities.Components.Interfaces;
using PhysicsTransform = ZxenLib.Physics.Common.Transform;
using Transform = Entities.Components.Transform;

public class Rigidbody2D : EntityComponent
{
    private bool isEnabled;
    private bool fixedRotation;
    private float angularVelocity;
    private float linearDampening;
    private float angularDampening;
    private float mass;
    private float inverseMass;
    private float coefficientOfRestitution = 1.0f;
    private Vector2 accumulatedForce;
    private Vector2 linearVelocity;
    private Vector2 position;
    private Transform parentTransform;
    private PhysicsTransform physTransform;
    private Body physicsBody;

    /// <summary>
    /// Creates a new instance of the <see cref="Rigidbody2D"/> class.
    /// </summary>
    public Rigidbody2D()
    {
        this.Id = Ids.GetNewId();
        this.isEnabled = true;
        this.physTransform = new();
    }

    /// <summary>
    /// Gets or sets the local position of this rigidbody, relative to its parent <see cref="Transform"/>.
    /// </summary>
    public PhysicsTransform PhysicsTransform
    {
        get => this.physTransform;
        set => this.physTransform = value;
    }

    public override bool IsEnabled
    {
        get => this.isEnabled;
        set
        {
            this.isEnabled = value;
            if (!value)
            {
                this.IsAwake = this.physicsBody.IsSleepingAllowed ? value : this.physicsBody.IsAwake;
            }
        }
    }

    /// <summary>
    /// Gets the parent transform of the <see cref="Rigidbody2D"/>.
    /// </summary>
    public Transform ParentTransform => this.parentTransform;

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
    /// Flag indicating if the simulation body is awake.
    /// </summary>
    public bool IsAwake
    {
        get => this.physicsBody.IsAwake;
        set
        {
            if (this.IsSleepingAllowed && value)
            {
                this.physicsBody.IsAwake = value;
            }
        }
    }

    /// <summary>
    /// Flag indicating if the simulation body is allowed to sleep.
    /// If false, the body cannot be put to sleep.
    /// </summary>
    public bool IsSleepingAllowed
    {
        get => this.physicsBody.IsSleepingAllowed;
        set => this.physicsBody.IsSleepingAllowed = value;
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
    /// Flag indicating if the <see cref="Rigidbody2D"/> has infinite mass.<br/>
    /// A rigidbody is considered to have infinite mass if its mass is 0.
    /// </summary>
    public bool IsInfiniteMass => this.mass == 0;

    /// <summary>
    /// Gets the inverse mass of this <see cref="Rigidbody2D"/>.
    /// </summary>
    public float InverseMass => this.inverseMass;

    /// <summary>
    /// Gets or sets the (linear) velocity of this <see cref="Rigidbody2D"/>.
    /// </summary>
    public Vector2 Velocity
    {
        get => this.linearVelocity;
        set => this.linearVelocity = value;
    }

    /// <summary>
    /// Gets or sets the coefficient of restitution for this <see cref="Rigidbody2D"/>.
    /// </summary>
    public float CoefficientOfRestitution
    {
        get => this.coefficientOfRestitution;
        set => this.coefficientOfRestitution = value;
    }

    /// <summary>
    /// Flag indicating if this <see cref="Rigidbody2D"/> is affected by gravity.
    /// </summary>
    public bool IsAffectedByGravity { get; set; }

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
        this.position += this.linearVelocity * deltaTime;

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