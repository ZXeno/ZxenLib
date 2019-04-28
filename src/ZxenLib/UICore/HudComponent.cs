namespace ZxenLib.UICore
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Xna.Framework.Graphics;

    /// <summary>
    /// Abstract implementation of a hud component class.
    /// </summary>
    public abstract class HudComponent
    {
        private List<IControl> controlsToAdd = new List<IControl>();

        /// <summary>
        /// Gets the <see cref="IUIPane"/> for this <see cref="HudComponent"/>.
        /// </summary>
        public IUIPane UiPane { get; private set; } = new UIPane();

        /// <summary>
        /// Gets or sets a value indicating whether this hud component is initialized.
        /// </summary>
        public bool IsInitialized { get; protected set; }

        /// <summary>
        /// Initializes the hud component.
        /// </summary>
        public abstract void Initialize();

        /// <summary>
        /// Performs once-per-frame updates of the hud component.
        /// </summary>
        /// <param name="deltaTime">Elapsed frame time of the previous frame.</param>
        public virtual void Update(float deltaTime)
        {
            if (this.UiPane != null)
            {
                if (this.controlsToAdd.Count > 0)
                {
                    List<IControl> cache = new List<IControl>(this.controlsToAdd);
                    foreach (IControl controlToAdd in cache)
                    {
                        this.UiPane.ChildControls.Add(controlToAdd);
                        this.controlsToAdd.Remove(controlToAdd);
                    }
                }

                this.UiPane.Update(deltaTime);
            }
        }

        /// <summary>
        /// Performs draw call batching for this <see cref="IUIPane"/>. Called every frame.
        /// </summary>
        /// <param name="sb">The <see cref="SpriteBatch"/> used for draw call batching.</param>
        public virtual void Draw(SpriteBatch sb)
        {
            if (this.UiPane != null)
            {
                this.UiPane.Draw(sb);
            }
        }

        /// <summary>
        /// Calls the <see cref="IUIPane"/> of this <see cref="HudComponent"/> to determine if the mouse is over any visible child controls.
        /// </summary>
        /// <returns>True if mouse hovers a child control.</returns>
        public bool IsMouseOverUI()
        {
            return this.UiPane.IsMouseOverUI();
        }

        /// <summary>
        /// Adds a control the the pane's child controls.
        /// </summary>
        /// <param name="control">The new <see cref="IControl"/> to add to this pane.</param>
        public virtual void AddControl(IControl control)
        {
            if (control == null)
            {
                return;
            }

            // Should we consider making IControl objects have changable parent panes?
            // TODO: investigate
            this.controlsToAdd.Add(control);
        }
    }
}
