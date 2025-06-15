using CentralAPI.ServerApp.Core.Loader;
using CommonLib;

namespace CentralAPI.ServerApp.Core.Logger;

/// <summary>
/// Displays logs from Common Library.
/// </summary>
public static class LogDisplay
{
    internal static void Init()
    {
        Task.Run(Update);

        Loader.Loader.Reported += Report;
    }

    private static void Report(LoaderException exception)
    {
        if (exception.Exception is null)
            return;
        
        CommonLog.Error(exception.Category, $"Exception occured while handling operation '{exception.Operation} ({exception.Severity})':\n{exception.Exception}");
    }
    
    private static void Update()
    {
        while (true)
        {
            while (CommonLog.Logs.TryDequeue(out var entry))
            {
                try
                {
                    switch (entry.Level)
                    {
                        case CommonLog.LogLevel.Debug when CommonLog.IsDebugEnabled:
                            Output(ConsoleColor.Cyan, ConsoleColor.White, "DEBUG", entry.Source, entry.Message);
                            break;

                        case CommonLog.LogLevel.Info:
                            Output(ConsoleColor.Green, ConsoleColor.White, "INFO", entry.Source, entry.Message);
                            break;

                        case CommonLog.LogLevel.Warn:
                            Output(ConsoleColor.Yellow, ConsoleColor.White, "WARN", entry.Source, entry.Message);
                            break;

                        case CommonLog.LogLevel.Error:
                            Output(ConsoleColor.Red, ConsoleColor.White, "ERROR", entry.Source, entry.Message);
                            break;
                    }
                }
                catch
                {
                    // ignored
                }
            }
        }
    }
    
    private static void Output(ConsoleColor tagColor, ConsoleColor textColor, string tag, string source, string message)
    {
        Console.ForegroundColor = tagColor;
        
        Console.Write("[");
        Console.Write(DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss"));
        Console.Write("] ");
        
        Console.Write("[ ");
        Console.Write(tag);
        Console.Write("] ");
        
        Console.Write(" [ ");
        Console.Write(source.ToUpper());
        Console.Write("] ");
        
        Console.ForegroundColor = textColor;
        
        Console.Write(message);
        Console.WriteLine();
        
        Console.ResetColor();
    }
}