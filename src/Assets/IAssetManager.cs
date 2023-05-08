namespace ZxenLib;

using System.Collections.Generic;
using Assets;
using Microsoft.Xna.Framework;
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
    Dictionary<string, Texture2D> TexturesDictionary { get; }

    /// <summary>
    /// Gets the dictionary of loaded <see cref="SpriteFont"/> content.
    /// </summary>
    Dictionary<string, SpriteFont> FontsDictionary { get; }

    /// <summary>
    /// Gets the dictionary of loaded <see cref="SoundEffect"/> content.
    /// </summary>
    Dictionary<string, SoundEffect> SoundFxdDictionary { get; }

    /// <summary>
    /// Gets the dicationary of loaded <see cref="Song"/> content.
    /// </summary>
    Dictionary<string, Song> SongsDictionary { get; }

    /// <summary>
    /// Gets the dictionary of loaded <see cref="string"/> content.
    /// </summary>
    Dictionary<string, string> Strings { get; }

    /// <summary>
    /// Initializes the AssetManager.
    /// </summary>
    /// <param name="game">The Game class used to set the ContentManager.</param>
    /// <param name="stringsFileName">The file name of the text file containing strings data.</param>
    /// <param name="stringsDirectory">The directory containing the strings file.</param>
    /// <param name="useStrings">Flag for if the built-in strings system should be used.</param>
    void Initialize(Game game, string stringsFileName = "strings.txt", string stringsDirectory = "", bool useStrings = true);

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

    /// <summary>
    /// Unloads all loaded assets in the AssetManager.
    /// </summary>
    void UnloadAssets();
}