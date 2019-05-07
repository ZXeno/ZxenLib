namespace ZxenLib.UICore
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using ZxenLib.Graphics;
    using ZxenLib.Input;

    /// <summary>
    /// A button control of the UI.
    /// </summary>
    public class Button : IControl
    {
        private Texture2D texture;
        private Label label;
        private Color currentColor;

        /// <summary>
        /// Initializes a new instance of the <see cref="Button"/> class.
        /// </summary>
        /// <param name="parent">The parent <see cref="IUIPane"/>.</param>
        /// <param name="sprite">The <see cref="StarTraders.Graphics.Sprite"/> used for the button.</param>
        /// <param name="texture">The image used for the button.</param>
        /// <param name="position">The position of the button.</param>
        /// <param name="font">The font used for the text of the button.</param>
        /// <param name="anchorRect">The anchor <see cref="Rectangle"/> of the button.</param>
        public Button(IUIPane parent, Sprite sprite, Texture2D texture, Point position, SpriteFont font, Rectangle anchorRect)
        {
            this.ID = Guid.NewGuid();
            this.Name = nameof(Button) + this.ID.ToString();
            this.ParentPane = parent;
            this.texture = texture;
            this.Position = position;
            this.DefaultColor = Color.White;
            this.HoverColor = Color.White;
            this.currentColor = this.DefaultColor;
            this.AnchorRect = anchorRect;
            this.Width = sprite.SourceRect.Width;
            this.Height = sprite.SourceRect.Height;
            this.CornerSize = sprite.Slice;
            this.Enabled = true;
            this.FixedWidth = false;
            this.ButtonTextAlignment = ButtonTextPosition.Center;
            this.Sprite = sprite;
            this.label = new Label(this.ParentPane, font, "Button", Point.Zero, anchorRect);
            this.label.AnchorRect = this.Bounds;
            this.ChildControls = new List<IControl>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Button"/> class.
        /// </summary>
        /// <param name="parent">The parent <see cref="IUIPane"/>.</param>
        /// <param name="sprite">The <see cref="StarTraders.Graphics.Sprite"/> used for the button.</param>
        /// <param name="texture">The image used for the button.</param>
        /// <param name="position">The position of the button.</param>
        /// <param name="font">The font used for the text of the button.</param>
        /// <param name="width">The width of the button.</param>
        /// <param name="height">The height of the button.</param>
        /// <param name="anchorRect">The anchor <see cref="Rectangle"/> of the button.</param>
        public Button(IUIPane parent, Sprite sprite, Texture2D texture, Point position, SpriteFont font, int width, int height, Rectangle anchorRect)
        {
            this.ID = Guid.NewGuid();
            this.Name = nameof(Button) + this.ID.ToString();
            this.ParentPane = parent;
            this.texture = texture;
            this.Position = position;
            this.DefaultColor = Color.White;
            this.HoverColor = Color.White;
            this.currentColor = this.DefaultColor;
            this.AnchorRect = anchorRect;
            this.CornerSize = sprite.Slice;
            this.Width = width;
            this.Height = height;
            this.Enabled = true;
            this.FixedWidth = true;
            this.ButtonTextAlignment = ButtonTextPosition.Center;
            this.Sprite = sprite;
            this.label = new Label(this.ParentPane, font, "Button", Point.Zero, anchorRect);
            this.label.AnchorRect = this.Bounds;
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
        /// Gets or sets the position.
        /// </summary>
        public Point Position { get; set; }

        /// <summary>
        /// Gets or sets the text position.
        /// </summary>
        public Point TextPosition { get; set; }

        /// <summary>
        /// Gets or sets the callback for this control.
        /// </summary>
        public Action<Button> Callback { get; set; }

        /// <summary>
        /// Gets or sets the hover color.
        /// </summary>
        public Color HoverColor { get; set; }

        /// <summary>
        /// Gets or sets the default color.
        /// </summary>
        public Color DefaultColor { get; set; }

        /// <summary>
        /// Gets or sets the sprite.
        /// </summary>
        public Sprite Sprite { get; set; }

        /// <summary>
        /// Gets or sets the anchor <see cref="Rectangle"/> for this control.
        /// </summary>
        public Rectangle AnchorRect { get; set; }

        /// <summary>
        /// Gets the list of child controls to this control.
        /// </summary>
        public IList<IControl> ChildControls { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether the background image should be stretched for the button.
        /// Defaults to true.
        /// </summary>
        public bool StretchBGImage { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether the control is enabled.
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// Gets or sets the corner size for the background image.
        /// </summary>
        public int CornerSize { get; set; }

        /// <summary>
        /// Gets or sets the width of the button.
        /// </summary>
        public int Width { get; set; }

        /// <summary>
        /// Gets or sets the height of the button.
        /// </summary>
        public int Height { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the button should be of a fixed width.
        /// </summary>
        public bool FixedWidth { get; set; }

        /// <summary>
        /// Gets or sets the padding for this control. Default is 4
        /// </summary>
        public int Padding { get; set; } = 4;

        /// <summary>
        /// Gets or sets the button text alignment.
        /// </summary>
        public ButtonTextPosition ButtonTextAlignment { get; set; }

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
        /// Sets the text for this control.
        /// </summary>
        /// <param name="text">Text to be set.</param>
        public void SetText(string text)
        {
            this.label.Text = text;

            if (string.IsNullOrWhiteSpace(text))
            {
                this.label.Enabled = false;

                return;
            }
            else if (!this.label.Enabled)
            {
                this.label.Enabled = true;
            }

            switch (this.ButtonTextAlignment)
            {
                case ButtonTextPosition.Left:
                    this.label.SetPosition(
                        new Point(
                            this.Bounds.Left - this.Padding - this.label.Bounds.Width,
                            (this.Bounds.Height / 2) - (this.label.Bounds.Height / 2)));
                    break;

                case ButtonTextPosition.Center:
                    if (!this.FixedWidth)
                    {
                        this.Width = (int)this.label.Font.MeasureString(this.label.Text).X + (this.Padding * 2);
                        this.Height = (int)this.label.Font.MeasureString(this.label.Text).Y + (this.Padding * 2);

                        this.label.AnchorRect = this.Bounds;

                        this.label.Position = new Point((this.Width / 2) - ((int)this.label.Font.MeasureString(this.label.Text).X / 2), 2 + (this.Height / 2) - ((int)this.label.Font.MeasureString(this.label.Text).Y / 2));
                    }
                    else
                    {
                        this.label.AnchorRect = this.Bounds;

                        this.label.Position = new Point((this.Width / 2) - ((int)this.label.Font.MeasureString(this.label.Text).X / 2), 2 + ((this.Height / 2) - ((int)this.label.Font.MeasureString(this.label.Text).Y / 2)));
                    }

                    break;

                case ButtonTextPosition.Right:
                    this.label.SetPosition(
                        new Point(
                            this.Bounds.Right + this.Padding,
                            (this.Bounds.Height / 2) - (this.label.Bounds.Height / 2)));
                    break;
            }
        }

        /// <summary>
        /// Sets the size for this button.
        /// </summary>
        /// <param name="width">The new width of the control.</param>
        /// <param name="height">The new height of the control.</param>
        public void SetSize(int width, int height)
        {
            this.Width = width;
            this.Height = height;

            if (!string.IsNullOrWhiteSpace(this.label.Text))
            {
                this.SetText(this.label.Text);
            }
        }

        /// <summary>
        /// Performs updates for this control. Called every frame.
        /// </summary>
        /// <param name="deltaTime">Elapsed frame time of the previous frame.</param>
        public void Update(float deltaTime)
        {
            if (this.Enabled)
            {
                this.label.AnchorRect = this.Bounds;

                if (this.Bounds.Contains(InputWrapper.MousePosition))
                {
                    this.currentColor = this.HoverColor;
                }
                else
                {
                    this.currentColor = this.DefaultColor;
                }

                if (!string.IsNullOrWhiteSpace(this.label.Text))
                {
                    this.SetText(this.label.Text);
                }

                this.HandleInput();
            }
        }

        /// <summary>
        /// Handles input for this control.
        /// </summary>
        public void HandleInput()
        {
            // Perform assigned delegate action
            if (InputWrapper.WindowIsActive && InputWrapper.GetButtonUp(0) && this.HitTest())
            {
                this.Callback?.Invoke(this);
            }
        }

        /// <summary>
        /// Performs draw call batching for this control. Called every frame.
        /// </summary>
        /// <param name="sb">The <see cref="SpriteBatch"/> used for draw call batching.</param>
        public void Draw(SpriteBatch sb)
        {
            if (!this.Enabled)
            {
                return;
            }

            if (this.Sprite != null)
            {
                if (this.StretchBGImage)
                {
                    SpriteHelper.DrawBox(sb, this.texture, this.Bounds, this.Sprite.SourceRect, this.CornerSize, this.currentColor);
                }
                else
                {
                    sb.Draw(
                    this.texture,
                    this.Bounds,
                    this.Sprite.SourceRect,
                    this.Sprite.DrawColor,
                    0, // Perhaps handle rotation later?
                    this.Sprite.Origin,
                    this.Sprite.SpriteEffects,
                    this.Sprite.Layer);
                }
            }

            this.label.Draw(sb);
        }

        /// <summary>
        /// Sets the position of this control.
        /// </summary>
        /// <param name="point">The <see cref="Point"/> position of this control.</param>
        public void SetPosition(Point point)
        {
            if (point == null)
            {
                return;
            }

            this.Position = point;

            if (!string.IsNullOrEmpty(this.label.Text))
            {
                this.SetText(this.label.Text);
            }
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
