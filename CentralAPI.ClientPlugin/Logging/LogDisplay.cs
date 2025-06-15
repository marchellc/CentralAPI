using CommonLib;

using LabExtended.Core;
using LabExtended.Utilities.Update;

namespace CentralAPI.ClientPlugin.Logging;

/// <summary>
/// Displays CommonLibrary logs in the server console.
/// </summary>
public static class LogDisplay
{
    private static async Task Update()
    {
        while (CommonLog.Logs.TryDequeue(out var entry))
        {
            switch (entry.Level)
            {
                case CommonLog.LogLevel.Info:
                    ApiLog.Info(entry.Source, entry.Message);
                    break;
                
                case CommonLog.LogLevel.Warn:
                    ApiLog.Warn(entry.Source, entry.Message);
                    break;
                
                case CommonLog.LogLevel.Error:
                    ApiLog.Error(entry.Source, entry.Message);
                    break;
                
                case CommonLog.LogLevel.Debug when CommonLog.IsDebugEnabled:
                    ApiLog.Debug(entry.Source, entry.Message);
                    break;
            }
        }
    }
    
    internal static void Init()
    {
        PlayerUpdateHelper.OnThreadUpdate += Update;
    }
}