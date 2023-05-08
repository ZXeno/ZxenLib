namespace ZxenLib.Infrastructure.Exceptions;

using System;
using System.Runtime.Serialization;

public class TypeResolverFailedException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TypeResolverFailedException"/> class.
    /// </summary>
    public TypeResolverFailedException() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="TypeResolverFailedException"/> class.
    /// </summary>
    /// <param name="message">Exception message.</param>
    public TypeResolverFailedException(string? message) : base(message) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="TypeResolverFailedException"/> class.
    /// </summary>
    /// <param name="message">Exception message.</param>
    /// <param name="innerException">Reference to the inner <see cref="Exception"/> that is the cause of this exception.</param>
    public TypeResolverFailedException(string? message, Exception? innerException) : base(message, innerException) { }
}