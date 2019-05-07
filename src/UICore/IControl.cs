namespace ZxenLib.UICore
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;

    /// <summary>
    /// Interface defining the implemenation of a UI Control.
    /// </summary>
    public interface IControl
    {
        /// <summary>
        /// Gets the system generated ID of the control.
        /// </summary>
        Guid ID { get; }

        /// <summary>
        /// Gets or sets the name of the control.
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// Gets the parent <see cref="IUIPane"/> of this control.
        /// </summary>
        IUIPane ParentPane { get; }

        /// <summary>
        /// Gets the list of child controls to this control.
        /// </summary>
        IList<IControl> ChildControls { get; }

        /// <summary>
        /// Gets or sets a value indicating whether this control is enabled.
        /// </summary>
        bool Enabled { get; set; }

        /// <summary>
        /// Gets or sets the anchor <see cref="Rectangle"/> for this control.
        /// </summary>
        Rectangle AnchorRect { get; set; }

        /// <summary>
        /// Gets the bounding <see cref="Rectangle"/> for this control.
        /// </summary>
        Rectangle Bounds { get; }

        /// <summary>
        /// Performs updates for this control. Called every frame.
        /// </summary>
        /// <param name="deltaTime">Elapsed frame time of the previous frame.</param>
        void Update(float deltaTime);

        /// <summary>
        /// Performs draw call batching for this control. Called every frame.
        /// </summary>
        /// <param name="sb">The <see cref="SpriteBatch"/> used for draw call batching.</param>
        void Draw(SpriteBatch sb);

        /// <summary>
        /// Handles input for this control.
        /// </summary>
        void HandleInput();

        /// <summary>
        /// Sets the position of this control.
        /// </summary>
        /// <param name="point">The <see cref="Point"/> position of this control.</param>
        void SetPosition(Point point);

        /// <summary>
        /// Gets a value indicating whether the control contains the cursor.
        /// </summary>
        /// <returns><see cref="bool"/></returns>
        bool HitTest();
    }
}
