using System.CommandLine;
using System.Security.Cryptography.X509Certificates;
using Mcpb.Core;

// ...existing code...

namespace Mcpb.Commands;

public static class InfoCommand
{
    public static Command Create()
    {
        var fileArg = new Argument<string>("mcpb-file", "Path to .mcpb file");
        var cmd = new Command("info", "Display information about an MCPB file") { fileArg };
        cmd.SetHandler((string file)=>
        {
            var path = Path.GetFullPath(file);
            if (!File.Exists(path)) { Console.Error.WriteLine($"ERROR: MCPB file not found: {file}"); return; }
            try
            {
                // Update telemetry project type based on bundle manifest, if available
                try
                {
                    var pt = ManifestProjectType.FromBundle(path);
                    if (!string.IsNullOrEmpty(pt)) Mcpb.Services.StaticTelemetryBridge.UpdateProjectType(pt);
                }
                catch { }
                var info = new FileInfo(path);
                var sizeKb = info.Length/1024.0;
                Console.WriteLine($"File: {info.Name}");
                Console.WriteLine($"Size: {sizeKb:F2} KB");
                Mcpb.Services.StaticTelemetryBridge.AddProperty("size_kb", sizeKb.ToString("F2"));
                var bytes = File.ReadAllBytes(path);
                var (original, sig) = SignatureHelpers.ExtractSignatureBlock(bytes);
                if (sig != null && SignatureHelpers.Verify(original, sig, out var cert) && cert != null)
                {
                    Console.WriteLine("\nSignature Information:");
                    Console.WriteLine($"  Subject: {cert.Subject}");
                    Console.WriteLine($"  Issuer: {cert.Issuer}");
                    Console.WriteLine($"  Valid from: {cert.NotBefore:MM/dd/yyyy} to {cert.NotAfter:MM/dd/yyyy}");
                    Console.WriteLine($"  Fingerprint: {cert.Thumbprint}");
                    Console.WriteLine($"  Status: Valid");
                    Mcpb.Services.StaticTelemetryBridge.AddProperty("signed", "true");
                }
                else
                {
                    Console.WriteLine("\nWARNING: Not signed");
                    Mcpb.Services.StaticTelemetryBridge.AddProperty("signed", "false");
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"ERROR: Failed to read MCPB info: {ex.Message}");
            }
        }, fileArg);
        return cmd;
    }
}
