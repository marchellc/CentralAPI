using System.Net;
using CentralAPI.ClientPlugin.Extensions;
using CentralAPI.ClientPlugin.Network;
using CentralAPI.ClientPlugin.PlayerProfiles;
using CentralAPI.ClientPlugin.PlayerProfiles.Internal;
using LabExtended.Commands;
using LabExtended.Commands.Attributes;
using LabExtended.Commands.Interfaces;
using LabExtended.Extensions;
using LabExtended.Utilities;

namespace CentralAPI.ClientPlugin.Commands.Profiles;

[Command("profile", "Commands for player profile management.")]
public class ProfileCommand : CommandBase, IServerSideCommand
{
    [CommandOverload("Shows the status.")]
    public void Status()
    {
        if (NetworkClient.Scp is null)
        {
            Fail($"The network is disconnected.");
            return;
        }
        
        Ok($"Received {PlayerProfileManager.Profiles.Count} profile(s) from the server.");
    }

    [CommandOverload("find", "Finds a specific player profile.")]
    public void Find(
        [CommandParameter("Query", "The search query (user ID, IP, nickname)")] string query)
    {
        if (NetworkClient.Scp is null)
        {
            Fail($"The network is disconnected.");
            return;
        }

        if (PlayerProfileManager.Profiles.Count < 1)
        {
            Fail("No profiles were received.");
            return;
        }

        query = query.Trim();

        PlayerProfileInstance? result = null;

        IPAddress.TryParse(query, out var ipQuery);

        foreach (var profile in PlayerProfileManager.Profiles)
        {
            if (string.Equals(query, profile.Key, StringComparison.InvariantCultureIgnoreCase))
            {
                result = profile.Value;
                break;
            }

            if (string.Equals(query, profile.Key.Split('@')[0], StringComparison.InvariantCultureIgnoreCase))
            {
                result = profile.Value;
                break;
            }

            if (!string.IsNullOrWhiteSpace(profile.Value.UsernameCache.LastValue))
            {
                if (string.Equals(profile.Value.UsernameCache.LastValue, query,
                        StringComparison.InvariantCultureIgnoreCase))
                {
                    result = profile.Value;
                    break;
                }
            }

            if (ipQuery != null && profile.Value.AddressCache.LastValue != null &&
                ipQuery.Equals(profile.Value.AddressCache.LastValue))
            {
                result = profile.Value;
                break;
            }
        }

        if (result is null)
        {
            Fail($"Could not find a profile matching your query: {query}");
            return;
        }

        Ok(x =>
        {
            x.AppendLine($"Found a profile for your query: {query}");
            x.AppendLine($" - ID: {result.UserId}");
            x.AppendLine($" - Creation Timestamp: {result.CreationTimestamp.ToString("F")}");
            x.AppendLine($" - Activity Timestamp: {result.CreationTimestamp.ToString("F")}");
            x.AppendLine(
                $" - Username: {result.UsernameCache.LastValue ?? "(null)"} ({result.UsernameCache.LastTimestamp.ToString("F")}");

            foreach (var cached in result.UsernameCache.History)
            {
                if (cached.Key.Ticks != result.UsernameCache.LastTimestamp.Ticks)
                {
                    x.AppendLine($"  -> {cached.Value} ({cached.Key.ToString("F")}");
                }
            }

            x.AppendLine($" - Address: {result.AddressCache.LastValue?.ToString() ?? "(null)"}");

            foreach (var cached in result.AddressCache.History)
            {
                if (cached.Key.Ticks != result.AddressCache.LastTimestamp.Ticks)
                {
                    x.AppendLine($"  -> {cached.Value?.ToString() ?? "(null)"} ({cached.Key.ToString("F")})");
                }
            }
        });
    }

    [CommandOverload("property", "Gets the value of a property.")]
    public void Property(
        [CommandParameter("Query", "The search query (user ID, IP, nickname)")] string query,
        [CommandParameter("Property", "The name of the property to get the value of.")] string property)
    {
        if (NetworkClient.Scp is null)
        {
            Fail($"The network is disconnected.");
            return;
        }

        if (PlayerProfileManager.Profiles.Count < 1)
        {
            Fail("No profiles were received.");
            return;
        }

        query = query.Trim();

        PlayerProfileInstance? result = null;

        IPAddress.TryParse(query, out var ipQuery);

        foreach (var profile in PlayerProfileManager.Profiles)
        {
            if (string.Equals(query, profile.Key, StringComparison.InvariantCultureIgnoreCase))
            {
                result = profile.Value;
                break;
            }

            if (string.Equals(query, profile.Key.Split('@')[0], StringComparison.InvariantCultureIgnoreCase))
            {
                result = profile.Value;
                break;
            }

            if (!string.IsNullOrWhiteSpace(profile.Value.UsernameCache.LastValue))
            {
                if (string.Equals(profile.Value.UsernameCache.LastValue, query,
                        StringComparison.InvariantCultureIgnoreCase))
                {
                    result = profile.Value;
                    break;
                }
            }

            if (ipQuery != null && profile.Value.AddressCache.LastValue != null &&
                ipQuery.Equals(profile.Value.AddressCache.LastValue))
            {
                result = profile.Value;
                break;
            }
        }

        if (result is null)
        {
            Fail($"Could not find a profile matching your query: {query}");
            return;
        }

        PlayerProfilePropertyBase? resultProperty = null;

        foreach (var target in result.Properties)
        {
            if (string.Equals(target.Key, property, StringComparison.InvariantCultureIgnoreCase))
            {
                resultProperty = target.Value;
                break;
            }
        }

        if (resultProperty is null)
        {
            Fail($"Could not fínd property '{property}' in profile '{result.UserId}'");
            return;
        }

        if (resultProperty.GetType().OverridesToString())
        {
            Ok($"Found property '{property}' in profile '{result.UserId}': {resultProperty.ToString()}");
            return;
        }

        if (resultProperty.ObjectValue is null)
        {
            Ok($"Found property '{property}' in profile '{result.UserId}': (null)");
            return;
        }
        
        Ok($"Found property '{property}' in profile '{result.UserId}': {resultProperty.ObjectValue}");
    }
}