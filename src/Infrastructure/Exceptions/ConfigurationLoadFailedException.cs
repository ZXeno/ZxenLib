namespace ZxenLib.Infrastructure.Exceptions
{
    using System;


    class ConfigurationLoadFailedException : Exception
    {
        private const string DEFAULT_MESSAGE = "The configuration file failed to load.";

        public string SettingsFilePath { get; private set; }

        public ConfigurationLoadFailedException() : base(DEFAULT_MESSAGE)
        {
        }

        public ConfigurationLoadFailedException(string path) : base(DEFAULT_MESSAGE)
        {
            this.SettingsFilePath = path;
        }

        public ConfigurationLoadFailedException(string path, string message = DEFAULT_MESSAGE) : base(message)
        {
            this.SettingsFilePath = path;
        }

        public ConfigurationLoadFailedException(string path, Exception innerException, string message = DEFAULT_MESSAGE) : base(message, innerException)
        {
            this.SettingsFilePath = path;
        }
    }
}
