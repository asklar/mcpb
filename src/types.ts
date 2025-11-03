import type * as z from "zod";

import type { McpbManifestSchema as McpbManifestSchemaAny } from "./schemas/any.js";
import type { VERSIONED_MANIFEST_SCHEMAS } from "./schemas/index.js";
import type { DEFAULT_MANIFEST_VERSION } from "./shared/constants.js";
// Import schema types from the version matching DEFAULT_MANIFEST_VERSION
import type {
  McpbManifestAuthorSchema,
  McpbManifestCompatibilitySchema,
  McpbManifestMcpConfigSchema,
  McpbManifestPlatformOverrideSchema,
  McpbManifestPromptSchema,
  McpbManifestRepositorySchema,
  McpbManifestServerSchema,
  McpbManifestToolSchema,
  McpbSignatureInfoSchema,
  McpbUserConfigurationOptionSchema,
  McpbUserConfigValuesSchema,
  McpServerConfigSchema,
} from "./schemas/0.2.js";

export type McpServerConfig = z.infer<typeof McpServerConfigSchema>;

export type McpbManifestAuthor = z.infer<typeof McpbManifestAuthorSchema>;

export type McpbManifestRepository = z.infer<
  typeof McpbManifestRepositorySchema
>;

export type McpbManifestPlatformOverride = z.infer<
  typeof McpbManifestPlatformOverrideSchema
>;

export type McpbManifestMcpConfig = z.infer<typeof McpbManifestMcpConfigSchema>;

export type McpbManifestServer = z.infer<typeof McpbManifestServerSchema>;

export type McpbManifestCompatibility = z.infer<
  typeof McpbManifestCompatibilitySchema
>;

export type McpbManifestTool = z.infer<typeof McpbManifestToolSchema>;

export type McpbManifestPrompt = z.infer<typeof McpbManifestPromptSchema>;

export type McpbUserConfigurationOption = z.infer<
  typeof McpbUserConfigurationOptionSchema
>;

export type McpbUserConfigValues = z.infer<typeof McpbUserConfigValuesSchema>;

/**
 * McpbManifest type representing the union of all manifest versions
 */
export type McpbManifestAny = z.infer<typeof McpbManifestSchemaAny>;

/**
 * McpbManifest type for the DEFAULT_MANIFEST_VERSION
 * Use this for creating new manifests with the default version.
 */
export type McpbManifestDefault = z.infer<
  (typeof VERSIONED_MANIFEST_SCHEMAS)[typeof DEFAULT_MANIFEST_VERSION]
>;

/**
 * @deprecated Use McpbManifestAny instead to support all manifest versions, or McpbManifestDefault for the default version.
 */
export type McpbManifest = McpbManifestDefault;

/**
 * Information about a MCPB package signature
 */
export type McpbSignatureInfo = z.infer<typeof McpbSignatureInfoSchema>;

export interface Logger {
  log: (...args: unknown[]) => void;
  error: (...args: unknown[]) => void;
  warn: (...args: unknown[]) => void;
}
