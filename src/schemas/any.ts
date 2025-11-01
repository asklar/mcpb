import * as z from "zod";

import * as v0_1 from "./0.1.js";
import * as v0_2 from "./0.2.js";
import * as v0_3 from "./0.3.js";

/**
 * Union schema that accepts any supported manifest version.
 * Uses preprocessing to normalize dxt_version to manifest_version,
 * then efficiently discriminates based on the now-required manifest_version field.
 */
export const McpbManifestSchema = z.preprocess(
  (val) => {
    // Normalize: if it has dxt_version, ensure manifest_version is also set
    if (val && typeof val === "object" && "dxt_version" in val) {
      const obj = val as Record<string, unknown>;
      if (!obj.manifest_version && obj.dxt_version) {
        return { ...obj, manifest_version: obj.dxt_version };
      }
    }
    return val;
  },
  z.union([
    v0_1.McpbManifestSchema,
    v0_2.McpbManifestSchema,
    v0_3.McpbManifestSchema,
  ]),
);
