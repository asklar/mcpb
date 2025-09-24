using System.IO.Compression;
using System.Text.Json;
using Mcpb.Core;
using Mcpb.Json;

namespace Mcpb.Core;

internal static class ManifestProjectType
{
    public static string? FromManifestFile(string path)
    {
        try
        {
            var json = File.ReadAllText(path);
            var manifest = JsonSerializer.Deserialize(json, McpbJsonContext.Default.McpbManifest);
            return Normalize(manifest?.Server?.Type);
        }
        catch { return null; }
    }

    public static string? FromBundle(string mcpbPath)
    {
        try
        {
            using var fs = File.OpenRead(mcpbPath);
            // Remove signature block if present
            byte[] raw = File.ReadAllBytes(mcpbPath);
            var (original, sig) = Mcpb.Commands.SignatureHelpers.ExtractSignatureBlock(raw);
            using var ms = new MemoryStream(original);
            using var zip = new ZipArchive(ms, ZipArchiveMode.Read, true);
            // Find first *.json containing "server" and "type"
            foreach (var entry in zip.Entries.Where(e => e.FullName.EndsWith(".json", StringComparison.OrdinalIgnoreCase)))
            {
                using var es = entry.Open();
                using var sr = new StreamReader(es);
                var text = sr.ReadToEnd();
                try
                {
                    var manifest = JsonSerializer.Deserialize(text, McpbJsonContext.Default.McpbManifest);
                    var type = Normalize(manifest?.Server?.Type);
                    if (!string.IsNullOrEmpty(type)) return type;
                }
                catch { /* continue */ }
            }
        }
        catch { }
        return null;
    }

    private static string? Normalize(string? t)
    {
        if (string.IsNullOrWhiteSpace(t)) return null;
        t = t.Trim().ToLowerInvariant();
        return t switch { "binary" => "exe", _ => t };
    }
}
