namespace ZxenLib.Entities.Components.Interfaces;

/// <summary>
/// The interface representing an EntityComponent.
/// </summary>
public interface IEntityComponent
{
    /// <summary>
    /// Gets a value indicating the ID of this component.
    /// </summary>
    uint Id { get; }

    /// <summary>
    /// Gets or sets a value indicating whether the EntityComponent is enabled.
    /// </summary>
    bool IsEnabled { get; set; }

    /// <summary>
    /// Gets a value representing the parent of this component.
    /// </summary>
    IEntity Parent { get; }

    /// <summary>
    /// Registers the component with a parent entity.
    /// </summary>
    /// <param name="parent">The <see cref="IEntity"/> object parent of this <see cref="IEntityComponent"/>.</param>
    void Register(IEntity parent);

    /// <summary>
    /// Unregisters the component from its parent.
    /// </summary>
    void Unregister();
}