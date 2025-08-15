# Server.json to Manifest.json Gap Analysis

This document analyzes the information gaps between the MCP (Model Context Protocol) `server.json` format and the DXT `manifest.json` format, identifying what information is missing and proposing potential enhancements to `server.json` to better support manifest generation.

## Overview

The `server.json` format is designed for package registry metadata, while `manifest.json` is designed for desktop extension configuration. This creates several information gaps when converting between formats.

## Information Available in Both Formats

### ✅ Fully Mapped Fields
- **Name**: `server.json.name` → `manifest.json.name`
- **Description**: `server.json.description` → `manifest.json.description`
- **Version**: `server.json.version_detail.version` → `manifest.json.version`
- **Repository URL**: `server.json.repository.url` → `manifest.json.repository.url`
- **Environment Variables**: `server.json.packages[].environment_variables` → `manifest.json.user_config` (with transformations)

### ⚠️ Partially Mapped Fields
- **Server Type**: Inferred from `packages[].registry_name` and `packages[].runtime_hint`
  - Current logic: npm/npx → "node", pypi/uvx → "python", docker/nuget → "binary"
  - **Gap**: No explicit server type declaration
- **Command**: Derived from package runtime hints (npx, uvx) or defaults
  - **Gap**: No explicit command specification for non-package-manager scenarios

## Critical Information Missing in server.json

### 1. Author Information 🔴
**Required in manifest.json but missing in server.json:**
- `author.name` (required)
- `author.email` (optional)
- `author.url` (optional)

**Current workaround**: Defaults to "Unknown Author"

**Proposed addition to server.json**:
```json
{
  "author": {
    "name": "string",
    "email": "string",
    "url": "string"
  }
}
```

### 2. Entry Point Specification 🔴
**Required for server execution but not in server.json:**
- `server.entry_point` - The actual file to execute

**Current workaround**: Defaults based on server type:
- Node: `"server/index.js"`
- Python: `"server/main.py"`
- Binary: `"server/server"`

**Proposed addition to server.json packages**:
```json
{
  "packages": [{
    "entry_point": "path/to/main.js"
  }]
}
```

### 3. Display and Marketing Information 🟡
**Optional but valuable fields missing:**
- `display_name` - Human-readable name
- `long_description` - Extended description
- `homepage` - Project homepage URL
- `documentation` - Documentation URL
- `support` - Support/issues URL
- `license` - License identifier
- `keywords` - Search keywords
- `icon` - Icon path
- `screenshots` - Screenshot paths

**Proposed additions to server.json**:
```json
{
  "display_name": "string",
  "long_description": "string",
  "homepage": "string",
  "documentation": "string",
  "support": "string",
  "license": "string",
  "keywords": ["string"],
  "icon": "string",
  "screenshots": ["string"]
}
```

### 4. Compatibility Information 🟡
**manifest.json supports detailed compatibility specs:**
- `compatibility.claude_desktop` - Required Claude Desktop version
- `compatibility.platforms` - Supported OS platforms
- `compatibility.runtimes.python` - Required Python version
- `compatibility.runtimes.node` - Required Node version

**server.json has no equivalent**

**Proposed addition**:
```json
{
  "compatibility": {
    "claude_desktop": ">=1.0.0",
    "platforms": ["darwin", "win32", "linux"],
    "runtimes": {
      "python": ">=3.8",
      "node": ">=18.0.0"
    }
  }
}
```

### 5. Tools and Prompts Metadata 🟡
**manifest.json can include:**
- `tools[]` - Available tools with descriptions
- `prompts[]` - Pre-defined prompts with arguments

**server.json has no equivalent**

### 6. Platform-Specific Overrides 🟡
**manifest.json supports:**
- `server.mcp_config.platform_overrides` - Different configs per OS

**server.json has no equivalent**

## Structural Differences

### Multiple Packages Problem
- **server.json**: Supports multiple packages (npm, pypi, docker versions)
- **manifest.json**: Expects a single server configuration
- **Current solution**: Select "best" package based on priority (node > python > binary)
- **Information loss**: Other package options are discarded

### Environment Variable Mapping Limitations
- **server.json**: `environment_variables` with `choices` constraint
- **manifest.json**: No support for choice constraints
- **Lost information**: Choice validations

### Repository Type
- **server.json**: Only has repository URL
- **manifest.json**: Expects repository type (e.g., "git")
- **Current workaround**: Assumes "git"

## Recommendations for server.json Enhancement

### Priority 1: Critical Missing Information
1. Add `author` object with name, email, url
2. Add `entry_point` to packages
3. Add explicit `server_type` field

### Priority 2: Improve Conversion Quality
1. Add `display_name`, `homepage`, `documentation`, `support`
2. Add `license` field
3. Add `keywords` array

### Priority 3: Feature Parity
1. Add `compatibility` object
2. Add `platform_overrides` support
3. Consider `tools` and `prompts` metadata

### Priority 4: Structural Improvements
1. Add a "primary_package" indicator for multi-package scenarios
2. Standardize repository object with type field
3. Add icon and screenshot support

## Example Enhanced server.json

```json
{
  "name": "my-mcp-server",
  "display_name": "My MCP Server",
  "description": "A powerful MCP server",
  "long_description": "Extended description...",
  "version_detail": {
    "version": "1.0.0"
  },
  "author": {
    "name": "John Doe",
    "email": "john@example.com",
    "url": "https://example.com"
  },
  "repository": {
    "type": "git",
    "url": "https://github.com/example/repo"
  },
  "homepage": "https://example.com",
  "documentation": "https://docs.example.com",
  "support": "https://github.com/example/repo/issues",
  "license": "MIT",
  "keywords": ["mcp", "ai", "tool"],
  "compatibility": {
    "claude_desktop": ">=1.0.0",
    "platforms": ["darwin", "win32", "linux"]
  },
  "packages": [
    {
      "registry_name": "npm",
      "name": "@example/mcp-server",
      "version": "1.0.0",
      "runtime_hint": "npx",
      "entry_point": "dist/index.js",
      "primary": true,
      "environment_variables": [...]
    }
  ]
}
```

## Conclusion

While the current conversion from `server.json` to `manifest.json` is functional, significant information gaps exist. The most critical missing pieces are:

1. **Author information** - Required but completely missing
2. **Entry points** - Currently using error-prone defaults
3. **Explicit server type** - Currently inferred with potential for mistakes

Enhancing `server.json` with these fields would enable more accurate and complete manifest generation, reducing manual intervention and improving the developer experience.