namespace ZxenLib.Entities.Components;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ZxenLib.Graphics;

/// <summary>
/// Defines a default drawable entity component with a single sprite.
/// </summary>
public class SpriteComponent : EntityComponent, IDrawableEntityComponent
{
    private readonly ISpriteManager spriteManager;
    private Atlas spriteAtlas;
    private Sprite sprite;
    private bool dirty;

    /// <summary>
    /// Initializes a new instance of the <see cref="SpriteComponent"/> class.
    /// </summary>
    /// <param name="spriteManager">Existing instance of the <see cref="ISpriteManager"/>.</param>
    /// <param name="atlasId">Atlas ID where the sprite can be found.</param>
    /// <param name="spriteId">Sprite ID for the sprite being assigned to this component.</param>
    public SpriteComponent(ISpriteManager spriteManager, string? atlasId = null, string? spriteId = null)
    {
        this.Id = Ids.GetNewId();
        this.IsEnabled = true;
        this.spriteManager = spriteManager;

        if (atlasId != null && spriteId != null)
        {
            this.spriteAtlas = this.spriteManager.GetAtlas(atlasId);
            this.sprite = this.spriteAtlas.GetSprite(spriteId);
            this.ShouldResizeTransform = true;
            this.dirty = true;
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SpriteComponent"/> class.
    /// </summary>
    /// <param name="spriteManager">Existing instance of the <see cref="ISpriteManager"/>.</param>
    /// <param name="parent">The parent <see cref="IEntity"/> for this component.</param>
    public SpriteComponent(ISpriteManager spriteManager, IEntity parent, string? atlasId = null, string? spriteId = null)
    {
        this.Id = Ids.GetNewId();
        this.IsEnabled = true;
        this.spriteManager = spriteManager;
        this.Parent = parent;

        if (atlasId != null && spriteId != null)
        {
            this.spriteAtlas = this.spriteManager.GetAtlas(atlasId);
            this.sprite = this.spriteAtlas.GetSprite(spriteId);
            this.ShouldResizeTransform = true;
            this.dirty = true;
        }
    }

    /// <summary>
    /// Gets or sets the sprite for this <see cref="SpriteComponent"/>.
    /// </summary>
    public Sprite Sprite
    {
        get => this.sprite;
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
    public TransformComponent? Transform { get; set; }

    /// <inheritdoc />
    public override void Register(IEntity parent)
    {
        base.Register(parent);

        this.Transform = this.Parent.GetComponent<TransformComponent>()!;

        if (this.ShouldResizeTransform)
        {
            this.Transform.Size = new Vector2(
                this.Sprite.SourceRect.Width,
                this.Sprite.SourceRect.Height);
        }
    }

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

        this.Clean();

        if (this.Sprite.Slice > 0)
        {
            GraphicsHelper.DrawBox(sb, this.spriteAtlas.TextureAtlas, this.Transform.Bounds, this.Sprite.SourceRect, this.Sprite.Slice, this.Sprite.DrawColor);
            return;
        }

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

    private void Clean()
    {
        if (!this.dirty)
        {
            return;
        }

        this.ResizeTransform(true);
        this.dirty = false;
    }

    private void OnSpriteChanged()
    {
        if (this.Transform == null)
        {
            this.Transform = this.Parent.GetComponent<TransformComponent>();
        }

        this.ResizeTransform();
    }

    private void ResizeTransform(bool forceOverride = false)
    {
        if (this.ShouldResizeTransform || forceOverride)
        {
            this.Transform.Size = new Vector2(
                this.Sprite.SourceRect.Width,
                this.Sprite.SourceRect.Height);
        }
    }
}