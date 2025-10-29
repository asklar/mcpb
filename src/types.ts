import type * as z from "zod";

import type { McpbManifestSchema as McpbManifestSchema_v0_1 } from "./schemas/0.1.js";
import type { McpbManifestSchema as McpbManifestSchema_v0_2 } from "./schemas/0.2.js";
import type { McpbManifestSchema as McpbManifestSchema_v0_3 } from "./schemas/0.3.js";
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
} from "./schemas/latest.js";

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
 * Discriminated union of all supported manifest versions.
 * Discriminated by manifest_version or dxt_version field.
 */
export type McpbManifest =
  | z.infer<typeof McpbManifestSchema_v0_1>
  | z.infer<typeof McpbManifestSchema_v0_2>
  | z.infer<typeof McpbManifestSchema_v0_3>;

/**
 * Information about a MCPB package signature
 */
export type McpbSignatureInfo = z.infer<typeof McpbSignatureInfoSchema>;

export interface Logger {
  log: (...args: unknown[]) => void;
  error: (...args: unknown[]) => void;
  warn: (...args: unknown[]) => void;
}
