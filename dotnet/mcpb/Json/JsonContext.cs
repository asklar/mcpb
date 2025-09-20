using System.Text.Json.Serialization;
using Mcpb.Core;

namespace Mcpb.Json;

[JsonSerializable(typeof(McpbManifest))]
[JsonSerializable(typeof(Dictionary<string,string>))]
[JsonSourceGenerationOptions(WriteIndented = true, PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
public partial class McpbJsonContext : JsonSerializerContext
{
}
