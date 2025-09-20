using System.Text.Json;
using Mcpb.Json;
using Xunit;
using System.Diagnostics;
using System.IO;

namespace Mcpb.Tests;

public class CliValidateTests
{
    private string CreateTempDir()
    {
        var dir = Path.Combine(Path.GetTempPath(), "mcpb_cli_validate_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        return dir;
    }
    private (int exitCode, string stdout, string stderr) InvokeCli(string workingDir, params string[] args)
    {
        var root = Mcpb.Commands.CliRoot.Build();
        var prev = Directory.GetCurrentDirectory();
        Directory.SetCurrentDirectory(workingDir);
        using var swOut = new StringWriter();
        using var swErr = new StringWriter();
        try {
            var code = CommandRunner.Invoke(root, args, swOut, swErr);
            return (code, swOut.ToString(), swErr.ToString());
        }
        finally { Directory.SetCurrentDirectory(prev); }
    }
    [Fact]
    public void Validate_ValidManifest_Succeeds()
    {
        var dir = CreateTempDir();
        var manifest = new Mcpb.Core.McpbManifest { Name = "ok", Description = "desc", Author = new Mcpb.Core.McpbManifestAuthor{ Name = "A"}, Server = new Mcpb.Core.McpbManifestServer{ Type="binary", EntryPoint="server/ok", McpConfig=new Mcpb.Core.McpServerConfigWithOverrides{ Command="${__dirname}/server/ok"}}};
        File.WriteAllText(Path.Combine(dir,"manifest.json"), JsonSerializer.Serialize(manifest, McpbJsonContext.Default.McpbManifest));
        var (code, stdout, stderr) = InvokeCli(dir, "validate", "manifest.json");
        Assert.Equal(0, code);
        Assert.Contains("Manifest is valid!", stdout);
        Assert.True(string.IsNullOrWhiteSpace(stderr));
    }

    [Fact]
    public void Validate_MissingDescription_Fails()
    {
        var dir = CreateTempDir();
        // Build JSON manually without description
        var json = "{" +
            "\"manifest_version\":\"0.2\"," +
            "\"name\":\"ok\"," +
            "\"version\":\"1.0.0\"," +
            "\"author\":{\"name\":\"A\"}," +
            "\"server\":{\"type\":\"binary\",\"entry_point\":\"server/ok\",\"mcp_config\":{\"command\":\"${__dirname}/server/ok\"}}" +
            "}";
        File.WriteAllText(Path.Combine(dir,"manifest.json"), json);
        var (code2, stdout2, stderr2) = InvokeCli(dir, "validate", "manifest.json");
        Assert.NotEqual(0, code2);
        Assert.Contains("description is required", stderr2);
    }

    [Fact]
    public void Validate_DxtVersionOnly_Warns()
    {
        var dir = CreateTempDir();
        // JSON with only dxt_version (deprecated) no manifest_version
        var json = "{" +
            "\"dxt_version\":\"0.2\"," +
            "\"name\":\"ok\"," +
            "\"version\":\"1.0.0\"," +
            "\"description\":\"desc\"," +
            "\"author\":{\"name\":\"A\"}," +
            "\"server\":{\"type\":\"binary\",\"entry_point\":\"server/ok\",\"mcp_config\":{\"command\":\"${__dirname}/server/ok\"}}" +
            "}";
        File.WriteAllText(Path.Combine(dir,"manifest.json"), json);
        var (code3, stdout3, stderr3) = InvokeCli(dir, "validate", "manifest.json");
        Assert.Equal(0, code3);
        Assert.Contains("Manifest is valid!", stdout3);
        Assert.Contains("deprecated", stdout3 + stderr3, StringComparison.OrdinalIgnoreCase);
    }
}