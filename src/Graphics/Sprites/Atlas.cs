namespace ZxenLib.Graphics;

using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

/// <summary>
/// Holds a collection of <see cref="Sprite"/> values indicated in the .
/// </summary>
public class Atlas
{
    /// <summary>
    /// Sprite Source Rectangle Dictionary.
    /// </summary>
    private readonly Dictionary<string, Sprite> spriteDictionary;

    /// <summary>
    /// Initializes a new instance of the <see cref="Atlas"/> class.
    /// </summary>
    /// <param name="name">The id of the atlas.</param>
    /// <param name="textureAtlas">The texture of the atlas.</param>
    /// <param name="dataFilePath">Path pointing at the data file for the sprite atlas.</param>
    public Atlas(string name, Texture2D textureAtlas, string dataFilePath)
    {
        this.AtlasName = name;
        this.spriteDictionary = new Dictionary<string, Sprite>();
        this.DataFilePath = dataFilePath;
        this.TextureAtlas = textureAtlas;
    }

    /// <summary>
    /// Gets or sets the id of the Atlas.
    /// </summary>
    public string AtlasName { get; set; }

    /// <summary>
    /// Gets or sets the texture atlas of the Atlas data.
    /// </summary>
    public Texture2D TextureAtlas { get; protected set; }

    /// <summary>
    /// Gets the file system path pointing to the location of the data file.
    /// </summary>
    protected string DataFilePath { get; private set; }

    /// <summary>
    /// Loads the atlas data from the data file.
    /// </summary>
    public virtual void LoadAtlasData()
    {
        if (string.IsNullOrWhiteSpace(this.DataFilePath))
        {
            throw new InvalidOperationException($"{nameof(this.DataFilePath)} cannot be null or empty.");
        }

        string dataFileContent = File.ReadAllText(this.DataFilePath);
        SpriteImportModel[] importedSprites = JsonSerializer.Deserialize<SpriteImportModel[]>(dataFileContent);
        foreach (SpriteImportModel import in importedSprites)
        {
            Rectangle sourceRect = new Rectangle(
                import.X,
                import.Y,
                import.Width,
                import.Height);

            Sprite newSprite = new Sprite(this.AtlasName, import.Id, sourceRect)
            {
                Slice = import.Slice,
            };

            this.spriteDictionary.Add(newSprite.SpriteId, newSprite);
        }
    }

    /// <summary>
    /// Get sprite source rectangle based on sprite ID from sprite dictionary.
    /// </summary>
    /// <param name="spriteId">Id of the sprite.</param>
    /// <returns><see cref="Rectangle"/>.</returns>
    public Sprite GetSprite(string spriteId)
    {
        return this.spriteDictionary[spriteId];
    }

    /// <summary>
    /// Disposes of the atlas.
    /// </summary>
    public void Dispose()
    {
        this.TextureAtlas.Dispose();
    }

    /// <summary>
    /// Model used exclusively for imported sprite data.
    /// </summary>
    internal sealed class SpriteImportModel
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("x")]
        public int X { get; set; }

        [JsonPropertyName("y")]
        public int Y { get; set; }

        [JsonPropertyName("width")]
        public int Width { get; set; }

        [JsonPropertyName("height")]
        public int Height { get; set; }

        [JsonPropertyName("slice")]
        public int Slice { get; set; }
    }
}