using SampleTech.Api.Services;

namespace SampleTech.Api.Tests.Helpers;

/// <summary>No-op document storage for unit tests — returns a predictable URL.</summary>
public class NullDocumentStorageService : IDocumentStorageService
{
    public Task<string> StoreAsync(Guid tenantId, Guid policyId, string fileName, string contentType, Stream content, CancellationToken ct = default)
        => Task.FromResult($"/test/docs/{policyId}/{fileName}");
}
