using System.Collections.Concurrent;
using System.Reflection;

using CentralAPI.ServerApp.Core.Logger;
using CentralAPI.ServerApp.Databases;
using CentralAPI.ServerApp.Network;
using CentralAPI.ServerApp.Server;

using CommonLib;
using CommonLib.Utilities.Console;

namespace CentralAPI.ServerApp.Core.Loader;

/// <summary>
/// Loads the server application.
/// </summary>
public static class Loader
{
    private static LoaderStatus status = new(LoaderStatusType.Starting, 0, string.Empty);
    private static volatile uint loopNumber = 0;
    
    /// <summary>
    /// Gets the status of the loader.
    /// </summary>
    public static LoaderStatus Status => status;

    /// <summary>
    /// Gets called before the application starts exiting.
    /// </summary>
    public static event Action? Exiting;

    /// <summary>
    /// Gets called a lot.
    /// </summary>
    public static event Action? Updated;

    /// <summary>
    /// Gets called when an exception is reported.
    /// </summary>
    public static event Action<LoaderException>? Reported;

    /// <summary>
    /// Gets the number of the current loop.
    /// </summary>
    public static uint Loop => loopNumber;
    
    /// <summary>
    /// List of loaded dependencies.
    /// </summary>
    public static volatile ConcurrentStack<Assembly> Dependencies = new();

    /// <summary>
    /// List of loaded assemblies.
    /// </summary>
    public static volatile ConcurrentStack<Assembly> Assemblies = new();

    /// <summary>
    /// Quits the application.
    /// </summary>
    /// <param name="code">The app exit code.</param>
    /// <param name="message">The message to log before exiting.</param>
    public static void Quit(int code = 0, string message = "")
    {
        if (status.Type != LoaderStatusType.Running)
            return;

        if (message is null)
            message = string.Empty;

        try
        {
            Exiting?.Invoke();
        }
        catch (Exception ex)
        {
            message += $"\nExiting exception: {ex}";
        }

        status = new(LoaderStatusType.Stopping, code, message);
    }

    /// <summary>
    /// Reports an exception.
    /// </summary>
    /// <param name="exception">The exception.</param>
    /// <param name="severity">The exception's severity.</param>
    /// <param name="category">The operation's category.</param>
    /// <param name="operation">The operation that caused the exception.</param>
    /// <param name="value">A custom value.</param>
    /// <exception cref="ArgumentNullException"></exception>
    public static void Report(Exception exception, LoaderExceptionSeverity severity, string category, string operation, object value)
    {
        if (exception is null)
            throw new ArgumentNullException(nameof(exception));
        
        if (string.IsNullOrEmpty(category))
            throw new ArgumentNullException(nameof(category));
        
        if (string.IsNullOrEmpty(operation))
            throw new ArgumentNullException(nameof(operation));

        try
        {
            Reported?.Invoke(new(exception, category, operation, severity, value));
        }
        catch
        {
            // ignored
        }
    }

    internal static void Start()
    {
        LogDisplay.Init();
        
        CommonLibrary.Initialize(Environment.GetCommandLineArgs());
        CommonLog.IsDebugEnabled = ConsoleArgs.HasSwitch("ShowDebug");
        
        var depsPath = Path.Combine(Directory.GetCurrentDirectory(), "dependencies");
        var asmPath = Path.Combine(Directory.GetCurrentDirectory(), "assemblies");
        
        if (!Directory.Exists(depsPath))
            Directory.CreateDirectory(depsPath);
        
        if (!Directory.Exists(asmPath))
            Directory.CreateDirectory(asmPath);
        
        CommonLog.Info("Loader", "Loading dependencies ..");

        foreach (var file in Directory.GetFiles(depsPath, "*.dll"))
        {
            try
            {
                var raw = File.ReadAllBytes(file);
                var assembly = Assembly.Load(raw);
                
                Dependencies.Push(assembly);
            }
            catch (Exception ex)
            {
                Report(ex, LoaderExceptionSeverity.High, "Startup", "DepsLoad", Path.GetFileName(file));
            }
        }
        
        CommonLog.Info("Loader", "Loading assemblies ..");

        foreach (var file in Directory.GetFiles(asmPath, "*.dll"))
        {
            try
            {
                var raw = File.ReadAllBytes(file);
                var assembly = Assembly.Load(raw);
                
                Assemblies.Push(assembly);
            }
            catch (Exception ex)
            {
                Report(ex, LoaderExceptionSeverity.High, "Startup", "AssembliesLoad", Path.GetFileName(file));
            }
        }
        
        Assemblies.Push(Program.Assembly);
        
        CommonLog.Info("Loader", "Starting the server ..");
        
        NetworkServer.Init();
        ScpManager.Init();
        
        CommonLog.Info("Loader", "Server started, calling EntryLoad methods ..");

        foreach (var assembly in Assemblies)
        {
            foreach (var type in assembly.DefinedTypes)
            {
                foreach (var method in type.DeclaredMethods)
                {
                    try
                    {
                        if (method.IsStatic
                            && method.GetParameters().Length == 0
                            && method.Name == "EntryLoad")
                        {
                            method.Invoke(null, []);
                        }
                    }
                    catch (Exception ex)
                    {
                        Report(ex, LoaderExceptionSeverity.Moderate, "Startup", "StartAttributes", method);
                    }
                }
            }
        }
        
        DatabaseDirector.Init();
        
        CommonLog.Info("Loader", "Startup finished.");
    }

    internal static void Update()
    {
        loopNumber = loopNumber + 1;
        
        try
        {
            Updated?.Invoke();
        }
        catch (Exception ex)
        {
            Report(ex, LoaderExceptionSeverity.Low, "LoaderLoop", "Update", null);
        }

        if (loopNumber + 1 >= uint.MaxValue)
            loopNumber = 0;
    }
}