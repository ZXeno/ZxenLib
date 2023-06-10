namespace ZxenLib.Assets;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Graphics.Sprites;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;
using ZxenLib.Graphics;

/// <summary>
/// Used for managing game assets such as textures, sounds, fonts, etc.
/// </summary>
public class AssetManager : IAssetManager
{
    private const string DataDirectory = "Data";
    private const string FontFolderName = "Fonts";
    private const string SfxFolderName = "SoundFX";
    private const string SongsFolderName = "Music";
    private const string TextureFolderName = "Textures";
    private const string StringsFileName = "strings.txt";

    private readonly ISpriteManager spriteManager;

    private ContentManager contentManager;
    private string sfxDirectoryPath;
    private string fontDirectoryPath;
    private string songsDirectoryPath;
    private string textureDirectoryPath;
    private string stringsDirectoryPath;
    private bool useStrings;

    /// <summary>
    /// Initializes a new instance of the <see cref="AssetManager"/> class.
    /// </summary>
    /// <param name="game">The Monogame <see cref="Game"/> dependency.</param>
    /// <param name="spriteManager">The ZxenLib <see cref="ISpriteManager"/> dependency.</param>
    public AssetManager(ISpriteManager spriteManager)
    {
        this.spriteManager = spriteManager;
    }

    /// <summary>
    /// Gets the dictionary of loaded <see cref="Texture2D"/>.
    /// </summary>
    public Dictionary<string, Texture2D> TexturesDictionary { get; private set; }

    /// <summary>
    /// Gets the dictionary of loaded <see cref="SpriteFont"/> content.
    /// </summary>
    public Dictionary<string, SpriteFont> FontsDictionary { get; private set; }

    /// <summary>
    /// Gets the dictionary of loaded <see cref="SoundEffect"/> content.
    /// </summary>
    public Dictionary<string, SoundEffect> SoundFxdDictionary { get; private set; }

    /// <summary>
    /// Gets the dictionary of loaded <see cref="Song"/> content.
    /// </summary>
    public Dictionary<string, Song> SongsDictionary { get; private set; }

    /// <summary>
    /// Gets the dictionary of loaded <see cref="string"/> content.
    /// </summary>
    public Dictionary<string, string> Strings { get; private set; }

    /// <summary>
    /// Initializes the AssetManager.
    /// </summary>
    /// <param name="stringsFileName">The file name of the text file containing strings data.</param>
    /// <param name="stringsDirectory">The directory containing the strings file.</param>
    /// <param name="useStrings">Flag for if the built-in strings system should be used.</param>
    public void Initialize(Game game, string stringsFileName = "strings.txt", string stringsDirectory = "", bool useStrings = true)
    {
        this.contentManager = game.Content;
        this.useStrings = useStrings;
        this.TexturesDictionary = new();
        this.FontsDictionary = new();
        this.SoundFxdDictionary = new();
        this.SongsDictionary = new();

        this.textureDirectoryPath = Path.Combine(this.contentManager.RootDirectory, AssetManager.TextureFolderName);
        this.fontDirectoryPath = Path.Combine(this.contentManager.RootDirectory, AssetManager.FontFolderName);
        this.sfxDirectoryPath = Path.Combine(this.contentManager.RootDirectory, AssetManager.SfxFolderName);
        this.songsDirectoryPath = Path.Combine(this.contentManager.RootDirectory, AssetManager.SongsFolderName);

        if (!Directory.Exists(this.textureDirectoryPath))
        {
            Directory.CreateDirectory(this.textureDirectoryPath);
        }

        if (!Directory.Exists(this.fontDirectoryPath))
        {
            Directory.CreateDirectory(this.fontDirectoryPath);
        }

        if (!Directory.Exists(this.sfxDirectoryPath))
        {
            Directory.CreateDirectory(this.sfxDirectoryPath);
        }

        if (!Directory.Exists(this.songsDirectoryPath))
        {
            Directory.CreateDirectory(this.songsDirectoryPath);
        }

        if (this.useStrings)
        {
            this.Strings = new();
            this.stringsDirectoryPath = Path.Combine(this.contentManager.RootDirectory, AssetManager.DataDirectory);
            if (!Directory.Exists(this.stringsDirectoryPath))
            {
                Directory.CreateDirectory(this.stringsDirectoryPath);
            }
        }
    }

