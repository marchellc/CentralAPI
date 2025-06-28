using CentralAPI.ServerApp.Core.Loader; 

using Discord;
using Discord.WebSocket;

namespace CentralAPI.ServerApp.Discord;

/// <summary>
/// A discord bot.
/// </summary>
public class DiscordBot
{
    /// <summary>
    /// Gets called when a bot completes connecting.
    /// </summary>
    public static event Action<DiscordBot>? Connected;

    /// <summary>
    /// Gets called when a bot disconnects.
    /// </summary>
    public static event Action<DiscordBot>? Disconnected; 
    
    private volatile Task startTask;
    private volatile Task stopTask;

    private volatile string token;

    /// <summary>
    /// Gets the bot's name.
    /// </summary>
    public volatile string Name;

    /// <summary>
    /// Whether or not the bot is ready.
    /// </summary>
    public volatile bool IsReady;

    /// <summary>
    /// Gets the bot's client.
    /// </summary>
    public volatile DiscordSocketClient Client;

    /// <summary>
    /// Creates a new <see cref="DiscordBot"/> instance.
    /// </summary>
    /// <param name="token">The bot's Discord token.</param>
    /// <exception cref="ArgumentNullException"></exception>
    public DiscordBot(string token)
    {
        if (string.IsNullOrEmpty(token))
            throw new ArgumentNullException(nameof(token));

        this.token = token;
    }

    /// <summary>
    /// Starts the bot.
    /// </summary>
    public void Start()
    {
        if (startTask != null || stopTask != null)
            return;

        Client = new();
        
        startTask = Task.Run(async () =>
        {
            try
            {
                await Client.StartAsync();
                await Client.LoginAsync(TokenType.Bot, token, true);
            }
            catch (Exception ex)
            {
                Loader.Report(ex, LoaderExceptionSeverity.High, "Discord", "BotStart", null);
            }

            startTask = null;

            IsReady = true;
        });
    }

    /// <summary>
    /// Stops the bot.
    /// </summary>
    public void Stop()
    {
        if (stopTask != null)
            return;

        stopTask = Task.Run(async () =>
        {
            try
            {
                IsReady = false;
                
                if (Client != null)
                {
                    await Client.LogoutAsync();
                    await Client.DisposeAsync();

                    Client = null;
                }
            }
            catch (Exception ex)
            {
                Loader.Report(ex, LoaderExceptionSeverity.Moderate, "Discord", "BotStop", null);
            }

            stopTask = null;
        });
    }
}