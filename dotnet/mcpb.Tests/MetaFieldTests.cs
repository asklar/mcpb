using System.Collections.Generic;
using System.Text.Json;
using Mcpb.Core;
using Mcpb.Json;
using Xunit;

namespace Mcpb.Tests;

public class MetaFieldTests
{
    [Fact]
    public void Manifest_CanHaveMeta()
    {
        var manifest = new McpbManifest
        {
            ManifestVersion = "0.2",
            Name = "test",
            Version = "1.0.0",
            Description = "Test manifest",
            Author = new McpbManifestAuthor { Name = "Test Author" },
            Server = new McpbManifestServer
            {
                Type = "node",
                EntryPoint = "server/index.js",
                McpConfig = new McpServerConfigWithOverrides
                {
                    Command = "node",
                    Args = new List<string> { "server/index.js" }
                }
            },
            Meta = new Dictionary<string, Dictionary<string, object>>
            {
                ["com.microsoft.windows"] = new Dictionary<string, object>
                {
                    ["package_family_name"] = "TestPackage_123",
                    ["channel"] = "stable"
                }
            }
        };

        var json = JsonSerializer.Serialize(manifest, McpbJsonContext.WriteOptions);
        Assert.Contains("\"_meta\"", json);
        Assert.Contains("\"com.microsoft.windows\"", json);
        Assert.Contains("\"package_family_name\"", json);

        var deserialized = JsonSerializer.Deserialize<McpbManifest>(json, McpbJsonContext.Default.McpbManifest);
        Assert.NotNull(deserialized);
        Assert.NotNull(deserialized.Meta);
        Assert.True(deserialized.Meta.ContainsKey("com.microsoft.windows"));
    }

    [Fact]
    public void Manifest_MetaIsOptional()
    {
        var manifest = new McpbManifest
        {
            ManifestVersion = "0.2",
            Name = "test",
            Version = "1.0.0",
            Description = "Test manifest",
            Author = new McpbManifestAuthor { Name = "Test Author" },
            Server = new McpbManifestServer
            {
                Type = "node",
                EntryPoint = "server/index.js",
                McpConfig = new McpServerConfigWithOverrides
                {
                    Command = "node",
                    Args = new List<string> { "server/index.js" }
                }
            }
        };

        var json = JsonSerializer.Serialize(manifest, McpbJsonContext.WriteOptions);
        Assert.DoesNotContain("\"_meta\"", json);
    }

    [Fact]
    public void Manifest_CanDeserializeWithWindowsMeta()
    {
        var json = @"{
            ""manifest_version"": ""0.2"",
            ""name"": ""test"",
            ""version"": ""1.0.0"",
            ""description"": ""Test manifest"",
            ""author"": { ""name"": ""Test Author"" },
            ""server"": {
                ""type"": ""node"",
                ""entry_point"": ""server/index.js"",
                ""mcp_config"": {
                    ""command"": ""node"",
                    ""args"": [""server/index.js""]
                }
            },
            ""_meta"": {
                ""com.microsoft.windows"": {
                    ""static_responses"": {
                        ""tools/list"": {
                            ""tools"": [
                                {
                                    ""name"": ""tool1"",
                                    ""description"": ""First tool""
                                }
                            ]
                        }
                    }
                }
            }
        }";

        var manifest = JsonSerializer.Deserialize<McpbManifest>(json, McpbJsonContext.Default.McpbManifest);
        Assert.NotNull(manifest);
        Assert.NotNull(manifest.Meta);
        Assert.True(manifest.Meta.ContainsKey("com.microsoft.windows"));
    }
}
