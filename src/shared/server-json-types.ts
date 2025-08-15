// Generated from https://github.com/modelcontextprotocol/registry/blob/main/docs/server-json/schema.json
// Do not edit manually. Run scripts/create-server-json-type.sh to regenerate.

/**
 * Schema for a static representation of an MCP server. Used in various contexts related to discovery, installation, and configuration.
 */
export type MCPServerDetail = {
  /**
   * Server name/identifier
   */
  name: string;
  /**
   * Human-readable description of the server's functionality
   */
  description: string;
  /**
   * Server lifecycle status. 'deprecated' indicates the server is no longer recommended for new usage.
   */
  status?: "active" | "deprecated";
  repository?: {
    url: string;
    /**
     * Repository hosting service
     */
    source: string;
    id: string;
  };
  /**
   * Version information for this server. Defined as an object to allow for downstream extensibility (e.g. release_date)
   */
  version_detail: {
    /**
     * Equivalent of Implementation.version in MCP specification.
     */
    version: string;
  };
} & {
  packages?: {
    /**
     * Package registry type
     */
    registry_name: string;
    /**
     * Package name in the registry
     */
    name: string;
    /**
     * Package version
     */
    version: string;
    /**
     * A hint to help clients determine the appropriate runtime for the package. This field should be provided when `runtime_arguments` are present.
     */
    runtime_hint?: string;
    /**
     * A list of arguments to be passed to the package's runtime command (such as docker or npx). The `runtime_hint` field should be provided when `runtime_arguments` are present.
     */
    runtime_arguments?: (
      | (({
          /**
           * A description of the input, which clients can use to provide context to the user.
           */
          description?: string;
          is_required?: boolean;
          /**
           * Specifies the input format. Supported values include `filepath`, which should be interpreted as a file on the user's filesystem.
           *
           * When the input is converted to a string, booleans should be represented by the strings "true" and "false", and numbers should be represented as decimal values.
           */
          format?: "string" | "number" | "boolean" | "filepath";
          /**
           * The default value for the input. If this is not set, the user may be prompted to provide a value. If a value is set, it should not be configurable by end users.
           *
           * Identifiers wrapped in `{curly_braces}` will be replaced with the corresponding properties from the input `variables` map. If an identifier in braces is not found in `variables`, or if `variables` is not provided, the `{curly_braces}` substring should remain unchanged.
           *
           */
          value?: string;
          /**
           * Indicates whether the input is a secret value (e.g., password, token). If true, clients should handle the value securely.
           */
          is_secret?: boolean;
          /**
           * The default value for the input.
           */
          default?: string;
          /**
           * A list of possible values for the input. If provided, the user must select one of these values.
           */
          choices?: string[];
        } & {
          /**
           * A map of variable names to their values. Keys in the input `value` that are wrapped in `{curly_braces}` will be replaced with the corresponding variable values.
           */
          variables?: {
            [k: string]: {
              /**
               * A description of the input, which clients can use to provide context to the user.
               */
              description?: string;
              is_required?: boolean;
              /**
               * Specifies the input format. Supported values include `filepath`, which should be interpreted as a file on the user's filesystem.
               *
               * When the input is converted to a string, booleans should be represented by the strings "true" and "false", and numbers should be represented as decimal values.
               */
              format?: "string" | "number" | "boolean" | "filepath";
              /**
               * The default value for the input. If this is not set, the user may be prompted to provide a value. If a value is set, it should not be configurable by end users.
               *
               * Identifiers wrapped in `{curly_braces}` will be replaced with the corresponding properties from the input `variables` map. If an identifier in braces is not found in `variables`, or if `variables` is not provided, the `{curly_braces}` substring should remain unchanged.
               *
               */
              value?: string;
              /**
               * Indicates whether the input is a secret value (e.g., password, token). If true, clients should handle the value securely.
               */
              is_secret?: boolean;
              /**
               * The default value for the input.
               */
              default?: string;
              /**
               * A list of possible values for the input. If provided, the user must select one of these values.
               */
              choices?: string[];
            };
          };
        }) & {
          [k: string]: unknown;
        })
      | (({
          /**
           * A description of the input, which clients can use to provide context to the user.
           */
          description?: string;
          is_required?: boolean;
          /**
           * Specifies the input format. Supported values include `filepath`, which should be interpreted as a file on the user's filesystem.
           *
           * When the input is converted to a string, booleans should be represented by the strings "true" and "false", and numbers should be represented as decimal values.
           */
          format?: "string" | "number" | "boolean" | "filepath";
          /**
           * The default value for the input. If this is not set, the user may be prompted to provide a value. If a value is set, it should not be configurable by end users.
           *
           * Identifiers wrapped in `{curly_braces}` will be replaced with the corresponding properties from the input `variables` map. If an identifier in braces is not found in `variables`, or if `variables` is not provided, the `{curly_braces}` substring should remain unchanged.
           *
           */
          value?: string;
          /**
           * Indicates whether the input is a secret value (e.g., password, token). If true, clients should handle the value securely.
           */
          is_secret?: boolean;
          /**
           * The default value for the input.
           */
          default?: string;
          /**
           * A list of possible values for the input. If provided, the user must select one of these values.
           */
          choices?: string[];
        } & {
          /**
           * A map of variable names to their values. Keys in the input `value` that are wrapped in `{curly_braces}` will be replaced with the corresponding variable values.
           */
          variables?: {
            [k: string]: {
              /**
               * A description of the input, which clients can use to provide context to the user.
               */
              description?: string;
              is_required?: boolean;
              /**
               * Specifies the input format. Supported values include `filepath`, which should be interpreted as a file on the user's filesystem.
               *
               * When the input is converted to a string, booleans should be represented by the strings "true" and "false", and numbers should be represented as decimal values.
               */
              format?: "string" | "number" | "boolean" | "filepath";
              /**
               * The default value for the input. If this is not set, the user may be prompted to provide a value. If a value is set, it should not be configurable by end users.
               *
               * Identifiers wrapped in `{curly_braces}` will be replaced with the corresponding properties from the input `variables` map. If an identifier in braces is not found in `variables`, or if `variables` is not provided, the `{curly_braces}` substring should remain unchanged.
               *
               */
              value?: string;
              /**
               * Indicates whether the input is a secret value (e.g., password, token). If true, clients should handle the value securely.
               */
              is_secret?: boolean;
              /**
               * The default value for the input.
               */
              default?: string;
              /**
               * A list of possible values for the input. If provided, the user must select one of these values.
               */
              choices?: string[];
            };
          };
        }) & {
          type: "named";
          /**
           * The flag name, including any leading dashes.
           */
          name: string;
          /**
           * Whether the argument can be repeated multiple times.
           */
          is_repeated?: boolean;
        })
    )[];
    /**
     * A list of arguments to be passed to the package's binary.
     */
    package_arguments?: (
      | (({
          /**
           * A description of the input, which clients can use to provide context to the user.
           */
          description?: string;
          is_required?: boolean;
          /**
           * Specifies the input format. Supported values include `filepath`, which should be interpreted as a file on the user's filesystem.
           *
           * When the input is converted to a string, booleans should be represented by the strings "true" and "false", and numbers should be represented as decimal values.
           */
          format?: "string" | "number" | "boolean" | "filepath";
          /**
           * The default value for the input. If this is not set, the user may be prompted to provide a value. If a value is set, it should not be configurable by end users.
           *
           * Identifiers wrapped in `{curly_braces}` will be replaced with the corresponding properties from the input `variables` map. If an identifier in braces is not found in `variables`, or if `variables` is not provided, the `{curly_braces}` substring should remain unchanged.
           *
           */
          value?: string;
          /**
           * Indicates whether the input is a secret value (e.g., password, token). If true, clients should handle the value securely.
           */
          is_secret?: boolean;
          /**
           * The default value for the input.
           */
          default?: string;
          /**
           * A list of possible values for the input. If provided, the user must select one of these values.
           */
          choices?: string[];
        } & {
          /**
           * A map of variable names to their values. Keys in the input `value` that are wrapped in `{curly_braces}` will be replaced with the corresponding variable values.
           */
          variables?: {
            [k: string]: {
              /**
               * A description of the input, which clients can use to provide context to the user.
               */
              description?: string;
              is_required?: boolean;
              /**
               * Specifies the input format. Supported values include `filepath`, which should be interpreted as a file on the user's filesystem.
               *
               * When the input is converted to a string, booleans should be represented by the strings "true" and "false", and numbers should be represented as decimal values.
               */
              format?: "string" | "number" | "boolean" | "filepath";
              /**
               * The default value for the input. If this is not set, the user may be prompted to provide a value. If a value is set, it should not be configurable by end users.
               *
               * Identifiers wrapped in `{curly_braces}` will be replaced with the corresponding properties from the input `variables` map. If an identifier in braces is not found in `variables`, or if `variables` is not provided, the `{curly_braces}` substring should remain unchanged.
               *
               */
              value?: string;
              /**
               * Indicates whether the input is a secret value (e.g., password, token). If true, clients should handle the value securely.
               */
              is_secret?: boolean;
              /**
               * The default value for the input.
               */
              default?: string;
              /**
               * A list of possible values for the input. If provided, the user must select one of these values.
               */
              choices?: string[];
            };
          };
        }) & {
          [k: string]: unknown;
        })
      | (({
          /**
           * A description of the input, which clients can use to provide context to the user.
           */
          description?: string;
          is_required?: boolean;
          /**
           * Specifies the input format. Supported values include `filepath`, which should be interpreted as a file on the user's filesystem.
           *
           * When the input is converted to a string, booleans should be represented by the strings "true" and "false", and numbers should be represented as decimal values.
           */
          format?: "string" | "number" | "boolean" | "filepath";
          /**
           * The default value for the input. If this is not set, the user may be prompted to provide a value. If a value is set, it should not be configurable by end users.
           *
           * Identifiers wrapped in `{curly_braces}` will be replaced with the corresponding properties from the input `variables` map. If an identifier in braces is not found in `variables`, or if `variables` is not provided, the `{curly_braces}` substring should remain unchanged.
           *
           */
          value?: string;
          /**
           * Indicates whether the input is a secret value (e.g., password, token). If true, clients should handle the value securely.
           */
          is_secret?: boolean;
          /**
           * The default value for the input.
           */
          default?: string;
          /**
           * A list of possible values for the input. If provided, the user must select one of these values.
           */
          choices?: string[];
        } & {
          /**
           * A map of variable names to their values. Keys in the input `value` that are wrapped in `{curly_braces}` will be replaced with the corresponding variable values.
           */
          variables?: {
            [k: string]: {
              /**
               * A description of the input, which clients can use to provide context to the user.
               */
              description?: string;
              is_required?: boolean;
              /**
               * Specifies the input format. Supported values include `filepath`, which should be interpreted as a file on the user's filesystem.
               *
               * When the input is converted to a string, booleans should be represented by the strings "true" and "false", and numbers should be represented as decimal values.
               */
              format?: "string" | "number" | "boolean" | "filepath";
              /**
               * The default value for the input. If this is not set, the user may be prompted to provide a value. If a value is set, it should not be configurable by end users.
               *
               * Identifiers wrapped in `{curly_braces}` will be replaced with the corresponding properties from the input `variables` map. If an identifier in braces is not found in `variables`, or if `variables` is not provided, the `{curly_braces}` substring should remain unchanged.
               *
               */
              value?: string;
              /**
               * Indicates whether the input is a secret value (e.g., password, token). If true, clients should handle the value securely.
               */
              is_secret?: boolean;
              /**
               * The default value for the input.
               */
              default?: string;
              /**
               * A list of possible values for the input. If provided, the user must select one of these values.
               */
              choices?: string[];
            };
          };
        }) & {
          type: "named";
          /**
           * The flag name, including any leading dashes.
           */
          name: string;
          /**
           * Whether the argument can be repeated multiple times.
           */
          is_repeated?: boolean;
        })
    )[];
    /**
     * A mapping of environment variables to be set when running the package.
     */
    environment_variables?: (({
      /**
       * A description of the input, which clients can use to provide context to the user.
       */
      description?: string;
      is_required?: boolean;
      /**
       * Specifies the input format. Supported values include `filepath`, which should be interpreted as a file on the user's filesystem.
       *
       * When the input is converted to a string, booleans should be represented by the strings "true" and "false", and numbers should be represented as decimal values.
       */
      format?: "string" | "number" | "boolean" | "filepath";
      /**
       * The default value for the input. If this is not set, the user may be prompted to provide a value. If a value is set, it should not be configurable by end users.
       *
       * Identifiers wrapped in `{curly_braces}` will be replaced with the corresponding properties from the input `variables` map. If an identifier in braces is not found in `variables`, or if `variables` is not provided, the `{curly_braces}` substring should remain unchanged.
       *
       */
      value?: string;
      /**
       * Indicates whether the input is a secret value (e.g., password, token). If true, clients should handle the value securely.
       */
      is_secret?: boolean;
      /**
       * The default value for the input.
       */
      default?: string;
      /**
       * A list of possible values for the input. If provided, the user must select one of these values.
       */
      choices?: string[];
    } & {
      /**
       * A map of variable names to their values. Keys in the input `value` that are wrapped in `{curly_braces}` will be replaced with the corresponding variable values.
       */
      variables?: {
        [k: string]: {
          /**
           * A description of the input, which clients can use to provide context to the user.
           */
          description?: string;
          is_required?: boolean;
          /**
           * Specifies the input format. Supported values include `filepath`, which should be interpreted as a file on the user's filesystem.
           *
           * When the input is converted to a string, booleans should be represented by the strings "true" and "false", and numbers should be represented as decimal values.
           */
          format?: "string" | "number" | "boolean" | "filepath";
          /**
           * The default value for the input. If this is not set, the user may be prompted to provide a value. If a value is set, it should not be configurable by end users.
           *
           * Identifiers wrapped in `{curly_braces}` will be replaced with the corresponding properties from the input `variables` map. If an identifier in braces is not found in `variables`, or if `variables` is not provided, the `{curly_braces}` substring should remain unchanged.
           *
           */
          value?: string;
          /**
           * Indicates whether the input is a secret value (e.g., password, token). If true, clients should handle the value securely.
           */
          is_secret?: boolean;
          /**
           * The default value for the input.
           */
          default?: string;
          /**
           * A list of possible values for the input. If provided, the user must select one of these values.
           */
          choices?: string[];
        };
      };
    }) & {
      /**
       * Name of the header or environment variable.
       */
      name: string;
    })[];
  }[];
  remotes?: {
    /**
     * Transport protocol type
     */
    transport_type: "streamable" | "sse";
    /**
     * Remote server URL
     */
    url: string;
    /**
     * HTTP headers to include
     */
    headers?: (({
      /**
       * A description of the input, which clients can use to provide context to the user.
       */
      description?: string;
      is_required?: boolean;
      /**
       * Specifies the input format. Supported values include `filepath`, which should be interpreted as a file on the user's filesystem.
       *
       * When the input is converted to a string, booleans should be represented by the strings "true" and "false", and numbers should be represented as decimal values.
       */
      format?: "string" | "number" | "boolean" | "filepath";
      /**
       * The default value for the input. If this is not set, the user may be prompted to provide a value. If a value is set, it should not be configurable by end users.
       *
       * Identifiers wrapped in `{curly_braces}` will be replaced with the corresponding properties from the input `variables` map. If an identifier in braces is not found in `variables`, or if `variables` is not provided, the `{curly_braces}` substring should remain unchanged.
       *
       */
      value?: string;
      /**
       * Indicates whether the input is a secret value (e.g., password, token). If true, clients should handle the value securely.
       */
      is_secret?: boolean;
      /**
       * The default value for the input.
       */
      default?: string;
      /**
       * A list of possible values for the input. If provided, the user must select one of these values.
       */
      choices?: string[];
    } & {
      /**
       * A map of variable names to their values. Keys in the input `value` that are wrapped in `{curly_braces}` will be replaced with the corresponding variable values.
       */
      variables?: {
        [k: string]: {
          /**
           * A description of the input, which clients can use to provide context to the user.
           */
          description?: string;
          is_required?: boolean;
          /**
           * Specifies the input format. Supported values include `filepath`, which should be interpreted as a file on the user's filesystem.
           *
           * When the input is converted to a string, booleans should be represented by the strings "true" and "false", and numbers should be represented as decimal values.
           */
          format?: "string" | "number" | "boolean" | "filepath";
          /**
           * The default value for the input. If this is not set, the user may be prompted to provide a value. If a value is set, it should not be configurable by end users.
           *
           * Identifiers wrapped in `{curly_braces}` will be replaced with the corresponding properties from the input `variables` map. If an identifier in braces is not found in `variables`, or if `variables` is not provided, the `{curly_braces}` substring should remain unchanged.
           *
           */
          value?: string;
          /**
           * Indicates whether the input is a secret value (e.g., password, token). If true, clients should handle the value securely.
           */
          is_secret?: boolean;
          /**
           * The default value for the input.
           */
          default?: string;
          /**
           * A list of possible values for the input. If provided, the user must select one of these values.
           */
          choices?: string[];
        };
      };
    }) & {
      /**
       * Name of the header or environment variable.
       */
      name: string;
    })[];
  }[];
};
