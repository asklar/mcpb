import type z from "zod";

import type { DxtManifestSchema } from "../schemas.js";
import type { DxtManifestRepository } from "../types.js";
import type { MCPServerDetail as ServerJson } from "./server-json-types.js";

type ServerJsonPackage = NonNullable<ServerJson["packages"]>[number];

// Type for ServerJson with guaranteed packages array
type ServerJsonWithPackages = ServerJson & {
  packages: NonNullable<ServerJson["packages"]>;
};

export interface ConversionResult {
  manifest: z.infer<typeof DxtManifestSchema>;
  warnings: string[];
}

/**
 * Convert a server.json object to a DXT manifest.json format
 */
export function convertServerJsonToManifest(
  serverJson: ServerJsonWithPackages,
): ConversionResult {
  const warnings: string[] = [];

  // Extract basic info
  const name = getNameFromServer(serverJson);
  const description = getDescriptionFromServer(serverJson);
  const version = getVersionFromServer(serverJson);
  const repository = getRepositoryFromServer(serverJson);

  const { serverType, selectedPackage } = analyzePackagesAndSelectBest(
    serverJson.packages,
  );

  // Build command and args
  const { command, args } = buildCommandFromPackage(
    serverType,
    selectedPackage,
  );

  // Handle multiple packages warning
  if (serverJson.packages.length > 1) {
    warnings.push(
      `server.json contains ${serverJson.packages.length} packages, only using the ${serverType} package (${selectedPackage.name})`,
    );
  }

  // Get environment variables from selected package
  const env = getEnvironmentVariablesFromPackage(selectedPackage) || {};

  // Handle user configuration from environment variables
  const userConfig: Record<
    string,
    {
      type: "string" | "number" | "boolean" | "directory" | "file";
      title: string;
      description: string;
      required?: boolean;
      sensitive?: boolean;
      default?: string | number | boolean | string[];
      multiple?: boolean;
      min?: number;
      max?: number;
    }
  > = {};

  if (selectedPackage.environment_variables) {
    for (const envVar of selectedPackage.environment_variables) {
      const configKey = envVar.name.toLowerCase().replace(/_/g, "_");

      // Map server.json format to DXT type
      let configType: "string" | "number" | "boolean" | "directory" | "file" =
        "string";
      if (envVar.format === "number") {
        configType = "number";
      } else if (envVar.format === "boolean") {
        configType = "boolean";
      } else if (envVar.format === "filepath") {
        configType = "file";
      }

      userConfig[configKey] = {
        type: configType,
        title: envVar.name,
        description:
          envVar.description || `${envVar.name} environment variable`,
        required: envVar.is_required || false,
        sensitive: envVar.is_secret || false,
      };

      if (envVar.default) {
        userConfig[configKey].default = envVar.default;
      }

      if (envVar.choices && envVar.choices.length > 0) {
        warnings.push(
          `Environment variable ${envVar.name} has choices constraint, which is not supported in DXT manifests`,
        );
      }

      // Update env to use user_config reference
      env[envVar.name] = `\${user_config.${configKey}}`;
    }
  }

  // Handle remotes
  if (serverJson.remotes && serverJson.remotes.length > 0) {
    warnings.push(
      "server.json contains remote configurations, which are not supported in DXT manifests",
    );
  }

  // Determine entry point
  let entryPoint: string;
  if (serverType === "node") {
    entryPoint = "server/index.js";
  } else if (serverType === "python") {
    entryPoint = "server/main.py";
  } else {
    entryPoint = "server/server";
  }
  warnings.push(`Using default entry point: ${entryPoint}`);

  // Build the manifest
  const manifest: z.infer<typeof DxtManifestSchema> = {
    dxt_version: "0.1",
    name,
    version,
    description,
    author: {
      name: "Unknown Author",
    },
    server: {
      type: serverType,
      entry_point: entryPoint,
      mcp_config: {
        command,
        ...(args.length > 0 && { args }),
        ...(Object.keys(env).length > 0 && { env }),
      },
    },
  };

  // Add repository if available
  if (repository) {
    manifest.repository = repository;
  }

  if (Object.keys(userConfig).length > 0) {
    manifest.user_config = userConfig;
  }

  return { manifest, warnings };
}

/**
 * Parse a server.json file content and convert it to a manifest
 */
export function parseAndConvertServerJson(
  serverJsonContent: string,
): ConversionResult {
  let parsedJson: unknown;

  try {
    parsedJson = JSON.parse(serverJsonContent);
  } catch (error) {
    throw new Error(
      `Failed to parse server.json: ${error instanceof Error ? error.message : "Unknown error"}`,
    );
  }

  // Type assertion - trusting that the JSON matches our ServerJson type
  const serverJson = parsedJson as ServerJson;

  // Ensure we have at least one package
  if (!serverJson.packages || serverJson.packages.length === 0) {
    throw new Error("server.json must contain at least one package");
  }

  // Type assertion - we've verified packages exist above
  const serverJsonWithPackages = serverJson as ServerJsonWithPackages;

  return convertServerJsonToManifest(serverJsonWithPackages);
}

