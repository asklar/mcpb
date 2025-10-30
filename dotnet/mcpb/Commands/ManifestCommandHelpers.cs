using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Mcpb.Core;
using Mcpb.Json;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;

namespace Mcpb.Commands;

internal static class ManifestCommandHelpers
{
    internal record CapabilityDiscoveryResult(
        List<McpbManifestTool> Tools,
        List<McpbManifestPrompt> Prompts,
        McpbInitializeResult? InitializeResponse,
        McpbToolsListResult? ToolsListResponse);

    /// <summary>
    /// Recursively filters out null properties from a JsonElement to match JsonIgnoreCondition.WhenWritingNull behavior
    /// </summary>
    private static object FilterNullProperties(JsonElement element)
    {
        if (element.ValueKind == JsonValueKind.Object)
        {
            var dict = new Dictionary<string, object>();
            foreach (var property in element.EnumerateObject())
            {
                if (property.Value.ValueKind != JsonValueKind.Null)
                {
                    dict[property.Name] = FilterNullProperties(property.Value);
                }
            }
            return dict;
        }
        else if (element.ValueKind == JsonValueKind.Array)
        {
            var list = new List<object>();
            foreach (var item in element.EnumerateArray())
            {
                list.Add(FilterNullProperties(item));
            }
            return list;
        }
        else if (element.ValueKind == JsonValueKind.String)
        {
            return element.GetString() ?? "";
        }
        else if (element.ValueKind == JsonValueKind.Number)
        {
            if (element.TryGetInt64(out var longValue))
                return longValue;
            return element.GetDouble();
        }
        else if (element.ValueKind == JsonValueKind.True)
        {
            return true;
        }
        else if (element.ValueKind == JsonValueKind.False)
        {
            return false;
        }
        else
        {
            // For other types, convert to JsonElement
            var json = JsonSerializer.Serialize(element);
            return JsonSerializer.Deserialize<JsonElement>(json);
        }
    }

    internal static List<string> ValidateReferencedFiles(McpbManifest manifest, string baseDir)
    {
        var errors = new List<string>();
        if (manifest.Server == null)
        {
            errors.Add("Manifest server configuration missing");
            return errors;
        }

        string Resolve(string rel)
        {
            var normalized = rel.Replace('\\', '/');
            if (Path.IsPathRooted(normalized))
            {
                return normalized.Replace('/', Path.DirectorySeparatorChar);
            }
            return Path.Combine(baseDir, normalized.Replace('/', Path.DirectorySeparatorChar));
        }

        void CheckFile(string? relativePath, string category)
        {
            if (string.IsNullOrWhiteSpace(relativePath)) return;
            var resolved = Resolve(relativePath);
            if (!File.Exists(resolved))
            {
                errors.Add($"Missing {category} file: {relativePath}");
            }
        }

        if (!string.IsNullOrWhiteSpace(manifest.Icon))
        {
            CheckFile(manifest.Icon, "icon");
        }

        if (!string.IsNullOrWhiteSpace(manifest.Server.EntryPoint))
        {
            CheckFile(manifest.Server.EntryPoint, "entry_point");
        }

        var command = manifest.Server.McpConfig?.Command;
        if (!string.IsNullOrWhiteSpace(command))
        {
            var cmd = command!;
            bool pathLike = cmd.Contains('/') || cmd.Contains('\\') ||
                cmd.StartsWith("${__dirname}", StringComparison.OrdinalIgnoreCase) ||
                cmd.StartsWith("./") || cmd.StartsWith("..") ||
                cmd.EndsWith(".js", StringComparison.OrdinalIgnoreCase) ||
                cmd.EndsWith(".py", StringComparison.OrdinalIgnoreCase) ||
                cmd.EndsWith(".exe", StringComparison.OrdinalIgnoreCase);
            if (pathLike)
            {
                var expanded = ExpandToken(cmd, baseDir);
                var normalized = NormalizePathForPlatform(expanded);
                var resolved = normalized;
                if (!Path.IsPathRooted(normalized))
                {
                    resolved = Path.Combine(baseDir, normalized);
                }
                if (!File.Exists(resolved))
                {
                    errors.Add($"Missing server.command file: {command}");
                }
            }
        }

        if (manifest.Screenshots != null)
        {
            foreach (var shot in manifest.Screenshots)
            {
                if (string.IsNullOrWhiteSpace(shot)) continue;
                CheckFile(shot, "screenshot");
            }
        }

        if (manifest.Icons != null)
        {
            for (int i = 0; i < manifest.Icons.Count; i++)
            {
                var icon = manifest.Icons[i];
                if (!string.IsNullOrWhiteSpace(icon.Src))
                {
                    CheckFile(icon.Src, $"icons[{i}]");
                }
            }
        }

        if (manifest.Localization != null)
        {
            // Check if the localization resources path exists
            // Resources defaults to "mcpb-resources/${locale}.json" if not specified
            var resourcePath = manifest.Localization.Resources ?? "mcpb-resources/${locale}.json";
            // DefaultLocale defaults to "en-US" if not specified
            var defaultLocale = manifest.Localization.DefaultLocale ?? "en-US";
            
            var defaultLocalePath = resourcePath.Replace("${locale}", defaultLocale, StringComparison.OrdinalIgnoreCase);
            var resolved = Resolve(defaultLocalePath);
            
            // Check if it's a file or directory
            if (!File.Exists(resolved) && !Directory.Exists(resolved))
            {
                errors.Add($"Missing localization resources for default locale: {defaultLocalePath}");
            }
        }

        return errors;
    }

