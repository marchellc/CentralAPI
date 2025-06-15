using System.Reflection;
using CommonLib;

namespace CentralAPI.ServerApp;

using Core.Loader;

/// <summary>
/// An entrypoint for the loader.
/// </summary>
public static class Program
{
    private static volatile Assembly assembly;

    /// <summary>
    /// Gets the application's assembly.
    /// </summary>
    public static Assembly Assembly => assembly;

    /// <summary>
    /// The main entrypoint.
    /// </summary>
    /// <param name="args">Startup arguments.</param>
    public static int Main(string[] args)
    {
        assembly = typeof(Program).Assembly;

        Loader.Start();

        while (true)
        {
            Loader.Update();

            if (Loader.Status.Type is LoaderStatusType.Stopping)
                break;
        }

        if (Loader.Status.Message?.Length > 1)
            Console.WriteLine(Loader.Status.Message);

        return Loader.Status.Code;
    }
}