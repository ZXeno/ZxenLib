namespace ZxenLib.Graphics;

using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework.Graphics;

/// <summary>
/// Manages all sprites through a collection of Atlases.
/// </summary>
public class SpriteManager : ISpriteManager
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SpriteManager"/> class.
    /// </summary>
    public SpriteManager()
    {
        this.AtlasDictionary = new Dictionary<string, Atlas>();
    }

    /// <summary>
    /// Gets the current dictionary of loaded <see cref="Atlas"/>es.
    /// </summary>
    public Dictionary<string, Atlas> AtlasDictionary { get; private set; }

    /// <summary>
    /// Adds an atlas to the <see cref="SpriteManager"/>.
    /// </summary>
    /// <param name="name">The name of the atlas.</param>
    /// <param name="texture">The loaded texture for the atlas.</param>
    /// <param name="dataPath">The path to the atlas data file.</param>
    public void AddAtlas(string name, Texture2D texture, string dataPath)
    {
        if (string.IsNullOrWhiteSpace(dataPath))
        {
            throw new ArgumentNullException(nameof(dataPath));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentNullException(nameof(name));
        }

        if (!File.Exists(dataPath))
        {
            throw new FileNotFoundException($"Unable to find data file at {dataPath}");
        }

        if (texture == null)
        {
            throw new ArgumentNullException(nameof(texture));
        }

        Atlas newAtlas = new Atlas(name, texture, dataPath);
        newAtlas.LoadAtlasData();
        this.AtlasDictionary.Add(name, newAtlas);
    }

    /// <summary>
    /// Gets an atlas from the Atlas Dictionary.
    /// </summary>
    /// <param name="atlasName">The name of the corresponding atlas.</param>
    /// <returns><see cref="Atlas"/>.</returns>
    public Atlas GetAtlas(string atlasName)
    {
        return this.AtlasDictionary[atlasName];
    }

    /// <summary>
    /// Gets the specified sprite directly from the specified atlas.
    /// </summary>
    /// <param name="atlasName">The atlas to find the sprite in.</param>
    /// <param name="spriteId">The sprite to retrieve.</param>
    /// <returns><see cref="Sprite"/>.</returns>
    public Sprite GetSprite(string atlasName, string spriteId)
    {
        return this.AtlasDictionary[atlasName].GetSprite(spriteId);
    }

    /// <summary>
    /// Unloads an <see cref="Atlas"/> from the <see cref="SpriteManager"/>.
    /// </summary>
    /// <param name="atlasName">The name of the atlas.</param>
    public void UnloadAtlas(string atlasName)
    {
        Atlas atlas = this.AtlasDictionary[atlasName];
        this.AtlasDictionary.Remove(atlasName);
        atlas.Dispose();
    }
}