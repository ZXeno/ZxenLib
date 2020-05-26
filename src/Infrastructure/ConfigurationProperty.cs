namespace ZxenLib.Infrastructure
{
    using System;
    using ZxenLib.Infrastructure.Exceptions;

    public class ConfigurationProperty
    {
        public string PropertyName { get; set; }
        public string PropertyType { get; set; }
        public object RawValue { get; set; }

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
