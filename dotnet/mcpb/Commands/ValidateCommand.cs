using System.CommandLine;
using Mcpb.Core;
using System.Text.Json;
using Mcpb.Json;
using Mcpb.Services;

namespace Mcpb.Commands;

public static class ValidateCommand
{
    public static Command Create()
    {
        var manifestArg = new Argument<string>("manifest", description: "Path to manifest.json or its directory");
        var cmd = new Command("validate", "Validate an MCPB manifest file") { manifestArg };
        cmd.SetHandler((string path) =>
        {
            if (Directory.Exists(path))
            {
                path = Path.Combine(path, "manifest.json");
            }
            if (!File.Exists(path))
            {
                Console.Error.WriteLine($"ERROR: File not found: {path}");
                Environment.ExitCode = 1;
                return;
            }
            try
            {
                var json = File.ReadAllText(path);
                if (Environment.GetEnvironmentVariable("MCPB_DEBUG_VALIDATE") == "1")
                {
                    Console.WriteLine($"DEBUG: Read manifest {path} length={json.Length}");
                }
                var issues = ManifestValidator.ValidateJson(json);
                // Telemetry update from this manifest
                try
                {
                    var pt = ManifestProjectType.FromManifestFile(path);
                    if (!string.IsNullOrEmpty(pt)) Mcpb.Services.StaticTelemetryBridge.UpdateProjectType(pt);
                }
                catch { }
                var errors = issues.Where(i => !(i.Path == "dxt_version" && i.Message.Contains("deprecated"))).ToList();
                var deprecations = issues.Where(i => i.Path == "dxt_version" && i.Message.Contains("deprecated")).ToList();
                if (errors.Count == 0)
                {
                    Console.WriteLine("Manifest is valid!");
                    foreach (var d in deprecations)
                        Console.WriteLine($"Warning: {d.Message}");
                    try { TelemetryEnricher.Validate(0, deprecations.Count, deprecations.Count); } catch { }
                    Console.Out.Flush();
                    return; // success
                }
                Console.Error.WriteLine("ERROR: Manifest validation failed:\n");
                foreach (var issue in errors)
                {
                    var pfx = string.IsNullOrEmpty(issue.Path) ? "" : issue.Path + ": ";
                    Console.Error.WriteLine($"  - {pfx}{issue.Message}");
                }
                foreach (var d in deprecations)
                    Console.Error.WriteLine($"  - {d.Path}: {d.Message}");
                try { TelemetryEnricher.Validate(errors.Count, 0, deprecations.Count); } catch { }
                Environment.ExitCode = 1;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"ERROR: {ex.Message}");
                Environment.ExitCode = 1;
            }
        }, manifestArg);
        return cmd;
    }
}