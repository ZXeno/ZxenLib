namespace ZxenLib.Infrastructure.Exceptions
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    internal class UnsupportedConfigValueException : Exception
    {
        public Type UnsupportedType { get; private set; }

        private const string DEFAULT_MESSAGE = "This type is not supported by the configuration sytem.";
        private const string FORMATTED_DEFAULT_MESSAGE = "This type is not supported by the configuration sytem. Type: {0}";

        public UnsupportedConfigValueException() : base(message: DEFAULT_MESSAGE)
        {
        }

        public UnsupportedConfigValueException(Type type): base(message: string.Format(FORMATTED_DEFAULT_MESSAGE, type.ToString()))
        {
            this.UnsupportedType = type;
        }

        public UnsupportedConfigValueException(string message, Type type) : base(message)
        {
            this.UnsupportedType = type;
        }

        public UnsupportedConfigValueException(string message, Type type, Exception innerException) : base(message, innerException)
        {
            this.UnsupportedType = type;
        }

        protected UnsupportedConfigValueException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
