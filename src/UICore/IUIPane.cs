namespace ZxenLib.UICore
{
    using System.Collections.Generic;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;

    /// <summary>
    /// Interface defining a UIPane
    /// </summary>
    public interface IUIPane
    {
        /// <summary>
        /// Gets the child controls of this <see cref="IUIPane"/>
        /// </summary>
        IList<IControl> ChildControls { get; }

        /// <summary>
        /// Gets or sets the rectangle representing the bounds of this UI Pane.
        /// </summary>
        Rectangle BoundingRectangle { get; set; }

        /// <summary>
        /// Performs updates for this <see cref="IUIPane"/>. Called every frame.
        /// </summary>
        /// <param name="deltaTime">Elapsed frame time of the previous frame.</param>
        void Update(float deltaTime);

        /// <summary>
        /// Performs draw call batching for this <see cref="IUIPane"/>. Called every frame.
        /// </summary>
        /// <param name="sb">The <see cref="SpriteBatch"/> used for draw call batching.</param>
        void Draw(SpriteBatch sb);

        /// <summary>
        /// Determines if the mouse is over any child element of this <see cref="IUIPane"/>.
        /// </summary>
        /// <returns><see cref="bool"/></returns>
        bool IsMouseOverUI();
    }
}
