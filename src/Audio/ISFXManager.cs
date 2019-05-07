namespace ZxenLib.Audio
{
    /// <summary>
    /// Interface for <see cref="SFXManager"/>
    /// </summary>
    public interface ISFXManager
    {
        /// <summary>
        /// Gets or sets the master volume, which all other volumes are mathematically based off of.
        /// </summary>
        float MasterVolume { get; set; }

        /// <summary>
        /// Gets or sets the volume of all music played.
        /// </summary>
        float MusicVolume { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether music should be played.
        /// </summary>
        bool ShouldPlayMusic { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether sound effects should be played.
        /// </summary>
        bool ShouldPlaySoundEffects { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether UI effects should be played.
        /// </summary>
        bool ShouldPlayUIEffects { get; set; }

        /// <summary>
        /// Gets or sets the volume of all sound effects.
        /// </summary>
        float SoundEffectsVolume { get; set; }

        /// <summary>
        /// Gets or sets the volume of UI sounds.
        /// </summary>
        float UIVolume { get; set; }

        /// <summary>
        /// Initializes the SFX manager.
        /// </summary>
        void Initialize();

        /// <summary>
        /// Plays the provided sound effect id.
        /// </summary>
        /// <param name="sfxId">The ID of the sound effect file.</param>
        void PlayEffect(string sfxId);

        /// <summary>
        /// Plays the music file from the provided song id.
        /// </summary>
        /// <param name="songId">The name of the song file to play</param>
        void PlayMusic(string songId);

        /// <summary>
        /// Plays the provided UI sound effect id.
        /// </summary>
        /// <param name="uiSoundId">The ID of the sound effect file.</param>
        void PlayUISound(string uiSoundId);

        /// <summary>
        /// Stops playing any playing music.
        /// </summary>
        void StopMusic();
    }
}
