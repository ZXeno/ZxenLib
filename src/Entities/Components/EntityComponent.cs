﻿namespace ZxenLib.Entities.Components
{
    using System;

    /// <summary>
    /// The base class of an EntityComponent which implements basic register/unregister functionality.
    /// </summary>
    public abstract class EntityComponent : IEntityComponent
    {
        /// <summary>
        /// Gets or sets gets a value indicating the ID of this component.
        /// </summary>
        public string Id { get; protected set; }

        /// <summary>
        /// Gets or sets a value indicating the programmatic id of this component.
        /// </summary>
        public string ProgrammaticId { get; protected set; }

        /// <summary>
        /// Gets or sets a value indicating whether the EntityComponent is enabled.
        /// </summary>
        public bool IsEnabled { get; set; }

        /// <summary>
        /// Gets or sets a value representing the parent of this component.
        /// </summary>
        public IEntity Parent { get; protected set; }

        /// <summary>
        /// Registers the component with a parent entity.
        /// </summary>
        /// <param name="parent">The <see cref="IEntity"/> object parent of this <see cref="IEntityComponent"/>.</param>
        public virtual void Register(IEntity parent)
        {
            if (parent == null)
            {
                throw new ArgumentNullException(nameof(parent));
            }

            if (this.Parent?.Id != parent.Id)
            {
                this.Parent = parent;
            }

            parent.RegisterComponent(this);
        }

        /// <summary>
        /// Unregisters the component from its parent.
        /// </summary>
        public virtual void Unregister()
        {
            this.Parent.UnregisterComponent(this.Id);
            this.Parent = null;
        }
    }
}
