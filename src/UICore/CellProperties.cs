namespace ZxenLib.UICore
{
    using Microsoft.Xna.Framework;

    /// <summary>
    /// Defines the properties of a cell.
    /// </summary>
    public class CellProperties
    {
        /// <summary>
        /// Gets or sets the position of this control.
        /// </summary>
        public Point Position { get; set; }

        /// <summary>
        /// Gets or sets the width this cell should be. Defaults to 0.
        /// </summary>
        public int Width { get; set; } = 0;

        /// <summary>
        /// Gets or sets the height this cell should be. Defaults to 0.
        /// </summary>
        public int Height { get; set; } = 0;

        /// <summary>
        /// Gets or sets the default padding for this cell. Default value is 4.
        /// </summary>
        public int Padding { get; set; } = 4;

        /// <summary>
        /// Gets or sets a value indicating whether this cell with have its width determined automatically or set manually.
        /// </summary>
        public bool ManualWidth { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this cell with have its height determined automatically or set manually.
        /// </summary>
        public bool ManualHeight { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Alignment"/> value for the content of the cell. Defaults to <see cref="Alignment.Auto"/>
        /// </summary>
        public Alignment ContentAlignment { get; set; } = Alignment.Auto;

        /// <summary>
        /// Returns the Width/Height as a <see cref="Point"/>
        /// </summary>
        /// <returns>The dimensions of the cell properties.</returns>
        public Point GetDimensions()
        {
            return new Point(this.Width, this.Height);
        }
    }
}
