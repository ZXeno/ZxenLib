namespace ZxenLib.Audio;

using System;
using Assets;
using Extensions;
using Microsoft.Xna.Framework.Media;
using ZxenLib.Events;

/// <summary>
/// Manages and plays all sound effects for the game.
/// </summary>
public class AudioManager : IAudioManager
{
    private readonly IEventDispatcher eventDispatcher;
    private readonly IAssetManager assetManager;

    /// <summary>
    /// Initializes a new instance of the <see cref="AudioManager"/> class.
    /// </summary>
    public AudioManager(IEventDispatcher eventDispatcher, IAssetManager assetManager)
    {
        this.eventDispatcher = eventDispatcher;
        this.assetManager = assetManager;

        // Default starting media player volume.
        MediaPlayer.Volume = 1f;
    }

    /// <summary>
    /// Gets or sets the master volume, which all other volumes are mathematically based off of.
    /// </summary>
    public float MasterVolume { get; set; }

    /// <summary>
    /// Gets or sets the volume of all sound effects.
    /// </summary>
    public float SoundEffectsVolume { get; set; }

    /// <summary>
    /// Gets or sets the volume of all music played.
    /// </summary>
    public float MusicVolume { get; set; }

    /// <summary>
    /// Gets or sets the volume of UI sounds.
    /// </summary>
    public float UIVolume { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether music should be played.
    /// </summary>
    public bool ShouldPlayMusic { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether sound effects should be played.
    /// </summary>
    public bool ShouldPlaySoundEffects { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether UI effects should be played.
    /// </summary>
    public bool ShouldPlayUIEffects { get; set; }

    /// <summary>
    /// Initializes the SFX manager.
    /// </summary>
    public void Initialize()
    {
        // TODO: Bind to controls mapping.
    }

    /// <summary>
    /// Plays the music file from the provided song id.
    /// </summary>
    /// <param name="songId">The name of the song file to play.</param>
    public void PlayMusic(string songId)
    {
        if (string.IsNullOrWhiteSpace(songId))
        {
            throw new ArgumentNullException(nameof(songId));
        }

        if (!this.ShouldPlayMusic)
        {
            return;
        }

        if (MediaPlayer.State == MediaState.Playing)
        {
            MediaPlayer.Stop();
        }

        MediaPlayer.Volume = this.MasterVolume * this.MusicVolume;
        MediaPlayer.Play(this.assetManager.SongsDictionary[songId]);
    }

    /// <summary>
    /// Stops playing any playing music.
    /// </summary>
    public void StopMusic()
    {
        MediaPlayer.Stop();
    }

    /// <summary>
    /// Plays the provided sound effect id.
    /// </summary>
    /// <param name="sfxId">The ID of the sound effect file.</param>
    public void PlayEffect(string sfxId)
    {
        if (!this.ShouldPlaySoundEffects)
        {
            return;
        }

        sfxId.ThrowIfNullOrWhitespace();

        this.assetManager.SoundFxdDictionary[sfxId].Play(this.MasterVolume * this.SoundEffectsVolume, 0, 0);
    }

    /// <summary>
    /// Plays the provided UI sound effect id.
    /// </summary>
    /// <param name="uiSoundId">The ID of the sound effect file.</param>
    public void PlayUISound(string uiSoundId)
    {
        if (!this.ShouldPlayUIEffects)
        {
            return;
        }

        uiSoundId.ThrowIfNullOrWhitespace();

        this.assetManager.SoundFxdDictionary[uiSoundId].Play(this.MasterVolume * this.UIVolume, 0, 0);
    }
}