namespace ZxenLib.Infrastructure.Exceptions;

using System;

/// <summary>
/// Defines an exception for configuration file write failures.
/// </summary>
public class ConfigurationWriteFailedException : Exception
{
    private const string DefaultMessage = "The configuration file failed to write to disk.";

    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigurationWriteFailedException"/> class.
    /// </summary>
    public ConfigurationWriteFailedException()
        : base(DefaultMessage)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigurationWriteFailedException"/> class.
    /// </summary>
    /// <param name="path">The path to the settings file.</param>
    public ConfigurationWriteFailedException(string path)
        : base(DefaultMessage)
    {
        this.SettingsFilePath = path;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigurationWriteFailedException"/> class.
    /// </summary>
    /// <param name="path">The path to the settings file.</param>
    /// <param name="message">Exception message.</param>
    public ConfigurationWriteFailedException(string path, string message = DefaultMessage)
        : base(message)
    {
        this.SettingsFilePath = path;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigurationWriteFailedException"/> class.
    /// </summary>
    /// <param name="path">The path to the settings file.</param>
    /// <param name="innerException">Reference to the inner <see cref="Exception"/> that is the cause of this exception.</param>
    /// <param name="message">Exception message.</param>
    public ConfigurationWriteFailedException(string path, Exception innerException, string message = DefaultMessage)
        : base(message, innerException)
    {
        this.SettingsFilePath = path;
    }

    /// <summary>
    /// The file path to the settings file.
    /// </summary>
    public string SettingsFilePath { get; private set; }
}