    internal static List<string> ValidateLocalizationCompleteness(McpbManifest manifest, string baseDir, HashSet<string>? rootProps = null)
    {
        var errors = new List<string>();
        
        if (manifest.Localization == null)
            return errors;

        // Get the resource path pattern and default locale
        var resourcePath = manifest.Localization.Resources ?? "mcpb-resources/${locale}.json";
        var defaultLocale = manifest.Localization.DefaultLocale ?? "en-US";

        // Determine localizable properties present in the manifest
        // Only check properties that were explicitly set in the JSON
        var localizableProperties = new List<string>();
        if (rootProps != null)
        {
            if (rootProps.Contains("display_name") && !string.IsNullOrWhiteSpace(manifest.DisplayName)) 
                localizableProperties.Add("display_name");
            if (rootProps.Contains("description") && !string.IsNullOrWhiteSpace(manifest.Description)) 
                localizableProperties.Add("description");
            if (rootProps.Contains("long_description") && !string.IsNullOrWhiteSpace(manifest.LongDescription)) 
                localizableProperties.Add("long_description");
            if (rootProps.Contains("author") && !string.IsNullOrWhiteSpace(manifest.Author?.Name)) 
                localizableProperties.Add("author.name");
            if (rootProps.Contains("license") && !string.IsNullOrWhiteSpace(manifest.License)) 
                localizableProperties.Add("license");
            if (rootProps.Contains("keywords") && manifest.Keywords != null && manifest.Keywords.Count > 0) 
                localizableProperties.Add("keywords");
        }
        else
        {
            // Fallback if rootProps not provided
            if (!string.IsNullOrWhiteSpace(manifest.DisplayName)) localizableProperties.Add("display_name");
            if (!string.IsNullOrWhiteSpace(manifest.Description)) localizableProperties.Add("description");
            if (!string.IsNullOrWhiteSpace(manifest.LongDescription)) localizableProperties.Add("long_description");
            if (!string.IsNullOrWhiteSpace(manifest.Author?.Name)) localizableProperties.Add("author.name");
            if (!string.IsNullOrWhiteSpace(manifest.License) && manifest.License != "MIT") localizableProperties.Add("license"); // Don't check default
            if (manifest.Keywords != null && manifest.Keywords.Count > 0) localizableProperties.Add("keywords");
        }

        // Also check tool and prompt descriptions
        var toolsWithDescriptions = manifest.Tools?.Where(t => !string.IsNullOrWhiteSpace(t.Description)).ToList() ?? new List<McpbManifestTool>();
        var promptsWithDescriptions = manifest.Prompts?.Where(p => !string.IsNullOrWhiteSpace(p.Description)).ToList() ?? new List<McpbManifestPrompt>();

        if (localizableProperties.Count == 0 && toolsWithDescriptions.Count == 0 && promptsWithDescriptions.Count == 0)
            return errors; // Nothing to localize

        // Find all locale files by scanning the directory pattern
        var localeFiles = FindLocaleFiles(resourcePath, baseDir, defaultLocale);
        
        if (localeFiles.Count == 0)
            return errors; // No additional locale files found, nothing to validate

        // Check each locale file for completeness
        foreach (var (locale, filePath) in localeFiles)
        {
            if (locale == defaultLocale)
                continue; // Skip default locale (values are in main manifest)

            try
            {
                if (!File.Exists(filePath))
                {
                    errors.Add($"Locale file not found: {filePath} (for locale {locale})");
                    continue;
                }

                var localeJson = File.ReadAllText(filePath);
                using var localeDoc = JsonDocument.Parse(localeJson);
                var root = localeDoc.RootElement;

                // Check for localizable properties
                foreach (var prop in localizableProperties)
                {
                    if (prop == "author.name")
                    {
                        if (!root.TryGetProperty("author", out var authorElem) || 
                            !authorElem.TryGetProperty("name", out _))
                        {
                            errors.Add($"Missing localization for '{prop}' in {locale} ({filePath})");
                        }
                    }
                    else if (!root.TryGetProperty(prop, out _))
                    {
                        errors.Add($"Missing localization for '{prop}' in {locale} ({filePath})");
                    }
                }

                // Check tool descriptions
                if (toolsWithDescriptions.Count > 0 && root.TryGetProperty("tools", out var toolsElem) && toolsElem.ValueKind == JsonValueKind.Array)
                {
                    var localizedTools = toolsElem.EnumerateArray().ToList();
                    foreach (var tool in toolsWithDescriptions)
                    {
                        var found = localizedTools.Any(t => 
                            t.TryGetProperty("name", out var nameElem) && 
                            nameElem.GetString() == tool.Name &&
                            t.TryGetProperty("description", out _));
                        
                        if (!found)
                        {
                            errors.Add($"Missing localized description for tool '{tool.Name}' in {locale} ({filePath})");
                        }
                    }
                }

                // Check prompt descriptions
                if (promptsWithDescriptions.Count > 0 && root.TryGetProperty("prompts", out var promptsElem) && promptsElem.ValueKind == JsonValueKind.Array)
                {
                    var localizedPrompts = promptsElem.EnumerateArray().ToList();
                    foreach (var prompt in promptsWithDescriptions)
                    {
                        var found = localizedPrompts.Any(p => 
                            p.TryGetProperty("name", out var nameElem) && 
                            nameElem.GetString() == prompt.Name &&
                            p.TryGetProperty("description", out _));
                        
                        if (!found)
                        {
                            errors.Add($"Missing localized description for prompt '{prompt.Name}' in {locale} ({filePath})");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                errors.Add($"Error reading locale file {filePath}: {ex.Message}");
            }
        }

        return errors;
    }

    private static List<(string locale, string filePath)> FindLocaleFiles(string resourcePattern, string baseDir, string defaultLocale)
    {
        var localeFiles = new List<(string, string)>();
        
        // Extract the directory and file pattern
        var patternIndex = resourcePattern.IndexOf("${locale}", StringComparison.OrdinalIgnoreCase);
        if (patternIndex < 0)
            return localeFiles;

        var beforePlaceholder = resourcePattern.Substring(0, patternIndex);
        var afterPlaceholder = resourcePattern.Substring(patternIndex + "${locale}".Length);
        
        var lastSlash = beforePlaceholder.LastIndexOfAny(new[] { '/', '\\' });
        string dirPath, filePrefix;
        
        if (lastSlash >= 0)
        {
            dirPath = beforePlaceholder.Substring(0, lastSlash);
            filePrefix = beforePlaceholder.Substring(lastSlash + 1);
        }
        else
        {
            dirPath = "";
            filePrefix = beforePlaceholder;
        }

        var fullDirPath = string.IsNullOrEmpty(dirPath) ? baseDir : Path.Combine(baseDir, dirPath.Replace('/', Path.DirectorySeparatorChar));
        
        if (!Directory.Exists(fullDirPath))
            return localeFiles;

        // Find all files matching the pattern
        var searchPattern = filePrefix + "*" + afterPlaceholder;
        var files = Directory.GetFiles(fullDirPath, searchPattern, SearchOption.TopDirectoryOnly);
        
        foreach (var file in files)
        {
            var fileName = Path.GetFileName(file);
            
            // Extract locale from filename
            if (fileName.StartsWith(filePrefix) && fileName.EndsWith(afterPlaceholder))
            {
                var localeStart = filePrefix.Length;
                var localeEnd = fileName.Length - afterPlaceholder.Length;
                if (localeEnd > localeStart)
                {
                    var locale = fileName.Substring(localeStart, localeEnd - localeStart);
                    localeFiles.Add((locale, file));
                }
            }
        }

        return localeFiles;
    }

    internal static async Task<CapabilityDiscoveryResult> DiscoverCapabilitiesAsync(
        string dir,
        McpbManifest manifest,
        Action<string>? logInfo,
        Action<string>? logWarning)
    {
        var overrideTools = TryParseToolOverride("MCPB_TOOL_DISCOVERY_JSON");
        var overridePrompts = TryParsePromptOverride("MCPB_PROMPT_DISCOVERY_JSON");
        if (overrideTools != null || overridePrompts != null)
        {
            return new CapabilityDiscoveryResult(
                overrideTools ?? new List<McpbManifestTool>(),
                overridePrompts ?? new List<McpbManifestPrompt>(),
                null,
                null);
        }

        var cfg = manifest.Server?.McpConfig ?? throw new InvalidOperationException("Manifest server.mcp_config missing");
        var command = cfg.Command;
        if (string.IsNullOrWhiteSpace(command)) throw new InvalidOperationException("Manifest server.mcp_config.command empty");
        var rawArgs = cfg.Args ?? new List<string>();
        command = ExpandToken(command, dir);
        var args = rawArgs.Select(a => ExpandToken(a, dir)).Where(a => !string.IsNullOrWhiteSpace(a)).ToList();
        command = NormalizePathForPlatform(command);
        for (int i = 0; i < args.Count; i++) args[i] = NormalizePathForPlatform(args[i]);

        Dictionary<string, string>? env = null;
        if (cfg.Env != null && cfg.Env.Count > 0)
        {
            env = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var kv in cfg.Env)
            {
                var expanded = ExpandToken(kv.Value, dir);
                env[kv.Key] = NormalizePathForPlatform(expanded);
            }
        }

        var toolInfos = new List<McpbManifestTool>();
        var promptInfos = new List<McpbManifestPrompt>();
        McpbInitializeResult? initializeResponse = null;
        McpbToolsListResult? toolsListResponse = null;
        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(8));
            IDictionary<string, string?>? envVars = null;
            if (env != null)
            {
                envVars = new Dictionary<string, string?>(env.ToDictionary(kv => kv.Key, kv => (string?)kv.Value), StringComparer.OrdinalIgnoreCase);
            }
            var transport = new StdioClientTransport(new StdioClientTransportOptions
            {
                Name = "mcpb-discovery",
                Command = command,
                Arguments = args.ToArray(),
                WorkingDirectory = dir,
                EnvironmentVariables = envVars
            });
            logInfo?.Invoke($"Discovering tools & prompts using: {command} {string.Join(' ', args)}");
            await using var client = await McpClient.CreateAsync(transport);

            // Capture initialize response using McpClient properties
            // Filter out null properties to match JsonIgnoreCondition.WhenWritingNull behavior
            try
            {
                // Serialize and filter capabilities
                object? capabilities = null;
                if (client.ServerCapabilities != null)
                {
                    var capJson = JsonSerializer.Serialize(client.ServerCapabilities);
                    var capElement = JsonSerializer.Deserialize<JsonElement>(capJson);
                    capabilities = FilterNullProperties(capElement);
                }

                // Serialize and filter serverInfo
                object? serverInfo = null;
                if (client.ServerInfo != null)
                {
                    var infoJson = JsonSerializer.Serialize(client.ServerInfo);
                    var infoElement = JsonSerializer.Deserialize<JsonElement>(infoJson);
                    serverInfo = FilterNullProperties(infoElement);
                }

                var instructions = string.IsNullOrWhiteSpace(client.ServerInstructions) ? null : client.ServerInstructions;

                initializeResponse = new McpbInitializeResult
                {
                    ProtocolVersion = client.NegotiatedProtocolVersion,
                    Capabilities = capabilities,
                    ServerInfo = serverInfo,
                    Instructions = instructions
                };
            }
            catch (Exception ex)
            {
                logWarning?.Invoke($"Failed to capture initialize response: {ex.Message}");
            }

            var tools = await client.ListToolsAsync(null, cts.Token);

            // Capture tools/list response using typed Tool objects
            // Filter out null properties to match JsonIgnoreCondition.WhenWritingNull behavior
            try
            {
                var toolsList = new List<object>();
                foreach (var tool in tools)
                {
                    // Serialize the tool and parse to JsonElement
                    var json = JsonSerializer.Serialize(tool.ProtocolTool);
                    var element = JsonSerializer.Deserialize<JsonElement>(json);

                    // Filter out null properties recursively
                    var filtered = FilterNullProperties(element);
                    toolsList.Add(filtered);
                }
                toolsListResponse = new McpbToolsListResult { Tools = toolsList };
            }
            catch (Exception ex)
            {
                logWarning?.Invoke($"Failed to capture tools/list response: {ex.Message}");
            }

            foreach (var tool in tools)
            {
                if (string.IsNullOrWhiteSpace(tool.Name)) continue;
                var manifestTool = new McpbManifestTool
                {
                    Name = tool.Name,
                    Description = string.IsNullOrWhiteSpace(tool.Description) ? null : tool.Description
                };
                toolInfos.Add(manifestTool);
            }
            try
            {
                var prompts = await client.ListPromptsAsync(cts.Token);
                foreach (var prompt in prompts)
                {
                    if (string.IsNullOrWhiteSpace(prompt.Name)) continue;
                    var manifestPrompt = new McpbManifestPrompt
                    {
                        Name = prompt.Name,
                        Description = string.IsNullOrWhiteSpace(prompt.Description) ? null : prompt.Description,
                        Arguments = prompt.ProtocolPrompt?.Arguments?
                            .Select(a => a.Name)
                            .Where(n => !string.IsNullOrWhiteSpace(n))
                            .Distinct(StringComparer.Ordinal)
                            .ToList()
                    };
                    if (manifestPrompt.Arguments != null && manifestPrompt.Arguments.Count == 0)
                    {
                        manifestPrompt.Arguments = null;
                    }
                    try
                    {
                        var promptResult = await client.GetPromptAsync(prompt.Name, cancellationToken: cts.Token);
                        manifestPrompt.Text = ExtractPromptText(promptResult);
                    }
                    catch (Exception ex)
                    {
                        logWarning?.Invoke($"Prompt '{prompt.Name}' content fetch failed: {ex.Message}");
                        manifestPrompt.Text = string.Empty;
                    }
                    promptInfos.Add(manifestPrompt);
                }
            }
            catch (Exception ex)
            {
                logWarning?.Invoke($"Prompt discovery skipped: {ex.Message}");
            }
        }
        catch (Exception ex)
        {
            logWarning?.Invoke($"MCP client discovery failed: {ex.Message}");
        }

        return new CapabilityDiscoveryResult(
            DeduplicateTools(toolInfos),
            DeduplicatePrompts(promptInfos),
            initializeResponse,
            toolsListResponse);
    }

