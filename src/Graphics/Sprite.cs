namespace ZxenLib.Graphics
{
    using System;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;

    /// <summary>
    /// Defines a sprite and it's properties.
    /// </summary>
    public class Sprite
    {
        private Rectangle destRect;

        private Vector2 origin;

        /// <summary>
        /// Initializes a new instance of the <see cref="Sprite"/> class.
        /// </summary>
        /// <param name="parentAtlasId">Id of the atlas this sprite belongs to.</param>
        /// <param name="spriteId">Id of this sprite.</param>
        /// <param name="sourceRectangle">The source rectangle on the atlas for this sprite.</param>
        public Sprite(string parentAtlasId, string spriteId, Rectangle sourceRectangle)
        {
            this.Id = Guid.NewGuid().ToString();
            this.Xoffset = 0;
            this.Yoffset = 0;
            this.RotationOffset = -1.5708f;
            this.ParentAtlas = parentAtlasId;
            this.SpriteId = spriteId;
            this.SourceRect = sourceRectangle;
            this.destRect = default(Rectangle);
            this.Center = new Vector2(.5f, .5f);
            this.DrawColor = Color.White;
            this.Layer = 0;
            this.SpriteEffects = SpriteEffects.None;
            this.Rotation = 0;
            this.OriginIsCenter = false;
            this.origin = new Vector2(0, 0);
        }

        /// <summary>
        /// Gets the unique identifier for this object.
        /// </summary>
        public string Id { get; private set; }

        /// <summary>
        /// Gets or sets the readable name for this sprite.
        /// </summary>
        public string SpriteId { get; set; }

        /// <summary>
        /// Gets or sets the X offset.
        /// </summary>
        public int Xoffset { get; set; }

        /// <summary>
        /// Gets or sets the Y offset.
        /// </summary>
        public int Yoffset { get; set; }

        /// <summary>
        /// Gets the parent atlas for this sprite.
        /// </summary>
        public string ParentAtlas { get; private set; }

        /// <summary>
        /// Gets the source rect on the atlas.
        /// </summary>
        public Rectangle SourceRect { get; private set; }

        /// <summary>
        /// Gets or sets the center value.
        /// </summary>
        public Vector2 Center { get; set; }

        /// <summary>
        /// Gets the draw layer defined for this sprite.
        /// </summary>
        public float Layer { get; private set; }

        /// <summary>
        /// Gets or sets the sprite effects.
        /// </summary>
        public SpriteEffects SpriteEffects { get; set; }

        /// <summary>
        /// Gets or sets the draw color.
        /// </summary>
        public Color DrawColor { get; set; }

        /// <summary>
        /// Gets or sets the rotation offset.
        /// </summary>
        public float RotationOffset { get; set; }

        /// <summary>
        /// Gets or sets the rotation.
        /// </summary>
        public float Rotation { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the center is the origin.
        /// </summary>
        public bool OriginIsCenter { get; set; }

        /// <summary>
        /// Gets or sets the sprite slicing size.
        /// </summary>
        public int Slice { get; set; }

        /// <summary>
        /// Gets or sets the destination <see cref="Rectangle"/>.
        /// </summary>
        public Rectangle DestinationRect
        {
            get => new Rectangle(
                this.destRect.X + this.Xoffset,
                this.destRect.Y + this.Yoffset,
                this.destRect.Width,
                this.destRect.Height);

            set => this.destRect = value;
        }

        /// <summary>
        ///  Gets or sets the origin. If Origin is center, returns center of sprite.
        /// </summary>
        public Vector2 Origin
        {
            get
            {
                if (this.OriginIsCenter)
                {
                    return new Vector2(
                        this.SourceRect.Width * this.Center.X,
                        this.SourceRect.Height * this.Center.Y);
                }

                return this.origin;
            }

            set => this.origin = value;
        }

        /// <summary>
        /// Set draw layer between 0f and 1f.
        /// </summary>
        /// <param name="layer">The desired layer for use.</param>
        public void SetDrawLayer(float layer)
        {
            this.Layer = MathHelper.Clamp(layer, 0, 1);
        }
    }
}
