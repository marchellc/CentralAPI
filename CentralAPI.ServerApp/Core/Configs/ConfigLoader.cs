using CommonLib;
using Newtonsoft.Json;

namespace CentralAPI.ServerApp.Core.Configs;

/// <summary>
/// Class responsible for loading and saving configuration files.
/// </summary>
public static class ConfigLoader
{
    /// <summary>
    /// Reads the config object from its file.
    /// </summary>
    /// <param name="configName">Name of the config file (without extension).</param>
    /// <param name="defaultConfig">Default config value.</param>
    /// <typeparam name="T">Config type.</typeparam>
    /// <returns>The loaded config object.</returns>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="Exception"></exception>
    public static T Load<T>(string configName, T defaultConfig)
    {
        if (string.IsNullOrEmpty(configName))
            throw new ArgumentException("configName cannot be null or empty");
        
        if (defaultConfig is null)
            throw new ArgumentException("defaultConfig cannot be null");

        var directory = Path.Combine(Directory.GetCurrentDirectory(), "configuration");
        
        if (!Directory.Exists(directory))
            Directory.CreateDirectory(directory);
        
        var path = Path.Combine(directory, configName + ".json");
        
        CommonLog.Debug("Config Loader", $"Loading config file '{configName} ({typeof(T).Name})' from '{path}'");

        if (!File.Exists(path))
        {
            CommonLog.Debug("Config Loader", "Config file does not exist, creating default ..");
            
            Write(configName, defaultConfig);
            return defaultConfig;
        }
        
        var json = File.ReadAllText(path);
        
        return JsonConvert.DeserializeObject<T>(json) ?? throw new Exception($"Could not load config '{configName}'");
    }

    /// <summary>
    /// Writes a config object to its file.
    /// </summary>
    /// <param name="configName">The name of the config file (without extension)</param>
    /// <param name="config">The config object.</param>
    /// <typeparam name="T">Config type.</typeparam>
    public static void Write<T>(string configName, T config)
    {
        var directory = Path.Combine(Directory.GetCurrentDirectory(), "configuration");
        
        if (!Directory.Exists(directory))
            Directory.CreateDirectory(directory);
        
        var path = Path.Combine(directory, configName + ".json");
        var json = JsonConvert.SerializeObject(config, Formatting.Indented);
        
        File.WriteAllText(path, json);
    }
}