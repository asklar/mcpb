using Mcpb.Core;
using ModelContextProtocol.Protocol;
using System.Text.Json;
using Xunit;

namespace Mcpb.Tests;

public class ToolsListComparerTests
{
    [Fact]
    public void AreEqual_BothNull_ReturnsTrue()
    {
        var result = ToolsListComparer.AreEqual(null, null);        
        Assert.True(result);
    }

    [Fact]
    public void AreEqual_ManifestNullServerNotNull_ReturnsFalse()
    {
        var serverToolsList = new McpbToolsListResult { Tools = new List<Tool>() };
        var result = ToolsListComparer.AreEqual(null, serverToolsList);
        Assert.False(result);
    }

    [Fact]
    public void AreEqual_ManifestNotNullServerNull_ReturnsFalse()
    {
        var manifestToolsList = new McpbToolsListResult { Tools = new List<Tool>() };
        var result = ToolsListComparer.AreEqual(manifestToolsList, null);
        Assert.False(result);
    }

    [Fact]
    public void AreEqual_EmptyToolsLists_ReturnsTrue()
    {
        var manifest = new McpbToolsListResult { Tools = new List<Tool>() };
        var server = new McpbToolsListResult { Tools = new List<Tool>() };
        var result = ToolsListComparer.AreEqual(manifest, server);
        Assert.True(result);
    }

    [Fact]
    public void AreEqual_MultipleTools_ReturnsTrue()
    {
        var manifest = new McpbToolsListResult
        {
            Tools = new List<Tool>
            {
                new Tool { Name = "tool1", Description = "First tool" },
                new Tool { Name = "tool2", Description = "Second tool" },
                new Tool { Name = "tool3", Description = "Third tool" }
            }
        };
        var server = new McpbToolsListResult
        {
            Tools = new List<Tool>
            {
                new Tool { Name = "tool1", Description = "First tool" },
                new Tool { Name = "tool2", Description = "Second tool" },
                new Tool { Name = "tool3", Description = "Third tool" }
            }
        };
        var result = ToolsListComparer.AreEqual(manifest, server);
        Assert.True(result);
    }

    [Fact]
    public void AreEqual_MultipleToolsDifferentOrder_ReturnsTrue()
    {
        var manifest = new McpbToolsListResult
        {
            Tools = new List<Tool>
            {
                new Tool { Name = "tool1", Description = "First tool" },
                new Tool { Name = "tool2", Description = "Second tool" }
            }
        };
        var server = new McpbToolsListResult
        {
            Tools = new List<Tool>
            {
                new Tool { Name = "tool2", Description = "Second tool" },
                new Tool { Name = "tool1", Description = "First tool" }
            }
        };
        var result = ToolsListComparer.AreEqual(manifest, server);
        Assert.True(result);
    }

    [Fact]
    public void AreEqual_DifferentToolCount_ReturnsFalse()
    {
        var manifest = new McpbToolsListResult
        {
            Tools = new List<Tool>
            {
                new Tool { Name = "tool1" }
            }
        };
        var server = new McpbToolsListResult
        {
            Tools = new List<Tool>
            {
                new Tool { Name = "tool1" },
                new Tool { Name = "tool2" }
            }
        };
        var result = ToolsListComparer.AreEqual(manifest, server);
        Assert.False(result);
    }

    [Fact]
    public void AreEqual_SameToolsWithoutSchemas_ReturnsTrue()
    {
        var manifest = new McpbToolsListResult
        {
            Tools = new List<Tool>
            {
                new Tool { Name = "tool1", Description = "Test tool" }
            }
        };
        var server = new McpbToolsListResult
        {
            Tools = new List<Tool>
            {
                new Tool { Name = "tool1", Description = "Test tool" }
            }
        };
        var result = ToolsListComparer.AreEqual(manifest, server);
        Assert.True(result);
    }

    [Fact]
    public void AreEqual_DifferentToolDescription_ReturnsFalse()
    {
        var manifest = new McpbToolsListResult
        {
            Tools = new List<Tool>
            {
                new Tool { Name = "tool1", Description = "Description 1" }
            }
        };
        var server = new McpbToolsListResult
        {
            Tools = new List<Tool>
            {
                new Tool { Name = "tool1", Description = "Description 2" }
            }
        };
        var result = ToolsListComparer.AreEqual(manifest, server);
        Assert.False(result);
    }

    [Fact]
    public void AreEqual_MatchingInputSchema_ReturnsTrue()
    {
        var schemaJson = @"{
            ""type"": ""object"",
            ""properties"": {
                ""param1"": { ""type"": ""string"" },
                ""param2"": { ""type"": ""number"" }
            },
            ""required"": [""param1""]
        }";
        var schema = JsonDocument.Parse(schemaJson).RootElement;

