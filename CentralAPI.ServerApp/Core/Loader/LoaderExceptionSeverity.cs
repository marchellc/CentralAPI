namespace CentralAPI.ServerApp.Core.Loader;

/// <summary>
/// Defines the severity of an exception.
/// </summary>
public enum LoaderExceptionSeverity : byte
{
    /// <summary>
    /// This exception can be safely ignored.
    /// </summary>
    Low,
    
    /// <summary>
    /// This exception should not break any functions, but may cause them to behave weirdly.
    /// </summary>
    Moderate,
    
    /// <summary>
    /// This exception may cause some bugs.
    /// </summary>
    High,
    
    /// <summary>
    /// This exception may lead to an app crash.
    /// </summary>
    Critical
}