﻿namespace ZxenLib.Entities
{
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
        string Id { get; }

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
        void UnregisterComponent(string componentId);

        /// <summary>
        /// Gets a component by its type.
        /// </summary>
        /// <typeparam name="T">The component of type. </typeparam>
        /// <returns><see cref="IEntityComponent"/> of type <typeparamref name="T"/>.</returns>
        T GetComponent<T>()
            where T : IEntityComponent;

        /// <summary>
        /// Gets all components of type.
        /// </summary>
        /// <typeparam name="T">The component of type. </typeparam>
        /// <returns><see cref="IEntityComponent"/> of type <typeparamref name="T"/>.</returns>
        IEnumerable<T> GetComponentsOfType<T>()
            where T : IEntityComponent;

        /// <summary>
        /// Returns a component based on it's ID.
        /// </summary>
        /// <param name="componentId">The ID of the component to search for.</param>
        /// <returns><see cref="IEntityComponent"/>.</returns>
        IEntityComponent GetComponentById(string componentId);

        /// <summary>
        /// Completely removes this entity from the entity system.
        /// </summary>
        void Destroy();
    }
}