        var manifest = new McpbToolsListResult
        {
            Tools = new List<Tool>
            {
                new Tool { Name = "tool1", InputSchema = schema }
            }
        };
        var server = new McpbToolsListResult
        {
            Tools = new List<Tool>
            {
                new Tool { Name = "tool1", InputSchema = schema.Clone() }
            }
        };
        var result = ToolsListComparer.AreEqual(manifest, server);
        Assert.True(result);
    }

    [Fact]
    public void AreEqual_DifferentInputSchemaPropertyTypes_ReturnsFalse()
    {
        var manifestSchemaJson = @"{
            ""type"": ""object"",
            ""properties"": {
                ""param1"": { ""type"": ""string"" }
            }
        }";
        var serverSchemaJson = @"{
            ""type"": ""object"",
            ""properties"": {
                ""param1"": { ""type"": ""number"" }
            }
        }";

        var manifest = new McpbToolsListResult
        {
            Tools = new List<Tool>
            {
                new Tool 
                { 
                    Name = "tool1", 
                    InputSchema = JsonDocument.Parse(manifestSchemaJson).RootElement 
                }
            }
        };
        var server = new McpbToolsListResult
        {
            Tools = new List<Tool>
            {
                new Tool 
                { 
                    Name = "tool1", 
                    InputSchema = JsonDocument.Parse(serverSchemaJson).RootElement 
                }
            }
        };
        var result = ToolsListComparer.AreEqual(manifest, server);
        Assert.False(result);
    }

    [Fact]
    public void AreEqual_DifferentInputSchemaPropertyCount_ReturnsFalse()
    {
        var manifestSchemaJson = @"{
            ""type"": ""object"",
            ""properties"": {
                ""param1"": { ""type"": ""string"" }
            }
        }";
        var serverSchemaJson = @"{
            ""type"": ""object"",
            ""properties"": {
                ""param1"": { ""type"": ""string"" },
                ""param2"": { ""type"": ""number"" }
            }
        }";

        var manifest = new McpbToolsListResult
        {
            Tools = new List<Tool>
            {
                new Tool 
                { 
                    Name = "tool1", 
                    InputSchema = JsonDocument.Parse(manifestSchemaJson).RootElement 
                }
            }
        };
        var server = new McpbToolsListResult
        {
            Tools = new List<Tool>
            {
                new Tool 
                { 
                    Name = "tool1", 
                    InputSchema = JsonDocument.Parse(serverSchemaJson).RootElement 
                }
            }
        };
        var result = ToolsListComparer.AreEqual(manifest, server);
        Assert.False(result);
    }

    [Fact]
    public void AreEqual_MatchingOutputSchema_ReturnsTrue()
    {
        var schemaJson = @"{
            ""type"": ""object"",
            ""properties"": {
                ""result"": { ""type"": ""string"" }
            }
        }";
        var schema = JsonDocument.Parse(schemaJson).RootElement;

        var manifest = new McpbToolsListResult
        {
            Tools = new List<Tool>
            {
                new Tool { Name = "tool1", OutputSchema = schema }
            }
        };
        var server = new McpbToolsListResult
        {
            Tools = new List<Tool>
            {
                new Tool { Name = "tool1", OutputSchema = schema.Clone() }
            }
        };
        var result = ToolsListComparer.AreEqual(manifest, server);
        Assert.True(result);
    }

    [Fact]
    public void AreEqual_DifferentOutputSchema_ReturnsFalse()
    {
        var manifestSchemaJson = @"{
            ""type"": ""object"",
            ""properties"": {
                ""result"": { ""type"": ""string"" }
            }
        }";
        var serverSchemaJson = @"{
            ""type"": ""object"",
            ""properties"": {
                ""result"": { ""type"": ""number"" }
            }
        }";

        var manifest = new McpbToolsListResult
        {
            Tools = new List<Tool>
            {
                new Tool 
                { 
                    Name = "tool1", 
                    OutputSchema = JsonDocument.Parse(manifestSchemaJson).RootElement 
                }
            }
        };
        var server = new McpbToolsListResult
        {
            Tools = new List<Tool>
            {
                new Tool 
                { 
                    Name = "tool1", 
                    OutputSchema = JsonDocument.Parse(serverSchemaJson).RootElement 
                }
            }
        };
        var result = ToolsListComparer.AreEqual(manifest, server);
        Assert.False(result);
    }

    [Fact]
    public void AreEqual_BothSchemasNotSet_ReturnsTrue()
    {
        var manifest = new McpbToolsListResult
        {
            Tools = new List<Tool>
            {
                new Tool { Name = "tool1" }
            }
        };
        var server = new McpbToolsListResult
        {
            Tools = new List<Tool>
            {
                new Tool { Name = "tool1" }
            }
        };
        var result = ToolsListComparer.AreEqual(manifest, server);
        Assert.True(result);
    }

    [Fact]
    public void AreEqual_ComplexNestedSchema_ReturnsTrue()
    {
        var schemaJson = @"{
            ""type"": ""object"",
            ""properties"": {
                ""nested"": {
                    ""type"": ""object"",
                    ""properties"": {
                        ""value"": { ""type"": ""string"" },
                        ""count"": { ""type"": ""number"" }
                    }
                },
                ""array"": {
                    ""type"": ""array"",
                    ""items"": { ""type"": ""string"" }
                }
            },
            ""required"": [""nested""]
        }";
        var schema = JsonDocument.Parse(schemaJson).RootElement;

        var manifest = new McpbToolsListResult
        {
            Tools = new List<Tool>
            {
                new Tool { Name = "tool1", InputSchema = schema }
            }
        };
        var server = new McpbToolsListResult
        {
            Tools = new List<Tool>
            {
                new Tool { Name = "tool1", InputSchema = schema.Clone() }
            }
        };
        var result = ToolsListComparer.AreEqual(manifest, server);
        Assert.True(result);
    }

    [Fact]
    public void AreEqual_CaseInsensitiveToolName_ReturnsTrue()
    {
        var manifest = new McpbToolsListResult
        {
            Tools = new List<Tool>
            {
                new Tool { Name = "ToolOne" }
            }
        };
        var server = new McpbToolsListResult
        {
            Tools = new List<Tool>
            {
                new Tool { Name = "toolone" }
            }
        };
        var result = ToolsListComparer.AreEqual(manifest, server);
        Assert.True(result);
    }

    [Fact]
    public void JsonElementDeepEquals_SameStringValues_ReturnsTrue()
    {
        var json1 = JsonDocument.Parse(@"""test""").RootElement;
        var json2 = JsonDocument.Parse(@"""test""").RootElement;

        var result = ToolsListComparer.JsonElementDeepEquals(json1, json2, out string? mismatchPath);

        Assert.True(result);
        Assert.Null(mismatchPath);
    }

    [Fact]
    public void JsonElementDeepEquals_DifferentStringValues_ReturnsFalse()
    {
        var json1 = JsonDocument.Parse(@"""test1""").RootElement;
        var json2 = JsonDocument.Parse(@"""test2""").RootElement;

        var result = ToolsListComparer.JsonElementDeepEquals(json1, json2, out string? mismatchPath);

        Assert.False(result);
        Assert.Equal(string.Empty, mismatchPath);
    }

    [Fact]
    public void JsonElementDeepEquals_SameNumberValues_ReturnsTrue()
    {
        var json1 = JsonDocument.Parse(@"42").RootElement;
        var json2 = JsonDocument.Parse(@"42").RootElement;

        var result = ToolsListComparer.JsonElementDeepEquals(json1, json2, out string? mismatchPath);

        Assert.True(result);
        Assert.Null(mismatchPath);
    }

    [Fact]
    public void JsonElementDeepEquals_DifferentNumberValues_ReturnsFalse()
    {
        var json1 = JsonDocument.Parse(@"42").RootElement;
        var json2 = JsonDocument.Parse(@"43").RootElement;

        var result = ToolsListComparer.JsonElementDeepEquals(json1, json2, out string? mismatchPath);

        Assert.False(result);
        Assert.Equal(string.Empty, mismatchPath);
    }

    [Fact]
    public void JsonElementDeepEquals_SameBooleanValues_ReturnsTrue()
    {
        var json1 = JsonDocument.Parse(@"true").RootElement;
        var json2 = JsonDocument.Parse(@"true").RootElement;

        var result = ToolsListComparer.JsonElementDeepEquals(json1, json2, out string? mismatchPath);

        Assert.True(result);
        Assert.Null(mismatchPath);
    }

    [Fact]
    public void JsonElementDeepEquals_NullValues_ReturnsTrue()
    {
        var json1 = JsonDocument.Parse(@"null").RootElement;
        var json2 = JsonDocument.Parse(@"null").RootElement;

        var result = ToolsListComparer.JsonElementDeepEquals(json1, json2, out string? mismatchPath);

        Assert.True(result);
        Assert.Null(mismatchPath);
    }

    [Fact]
    public void JsonElementDeepEquals_DifferentValueKinds_ReturnsFalse()
    {
        var json1 = JsonDocument.Parse(@"""test""").RootElement;
        var json2 = JsonDocument.Parse(@"42").RootElement;

        var result = ToolsListComparer.JsonElementDeepEquals(json1, json2, out string? mismatchPath);

        Assert.False(result);
        Assert.Equal(string.Empty, mismatchPath);
    }

    [Fact]
    public void JsonElementDeepEquals_SameObjects_ReturnsTrue()
    {
        var json1 = JsonDocument.Parse(@"{""key"": ""value"", ""num"": 42}").RootElement;
        var json2 = JsonDocument.Parse(@"{""key"": ""value"", ""num"": 42}").RootElement;

        var result = ToolsListComparer.JsonElementDeepEquals(json1, json2, out string? mismatchPath);

        Assert.True(result);
        Assert.Null(mismatchPath);
    }

    [Fact]
    public void JsonElementDeepEquals_ObjectsWithDifferentPropertyCount_ReturnsFalse()
    {
        var json1 = JsonDocument.Parse(@"{""key"": ""value""}").RootElement;
        var json2 = JsonDocument.Parse(@"{""key"": ""value"", ""extra"": ""prop""}").RootElement;

        var result = ToolsListComparer.JsonElementDeepEquals(json1, json2, out string? mismatchPath);

        Assert.False(result);
        Assert.Equal(string.Empty, mismatchPath);
    }

    [Fact]
    public void JsonElementDeepEquals_ObjectsMissingProperty_ReturnsFalse()
    {
        var json1 = JsonDocument.Parse(@"{""key1"": ""value""}").RootElement;
        var json2 = JsonDocument.Parse(@"{""key2"": ""value""}").RootElement;

        var result = ToolsListComparer.JsonElementDeepEquals(json1, json2, out string? mismatchPath);

        Assert.False(result);
        Assert.Equal("key1", mismatchPath);
    }

    [Fact]
    public void JsonElementDeepEquals_NestedObjectsDifferent_ReturnsFalseWithPath()
    {
        var json1 = JsonDocument.Parse(@"{""outer"": {""inner"": ""value1""}}").RootElement;
        var json2 = JsonDocument.Parse(@"{""outer"": {""inner"": ""value2""}}").RootElement;

        var result = ToolsListComparer.JsonElementDeepEquals(json1, json2, out string? mismatchPath);

        Assert.False(result);
        Assert.Equal("outer.inner", mismatchPath);
    }

    [Fact]
    public void JsonElementDeepEquals_SameArrays_ReturnsTrue()
    {
        var json1 = JsonDocument.Parse(@"[1, 2, 3]").RootElement;
        var json2 = JsonDocument.Parse(@"[1, 2, 3]").RootElement;

        var result = ToolsListComparer.JsonElementDeepEquals(json1, json2, out string? mismatchPath);

        Assert.True(result);
        Assert.Null(mismatchPath);
    }

    [Fact]
    public void JsonElementDeepEquals_DifferentArrayLengths_ReturnsFalse()
    {
        var json1 = JsonDocument.Parse(@"[1, 2]").RootElement;
        var json2 = JsonDocument.Parse(@"[1, 2, 3]").RootElement;

        var result = ToolsListComparer.JsonElementDeepEquals(json1, json2, out string? mismatchPath);

        Assert.False(result);
        Assert.Equal(string.Empty, mismatchPath);
    }

    [Fact]
    public void JsonElementDeepEquals_ArrayElementsDifferent_ReturnsFalseWithPath()
    {
        var json1 = JsonDocument.Parse(@"[1, 2, 3]").RootElement;
        var json2 = JsonDocument.Parse(@"[1, 5, 3]").RootElement;

        var result = ToolsListComparer.JsonElementDeepEquals(json1, json2, out string? mismatchPath);

        Assert.False(result);
        Assert.Equal("[1]", mismatchPath);
    }

    [Fact]
    public void AreEqual_BothInputAndOutputSchemas_ReturnsTrue()
    {
        var inputSchemaJson = @"{
            ""type"": ""object"",
            ""properties"": {
                ""input"": { ""type"": ""string"" }
            }
        }";
        var outputSchemaJson = @"{
            ""type"": ""object"",
            ""properties"": {
                ""output"": { ""type"": ""string"" }
            }
        }";

        var manifest = new McpbToolsListResult
        {
            Tools = new List<Tool>
            {
                new Tool 
                { 
                    Name = "tool1",
                    Description = "A test tool",
                    InputSchema = JsonDocument.Parse(inputSchemaJson).RootElement,
                    OutputSchema = JsonDocument.Parse(outputSchemaJson).RootElement
                }
            }
        };
        var server = new McpbToolsListResult
        {
            Tools = new List<Tool>
            {
                new Tool 
                { 
                    Name = "tool1",
                    Description = "A test tool",
                    InputSchema = JsonDocument.Parse(inputSchemaJson).RootElement,
                    OutputSchema = JsonDocument.Parse(outputSchemaJson).RootElement
                }
            }
        };
        var result = ToolsListComparer.AreEqual(manifest, server);
        Assert.True(result);
    }
}
