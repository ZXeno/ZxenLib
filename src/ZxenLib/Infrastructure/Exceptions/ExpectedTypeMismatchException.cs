namespace ZxenLib.Infrastructure.Exceptions;

using System;

/// <summary>
/// Custom exception outlining a type mismatch exception.
/// </summary>
public class ExpectedTypeMismatchException : Exception
{
    private const string ExpectedTypeMismatchDefaultMessage = "The expected type does not match the actual type.";
    private const string FormatedExpectedTypeMismatchDefaultMessage = "The expected type does not match the actual type. Expected: {0}, Actual: {1}";

    /// <summary>
    /// Initializes a new instance of the <see cref="ExpectedTypeMismatchException"/> class.
    /// </summary>
    public ExpectedTypeMismatchException()
        : base(ExpectedTypeMismatchDefaultMessage)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ExpectedTypeMismatchException"/> class.
    /// </summary>
    /// <param name="expectedType">The <see cref="Type"/> expected.</param>
    /// <param name="actualTypeName">The <see cref="string"/> type name of the actual type.</param>
    public ExpectedTypeMismatchException(Type expectedType, string actualTypeName)
        : base(string.Format(FormatedExpectedTypeMismatchDefaultMessage, expectedType.Name, actualTypeName))
    {
        this.ExpectedTypeName = expectedType.Name;
        this.ActualTypeName = actualTypeName;
    }

    /// <summary>
    /// The type name of the actual value.
    /// </summary>
    public string ActualTypeName { get; private set; }

    /// <summary>
    /// The expected type name of the value.
    /// </summary>
    public string ExpectedTypeName { get; private set; }
}