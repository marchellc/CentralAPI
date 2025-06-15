namespace CentralAPI.ServerApp.Core.Loader;

/// <summary>
/// Defines a reported exception.
/// </summary>
public struct LoaderException
{
    /// <summary>
    /// The exception.
    /// </summary>
    public readonly Exception Exception;

    /// <summary>
    /// Category of the operation.
    /// </summary>
    public readonly string Category;

    /// <summary>
    /// Name of the operation.
    /// </summary>
    public readonly string Operation;

    /// <summary>
    /// Custom value.
    /// </summary>
    public readonly object Value;

    /// <summary>
    /// The exception's severity.
    /// </summary>
    public readonly LoaderExceptionSeverity Severity;

    /// <summary>
    /// Creates a new <see cref="LoaderException"/> instance.
    /// </summary>
    /// <param name="exception">The exception.</param>
    /// <param name="category">Operation category.</param>
    /// <param name="operation">Operation name.</param>
    /// <param name="severity">Exception severity.</param>
    /// <param name="value">Operation value.</param>
    public LoaderException(Exception exception, string category, string operation, LoaderExceptionSeverity severity, object value)
    {
        Exception = exception;
        Category = category;
        Operation = operation;
        Severity = severity;
        Value = value;
    }
}