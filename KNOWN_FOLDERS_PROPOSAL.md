# Known Folders Proposal for MCPB v0.3

## Summary

This proposal extends the MCPB manifest's variable substitution system to support a comprehensive set of known folder tokens, enabling MCP extensions to reference common user directories in a cross-platform manner while also supporting platform-specific folders when needed.

## Background

The MCPB manifest currently supports a limited set of folder replacement tokens:

- `${HOME}` - User's home directory
- `${DESKTOP}` - User's desktop directory
- `${DOCUMENTS}` - User's documents directory
- `${DOWNLOADS}` - User's downloads directory

Windows has a rich set of "known folders" (via KNOWNFOLDERID) that go beyond these basic directories. Similarly, macOS and Linux have their own sets of well-known user directories. This proposal aims to:

1. Extend support for cross-platform user directories
2. Provide a mechanism for platform-specific directories
3. Maintain clarity for manifest authors about which tokens work on which platforms

## Proposal

### Cross-Platform Folder Tokens

These tokens are supported on all platforms (Windows, macOS, Linux) with appropriate platform-specific resolution:

| Token          | Description           | Windows         | macOS       | Linux (XDG)           |
| -------------- | --------------------- | --------------- | ----------- | --------------------- |
| `${HOME}`      | User's home directory | `%USERPROFILE%` | `$HOME`     | `$HOME`               |
| `${DESKTOP}`   | Desktop folder        | `Desktop`       | `Desktop`   | `XDG_DESKTOP_DIR`     |
| `${DOCUMENTS}` | Documents folder      | `Documents`     | `Documents` | `XDG_DOCUMENTS_DIR`   |
| `${DOWNLOADS}` | Downloads folder      | `Downloads`     | `Downloads` | `XDG_DOWNLOAD_DIR`    |
| `${MUSIC}`     | Music folder          | `Music`         | `Music`     | `XDG_MUSIC_DIR`       |
| `${PICTURES}`  | Pictures folder       | `Pictures`      | `Pictures`  | `XDG_PICTURES_DIR`    |
| `${VIDEOS}`    | Videos folder         | `Videos`        | `Videos`    | `XDG_VIDEOS_DIR`      |
| `${TEMPLATES}` | Templates folder      | `Templates`     | `Templates` | `XDG_TEMPLATES_DIR`   |
| `${PUBLIC}`    | Public/shared folder  | `Public`        | `Public`    | `XDG_PUBLICSHARE_DIR` |

### Platform-Specific Folder Tokens

For platform-specific needs, tokens include a platform prefix:

#### Windows-Specific Tokens

| Token                       | Description              | Windows Path                   |
| --------------------------- | ------------------------ | ------------------------------ |
| `${WINDOWS:LOCALAPPDATA}`   | Local application data   | `%LOCALAPPDATA%`               |
| `${WINDOWS:ROAMINGAPPDATA}` | Roaming application data | `%APPDATA%`                    |
| `${WINDOWS:PROGRAMDATA}`    | Common application data  | `%PROGRAMDATA%`                |
| `${WINDOWS:STARTUP}`        | User startup folder      | `Startup` in user's Start Menu |
| `${WINDOWS:TEMP}`           | Temporary files          | `%TEMP%`                       |

#### macOS-Specific Tokens

| Token                          | Description         | macOS Path                      |
| ------------------------------ | ------------------- | ------------------------------- |
| `${MACOS:APPLICATION_SUPPORT}` | Application support | `~/Library/Application Support` |
| `${MACOS:CACHES}`              | Cache directory     | `~/Library/Caches`              |
| `${MACOS:LIBRARY}`             | User library        | `~/Library`                     |

#### Linux-Specific Tokens

| Token             | Description      | Linux Path                           |
| ----------------- | ---------------- | ------------------------------------ |
| `${LINUX:CONFIG}` | Config directory | `$XDG_CONFIG_HOME` or `~/.config`    |
| `${LINUX:DATA}`   | Data directory   | `$XDG_DATA_HOME` or `~/.local/share` |
| `${LINUX:CACHE}`  | Cache directory  | `$XDG_CACHE_HOME` or `~/.cache`      |

### Resolution Behavior

