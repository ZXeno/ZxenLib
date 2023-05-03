namespace ZxenLib.Entities.Components;

using System.Diagnostics;
using Microsoft.Xna.Framework;

/// <summary>
/// Defines the position component for entities.
/// </summary>
public class TransformComponent : EntityComponent
{
    /// <summary>
    /// Defines the programmatic id of the <see cref="TransformComponent"/>.
    /// </summary>
    public const string TransformComponentProgrammaticId = "TransformComponent";
    private Vector2 scale;
    private Vector2 position;
    private bool isDirty;
    private Angle angle;
    private Rectangle bounds;

    /// <summary>
    /// Initializes a new instance of the <see cref="TransformComponent"/> class.
    /// </summary>
    public TransformComponent()
    {
        this.Scale = Vector2.One;
        this.Size = Vector2.One;
        this.Angle = new Angle();
        this.ProgrammaticId = TransformComponentProgrammaticId;
        this.isDirty = true;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TransformComponent"/> class.
    /// </summary>
    /// <param name="parent">The parent of this component.</param>
    public TransformComponent(IEntity parent)
    {
        this.Scale = Vector2.One;
        this.Size = Vector2.One;
        this.Angle = new Angle();
        this.Parent = parent;
        this.ProgrammaticId = TransformComponentProgrammaticId;
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
        get
        {
            return this.scale;
        }

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
    public Vector2 Velocity { get; set; }

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
    /// <param name="velocityToAdd">The volicity to be appended.</param>
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
    /// <param name="scale">The amoutn to scale the object.</param>
    public void ScaleTransform(Vector2 scale)
    {
        this.Scale += scale;
    }

    private void RecalculateBounds()
    {
        this.bounds = new Rectangle(
            (int)(this.Position.X - (this.Size.X / 2 * this.Scale.X)),
            (int)(this.Position.Y - (this.Size.Y / 2 * this.Scale.Y)),
            (int)(this.Size.X * this.Scale.X),
            (int)(this.Size.Y * this.Scale.Y));
    }
}