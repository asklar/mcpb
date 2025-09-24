namespace Mcpb.Services;

public interface ITelemetryService
{
    /// <summary>
    /// Tracks a command execution with timing and result information
    /// </summary>
    /// <param name="commandName">The name of the command executed</param>
    /// <param name="duration">How long the command took to execute</param>
    /// <param name="success">Whether the command succeeded</param>
    /// <param name="properties">Additional properties to track</param>
    void TrackCommand(string commandName, TimeSpan duration, bool success, IDictionary<string, string>? properties = null);

    /// <summary>
    /// Tracks an error that occurred during command execution
    /// </summary>
    /// <param name="exception">The exception that occurred</param>
    /// <param name="commandName">The command where the error occurred</param>
    /// <param name="properties">Additional properties to track</param>
    void TrackError(Exception exception, string? commandName = null, IDictionary<string, string>? properties = null);

    /// <summary>
    /// Flushes any pending telemetry data
    /// </summary>
    Task FlushAsync();

    /// <summary>
    /// Indicates whether telemetry is enabled
    /// </summary>
    bool IsEnabled { get; }
}