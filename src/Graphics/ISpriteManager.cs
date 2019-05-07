namespace ZxenLib.Graphics
{
    using System.Collections.Generic;
    using Microsoft.Xna.Framework.Graphics;

    /// <summary>
    /// Interface for the <see cref="SpriteManager"/> class.
    /// </summary>
    public interface ISpriteManager
    {
        /// <summary>
        /// Gets the dictionary of loaded <see cref="Atlas"/>es.
        /// </summary>
        Dictionary<string, Atlas> AtlasDictionary { get; }

        /// <summary>
        /// Adds an atlas to the <see cref="ISpriteManager"/>.
        /// </summary>
        /// <param name="name">The name of the atlas.</param>
        /// <param name="texture">The loaded texture for the atlas.</param>
        /// <param name="dataPath">The path to the atlas data file.</param>
        void AddAtlas(string name, Texture2D texture, string dataPath);

        /// <summary>
        /// Gets an <see cref="Atlas"/> from the atlas name.
        /// </summary>
        /// <param name="atlasName">The name of the atlas.</param>
        /// <returns><see cref="Atlas"/></returns>
        Atlas GetAtlas(string atlasName);

        /// <summary>
        /// Gets the specified sprite directly from the specified atlas.
        /// </summary>
        /// <param name="atlasName">The atlas to find the sprite in.</param>
        /// <param name="spriteId">The sprite to retrieve.</param>
        /// <returns><see cref="Sprite"/></returns>
        Sprite GetSprite(string atlasName, string spriteId);

        /// <summary>
        /// Unloads an <see cref="Atlas"/> from the <see cref="SpriteManager"/>
        /// </summary>
        /// <param name="atlasName">The name of the atlas.</param>
        void UnloadAtlas(string atlasName);
    }
}
