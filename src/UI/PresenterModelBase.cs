namespace ZxenLib.UI
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// <see langword="Abstract"/> class for wrapping a presenter's required dependencies and data.
    /// Presenter models should only have a single constructor with all resolvable dependencies as parameters.
    /// </summary>
    public abstract class PresenterModelBase
    {
        /// <summary>
        /// Gets or sets the list of resolved presenter dependencies.
        /// </summary>
        public Dictionary<Type, object> PresenterDependencies { get; protected set; }
    }
}
