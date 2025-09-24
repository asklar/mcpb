using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.ApplicationInsights.Extensibility.Implementation.ApplicationId;

namespace Mcpb.Services;

public class TelemetryService : ITelemetryService, IDisposable
{
    private readonly TelemetryClient? _telemetryClient;
    private readonly TelemetryConfiguration? _configuration;
    private readonly bool _isEnabled;
    private string _projectType = "unknown";

    public TelemetryService()
    {
        // Check if telemetry is disabled via environment variable
        var disableTelemetry = Environment.GetEnvironmentVariable("MCPB_DISABLE_TELEMETRY");
        if (!string.IsNullOrEmpty(disableTelemetry) &&
            (disableTelemetry.Equals("1", StringComparison.OrdinalIgnoreCase) ||
             disableTelemetry.Equals("true", StringComparison.OrdinalIgnoreCase)))
        {
            _isEnabled = false;
            return;
        }

        // Get the instrumentation key from build-time injection
        var instrumentationKey = GetInstrumentationKey();
        if (string.IsNullOrEmpty(instrumentationKey))
        {
            _isEnabled = false;
            return;
        }

        try
        {
            _configuration = TelemetryConfiguration.CreateDefault();

            // Use connection string format for modern Application Insights
            _configuration.ConnectionString = $"InstrumentationKey={instrumentationKey};IngestionEndpoint=https://eastus-8.in.applicationinsights.azure.com/;LiveEndpoint=https://eastus.livediagnostics.monitor.azure.com/;ApplicationId=6ac82b1d-de82-4a24-bd69-70077e85b949";
            // Configure for minimal data collection
            _configuration.TelemetryInitializers.Add(new TelemetryInitializer());

            _telemetryClient = new TelemetryClient(_configuration);
            _projectType = ProjectTypeDetector.Detect(); // fallback; may be overridden by manifest
            _isEnabled = true;
        }
        catch
        {
            // If telemetry initialization fails, disable it silently
            _isEnabled = false;
        }
    }

    public bool IsEnabled => _isEnabled;

    // Allow manifest-driven override; first non-unknown wins, or explicit override forces when force=true
    public void UpdateProjectType(string? projectType, bool force = false)
    {
        if (!_isEnabled) return;
        if (string.IsNullOrWhiteSpace(projectType)) return;
        if (!force && _projectType != "unknown") return;
        _projectType = projectType.Trim().ToLowerInvariant();
    }
    public void TrackCommand(string commandName, TimeSpan duration, bool success, IDictionary<string, string>? properties = null)
    {
        if (!_isEnabled || _telemetryClient == null) return;

        try
        {
            var eventProperties = new Dictionary<string, string>
            {
                ["command"] = commandName,
                ["success"] = success.ToString(),
                ["duration_ms"] = duration.TotalMilliseconds.ToString("F0")
            };

            if (!eventProperties.ContainsKey("projectType") && !string.IsNullOrEmpty(_projectType))
            {
                eventProperties["projectType"] = _projectType;
            }

            // Merge any ad-hoc properties queued via the static bridge first
            try
            {
                var drained = StaticTelemetryBridge.DrainProperties();
                if (drained != null)
                {
                    foreach (var kvp in drained)
                    {
                        eventProperties[kvp.Key] = kvp.Value;
                    }
                }
            }
            catch { }

            if (properties != null)
            {
                foreach (var kvp in properties)
                {
                    eventProperties[kvp.Key] = kvp.Value;
                }
            }

            // Emit event with name derived from command, e.g. mcpb.pack, mcpb.sign
            var eventName = $"mcpb.{commandName.ToLowerInvariant()}";
            _telemetryClient.TrackEvent(eventName, eventProperties);
        }
        catch
        {
            // Silently ignore telemetry errors
        }
    }

    public void TrackError(Exception exception, string? commandName = null, IDictionary<string, string>? properties = null)
    {
        if (!_isEnabled || _telemetryClient == null) return;

        try
        {
            var eventProperties = new Dictionary<string, string>();

            if (!string.IsNullOrEmpty(commandName))
            {
                eventProperties["command"] = commandName;
            }

            if (properties != null)
            {
                foreach (var kvp in properties)
                {
                    eventProperties[kvp.Key] = kvp.Value;
                }
            }

            if (!eventProperties.ContainsKey("projectType") && !string.IsNullOrEmpty(_projectType))
            {
                eventProperties["projectType"] = _projectType;
            }

            _telemetryClient.TrackException(exception, eventProperties);
        }
        catch
        {
            // Silently ignore telemetry errors
        }
    }

    public async Task FlushAsync()
    {
        if (!_isEnabled || _telemetryClient == null) return;

        try
        {
            _telemetryClient.Flush();
            // Wait a bit for telemetry to be sent
            await Task.Delay(TimeSpan.FromSeconds(2));
        }
        catch
        {
            // Silently ignore telemetry errors
        }
    }

    private static string GetInstrumentationKey() =>
             Mcpb.Generated.InstrumentationConfig.InstrumentationKey;

    public void Dispose()
    {
        _telemetryClient?.Flush();
        _configuration?.Dispose();
    }
}

/// <summary>
/// Custom telemetry initializer to set minimal required properties
/// </summary>
internal class TelemetryInitializer : ITelemetryInitializer
{
    public void Initialize(ITelemetry telemetry)
    {
        // Set a generic user ID based on machine (but not personally identifiable)
        if (string.IsNullOrEmpty(telemetry.Context.User.Id))
        {
            telemetry.Context.User.Id = GenerateAnonymousUserId();
        }

        // Set the application version
        if (string.IsNullOrEmpty(telemetry.Context.Component.Version))
        {
            telemetry.Context.Component.Version = typeof(TelemetryService).Assembly.GetName().Version?.ToString() ?? "unknown";
        }
    }

    private static string GenerateAnonymousUserId()
    {
        try
        {
            // Create a deterministic but anonymous user ID based on machine name + username hash
            var machineInfo = $"{Environment.MachineName}-{Environment.UserName}";
            var hash = System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(machineInfo));
            return Convert.ToHexString(hash)[..16]; // Take first 16 characters
        }
        catch
        {
            return "anonymous";
        }
    }
}