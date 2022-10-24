namespace ZxenLib.Entities
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Runtime.Serialization.Formatters.Binary;
    using Microsoft.Xna.Framework.Graphics;
    using ZxenLib.Entities.Components;
    using ZxenLib.Events;

    /// <summary>
    /// Used to store all objects of type <see cref="IEntity"/> and calls all Update and Draw functions for all <see cref="IEntityComponent"/> objects that implement <see cref="IUpdatableEntityComponent"/> and <see cref="IDrawableEntityComponent"/>.
    /// </summary>
    public class EntityManager : IEntityManager
    {
        private readonly List<IEntity> disabledEntitiesList;
        private readonly List<IEntity> allEntities;
        private readonly List<IEntity> toEnable;
        private readonly List<IEntity> toDisable;
        private readonly List<IEntity> toRemove;
        private readonly IEventDispatcher eventDispatcher;

        private readonly List<IUpdatableEntityComponent> updatableComponents;
        private readonly List<IDrawableEntityComponent> drawableEntityComponents;

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityManager"/> class.
        /// </summary>
        public EntityManager(IEventDispatcher eventDispatcher)
        {
            this.disabledEntitiesList = new List<IEntity>();
            this.allEntities = new List<IEntity>();
            this.toEnable = new List<IEntity>();
            this.toDisable = new List<IEntity>();
            this.toRemove = new List<IEntity>();
            this.updatableComponents = new List<IUpdatableEntityComponent>();
            this.drawableEntityComponents = new List<IDrawableEntityComponent>();

            this.eventDispatcher = eventDispatcher;
        }

        /// <summary>
        /// Adds a new <see cref="Entity"/> to the EntityManager.
        /// </summary>
        /// <param name="newEntity">The <see cref="Entity"/> to add.</param>
        public void AddNewEntity(IEntity newEntity)
        {
            if (newEntity == null)
            {
                throw new ArgumentNullException(nameof(newEntity));
            }

            if (!newEntity.IsEnabled)
            {
                newEntity.IsEnabled = true;
            }

            this.allEntities.Add(newEntity);

            this.updatableComponents.AddRange(newEntity.GetComponentsOfType<IUpdatableEntityComponent>());
            this.drawableEntityComponents.AddRange(newEntity.GetComponentsOfType<IDrawableEntityComponent>());

            if (!newEntity.IsInitialized)
            {
                newEntity.Initialize();
            }

            this.eventDispatcher.Publish(new EventData()
            {
                EventId = Entity.EntityCreatedProgrammaticId,
                Sender = this,
                TargetObjectId = newEntity.Id
            });
        }

        /// <summary>
        /// Removes an <see cref="IEntity"/> from the EntityManager.
        /// </summary>
        /// <param name="id">The target <see cref="IEntity"/> id.</param>
        public void RemoveEntity(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException(nameof(id));
            }

            IEntity targetEntity = this.allEntities.SingleOrDefault(x => x.Id == id);
            if (targetEntity != null)
            {
                this.RemoveEntity(targetEntity);
            }
        }

        /// <summary>
        /// Removes all entities from the EntityManager;.
        /// </summary>
        public void RemoveAllEntities()
        {
            List<IEntity> cachedEntities = this.allEntities;

            foreach (IEntity entity in cachedEntities)
            {
                this.allEntities.Remove(entity);

                entity.Destroy();
            }

            this.toRemove.Clear();
            this.toDisable.Clear();
            this.disabledEntitiesList.Clear();
        }

        /// <summary>
        /// Gets an entity from the EntityManager by its ID.
        /// </summary>
        /// <param name="id">The ID to search.</param>
        /// <returns><see cref="IEntity"/> with matching ID. Null if not found.</returns>
        public IEntity GetEntityById(string id)
        {
            return this.allEntities.SingleOrDefault(x => x.Id == id);
        }

        /// <summary>
        /// Gets the first <see cref="IEntity"/> that contains component of type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type being checked for.</typeparam>
        /// <returns><see cref="IEntity"/>.</returns>
        public IEntity GetEntityWithComponentOfType<T>()
            where T : IEntityComponent
        {
            foreach (IEntity entity in this.allEntities)
            {
                if (entity.GetComponent<T>() != null)
                {
                    return entity;
                }
            }

            return null;
        }

        /// <summary>
        /// Gets all <see cref="IEntity"/> objects that contain component of type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type being checked for.</typeparam>
        /// <returns><see cref="IEnumerable{IEntity}"/>.</returns>
        public IEnumerable<IEntity> GetAllEntitiesWithComponentOfType<T>()
            where T : IEntityComponent
        {
            List<IEntity> retval = new List<IEntity>();

            foreach (IEntity entity in this.allEntities)
            {
                if (entity.GetComponent<T>() != null)
                {
                    retval.Add(entity);
                }
            }

            return retval;
        }

        /// <summary>
        /// Initiates all data saves for entities.
        /// </summary>
        /// <param name="fileStream">The <see cref="FileStream"/> for the save file.</param>
        /// <param name="formatter">The <see cref="BinaryFormatter"/> used to format the data.</param>
        public void Load(FileStream fileStream, BinaryFormatter formatter)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Initiates all data saves for entities.
        /// </summary>
        /// <param name="fileStream">The <see cref="FileStream"/> for the save file.</param>
        /// <param name="formatter">The <see cref="BinaryFormatter"/> used to format the data.</param>
        public void Save(ref FileStream fileStream, BinaryFormatter formatter)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Frame Update function. This will first enable any entities waiting to be enabled,
        /// update all updatable components, process pending entity disables, then process
        /// entity removals.
        /// </summary>
        /// <param name="deltaTime">The time delta of last frame.</param>
        public void Update(float deltaTime)
        {
            // Enable Entities
            if (this.toEnable.Count > 0)
            {
                List<IEntity> toEnableCache = new List<IEntity>(this.toEnable.Count);
                foreach (IEntity entityToEnable in toEnableCache)
                {
                    // Check if the component is currently in the disabled group,
                    // and remove it.
                    if (entityToEnable.IsEnabled && !entityToEnable.RemoveFlag
                        && this.disabledEntitiesList.Contains(entityToEnable))
                    {
                        this.disabledEntitiesList.Remove(entityToEnable);
                    }
                    else if (entityToEnable.RemoveFlag)
                    {
                        // but if we have a remove flag, we definitely shouldn't enable this
                        // entity and should instead deferr to the removal.
                        if (!this.toRemove.Contains(entityToEnable))
                        {
                            this.toRemove.Add(entityToEnable);
                        }

                        this.toEnable.Remove(entityToEnable);
                        entityToEnable.IsEnabled = false;

                        // We also want to be sure to disable any components before we
                        // update or draw this frame. This should probably be smarter,
                        // but will do for now.
                        IEnumerable<IEntityComponent> componentsToDisable =
                            entityToEnable.GetComponentsOfType<IEntityComponent>();

                        if (componentsToDisable != null && componentsToDisable.Any())
                        {
                            foreach (IEntityComponent component in componentsToDisable)
                            {
                                component.IsEnabled = false;
                            }
                        }

                        continue;
                    }

                    entityToEnable.IsEnabled = true;

                    // Enable any components on this entity
                    IEnumerable<IEntityComponent> entityComponents =
                        entityToEnable.GetComponentsOfType<IEntityComponent>();
                    if (entityComponents != null && entityComponents.Any())
                    {
                        foreach (IEntityComponent component in entityComponents)
                        {
                            component.IsEnabled = true;
                        }
                    }

                    this.toEnable.Remove(entityToEnable);
                }
            }

            // update updatable components
            if (this.updatableComponents.Count > 0)
            {
                foreach (IUpdatableEntityComponent updatableComponent in this.updatableComponents)
                {
                    if (updatableComponent.Parent.IsEnabled)
                    {
                        if (updatableComponent.IsEnabled)
                        {
                            updatableComponent.Update(deltaTime);
                            continue;
                        }
                    }
                    else if (!updatableComponent.Parent.IsEnabled && !this.disabledEntitiesList.Contains(updatableComponent.Parent) && !this.toDisable.Contains(updatableComponent.Parent))
                    {
                        this.toDisable.Add(updatableComponent.Parent);
                    }
                }
            }

            // Re-Enable previously disabled entities that are now marked as enabled.
            if (this.disabledEntitiesList.Count > 0)
            {
                List<IEntity> toEnableCache = new List<IEntity>(this.disabledEntitiesList);
                foreach (IEntity entity in toEnableCache)
                {
                    if (entity.IsEnabled && !this.toDisable.Contains(entity))
                    {
                        // Enable any components on this entity
                        IEnumerable<IEntityComponent> entityComponents =
                            entity.GetComponentsOfType<IEntityComponent>();
                        if (entityComponents != null && entityComponents.Any())
                        {
                            foreach (IEntityComponent component in entityComponents)
                            {
                                component.IsEnabled = true;
                            }
                        }

                        this.disabledEntitiesList.Remove(entity);
                    }
                }
            }

            // These must occur after other update methods
            if (this.toDisable.Count > 0)
            {
                List<IEntity> toDisableCache = new List<IEntity>(this.toDisable);
                foreach (IEntity entityToDisable in toDisableCache)
                {
                    if (this.toEnable.Contains(entityToDisable))
                    {
                        throw new Exception("Cannot disable and enable entity in same frame. This should never happen.");
                    }

                    entityToDisable.IsEnabled = false;

                    IEnumerable<IEntityComponent> entityComponents =
                        entityToDisable.GetComponentsOfType<IEntityComponent>();
                    if (entityComponents != null && entityComponents.Any())
                    {
                        foreach (IEntityComponent component in entityComponents)
                        {
                            component.IsEnabled = false;
                        }
                    }

                    this.disabledEntitiesList.Add(entityToDisable);
                    this.toDisable.Remove(entityToDisable);
                }
            }

            // Destroy entities queued for removal
            if (this.toRemove.Count > 0)
            {
                List<IEntity> removeListCache = new List<IEntity>(this.toDisable);

                foreach (Entity entityToRemove in removeListCache)
                {
                    this.RemoveEntity(entityToRemove);
                    this.toRemove.Remove(entityToRemove);
                    entityToRemove.Destroy();
                }
            }
        }

        /// <summary>
        /// Called every frame draw. Processes all registered drawable entity components.
        /// </summary>
        /// <param name="sb">The <see cref="SpriteBatch"/> for the Draw method call.</param>
        public void Draw(SpriteBatch sb)
        {
            foreach (IDrawableEntityComponent drawableComponent in this.drawableEntityComponents)
            {
                if (drawableComponent.IsEnabled)
                {
                    drawableComponent.Draw(sb);
                }
            }
        }

        /// <summary>
        /// Rmeoves an <see cref="IEntity"/> from the EntityManager.
        /// </summary>
        /// <param name="entity">The target <see cref="Entity"/>.</param>
        private void RemoveEntity(IEntity entity)
        {
            this.toRemove.Remove(entity);

            if (this.allEntities.Contains(entity))
            {
                this.allEntities.Remove(entity);
            }

            if (this.disabledEntitiesList.Contains(entity))
            {
                this.disabledEntitiesList.Remove(entity);
            }

            if (this.toRemove.Contains(entity))
            {
                this.toRemove.Remove(entity);
            }

            IEnumerable<IUpdatableEntityComponent> entityUpdatableComponents =
                entity.GetComponentsOfType<IUpdatableEntityComponent>();
            if (entityUpdatableComponents != null && entityUpdatableComponents.Any())
            {
                foreach (IUpdatableEntityComponent updatableComponent in entityUpdatableComponents)
                {
                    this.updatableComponents.Remove(updatableComponent);
                }
            }

            IEnumerable<IDrawableEntityComponent> entityDrawableComponents =
                entity.GetComponentsOfType<IDrawableEntityComponent>();
            if (entityDrawableComponents != null && entityDrawableComponents.Any())
            {
                foreach (IDrawableEntityComponent drawableComponent in entityDrawableComponents)
                {
                    this.drawableEntityComponents.Remove(drawableComponent);
                }
            }

            entity.Destroy();

            this.eventDispatcher.Publish(new EventData()
            {
                EventId = Entity.EntityRemovedProgrammaticId,
                Sender = this,
                TargetObjectId = entity.Id
            });
        }
    }
}
