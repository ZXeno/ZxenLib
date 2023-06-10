﻿namespace ZxenLib.Entities;

using System;
using System.Collections.Generic;
using System.Linq;
using Components.Interfaces;
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

    private readonly IEventDispatcher eventDispatcher;
    private readonly IList<IEntity> children = new List<IEntity>();
    private readonly IList<IEntityComponent> componentList = new List<IEntityComponent>();

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
    public uint Id { get; protected set; }

    /// <inheritdoc />
    public bool IsEnabled { get; set; }

    /// <inheritdoc />
    public bool RemoveFlag { get; set; }

    /// <inheritdoc />
    public bool IsInitialized { get; private set; }

    /// <inheritdoc />
    public IEntity? Parent { get; set; }

    /// <inheritdoc />
    public virtual void Initialize()
    {
        this.Enable();
        this.IsInitialized = true;
    }

    /// <inheritdoc />
    public virtual IEnumerable<IEntity> GetChildren()
    {
        return this.children.ToArray();
    }

    /// <inheritdoc />
    public virtual IEntity? GetChild(uint childId)
    {
        for (int x = 0; x < this.children.Count; x++)
        {
            if (this.children[x].Id == childId)
            {
                return this.children[x];
            }
        }

        return null;
    }

    /// <inheritdoc />
    public virtual IEntity? GetFirstChild()
    {
        return this.children.Count > 0 ? this.children[0] : null;
    }

    /// <inheritdoc />
    public virtual IEntity? GetLastChild()
    {
        return this.children.Count > 0 ? this.children[this.children.Count - 1] : null;
    }

    /// <inheritdoc />
    public virtual void AddChild(IEntity child)
    {
        if (!this.children.Contains(child))
        {
            this.children.Add(child);
        }

        if (child.Parent?.Id == this.Id)
        {
            return;
        }

        child.Parent = this;
    }

    /// <inheritdoc />
    public virtual void RemoveChild(uint childId)
    {
        IEntity? resolvedEntity = null;
        for (int x = 0; x < this.componentList.Count; x++)
        {
            if (this.children[x].Id == childId)
            {
                resolvedEntity = this.children[x];
                break;
            }
        }

        if (resolvedEntity != null)
        {
            this.children.Remove(resolvedEntity);
            if (resolvedEntity.Parent?.Id == this.Id)
            {
                resolvedEntity.Parent = null;
            }
        }
    }

    /// <inheritdoc />
    public T? GetComponent<T>()
        where T : IEntityComponent
    {
        for (int x = 0; x < this.componentList.Count; x++)
        {
            if (this.componentList[x] is T resolvedType)
            {
                return resolvedType;
            }
        }

        return default;
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
        for (int x = 0; x < this.componentList.Count; x++)
        {
            if (this.componentList[x].Id == componentId)
            {
                return this.componentList[x];
            }
        }

        return null;
    }

    /// <inheritdoc />
    public void RegisterComponent(IEntityComponent component)
    {
        ArgumentNullException.ThrowIfNull(component);

        if (this.componentList.Contains(component))
        {
            return;
        }

        this.componentList.Add(component);

        if (component.Parent?.Id != this.Id)
        {
            component.Register(this);
        }
    }

    /// <inheritdoc />
    public void UnregisterComponent(uint componentId)
    {
        IEntityComponent? resolvedComponent = null;
        for (int x = 0; x < this.componentList.Count; x++)
        {
            if (this.componentList[x].Id == componentId)
            {
                resolvedComponent = this.componentList[x];
                break;
            }
        }

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