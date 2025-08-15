import { existsSync, readFileSync, writeFileSync } from "fs";
import { dirname, join, resolve } from "path";

import { parseAndConvertServerJson } from "../shared/from-server-json.js";
import { getLogger } from "../shared/log.js";

export interface ImportOptions {
  serverJsonPath: string;
  outputPath?: string;
}

/**
 * Import a server.json file and convert it to manifest.json
 */
export async function importServerJson(
  options: ImportOptions,
): Promise<boolean> {
  const logger = getLogger();

  // Resolve paths
  const serverPath = resolve(options.serverJsonPath);
  const serverDir = dirname(serverPath);
  const outputPath = options.outputPath
    ? resolve(options.outputPath)
    : join(serverDir, "manifest.json");

  // Check if server.json exists
  if (!existsSync(serverPath)) {
    logger.error(`ERROR: File not found: ${serverPath}`);
    return false;
  }

  // Read server.json
  let serverJsonContent: string;
  try {
    serverJsonContent = readFileSync(serverPath, "utf-8");
  } catch (error) {
    logger.error(
      `ERROR: Failed to read server.json: ${error instanceof Error ? error.message : "Unknown error"}`,
    );
    return false;
  }

  logger.log(`Importing from: ${serverPath}`);

  try {
    const { manifest: convertedManifest, warnings } = parseAndConvertServerJson(
      serverJsonContent,
    );

    // Show warnings
    if (warnings.length > 0) {
      logger.log("\nConversion warnings:");
      for (const warning of warnings) {
        logger.log(`  ⚠️  ${warning}`);
      }
      logger.log(""); // Empty line after warnings
    }

    // Check if output file already exists
    if (existsSync(outputPath)) {
      logger.log(
        `\n⚠️  Warning: ${outputPath} already exists and will be overwritten.`,
      );
    }

    // Write the converted manifest
    writeFileSync(outputPath, JSON.stringify(convertedManifest, null, 2));
    logger.log(`\n✅ Successfully converted server.json to manifest.json`);
    logger.log(`   Output: ${outputPath}`);

    // Show a summary of what was created
    logger.log("\nManifest summary:");
    logger.log(`  Name: ${convertedManifest.name}`);
    logger.log(`  Version: ${convertedManifest.version}`);
    logger.log(`  Description: ${convertedManifest.description}`);
    logger.log(`  Server type: ${convertedManifest.server.type}`);
    logger.log(`  Entry point: ${convertedManifest.server.entry_point}`);
    logger.log(`  Author: ${convertedManifest.author.name}`);

    if (
      convertedManifest.user_config &&
      Object.keys(convertedManifest.user_config).length > 0
    ) {
      logger.log(
        `  User config: ${Object.keys(convertedManifest.user_config).length} option(s)`,
      );
    }

    // Show note about missing fields
    logger.log(
      "\n📝 Note: Please update the following fields in manifest.json:",
    );
    logger.log("  - author.name (currently set to 'Unknown Author')");
    logger.log("  - Verify the server.entry_point is correct");
    logger.log("  - Add any additional metadata (homepage, license, etc.)");

    return true;
  } catch (error) {
    logger.error(
      `ERROR: Failed to convert server.json: ${error instanceof Error ? error.message : "Unknown error"}`,
    );
    return false;
  }
}