    internal static string NormalizePathForPlatform(string value)
    {
        if (string.IsNullOrEmpty(value)) return value;
        if (value.Contains("://")) return value;
        if (value.StartsWith("-")) return value;
        var sep = Path.DirectorySeparatorChar;
        return value.Replace('\\', sep).Replace('/', sep);
    }

    internal static string ExpandToken(string value, string dir)
    {
        if (string.IsNullOrEmpty(value)) return value;
        var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        string desktop = SafeGetSpecial(Environment.SpecialFolder.Desktop, Path.Combine(home, "Desktop"));
        string documents = SafeGetSpecial(Environment.SpecialFolder.MyDocuments, Path.Combine(home, "Documents"));
        string downloads = Path.Combine(home, "Downloads");
        string sep = Path.DirectorySeparatorChar.ToString();
        return Regex.Replace(value, "\\$\\{([^}]+)\\}", m =>
        {
            var token = m.Groups[1].Value;
            if (string.Equals(token, "__dirname", StringComparison.OrdinalIgnoreCase)) return dir.Replace('\\', '/');
            if (string.Equals(token, "HOME", StringComparison.OrdinalIgnoreCase)) return home;
            if (string.Equals(token, "DESKTOP", StringComparison.OrdinalIgnoreCase)) return desktop;
            if (string.Equals(token, "DOCUMENTS", StringComparison.OrdinalIgnoreCase)) return documents;
            if (string.Equals(token, "DOWNLOADS", StringComparison.OrdinalIgnoreCase)) return downloads;
            if (string.Equals(token, "pathSeparator", StringComparison.OrdinalIgnoreCase) || token == "/") return sep;
            if (token.StartsWith("user_config.", StringComparison.OrdinalIgnoreCase)) return string.Empty;
            return m.Value;
        });
    }

