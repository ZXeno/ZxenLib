﻿namespace ZxenLib.Infrastructure
{
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// Contains the game's individual configuration.
    /// </summary>
    public class Configuration
    {
        [JsonProperty]
        private Dictionary<string, ConfigurationProperty> configProperties;

        /// <summary>
        /// Initializes a new instance of the <see cref="Configuration"/> class.
        /// </summary>
        public Configuration()
        {
            this.Resolution = new ResolutionConfiguration();
            this.configProperties = new Dictionary<string, ConfigurationProperty>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Configuration"/> class.
        /// </summary>
        /// <param name="config"><see cref="Configuration"/> to build a new one from.</param>
        public Configuration(Configuration config)
        {
            PropertyInfo[] properties = config.GetType().GetProperties();
            foreach (PropertyInfo property in properties)
            {
                if (property.CanRead
                    && property.CanWrite
                    && !Attribute.IsDefined(property, typeof(ObsoleteAttribute)))
                {
                    var value = property.GetValue(config);
                    property.SetValue(this, value);
                }
            }
        }

        /// <summary>
        /// Gets the configuration property from the config dictionary.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public ConfigurationProperty GetConfigProperty(string key)
        {
            return this.configProperties[key];
        }

        /// <summary>
        /// Sets a config property in the config dictionary.
        /// Value <b>MUST</b> be a primitive type or string.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void SetConfigProperty(string key, object value)
        {
            var newProp = new ConfigurationProperty { PropertyName = key, PropertyType = value.GetType().Name, RawValue = value };
            if (this.configProperties.ContainsKey(key))
            {
                this.configProperties[key] = newProp;
                return;
            }

            this.configProperties.Add(key, newProp);
        }

        /// <summary>
        /// Determines if the config dictionary contains the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>True if key is present.</returns>
        public bool ContainsKey(string key)
        {
            return this.configProperties.ContainsKey(key);
        }

        /// <summary>
        /// Gets or sets the resolution cofiguration options.
        /// </summary>
        public ResolutionConfiguration Resolution { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the mouse is visible in the game. Default value is true.
        /// </summary>
        public bool IsMouseVisible { get; set; } = true;
    }
}