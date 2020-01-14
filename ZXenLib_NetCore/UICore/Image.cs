namespace ZxenLib.UICore
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using ZxenLib.Graphics;
    using ZxenLib.Input;

    /// <summary>
    /// An Image control for the UI.
    /// </summary>
    internal class Image : IControl
    {
        private Texture2D spriteTexture;

        /// <summary>
        /// Initializes a new instance of the <see cref="Image"/> class.
        /// </summary>
        /// <param name="parent">The parent <see cref="IUIPane"/></param>
        /// <param name="sprite">The sprite drawn by this control.</param>
        /// <param name="texture">The texture to be drawn</param>
        /// <param name="position">The position.</param>
        /// <param name="anchorRect">The anchor <see cref="Rectangle"/></param>
        public Image(IUIPane parent, Sprite sprite, Texture2D texture, Point position, Rectangle anchorRect)
        {
            this.ParentPane = parent;
            this.Sprite = sprite;
            this.Position = position;
            this.Width = sprite.SourceRect.Width;
            this.Height = sprite.SourceRect.Height;
            this.CornerSize = sprite.Slice;
            this.AnchorRect = anchorRect;
            this.Enabled = true;
            this.spriteTexture = texture;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Image"/> class.
        /// </summary>
        /// <param name="parent">The parent <see cref="IUIPane"/>.</param>
        /// <param name="sprite">The sprite drawn by this control.</param>
        /// <param name="texture">The texture to be drawn</param>
        /// <param name="position">The position.</param>
        /// <param name="width">The width of this control.</param>
        /// <param name="height">The height of this control.</param>
        /// <param name="anchorRect">The anchor <see cref="Rectangle"/></param>
        public Image(IUIPane parent, Sprite sprite, Texture2D texture, Point position, int width, int height, Rectangle anchorRect)
        {
            this.ID = Guid.NewGuid();
            this.Name = nameof(Button) + this.ID.ToString();
            this.ParentPane = parent;
            this.Sprite = sprite;
            this.Position = position;
            this.Width = width;
            this.Height = height;
            this.CornerSize = 0;
            this.AnchorRect = anchorRect;
            this.Enabled = true;
            this.spriteTexture = texture;
            this.ChildControls = new List<IControl>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Image"/> class.
        /// </summary>
        /// <param name="parent">The parent <see cref="IUIPane"/>.</param>
        /// <param name="sprite">The sprite drawn by this control.</param>
        /// <param name="texture">The texture to be drawn</param>
        /// <param name="position">The position.</param>
        /// <param name="width">The width of this control.</param>
        /// <param name="height">The height of this control.</param>
        /// <param name="corners">The size of the corner slices of this control.</param>
        /// <param name="anchorRect">The anchor <see cref="Rectangle"/></param>
        public Image(IUIPane parent, Sprite sprite, Texture2D texture, Point position, int width, int height, int corners, Rectangle anchorRect)
        {
            this.ID = Guid.NewGuid();
            this.Name = nameof(Button) + this.ID.ToString();
            this.ParentPane = parent;
            this.Sprite = sprite;
            this.Position = position;
            this.Width = width;
            this.Height = height;
            this.CornerSize = corners;
            this.AnchorRect = anchorRect;
            this.Enabled = true;
            this.spriteTexture = texture;
            this.ChildControls = new List<IControl>();
        }

        /// <summary>
        /// Gets the system generated ID of the control.
        /// </summary>
        public Guid ID { get; }

        /// <summary>
        /// Gets or sets the name of the control.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets the parent <see cref="IUIPane"/> of this control.
        /// </summary>
        public IUIPane ParentPane { get; private set; }

        /// <summary>
        /// Gets or sets the sprite to be drawn by this image control.
        /// </summary>
        public Sprite Sprite { get; set; }

        /// <summary>
        /// Gets or sets the position of this control.
        /// </summary>
        public Point Position { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this control is enabled.
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// Gets or sets the anchor <see cref="Rectangle"/>
        /// </summary>
        public Rectangle AnchorRect { get; set; }

        /// <summary>
        /// Gets the list of child controls to this control.
        /// </summary>
        public IList<IControl> ChildControls { get; private set; }

        /// <summary>
        /// Gets or sets the width.
        /// </summary>
        public int Width { get; set; }

        /// <summary>
        /// Gets or sets the height.
        /// </summary>
        public int Height { get; set; }

        /// <summary>
        /// Gets or sets the corner size used for slicing the image.
        /// </summary>
        public int CornerSize { get; set; }

        /// <summary>
        /// Gets the bounds of this control.
        /// </summary>
        public Rectangle Bounds
        {
            get
            {
                return new Rectangle(
                    this.Position.X + this.AnchorRect.X,
                    this.Position.Y + this.AnchorRect.Y,
                    this.Width,
                    this.Height);
            }
        }

        /// <summary>
        /// Performs updates for this control. Called every frame.
        /// </summary>
        /// <param name="deltaTime">Elapsed frame time of the previous frame.</param>
        public void Update(float deltaTime)
        {
        }

        /// <summary>
        /// Performs draw call batching for this control. Called every frame.
        /// </summary>
        /// <param name="sb">The <see cref="SpriteBatch"/> used for draw call batching.</param>
        public void Draw(SpriteBatch sb)
        {
            if (this.CornerSize > 0)
            {
                SpriteHelper.DrawBox(sb, this.spriteTexture, this.Bounds, this.Sprite.SourceRect, this.CornerSize, Color.White);
            }
            else
            {
                sb.Draw(
                    this.spriteTexture,
                    this.Bounds,
                    this.Sprite.SourceRect,
                    this.Sprite.DrawColor,
                    0, // Perhaps handle rotation later?
                    this.Sprite.Origin,
                    this.Sprite.SpriteEffects,
                    this.Sprite.Layer);
            }
        }

        /// <summary>
        /// Handles input for this control.
        /// </summary>
        public void HandleInput()
        {
        }

        /// <summary>
        /// Sets the position of this control.
        /// </summary>
        /// <param name="point">The <see cref="Point"/> position of this control.</param>
        public void SetPosition(Point point)
        {
            this.Position = point;
        }

        /// <summary>
        /// Gets a value indicating whether the control contains the cursor.
        /// </summary>
        /// <returns><see cref="bool"/></returns>
        public bool HitTest()
        {
            if (this.Bounds.Contains(InputWrapper.MousePosition))
            {
                return true;
            }

            return false;
        }
    }
}
