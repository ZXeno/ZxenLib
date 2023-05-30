namespace ZxenLib.Entities;

using System.Collections.Generic;
using ZxenLib.Entities.Components;

/// <summary>
/// Interface for type <see cref="Entity"/>.
/// </summary>
public interface IEntity
{
    /// <summary>
    /// Gets a value indicating the ID of this Entity.
    /// </summary>
    uint Id { get; }

    /// <summary>
    /// Gets or sets a value indicating whether this Entity is enabled.
    /// </summary>
    bool IsEnabled { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this Entity should be removed.
    /// </summary>
    bool RemoveFlag { get; set; }

    /// <summary>
    /// Gets a value indicating whether this entity is initialized.
    /// </summary>
    bool IsInitialized { get; }

    /// <summary>
    /// Gets or sets the parent of this entity. If this value is null, it is a root entity object.
    /// </summary>
    IEntity? Parent { get; set; }

    /// <summary>
    /// Initializes the entity.
    /// </summary>
    void Initialize();

    /// <summary>
    /// Registers a component with this entity.
    /// </summary>
    /// <param name="component">The <see cref="IEntityComponent"/> to register.</param>
    void RegisterComponent(IEntityComponent component);

    /// <summary>
    /// Unregisters a component from this entity.
    /// </summary>
    /// <param name="componentId">The ID of the component to unregister.</param>
    void UnregisterComponent(uint componentId);

    /// <summary>
    /// Gets a component by its type.
    /// </summary>
    /// <typeparam name="T">The component of type. </typeparam>
    /// <returns><see cref="IEntityComponent"/> of type <typeparamref name="T"/>.</returns>
    T? GetComponent<T>()
        where T : IEntityComponent;

    /// <summary>
    /// Gets all components of type.
    /// </summary>
    /// <typeparam name="T">The component of type. </typeparam>
    /// <returns><see cref="IEntityComponent"/> of type <typeparamref name="T"/>.</returns>
    IEnumerable<T>? GetComponentsOfType<T>()
        where T : IEntityComponent;

    /// <summary>
    /// Returns a component based on it's ID.
    /// </summary>
    /// <param name="componentId">The ID of the component to search for.</param>
    /// <returns><see cref="IEntityComponent"/>.</returns>
    IEntityComponent? GetComponentById(uint componentId);

    /// <summary>
    /// Enables the entity and all of its children and child components.
    /// </summary>
    void Enable();

    /// <summary>
    /// Disables the entity and all of its children and child components.
    /// </summary>
    void Disable();

    /// <summary>
    /// Completely removes this entity from the entity system.
    /// </summary>
    void Destroy();

    /// <summary>
    /// Gets all children of this entity.
    /// </summary>
    /// <returns>An array of the children of this entity.</returns>
    IEnumerable<IEntity> GetChildren();

    /// <summary>
    /// Gets the child with the specified ID.
    /// </summary>
    /// <param name="childId"></param>
    /// <returns></returns>
    IEntity? GetChild(uint childId);

    /// <summary>
    /// Gets the first child of this entity.
    /// </summary>
    /// <returns>The first <see cref="IEntity"/> child. It will return null if there are no children.</returns>
    IEntity? GetFirstChild();

    /// <summary>
    /// Gets the last child in the child collection of this entity.
    /// </summary>
    /// <returns>The last <see cref="IEntity"/> element in the children collection.</returns>
    IEntity? GetLastChild();

    /// <summary>
    /// Adds a new <see cref="IEntity"/> child.
    /// </summary>
    /// <param name="child"></param>
    void AddChild(IEntity child);

    /// <summary>
    /// Removes an <see cref="IEntity"/> from the children of this entity.
    /// </summary>
    /// <param name="childId"></param>
    void RemoveChild(uint childId);
}