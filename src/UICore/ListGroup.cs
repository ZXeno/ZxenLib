namespace ZxenLib.UICore
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using ZxenLib.Input;

    /// <summary>
    /// Combines a group of <see cref="ListGroupRow"/>s
    /// </summary>
    public class ListGroup : IControl
    {
        private List<IControl> controlsToRemove;
        private List<IControl> controlsToAdd;

        /// <summary>
        /// Initializes a new instance of the <see cref="ListGroup"/> class.
        /// </summary>
        /// <param name="parent">The parent <see cref="IUIPane"/>.</param>
        /// <param name="position">The position of the control.</param>
        /// <param name="width">The width of the control</param>
        /// <param name="height">The height of the control.</param>
        /// <param name="controls">Optional parameters of child controls.</param>
        public ListGroup(IUIPane parent, Point position, int width, int height, params IControl[] controls)
        {
            this.ID = Guid.NewGuid();
            this.Name = nameof(Button) + this.ID.ToString();
            this.ParentPane = parent;
            this.Position = position;
            this.Width = width;
            this.Height = height;
            this.ChildControls = new List<IControl>();
            this.controlsToRemove = new List<IControl>();
            this.controlsToAdd = new List<IControl>();
            this.Enabled = true;

            if (controls != null && controls.Length > 0)
            {
                foreach (IControl control in controls)
                {
                    this.AddControl(control);
                }
            }
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
        /// Gets or sets the width.
        /// </summary>
        public int Width { get; set; }

        /// <summary>
        /// Gets or sets the height.
        /// </summary>
        public int Height { get; set; }

        /// <summary>
        /// Gets or sets the line height for the <see cref="ListGroup"/>. Default is 20
        /// </summary>
        public int RowHeight { get; set; } = 20;

        /// <summary>
        /// Gets or sets the anchor <see cref="Rectangle"/>.
        /// </summary>
        public Rectangle AnchorRect { get; set; }

        /// <summary>
        /// Gets or sets the padding for this control. Default is 4
        /// </summary>
        public int Padding { get; set; } = 4;

        /// <summary>
        /// Gets the list of child controls to this control.
        /// </summary>
        public IList<IControl> ChildControls { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether this control is enabled.
        /// </summary>
        public bool Enabled { get; set; }

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
            if (this.Enabled)
            {
                if (this.controlsToRemove.Count > 0)
                {
                    var cache = new List<IControl>(this.controlsToRemove);
                    foreach (IControl c in cache)
                    {
                        this.ChildControls.Remove(c);
                        this.controlsToRemove.Remove(c);
                    }
                }

                if (this.controlsToAdd.Count > 0)
                {
                    var cache = new List<IControl>(this.controlsToAdd);
                    foreach (IControl c in cache)
                    {
                        this.ChildControls.Add(c);
                        this.controlsToAdd.Remove(c);
                    }

                    this.RepositionChildControls();
                }

                foreach (IControl c in this.ChildControls)
                {
                    c.AnchorRect = this.Bounds;
                    c.Update(deltaTime);
                }
            }
        }

        /// <summary>
        /// Performs draw call batching for this control. Called every frame.
        /// </summary>
        /// <param name="sb">The <see cref="SpriteBatch"/> used for draw call batching.</param>
        public void Draw(SpriteBatch sb)
        {
            if (this.Enabled)
            {
                foreach (IControl c in this.ChildControls)
                {
                    c.Draw(sb);
                }
            }
        }

        /// <summary>
        /// Adds a control to the <see cref="ListGroup"/>.
        /// </summary>
        /// <param name="control">The <see cref="IControl"/> being added.</param>
        public void AddControl(IControl control)
        {
            if (control is ListGroupRow row)
            {
                this.controlsToAdd.Add(control);
            }
            else
            {
                this.controlsToAdd.Add(new ListGroupRow(this.ParentPane, control));
            }
        }

        /// <summary>
        /// Removes the control from the <see cref="ListGroup"/> during the next update.
        /// </summary>
        /// <param name="control">The <see cref="IControl"/> to remove.</param>
        public void RemoveControl(IControl control)
        {
            if (!this.controlsToRemove.Contains(control) && this.ChildControls.Contains(control))
            {
                this.controlsToRemove.Add(control);
            }
        }

        /// <summary>
        /// Removes the control from the <see cref="ListGroup"/> during the next update.
        /// </summary>
        /// <param name="controlID">The ID of the <see cref="IControl"/> to remove.</param>
        public void RemoveControl(string controlID)
        {
            if (string.IsNullOrWhiteSpace(controlID))
            {
                return;
            }

            IControl controlToRemove = this.ChildControls.FirstOrDefault(x => x.ID.ToString() == controlID);

            if (controlToRemove != null)
            {
                this.controlsToRemove.Add(controlToRemove);
            }
        }

        /// <summary>
        /// Repositions all child controls in this <see cref="ListGroup"/>
        /// </summary>
        public void RepositionChildControls()
        {
            this.EqualizeColumnsToLargest();
            this.RecalculateRowHeight();

            for (int i = 0; i < this.ChildControls.Count; i++)
            {
                IControl control = this.ChildControls[i];
                control.AnchorRect = this.Bounds;
                control.SetPosition(new Point(4, 4 + (this.RowHeight * i)));
            }
        }

        /// <summary>
        /// Clears all controls from this <see cref="ListGroup"/>
        /// </summary>
        public void Clear()
        {
            this.controlsToRemove.AddRange(this.ChildControls);
        }

        /// <summary>
        /// Sets the position of this listgroup.
        /// </summary>
        /// <param name="point">The new position.</param>
        public void SetPosition(Point point)
        {
            this.Position = point;
            this.RepositionChildControls();
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
        /// Recalculates the row height based on the tallest row.
        /// </summary>
        public void RecalculateRowHeight()
        {
            foreach (IControl control in this.ChildControls)
            {
                if (control is ListGroupRow row)
                {
                    row.RecalculateDimensions();

                    if (row.RowHeight > this.RowHeight)
                    {
                        this.RowHeight = row.GetRowHeight();
                    }
                }
            }
        }

        /// <summary>
        /// Equalizes the columns so they are all the same width.
        /// </summary>
        public void EqualizeColumnsToLargest()
        {
            Dictionary<int, int> columnWidths = new Dictionary<int, int>(this.ChildControls.Count);

            for (int i = 0; i < this.ChildControls.Count; i++)
            {
                if (this.ChildControls[i] is ListGroupRow row)
                {
                    int index = 0;
                    while (index < row.ColumnCount)
                    {
                        if (columnWidths.ContainsKey(index))
                        {
                            if (columnWidths[index] < row.GetColumnWidth(index))
                            {
                                columnWidths[index] = row.GetColumnWidth(index);
                            }
                        }
                        else
                        {
                            columnWidths.Add(index, row.GetColumnWidth(index));
                        }

                        index++;
                    }
                }
            }

            for (int x = 0; x < this.ChildControls.Count; x++)
            {
                if (this.ChildControls[x] is ListGroupRow row)
                {
                    row.NormalizeRowHeight();

                    for (int y = 0; y < row.ColumnCount; y++)
                    {
                        if (row[y] is ListGroupCell cell)
                        {
                            cell.Properties.Width = columnWidths[y];
                            cell.RepositionContent();
                        }
                    }

                    row.RepositionChildren();
                }
            }
        }
    }
}
