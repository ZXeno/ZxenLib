namespace ZxenLib.Entities
{
    using System.Collections.Generic;
    using System.IO;
    using System.Runtime.Serialization.Formatters.Binary;
    using Microsoft.Xna.Framework.Graphics;
    using ZxenLib.Entities.Components;

    /// <summary>
    /// Interface for the <see cref="EntityManager"/> class.
    /// </summary>
    public interface IEntityManager
    {
        /// <summary>
        /// Gets an entity from the EntityManager by its ID.
        /// </summary>
        /// <param name="id">The ID to search.</param>
        /// <returns><see cref="IEntity"/> with matching ID. Null if not found.</returns>
        IEntity GetEntityById(string id);

        /// <summary>
        /// Adds a new <see cref="Entity"/> to the EntityManager.
        /// </summary>
        /// <param name="newEntity">The <see cref="IEntity"/> to add.</param>
        void AddNewEntity(IEntity newEntity);

        /// <summary>
        /// Removes an <see cref="Entity"/> from the EntityManager.
        /// </summary>
        /// <param name="id">The target <see cref="IEntity"/> id.</param>
        void RemoveEntity(string id);

        /// <summary>
        /// Removes all entities from the EntityManager;
        /// </summary>
        void RemoveAllEntities();

        /// <summary>
        /// Gets the first <see cref="IEntity"/> that contains component of type <typeparamref name="T"/>
        /// </summary>
        /// <typeparam name="T">The type being checked for.</typeparam>
        /// <returns><see cref="IEntity"/></returns>
        IEntity GetEntityWithComponentOfType<T>()
            where T : IEntityComponent;

        /// <summary>
        /// Gets all <see cref="IEntity"/> objects that contain component of type <typeparamref name="T"/>
        /// </summary>
        /// <typeparam name="T">The type being checked for.</typeparam>
        /// <returns><see cref="IEnumerable{IEntity}"/></returns>
        IEnumerable<IEntity> GetAllEntitiesWithComponentOfType<T>()
            where T : IEntityComponent;

        /// <summary>
        /// Frame Update function
        /// </summary>
        /// <param name="deltaTime">The time delta of last frame.</param>
        void Update(float deltaTime);

        /// <summary>
        /// Called every frame draw.
        /// </summary>
        /// <param name="sb">The <see cref="SpriteBatch"/> for the Draw method call.</param>
        void Draw(SpriteBatch sb);

        /// <summary>
        /// Initiates all data saves for entities.
        /// </summary>
        /// <param name="fileStream">The <see cref="FileStream"/> for the save file.</param>
        /// <param name="formatter">The <see cref="BinaryFormatter"/> used to format the data.</param>
        void Save(ref FileStream fileStream, BinaryFormatter formatter);

        /// <summary>
        /// Initiates all data saves for entities.
        /// </summary>
        /// <param name="fileStream">The <see cref="FileStream"/> for the save file.</param>
        /// <param name="formatter">The <see cref="BinaryFormatter"/> used to format the data.</param>
        void Load(FileStream fileStream, BinaryFormatter formatter);
    }
}
