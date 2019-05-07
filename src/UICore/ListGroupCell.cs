namespace ZxenLib.UICore
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using ZxenLib.Input;

    /// <summary>
    /// Implementation of a <see cref="ListGroup"/>'s <see cref="ListGroupRow"/> cell.
    /// </summary>
    public class ListGroupCell : IControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ListGroupCell"/> class.
        /// </summary>
        public ListGroupCell()
        {
            this.ID = Guid.NewGuid();
            this.Name = "Cell_" + this.ID.ToString();
            this.Properties = new CellProperties();
            this.ChildControls = new List<IControl>();
            this.Enabled = true;
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
        /// Gets the list of child <see cref="IControl"/> elements.
        /// </summary>
        public IList<IControl> ChildControls { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether this control is enabled.
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="CellProperties"/> object of this <see cref="ListGroupCell"/>
        /// </summary>
        public CellProperties Properties { get; set; }

        /// <summary>
        /// Gets or sets the anchor <see cref="Rectangle"/>.
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
                   this.Properties.Position.X + this.AnchorRect.X,
                   this.Properties.Position.Y + this.AnchorRect.Y,
                   this.Properties.Width + (this.Properties.Padding * 2),
                   this.Properties.Height + (this.Properties.Padding * 2));
            }
        }

        /// <summary>
        /// Performs draw call batching for this control. Called every frame.
        /// </summary>
        /// <param name="sb">The <see cref="SpriteBatch"/> used for draw call batching.</param>
        public void Draw(SpriteBatch sb)
        {
            for (int i = 0; i < this.ChildControls.Count; i++)
            {
                this.ChildControls[i].Draw(sb);
            }
        }

        /// <summary>
        /// Handles input for this control.
        /// </summary>
        public void HandleInput()
        {
            for (int i = 0; i < this.ChildControls.Count; i++)
            {
                this.ChildControls[i].HandleInput();
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

        /// <summary>
        /// Sets the position of this control.
        /// </summary>
        /// <param name="point">The <see cref="Point"/> position of this control.</param>
        public void SetPosition(Point point)
        {
            this.Properties.Position = point;
        }

        /// <summary>
        /// Performs updates for this control. Called every frame.
        /// </summary>
        /// <param name="deltaTime">Elapsed frame time of the previous frame.</param>
        public void Update(float deltaTime)
        {
            for (int i = 0; i < this.ChildControls.Count; i++)
            {
                this.ChildControls[i].AnchorRect = this.Bounds;
                this.ChildControls[i].Update(deltaTime);
            }
        }

        /// <summary>
        /// Adds content controls to this cell.
        /// </summary>
        /// <param name="control">The <see cref="IControl"/> to add.</param>
        public void AddContentControl(params IControl[] control)
        {
            for (int i = 0; i < control.Length; i++)
            {
                if (control[i] != null)
                {
                    this.ChildControls.Add(control[i]);

                    if (!this.Properties.ManualWidth)
                    {
                        if (this.Properties.Width < control[i].Bounds.Width)
                        {
                            this.Properties.Width = control[i].Bounds.Width;
                        }
                    }

                    if (!this.Properties.ManualHeight)
                    {
                        this.Properties.Height += control[i].Bounds.Height;
                    }

                    control[i].AnchorRect = this.Bounds;
                }
            }
        }

        /// <summary>
        /// Repositions child content to fit the cell correctly.
        /// </summary>
        public void RepositionContent()
        {
            foreach (IControl control in this.ChildControls)
            {
                control.SetPosition(
                    new Point(
                        this.Properties.Padding,
                        (this.Bounds.Height / 2) - (control.Bounds.Height / 2)));
            }
        }
    }
}
