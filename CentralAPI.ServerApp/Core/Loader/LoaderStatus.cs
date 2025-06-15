namespace CentralAPI.ServerApp.Core.Loader;

/// <summary>
/// Defines the status of the loader - controls voluntary exit.
/// </summary>
public struct LoaderStatus
{
    /// <summary>
    /// The type of the loader status.
    /// </summary>
    public readonly LoaderStatusType Type = LoaderStatusType.Running;

    /// <summary>
    /// The return code.
    /// </summary>
    public readonly int Code = 0;
    
    /// <summary>
    /// The log message.
    /// </summary>
    public readonly string Message = "";

    /// <summary>
    /// Creates a new <see cref="LoaderStatus"/> instance.
    /// </summary>
    /// <param name="type">The status type.</param>
    /// <param name="code">Status code.</param>
    /// <param name="message">Status message.</param>
    public LoaderStatus(LoaderStatusType type, int code, string message)
    {
        Type = type;
        Code = code;
        Message = message;
    }
}