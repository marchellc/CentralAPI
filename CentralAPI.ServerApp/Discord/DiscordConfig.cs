using Newtonsoft.Json;

namespace CentralAPI.ServerApp.Discord;

/// <summary>
/// Discord server configuration.
/// </summary>
public class DiscordConfig
{
    /// <summary>
    /// Bot tokens.
    /// </summary>
    [JsonProperty("tokens")]
    public Dictionary<string, string> Tokens { get; set; } = new()
    {
        ["exampleBot"] = "exampleToken"
    };
}