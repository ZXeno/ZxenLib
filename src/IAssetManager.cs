namespace ZxenLib
{
    using System.Collections.Generic;
    using Microsoft.Xna.Framework.Audio;
    using Microsoft.Xna.Framework.Graphics;
    using Microsoft.Xna.Framework.Media;

    /// <summary>
    /// Interface for the <see cref="AssetManager"/> class.
    /// </summary>
    public interface IAssetManager
    {
        /// <summary>
        /// Gets the dictionary of loaded <see cref="Texture2D"/>.
        /// </summary>
        Dictionary<string, Texture2D> Textures { get; }

        /// <summary>
        /// Gets the dictionary of loaded <see cref="SpriteFont"/> content.
        /// </summary>
        Dictionary<string, SpriteFont> Fonts { get; }

        /// <summary>
        /// Gets the dictionary of loaded <see cref="SoundEffect"/> content.
        /// </summary>
        Dictionary<string, SoundEffect> SoundFX { get; }

        /// <summary>
        /// Gets the dicationary of loaded <see cref="Song"/> content.
        /// </summary>
        Dictionary<string, Song> Songs { get; }

        /// <summary>
        /// Gets the dictionary of loaded <see cref="string"/> content.
        /// </summary>
        Dictionary<string, string> Strings { get; }

        /// <summary>
        /// Initializes the AssetManager.
        /// </summary>
        void Initialize();

        /// <summary>
        /// Gets all file names under a given folder path. Does not enumerate subfolders.
        /// </summary>
        /// <param name="path">The path to enumerate files from.</param>
        /// <returns>Enumerable of file names.</returns>
        IEnumerable<string> GetAllFileNames(string path);

        /// <summary>
        /// Loads all assets that have been found.
        /// </summary>
        void LoadAssets();
    }
}
