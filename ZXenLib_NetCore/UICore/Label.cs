namespace ZxenLib.UICore
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;

    /// <summary>
    /// A text control of the UI.
    /// </summary>
    public class Label : IControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Label"/> class.
        /// </summary>
        /// <param name="parent">The parent <see cref="IUIPane"/>.</param>
        /// <param name="font">The font used for this label.</param>
        /// <param name="txt">The starting text.</param>
        /// <param name="position">The position of this label.</param>
        /// <param name="anchorRect">The default anchored <see cref="Rectangle"/>.</param>
        public Label(IUIPane parent, SpriteFont font, string txt, Point position, Rectangle anchorRect)
        {
            this.ID = Guid.NewGuid();
            this.Name = nameof(Button) + this.ID.ToString();
            this.ParentPane = parent;
            this.Font = font;
            this.Text = txt;
            this.Position = position;
            this.AnchorRect = anchorRect;
            this.Enabled = true;
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
        /// Gets or sets the font used for this label.
        /// </summary>
        public SpriteFont Font { get; set; }

        /// <summary>
        /// Gets or sets the text of this label.
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// Gets or sets the position of this label.
        /// </summary>
        public Point Position { get; set; }

        /// <summary>
        /// Gets the list of child controls to this control.
        /// </summary>
        public IList<IControl> ChildControls { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether this label is enabled.
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// Gets or sets the anchro <see cref="Rectangle"/> of this control.
        /// </summary>
        public Rectangle AnchorRect { get; set; }

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
                    (int)this.Font.MeasureString(this.Text).X + 1,
                    (int)this.Font.MeasureString(this.Text).Y + 1);
            }
        }

        /// <summary>
        /// Handles updates for this control. Called every frame.
        /// </summary>
        /// <param name="deltaTime">The elapsed time of the previous frame.</param>
        public virtual void Update(float deltaTime)
        {
        }

        /// <summary>
        /// Performs draw call batching for this control. Called every frame.
        /// </summary>
        /// <param name="sb">The <see cref="SpriteBatch"/> used for batching draw calls.</param>
        public void Draw(SpriteBatch sb)
        {
            if (this.Enabled)
            {
                sb.DrawString(this.Font, this.Text, new Vector2(this.Bounds.X, this.Bounds.Y), Color.White);
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
        /// <returns>Always retuns false for <see cref="Label"/> type.</returns>
        public bool HitTest()
        {
            return false;
        }
    }
}
