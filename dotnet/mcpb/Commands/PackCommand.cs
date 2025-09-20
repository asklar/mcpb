using System.CommandLine;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using Mcpb.Core;
using System.Text.Json;
using Mcpb.Json;

namespace Mcpb.Commands;

public static class PackCommand
{
    private static readonly string[] BaseExcludePatterns = new []{
        ".DS_Store","Thumbs.db",".gitignore",".git",".mcpbignore","*.log",".env",".npm",".npmrc",".yarnrc",".yarn",".eslintrc",".editorconfig",".prettierrc",".prettierignore",".eslintignore",".nycrc",".babelrc",".pnp.*","node_modules/.cache","node_modules/.bin","*.map",".env.local",".env.*.local","npm-debug.log*","yarn-debug.log*","yarn-error.log*","package-lock.json","yarn.lock","*.mcpb","*.d.ts","*.tsbuildinfo","tsconfig.json"
    };

    public static Command Create()
    {
        var dirArg = new Argument<string?>("directory", () => Directory.GetCurrentDirectory(), "Extension directory");
        var outputArg = new Argument<string?>("output", () => null, "Output .mcpb path");
        var cmd = new Command("pack", "Pack a directory into an MCPB extension") { dirArg, outputArg };
        cmd.SetHandler(async (string? directory, string? output) =>
        {
            var dir = Path.GetFullPath(directory ?? Directory.GetCurrentDirectory());
            if (!Directory.Exists(dir)) { Console.Error.WriteLine($"ERROR: Directory not found: {dir}"); return; }
            var manifestPath = Path.Combine(dir, "manifest.json");
            if (!File.Exists(manifestPath)) { Console.Error.WriteLine("ERROR: manifest.json not found"); return; }
            if (!ValidateManifestBasic(manifestPath)) { Console.Error.WriteLine("ERROR: Cannot pack invalid manifest"); return; }

            var outPath = output != null ? Path.GetFullPath(output) : Path.Combine(Directory.GetCurrentDirectory(), new DirectoryInfo(dir).Name + ".mcpb");
            Directory.CreateDirectory(Path.GetDirectoryName(outPath)!);

            var ignorePatterns = LoadIgnoreFile(dir);
            var files = CollectFiles(dir, ignorePatterns, out var ignoredCount);

            // Parse manifest for name/version
            var manifest = JsonSerializer.Deserialize<McpbManifest>(File.ReadAllText(manifestPath), McpbJsonContext.Default.McpbManifest)!;

            // Header
            Console.WriteLine($"\n📦  {manifest.Name}@{manifest.Version}");
            Console.WriteLine("Archive Contents");

            long totalUnpacked = 0;
            // Build list with sizes
            var fileEntries = files.Select(t => new { t.fullPath, t.relative, Size = new FileInfo(t.fullPath).Length }).ToList();
            fileEntries.Sort((a,b) => string.Compare(a.relative, b.relative, StringComparison.Ordinal));

            // Group deep ( >3 parts ) similar to TS (first 3 segments)
            var deepGroups = new Dictionary<string, (List<string> Files,long Size)>();
            var shallow = new List<(string Rel,long Size)>();
            foreach (var fe in fileEntries)
            {
                totalUnpacked += fe.Size;
                var parts = fe.relative.Split('/', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length > 3)
                {
                    var key = string.Join('/', parts.Take(3));
                    if (!deepGroups.TryGetValue(key, out var val)) { val = (new List<string>(),0); }
                    val.Files.Add(fe.relative); val.Size += fe.Size; deepGroups[key] = val;
                }
                else shallow.Add((fe.relative, fe.Size));
            }
            foreach (var s in shallow) Console.WriteLine($"{FormatSize(s.Size).PadLeft(8)} {s.Rel}");
            foreach (var kv in deepGroups)
            {
                var (list,size) = kv.Value;
                if (list.Count == 1)
                    Console.WriteLine($"{FormatSize(size).PadLeft(8)} {list[0]}");
                else
                    Console.WriteLine($"{FormatSize(size).PadLeft(8)} {kv.Key}/ [and {list.Count} more files]");
            }

            using var mem = new MemoryStream();
            using (var zip = new ZipArchive(mem, ZipArchiveMode.Create, true, Encoding.UTF8))
            {
                foreach (var (filePath, rel) in files)
                {
                    var entry = zip.CreateEntry(rel, CompressionLevel.SmallestSize);
                    using var es = entry.Open();
                    await using var fs = File.OpenRead(filePath);
                    await fs.CopyToAsync(es);
                }
            }
            var zipData = mem.ToArray();
            await File.WriteAllBytesAsync(outPath, zipData);

            var sha1 = SHA1.HashData(zipData);
            var sanitizedName = SanitizeFileName(manifest.Name);
            var archiveName = $"{sanitizedName}-{manifest.Version}.mcpb";
            Console.WriteLine("\nArchive Details");
            Console.WriteLine($"name: {manifest.Name}");
            Console.WriteLine($"version: {manifest.Version}");
            Console.WriteLine($"filename: {archiveName}");
            Console.WriteLine($"package size: {FormatSize(zipData.Length)}");
            Console.WriteLine($"unpacked size: {FormatSize(totalUnpacked)}");
            Console.WriteLine($"shasum: {Convert.ToHexString(sha1).ToLowerInvariant()}");
            Console.WriteLine($"total files: {fileEntries.Count}");
            Console.WriteLine($"ignored (.mcpbignore) files: {ignoredCount}");
            Console.WriteLine($"\nOutput: {outPath}");
        }, dirArg, outputArg);
        return cmd;
    }

