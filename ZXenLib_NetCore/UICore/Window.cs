namespace ZxenLib.UICore
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using Microsoft.Xna.Framework.Input;
    using ZxenLib.Graphics;
    using ZxenLib.Input;

    /// <summary>
    /// Defines a window used in the UI
    /// </summary>
    public class Window : IControl
    {
        private readonly Sprite bgImage;
        private readonly Sprite closeButtonSprite;
        private readonly Texture2D spriteSheet;
        private readonly Button closeButton;
        private readonly Label titleLabel;
        private readonly SpriteFont font;
        private int width;
        private int height;

        /// <summary>
        /// Initializes a new instance of the <see cref="Window"/> class.
        /// </summary>
        /// <param name="parent">The parent <see cref="IUIPane"/>.</param>
        /// <param name="bgSprite">The sprite used for this window.</param>
        /// <param name="closeButtonSprite">The close button sprite used for this window.</param>
        /// <param name="spriteSheet">The atlas used for window sprites.</param>
        /// <param name="font">The font used for text.</param>
        /// <param name="position"><see cref="Point"/> coordinate position.</param>
        /// <param name="windowtitle">The title of the window.</param>
        /// <param name="width">Window width.</param>
        /// <param name="height">Window height.</param>
        /// <param name="anchorRect">The anchor rectangle for this window.</param>
        public Window(IUIPane parent, Sprite bgSprite, Sprite closeButtonSprite, Texture2D spriteSheet, SpriteFont font, Point position, string windowtitle, int width, int height, Rectangle anchorRect)
        {
            this.ID = Guid.NewGuid();
            this.Name = nameof(Button) + this.ID.ToString();
            this.ParentPane = parent;

            // Set window parameters
            this.Width = width;
            this.Height = height;
            this.CornerSize = bgSprite.Slice;
            this.Position = position;
            this.Enabled = true;
            this.CanDrag = false;

            // Set window resources
            this.spriteSheet = spriteSheet;
            this.bgImage = bgSprite;
            this.font = font;

            // Establish child controls container and anchor
            this.ChildControls = new List<IControl>();
            this.AnchorRect = anchorRect;

            // Set window title
            string title = windowtitle;

            this.titleLabel = new Label(this.ParentPane, this.font, title, new Point((this.Width / 2) - (Convert.ToInt32(this.font.MeasureString(title).X) / 2), 2), anchorRect);

            // Set close button
            this.closeButtonSprite = closeButtonSprite;
            this.closeButton = new Button(this.ParentPane, this.closeButtonSprite, this.spriteSheet, new Point(this.Width - this.closeButtonSprite.SourceRect.Width, 0), this.font, anchorRect)
            {
                Callback = new Action<Button>(
                o =>
                {
                    this.OnWindowClosing(this, EventArgs.Empty);
                    this.Enabled = false;
                }),

                StretchBGImage = false,
                AnchorRect = this.Bounds
            };
            this.closeButton.SetText(string.Empty);
            this.closeButton.SetSize(closeButtonSprite.SourceRect.Width, closeButtonSprite.SourceRect.Height);

            this.WindowSizeChanged += this.RecalculateWindowControlsOnSizeChange;

            // Add close button and title to window
            this.AddChildControl(this.titleLabel);
            this.AddChildControl(this.closeButton);
        }

        /// <summary>
        /// Event for notifying the window size has changed.
        /// </summary>
        public event EventHandler WindowSizeChanged;

        /// <summary>
        /// Event for notifying the window size has changed.
        /// </summary>
        public event EventHandler WindowClosing;

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
        /// Gets or sets the anchor <see cref="Rectangle"/>
        /// </summary>
        public Rectangle AnchorRect { get; set; }

        /// <summary>
        /// Gets or sets the position.
        /// </summary>
        public Point Position { get; set; }

        /// <summary>
        /// Gets or sets the width.
        /// </summary>
        public int Width
        {
            get => this.width;
            set
            {
                if (this.width != value)
                {
                    this.width = value;
                    this.OnWindowSizeChanged(this, null);
                }
            }
        }

        /// <summary>
        /// Gets or sets the height.
        /// </summary>
        public int Height
        {
            get => this.height;
            set
            {
                if (this.height != value)
                {
                    this.height = value;
                    this.OnWindowSizeChanged(this, null);
                }
            }
        }

        /// <summary>
        /// Gets or sets the corner size of the sprite. Used for sprite slicing.
        /// </summary>
        public int CornerSize { get; set; }

        /// <summary>
        /// Gets the list of child controls to this control.
        /// </summary>
        public IList<IControl> ChildControls { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether this window is enabled.
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this window can be dragged.
        /// </summary>
        public bool CanDrag { get; set; }

        /// <summary>
        /// Gets a value indicating whether this window has mouse hover.
        /// </summary>
        public bool IsHovered { get; private set; }

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
        /// Fires the window size changed event.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e"><see cref="EventArgs"/> of the event.</param>
        public void OnWindowSizeChanged(object sender, EventArgs e)
        {
            this.WindowSizeChanged?.Invoke(sender, e);
        }

        /// <summary>
        /// Fires the window closing event.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e"><see cref="EventArgs"/> of the event.</param>
        public void OnWindowClosing(object sender, EventArgs e)
        {
            this.WindowClosing?.Invoke(sender, e);
        }

        /// <summary>
        /// Performs updates for this control. Called every frame.
        /// </summary>
        /// <param name="deltaTime">Elapsed frame time of the previous frame.</param>
        public void Update(float deltaTime)
        {
            this.IsHovered = false;

            if (this.Enabled)
            {
                this.HandleInput();

                if (this.ChildControls.Count > 0)
                {
                    // update each control, assign anchor
                    foreach (IControl ctrl in this.ChildControls)
                    {
                        ctrl.AnchorRect = this.Bounds;
                        ctrl.Update(deltaTime);
                    }
                }
            }
        }

        /// <summary>
        /// Handles input for this control.
        /// </summary>
        public void HandleInput()
        {
            if (!InputWrapper.WindowIsActive)
            {
                return;
            }

            if (this.Enabled)
            {
                // handle dragging
                if (this.CanDrag && this.HitTest())
                {
                    if (InputWrapper.GetButton(0) && InputWrapper.LastMouseState.LeftButton == ButtonState.Pressed)
                    {
                        if (InputWrapper.MousePosition != InputWrapper.LastMouseState.Position)
                        {
                            // set window position
                            this.Position = new Point(
                                this.Position.X + (InputWrapper.MousePosition.X - InputWrapper.LastMouseState.Position.X),
                                this.Position.Y + (InputWrapper.MousePosition.Y - InputWrapper.LastMouseState.Position.Y));
                        }
                    }
                }

                // check for hover
                if (!this.IsHovered && this.HitTest())
                {
                    this.IsHovered = true;
                }
            }
        }

        /// <summary>
        /// Performs draw call batching for this control. Called every frame.
        /// </summary>
        /// <param name="sb">The <see cref="SpriteBatch"/> used for draw call batching.</param>
        public void Draw(SpriteBatch sb)
        {
            // If enabled
            if (this.Enabled)
            {
                // Draw the Window background according to the slice layout
                SpriteHelper.DrawBox(sb, this.spriteSheet, this.Bounds, this.bgImage.SourceRect, this.CornerSize, Color.White);

                if (this.ChildControls.Count > 0)
                {
                    // Draw controls
                    foreach (IControl ctrl in this.ChildControls)
                    {
                        ctrl.Draw(sb);
                    }
                }
            }
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
        /// Adds a control as a child of this window.
        /// </summary>
        /// <param name="control">The control to be childed</param>
        public void AddChildControl(IControl control)
        {
            this.ChildControls.Add(control);
            control.AnchorRect = this.Bounds;
        }

        /// <summary>
        /// Removes a child control.
        /// </summary>
        /// <param name="control">The control to be removed.</param>
        public void RemoveChildControl(IControl control)
        {
            this.ChildControls.Remove(control);
            control.AnchorRect = new Rectangle(0, 0, int.MaxValue, int.MaxValue);
        }

        /// <summary>
        /// Enables all child controls of this window.
        /// </summary>
        public void EnableAllControls()
        {
            foreach (IControl ctrl in this.ChildControls)
            {
                ctrl.Enabled = true;
            }
        }

        /// <summary>
        /// Disables all child controls of this window.
        /// </summary>
        public void DisableAllControls()
        {
            foreach (IControl ctrl in this.ChildControls)
            {
                ctrl.Enabled = false;
            }
        }

        /// <summary>
        /// Sets the title text of this window.
        /// </summary>
        /// <param name="newtitle">The string value of the new title.</param>
        public void SetWindowTitle(string newtitle)
        {
            this.titleLabel.Text = newtitle;
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

        /// <summary>
        /// Handles window size changed events.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e"><see cref="EventArgs"/> of the event.</param>
        protected virtual void RecalculateWindowControlsOnSizeChange(object sender, EventArgs e)
        {
            this.titleLabel.SetPosition(new Point(((this.Width + this.bgImage.Slice) / 2) - (Convert.ToInt32(this.font.MeasureString(this.titleLabel.Text).X) / 2), this.bgImage.Slice + 4));
            this.closeButton.SetPosition(new Point(this.Width - this.closeButtonSprite.SourceRect.Width, 0));
        }
    }
}
