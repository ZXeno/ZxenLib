namespace ZxenLib.Infrastructure
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text.Json;
    using System.Threading.Tasks;
    using ZxenLib.Infrastructure.Exceptions;

    /// <summary>
    /// Manages game configuration.
    /// </summary>
    public class ConfigurationManager
    {
        private const string FileName = "ConfigurationSettings.json";
        private string settingsDirectory = string.Empty;
        private string filePath = string.Empty;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigurationManager"/> class.
        /// </summary>
        public ConfigurationManager()
        {
            this.Config = new Configuration();
            this.GameSettingsDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "My Games", "MyGame");
            this.filePath = Path.Combine(this.settingsDirectory, FileName);
        }

        /// <summary>
        /// Gets or sets the current configuration.
        /// </summary>
        public Configuration Config { get; set; }

        /// <summary>
        /// Gets or sets the path to the configuration directory and updates the filePath automatically.
        /// </summary>
        public string GameSettingsDirectory
        {
            get => this.settingsDirectory;
            set
            {
                this.settingsDirectory = value;
                this.filePath = Path.Combine(this.settingsDirectory, FileName);
            }
        }

        /// <summary>
        /// Loads the configuration file from the specified directory.
        ///
        /// The default directory is "%userprofile%\Documents\My Games\MyGame".
        /// </summary>
        public void LoadConfiguration()
        {
            Configuration newConfig = new Configuration();

            if (File.Exists(this.filePath))
            {
                string inputJson = string.Empty;

                try
                {
                    using var reader = File.OpenText(this.filePath);
                    inputJson = reader.ReadToEnd();
                }
                catch (Exception ex)
                {
                    throw new ConfigurationLoadFailedException(this.filePath, ex);
                }

                try
                {
                    if (!string.IsNullOrWhiteSpace(inputJson))
                    {
                        this.Config = JsonSerializer.Deserialize<Configuration>(inputJson);
                    }
                }
                catch (Exception ex)
                {
                    throw new ConfigurationLoadFailedException(this.filePath, ex);
                }
            }
            else
            {
                try
                {
                    this.CreateConfigFileIfNotExist();
                }
                catch (Exception ex)
                {
                    throw new ConfigurationWriteFailedException(this.filePath, ex);
                }

                this.Config = newConfig;
                Task.Factory.StartNew(async () => { await this.SaveConfiguration(); }).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Saves the current configuration file to the path value in the SettingsDirectory property.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task SaveConfiguration()
        {
            string serializedConfig = string.Empty;

            lock (this.Config)
            {
                serializedConfig = JsonSerializer.Serialize(this.Config);
            }

            if (string.IsNullOrWhiteSpace(serializedConfig))
            {
                return;
            }

            try
            {
                this.CreateConfigFileIfNotExist();
            }
            catch (Exception ex)
            {
                throw new ConfigurationWriteFailedException(this.filePath, ex);
            }

            using StreamWriter sw = new StreamWriter(this.filePath);
            try
            {
                await sw.WriteAsync(serializedConfig);
            }
            catch (Exception ex)
            {
                throw new ConfigurationWriteFailedException(this.filePath, ex);
            }
        }

        /// <summary>
        /// Gets a configuration setting from the custom configuration properties dictionary.
        /// </summary>
        /// <typeparam name="T">The type the configuration property should return.</typeparam>
        /// <param name="key">The property name.</param>
        /// <returns>The object as the typeparam.</returns>
        public T GetConfigOption<T>(string key)
        {
            if (this.Config.ContainsKey(key))
            {
                ConfigurationProperty value = this.Config.GetConfigProperty(key);
                if (value.PropertyType == typeof(T).Name)
                {
                    return value.GetExpectedType<T>();
                }
                else
                {
                    throw new ExpectedTypeMismatchException(typeof(T), value.PropertyType);
                }
            }
            else
            {
                throw new KeyNotFoundException("The specified configuration key is not in the dictionary.");
            }
        }

        /// <summary>
        /// Sets a custom config value to the <see cref="Configuration"/>.
        ///
        /// This will <b>only accepts</b> primitive types and strings. Throws an
        /// exception if provided value is not primitive or string.
        /// </summary>
        /// <param name="key">The key for this option.</param>
        /// <param name="value">The value for this option.</param>
        public void SetConfigOption(string key, object value)
        {
            if (!value.GetType().IsPrimitive || value.GetType() != typeof(string))
            {
                throw new UnsupportedConfigValueException(value.GetType());
            }

            this.Config.SetConfigProperty(key, value);
        }

        private void CreateConfigFileIfNotExist()
        {
            if (!Directory.Exists(this.settingsDirectory))
            {
                Directory.CreateDirectory(this.settingsDirectory);
            }

            if (!File.Exists(this.filePath))
            {
                using StreamWriter sw = File.CreateText(this.filePath);
                sw.Close();
            }
        }
    }
}