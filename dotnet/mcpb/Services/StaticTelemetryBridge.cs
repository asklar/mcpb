namespace Mcpb.Services;

internal static class StaticTelemetryBridge
{
    private static TelemetryService? _active;
    private static readonly object _lock = new();
    private static Dictionary<string,string>? _pendingProperties;

    public static void Register(TelemetryService svc)
    {
        _active = svc;
    }

    public static void UpdateProjectType(string projectType)
    {
        try { _active?.UpdateProjectType(projectType, force:true); } catch { }
    }

    public static void AddProperty(string key, string value)
    {
        if (string.IsNullOrWhiteSpace(key)) return;
        lock (_lock)
        {
            _pendingProperties ??= new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            _pendingProperties[key] = value;
        }
    }

    // Called by TelemetryService at TrackCommand time to merge and clear
    internal static Dictionary<string,string>? DrainProperties()
    {
        lock (_lock)
        {
            if (_pendingProperties == null || _pendingProperties.Count == 0) return null;
            var copy = _pendingProperties;
            _pendingProperties = null;
            return copy;
        }
    }
}