    private static string SafeGetSpecial(Environment.SpecialFolder folder, string fallback)
    {
        try { var p = Environment.GetFolderPath(folder); return string.IsNullOrEmpty(p) ? fallback : p; }
        catch { return fallback; }
    }

    private static List<McpbManifestTool>? TryParseToolOverride(string envVar)
    {
        var json = Environment.GetEnvironmentVariable(envVar);
        if (string.IsNullOrWhiteSpace(json)) return null;
        try
        {
            using var doc = JsonDocument.Parse(json);
            if (doc.RootElement.ValueKind != JsonValueKind.Array) return null;
            var list = new List<McpbManifestTool>();
            foreach (var el in doc.RootElement.EnumerateArray())
            {
                if (el.ValueKind == JsonValueKind.String)
                {
                    var name = el.GetString();
                    if (!string.IsNullOrWhiteSpace(name))
                    {
                        list.Add(new McpbManifestTool { Name = name! });
                    }
                    continue;
                }

                if (el.ValueKind != JsonValueKind.Object || !el.TryGetProperty("name", out var nameProp) || nameProp.ValueKind != JsonValueKind.String)
                {
                    continue;
                }

                var tool = new McpbManifestTool
                {
                    Name = nameProp.GetString() ?? string.Empty
                };

                if (el.TryGetProperty("description", out var descProp) && descProp.ValueKind == JsonValueKind.String)
                {
                    var desc = descProp.GetString();
                    tool.Description = string.IsNullOrWhiteSpace(desc) ? null : desc;
                }

                list.Add(tool);
            }

            return list.Count == 0 ? null : DeduplicateTools(list);
        }
        catch
        {
            return null;
        }
    }

