namespace ZxenLib.Configuration;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ZxenLib.Infrastructure.Exceptions;

/// <summary>
/// Contains the game's individual configuration.
/// </summary>
public class Configuration
{
    private static readonly Type[] SupportedConfigValueTypes = new Type[]
    {
        typeof(int),
        typeof(uint),
        typeof(short),
        typeof(ushort),
        typeof(long),
        typeof(ulong),
        typeof(float),
        typeof(double),
        typeof(string),
        typeof(bool),
        typeof(byte)
    };

    /// <summary>
    /// Initializes a new instance of the <see cref="Configuration"/> class.
    /// </summary>
    public Configuration()
    {
        this.Resolution = new ResolutionConfiguration();
        this.ConfigProperties = new Dictionary<string, ConfigurationProperty>();
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
    /// Gets or sets the resolution cofiguration options.
    /// </summary>
    public ResolutionConfiguration Resolution { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the mouse is visible in the game. Default value is true.
    /// </summary>
    public bool IsMouseVisible { get; set; } = true;

    /// <summary>
    /// The configuration properties for this configuration.
    /// </summary>
    public Dictionary<string, ConfigurationProperty> ConfigProperties { get; set; }

    /// <summary>
    /// Gets the configuration property from the config dictionary.
    /// </summary>
    /// <param name="key">The key of the property.</param>
    /// <returns><see cref="ConfigurationProperty"/> matching they key.</returns>
    public ConfigurationProperty GetConfigProperty(string key)
    {
        return this.ConfigProperties[key];
    }

    /// <summary>
    /// Sets a config property in the config dictionary.
    /// Value <b>MUST</b> be a primitive type or string.
    /// </summary>
    /// <param name="key">The key of the property to set.</param>
    /// <param name="value">The value of the property to set.</param>
    public void SetConfigProperty(string key, object value)
    {
        Type valueType = value.GetType();
        if (!SupportedConfigValueTypes.Contains(valueType))
        {
            throw new UnsupportedConfigValueException(valueType);
        }

        var newProp = new ConfigurationProperty { PropertyName = key, PropertyType = valueType.Name, RawValue = value };
        if (this.ConfigProperties.ContainsKey(key))
        {
            this.ConfigProperties[key] = newProp;
            return;
        }

        this.ConfigProperties.Add(key, newProp);
    }

    /// <summary>
    /// Determines if the config dictionary contains the specified key.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <returns>True if key is present.</returns>
    public bool ContainsKey(string key)
    {
        return this.ConfigProperties.ContainsKey(key);
    }
}