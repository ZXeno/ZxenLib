namespace ZxenLib.UICore
{
    using System.Collections.Generic;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using ZxenLib.Input;

    /// <summary>
    /// A container class for UI elements.
    /// </summary>
    public class UIPane : IUIPane
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UIPane"/> class.
        /// </summary>
        public UIPane()
        {
            this.ChildControls = new List<IControl>();
        }

        /// <summary>
        /// Gets the child controls of this <see cref="UIPane"/>
        /// </summary>
        public IList<IControl> ChildControls { get; private set; }

        /// <summary>
        /// Gets or sets the rectangle representing the bounds of this UI Pane.
        /// </summary>
        public Rectangle BoundingRectangle { get; set; }

        /// <summary>
        /// Performs updates for this <see cref="IUIPane"/>. Called every frame.
        /// </summary>
        /// <param name="deltaTime">Elapsed frame time of the previous frame.</param>
        public void Update(float deltaTime)
        {
            foreach (IControl control in this.ChildControls)
            {
                if (control.Enabled)
                {
                    control.Update(deltaTime);
                }
            }
        }

        /// <summary>
        /// Performs draw call batching for this <see cref="UIPane"/>. Called every frame.
        /// </summary>
        /// <param name="sb">The <see cref="SpriteBatch"/> used for draw call batching.</param>
        public void Draw(SpriteBatch sb)
        {
            foreach (IControl control in this.ChildControls)
            {
                if (control.Enabled)
                {
                    control.Draw(sb);
                }
            }
        }

        /// <summary>
        /// Determines if the mouse is over any child element of this <see cref="UIPane"/>.
        /// </summary>
        /// <returns><see cref="bool"/></returns>
        public bool IsMouseOverUI()
        {
            if (this.ChildControls.Count == 0 || !InputWrapper.WindowIsActive)
            {
                return false;
            }

            foreach (IControl control in this.ChildControls)
            {
                if (control.Enabled)
                {
                    if (control.HitTest())
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