    private static List<McpbManifestPrompt>? TryParsePromptOverride(string envVar)
    {
        var json = Environment.GetEnvironmentVariable(envVar);
        if (string.IsNullOrWhiteSpace(json)) return null;
        try
        {
            using var doc = JsonDocument.Parse(json);
            if (doc.RootElement.ValueKind != JsonValueKind.Array) return null;
            var list = new List<McpbManifestPrompt>();
            foreach (var el in doc.RootElement.EnumerateArray())
            {
                if (el.ValueKind == JsonValueKind.String)
                {
                    var name = el.GetString();
                    if (!string.IsNullOrWhiteSpace(name)) list.Add(new McpbManifestPrompt { Name = name!, Text = string.Empty });
                    continue;
                }

                if (el.ValueKind != JsonValueKind.Object || !el.TryGetProperty("name", out var nameProp) || nameProp.ValueKind != JsonValueKind.String)
                {
                    continue;
                }

                var prompt = new McpbManifestPrompt
                {
                    Name = nameProp.GetString() ?? string.Empty,
                    Text = string.Empty
                };

                if (el.TryGetProperty("description", out var descProp) && descProp.ValueKind == JsonValueKind.String)
                {
                    var desc = descProp.GetString();
                    prompt.Description = string.IsNullOrWhiteSpace(desc) ? null : desc;
                }

                if (el.TryGetProperty("arguments", out var argsProp) && argsProp.ValueKind == JsonValueKind.Array)
                {
                    var args = new List<string>();
                    foreach (var arg in argsProp.EnumerateArray())
                    {
                        if (arg.ValueKind == JsonValueKind.String)
                        {
                            var argName = arg.GetString();
                            if (!string.IsNullOrWhiteSpace(argName)) args.Add(argName!);
                        }
                    }
                    prompt.Arguments = args.Count > 0 ? args : null;
                }

                if (el.TryGetProperty("text", out var textProp) && textProp.ValueKind == JsonValueKind.String)
                {
                    prompt.Text = textProp.GetString() ?? string.Empty;
                }

                list.Add(prompt);
            }

            return list.Count == 0 ? null : DeduplicatePrompts(list);
        }
        catch
        {
            return null;
        }
    }

