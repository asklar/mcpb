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

- `mcpb init [directory] [--server-type node|python|binary|auto] [--entry-point path]` Create manifest.json (auto-detects server type if not specified: prefers node > python > binary). For binary (default for .NET), entry point defaults to `server/<folderName>`.
- `mcpb validate <manifest|directory>` Validate manifest
- `mcpb pack [directory] [output]` Create .mcpb archive
- `mcpb unpack <file> [outputDir]` Extract archive
- `mcpb sign <file> [--cert cert.pem --key key.pem --self-signed]` Sign bundle
- `mcpb verify <file>` Verify signature
- `mcpb info <file>` Show bundle info (and signature)
- `mcpb unsign <file>` Remove signature

## License Compliance
All referenced NuGet packages are MIT licensed (System.*, Spectre.Console).
