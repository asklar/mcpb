import {
  McpbManifestSchema as McpbManifestSchemaLatest,
  McpbSignatureInfoSchema,
} from "../dist/schemas/latest.js";
import { McpbManifestSchema as McpbManifestSchema_v0_1 } from "../dist/schemas/0.1.js";
import { McpbManifestSchema as McpbManifestSchema_v0_2 } from "../dist/schemas/0.2.js";
import { McpbManifestSchema as McpbManifestSchema_v0_3 } from "../dist/schemas/0.3.js";
import { zodToJsonSchema } from "zod-to-json-schema";
import fs from "node:fs/promises";
import path from "node:path";

const distDir = path.join(import.meta.dirname, "../dist");
const schemasDir = path.join(import.meta.dirname, "../schemas");

// Versioned manifest schemas (for both dist/ and schemas/)
const versionedManifestSchemas = {
  "mcpb-manifest-v0.1": McpbManifestSchema_v0_1,
  "mcpb-manifest-v0.2": McpbManifestSchema_v0_2,
  "mcpb-manifest-v0.3": McpbManifestSchema_v0_3,
  "mcpb-manifest-latest": McpbManifestSchemaLatest,
};

// Legacy alias (only for schemas/ directory, not exported from package)
const schemasOnlyAliases = {
  "mcpb-manifest": McpbManifestSchemaLatest,
};

// Other schemas
const otherSchemas = {
  "mcpb-signature-info": McpbSignatureInfoSchema,
};

// Generate versioned schemas and other schemas to dist/
await fs.mkdir(distDir, { recursive: true });
for (const [key, schema] of Object.entries({
  ...versionedManifestSchemas,
  ...otherSchemas,
})) {
  const jsonSchema = zodToJsonSchema(schema);
  const filePath = path.join(distDir, `${key}.schema.json`);
  await fs.writeFile(filePath, JSON.stringify(jsonSchema, null, 2), {
    encoding: "utf8",
  });
}

// Generate all manifest schemas to schemas/ (including legacy alias)
await fs.mkdir(schemasDir, { recursive: true });
for (const [key, schema] of Object.entries({
  ...versionedManifestSchemas,
  ...schemasOnlyAliases,
})) {
  const jsonSchema = zodToJsonSchema(schema);
  const filePath = path.join(schemasDir, `${key}.schema.json`);
  await fs.writeFile(filePath, JSON.stringify(jsonSchema, null, 2), {
    encoding: "utf8",
  });
}