    private static List<McpbManifestTool> DeduplicateTools(IEnumerable<McpbManifestTool> tools)
    {
        return tools
            .Where(t => !string.IsNullOrWhiteSpace(t.Name))
            .GroupBy(t => t.Name, StringComparer.Ordinal)
            .Select(g => MergeToolGroup(g))
            .OrderBy(t => t.Name, StringComparer.Ordinal)
            .ToList();
    }

    private static McpbManifestTool MergeToolGroup(IEnumerable<McpbManifestTool> group)
    {
        var first = group.First();
        if (!string.IsNullOrWhiteSpace(first.Description)) return first;
        var description = group.Select(t => t.Description).FirstOrDefault(d => !string.IsNullOrWhiteSpace(d));
        return new McpbManifestTool
        {
            Name = first.Name,
            Description = string.IsNullOrWhiteSpace(description) ? null : description
        };
    }

    private static List<McpbManifestPrompt> DeduplicatePrompts(IEnumerable<McpbManifestPrompt> prompts)
    {
        return prompts
            .Where(p => !string.IsNullOrWhiteSpace(p.Name))
            .GroupBy(p => p.Name, StringComparer.Ordinal)
            .Select(g => MergePromptGroup(g))
            .OrderBy(p => p.Name, StringComparer.Ordinal)
            .ToList();
    }

    private static McpbManifestPrompt MergePromptGroup(IEnumerable<McpbManifestPrompt> group)
    {
        var first = group.First();
        var description = !string.IsNullOrWhiteSpace(first.Description)
            ? first.Description
            : group.Select(p => p.Description).FirstOrDefault(d => !string.IsNullOrWhiteSpace(d));
        var aggregatedArgs = first.Arguments != null && first.Arguments.Count > 0
            ? new List<string>(first.Arguments)
            : group.SelectMany(p => p.Arguments ?? new List<string>()).Distinct(StringComparer.Ordinal).ToList();

        var text = !string.IsNullOrWhiteSpace(first.Text)
            ? first.Text
            : group.Select(p => p.Text).FirstOrDefault(t => !string.IsNullOrWhiteSpace(t)) ?? string.Empty;

        return new McpbManifestPrompt
        {
            Name = first.Name,
            Description = string.IsNullOrWhiteSpace(description) ? null : description,
            Arguments = aggregatedArgs.Count > 0 ? aggregatedArgs : null,
            Text = text
        };
    }

    private static string ExtractPromptText(GetPromptResult? promptResult)
    {
        if (promptResult?.Messages == null) return string.Empty;
        var builder = new StringBuilder();
        foreach (var message in promptResult.Messages)
        {
            if (message?.Content == null) continue;
            AppendContentBlocks(builder, message.Content);
        }
        return builder.ToString();
    }

    private static void AppendContentBlocks(StringBuilder builder, object content)
    {
        switch (content)
        {
            case null:
                return;
            case TextContentBlock textBlock:
                AppendText(builder, textBlock);
                return;
            case IEnumerable<ContentBlock> enumerableBlocks:
                foreach (var block in enumerableBlocks)
                {
                    AppendText(builder, block as TextContentBlock);
                }
                return;
            case ContentBlock singleBlock:
                AppendText(builder, singleBlock as TextContentBlock);
                return;
        }
    }

