namespace SampleTech.Api.Services;

/// <summary>
/// Abstraction over blob storage for policy documents.
/// Local file system in development; swap for S3 in production via DI.
/// </summary>
public interface IDocumentStorageService
{
    /// <summary>
    /// Stores the uploaded file and returns a retrievable URL/path.
    /// </summary>
    Task<string> StoreAsync(Guid tenantId, Guid policyId, string fileName, string contentType, Stream content, CancellationToken ct = default);
}
