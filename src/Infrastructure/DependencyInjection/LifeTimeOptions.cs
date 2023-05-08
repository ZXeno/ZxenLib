namespace ZxenLib.Infrastructure.DependencyInjection;

/// <summary>
/// Defines the lifetime options of the dependency container.
/// </summary>
public enum LifeTimeOptions
{
    /// <summary>
    /// Type instance lives only for as long as it is referred to.
    /// </summary>
    Transient,

    /// <summary>
    /// Type instance lives for the life of the Dependency Container
    /// </summary>
    ContainerControlled,
}