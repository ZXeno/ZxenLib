namespace ZxenLib.Entities
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using ZxenLib.Entities.Components;
    using ZxenLib.Events;

    /// <summary>
    /// Base entity class for holding <see cref="IEntityComponent"/> objects.
    /// </summary>
    public class Entity : IEntity
    {
        /// <summary>
        /// Defines the programmatic id for the Entity Removed event.
        /// </summary>
        public const string EntityRemovedProgrammaticId = "EntityRemoved";

        /// <summary>
        /// Defines the programmatic id for the Entity Created event.
        /// </summary>
        public const string EntityCreatedProgrammaticId = "EntityCreated";

        private readonly IList<IEntityComponent> componentList = new List<IEntityComponent>();
        private readonly IEventDispatcher eventDispatcher;

        /// <summary>
        /// Initializes a new instance of the <see cref="Entity"/> class.
        /// </summary>
        public Entity(IEventDispatcher eventDispatcher)
        {
            this.Id = Guid.NewGuid().ToString();
            this.eventDispatcher = eventDispatcher;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Entity"/> class.
        /// </summary>
        /// <param name="componentCollection">The collection of components to initialize this <see cref="Entity"/> with.</param>
        public Entity(IEventDispatcher eventDispatcher, IEnumerable<IEntityComponent> componentCollection)
        {
            this.Id = Guid.NewGuid().ToString();
            this.eventDispatcher = eventDispatcher;

            if (componentCollection == null)
            {
                throw new ArgumentNullException(nameof(componentCollection));
            }

            foreach (IEntityComponent component in componentCollection)
            {
                this.componentList.Add(component);
            }
        }

        /// <summary>
        /// Gets a value indicating the ID of this Entity.
        /// </summary>
        public string Id { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether this Entity is enabled.
        /// </summary>
        public bool IsEnabled { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this Entity should be removed.
        /// </summary>
        public bool RemoveFlag { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this entity is initialized.
        /// </summary>
        public bool IsInitialized { get; set; }

        /// <summary>
        /// Initializes the entity.
        /// </summary>
        public virtual void Initialize()
        {
            this.IsEnabled = true;
            this.IsInitialized = true;
        }

        /// <summary>
        /// Gets a component by its type.
        /// </summary>
        /// <typeparam name="T">The component of type. </typeparam>
        /// <returns><see cref="IEntityComponent"/> of type <typeparamref name="T"/>.</returns>
        public T GetComponent<T>()
            where T : IEntityComponent
        {
            return (T)this.componentList.FirstOrDefault(x => x is T);
        }

        /// <summary>
        /// Gets all components of specified type.
        /// </summary>
        /// <typeparam name="T">The component of type. </typeparam>
        /// <returns><see cref="IEnumerable{T}"/>.</returns>
        public IEnumerable<T> GetComponentsOfType<T>()
            where T : IEntityComponent
        {
            return this.componentList.Where(x => x is T).Cast<T>();
        }

        /// <summary>
        /// Returns a component based on it's ID.
        /// </summary>
        /// <param name="componentId">The ID of the component to search for.</param>
        /// <returns><see cref="IEntityComponent"/>.</returns>
        public IEntityComponent GetComponentById(string componentId)
        {
            if (string.IsNullOrWhiteSpace(componentId))
            {
                throw new ArgumentNullException(nameof(componentId));
            }

            return this.componentList.FirstOrDefault(x => x.Id == componentId);
        }

        /// <summary>
        /// Registers a component with this entity.
        /// </summary>
        /// <param name="component">The <see cref="IEntityComponent"/> to register.</param>
        public void RegisterComponent(IEntityComponent component)
        {
            if (component == null)
            {
                throw new ArgumentNullException(nameof(component));
            }

            if (!this.componentList.Contains(component))
            {
                this.componentList.Add(component);

                if (component.Parent?.Id != this.Id)
                {
                    component.Register(this);
                }
            }
        }

        /// <summary>
        /// Unregisters a component from this entity.
        /// </summary>
        /// <param name="componentId">The ID of the component to unregister.</param>
        public void UnregisterComponent(string componentId)
        {
            if (string.IsNullOrWhiteSpace(componentId))
            {
                throw new ArgumentNullException(nameof(componentId));
            }

            IEntityComponent resolvedComponent = this.componentList.FirstOrDefault(x => x.Id == componentId);
            if (resolvedComponent != null)
            {
                this.componentList.Remove(resolvedComponent);
            }
        }

        /// <summary>
        /// Completely removes this entity from the entity system.
        /// </summary>
        public virtual void Destroy()
        {
            this.eventDispatcher.Publish(new EventData
            {
                EventId = Entity.EntityRemovedProgrammaticId,
                Sender = this,
                TargetObjectId = this.Id,
                EventArguments = EventArgs.Empty
            });

            this.IsEnabled = false;
        }
    }
}