    private static void AppendText(StringBuilder builder, TextContentBlock? textBlock)
    {
        if (textBlock == null || string.IsNullOrWhiteSpace(textBlock.Text)) return;
        if (builder.Length > 0) builder.AppendLine();
        builder.Append(textBlock.Text);
    }

    internal static List<string> GetToolMetadataDifferences(IEnumerable<McpbManifestTool>? manifestTools, IEnumerable<McpbManifestTool> discoveredTools)
    {
        var differences = new List<string>();
        if (manifestTools == null) return differences;
        var manifestByName = manifestTools
            .Where(t => !string.IsNullOrWhiteSpace(t.Name))
            .ToDictionary(t => t.Name, StringComparer.Ordinal);

        foreach (var tool in discoveredTools)
        {
            if (string.IsNullOrWhiteSpace(tool.Name)) continue;
            if (!manifestByName.TryGetValue(tool.Name, out var existing)) continue;

            if (!StringEqualsNormalized(existing.Description, tool.Description))
            {
                differences.Add($"Tool '{tool.Name}' description differs (manifest: {FormatValue(existing.Description)}, discovered: {FormatValue(tool.Description)}).");
            }
        }

        return differences;
    }

    internal static List<string> GetPromptMetadataDifferences(IEnumerable<McpbManifestPrompt>? manifestPrompts, IEnumerable<McpbManifestPrompt> discoveredPrompts)
    {
        var differences = new List<string>();
        if (manifestPrompts == null) return differences;
        var manifestByName = manifestPrompts
            .Where(p => !string.IsNullOrWhiteSpace(p.Name))
            .ToDictionary(p => p.Name, StringComparer.Ordinal);

        foreach (var prompt in discoveredPrompts)
        {
            if (string.IsNullOrWhiteSpace(prompt.Name)) continue;
            if (!manifestByName.TryGetValue(prompt.Name, out var existing)) continue;

            if (!StringEqualsNormalized(existing.Description, prompt.Description))
            {
                differences.Add($"Prompt '{prompt.Name}' description differs (manifest: {FormatValue(existing.Description)}, discovered: {FormatValue(prompt.Description)}).");
            }

            var manifestArgs = NormalizeArguments(existing.Arguments);
            var discoveredArgs = NormalizeArguments(prompt.Arguments);
            if (!manifestArgs.SequenceEqual(discoveredArgs, StringComparer.Ordinal))
            {
                differences.Add($"Prompt '{prompt.Name}' arguments differ (manifest: {FormatArguments(manifestArgs)}, discovered: {FormatArguments(discoveredArgs)}).");
            }

            var manifestText = NormalizeString(existing.Text);
            var discoveredText = NormalizeString(prompt.Text);
            if (manifestText == null && discoveredText != null)
            {
                differences.Add($"Prompt '{prompt.Name}' text differs (manifest length {existing.Text?.Length ?? 0}, discovered length {prompt.Text?.Length ?? 0}).");
            }
        }

        return differences;
    }

    internal static List<string> GetPromptTextWarnings(IEnumerable<McpbManifestPrompt>? manifestPrompts, IEnumerable<McpbManifestPrompt> discoveredPrompts)
    {
        var warnings = new List<string>();
        var manifestByName = manifestPrompts?
            .Where(p => !string.IsNullOrWhiteSpace(p.Name))
            .ToDictionary(p => p.Name, StringComparer.Ordinal);

        foreach (var prompt in discoveredPrompts)
        {
            if (string.IsNullOrWhiteSpace(prompt.Name)) continue;
            var discoveredText = NormalizeString(prompt.Text);
            if (discoveredText != null) continue;

            McpbManifestPrompt? existing = null;
            if (manifestByName != null)
            {
                manifestByName.TryGetValue(prompt.Name, out existing);
            }
            var existingHasText = existing != null && !string.IsNullOrWhiteSpace(existing.Text);
            if (existingHasText)
            {
                warnings.Add($"Prompt '{prompt.Name}' did not return text during discovery; keeping manifest text.");
            }
            else
            {
                warnings.Add($"Prompt '{prompt.Name}' did not return text during discovery; consider adding text to manifest manually.");
            }
        }

        return warnings;
    }

    internal static List<McpbManifestPrompt> MergePromptMetadata(IEnumerable<McpbManifestPrompt>? manifestPrompts, IEnumerable<McpbManifestPrompt> discoveredPrompts)
    {
        var manifestByName = manifestPrompts?
            .Where(p => !string.IsNullOrWhiteSpace(p.Name))
            .ToDictionary(p => p.Name, StringComparer.Ordinal);

        return discoveredPrompts
            .Where(p => !string.IsNullOrWhiteSpace(p.Name))
            .Select(p =>
            {
                McpbManifestPrompt? existing = null;
                if (manifestByName != null)
                {
                    manifestByName.TryGetValue(p.Name, out existing);
                }
                var mergedText = existing != null && !string.IsNullOrWhiteSpace(existing.Text)
                    ? existing.Text!
                    : (!string.IsNullOrWhiteSpace(p.Text) ? p.Text! : string.Empty);
                return new McpbManifestPrompt
                {
                    Name = p.Name,
                    Description = p.Description,
                    Arguments = p.Arguments != null && p.Arguments.Count > 0
                        ? new List<string>(p.Arguments)
                        : null,
                    Text = mergedText
                };
            })
            .ToList();
    }

