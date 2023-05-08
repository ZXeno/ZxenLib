namespace ZxenLib.Infrastructure.Exceptions;

using System;

public class TypeNotRegisteredException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TypeNotRegisteredException"/> class.
    /// </summary>
    public TypeNotRegisteredException() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="TypeNotRegisteredException"/> class.
    /// </summary>
    /// <param name="message">Exception message.</param>
    public TypeNotRegisteredException(string? message) : base(message) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="TypeNotRegisteredException"/> class.
    /// </summary>
    /// <param name="message">Exception message.</param>
    /// <param name="innerException">Reference to the inner <see cref="Exception"/> that is the cause of this exception.</param>
    public TypeNotRegisteredException(string? message, Exception? innerException) : base(message, innerException) { }
}