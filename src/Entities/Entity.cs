namespace ZxenLib.Entities;

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
    /// <param name="eventDispatcher">Implementation of <see cref="IEventDispatcher"/> used for creation/removal entity events.</param>
    public Entity(IEventDispatcher eventDispatcher)
    {
        this.Id = Ids.GetNewId();
        this.eventDispatcher = eventDispatcher;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Entity"/> class.
    /// </summary>
    /// <param name="eventDispatcher">Implementation of <see cref="IEventDispatcher"/> used for creation/removal entity events.</param>
    /// <param name="componentCollection">The collection of components to initialize this <see cref="Entity"/> with.</param>
    public Entity(IEventDispatcher eventDispatcher, IEnumerable<IEntityComponent> componentCollection)
    {
        this.Id = Ids.GetNewId();
        this.eventDispatcher = eventDispatcher;

        if (componentCollection == null)
        {
            throw new ArgumentNullException(nameof(componentCollection));
        }

        foreach (IEntityComponent component in componentCollection)
        {
            this.componentList.Add(component);
            component.Register(this);
        }
    }

    /// <inheritdoc />
    public uint Id { get; private set; }

    /// <inheritdoc />
    public bool IsEnabled { get; set; }

    /// <inheritdoc />
    public bool RemoveFlag { get; set; }

    /// <inheritdoc />
    public bool IsInitialized { get; private set; }

    /// <inheritdoc />
    public virtual void Initialize()
    {
        this.Enable();
        this.IsInitialized = true;
    }

    /// <inheritdoc />
    public T? GetComponent<T>()
        where T : IEntityComponent
    {
        return (T?)this.componentList.FirstOrDefault(x => x is T);
    }

    /// <inheritdoc />
    public IEnumerable<T>? GetComponentsOfType<T>()
        where T : IEntityComponent
    {
        return this.componentList.Where(x => x is T)?.Cast<T>();
    }

    /// <inheritdoc />
    public IEntityComponent? GetComponentById(uint componentId)
    {
        return this.componentList.FirstOrDefault(x => x.Id == componentId);
    }

    /// <inheritdoc />
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

    /// <inheritdoc />
    public void UnregisterComponent(uint componentId)
    {
        IEntityComponent? resolvedComponent = this.componentList.FirstOrDefault(x => x.Id == componentId);
        if (resolvedComponent != null)
        {
            this.componentList.Remove(resolvedComponent);
        }
    }

    /// <inheritdoc />
    public void Enable()
    {
        this.SetEnabled(true);
    }

    /// <inheritdoc />
    public void Disable()
    {
        this.SetEnabled(false);
    }

    /// <inheritdoc />
    public virtual void Destroy()
    {
        this.eventDispatcher.Publish(new EventData
        {
            EventId = Entity.EntityRemovedProgrammaticId,
            Sender = this,
            TargetObjectId = this.Id,
            EventArguments = EventArgs.Empty
        });

        this.Disable();
    }

    private void SetEnabled(bool enabled)
    {
        foreach (IEntityComponent entityComponent in this.componentList)
        {
            entityComponent.IsEnabled = enabled;
        }
    }
}