    internal static bool ApplyWindowsMetaStaticResponses(
        McpbManifest manifest,
        McpbInitializeResult? initializeResponse,
        McpbToolsListResult? toolsListResponse)
    {
        if (initializeResponse == null && toolsListResponse == null)
        {
            return false;
        }

        var windowsMeta = GetWindowsMeta(manifest);
        var staticResponses = windowsMeta.StaticResponses ?? new McpbStaticResponses();
        bool changed = false;

        if (initializeResponse != null)
        {
            var initializePayload = BuildInitializeStaticResponse(initializeResponse);
            if (!AreJsonEquivalent(staticResponses.Initialize, initializePayload))
            {
                staticResponses.Initialize = initializePayload;
                changed = true;
            }
        }

        if (toolsListResponse != null)
        {
            if (!AreJsonEquivalent(staticResponses.ToolsList, toolsListResponse))
            {
                staticResponses.ToolsList = toolsListResponse;
                changed = true;
            }
        }

        if (!changed)
        {
            return false;
        }

        windowsMeta.StaticResponses = staticResponses;
        SetWindowsMeta(manifest, windowsMeta);
        return true;
    }

    private static bool StringEqualsNormalized(string? a, string? b)
        => string.Equals(NormalizeString(a), NormalizeString(b), StringComparison.Ordinal);

    private static string? NormalizeString(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value;

    private static Dictionary<string, object> BuildInitializeStaticResponse(McpbInitializeResult response)
    {
        var result = new Dictionary<string, object>();
        if (!string.IsNullOrWhiteSpace(response.ProtocolVersion))
        {
            result["protocolVersion"] = response.ProtocolVersion!;
        }
        if (response.Capabilities != null)
        {
            result["capabilities"] = response.Capabilities;
        }
        if (response.ServerInfo != null)
        {
            result["serverInfo"] = response.ServerInfo;
        }
        if (!string.IsNullOrWhiteSpace(response.Instructions))
        {
            result["instructions"] = response.Instructions!;
        }
        return result;
    }

    private static IReadOnlyList<string> NormalizeArguments(IReadOnlyCollection<string>? args)
    {
        if (args == null || args.Count == 0) return Array.Empty<string>();
        return args.Where(a => !string.IsNullOrWhiteSpace(a)).Select(a => a).ToArray();
    }

    private static string FormatArguments(IReadOnlyList<string> args)
    {
        if (args.Count == 0) return "[]";
        return "[" + string.Join(", ", args) + "]";
    }

    private static string FormatValue(string? value)
    {
        var normalized = NormalizeString(value);
        return normalized ?? "(none)";
    }

    private static McpbWindowsMeta GetWindowsMeta(McpbManifest manifest)
    {
        if (manifest.Meta == null)
        {
            return new McpbWindowsMeta();
        }

        if (!manifest.Meta.TryGetValue("com.microsoft.windows", out var windowsMetaDict))
        {
            return new McpbWindowsMeta();
        }

        try
        {
            var json = JsonSerializer.Serialize(windowsMetaDict, McpbJsonContext.WriteOptions);
            return JsonSerializer.Deserialize(json, McpbJsonContext.Default.McpbWindowsMeta) as McpbWindowsMeta ?? new McpbWindowsMeta();
        }
        catch
        {
            return new McpbWindowsMeta();
        }
    }

    private static void SetWindowsMeta(McpbManifest manifest, McpbWindowsMeta windowsMeta)
    {
        manifest.Meta ??= new Dictionary<string, Dictionary<string, object>>(StringComparer.Ordinal);

        var json = JsonSerializer.Serialize(windowsMeta, McpbJsonContext.WriteOptions);
        var dict = JsonSerializer.Deserialize(json, McpbJsonContext.Default.DictionaryStringObject) as Dictionary<string, object> ?? new Dictionary<string, object>();

        manifest.Meta["com.microsoft.windows"] = dict;
    }

    private static bool AreJsonEquivalent(object? a, object? b)
    {
        if (ReferenceEquals(a, b)) return true;
        if (a == null || b == null) return false;

        try
        {
            var jsonA = JsonSerializer.Serialize(a, McpbJsonContext.WriteOptions);
            var jsonB = JsonSerializer.Serialize(b, McpbJsonContext.WriteOptions);
            return string.Equals(jsonA, jsonB, StringComparison.Ordinal);
        }
        catch
        {
            return false;
        }
    }
}