/**
 * Extract the name from a server.json object
 */
export function getNameFromServer(serverJson: ServerJson): string {
  return serverJson.name;
}

/**
 * Extract the description from a server.json object
 */
export function getDescriptionFromServer(serverJson: ServerJson): string {
  return serverJson.description;
}

/**
 * Extract the version from a server.json object
 */
export function getVersionFromServer(serverJson: ServerJson): string {
  return serverJson.version_detail.version;
}

/**
 * Convert server.json repository to manifest.json repository format
 * Note: manifest.json only uses type and url, so we omit source and id
 */
export function getRepositoryFromServer(
  serverJson: ServerJson,
): DxtManifestRepository | undefined {
  if (!serverJson.repository) {
    return undefined;
  }

  return {
    type: "git", // Assuming git as default since server.json doesn't specify type
    url: serverJson.repository.url,
  };
}

/**
 * Extract environment variables from a package
 */
export function getEnvironmentVariablesFromPackage(
  pkg: ServerJsonPackage,
): Record<string, string> | undefined {
  if (!pkg.environment_variables || pkg.environment_variables.length === 0) {
    return undefined;
  }

  const env: Record<string, string> = {};
  for (const envVar of pkg.environment_variables) {
    // Use the value if provided, otherwise use the default, or empty string
    const value = envVar.value || envVar.default || "";
    env[envVar.name] = String(value);
  }

  return env;
}

/**
 * Analyze packages and determine the best server type and package to use
 * Priority order: node > python > binary
 */
export function analyzePackagesAndSelectBest(
  packages: ServerJsonWithPackages["packages"],
): {
  serverType: "node" | "python" | "binary";
  selectedPackage: ServerJsonPackage;
} {
  const isNodePkg = (pkg: ServerJsonPackage) =>
    pkg.registry_name === "npm" ||
    pkg.runtime_hint === "npx" ||
    pkg.runtime_hint === "node";

  const isPythonPkg = (pkg: ServerJsonPackage) =>
    pkg.registry_name === "pypi" ||
    pkg.runtime_hint === "uvx" ||
    pkg.runtime_hint === "python";

  const nodePackage = packages.find(isNodePkg);
  const pythonPackage = packages.find(isPythonPkg);

  if (nodePackage) {
    return { serverType: "node", selectedPackage: nodePackage };
  } else if (pythonPackage) {
    return { serverType: "python", selectedPackage: pythonPackage };
  } else {
    return { serverType: "binary", selectedPackage: packages[0] };
  }
}

/**
 * Build command and arguments based on server type and package
 */
function buildCommandFromPackage(
  serverType: "node" | "python" | "binary",
  selectedPackage: ServerJsonPackage,
): { command: string; args: string[] } {
  let command: string;
  let args: string[] = [];

  if (serverType === "node") {
    if (selectedPackage.runtime_hint === "npx") {
      command = "npx";
      args = [selectedPackage.name];
      if (selectedPackage.version && selectedPackage.version !== "latest") {
        args[0] = `${selectedPackage.name}@${selectedPackage.version}`;
      }
    } else {
      command = "node";
      args = ["${__dirname}/server/index.js"];
    }
  } else if (serverType === "python") {
    if (selectedPackage.runtime_hint === "uvx") {
      command = "uvx";
      args = [selectedPackage.name];
      if (selectedPackage.version && selectedPackage.version !== "latest") {
        args[0] = `${selectedPackage.name}@${selectedPackage.version}`;
      }
    } else {
      command = "python";
      args = ["${__dirname}/server/main.py"];
    }
  } else {
    // Binary type
    command = "${__dirname}/server/server";
    args = [];
  }

  // Add package arguments
  const packageArgs = getCommandArgumentsFromPackage(selectedPackage);
  if (packageArgs) {
    args.push(...packageArgs);
  }

  return { command, args };
}

/**
 * Extract command arguments from a package
 */
export function getCommandArgumentsFromPackage(
  pkg: ServerJsonPackage,
): string[] | undefined {
  if (!pkg.package_arguments || pkg.package_arguments.length === 0) {
    return undefined;
  }

  const args: string[] = [];

  for (const arg of pkg.package_arguments) {
    if ("type" in arg && arg.type === "positional") {
      // For positional arguments, use the value if provided
      if (arg.value) {
        args.push(String(arg.value));
      }
    } else if ("type" in arg && arg.type === "named" && "name" in arg) {
      // For named arguments, add the flag and value separately
      args.push(String(arg.name));
      if (arg.value) {
        args.push(String(arg.value));
      }
    }
  }

  return args.length > 0 ? args : undefined;
}

/**
 * Helper function to determine if a value from server.json can be imported
 * Returns true if the value is non-empty and meaningful
 */
export function canImportValue(value: unknown): boolean {
  if (value === null || value === undefined) {
    return false;
  }

  if (typeof value === "string") {
    return value.trim().length > 0;
  }

  if (Array.isArray(value)) {
    return value.length > 0;
  }

  if (typeof value === "object") {
    return Object.keys(value).length > 0;
  }

  return true;
}
