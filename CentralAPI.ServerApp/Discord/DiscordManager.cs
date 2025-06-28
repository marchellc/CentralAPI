using System.Collections.Concurrent;

using CentralAPI.ServerApp.Core.Configs;

namespace CentralAPI.ServerApp.Discord;

/// <summary>
/// Manages the Discord bot synchronization.
/// </summary>
public static class DiscordManager
{
    /// <summary>
    /// Gets the list of active Discord bots.
    /// </summary>
    public static volatile ConcurrentDictionary<string, DiscordBot> Bots = new();

    /// <summary>
    /// Gets the active Discord config.
    /// </summary>
    public static volatile DiscordConfig Config;
    
    internal static void Init()
    {
        Config = ConfigLoader.Load("discord", new DiscordConfig());

        foreach (var bot in Config.Tokens)
        {
            if (string.Equals(bot.Key, "exampleBot"))
                continue;

            var discordBot = new DiscordBot(bot.Value);

            discordBot.Name = bot.Key;
            discordBot.Start();
            
            Bots.TryAdd(bot.Key, discordBot);
        }
    }
}