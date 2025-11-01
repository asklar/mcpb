import type * as z from "zod";

import * as v0_1 from "./0.1.js";
import * as v0_2 from "./0.2.js";
import * as v0_3 from "./0.3.js";

type McpbManifestV0_1 = z.infer<typeof v0_1.McpbManifestSchema>;
type McpbManifestV0_2 = z.infer<typeof v0_2.McpbManifestSchema>;
type McpbManifestV0_3 = z.infer<typeof v0_3.McpbManifestSchema>;

/**
 * Union type representing any supported manifest version.
 * This is the return type of validateManifest.
 */
type ValidatedManifest = McpbManifestV0_1 | McpbManifestV0_2 | McpbManifestV0_3;

/**
 * Validates a manifest using the appropriate schema based on its manifest_version.
 *
 * This function automatically detects the manifest version from the data and applies
 * the correct schema. This avoids TypeScript "excessively deep" errors that can occur
 * when using union schemas across package boundaries.
 *
 * @param data - The manifest data to validate
 * @returns The validated manifest with the correct type
 * @throws ZodError if validation fails
 *
 * @example
 * ```typescript
 * import { validateManifest } from '@anthropic-ai/mcpb/browser';
 *
 * const manifest = validateManifest(unknownData);
 * // manifest is typed as McpbManifestAny (union of all versions)
 * ```
 */
export function validateManifest(data: unknown): ValidatedManifest {
  // Read manifest_version or dxt_version from data
  const version = (data as any)?.manifest_version || (data as any)?.dxt_version;

  // Use appropriate schema based on version
  if (version === "0.1") {
    return v0_1.McpbManifestSchema.parse(data);
  }

  if (version === "0.2") {
    return v0_2.McpbManifestSchema.parse(data);
  }

  // Default to latest version (0.3) if not specified or if version is "0.3"
  return v0_3.McpbManifestSchema.parse(data);
}

/**
 * Validates a manifest using the appropriate schema, returning a Result-style object.
 *
 * @param data - The manifest data to validate
 * @returns An object with either { success: true, data } or { success: false, error }
 *
 * @example
 * ```typescript
 * import { validateManifestSafe } from '@anthropic-ai/mcpb/browser';
 *
 * const result = validateManifestSafe(unknownData);
 * if (result.success) {
 *   console.log(result.data);
 * } else {
 *   console.error(result.error);
 * }
 * ```
 */
export function validateManifestSafe(data: unknown):
  | { success: true; data: ValidatedManifest }
  | { success: false; error: unknown } {
  try {
    return { success: true, data: validateManifest(data) };
  } catch (error) {
    return { success: false, error };
  }
}

/**
 * Get the appropriate schema for a given manifest version.
 * Useful when you need to work with the schema directly.
 *
 * @param version - The manifest version (e.g., "0.1", "0.2", "0.3")
 * @returns The schema for that version
 *
 * @example
 * ```typescript
 * import { getSchemaForVersion } from '@anthropic-ai/mcpb/browser';
 *
 * const schema = getSchemaForVersion("0.2");
 * const result = schema.safeParse(data);
 * ```
 */
export function getSchemaForVersion(version: "0.1" | "0.2" | "0.3") {
  if (version === "0.1") return v0_1.McpbManifestSchema;
  if (version === "0.2") return v0_2.McpbManifestSchema;
  return v0_3.McpbManifestSchema;
}
