namespace ZxenLib.UI
{
    using System;
    using System.Collections.Generic;
    using ZxenLib.UICore;

    /// <summary>
    /// Implements a wrapper that will show/hide a window and hold both the window itself and a data object for the window.
    /// </summary>
    public class WindowPresenter
    {
        private object dataObject;
        private List<IControl> controlsToAdd = new List<IControl>();

        /// <summary>
        /// Initializes a new instance of the <see cref="WindowPresenter"/> class.
        /// </summary>
        public WindowPresenter()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WindowPresenter"/> class.
        /// </summary>
        /// <param name="window">The window used for this <see cref="WindowPresenter"/> object.</param>
        public WindowPresenter(Window window)
        {
            this.Window = window;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WindowPresenter"/> class.
        /// </summary>
        /// <param name="window">The window used for this <see cref="WindowPresenter"/> object.</param>
        /// <param name="dataObject">The object used to provide data to the window.</param>
        public WindowPresenter(Window window, object dataObject)
        {
            this.Window = window;
            this.DataObject = dataObject;
        }

        /// <summary>
        /// Event signaling a dataobject binding change.
        /// </summary>
        protected event EventHandler<DataObjectChangedArgs> DataObjectChanged;

        /// <summary>
        /// Gets or sets the data object for this presenter.
        /// </summary>
        public object DataObject
        {
            get
            {
                return this.dataObject;
            }

            set
            {
                if (value != this.dataObject)
                {
                    object previousValue = this.dataObject;
                    this.dataObject = value;
                    this.OnDataObjectChanged(this, new DataObjectChangedArgs(previousValue, value));
                }
            }
        }

        /// <summary>
        /// Gets or sets the window object for this presenter.
        /// </summary>
        public Window Window { get; set; }

        /// <summary>
        /// Gets the parent UI Pane for this presenter.
        /// </summary>
        public IUIPane UiPane { get => this.Window?.ParentPane ?? null; }

        /// <summary>
        /// Gets or sets a value indicating whether the presentation has been built.
        /// </summary>
        public bool PresentationBuilt { get; protected set; }

        /// <summary>
        /// Builds the window presentation. This *must* be inherited for the window to display anything.
        /// </summary>
        public virtual void BuildPresentation()
        {
        }

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
            }
        }

        /// <summary>
        /// Determines if the current window is showing.
        /// </summary>
        /// <returns>True if window is showing/enabled.</returns>
        public bool IsShowing()
        {
            return this.Window?.Enabled ?? false;
        }

        /// <summary>
        /// Shows the window.
        /// </summary>
        public virtual void Show()
        {
            this.Window.Enabled = true;
        }

        /// <summary>
        /// Hides the window.
        /// </summary>
        public virtual void Hide()
        {
            this.Window.Enabled = false;
        }

        /// <summary>
        /// Binds the data object with a new object.
        /// </summary>
        /// <param name="dataObject">The new data object of the window.</param>
        public virtual void DataBind(object dataObject)
        {
            this.DataObject = dataObject;
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

        /// <summary>
        /// Called when the data object has been changed.
        /// </summary>
        /// <param name="sender">The sender object of the event.</param>
        /// <param name="args">Arguments for the event.</param>
        private void OnDataObjectChanged(object sender, DataObjectChangedArgs args)
        {
            this.DataObjectChanged?.Invoke(sender, args);
        }
    }
}
