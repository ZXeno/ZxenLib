namespace ZxenLib.UI
{
    using System;

    /// <summary>
    /// Arguments class for passing the DataObjectChanged event.
    /// </summary>
    public class DataObjectChangedArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DataObjectChangedArgs"/> class.
        /// </summary>
        public DataObjectChangedArgs()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DataObjectChangedArgs"/> class.
        /// </summary>
        /// <param name="oldObject">The previous data object.</param>
        /// <param name="newObject">The new data object.</param>
        public DataObjectChangedArgs(object oldObject, object newObject)
        {
            this.OldDataObject = oldObject;
            this.NewDataObject = newObject;
        }

        /// <summary>
        /// Gets or sets the old data object.
        /// </summary>
        public object OldDataObject { get; set; }

        /// <summary>
        /// Gets or sets the new data object.
        /// </summary>
        public object NewDataObject { get; set; }
    }
}