    private static bool ValidateManifestBasic(string manifestPath)
    {
        try { var json = File.ReadAllText(manifestPath); return JsonSerializer.Deserialize(json, McpbJsonContext.Default.McpbManifest) != null; }
        catch { return false; }
    }

    private static List<(string fullPath,string relative)> CollectFiles(string baseDir, List<string> additionalPatterns, out int ignoredCount)
    {
        ignoredCount = 0;
        var results = new List<(string,string)>();
        foreach (var file in Directory.GetFiles(baseDir, "*", SearchOption.AllDirectories))
        {
            var rel = Path.GetRelativePath(baseDir, file).Replace('\\','/');
            if (ShouldExclude(rel, additionalPatterns)) { ignoredCount++; continue; }
            results.Add((file, rel));
        }
        return results;
    }

    private static bool ShouldExclude(string relative, List<string> additional)
    {
        return Matches(relative, BaseExcludePatterns) || Matches(relative, additional);
    }

    private static bool Matches(string relative, IEnumerable<string> patterns)
    {
        foreach (var pattern in patterns)
        {
            if (GlobMatch(relative, pattern)) return true;
        }
        return false;
    }

    private static bool GlobMatch(string text, string pattern)
    {
        // Simple glob: * wildcard, ? single char, supports '**/' for any dir depth
        // Convert to regex
        var regex = System.Text.RegularExpressions.Regex.Escape(pattern)
            .Replace(@"\*\*\/", @"(?:(?:.+/)?)")
            .Replace(@"\*", @"[^/]*")
            .Replace(@"\?", @".");
        return System.Text.RegularExpressions.Regex.IsMatch(text, "^"+regex+"$");
    }

    private static List<string> LoadIgnoreFile(string baseDir)
    {
        var path = Path.Combine(baseDir, ".mcpbignore");
        if (!File.Exists(path)) return new List<string>();
        return File.ReadAllLines(path)
            .Select(l => l.Trim())
            .Where(l => l.Length>0 && !l.StartsWith("#"))
            .ToList();
    }

    private static string FormatSize(long bytes)
    {
        if (bytes < 1024) return $"{bytes}B"; if (bytes < 1024*1024) return $"{bytes/1024.0:F1}kB"; return $"{bytes/(1024.0*1024):F1}MB";
    }

    private static string SanitizeFileName(string name)
    {
        var lower = name.ToLowerInvariant();
        lower = RegexReplace(lower, "\\s+", "-");
        lower = RegexReplace(lower, "[^a-z0-9-_.]", "");
        lower = RegexReplace(lower, "-+", "-");
        lower = lower.Trim('-');
        if (lower.Length > 100) lower = lower.Substring(0,100);
        return lower;
    }
    private static string RegexReplace(string input,string pattern,string replacement) => System.Text.RegularExpressions.Regex.Replace(input, pattern, replacement);
}
