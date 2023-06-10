namespace ZxenLib.Entities.Components;

using System;
using System.Diagnostics;
using Interfaces;
using Microsoft.Xna.Framework;

/// <summary>
/// Defines the position component for entities.
/// </summary>
public class Transform : EntityComponent
{
    private Vector2 scale;
    private Vector2 position;
    private bool isDirty;
    private Angle angle;
    private Rectangle bounds;
    private Vector2 localPosition;
    private Transform? parentTransform;

    /// <summary>
    /// Initializes a new instance of the <see cref="Transform"/> class.
    /// </summary>
    public Transform()
    {
        this.Scale = Vector2.One;
        this.Size = Vector2.One;
        this.angle = new Angle();
        this.isDirty = true;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Transform"/> class with a provided parent.
    /// </summary>
    /// <param name="parent">The parent of this component.</param>
    public Transform(IEntity parent)
    {
        this.Scale = Vector2.One;
        this.Size = Vector2.One;
        this.angle = new Angle();
        this.Parent = parent;

        Transform? proposedParent = parent?.GetComponent<Transform>();
        if (proposedParent != null)
        {
            this.parentTransform = proposedParent;
        }

        this.isDirty = true;
    }

    /// <summary>
    /// Gets or sets the entity's <see cref="Vector2"/> position.
    /// </summary>
    public Vector2 Position
    {
        get => this.position;
        set
        {
            this.position = value;
            this.isDirty = true;
        }
    }

    /// <summary>
    /// Gets or sets the angle of this transform component.
    /// </summary>
    public Angle Angle
    {
        get => this.angle;
        set
        {
            this.angle = value;
            this.isDirty = true;
        }
    }

    /// <summary>
    /// Gets or sets the scale of object. Used in GameObject bounds calculations.
    /// </summary>
    public Vector2 Scale
    {
        get => this.scale;
        set
        {
            Debug.Assert(value.X > 0, "Scale value X must be greater than 0.");
            Debug.Assert(value.Y > 0, "Scale value Y must be greater than 0.");
            this.scale = value;
        }
    }

    /// <summary>
    /// Gets or sets the size of the object. Used in bounds calculations.
    /// </summary>
    public Vector2 Size { get; set; }

    /// <summary>
    /// Gets the bounds of the object surrounding its position.
    /// </summary>
    public Rectangle Bounds
    {
        get
        {
            if (this.isDirty)
            {
                this.RecalculateBounds();
            }

            return this.bounds;
        }
    }

    /// <summary>
    /// Gets or sets the velocity of this transform.
    /// </summary>
    [Obsolete("This will be removed in the near future. If you need velocity, use the ZxPhysics2D system.")]
    public Vector2 Velocity { get; set; }

    /// <summary>
    /// Gets the current <see cref="Transform"/> of the passed entity.
    /// If one is not found, it will register a new one and return it to the caller.
    /// </summary>
    /// <param name="parent">The parent <see cref="IEntity"/> of the <see cref="Transform"/></param>
    /// <returns>Existing <see cref="Transform"/> if one is found, or a new instance if not.</returns>
    public static Transform GetOrAddTransform(IEntity parent)
    {
        Transform? parentTransform = parent.GetComponent<Transform>();
        if (parentTransform != null)
        {
            return parentTransform;
        }

        parentTransform = new Transform(parent);
        parent.RegisterComponent(parentTransform);

        return parentTransform;
    }

    /// <summary>
    /// Adds passed vector to object's position.
    /// </summary>
    /// <param name="vector">The coordinates to append to the position.</param>
    public void Translate(Vector2 vector)
    {
        this.Position += vector;
    }

    /// <summary>
    /// Adds passed vector to object's velocity.
    /// </summary>
    /// <param name="velocityToAdd">The velocity to be appended.</param>
    public void AddVelocity(Vector2 velocityToAdd)
    {
        this.Velocity += velocityToAdd;
    }

    /// <summary>
    /// Rotates the transform by the provided degrees.
    /// </summary>
    /// <param name="degreesToRotate">Degrees to rotate.</param>
    public void Rotate(float degreesToRotate)
    {
        this.Angle.Rotate(degreesToRotate);
    }

    /// <summary>
    /// Directly sets object's rotation in degrees.
    /// </summary>
    /// <param name="degreeRotation">The rotation to set.</param>
    public void SetRotation(float degreeRotation)
    {
        this.Angle.SetRotation(degreeRotation);
    }

    /// <summary>
    /// Adds passed vector to object's scale.
    /// </summary>
    /// <param name="scale">The amount to scale the object.</param>
    public void ScaleTransform(Vector2 scale)
    {
        this.Scale += scale;
    }

    /// <summary>
    /// Transforms a vector from local space to world space.
    /// </summary>
    /// <param name="vectorToTransform"></param>
    /// <returns></returns>
    public Vector2 TransformLocalVectorToWorld(Vector2 vectorToTransform)
    {
        // Create a rotation matrix from the object's rotation angle
        Matrix rotationMatrix = Matrix.CreateRotationZ(this.Angle.Radians);

        // Transform the local coordinates using the rotation matrix
        Vector2 rotatedCoordinates = Vector2.Transform(vectorToTransform, rotationMatrix);

        // Add the object's position to the rotated coordinates to get the world coordinates
        Vector2 worldCoordinates = rotatedCoordinates + this.position + (this.parentTransform?.position ?? Vector2.Zero);

        return worldCoordinates;
    }

    /// <summary>
    /// Registers the component with a parent entity.
    /// </summary>
    /// <param name="parent">The <see cref="IEntity"/> object parent of this <see cref="IEntityComponent"/>.</param>
    public virtual void Register(IEntity parent)
    {
        ArgumentNullException.ThrowIfNull(parent);

        if (this.Parent?.Id != parent.Id)
        {
            this.Parent = parent;
        }

        parent.RegisterComponent(this);
    }

    /// <summary>
    /// Unregisters the component from its parent.
    /// </summary>
    public virtual void Unregister()
    {
        this.Parent.UnregisterComponent(this.Id);
        this.Parent = null!;
    }

    private void RecalculateBounds()
    {
        this.bounds = new Rectangle(
            (int)(this.Position.X - (this.Size.X / 2 * this.Scale.X)),
            (int)(this.Position.Y - (this.Size.Y / 2 * this.Scale.Y)),
            (int)(this.Size.X * this.Scale.X),
            (int)(this.Size.Y * this.Scale.Y));

        this.isDirty = false;
    }
}