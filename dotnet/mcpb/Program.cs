using Mcpb.Commands;
using Mcpb.Services;
using System.CommandLine;
using System.Diagnostics;

// Initialize telemetry service
using var telemetryService = new TelemetryService();
Mcpb.Services.StaticTelemetryBridge.Register(telemetryService);

var stopwatch = Stopwatch.StartNew();
var root = CliRoot.Build();

// Attempt to detect project type from an actual manifest file (not assuming name)
try
{
    var cwd = Directory.GetCurrentDirectory();
    var candidateFiles = new List<string>();

    // 1. Any json file paths passed explicitly in args
    foreach (var a in args)
    {
        try
        {
            if (a.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
            {
                var full = Path.GetFullPath(a);
                if (File.Exists(full)) candidateFiles.Add(full);
            }
        }
        catch { }
    }

    // 2. If none, scan current directory for *.json (shallow) with a reasonable cap
    if (candidateFiles.Count == 0)
    {
        foreach (var file in Directory.GetFiles(cwd, "*.json", SearchOption.TopDirectoryOnly))
        {
            candidateFiles.Add(file);
            if (candidateFiles.Count >= 10) break; // avoid excessive scanning
        }
    }

    // 3. Prioritize file literally named manifest.json if present
    candidateFiles = candidateFiles
        .Distinct(StringComparer.OrdinalIgnoreCase)
        .OrderBy(f => !string.Equals(Path.GetFileName(f), "manifest.json", StringComparison.OrdinalIgnoreCase))
        .ToList();

    foreach (var file in candidateFiles)
    {
        try
        {
            var json = File.ReadAllText(file);
            var manifest = System.Text.Json.JsonSerializer.Deserialize(json, Mcpb.Json.McpbJsonContext.Default.McpbManifest);
            if (manifest?.Server?.Type is string t && !string.IsNullOrWhiteSpace(t))
            {
                telemetryService.UpdateProjectType(t, force: true);
                break; // first valid manifest wins
            }
        }
        catch { /* continue trying other files */ }
    }
}
catch { /* suppress all telemetry detection errors */ }

try
{
    var invokeResult = await root.InvokeAsync(args);
    
    // Track overall command execution
    if (args.Length > 0)
    {
        var commandName = args[0];
        var success = invokeResult == 0 && Environment.ExitCode == 0;
        telemetryService.TrackCommand(commandName, stopwatch.Elapsed, success);
    }
    
    if (Environment.ExitCode != 0 && invokeResult == 0)
        return Environment.ExitCode;
    return invokeResult;
}
catch (Exception ex)
{
    // Track unhandled errors
    telemetryService.TrackError(ex, args.Length > 0 ? args[0] : "unknown");
    throw;
}
finally
{
    // Ensure telemetry is flushed before exit
    await telemetryService.FlushAsync();
}
