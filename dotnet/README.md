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

| Command                                                                                 | Description                      |
| --------------------------------------------------------------------------------------- | -------------------------------- |
| `mcpb init [directory] [--server-type node\|python\|binary\|auto] [--entry-point path]` | Create manifest.json             |
| `mcpb validate [manifest\|directory]`                                                   | Validate manifest                |
| `mcpb pack [directory] [output]`                                                        | Create .mcpb archive             |
| `mcpb unpack <file> [outputDir]`                                                        | Extract archive                  |
| `mcpb sign <file> [--cert cert.pem --key key.pem --self-signed]`                        | Sign bundle                      |
| `mcpb verify <file>`                                                                    | Verify signature                 |
| `mcpb info <file>`                                                                      | Show bundle info (and signature) |
| `mcpb unsign <file>`                                                                    | Remove signature                 |

## Windows `_meta` Updates

When you run `mcpb validate --update` or `mcpb pack --update`, the tool captures the Windows-focused initialize and tools/list responses returned during MCP discovery. The static responses are written to `manifest._meta["com.microsoft.windows"].static_responses` so Windows clients can use cached protocol data without invoking the server. Re-run either command with `--update` whenever you want to refresh those cached responses.

## License Compliance

MIT licensed
