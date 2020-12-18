namespace ZxenLib.Infrastructure
{
    using System;
    using ZxenLib.Infrastructure.Exceptions;

    /// <summary>
    /// Represents a property in the configuration file.
    /// </summary>
    public class ConfigurationProperty
    {
        /// <summary>
        /// The name of the conifugration property.
        /// </summary>
        public string PropertyName { get; set; }

        /// <summary>
        /// String representing the property Type.
        /// </summary>
        public string PropertyType { get; set; }

        /// <summary>
        /// Object representing the value of the property.
        /// </summary>
        public object RawValue { get; set; }

        /// <summary>
        /// Gets the object as type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type being requested.</typeparam>
        /// <returns>Attempts to return the object as type <typeparamref name="T"/>.</returns>
        public T GetExpectedType<T>()
        {
            if (typeof(T).Name != this.PropertyType)
            {
                throw new ExpectedTypeMismatchException(typeof(T), this.PropertyName);
            }

            return (T)Convert.ChangeType(this.RawValue, typeof(T));
        }
    }
}
