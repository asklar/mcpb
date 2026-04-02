# MCPB .NET CLI

Experimental .NET port of the MCPB CLI.

## Build

```pwsh
cd dotnet/mcpb
dotnet build -c Release
```

## Install as local tool

```pwsh
cd dotnet/mcpb
dotnet pack -c Release
# Find generated nupkg in bin/Release
dotnet tool install --global Mcpb.Cli --add-source ./bin/Release
```

## Commands

### `mcpb init [directory]`

Create a new MCPB extension manifest (`manifest.json`). Launches an interactive
wizard unless `--yes` is provided.

| Option / Argument | Description |
| --- | --- |
| `directory` | Target directory (default: current directory) |
| `--yes`, `-y` | Accept defaults, skip interactive prompts |
| `--server-type <type>` | Server type: `node`, `python`, `binary`, or `auto` (default: `auto`) |
| `--entry-point <path>` | Override entry point path (relative to manifest) |

---

### `mcpb validate [manifest]`

Validate an MCPB manifest file. Optionally runs dynamic tool/prompt discovery
and can auto-update the manifest to match discovered results.

| Option / Argument | Description |
| --- | --- |
| `manifest` | Path to `manifest.json` or its containing directory |
| `--dirname <dir>` | Directory containing referenced files and server entry point |
| `--update` | Update manifest tools/prompts and `_meta` static responses to match discovery results (requires `--dirname`) |

---

### `mcpb pack [directory] [output]`

Pack a directory into an `.mcpb` extension archive. Performs manifest validation,
file collection (respecting `.mcpbignore`), and optional dynamic tool/prompt
discovery before bundling.

| Option / Argument | Description |
| --- | --- |
| `directory` | Extension directory (default: current directory) |
| `output` | Output `.mcpb` file path (default: `<name>.mcpb` in current directory) |
| `--force` | Proceed even if discovered tools/prompts differ from the manifest |
| `--update` | Update manifest tools/prompts and `_meta` static responses to match discovery |
| `--no-discover` | Skip dynamic tool/prompt discovery (for offline or testing use) |

---

### `mcpb unpack <mcpb-file> [output]`

Extract the contents of an `.mcpb` archive.

| Option / Argument | Description |
| --- | --- |
| `mcpb-file` | Path to the `.mcpb` file (required) |
| `output` | Output directory (default: current directory) |

---

### `mcpb sign <mcpb-file>`

Sign an `.mcpb` extension file with a PKCS#7 detached signature.

| Option / Argument | Description |
| --- | --- |
| `mcpb-file` | Path to the `.mcpb` file (required) |
| `--cert`, `-c` | Path to certificate PEM file (default: `cert.pem`) |
| `--key`, `-k` | Path to private key PEM file (default: `key.pem`) |
| `--self-signed` | Create a self-signed certificate if the cert/key files are missing |

---

### `mcpb verify <mcpb-file>`

Verify the signature of an `.mcpb` file. Prints signer details when valid.

| Option / Argument | Description |
| --- | --- |
| `mcpb-file` | Path to the `.mcpb` file (required) |

---

### `mcpb info <mcpb-file>`

Display file size and signature information for an `.mcpb` file.

| Option / Argument | Description |
| --- | --- |
| `mcpb-file` | Path to the `.mcpb` file (required) |

---

### `mcpb unsign <mcpb-file>`

Remove the signature block from an `.mcpb` file.

| Option / Argument | Description |
| --- | --- |
| `mcpb-file` | Path to the `.mcpb` file (required) |

## License Compliance

MIT licensed
