using System.Text.Json;
using Mcpb.Core;
using Mcpb.Json;

namespace Mcpb.Commands;

/// <summary>
/// Shared logic for checking and updating _meta static_responses
/// used by both pack and validate commands.
/// </summary>
internal static class StaticResponsesHelper
{
    /// <summary>
    /// Checks discovered static responses against the manifest and optionally
    /// updates the manifest in place when <paramref name="update"/> is true.
    /// </summary>
    /// <returns>Whether a tools/list mismatch was detected.</returns>
    internal static bool CheckAndUpdate(
        McpbManifest manifest,
        McpbInitializeResult? discoveredInitResponse,
        McpbToolsListResult? discoveredToolsListResponse,
        bool update,
        out string? message)
    {
        message = null;
        if (!update || (discoveredInitResponse == null && discoveredToolsListResponse == null))
            return false;

        var windowsMeta = GetOrCreateWindowsMeta(manifest);
        var staticResponses = windowsMeta.StaticResponses ?? new McpbStaticResponses();
        bool mismatch = false;

        if (discoveredInitResponse != null)
        {
            var initDict = new Dictionary<string, object>();
            if (discoveredInitResponse.ProtocolVersion != null)
                initDict["protocolVersion"] = discoveredInitResponse.ProtocolVersion;
            if (discoveredInitResponse.Capabilities != null)
                initDict["capabilities"] = discoveredInitResponse.Capabilities;
            if (discoveredInitResponse.ServerInfo != null)
                initDict["serverInfo"] = discoveredInitResponse.ServerInfo;
            if (!string.IsNullOrWhiteSpace(discoveredInitResponse.Instructions))
                initDict["instructions"] = discoveredInitResponse.Instructions;

            staticResponses.Initialize = initDict;
        }

        if (discoveredToolsListResponse != null)
        {
            string staticResponsesToolsListJson = JsonSerializer.Serialize(staticResponses.ToolsList, McpbJsonContext.WriteOptions);
            string discoveredToolsListJson = JsonSerializer.Serialize(discoveredToolsListResponse, McpbJsonContext.WriteOptions);

            if (!string.Equals(staticResponsesToolsListJson, discoveredToolsListJson, StringComparison.Ordinal))
            {
                mismatch = true;
                message = "Tool schema mismatch in _meta static_responses:";
            }

            staticResponses.ToolsList = discoveredToolsListResponse;
        }

        windowsMeta.StaticResponses = staticResponses;
        SetWindowsMeta(manifest, windowsMeta);

        return mismatch;
    }

    internal static McpbWindowsMeta GetOrCreateWindowsMeta(McpbManifest manifest)
    {
        manifest.Meta ??= new Dictionary<string, Dictionary<string, object>>();

        if (!manifest.Meta.TryGetValue("com.microsoft.windows", out var windowsMetaDict))
        {
            return new McpbWindowsMeta();
        }

        try
        {
            var json = JsonSerializer.Serialize(windowsMetaDict, McpbJsonContext.WriteOptions);
            return JsonSerializer.Deserialize<McpbWindowsMeta>(json) ?? new McpbWindowsMeta();
        }
        catch
        {
            return new McpbWindowsMeta();
        }
    }

    internal static void SetWindowsMeta(McpbManifest manifest, McpbWindowsMeta windowsMeta)
    {
        manifest.Meta ??= new Dictionary<string, Dictionary<string, object>>();

        var json = JsonSerializer.Serialize(windowsMeta, McpbJsonContext.WriteOptions);
        var dict = JsonSerializer.Deserialize<Dictionary<string, object>>(json) ?? new Dictionary<string, object>();

        manifest.Meta["com.microsoft.windows"] = dict;
    }
}
