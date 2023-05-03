namespace ZxenLib.Entities.Components;

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ZxenLib.Graphics;

/// <summary>
/// Defines a default drawable entity component with a single sprite.
/// </summary>
public class SpriteComponent : EntityComponent, IDrawableEntityComponent
{
    /// <summary>
    /// Defines the programmatic id of the <see cref="SpriteComponent"/>.
    /// </summary>
    public const string SpriteComponentProgrammaticId = "SpriteComponent";

    private readonly ISpriteManager spriteManager;
    private Atlas spriteAtlas;
    private Sprite sprite;

    /// <summary>
    /// Initializes a new instance of the <see cref="SpriteComponent"/> class.
    /// </summary>
    /// <param name="spriteManager">Existing instance of the <see cref="ISpriteManager"/>.</param>
    public SpriteComponent(ISpriteManager spriteManager)
    {
        this.Id = Guid.NewGuid().ToString();
        this.IsEnabled = true;
        this.ProgrammaticId = SpriteComponentProgrammaticId;
        this.spriteManager = spriteManager;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SpriteComponent"/> class.
    /// </summary>
    /// <param name="spriteManager">Existing instance of the <see cref="ISpriteManager"/>.</param>
    /// <param name="parent">The parent <see cref="IEntity"/> for this component.</param>
    public SpriteComponent(ISpriteManager spriteManager, IEntity parent)
    {
        this.Id = Guid.NewGuid().ToString();
        this.IsEnabled = true;
        this.ProgrammaticId = SpriteComponentProgrammaticId;
        this.spriteManager = spriteManager;
        this.Parent = parent;
    }

    /// <summary>
    /// Gets or sets the sprite for this <see cref="SpriteComponent"/>.
    /// </summary>
    public Sprite Sprite
    {
        get
        {
            return this.sprite;
        }

        set
        {
            if (this.sprite?.Id != value.Id)
            {
                this.sprite = value;
                this.OnSpriteChanged();
            }
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether this <see cref="SpriteComponent"/> should resize the <see cref="Entity"/>'s <see cref="TransformComponent"/>.
    /// </summary>
    public bool ShouldResizeTransform { get; set; }

    /// <summary>
    /// Gets or sets the <see cref="TransformComponent"/> dependency. Used for sprite positioning.
    /// </summary>
    public TransformComponent Transform { get; set; }

    /// <summary>
    /// Method for batching draw calls. Called every frame.
    /// </summary>
    /// <param name="sb">The <see cref="SpriteBatch"/> for this component.</param>
    public void Draw(SpriteBatch sb)
    {
        if (!this.IsEnabled)
        {
            return;
        }

        if (this.Transform == null)
        {
            TransformComponent transformComponent = this.Parent.GetComponent<TransformComponent>();
            this.Transform = transformComponent ?? throw new NullReferenceException(nameof(this.Transform));
            if (this.ShouldResizeTransform)
            {
                this.Transform.Size = new Vector2(
                    this.Sprite.SourceRect.Width,
                    this.Sprite.SourceRect.Height);
            }
        }

        if (this.spriteAtlas == null)
        {
            this.spriteAtlas = this.spriteManager.GetAtlas(this.Sprite.ParentAtlas);
            if (this.spriteAtlas == null)
            {
                throw new NullReferenceException(nameof(this.spriteAtlas));
            }
        }

        if (this.Sprite.Slice > 0)
        {
            GraphicsHelper.DrawBox(sb, this.spriteAtlas.TextureAtlas, this.Transform.Bounds, this.Sprite.SourceRect, this.Sprite.Slice, this.Sprite.DrawColor);
        }
        else
        {
            sb.Draw(
                this.spriteAtlas.TextureAtlas,
                new Rectangle(
                    this.Transform.Bounds.X + (this.Sprite.SourceRect.Width / 2),
                    this.Transform.Bounds.Y + (this.Sprite.SourceRect.Height / 2),
                    this.Transform.Bounds.Width,
                    this.Transform.Bounds.Height),
                this.Sprite.SourceRect,
                this.Sprite.DrawColor,
                this.Transform.Angle.Radians + this.Sprite.RotationOffset,
                this.Sprite.Origin,
                this.Sprite.SpriteEffects,
                this.Sprite.Layer);
        }
    }

    private void OnSpriteChanged()
    {
        if (this.Transform == null)
        {
            this.Transform = this.Parent.GetComponent<TransformComponent>();
        }

        if (this.ShouldResizeTransform)
        {
            this.Transform.Size = new Vector2(
                this.Sprite.SourceRect.Width,
                this.Sprite.SourceRect.Height);
        }
    }
}