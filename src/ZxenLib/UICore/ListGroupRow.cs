namespace ZxenLib.UICore
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using ZxenLib.Input;

    /// <summary>
    /// Implemenation of a ListGroupItem used by the ListGroup.
    /// </summary>
    public class ListGroupRow : IControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ListGroupRow"/> class.
        /// </summary>
        /// <param name="parent">The parent <see cref="IUIPane"/>.</param>
        /// <param name="childControls">Collection of child controls used by the <see cref="ListGroupRow"/></param>
        public ListGroupRow(IUIPane parent, params IControl[] childControls)
        {
            this.ID = Guid.NewGuid();
            this.Name = nameof(Button) + this.ID.ToString();
            this.ParentPane = parent;
            this.ChildControls = new List<IControl>();

            if (childControls != null && childControls.Length > 0)
            {
                this.AddRowContent(childControls);
                this.RecalculateDimensions();
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
        /// Gets the list of child <see cref="IControl"/> elements.
        /// </summary>
        public IList<IControl> ChildControls { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether this control is enabled.
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// Gets or sets the position of this control.
        /// </summary>
        public Point Position { get; set; }

        /// <summary>
        /// Gets or sets the anchor <see cref="Rectangle"/>.
        /// </summary>
        public Rectangle AnchorRect { get; set; }

        /// <summary>
        /// Gets or sets the column width.
        /// </summary>
        public int RowWidth { get; set; }

        /// <summary>
        /// Gets or sets the row height.
        /// </summary>
        public int RowHeight { get; set; }

        /// <summary>
        /// Gets the column count.
        /// </summary>
        public int ColumnCount { get => this.ChildControls.Count; }

        /// <summary>
        /// Gets or sets the padding for this control. Default is 4
        /// </summary>
        public int Padding { get; set; } = 4;

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
                    this.RowWidth,
                    this.RowHeight);
            }
        }

        /// <summary>
        /// Indexer for accessing child controls.
        /// </summary>
        /// <param name="index">The index of the child control.</param>
        /// <returns><see cref="IControl"/> contained in the <see cref="ChildControls"/> property</returns>
        public IControl this[int index]
        {
            get
            {
                if (index < this.ChildControls.Count)
                {
                    return this.ChildControls[index];
                }

                return null;
            }

            set
            {
                this.ChildControls[index] = value;
            }
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

        /// <summary>
        /// Adds a control to a cell and adds the cell to this row.
        /// </summary>
        /// <param name="control">The control(s) to add to this row.</param>
        public void AddRowContent(params IControl[] control)
        {
            if (control == null)
            {
                return;
            }

            for (int i = 0; i < control.Length; i++)
            {
                if (control[i] != null)
                {
                    if (control[i] is ListGroupCell lgc)
                    {
                        lgc.AnchorRect = this.Bounds;
                        this.AddCell(lgc);
                        continue;
                    }

                    ListGroupCell cell = new ListGroupCell();
                    cell.AnchorRect = this.Bounds;

                    if (i > 0 && this.ChildControls.Count >= 1
                        && this.ChildControls[i - 1] is ListGroupCell prevCell)
                    {
                        cell.Properties.Position = new Point(prevCell.Bounds.Right, 0);
                    }

                    cell.AddContentControl(control[i]);

                    this.AddCell(cell);
                }
            }
        }

        /// <summary>
        /// Adds an individual cell to the row.
        /// </summary>
        /// <param name="cell">The cell being added.</param>
        public void AddCell(ListGroupCell cell)
        {
            if (cell == null)
            {
                return;
            }

            if (this.ChildControls.Count > 1)
            {
                cell.Properties.Position = new Point(this.ChildControls.Last().Bounds.Right, 0);
            }

            this.ChildControls.Add(cell);
            this.RecalculateDimensions();
            cell.AnchorRect = this.Bounds;
        }

        /// <summary>
        /// Returns the width of the requested column.
        /// </summary>
        /// <param name="columnIndex">The zero-based index of the column requested</param>
        /// <returns>The <see cref="int"/> width of the column. -1 if the index is invalid.</returns>
        public int GetColumnWidth(int columnIndex)
        {
            if (this.ChildControls.Count > columnIndex)
            {
                return (this.ChildControls[columnIndex] as ListGroupCell).Bounds.Width;
            }

            return -1;
        }

        /// <summary>
        /// Gets the combined width of child controls in the row.
        /// </summary>
        /// <returns>The row width.</returns>
        public int GetRowWidth()
        {
            int width = 0;

            if (this.ChildControls.Count > 0)
            {
                foreach (var cell in this.ChildControls)
                {
                    width += (cell as ListGroupCell).Properties?.Width ?? 0;
                }
            }

            return width;
        }

        /// <summary>
        /// Gets the largest height of child controls in the row.
        /// </summary>
        /// <returns>The row height.</returns>
        public int GetRowHeight()
        {
            int height = 0;

            if (this.ChildControls.Count > 0)
            {
                foreach (var control in this.ChildControls)
                {
                    if (control is ListGroupCell cell
                        && cell.Properties.Height > height)
                    {
                        height = cell.Properties.Height;
                    }
                }
            }

            return height;
        }

        /// <summary>
        /// Recalculates the row's dimensions.
        /// </summary>
        public void RecalculateDimensions()
        {
            this.RowWidth = (this.Padding * 2) + this.GetRowWidth();
            this.RowHeight = (this.Padding * 2) + this.GetRowHeight();
        }

        /// <summary>
        /// Repositions child controls
        /// </summary>
        public void RepositionChildren()
        {
            for (int i = 0; i < this.ChildControls.Count; i++)
            {
                if (this.ChildControls[i] != null)
                {
                    if (i > 0 && this.ChildControls.Count >= 1
                        && this.ChildControls[i] is ListGroupCell thisCell
                        && this.ChildControls[i - 1] is ListGroupCell prevCell)
                    {
                        thisCell.SetPosition(new Point(prevCell.Bounds.Right, thisCell.Properties.Position.Y));
                    }
                }
            }
        }

        /// <summary>
        /// Normalizes all cells in this row to have the same height.
        /// </summary>
        public void NormalizeRowHeight()
        {
            this.RecalculateDimensions();

            foreach (IControl control in this.ChildControls)
            {
                if (control is ListGroupCell cell)
                {
                    cell.Properties.Height = this.RowHeight;
                }
            }
        }
    }
}
