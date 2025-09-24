using System;
using System.IO;

namespace Mcpb.Services;

internal static class ProjectTypeDetector
{
    // Order matters: first positive match wins
    public static string Detect(string? directory = null)
    {
        try
        {
            var dir = string.IsNullOrWhiteSpace(directory) ? Directory.GetCurrentDirectory() : directory;
            if (!Directory.Exists(dir)) return "unknown";

            // Normalize root (no recursion for perf; heuristic only)
            // Node: package.json
            if (File.Exists(Path.Combine(dir, "package.json"))) return "node";

            // Python: pyproject.toml or requirements.txt
            if (File.Exists(Path.Combine(dir, "pyproject.toml")) || File.Exists(Path.Combine(dir, "requirements.txt"))) return "python";

            // Executable / binary style project: any *.csproj or direct exe in root
            var hasCsproj = Directory.GetFiles(dir, "*.csproj", SearchOption.TopDirectoryOnly).Length > 0;
            if (hasCsproj) return "exe"; // treat dotnet project as exe category per requirements

            // Look for built binaries (rare case) – light scan
            foreach (var file in Directory.GetFiles(dir, "*.exe", SearchOption.TopDirectoryOnly))
            {
                if (!string.IsNullOrEmpty(file)) return "exe";
            }

            return "unknown";
        }
        catch
        {
            return "unknown";
        }
    }
}
