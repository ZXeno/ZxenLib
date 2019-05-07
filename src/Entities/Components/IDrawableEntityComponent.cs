namespace ZxenLib.Entities.Components
{
    using Microsoft.Xna.Framework.Graphics;

    /// <summary>
    /// Interface for implementing a drawable entity component.
    /// </summary>
    public interface IDrawableEntityComponent : IEntityComponent
    {
        /// <summary>
        /// Method for batching draw calls. Called every frame.
        /// </summary>
        /// <param name="sb">The <see cref="SpriteBatch"/> for this component.</param>
        void Draw(SpriteBatch sb);
    }
}