1. **Cross-platform tokens**: Always resolved on all platforms. If the folder doesn't exist, the implementing app should create it or use a reasonable fallback.

2. **Platform-specific tokens**:
   - On the matching platform: Resolved to the appropriate path
   - On other platforms: Token remains unreplaced as a literal string (e.g., `${WINDOWS:LOCALAPPDATA}`)
   - Manifest authors should use platform-specific tokens only within platform-specific configurations (e.g., in `platform_overrides`)

3. **Error handling**: If a folder cannot be resolved (e.g., XDG directory not configured), implementations should:
   - Log a warning
   - Fall back to a sensible default (e.g., `~/Music` for `${MUSIC}`)
   - Document the fallback behavior

### Example Usage

#### Cross-Platform Configuration

```json
{
  "user_config": {
    "music_library": {
      "type": "directory",
      "title": "Music Library",
      "description": "Your music library location",
      "default": ["${MUSIC}"]
    }
  },
  "server": {
    "mcp_config": {
      "command": "node",
      "args": ["${__dirname}/server.js"],
      "env": {
        "MUSIC_PATH": "${user_config.music_library}"
      }
    }
  }
}
```

#### Platform-Specific Configuration

```json
{
  "server": {
    "mcp_config": {
      "command": "node",
      "args": ["${__dirname}/server.js"],
      "env": {
        "DATA_PATH": "${HOME}/.myapp"
      },
      "platform_overrides": {
        "win32": {
          "env": {
            "DATA_PATH": "${WINDOWS:LOCALAPPDATA}/MyApp"
          }
        },
        "darwin": {
          "env": {
            "DATA_PATH": "${MACOS:APPLICATION_SUPPORT}/MyApp"
          }
        },
        "linux": {
          "env": {
            "DATA_PATH": "${LINUX:DATA}/myapp"
          }
        }
      }
    }
  }
}
```

### Migration Path

Existing manifests using `${HOME}`, `${DESKTOP}`, `${DOCUMENTS}`, and `${DOWNLOADS}` continue to work without changes. New tokens are additive only.

### Documentation Requirements

The MANIFEST.md documentation should include:

1. A complete table of all supported folder tokens
2. Clear indication of which tokens work on which platforms
3. Examples of cross-platform usage
4. Examples of platform-specific usage
5. Guidance on fallback behavior
6. Best practices (e.g., use cross-platform tokens when possible, use platform-specific tokens within platform_overrides)

## Implementation Notes

1. **Node.js Implementation**: Use `os.homedir()` as base and well-known subdirectories. On Linux, check XDG environment variables first.

2. **Windows Implementation**: Use appropriate Windows APIs (e.g., `SHGetKnownFolderPath`) for known folders.

3. **macOS Implementation**: Use appropriate APIs to get standard directories (e.g., via NSFileManager).

4. **Testing**: Create comprehensive tests covering:
   - All cross-platform tokens on current platform
   - Platform-specific token behavior
   - Fallback behavior
   - Token replacement in various contexts (args, env, user_config defaults)

## Benefits

1. **Better cross-platform support**: Extensions can reference common directories with confidence
2. **Windows parity**: Enables Windows-specific features that require access to system folders
3. **Developer clarity**: Clear naming conventions make it obvious which tokens work where
4. **Backward compatible**: Existing manifests continue to work
5. **Extensible**: Easy to add new tokens in the future

## Alternatives Considered

### Alternative 1: Platform-agnostic tokens only

Only support tokens that work everywhere. Rejected because it limits Windows extensions from accessing necessary system folders.

### Alternative 2: Automatic platform detection without prefixes

Use the same token name (e.g., `${APPDATA}`) and resolve based on platform. Rejected because it's unclear to manifest authors what each token means on different platforms.

### Alternative 3: Separate manifest per platform

Have completely different manifests for each platform. Rejected because it creates maintenance burden and isn't aligned with MCPB's goal of portable extensions.

## Open Questions

1. Should we support tokens like `${WINDOWS:SYSTEM32}` for system directories? (Recommendation: No, for security reasons)
2. Should we support environment variable expansion like `${ENV:MY_CUSTOM_VAR}`? (Recommendation: Separate proposal if needed)
3. How should we handle folders that require admin permissions? (Recommendation: Document as not supported, use only user-accessible folders)
