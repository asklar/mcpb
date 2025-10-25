# Media Organizer Example

This example demonstrates the use of the new known folder tokens in MCPB v0.3 manifest.

## Features Demonstrated

### Cross-Platform Folder Tokens

The manifest uses cross-platform folder tokens that work on all platforms:

- `${MUSIC}` - Music directory
- `${PICTURES}` - Pictures directory
- `${VIDEOS}` - Videos directory

These tokens are automatically resolved to the appropriate paths on each platform:

- Windows: `C:\Users\{username}\Music`, `C:\Users\{username}\Pictures`, etc.
- macOS: `~/Music`, `~/Pictures`, etc.
- Linux: XDG directories or `~/Music`, `~/Pictures`, etc.

### Platform-Specific Folder Tokens

The manifest uses `platform_overrides` to specify platform-specific application data folders:

- Windows: `${WINDOWS:LOCALAPPDATA}/MediaOrganizer`
- macOS: `${MACOS:APPLICATION_SUPPORT}/MediaOrganizer`
- Linux: `${LINUX:DATA}/media-organizer`

This demonstrates best practices for handling platform-specific folders while maintaining a single, portable manifest.

## Usage

This is a minimal example that demonstrates the folder token functionality. In a real implementation, you would:

1. Create a `server/index.js` file that implements the MCP server
2. Access the environment variables to get the configured folder paths
3. Implement the tools to organize and search media files

## Testing

You can test variable substitution by running:

```bash
# From the repository root
yarn build
node -e "
  const { getMcpConfigForManifest } = require('./dist/shared/config.js');
  const manifest = require('./examples/media-organizer/manifest.json');

  getMcpConfigForManifest({
    manifest,
    extensionPath: process.cwd() + '/examples/media-organizer',
    userConfig: {},
    pathSeparator: require('path').sep
  }).then(config => {
    console.log('Resolved MCP config:');
    console.log(JSON.stringify(config, null, 2));
  });
"
```

This will show how the folder tokens are resolved on your platform.
