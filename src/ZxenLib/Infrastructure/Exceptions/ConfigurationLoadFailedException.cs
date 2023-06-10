namespace ZxenLib.Infrastructure.Exceptions;

using System;

/// <summary>
/// Defines an exception for configuration file loading failures.
/// </summary>
public class ConfigurationLoadFailedException : Exception
{
    private const string DefaultMessage = "The configuration file failed to load.";

    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigurationLoadFailedException"/> class.
    /// </summary>
    public ConfigurationLoadFailedException()
        : base(DefaultMessage)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigurationLoadFailedException"/> class.
    /// </summary>
    /// <param name="path">The path to the settings file.</param>
    public ConfigurationLoadFailedException(string path)
        : base(DefaultMessage)
    {
        this.SettingsFilePath = path;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigurationLoadFailedException"/> class.
    /// </summary>
    /// <param name="path">The path to the settings file.</param>
    /// <param name="message">Exception message.</param>
    public ConfigurationLoadFailedException(string path, string message = DefaultMessage)
        : base(message)
    {
        this.SettingsFilePath = path;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigurationLoadFailedException"/> class.
    /// </summary>
    /// <param name="path">The path to the settings file.</param>
    /// <param name="innerException">Reference to the inner <see cref="Exception"/> that is the cause of this exception.</param>
    /// <param name="message">Exception message.</param>
    public ConfigurationLoadFailedException(string path, Exception innerException, string message = DefaultMessage)
        : base(message, innerException)
    {
        this.SettingsFilePath = path;
    }

    /// <summary>
    /// The file path to the settings file.
    /// </summary>
    public string SettingsFilePath { get; private set; }
}