namespace ZxenLib.Infrastructure.Exceptions;

using System;

/// <summary>
/// Custom exception outlining an unsupported configuration value.
/// </summary>
internal class UnsupportedConfigValueException : Exception
{
    private const string DefaultMessage = "This type is not supported by the configuration sytem.";
    private const string FormattedDefaultMessage = "This type is not supported by the configuration sytem. Type: {0}";

    /// <summary>
    /// Initializes a new instance of the <see cref="UnsupportedConfigValueException"/> class.
    /// </summary>
    public UnsupportedConfigValueException()
        : base(message: DefaultMessage)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="UnsupportedConfigValueException"/> class.
    /// </summary>
    /// <param name="type">The unsupported type.</param>
    public UnsupportedConfigValueException(Type type)
        : base(message: string.Format(FormattedDefaultMessage, type.ToString()))
    {
        this.UnsupportedType = type;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="UnsupportedConfigValueException"/> class.
    /// </summary>
    /// <param name="message">Message about the exception.</param>
    /// <param name="type">The unsupported type.</param>
    public UnsupportedConfigValueException(string message, Type type)
        : base(message)
    {
        this.UnsupportedType = type;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="UnsupportedConfigValueException"/> class.
    /// </summary>
    /// <param name="message">Message about the exception.</param>
    /// <param name="type">The unsupported type.</param>
    /// <param name="innerException">Exception that cuaused this exception to be thrown.</param>
    public UnsupportedConfigValueException(string message, Type type, Exception innerException)
        : base(message, innerException)
    {
        this.UnsupportedType = type;
    }

    /// <summary>
    /// The type that is not supported by the configuration system.
    /// </summary>
    public Type UnsupportedType { get; private set; }
}