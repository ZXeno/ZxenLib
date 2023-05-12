namespace ZxenLib.Configuration;

using System.Threading.Tasks;

public interface IConfigurationManager
{
    string GameName { get; set; }

    /// <summary>
    /// Gets or sets the current configuration.
    /// </summary>
    Configuration Config { get; set; }

    /// <summary>
    /// Gets or sets the path to the configuration directory and updates the filePath automatically.
    /// </summary>
    string GameSettingsDirectory { get; set; }

    /// <summary>
    /// Loads the configuration file from the specified directory.
    ///
    /// The default directory is "%userprofile%\Documents\My Games\MyGame".
    /// </summary>
    Task LoadConfiguration();

    /// <summary>
    /// Saves the current configuration file to the path value in the SettingsDirectory property.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task SaveConfiguration();

    /// <summary>
    /// Gets a configuration setting from the custom configuration properties dictionary.
    /// </summary>
    /// <typeparam name="T">The type the configuration property should return.</typeparam>
    /// <param name="key">The property name.</param>
    /// <returns>The object as the typeparam.</returns>
    T GetConfigOption<T>(string key);

    /// <summary>
    /// Sets a custom config value to the <see cref="Configuration"/>.
    ///
    /// This will <b>only accepts</b> primitive types and strings. Throws an
    /// exception if provided value is not primitive or string.
    /// </summary>
    /// <param name="key">The key for this option.</param>
    /// <param name="value">The value for this option.</param>
    void SetConfigOption(string key, object value);
}