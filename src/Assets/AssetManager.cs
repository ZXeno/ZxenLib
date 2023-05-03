namespace ZxenLib;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

    private readonly ContentManager contentManager;
    private readonly ISpriteManager spriteManager;

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
    public AssetManager(Game game, ISpriteManager spriteManager)
    {
        this.contentManager = game.Content;
        this.spriteManager = spriteManager;
    }

    /// <summary>
    /// Gets the dictionary of loaded <see cref="Texture2D"/>.
    /// </summary>
    public Dictionary<string, Texture2D> Textures { get; private set; }

    /// <summary>
    /// Gets the dictionary of loaded <see cref="SpriteFont"/> content.
    /// </summary>
    public Dictionary<string, SpriteFont> Fonts { get; private set; }

    /// <summary>
    /// Gets the dictionary of loaded <see cref="SoundEffect"/> content.
    /// </summary>
    public Dictionary<string, SoundEffect> SoundFX { get; private set; }

    /// <summary>
    /// Gets the dicationary of loaded <see cref="Song"/> content.
    /// </summary>
    public Dictionary<string, Song> Songs { get; private set; }

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
    public void Initialize(string stringsFileName = "strings.txt", string stringsDirectory = "", bool useStrings = true)
    {
        this.useStrings = useStrings;
        this.Textures = new Dictionary<string, Texture2D>();
        this.Fonts = new Dictionary<string, SpriteFont>();
        this.SoundFX = new Dictionary<string, SoundEffect>();
        this.Songs = new Dictionary<string, Song>();

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
            this.Strings = new Dictionary<string, string>();
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
                this.Textures.Add(fileName, this.contentManager.Load<Texture2D>(Path.Join(AssetManager.TextureFolderName, fileName)));
            }

            foreach (string filePath in fileNames.Where(x => x.EndsWith(".json")))
            {
                string fileName = Path.GetFileNameWithoutExtension(filePath);
                string fullyQualifiedPath = Path.Combine(Environment.CurrentDirectory, filePath);
                this.spriteManager.AddAtlas(fileName, this.Textures[fileName], fullyQualifiedPath);
            }
        }

        // Load fonts
        if (Directory.Exists(this.fontDirectoryPath))
        {
            fileNames = this.GetAllFileNames(this.fontDirectoryPath).ToList();
            foreach (string filePath in fileNames)
            {
                string fileName = Path.GetFileNameWithoutExtension(filePath);
                this.Fonts.Add(fileName, this.contentManager.Load<SpriteFont>(Path.Join(AssetManager.FontFolderName, fileName)));
            }
        }

        // Load SoundFX
        if (Directory.Exists(this.sfxDirectoryPath))
        {
            fileNames = this.GetAllFileNames(this.sfxDirectoryPath).ToList();
            foreach (string filePath in fileNames)
            {
                string fileName = Path.GetFileNameWithoutExtension(filePath);
                this.SoundFX.Add(fileName, this.contentManager.Load<SoundEffect>(Path.Join(AssetManager.SfxFolderName, fileName)));
            }
        }

        // Load Songs
        if (Directory.Exists(this.songsDirectoryPath))
        {
            fileNames = this.GetAllFileNames(this.songsDirectoryPath).Where(x => x.EndsWith(".xnb")).ToList();
            foreach (string filePath in fileNames)
            {
                string fileName = Path.GetFileNameWithoutExtension(filePath);
                this.Songs.Add(fileName, this.contentManager.Load<Song>(Path.Join(AssetManager.SongsFolderName, fileName)));
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
}