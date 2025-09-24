using System.CommandLine;

namespace Mcpb.Commands;

public static class CliRoot
{
    public static RootCommand Build()
    {
        var description = """
            Tools for building MCP Bundles (.mcpb)
            
            TELEMETRY NOTICE:
            This tool collects anonymous usage telemetry to help improve the product.
            No personal information is collected. To opt out, set the environment
            variable MCPB_DISABLE_TELEMETRY=1 or MCPB_DISABLE_TELEMETRY=true.
            """;
            
        var root = new RootCommand(description);
        root.AddCommand(InitCommand.Create());
        root.AddCommand(ValidateCommand.Create());
        root.AddCommand(PackCommand.Create());
        root.AddCommand(UnpackCommand.Create());
        root.AddCommand(SignCommand.Create());
        root.AddCommand(VerifyCommand.Create());
        root.AddCommand(InfoCommand.Create());
        root.AddCommand(UnsignCommand.Create());
        return root;
    }
}