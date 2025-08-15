#!/bin/bash

# Script to generate TypeScript types from server.json JSON schema
# This creates type definitions we can use in our TypeScript code

set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
ROOT_DIR="$(cd "$SCRIPT_DIR/.." && pwd)"
SCHEMA_URL="https://raw.githubusercontent.com/modelcontextprotocol/registry/refs/heads/main/docs/server-json/schema.json"
TEMP_SCHEMA="$ROOT_DIR/temp-server-schema.json"
OUTPUT_FILE="$ROOT_DIR/src/shared/server-json-types.ts"

echo "Creating server.json TypeScript types..."

# Download the schema
echo "Downloading schema from $SCHEMA_URL..."
curl -s -o "$TEMP_SCHEMA" "$SCHEMA_URL"

# First, let's dereference the JSON schema to resolve all $refs
echo "Dereferencing JSON schema..."
node "$SCRIPT_DIR/dereference-schema.js" "$TEMP_SCHEMA" "$TEMP_SCHEMA.dereferenced"

# Convert JSON schema to TypeScript types using json-schema-to-typescript
echo "Converting to TypeScript types..."
npx json-schema-to-typescript "$TEMP_SCHEMA.dereferenced" -o "$OUTPUT_FILE" --bannerComment "// Generated from https://github.com/modelcontextprotocol/registry/blob/main/docs/server-json/schema.json
// Do not edit manually. Run scripts/create-server-json-type.sh to regenerate." --no-additionalProperties

# Clean up
rm -f "$TEMP_SCHEMA" "$TEMP_SCHEMA.dereferenced"

echo "✅ Generated TypeScript types at $OUTPUT_FILE"
echo ""
echo "You can now import and use the types:"
echo "  import type { ServerDetail } from './shared/server-json-types.js';"