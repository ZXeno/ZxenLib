﻿namespace ZxenLib.Managers
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Microsoft.Xna.Framework.Audio;
    using Microsoft.Xna.Framework.Content;
    using Microsoft.Xna.Framework.Graphics;
    using Microsoft.Xna.Framework.Media;

    /// <summary>
    /// Used for managing game assets such as textures, sounds, fonts, etc.
    /// </summary>
    public class AssetManager : IAssetManager
    {
        private const string TextureFolderName = "Textures";
        private const string FontFolderName = "Fonts";
        private const string SfxFolderName = "SoundFX";
        private const string SongsFolderName = "Music";
        private const string StringsFileName = "strings.txt";

        private string textureDirectoryPath;
        private string fontDirectoryPath;
        private string sfxDirectoryPath;
        private string songsDirectoryPath;
        private string stringsDirectoryPath;

        private ContentManager contentManager;
        private ISpriteManager spriteManager;

        public AssetManager(ContentManager contentManager, ISpriteManager spriteManager)
        {
            this.contentManager = contentManager;
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
        public void Initialize()
        {
            this.Textures = new Dictionary<string, Texture2D>();
            this.Fonts = new Dictionary<string, SpriteFont>();
            this.SoundFX = new Dictionary<string, SoundEffect>();
            this.Songs = new Dictionary<string, Song>();
            this.Strings = new Dictionary<string, string>();

            this.textureDirectoryPath = Path.Combine(this.contentManager.RootDirectory, AssetManager.TextureFolderName);
            this.fontDirectoryPath = Path.Combine(this.contentManager.RootDirectory, AssetManager.FontFolderName);
            this.sfxDirectoryPath = Path.Combine(this.contentManager.RootDirectory, AssetManager.SfxFolderName);
            this.songsDirectoryPath = Path.Combine(this.contentManager.RootDirectory, AssetManager.SongsFolderName);
            this.stringsDirectoryPath = Path.Combine(this.contentManager.RootDirectory, "Data");
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
            // TODO: There must be a way to dynamically load these assets and sort them by type without needing to know which folders they are in beforehand.

            // Load textures
            List<string> fileNames = this.GetAllFileNames(this.textureDirectoryPath).ToList();
            foreach (string filePath in fileNames.Where(x => !x.EndsWith(".json")))
            {
                string fileName = Path.GetFileNameWithoutExtension(filePath);
                this.Textures.Add(fileName, this.contentManager.Load<Texture2D>($"{AssetManager.TextureFolderName}\\{fileName}"));
            }

            foreach (string filePath in fileNames.Where(x => x.EndsWith(".json")))
            {
                string fileName = Path.GetFileNameWithoutExtension(filePath);
                string fullyQualifiedPath = Path.Combine(Environment.CurrentDirectory, filePath);
                this.spriteManager.AddAtlas(fileName, this.Textures[fileName], fullyQualifiedPath);
            }

            // Load fonts
            fileNames = this.GetAllFileNames(this.fontDirectoryPath).ToList();
            foreach (string filePath in fileNames)
            {
                string fileName = Path.GetFileNameWithoutExtension(filePath);
                this.Fonts.Add(fileName, this.contentManager.Load<SpriteFont>($"{AssetManager.FontFolderName}\\{fileName}"));
            }

            // Load SoundFX
            fileNames = this.GetAllFileNames(this.sfxDirectoryPath).ToList();
            foreach (string filePath in fileNames)
            {
                string fileName = Path.GetFileNameWithoutExtension(filePath);
                this.SoundFX.Add(fileName, this.contentManager.Load<SoundEffect>($"{AssetManager.SfxFolderName}\\{fileName}"));
            }

            // Load Songs
            fileNames = this.GetAllFileNames(this.songsDirectoryPath).Where(x => x.EndsWith(".xnb")).ToList();
            foreach (string filePath in fileNames)
            {
                string fileName = Path.GetFileNameWithoutExtension(filePath);
                this.Songs.Add(fileName, this.contentManager.Load<Song>($"{AssetManager.SongsFolderName}\\{fileName}"));
            }

            // Load Strings
            var stringsData = File.ReadAllText(Path.Combine(this.stringsDirectoryPath, AssetManager.StringsFileName));
            string[] splitdata = stringsData.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            for (int x = 0; x < splitdata.Length; x++)
            {
                var splitLine = splitdata[x].Split(new[] { "=" }, StringSplitOptions.None);
                this.Strings.Add(splitLine[0].Trim(), splitLine[1].Trim());
            }
        }
    }
}