    /// <summary>
    /// Gets all file names under a given folder path. Does not enumerate subfolders.
    /// </summary>
    /// <param name="path">The path to enumerate files from.</param>
    /// <returns>Enumerable of file names.</returns>
    public IEnumerable<string> GetAllFileNames(string path)
    {
        return Directory.EnumerateFiles(path);
    }

    /// <summary>
    /// Loads all assets that have been found.
    /// </summary>
    public void LoadAssets()
    {
        /* TODO: There must be a way to dynamically load
         * assets and sort them by type without needing
         * to know which folders they are in beforehand.
         *
         * Pretty sure the answer to this is import all
         * accepted file types and have a target image
         * file name as a property of the atlas JSON.
         *
         * Same for other file types, minus JSON manifests.
         */

        List<string> fileNames = null;

        // Load textures
        if (Directory.Exists(this.textureDirectoryPath))
        {
            fileNames = this.GetAllFileNames(this.textureDirectoryPath).ToList();
            foreach (string filePath in fileNames.Where(x => !x.EndsWith(".json")))
            {
                string fileName = Path.GetFileNameWithoutExtension(filePath);
                this.TexturesDictionary.Add(fileName, this.contentManager.Load<Texture2D>(Path.Join(AssetManager.TextureFolderName, fileName)));
            }

            foreach (string filePath in fileNames.Where(x => x.EndsWith(".json")))
            {
                string fileName = Path.GetFileNameWithoutExtension(filePath);
                string fullyQualifiedPath = Path.Combine(Environment.CurrentDirectory, filePath);
                this.spriteManager.AddAtlas(fileName, this.TexturesDictionary[fileName], fullyQualifiedPath);
            }
        }

        // Load fonts
        if (Directory.Exists(this.fontDirectoryPath))
        {
            fileNames = this.GetAllFileNames(this.fontDirectoryPath).ToList();
            foreach (string filePath in fileNames)
            {
                string fileName = Path.GetFileNameWithoutExtension(filePath);
                this.FontsDictionary.Add(fileName, this.contentManager.Load<SpriteFont>(Path.Join(AssetManager.FontFolderName, fileName)));
            }
        }

        // Load SoundFX
        if (Directory.Exists(this.sfxDirectoryPath))
        {
            fileNames = this.GetAllFileNames(this.sfxDirectoryPath).ToList();
            foreach (string filePath in fileNames)
            {
                string fileName = Path.GetFileNameWithoutExtension(filePath);
                this.SoundFxdDictionary.Add(fileName, this.contentManager.Load<SoundEffect>(Path.Join(AssetManager.SfxFolderName, fileName)));
            }
        }

        // Load Songs
        if (Directory.Exists(this.songsDirectoryPath))
        {
            fileNames = this.GetAllFileNames(this.songsDirectoryPath).Where(x => x.EndsWith(".xnb")).ToList();
            foreach (string filePath in fileNames)
            {
                string fileName = Path.GetFileNameWithoutExtension(filePath);
                this.SongsDictionary.Add(fileName, this.contentManager.Load<Song>(Path.Join(AssetManager.SongsFolderName, fileName)));
            }
        }

        // Load Strings
        if (this.useStrings && Directory.Exists(this.stringsDirectoryPath))
        {
            string stringsData = File.ReadAllText(Path.Combine(this.stringsDirectoryPath, AssetManager.StringsFileName));
            string[] splitdata = stringsData.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            for (int x = 0; x < splitdata.Length; x++)
            {
                string[] splitLine = splitdata[x].Split(new[] { "=" }, StringSplitOptions.None);
                this.Strings.Add(splitLine[0].Trim(), splitLine[1].Trim());
            }
        }
    }

    /// <summary>
    /// Unloads all loaded assets in the AssetManager.
    /// </summary>
    public void UnloadAssets()
    {
        this.UnloadDictionaryAssets(this.TexturesDictionary);
        this.UnloadDictionaryAssets(this.FontsDictionary);
        this.UnloadDictionaryAssets(this.SoundFxdDictionary);
        this.UnloadDictionaryAssets(this.SongsDictionary);
    }

    private void UnloadDictionaryAssets<T>(IDictionary<string, T> dict)
    {
        while (dict.Count > 0)
        {
            KeyValuePair<string, T> entry = dict.First();
            this.contentManager.UnloadAsset(entry.Key);
            dict.Remove(entry.Key);
        }
    }
}