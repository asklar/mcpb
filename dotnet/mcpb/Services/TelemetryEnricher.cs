namespace Mcpb.Services;

internal static class TelemetryEnricher
{
    public static void Pack(int fileCount, int ignoredCount, long packageBytes, long unpackedBytes)
    {
        StaticTelemetryBridge.AddProperty("files_total", fileCount.ToString());
        StaticTelemetryBridge.AddProperty("files_ignored", ignoredCount.ToString());
        StaticTelemetryBridge.AddProperty("size_package_bytes", packageBytes.ToString());
        StaticTelemetryBridge.AddProperty("size_unpacked_bytes", unpackedBytes.ToString());
    }

    public static void Validate(int errorCount, int warningCount, int deprecationCount)
    {
        StaticTelemetryBridge.AddProperty("manifest_errors", errorCount.ToString());
        StaticTelemetryBridge.AddProperty("manifest_warnings", warningCount.ToString());
        StaticTelemetryBridge.AddProperty("manifest_deprecations", deprecationCount.ToString());
    }

    public static void Sign(bool selfSigned, bool alreadySigned)
    {
        StaticTelemetryBridge.AddProperty("self_signed", selfSigned.ToString().ToLowerInvariant());
        StaticTelemetryBridge.AddProperty("already_signed", alreadySigned.ToString().ToLowerInvariant());
    }

    public static void Verify(bool valid, bool hadSignature)
    {
        StaticTelemetryBridge.AddProperty("signature_present", hadSignature.ToString().ToLowerInvariant());
        StaticTelemetryBridge.AddProperty("signature_valid", valid.ToString().ToLowerInvariant());
    }

    public static void Unpack(int entries, long totalBytes)
    {
        StaticTelemetryBridge.AddProperty("entries", entries.ToString());
        StaticTelemetryBridge.AddProperty("entries_total_bytes", totalBytes.ToString());
    }

    public static void Unsign(bool hadSignature)
    {
        StaticTelemetryBridge.AddProperty("removed_signature", hadSignature.ToString().ToLowerInvariant());
    }
}