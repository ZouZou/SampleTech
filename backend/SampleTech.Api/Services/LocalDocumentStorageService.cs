using Microsoft.Extensions.Configuration;

namespace SampleTech.Api.Services;

/// <summary>
/// Development-only document storage: saves files to a local directory.
/// Replace with S3DocumentStorageService in production by swapping the DI registration.
/// </summary>
public class LocalDocumentStorageService(IConfiguration config, ILogger<LocalDocumentStorageService> logger) : IDocumentStorageService
{
    private readonly string _rootPath = config["DocumentStorage:LocalPath"] ?? Path.Combine(Path.GetTempPath(), "sampletech-docs");

    public async Task<string> StoreAsync(
        Guid tenantId, Guid policyId, string fileName, string contentType,
        Stream content, CancellationToken ct = default)
    {
        var dir = Path.Combine(_rootPath, tenantId.ToString(), policyId.ToString());
        Directory.CreateDirectory(dir);

        // Prefix with timestamp to avoid collisions
        var safeFileName = $"{DateTimeOffset.UtcNow:yyyyMMddHHmmss}_{Path.GetFileName(fileName)}";
        var filePath = Path.Combine(dir, safeFileName);

        await using var fs = File.Create(filePath);
        await content.CopyToAsync(fs, ct);

        logger.LogInformation("Stored document {FileName} for policy {PolicyId} at {Path}", fileName, policyId, filePath);

        // Return a relative path that can later be resolved via a file-serving endpoint
        return $"/api/policies/{policyId}/documents/files/{safeFileName}";
    }
}
