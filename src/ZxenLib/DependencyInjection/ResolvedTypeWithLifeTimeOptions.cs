namespace ZxenLib.DependencyInjection;

using System;

/// <summary>
/// Wrapper class for the resolved type of the <see cref="DependencyContainer"/>
/// </summary>
public class ResolvedTypeWithLifeTimeOptions
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ResolvedTypeWithLifeTimeOptions"/> class.
    /// </summary>
    /// <param name="resolvedType">The <see cref="Type"/> to be resolved.</param>
    public ResolvedTypeWithLifeTimeOptions(Type resolvedType)
    {
        this.ResolvedType = resolvedType;
        this.LifeTimeOption = LifeTimeOptions.Transient;
        this.InstanceValue = null!;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ResolvedTypeWithLifeTimeOptions"/> class.
    /// </summary>
    /// <param name="resolvedType">The <see cref="Type"/> to be resolved.</param>
    /// <param name="lifeTimeOptions">The <see cref="LifeTimeOptions"/> of the type to be resolved.</param>
    public ResolvedTypeWithLifeTimeOptions(Type resolvedType, LifeTimeOptions lifeTimeOptions)
    {
        this.ResolvedType = resolvedType;
        this.LifeTimeOption = lifeTimeOptions;
        this.InstanceValue = null!;
    }

    /// <summary>
    /// Gets or sets the resolved type.
    /// </summary>
    public Type ResolvedType { get; set; }

    /// <summary>
    /// Gets or sets the lifetime options of the resolved type.
    /// </summary>
    public LifeTimeOptions LifeTimeOption { get; set; }

    /// <summary>
    /// Gets or sets the instance object of the resolved type.
    /// </summary>
    public object? InstanceValue { get; set; }
}