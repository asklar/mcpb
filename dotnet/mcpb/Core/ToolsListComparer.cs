using ModelContextProtocol.Protocol;

namespace Mcpb.Core;

/// <summary>
/// Compares static_response tools/list (from the manifest) against the runtime tools/list
/// response from the server, ensuring the server's actual tools match what was declared.
/// </summary>
public static class ToolsListComparer
{
    /// <summary>
    /// Validates that the server's tools/list response matches the static_response tools declared in the manifest.
    /// Returns false on the first mismatch found.
    /// </summary>
    /// <param name="manifestToolsListResult">The tools/list declared in the manifest's static_response.</param>
    /// <param name="serverToolsListResult">The result retrieved from the server's tools/list response.</param>
    /// <returns><c>true</c> if the server tools/list response matches the static_response; otherwise, <c>false</c>.</returns>
    public static bool AreEqual(McpbToolsListResult? manifestToolsListResult, McpbToolsListResult? serverToolsListResult)
    {
        if ((manifestToolsListResult is null && serverToolsListResult is not null) || (manifestToolsListResult is not null && serverToolsListResult is null))
        {
            return false;
        }

        if (manifestToolsListResult is not null)
        {
            List<Tool> manifestTools = manifestToolsListResult.Tools ?? new List<Tool>();
            List<Tool> serverTools = serverToolsListResult?.Tools ?? new List<Tool>();

            int manifestCount = manifestTools.Count;
            int serverCount = serverTools.Count;

            if (manifestCount != serverCount)
            {
                return false;
            }

            foreach (Tool manifestTool in manifestTools)
            {
                bool foundMatch = false;

                foreach (Tool serverTool in serverTools!)
                {
                    if (string.Equals(manifestTool.Name, serverTool.Name, StringComparison.OrdinalIgnoreCase))
                    {
                        if (CompareToolProperties(manifestTool, serverTool))
                        {
                            foundMatch = true;
                            break;
                        }
                    }
                }

                if (!foundMatch)
                {
                    return false;
                }
            }
        }

        return true;
    }

    private static bool CompareToolProperties(
        Tool manifestTool,
        Tool serverTool)
    {
        // Compare description (optional — only check if static_response has one).
        if (manifestTool.Description is not null)
        {
            string? serverToolDescription = serverTool.Description;
            if (!string.Equals(manifestTool.Description, serverToolDescription, StringComparison.Ordinal))
            {
                return false;
            }
        }

        // Compare inputSchema.
        if (!CompareSchema(manifestTool.InputSchema, serverTool.InputSchema))
        {
            return false;
        }

        // Compare outputSchema.
        if (!CompareSchema(manifestTool.OutputSchema, serverTool.OutputSchema))
        {
            return false;
        }

        return true;
    }

    private static bool CompareSchema(JsonElement? manifestSchema, JsonElement? serverSchema)
    {
        bool manifestDefinesSchema = manifestSchema.HasValue && manifestSchema.Value.ValueKind != JsonValueKind.Undefined;
        bool serverDefinesSchema = serverSchema.HasValue && serverSchema.Value.ValueKind != JsonValueKind.Undefined;

        if (manifestDefinesSchema != serverDefinesSchema)
        {
            return false;
        }

        if (manifestDefinesSchema &&
            !JsonElementDeepEquals(manifestSchema!.Value, serverSchema!.Value))
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Performs a deep comparison of two <see cref="JsonElement"/> values.
    /// </summary>
    /// <param name="element1">The first element to compare.</param>
    /// <param name="element2">The second element to compare.</param>
    /// <param name="mismatchPath">When the elements differ, the JSON path where the mismatch was found.</param>
    /// <returns><c>true</c> if the elements are deeply equal; otherwise, <c>false</c>.</returns>
    internal static bool JsonElementDeepEquals(JsonElement element1, JsonElement element2)
    {
        return JsonElementDeepEqualsCore(element1, element2, string.Empty);
    }

    private static bool JsonElementDeepEqualsCore(JsonElement element1, JsonElement element2, string currentPath)
    {
        if (element1.ValueKind != element2.ValueKind)
        {
            return false;
        }

        switch (element1.ValueKind)
        {
            case JsonValueKind.Object:
                int count1 = 0;
                foreach (JsonProperty _ in element1.EnumerateObject())
                {
                    count1++;
                }

                int count2 = 0;
                foreach (JsonProperty _ in element2.EnumerateObject())
                {
                    count2++;
                }

                if (count1 != count2)
                {
                    return false;
                }

                foreach (JsonProperty property1 in element1.EnumerateObject())
                {
                    string propertyPath = string.IsNullOrEmpty(currentPath) ? property1.Name : $"{currentPath}.{property1.Name}";

                    if (!element2.TryGetProperty(property1.Name, out JsonElement property2Value))
                    {
                        return false;
                    }

                    if (!JsonElementDeepEqualsCore(property1.Value, property2Value, propertyPath))
                    {
                        return false;
                    }
                }

                return true;

            case JsonValueKind.Array:
                int arrayLength1 = 0;
                foreach (JsonElement _ in element1.EnumerateArray())
                {
                    arrayLength1++;
                }

                int arrayLength2 = 0;
                foreach (JsonElement _ in element2.EnumerateArray())
                {
                    arrayLength2++;
                }

                if (arrayLength1 != arrayLength2)
                {
                    return false;
                }

                int index = 0;
                using (JsonElement.ArrayEnumerator enumerator1 = element1.EnumerateArray())
                using (JsonElement.ArrayEnumerator enumerator2 = element2.EnumerateArray())
                {
                    while (enumerator1.MoveNext() && enumerator2.MoveNext())
                    {
                        string arrayPath = $"{currentPath}[{index}]";
                        if (!JsonElementDeepEqualsCore(enumerator1.Current, enumerator2.Current, arrayPath))
                        {
                            return false;
                        }

                        index++;
                    }
                }

                return true;

            case JsonValueKind.String:
                if (element1.GetString() != element2.GetString())
                {
                    return false;
                }

                return true;

                // Compare numbers by value rather than by their raw JSON text to avoid
                // treating numerically equivalent values with different formatting as mismatches.
                if (element1.TryGetDecimal(out decimal decimal1) && element2.TryGetDecimal(out decimal decimal2))
                {
                    if (decimal1 != decimal2)
                    {
                        mismatchPath = currentPath;
                        return false;
                    }
                }
                else if (element1.TryGetDouble(out double double1) && element2.TryGetDouble(out double double2))
                {
                    if (double1 != double2)
                    {
                        mismatchPath = currentPath;
                        return false;
                    }
                }
                else
                {
                    // Fallback to raw text comparison if numeric parsing is not possible.
                    if (element1.GetRawText() != element2.GetRawText())
                    {
                        mismatchPath = currentPath;
                        return false;
                    }
                }

                return true;

            case JsonValueKind.True:
            case JsonValueKind.False:
            case JsonValueKind.Null:
                return true;

            default:
                return false;
        }
    }
}
